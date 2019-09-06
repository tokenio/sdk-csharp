using Tokenio.Proto.Common.AliasProtos;
using Tokenio.TokenRequests;
using Tokenio.Tpp.Utils;
using ResourceType = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types.ResourceType;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;

namespace TokenioSample
{
    /// <summary>
    /// Stores and retrieves a token request.
    /// </summary>
    public static class StoreAndRetrieveTokenRequestSample
    {
        /// <summary>
        /// Stores a transfer token request.
        /// </summary>
        /// <param name="payee">Payee Token member (the member requesting the transfer token be created)</param>
        /// <returns>a token request id</returns>
        public static string StoreTransferTokenRequest(TppMember payee)
        {
            // Create token request to be stored
            TokenRequest request = TokenRequest.TransferTokenRequestBuilder(100, "EUR")
                    .SetToMemberId(payee.MemberId())
                    .SetDescription("Book purchase") // optional description
                    .SetRedirectUrl("https://token.io/callback") // callback URL
                    .SetFromAlias(new Alias
                    {
                        Value = "payer-alias@token.io",
                        Type = Alias.Types.Type.Email
                    })
                    .SetBankId("iron") // bank ID
                    .SetCsrfToken(Util.Nonce()) // nonce for CSRF check
                    .Build();

            // Store token request
            return payee.StoreTokenRequestBlocking(request);
        }

        /// <summary>
        /// Stores an access token request.
        /// </summary>
        /// <param name="grantee">Token member requesting the access token be created</param>
        /// <returns>a token request id</returns>
        public static string StoreAccessTokenRequest(TppMember grantee)
        {
            // Create token request to be stored
            TokenRequest request = TokenRequest.AccessTokenRequestBuilder(ResourceType.Accounts, ResourceType.Balances)
                    .SetToMemberId(grantee.MemberId())
                    .SetRedirectUrl("https://token.io/callback") // callback URL
                    .SetFromAlias(new Alias
                    {
                        Value = "grantor-alias@token.io",
                        Type = Alias.Types.Type.Email
                    })
                    .SetBankId("iron") // bank ID
                    .SetCsrfToken(Util.Nonce()) // nonce for CSRF check
                    .Build();

            return grantee.StoreTokenRequestBlocking(request);
        }

        /// <summary>
        /// Retrieves a token request.
        /// </summary>
        /// <param name="tokenClient">tokenIO instance to use</param>
        /// <param name="requestId">id of request to retrieve</param>
        /// <returns>token request that was stored with the request id</returns>
        public static TokenRequest RetrieveTokenRequest(
                TokenClient tokenClient,
                string requestId)
        {
            return tokenClient.RetrieveTokenRequestBlocking(requestId);
        }
    }
}
