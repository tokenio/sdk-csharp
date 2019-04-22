using System;
using System.Collections.Generic;
using System.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Rpc;
using Tokenio.Security;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;

namespace Tokenio
{
    [Obsolete("deprecated, use TokenClient instead")]
    public class TokenIO : IDisposable
    {
        private readonly TokenIOAsync async;

        public TokenIO(TokenIOAsync async)
        {
            this.async = async;
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
        public static TokenIO Create(TokenCluster cluster, string developerKey)
        {
            return NewBuilder()
                .ConnectTo(cluster)
                .DeveloperKey(developerKey)
                .Build();
        }

        /// <summary>
        /// Creates a new instance of <see cref="TokenIOAsync"/> that's configured to use
        /// the specified environment.
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="developerKey"></param>
        /// <returns>an instance of <see cref="TokenIOAsync"/></returns>
        public static TokenIOAsync CreateAsync(TokenCluster cluster, string developerKey)
        {
            return NewBuilder()
                .ConnectTo(cluster)
                .DeveloperKey(developerKey)
                .BuildAsync();
        }


        /// <summary>
        /// Returns an async version of the API.
        /// </summary>
        /// <returns>the asynchronous version of the API</returns>
        public TokenIOAsync Async()
        {
            return async;
        }

        /// <summary>
        /// Checks if a given alias already exists.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>true if alias exists, false otherwise</returns>
        public bool AliasExists(Alias alias)
        {
            return async.AliasExists(alias).Result;
        }

        /// <summary>
        /// Looks up member id for a given alias.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>member id if alias already exists, null otherwise</returns>
        public string GetMemberId(Alias alias)
        {
            return async.GetMemberId(alias).Result;
        }

        /// <summary>
        /// Creates a new business-use Token member with a set of auto-generated keys and alias.
        /// </summary>
        /// <param name="alias">the alias to be associated with member</param>
        /// <returns>the created member</returns>
        public MemberSync CreateBusinessMember(Alias alias)
        {
            return async.CreateBusinessMember(alias)
                .Map(memberAsyc => memberAsyc.Sync())
                .Result;
        }

        /// <summary>
        /// Creates a new personal-use Token member with a set of auto generated keys and the
        /// given alias.
        /// </summary>
        /// <param name="alias">the member alias to use, must be unique</param>
        /// <returns>the created member</returns>
        public MemberSync CreateMember(Alias alias)
        {
            return async.CreateMember(alias)
                .Map(memberAsyc => memberAsyc.Sync())
                .Result;
        }

        /// <summary>
        /// Creates a new personal-use Token member with a set of auto generated keys and no alias.
        /// </summary>
        /// <returns>the created member</returns>
        public MemberSync CreateMember()
        {
            return async.CreateMember()
                .Map(memberAsyc => memberAsyc.Sync())
                .Result;
        }

        /// <summary>
        /// Provisions a new device for an existing user. The call generates a set of keys
        /// that are returned back. The keys need to be approved by an existing device/keys.
        /// </summary>
        /// <param name="alias">the alias to provision the device for</param>
        /// <returns>information of the device</returns>
        public DeviceInfo ProvisionDevice(Alias alias)
        {
            return async.ProvisionDevice(alias).Result;
        }

        /// <summary>
        /// Return a MemberAsync set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">the member ID</param>
        /// <returns>the member</returns>
        public MemberSync GetMember(string memberId)
        {
            return async.GetMember(memberId)
                .Map(memberAsyc => memberAsyc.Sync())
                .Result;
        }

        /// <summary>
        /// Returns a token request for a specified token request id.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <returns>the token request</returns>
        public TokenRequest RetrieveTokenRequest(string requestId)
        {
            return async.RetrieveTokenRequest(requestId).Result;
        }

        /// <summary>
        /// Notifies to add a key.
        /// </summary>
        /// <param name="alias">alias to notify</param>
        /// <param name="keys">keys that need approval</param>
        /// <param name="deviceMetadata">device metadata of the keys</param>
        /// <returns>status of the notification request</returns>
        public NotifyStatus NotifyAddKey(
            Alias alias,
            IList<Key> keys,
            DeviceMetadata deviceMetadata)
        {
            return async.NotifyAddKey(alias, keys, deviceMetadata).Result;
        }

        /// <summary>
        /// Sends a notification to request a payment.
        /// </summary>
        /// <param name="tokenPayload">the payload of a token to be sent</param>
        /// <returns>status of the notification request</returns>
        public NotifyStatus NotifyPaymentRequest(TokenPayload tokenPayload)
        {
            return async.NotifyPaymentRequest(tokenPayload).Result;
        }

        /// <summary>
        /// Begins account recovery.
        /// </summary>
        /// <param name="alias">the used to recover</param>
        /// <returns>the verification id</returns>
        public string BeginRecovery(Alias alias)
        {
            return async.BeginRecovery(alias).Result;
        }

        /// <summary>
        /// Create a recovery authorization for some agent to sign.
        /// </summary>
        /// <param name="memberId">the ID of the member we claim to be.</param>
        /// <param name="privilegedKey">the new privileged key we want to use.</param>
        /// <returns>the authorization</returns>
        public Authorization CreateRecoveryAuthorization(string memberId, Key privilegedKey)
        {
            return async.CreateRecoveryAuthorization(memberId, privilegedKey).Result;
        }

        /// <summary>
        /// Gets recovery authorization from Token.
        /// </summary>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the verification code</param>
        /// <param name="key">the privileged key</param>
        /// <returns>the member recovery operation</returns>
        public MemberRecoveryOperation GetRecoveryAuthorization(
            string verificationId,
            string code,
            Key key)
        {
            return async.GetRecoveryAuthorization(verificationId, code, key).Result;
        }

        /// <summary>
        /// Completes account recovery.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="recoveryOperations">the member recovery operations</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>the new member</returns>
        public MemberSync CompleteRecovery(
            string memberId,
            IList<MemberRecoveryOperation> recoveryOperations,
            Key privilegedKey,
            ICryptoEngine cryptoEngine)
        {
            return async.CompleteRecovery(memberId, recoveryOperations, privilegedKey, cryptoEngine)
                .Map(memberAsyc => memberAsyc.Sync())
                .Result;
        }

        /// <summary>
        /// Completes account recovery if the default recovery rule was set.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the code</param>
        /// <returns>the new member</returns>
        public MemberSync CompleteRecoveryWithDefaultRule(
            string memberId,
            string verificationId,
            string code)
        {
            return async.CompleteRecoveryWithDefaultRule(memberId, verificationId, code)
                .Map(memberAsyc => memberAsyc.Sync())
                .Result;
        }

        /// <summary>
        /// Returns the first 200 available banks for linking.
        /// </summary>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanks()
        {
            return GetBanks(1, 200);
        }

        /// <summary>
        /// Returns banks from a given list of bank IDs (case-insensitive).
        /// </summary>
        /// <param name="ids">the bank IDs</param>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanks(IList<string> ids)
        {
            return GetBanks(ids, null, null, null, null, null);
        }

        /// <summary>
        /// Return banks whose 'name' or 'identifier' contains the given search string (case-insensitive).
        /// </summary>
        /// <param name="search">the keyword to search for</param>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanks(string search)
        {
            return GetBanks(null, search, null, null, null, null);
        }

        /// <summary>
        /// Returns banks with specified paging information.
        /// </summary>
        /// <param name="page">the result page to retrieve</param>
        /// <param name="perPage">max number of records per page, can be at most 200</param>
        /// <returns>banks with paging information</returns>
        public PagedBanks GetBanks(
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
        public PagedBanks GetBanks(
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
        public PagedBanks GetBanks(
            IList<string> ids,
            string search,
            string country,
            int? page,
            int? perPage,
            string sort)
        {
            return async.GetBanks(ids, search, country, page, perPage, sort).Result;
        }

        /// <summary>
        /// Generates a Token request URL from a request ID, and state. This does not set a CSRF token
        /// or pass in a state.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <returns>the token request url</returns>
        public string GenerateTokenRequestUrl(string requestId)
        {
            return async.GenerateTokenRequestUrl(requestId).Result;
        }

        /// <summary>
        /// Generates a Token request URL from a request ID, and state. This does not set a CSRF token.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <param name="state">the state</param>
        /// <returns>the token request url</returns>
        public string GenerateTokenRequestUrl(string requestId, string state)
        {
            return async.GenerateTokenRequestUrl(requestId, state).Result;
        }

        /// <summary>
        /// Generates a Token request URL from a request ID, an original state and a CSRF token.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <param name="state">the state</param>
        /// <param name="csrfToken">the csrf token</param>
        /// <returns>the token request url</returns>
        public string GenerateTokenRequestUrl(string requestId, string state, string csrfToken)
        {
            return async.GenerateTokenRequestUrl(requestId, state, csrfToken).Result;
        }

        /// <summary>
        /// Parse the token request callback URL to extract the state and the token ID. This assumes
        /// that no CSRF token was set.
        /// </summary>
        /// <param name="callbackUrl">the token request callback url</param>
        /// <returns>an instance of <see cref="TokenRequestCallback"/></returns>
        public TokenRequestCallback ParseTokenRequestCallbackUrl(string callbackUrl)
        {
            return async.ParseTokenRequestCallbackUrl(callbackUrl).Result;
        }

        /// <summary>
        /// Parse the token request callback URL to extract the state and the token ID. Verify that the
        /// state contains the CSRF token hash and that the signature on the state and CSRF token is
        /// valid.
        /// </summary>
        /// <param name="callbackUrl">the token request callback url</param>
        /// <param name="csrfToken">the csrf token</param>
        /// <returns>an instance of <see cref="TokenRequestCallback"/></returns>
        public TokenRequestCallback ParseTokenRequestCallbackUrl(string callbackUrl, string csrfToken)
        {
            return async.ParseTokenRequestCallbackUrl(callbackUrl, csrfToken).Result;
        }

        /// <summary>
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request result</returns>
        public TokenRequestResult GetTokenRequestResult(string tokenRequestId)
        {
            return async.GetTokenRequestResult(tokenRequestId).Result;
        }

        public void Dispose()
        {
            async.Dispose();
        }

        public class Builder
        {
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
            /// Builds and returns a new <see cref="TokenIO"/> instance.
            /// </summary>
            /// <returns>the <see cref="TokenIO"/> instance</returns>
            public TokenIO Build()
            {
                return BuildAsync().Sync();
            }

            /// <summary>
            /// Builds and returns a new <see cref="TokenIOAsync"/> instance.
            /// </summary>
            /// <returns>the <see cref="TokenIOAsync"/> instance</returns>
            public TokenIOAsync BuildAsync()
            {
                if (devKey == null || devKey.Equals(string.Empty))
                {
                    throw new Exception("Please provide a developer key. Contact Token for more details.");
                }

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

                return new TokenIOAsync(
                    newChannel,
                    cryptoEngine ?? new TokenCryptoEngineFactory(new InMemoryKeyStore()),
                    tokenCluster ?? TokenCluster.SANDBOX);
            }
        }
    }
}
