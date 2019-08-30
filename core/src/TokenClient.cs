using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Rpc;
using Tokenio.Security;
using Tokenio.Utils;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;


namespace Tokenio
{
    public class TokenClient : IDisposable
    {
        protected readonly ManagedChannel channel;
        protected readonly TokenCluster tokenCluster;
        protected readonly ICryptoEngineFactory cryptoEngineFactory;

        /// <summary>
        /// Creates an instance of a Token SDK.
        /// </summary>
        /// <param name="channel">the gRPC channel</param>
        /// <param name="cryptoEngineFactory">the crypto factory to create crypto engine</param>
        /// <param name="tokenCluster">the token cluster to connect to</param>
        public TokenClient(
            ManagedChannel channel,
            ICryptoEngineFactory cryptoEngineFactory,
            TokenCluster tokenCluster)
        {
            this.channel = channel;
            this.cryptoEngineFactory = cryptoEngineFactory;
            this.tokenCluster = tokenCluster;
        }

        /// <summary>
        /// Resolve an alias to a TokenMember object, containing member ID and
        /// the alias with the correct type.
        /// </summary>
        /// <param name="alias">alias to resolve</param>
        /// <returns>TokenMember</returns>
        public Task<TokenMember> ResolveAlias(Alias alias)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.ResolveAlias(alias);
        }

        /// <summary>
        /// Resolve an alias to a TokenMember object, containing member ID and
        /// the alias with the correct type.
        /// </summary>
        /// <param name="alias">alias to resolve</param>
        /// <returns>TokenMember</returns>
        public TokenMember ResolveAliasBlocking(Alias alias)
        {
            return ResolveAlias(alias).Result;
        }

