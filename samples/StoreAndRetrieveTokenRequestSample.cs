using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using TokenRequest = Tokenio.TokenRequest;

namespace Sample {
    public class StoreAndRetrieveTokenRequestSample {

        /// <summary>
        /// Stores a transfer token request.
        /// </summary>
        /// <param name="payee">Payee Token member (the member requesting the transfer token be created)</param>
        /// <returns>a token request id</returns>
        public static string StoreTransferTokenRequest(Member payee) {
            var request = TokenRequest.TransferTokenRequestBuilder(100, "EUR")
                .SetToMemberId(payee.MemberId())
                .SetDescription("Book purchase")
                .SetRedirectUrl("https://token.io/callback")
                .SetFromAlias(new Alias {
                    Value = "payer-alias@token.io,",
                        Type = Alias.Types.Type.Email,
                })
                .SetBankId("iron")
                .SetCsrfToken(Util.Nonce())
                .AddDestination(new TransferDestination {
                    Sepa = new TransferDestination.Types.Sepa {
                        Bic = "XUIWC2489",
                            Iban = "DE89370400440532013000"
                    }
                })
                .build();

            return payee.StoreTokenRequestBlocking(request);
        }

        /// <summary>
        /// Stores an access token request.
        /// </summary>
        /// <param name="grantee">Token member requesting the access token be created</param>
        /// <returns>a token request id</returns>
        public static string StoreAccessTokenRequest(Member grantee) {
            var request = TokenRequest.AccessTokenRequestBuilder(
                    TokenRequestPayload.Types.AccessBody.Types.ResourceType.Accounts,
                    TokenRequestPayload.Types.AccessBody.Types.ResourceType.Balances)
                .SetToMemberId(grantee.MemberId())
                .SetRedirectUrl("https://token.io/callback")
                .SetFromAlias(new Alias {
                    Value = "grantor-alias@token.io",
                        Type = Alias.Types.Type.Email
                })
                .SetBankId("iron")
                .SetCsrfToken(Util.Nonce())
                .build();

            return grantee.StoreTokenRequestBlocking(request);
        }

        /// <summary>
        /// Retrieves a token request.
        /// </summary>
        /// <param name="tokenClient">tokenIO instance to use</param>
        /// <param name="requestId">id of request to retrieve</param>
        /// <returns>token request that was stored with the request id</returns>
        public static TokenRequest RetrieveTokenRequest(TokenClient tokenClient, string requestId) {
            return tokenClient.RetrieveTokenRequestBlocking(requestId);
        }
    }
}
