using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Rpc;
using Tokenio.Security;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio
{
    [Obsolete("deprecated, use TokenClient instead")]
    public class TokenIOAsync : IDisposable
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
        internal TokenIOAsync(
            ManagedChannel channel,
            ICryptoEngineFactory cryptoEngineFactory,
            TokenCluster tokenCluster)
        {
            this.channel = channel;
            this.cryptoEngineFactory = cryptoEngineFactory;
            this.tokenCluster = tokenCluster;
        }

        /// <summary>
        /// Returns a synchronous version of the API.
        /// </summary>
        /// <returns>the synchronous API</returns>
        public TokenIO Sync()
        {
            return new TokenIO(this);
        }

        /// <summary>
        /// Checks if a given alias already exists.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>true if alias exists, false otherwise</returns>
        public Task<Boolean> AliasExists(Alias alias)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.ResolveAlias(alias)
                .Map(mem => mem != null);
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
        /// Creates a new Token member with a set of auto-generated keys, an alias, and member type.
        /// </summary>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias
        /// will be created with the member</param>
        /// <param name="createMemberType">the type of member to register</param>
        /// <returns>the created member</returns>
        public Task<MemberAsync> CreateMember(Alias alias, CreateMemberType createMemberType)
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
                    return new MemberAsync(client);
                });
        }

        /// <summary>
        /// Creates a new personal-use Token member with a set of auto generated keys and no alias.
        /// </summary>
        /// <returns>the created member</returns>
        public Task<MemberAsync> CreateMember()
        {
            return CreateMember(null, CreateMemberType.Personal);
        }

        /// <summary>
        /// Creates a new personal-use Token member with a set of auto generated keys and the
        /// given alias.
        /// </summary>
        /// <param name="alias">the member alias to use, must be unique</param>
        /// <returns>the created member</returns>
        public Task<MemberAsync> CreateMember(Alias alias)
        {
            return CreateMember(alias, CreateMemberType.Personal);
        }

        /// <summary>
        /// Creates a new business-use Token member with a set of auto-generated keys and alias.
        /// </summary>
        /// <param name="alias">the alias to be associated with member</param>
        /// <returns>the created member</returns>
        public Task<MemberAsync> CreateBusinessMember(Alias alias)
        {
            return CreateMember(alias, CreateMemberType.Business);
        }

        /// <summary>
        /// Provisions a new device for an existing user. The call generates a set of keys
        /// that are returned back. The keys need to be approved by an existing device/keys.
        /// </summary>
        /// <param name="alias">the alias to provision the device for</param>
        /// <returns>information of the device</returns>
        public Task<DeviceInfo> ProvisionDevice(Alias alias)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated
                .GetMemberId(alias)
                .Map(memberId =>
                {
                    if (memberId == null)
                    {
                        throw new ArgumentException($"Alias:{alias.Value} is not found");
                    }

                    var crypto = cryptoEngineFactory.Create(memberId);
                    return new DeviceInfo(memberId, new List<Key>
                    {
                        crypto.GenerateKey(Level.Privileged),
                        crypto.GenerateKey(Level.Standard),
                        crypto.GenerateKey(Level.Low)
                    });
                });
        }

        /// <summary>
        /// Return a MemberAsync set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">the member ID</param>
        /// <returns>the member</returns>
        public Task<MemberAsync> GetMember(string memberId)
        {
            var crypto = cryptoEngineFactory.Create(memberId);
            var client = ClientFactory.Authenticated(channel, memberId, crypto);
            return client
                .GetMember(memberId)
                .Map(member => new MemberAsync(client));
        }

        /// <summary>
        /// Returns a token request for a specified token request id.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <returns>the token request</returns>
        public Task<TokenRequest> RetrieveTokenRequest(string requestId)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.RetrieveTokenRequest(requestId);
        }

        /// <summary>
        /// Notifies to add a key.
        /// </summary>
        /// <param name="alias">alias to notify</param>
        /// <param name="keys">keys that need approval</param>
        /// <param name="deviceMetadata">device metadata of the keys</param>
        /// <returns>status of the notification</returns>
        public Task<NotifyStatus> NotifyAddKey(
            Alias alias,
            IList<Key> keys,
            DeviceMetadata deviceMetadata)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            var addKey = new AddKey
            {
                Keys = {keys},
                DeviceMetadata = deviceMetadata
            };
            return unauthenticated.NotifyAddKey(alias, addKey);
        }

        /// <summary>
        /// Sends a notification to request a payment.
        /// </summary>
        /// <param name="tokenPayload">the payload of a token to be sent</param>
        /// <returns>status of the notification request</returns>
        public Task<NotifyStatus> NotifyPaymentRequest(TokenPayload tokenPayload)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            if (string.IsNullOrEmpty(tokenPayload.RefId))
            {
                tokenPayload.RefId = Util.Nonce();
            }

            return unauthenticated.NotifyPaymentRequest(tokenPayload);
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
        /// Create a recovery authorization for some agent to sign.
        /// </summary>
        /// <param name="memberId">the ID of the member we claim to be.</param>
        /// <param name="privilegedKey">the new privileged key we want to use.</param>
        /// <returns>the authorization</returns>
        public Task<MemberRecoveryOperation.Types.Authorization> CreateRecoveryAuthorization(
            string memberId,
            Key privilegedKey)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.CreateRecoveryAuthorization(memberId, privilegedKey);
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
        /// Completes account recovery.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="recoveryOperations">the member recovery operations</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>the new member</returns>
        public Task<MemberAsync> CompleteRecovery(
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
                    return new MemberAsync(client);
                });
        }

        /// <summary>
        /// Completes account recovery if the default recovery rule was set.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the code</param>
        /// <returns>the new member</returns>
        public Task<MemberAsync> CompleteRecoveryWithDefaultRule(
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
                    return new MemberAsync(client);
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
        /// Generates a Token request URL from a request ID, and state. This does not set a CSRF token
        /// or pass in a state.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <returns>the token request url</returns>
        public Task<string> GenerateTokenRequestUrl(string requestId)
        {
            return GenerateTokenRequestUrl(requestId, string.Empty, string.Empty);
        }

        /// <summary>
        /// Generates a Token request URL from a request ID, and state. This does not set a CSRF token.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <param name="state">the state</param>
        /// <returns>the token request url</returns>
        public Task<string> GenerateTokenRequestUrl(
            string requestId,
            string state)
        {
            return GenerateTokenRequestUrl(requestId, state, string.Empty);
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
            string state,
            string csrfToken)
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
        /// Parse the token request callback URL to extract the state and the token ID. This assumes
        /// that no CSRF token was set.
        /// </summary>
        /// <param name="callbackUrl">the token request callback url</param>
        /// <returns>an instance of <see cref="TokenRequestCallback"/></returns>
        public Task<TokenRequestCallback> ParseTokenRequestCallbackUrl(string callbackUrl)
        {
            return ParseTokenRequestCallbackUrl(callbackUrl, string.Empty);
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
            string csrfToken)
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
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request result</returns>
        public Task<TokenRequestResult> GetTokenRequestResult(string tokenRequestId)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetTokenRequestResult(tokenRequestId);
        }

        public void Dispose()
        {
            channel.Dispose();
        }
    }
}