        /// <summary>
        /// Looks up member id for a given alias.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>member id if alias already exists, null otherwise</returns>
        public Task<string> GetMemberId(Alias alias)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetMemberId(alias);
        }

        /// <summary>
        /// Looks up member id for a given alias.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>member id if alias already exists, null otherwise</returns>
        public string GetMemberIdBlocking(Alias alias)
        {
            return GetMemberId(alias).Result;
        }

        /// <summary>
        /// Creates a new Token member with a set of auto-generated keys, an alias, and member type.
        /// Impl method returns incomplete member object that can be used for its instance
        /// fields but will not be able to make calls
        /// </summary>
        /// <returns>newly created member</returns>
        /// <param name="alias">Alias.</param>
        /// <param name="createMemberType">Create member type.</param>
        /// <param name="recoveryAgent">Recovery agent.</param>
        /// <param name="partnerId">Partner identifier.</param>
        /// <param name="realmId">Realm identifier.</param>

        public Task<Member> CreateMemberImpl(
        Alias alias,
        CreateMemberType createMemberType,
        string recoveryAgent,
        string partnerId = null,
        string realmId = null)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated
                .CreateMemberId(createMemberType, null, partnerId, realmId)
                .FlatMap(memberId =>
                {
                    return SetUpMemberImpl(memberId, alias, recoveryAgent);
                });

        }

        /// <summary>
        /// Sets up member impl.
        /// </summary>
        /// <returns>The up member impl.</returns>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="alias">Alias.</param>
        /// <param name="agent">Agent.</param>
        protected Task<Member> SetUpMemberImpl(string memberId,
             Alias alias,
             string agent = null)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return (agent == null ? unauthenticated.GetDefaultAgent()
                : Task.Factory.StartNew(() => { return agent; }))
                 .FlatMap(agentId =>
                 {
                     var crypto = cryptoEngineFactory.Create(memberId);
                     var operations = new List<MemberOperation>
                    {
                        Util.ToAddKeyOperation(crypto.GenerateKey(Level.Privileged)),
                        Util.ToAddKeyOperation(crypto.GenerateKey(Level.Standard)),
                        Util.ToAddKeyOperation(crypto.GenerateKey(Level.Low)),
                        Util.ToRecoveryAgentOperation(agentId)

                 };
                     var metadata = new List<MemberOperationMetadata>();
                     if (alias != null)
                     {
                         operations.Add(Util.ToAddAliasOperation(alias.ToNormalized()));
                         metadata.Add(Util.ToAddAliasMetadata(alias.ToNormalized()));

                     }

                     var signer = crypto.CreateSigner(Level.Privileged);
                     var mem = unauthenticated.CreateMember(memberId, operations, metadata, signer);
                     return mem;
                 }).Map(member =>
                 {
                     return new Member(member.Id, null, tokenCluster, member.PartnerId, member.RealmId);
                 });
        }





        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">the member ID</param>
        /// <returns>the member</returns>
        public Task<Member> GetMemberImpl(string memberId, Client client)
        {
            return client
                .GetMember(memberId)
                .Map(member => new Member(member.Id, null,
                tokenCluster, member.PartnerId, member.RealmId));
        }




        /// <summary>
        /// Begins account recovery.
        /// </summary>
        /// <param name="alias">the used to recover</param>
        /// <returns>the verification id</returns>
        public Task<string> BeginRecovery(Alias alias)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.BeginRecovery(alias);
        }

        /// <summary>
        /// Begins account recovery.
        /// </summary>
        /// <param name="alias">the used to recover</param>
        /// <returns>the verification id</returns>
        public string BeginRecoveryBlocking(Alias alias)
        {
            return BeginRecovery(alias).Result;
        }

        /// <summary>
        /// Create a recovery authorization for some agent to sign.
        /// </summary>
        /// <param name="memberId">the ID of the member we claim to be.</param>
        /// <param name="privilegedKey">the new privileged key we want to use.</param>
        /// <returns>the authorization</returns>
        public Task<Authorization> CreateRecoveryAuthorization(
            string memberId,
            Key privilegedKey)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.CreateRecoveryAuthorization(memberId, privilegedKey);
        }

        /// <summary>
        /// Create a recovery authorization for some agent to sign.
        /// </summary>
        /// <param name="memberId">the ID of the member we claim to be.</param>
        /// <param name="privilegedKey">the new privileged key we want to use.</param>
        /// <returns>the authorization</returns>
        public Authorization CreateRecoveryAuthorizationBlocking(
            string memberId,
            Key privilegedKey)
        {
            return CreateRecoveryAuthorization(memberId, privilegedKey).Result;
        }

        /// <summary>
        /// Gets recovery authorization from Token.
        /// </summary>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the verification code</param>
        /// <param name="key">the privileged key</param>
        /// <returns>the member recovery operation</returns>
        public Task<MemberRecoveryOperation> GetRecoveryAuthorization(
            string verificationId,
            string code,
            Key key)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetRecoveryAuthorization(verificationId, code, key);
        }

        /// <summary>
        /// Gets recovery authorization from Token.
        /// </summary>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the verification code</param>
        /// <param name="key">the privileged key</param>
        /// <returns>the member recovery operation</returns>
        public MemberRecoveryOperation GetRecoveryAuthorizationBlocking(
            string verificationId,
            string code,
            Key key)
        {
            return GetRecoveryAuthorization(verificationId, code, key).Result;
        }

        /// <summary>
        /// Completes account recovery.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="recoveryOperations">the member recovery operations</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>the new member</returns>
        public Task<Member> CompleteRecoveryImpl(
            string memberId,
            IList<MemberRecoveryOperation> recoveryOperations,
            Key privilegedKey,
            ICryptoEngine cryptoEngine)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated
                .CompleteRecovery(memberId, recoveryOperations, privilegedKey, cryptoEngine)
                .Map(member =>
                {
                    return new Member(member.Id, null, tokenCluster, member.PartnerId, member.RealmId);
                });
        }


        /// <summary>
        /// Completes account recovery if the default recovery rule was set.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the code</param>
        /// <returns>the new member</returns>
        public Task<Member> CompleteRecoveryWithDefaultRuleImpl(
            string memberId,
            string verificationId,
            string code, ICryptoEngine cryptoEngine)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            //var cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
            return unauthenticated
                .CompleteRecoveryWithDefaultRule(memberId, verificationId, code, cryptoEngine)
                .Map(member =>
                {
                    return new Member(member.Id, null, tokenCluster, member.PartnerId, member.RealmId);

                });
        }



        /// <summary>
        /// Returns the first 200 available banks for linking.
        /// </summary>
        /// <returns>banks with paging information</returns>
        public Task<PagedBanks> GetBanks()
        {
            return GetBanks(1, 200);
        }

        /// <summary>
        /// Returns banks from a given list of bank IDs (case-insensitive).
        /// </summary>
        /// <param name="ids">the bank IDs</param>
        /// <returns>banks with paging information</returns>
        public Task<PagedBanks> GetBanks(IList<string> ids)
        {
            return GetBanks(ids, null, null, null, null, null);
        }

        /// <summary>
        /// Return banks whose 'name' or 'identifier' contains the given search string (case-insensitive).
        /// </summary>
        /// <param name="search">the keyword to search for</param>
        /// <returns>banks with paging information</returns>
        public Task<PagedBanks> GetBanks(string search)
        {
            return GetBanks(null, search, null, null, null, null);
        }

        /// <summary>
        /// Returns banks with specified paging information.
        /// </summary>
        /// <param name="page">the result page to retrieve</param>
        /// <param name="perPage">max number of records per page, can be at most 200</param>
        /// <returns>banks with paging information</returns>
        public Task<PagedBanks> GetBanks(
            int page,
            int perPage)
        {
            return GetBanks(null, null, null, page, perPage, null);
        }

        /// <summary>
        /// Return banks whose 'country' matches the given country code (case-insensitive).
        /// </summary>
        /// <param name="country">the ISO 3166-1 alpha-2 country code</param>
        /// <param name="page">the result page to retrieve</param>
        /// <param name="perPage">max number of records per page, can be at most 200</param>
        /// <returns>banks with paging information</returns>
        public Task<PagedBanks> GetBanks(
            string country,
            int page,
            int perPage)
        {
            return GetBanks(null, null, country, page, perPage, null);
        }

        /// <summary>
        /// Return banks that satisfy given filtering requirements.
        /// </summary>
        /// <param name="ids">the bank IDs to fetch</param>
        /// <param name="search">the keyword to search the fields 'name' and 'identifier' for</param>
        /// <param name="country">ISO 3166-1 alpha-2 country code of the banks</param>
        /// <param name="page">the result page to retrieve</param>
        /// <param name="perPage">max number of records per page, can be at most 200</param>
        /// <param name="sort">the key to sort the results, one of: name, provider and country</param>
        /// <returns>banks with paging information</returns>
        /// <remarks>
        /// All fields are optional. Set to null if absent. The default value for page is 1; the default
        /// value for perPage is 200. Values set out of range will be treated as default value.
        /// </remarks>
        public Task<PagedBanks> GetBanks(
            IList<string> ids,
            string search,
            string country,
            int? page,
            int? perPage,
            string sort)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetBanks(ids, search, country, page, perPage, sort);
        }

        /// <summary>
        /// Returns the first 200 available banks for linking.
        /// </summary>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanksBlocking()
        {
            return GetBanks().Result;
        }

        /// <summary>
        /// Returns banks from a given list of bank IDs (case-insensitive).
        /// </summary>
        /// <param name="ids">the bank IDs</param>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanksBlocking(IList<string> ids)
        {
            return GetBanks(ids).Result;
        }

        /// <summary>
        /// Return banks whose 'name' or 'identifier' contains the given search string (case-insensitive).
        /// </summary>
        /// <param name="search">the keyword to search for</param>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanksBlocking(string search)
        {
            return GetBanks(search).Result;
        }

        /// <summary>
        /// Returns banks with specified paging information.
        /// </summary>
        /// <param name="page">the result page to retrieve</param>
        /// <param name="perPage">max number of records per page, can be at most 200</param>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanksBlocking(
            int page,
            int perPage)
        {
            return GetBanks(page, perPage).Result;
        }

        /// <summary>
        /// Return banks whose 'country' matches the given country code (case-insensitive).
        /// </summary>
        /// <param name="country">the ISO 3166-1 alpha-2 country code</param>
        /// <param name="page">the result page to retrieve</param>
        /// <param name="perPage">max number of records per page, can be at most 200</param>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanksBlocking(
            string country,
            int page,
            int perPage)
        {
            return GetBanks(country, page, perPage).Result;
        }

        /// <summary>
        /// Return banks that satisfy given filtering requirements.
        /// </summary>
        /// <param name="ids">the bank IDs to fetch</param>
        /// <param name="search">the keyword to search the fields 'name' and 'identifier' for</param>
        /// <param name="country">ISO 3166-1 alpha-2 country code of the banks</param>
        /// <param name="page">the result page to retrieve</param>
        /// <param name="perPage">max number of records per page, can be at most 200</param>
        /// <param name="sort">the key to sort the results, one of: name, provider and country</param>
        /// <returns>banks with paging information</returns>
        /// <remarks>
        /// All fields are optional. Set to null if absent. The default value for page is 1; the default
        /// value for perPage is 200. Values set out of range will be treated as default value.
        /// </remarks>
        public PagedBanks GetBanksBlocking(
            IList<string> ids,
            string search,
            string country,
            int? page,
            int? perPage,
            string sort)
        {
            return GetBanks(ids, search, country, page, perPage, sort).Result;
        }



        /// <summary>
        /// Returns a list of countries with Token-enabled banks.
        /// </summary>
        /// <param name="provider">If specified, return banks whose 'provider' matches the given provider</param>
        /// <returns>a list of country codes</returns>
        public Task<IList<string>> GetCountries(string provider)
        {
            UnauthenticatedClient unauthenticatedClient = ClientFactory.Unauthenticated(channel);
            return unauthenticatedClient.GetCountries(provider);
        }

        /// <summary>
        /// Returns a list of countries with Token-enabled banks.
        /// </summary>
        /// <param name="provider">If specified, return banks whose 'provider' matches the given provider</param>
        /// <returns>a list of country codes</returns>
        public IList<string> GetCountriesBlocking(string provider)
        {
            return GetCountries(provider).Result;
        }

        public ICryptoEngineFactory GetCryptoEngineFactory()
        {
            return this.cryptoEngineFactory;
        }

        public void Dispose()
        {
            channel.Dispose();
        }




        public class Builder<T> where T : Builder<T>
        {
            private static readonly string DEFAULT_DEV_KEY = "4qY7lqQw8NOl9gng0ZHgT4xdiDqxqoGVutuZwrUYQsI";
            private static readonly long DEFAULT_TIMEOUT_MS = 10_000L;
            private static readonly int DEFAULT_SSL_PORT = 443;
            private static readonly int DEFAULT_KEEP_ALIVE_TIME_MS = 50_000;
            private static readonly bool DEFAULT_KEEP_ALIVE_PERMIT_WITHOUT_CALLS = true;

            protected int port;
            protected bool useSsl;
            protected TokenCluster tokenCluster;
            protected string hostName;
            protected long timeoutMs;
            protected ICryptoEngineFactory cryptoEngine;
            protected string devKey;
            protected List<string> featureCodes;
            protected static readonly string FEATURE_CODE_KEY = "feature-codes";
            protected bool keepAlive = DEFAULT_KEEP_ALIVE_PERMIT_WITHOUT_CALLS;
            protected int keepAliveTimeMs = DEFAULT_KEEP_ALIVE_TIME_MS;

            /// <summary>
            /// Creates new builder instance with the defaults initialized.
            /// </summary>
            public Builder()
            {
                devKey = DEFAULT_DEV_KEY;
                timeoutMs = DEFAULT_TIMEOUT_MS;
                port = DEFAULT_SSL_PORT;
                useSsl = true;
            }

            /// <summary>
            /// Sets the host name of the Token Gateway Service to connect to.
            /// </summary>
            /// <param name="hostName">the host name to set</param>
            public T HostName(string hostName)
            {
                this.hostName = hostName;
                return (T)this;
            }

            /// <summary>
            /// Sets the port of the Token Gateway Service to connect to.
            /// </summary>
            /// <param name="port">the port number</param>
            /// <returns>this builder instance</returns>
            public T Port(int port)
            {
                this.port = port;
                this.useSsl = port == DEFAULT_SSL_PORT;
                return (T)this;
            }

            /// <summary>
            /// Sets Token cluster to connect to.
            /// </summary>
            /// <param name="cluster">the token cluster</param>
            /// <returns>this builder instance</returns>
            public T ConnectTo(TokenCluster cluster)
            {
                this.tokenCluster = cluster;
                this.hostName = cluster.Url;
                return (T)this;
            }

            /// <summary>
            /// Sets timeoutMs that is used for the RPC calls.
            /// </summary>
            /// <param name="timeoutMs">the RPC call timeoutMs</param>
            /// <returns>this builder instance</returns>
            public T Timeout(long timeoutMs)
            {
                this.timeoutMs = timeoutMs;
                return (T)this;
            }

            /// <summary>
            /// Sets the keystore to be used with the SDK.
            /// </summary>
            /// <param name="keyStore">the key store to be used</param>
            /// <returns>this builder instance</returns>
            public T WithKeyStore(IKeyStore keyStore)
            {
                this.cryptoEngine = new TokenCryptoEngineFactory(keyStore);
                return (T)this;
            }

            /// <summary>
            /// Sets the crypto engine to be used with the SDK.
            /// </summary>
            /// <param name="cryptoEngineFactory">the crypto engine factory to use</param>
            /// <returns>this builder instance</returns>
            public T WithCryptoEngine(ICryptoEngineFactory cryptoEngineFactory)
            {
                this.cryptoEngine = cryptoEngineFactory;
                return (T)this;
            }


            public T WithFeatureCodes(params string[] featureCodes)
            {
                this.featureCodes = featureCodes.ToList();
                return (T)this;
            }

            /// <summary>
            /// Sets the developer key to be used with the SDK.
            /// </summary>
            /// <param name="devKey">the developer key</param>
            /// <returns>this builder instance</returns>
            public T DeveloperKey(string devKey)
            {
                this.devKey = devKey;
                return (T)this;
            }


            /// <summary>
            /// Sets whether the connection will allow keep-alive pings.
            /// </summary>
            /// <param name="keepAlive">whether keep-alive is enabled</param>
            /// <returns>this builder instance</returns>
            public T KeepAlive(bool keepAlive)
            {
                this.keepAlive = keepAlive;
                return (T)this;
            }

            /// <summary>
            /// Sets the keep-alive time in milliseconds.
            /// </summary>
            /// <param name="keepAliveTimeMs">keep-alive time in milliseconds</param>
            /// <returns>this builder instance</returns>
            public T KeepAliveTime(int keepAliveTimeMs)
            {
                this.keepAliveTimeMs = keepAliveTimeMs;
                return (T)this;
            }

            /// <summary>
            /// Get the headers.
            /// </summary>
            /// <returns>return metadata</returns>
            protected Metadata GetHeaders()
            {
                Metadata metadata = new Metadata();
                metadata.Add("token-sdk", "csharp");
                metadata.Add(
                "token-sdk-version",
                Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
                metadata.Add("token-dev-key", devKey);
                if (featureCodes != null)
                {
                    featureCodes.ForEach(f => metadata.Add(FEATURE_CODE_KEY, f));
                }
                return metadata;
            }


            /// <summary>
            /// Builds and returns a new <see cref="TokenClient"/> instance.
            /// </summary>
            /// <returns>the <see cref="TokenClient"/> instance</returns>
            public virtual TokenClient Build()
            {
                var metadata = GetHeaders();
                var newChannel = ManagedChannel.NewBuilder(hostName, port, useSsl)
                    .WithTimeout(timeoutMs)
                    .WithMetadata(metadata)
                    .UseKeepAlive(keepAlive)
                    .WithKeepAliveTime(keepAliveTimeMs)
                    .Build();

                return new TokenClient(
                    newChannel,
                    cryptoEngine ?? new TokenCryptoEngineFactory(new InMemoryKeyStore()),
                    tokenCluster ?? TokenCluster.SANDBOX);
            }
        }
    }
}
