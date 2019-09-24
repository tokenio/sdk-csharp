using System.Collections.Generic;
using System.Threading.Tasks;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using Tokenio.TokenRequests;
using Tokenio.Tpp.Rpc;
using Tokenio.Tpp.TokenRequests;
using Tokenio.Tpp.Utils;
using ManagedChannel = Tokenio.Rpc.ManagedChannel;
using TokenRequestStatePayload = Tokenio.Proto.Common.TokenProtos.TokenRequestStatePayload;
using WebUtility = System.Net.WebUtility;

namespace Tokenio.Tpp
{
    public class TokenClient : Tokenio.TokenClient
    {
        private static readonly string TOKEN_REQUEST_TEMPLATE =
            "https://{0}/request-token/{1}?state={2}";

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
             : base(channel, cryptoEngineFactory, tokenCluster) { }


        /// <summary>
        /// Creates a new <see cref="Builder"/> instance that is used to configure and
        /// build a <see cref="TokenClient"/> instance.
        /// </summary>
        /// <returns>the builder</returns>
        public static Builder NewBuilder()
        {
            return new Builder();
        }

        /// <summary>
        /// Creates a new instance of <see cref="TokenClient"/> that's configured to use
        /// the specified environment.
        /// </summary>
        /// <param name="cluster">the token cluster to connect to</param>
        /// <param name="developerKey">the developer key</param>
        /// <returns>an instance of <see cref="TokenClient"/></returns>
        public static TokenClient Create(TokenCluster cluster, string developerKey)
        {
            return NewBuilder()
            .ConnectTo(cluster)
            .DeveloperKey(developerKey)
            .Build();
        }


        /// <summary>
        /// Creates a new Token member with a set of auto-generated keys, an alias, and member type.
        /// </summary>        
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias will
        ///    be created with the member..</param>
        /// <param name="partnerId">of partner member.</param>
        /// <param name="realmId">member Id of existing member to which this new member is associated with</param>
        /// <returns>newly created member</returns>
        public Task<Member> CreateMember(
          Alias alias,
          string partnerId = null,
          string realmId = null)
        {
            return CreateMemberImpl(alias, CreateMemberType.Business, null, partnerId, realmId)
                .Map(member =>
                {
                    var crypto = cryptoEngineFactory.Create(member.MemberId());
                    var client = ClientFactory.Authenticated(channel, member.MemberId(), crypto);
                    return new Member(member.MemberId(), client, tokenCluster, partnerId, realmId);
                });
        }



        /// <summary>
        /// Creates a new business-use Token member with a set of auto-generated keys and and an alias.
        /// </summary>
        /// <returns>newly created member</returns>
        /// <param name="alias">alias to associate with member.</param>
        /// <param name="partnerId">ID of partner member</param>
        public Member CreateMemberBlocking(
          Alias alias,
          string partnerId = null)
        {
            return CreateMember(alias, partnerId, null).Result;
        }

        /// <summary>
        /// Creates a new Token member in the provided realm with a set of auto-generated keys, an alias,
        /// and member type.
        /// </summary>
        /// <returns>newly created member</returns>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias will
        ///    be created with the member..</param>
        /// <param name="realmId">member id of the Member whose realm this new Member belongs..</param>
        public Member CreateMemberInRealmBlocking(Alias alias, string realmId)
        {
            return CreateMember(alias, null, realmId).Result;
        }


        /// <summary>
        /// Sets up a member given a specific ID of a member that already exists in the system. If
        /// the member ID already has keys, this will not succeed.Used for testing since this
        /// gives more control over the member creation process.
        /// Adds an alias and a set of auto-generated keys to the member.
        /// </summary>
        /// <returns>newly created member</returns>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias will
        ///     be created with the member.</param>
        public Task<Member> SetUpMember(string memberId,
        Alias alias)
        {
            return SetUpMemberImpl(memberId, alias)
                .Map(member =>
                {
                    var crypto = cryptoEngineFactory.Create(member.MemberId());
                    var client = ClientFactory.Authenticated(channel, member.MemberId(), crypto);
                    return new Member(member.MemberId(), client,
                        tokenCluster, member.PartnerId(), member.RealmId());
                });
        }

