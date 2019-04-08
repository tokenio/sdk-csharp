using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Rpc;
using Tokenio.Security;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using WebUtility = System.Net.WebUtility;

namespace Tokenio
{
    public class TokenClient : IDisposable
    {
        private static readonly string TOKEN_REQUEST_TEMPLATE =
            "https://{0}/request-token/{1}?state={2}";

        private readonly ManagedChannel channel;
        private readonly TokenCluster tokenCluster;
        private readonly ICryptoEngineFactory cryptoEngineFactory;

        /// <summary>
        /// Creates an instance of a Token SDK.
        /// </summary>
        /// <param name="channel">the gRPC channel</param>
        /// <param name="cryptoEngineFactory">the crypto factory to create crypto engine</param>
        /// <param name="tokenCluster">the token cluster to connect to</param>
        internal TokenClient(
            ManagedChannel channel,
            ICryptoEngineFactory cryptoEngineFactory,
            TokenCluster tokenCluster)
        {
            this.channel = channel;
            this.cryptoEngineFactory = cryptoEngineFactory;
            this.tokenCluster = tokenCluster;
        }
        
        /// <summary>
        /// Creates a new <see cref="Builder"/> instance that is used to configure and
        /// </summary>
        /// <returns>the builder</returns>
        public static Builder NewBuilder()
        {
            return new Builder();
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="TokenIO"/> that's configured to use
        /// the specified environment.
        /// </summary>
        /// <param name="cluster">the token cluster to connect to</param>
        /// <param name="developerKey">the developer key</param>
        /// <returns>an instance of <see cref="TokenIO"/></returns>
        public static TokenClient Create(TokenCluster cluster, string developerKey)
        {
            return NewBuilder()
                .ConnectTo(cluster)
                .DeveloperKey(developerKey)
                .Build();
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
        /// Checks if a given alias already exists.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>true if alias exists, false otherwise</returns>
        [Obsolete("Deprecated. Use ResolveAlias instead.")]
        public Task<Boolean> AliasExists(Alias alias)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.ResolveAlias(alias)
                .Map(mem => mem != null);
        }
        
        /// <summary>
        /// Checks if a given alias already exists.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>true if alias exists, false otherwise</returns>
        [Obsolete("Deprecated. Use ResolveAliasBlocking instead.")]
        public bool AliasExistsBlocking(Alias alias)
        {
            return AliasExists(alias).Result;
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
        /// </summary>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias
        /// will be created with the member</param>
        /// <param name="createMemberType">the type of member to register</param>
        /// <returns>the created member</returns>
        public Task<Member> CreateMember(Alias alias = null, CreateMemberType createMemberType = CreateMemberType.Personal)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated
                .CreateMemberId(createMemberType)
                .FlatMap(memberId =>
                {
                    var crypto = cryptoEngineFactory.Create(memberId);
                    var operations = new List<MemberOperation>
                    {
                        Util.ToAddKeyOperation(crypto.GenerateKey(Level.Privileged)),
                        Util.ToAddKeyOperation(crypto.GenerateKey(Level.Standard)),
                        Util.ToAddKeyOperation(crypto.GenerateKey(Level.Low))
                    };
                    var metadata = new List<MemberOperationMetadata>();
                    if (alias != null)
                    {
                        operations.Add(Util.ToAddAliasOperation(alias.ToNormalized()));
                        metadata.Add(Util.ToAddAliasMetadata(alias.ToNormalized()));
                    }

                    var signer = crypto.CreateSigner(Level.Privileged);
                    return unauthenticated.CreateMember(memberId, operations, metadata, signer);
                })
                .Map(member =>
                {
                    var crypto = cryptoEngineFactory.Create(member.Id);
                    var client = ClientFactory.Authenticated(channel, member.Id, crypto);
                    return new Member(client);
                });
        }

        /// <summary>
        /// Creates a new Token member with a set of auto-generated keys, an alias, and member type.
        /// </summary>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias
        /// will be created with the member</param>
        /// <param name="createMemberType">the type of member to register</param>
        /// <returns>the created member</returns>
        public Member CreateMemberBlocking(
            Alias alias = null,
            CreateMemberType createMemberType = CreateMemberType.Personal)
        {
            return CreateMember(alias, createMemberType).Result;
        }

        /// <summary>
        /// Creates a new business-use Token member with a set of auto-generated keys and alias.
        /// </summary>
        /// <param name="alias">the alias to be associated with member</param>
        /// <returns>the created member</returns>
        public Task<Member> CreateBusinessMember(Alias alias)
        {
            return CreateMember(alias, CreateMemberType.Business);
        }
        
        /// <summary>
        /// Creates a new business-use Token member with a set of auto-generated keys and alias.
        /// </summary>
        /// <param name="alias">the alias to be associated with member</param>
        /// <returns>the created member</returns>
        public Member CreateBusinessMemberBlocking(Alias alias)
        {
            return CreateBusinessMember(alias).Result;
        }

        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">the member ID</param>
        /// <returns>the member</returns>
        public Task<Member> GetMember(string memberId)
        {
            var crypto = cryptoEngineFactory.Create(memberId);
            var client = ClientFactory.Authenticated(channel, memberId, crypto);
            return client
                .GetMember(memberId)
                .Map(member => new Member(client));
        }
        
        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">the member ID</param>
        /// <returns>the member</returns>
        public Member GetMemberBlocking(string memberId)
        {
            return GetMember(memberId).Result;
        }

        /// <summary>
        /// Returns a token request for a specified token request id.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <returns>the token request</returns>
        public Task<TokenRequest> RetrieveTokenRequest(string requestId)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.RetrieveTokenRequest(requestId)
                .Map(tokenRequest => TokenRequest.Create(
                    tokenRequest.RequestPayload, 
                    tokenRequest.RequestOptions));
        }
        
