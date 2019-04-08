using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using TokenRequest = Tokenio.TokenRequest;

namespace samples
{
    public class StoreAndRetrieveTokenRequestSample
    {

        public static string StoreTransferTokenRequest(Member payee)
        {
            var request = TokenRequest.transferTokenRequestBuilder(100, "EUR")
                .SetToMemberId(payee.MemberId())
                .SetDescription("Book purchase")
                .SetRedirectUrl("https://token.io/callback")
                .SetFromAlias(new Alias
                {
                    Value = "payer-alias@token.io,",
                    Type = Alias.Types.Type.Email,
                })
                .SetBankId("iron")
                .SetCsrfToken(Util.Nonce())
                .build();

            return payee.StoreTokenRequestBlocking(request);
        }

        public static string storeAccessTokenRequest(Member grantee)
        {
            var request = TokenRequest.accessTokenRequestBuilder(
                    TokenRequestPayload.Types.AccessBody.Types.ResourceType.Accounts,
                    TokenRequestPayload.Types.AccessBody.Types.ResourceType.Balances)
                .SetToMemberId(grantee.MemberId())
                .SetRedirectUrl("https://token.io/callback")
                .SetFromAlias(new Alias
                {
                    Value = "grantor-alias@token.io",
                    Type = Alias.Types.Type.Email
                })
                .SetBankId("iron")
                .SetCsrfToken(Util.Nonce())
                .build();

            return grantee.StoreTokenRequestBlocking(request);
        }

        public static TokenRequest retireveTokenRequest(TokenClient tokenClient, string requestId)
        {
            return null; //tokenClient.RetrieveTokenRequestBlocking(requestId);
        }
    }
}