        /// <summary>
        /// Sets up member blocking.
        /// </summary>
        /// <returns>The up member blocking.</returns>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="alias">Alias.</param>
        public Member SetUpMemberBlocking(string memberId,
          Alias alias = null)
        {
            return SetUpMember(memberId, alias).Result;
        }


        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <returns>The member.</returns>
        /// <param name="memberId">Member identifier.</param>
        public Task<Member> GetMember(string memberId)
        {
            var crypto = cryptoEngineFactory.Create(memberId);
            var client = ClientFactory.Authenticated(channel, memberId, crypto);

            return GetMemberImpl(memberId, client)
             .Map(member =>
             {

                 return new Member(member.MemberId(), client,
                     tokenCluster, member.PartnerId(), member.RealmId());
             });
        }

        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <returns>The member blocking.</returns>
        /// <param name="memberId">Member identifier.</param>
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
                .Map(tokenRequest => TokenRequest.FromProtos(
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
        /// Completes the recovery.
        /// </summary>
        /// <returns>The recovery.</returns>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="recoveryOperations">Recovery operations.</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">Crypto engine.</param>
        public Task<Member> CompleteRecovery(
                           string memberId,
                           IList<MemberRecoveryOperation> recoveryOperations,
                           Key privilegedKey,
                           ICryptoEngine cryptoEngine)
        {
            return CompleteRecoveryImpl(memberId, recoveryOperations, privilegedKey, cryptoEngine)
                .Map(member =>
                {
                    var client = ClientFactory.Authenticated(channel, member.MemberId(), cryptoEngine);
                    return new Member(member.MemberId(), client,
                        tokenCluster, member.PartnerId(), member.RealmId());
                });
        }

        /// <summary>
        /// Completes the recovery blocking.
        /// </summary>
        /// <returns>The recovery blocking.</returns>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="recoveryOperations">the member Recovery operations.</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">New Crypto engine.</param>
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
                string code,
                ICryptoEngine cryptoEngine)
        {

            return CompleteRecoveryWithDefaultRuleImpl(memberId, verificationId, code, cryptoEngine)
                .Map(member =>
                {
                    var client = ClientFactory.Authenticated(channel, member.MemberId(), cryptoEngine);
                    return new Member(member.MemberId(), client,
                        tokenCluster, member.PartnerId(), member.RealmId());
                });
        }

        /// <summary>
        /// Completes the recovery with default rule blocking.
        /// </summary>
        /// <returns>The recovery with default rule blocking.</returns>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="verificationId">Verification identifier.</param>
        /// <param name="code">Code.</param>
        public Member CompleteRecoveryWithDefaultRuleBlocking(
                string memberId,
                string verificationId,
                string code,
                ICryptoEngine cryptoEngine)
        {
            return CompleteRecoveryWithDefaultRule(memberId, verificationId, code, cryptoEngine).Result;
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
                        //ToDo(RD-2410): Remove WebUtility.UrlEncode call. It's only for backward compatibility with the old Token Request Flow.
                        State = WebUtility.UrlEncode(parameters.SerializedState)
                    };

                    Util.VerifySignature(member, payload, parameters.Signature);
                    return TokenRequestCallback.Create(
                        parameters.TokenId,
                        state.InnerState);
                });
        }

        /// <summary>
        /// Parse the Set Transfer Destinations Url callback parameters to extract state,
        /// region and supported.Check the CSRF token against the initial request and verify
        /// the signature.
        /// </summary>
        /// <param name="url">token request callback url</param>
        /// <returns>TokenRequestSetTransferDestinationUrl object containing the token id and
        ///    the original state</returns>
        public TokenRequestTransferDestinationsCallbackParameters ParseSetTransferDestinationsUrl(
                 string url)
        {
            return TokenRequestTransferDestinationsCallbackParameters.Create(url);
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

        public class Builder : Builder<Builder>
        {
            /// <inheritdoc />
            protected override string GetPlatform()
            {
                return "csharp-tpp";
            }

            /// <summary>
            /// Creates new builder instance with the defaults initialized.
            /// </summary>
            /// <returns></returns>
            public new TokenClient Build()
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