        /// <summary>
        /// Returns a token request for a specified token request id.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <returns>the token request</returns>
        public TokenRequest RetrieveTokenRequestBlocking(string requestId)
        {
            return RetrieveTokenRequest(requestId).Result;
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
        public Task<Member> CompleteRecovery(
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
                    var client = ClientFactory.Authenticated(channel, member.Id, cryptoEngine);
                    return new Member(client);
                });
        }

        /// <summary>
        /// Completes account recovery.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="recoveryOperations">the member recovery operations</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>the new member</returns>
        public Member CompleteRecoveryBlocking(
            string memberId,
            IList<MemberRecoveryOperation> recoveryOperations,
            Key privilegedKey,
            ICryptoEngine cryptoEngine)
        {
            return CompleteRecovery(memberId, recoveryOperations, privilegedKey, cryptoEngine).Result;
        }

        /// <summary>
        /// Completes account recovery if the default recovery rule was set.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the code</param>
        /// <returns>the new member</returns>
        public Task<Member> CompleteRecoveryWithDefaultRule(
            string memberId,
            string verificationId,
            string code)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            var cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
            return unauthenticated
                .CompleteRecoveryWithDefaultRule(memberId, verificationId, code, cryptoEngine)
                .Map(member =>
                {
                    var client = ClientFactory.Authenticated(channel, member.Id, cryptoEngine);
                    return new Member(client);
                });
        }

        /// <summary>
        /// Completes account recovery if the default recovery rule was set.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the code</param>
        /// <returns>the new member</returns>
        public Member CompleteRecoveryWithDefaultRuleBlocking(
            string memberId,
            string verificationId,
            string code)
        {
            return CompleteRecoveryWithDefaultRule(memberId, verificationId, code).Result;
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
        /// Generates a Token request URL from a request ID, an original state and a CSRF token.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <param name="state">the state</param>
        /// <param name="csrfToken">the csrf token</param>
        /// <returns>the token request url</returns>
        public Task<string> GenerateTokenRequestUrl(
            string requestId,
            string state = "",
            string csrfToken = "")
        {
            var csrfTokenHash = Util.HashString(csrfToken);
            var tokenRequestState = TokenRequestState.Create(csrfTokenHash, state);
            return Task.FromResult(string.Format(
                TOKEN_REQUEST_TEMPLATE,
                tokenCluster.WebAppUrl,
                requestId,
                WebUtility.UrlEncode(tokenRequestState.Serialize())));
        }

        /// <summary>
        /// Generates a Token request URL from a request ID, an original state and a CSRF token.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <param name="state">the state</param>
        /// <param name="csrfToken">the csrf token</param>
        /// <returns>the token request url</returns>
        public string GenerateTokenRequestUrlBlocking(
            string requestId,
            string state = "",
            string csrfToken = "")
        {
            return GenerateTokenRequestUrl(requestId, state, csrfToken).Result;
        }

        /// <summary>
        /// Parse the token request callback URL to extract the state and the token ID. Verify that the
        /// state contains the CSRF token hash and that the signature on the state and CSRF token is
        /// valid.
        /// </summary>
        /// <param name="callbackUrl">the token request callback url</param>
        /// <param name="csrfToken">the csrf token</param>
        /// <returns>an instance of <see cref="TokenRequestCallback"/></returns>
        public Task<TokenRequestCallback> ParseTokenRequestCallbackUrl(
            string callbackUrl,
            string csrfToken = "")
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetTokenMember()
                .Map(member =>
                {
                    var parameters = TokenRequestCallbackParameters.Create(callbackUrl);
                    var state = TokenRequestState.ParseFrom(parameters.SerializedState);
                    if (!state.CsrfTokenHash.Equals(Util.HashString(csrfToken)))
                    {
                        throw new InvalidStateException(csrfToken);
                    }

                    var payload = new TokenRequestStatePayload
                    {
                        TokenId = parameters.TokenId,
                        State = WebUtility.UrlEncode(parameters.SerializedState)
                    };

                    Util.VerifySignature(member, payload, parameters.Signature);

                    return TokenRequestCallback.Create(parameters.TokenId, state.InnerState);
                });
        }

        /// <summary>
        /// Parse the token request callback URL to extract the state and the token ID. Verify that the
        /// state contains the CSRF token hash and that the signature on the state and CSRF token is
        /// valid.
        /// </summary>
        /// <param name="callbackUrl">the token request callback url</param>
        /// <param name="csrfToken">the csrf token</param>
        /// <returns>an instance of <see cref="TokenRequestCallback"/></returns>
        public TokenRequestCallback ParseTokenRequestCallbackUrlBlocking(
            string callbackUrl,
            string csrfToken = "")
        {
            return ParseTokenRequestCallbackUrl(callbackUrl, csrfToken).Result;
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
        
        /// <summary>
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request result</returns>
        public Task<TokenRequestResult> GetTokenRequestResult(string tokenRequestId)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetTokenRequestResult(tokenRequestId);
        }

        /// <summary>
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request result</returns>
        public TokenRequestResult GetTokenRequestResultBlocking(string tokenRequestId)
        {
            return GetTokenRequestResult(tokenRequestId).Result;
        }

        public void Dispose()
        {
            channel.Dispose();
        }
        
        public class Builder
        {
            private static readonly string DEFAULT_DEV_KEY = "4qY7lqQw8NOl9gng0ZHgT4xdiDqxqoGVutuZwrUYQsI";
            private static readonly long DEFAULT_TIMEOUT_MS = 10_000L;
            private static readonly int DEFAULT_SSL_PORT = 443;

            private int port;
            private bool useSsl;
            private TokenCluster tokenCluster;
            private string hostName;
            private long timeoutMs;
            private ICryptoEngineFactory cryptoEngine;
            private string devKey;

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
            public Builder HostName(string hostName)
            {
                this.hostName = hostName;
                return this;
            }

            /// <summary>
            /// Sets the port of the Token Gateway Service to connect to.
            /// </summary>
            /// <param name="port">the port number</param>
            /// <returns>this builder instance</returns>
            public Builder Port(int port)
            {
                this.port = port;
                this.useSsl = port == DEFAULT_SSL_PORT;
                return this;
            }

            /// <summary>
            /// Sets Token cluster to connect to.
            /// </summary>
            /// <param name="cluster">the token cluster</param>
            /// <returns>this builder instance</returns>
            public Builder ConnectTo(TokenCluster cluster)
            {
                this.tokenCluster = cluster;
                this.hostName = cluster.Url;
                return this;
            }

            /// <summary>
            /// Sets timeoutMs that is used for the RPC calls.
            /// </summary>
            /// <param name="timeoutMs">the RPC call timeoutMs</param>
            /// <returns>this builder instance</returns>
            public Builder Timeout(long timeoutMs)
            {
                this.timeoutMs = timeoutMs;
                return this;
            }

            /// <summary>
            /// Sets the keystore to be used with the SDK.
            /// </summary>
            /// <param name="keyStore">the key store to be used</param>
            /// <returns>this builder instance</returns>
            public Builder WithKeyStore(IKeyStore keyStore)
            {
                this.cryptoEngine = new TokenCryptoEngineFactory(keyStore);
                return this;
            }

            /// <summary>
            /// Sets the crypto engine to be used with the SDK.
            /// </summary>
            /// <param name="cryptoEngineFactory">the crypto engine factory to use</param>
            /// <returns>this builder instance</returns>
            public Builder WithCryptoEngine(ICryptoEngineFactory cryptoEngineFactory)
            {
                this.cryptoEngine = cryptoEngineFactory;
                return this;
            }

            /// <summary>
            /// Sets the developer key to be used with the SDK.
            /// </summary>
            /// <param name="devKey">the developer key</param>
            /// <returns>this builder instance</returns>
            public Builder DeveloperKey(string devKey)
            {
                this.devKey = devKey;
                return this;
            }

            /// <summary>
            /// Builds and returns a new <see cref="TokenClient"/> instance.
            /// </summary>
            /// <returns>the <see cref="TokenClient"/> instance</returns>
            public TokenClient Build()
            {
                var channel = new Channel(hostName, port, useSsl ? new SslCredentials() : ChannelCredentials.Insecure);
                Interceptor[] interceptors =
                {
                    new AsyncTimeoutInterceptor(timeoutMs),
                    new AsyncMetadataInterceptor(metadata =>
                    {
                        metadata.Add("token-sdk", "csharp");
                        metadata.Add(
                            "token-sdk-version",
                            Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
                        metadata.Add("token-dev-key", devKey);
                        return metadata;
                    })
                };
                var newChannel = new ManagedChannel(channel, interceptors);

                return new TokenClient(
                    newChannel,
                    cryptoEngine ?? new TokenCryptoEngineFactory(new InMemoryKeyStore()),
                    tokenCluster ?? TokenCluster.SANDBOX);
            }
        }
        
    }
}
