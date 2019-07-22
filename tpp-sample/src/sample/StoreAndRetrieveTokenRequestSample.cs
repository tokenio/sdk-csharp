using System;
using Tokenio.Proto.Common.AliasProtos;
using ResourceType = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types.ResourceType;
using Tokenio.TokenRequests;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;
using Tokenio.Tpp.Utils;
using Tokenio.Proto.Common.TransactionProtos;

namespace TokenioSample
{
    public class StoreAndRetrieveTokenRequestSample
    {


       /// <summary>
       /// Stores the transfer token request.
       /// </summary>
       /// <returns>The transfer token request.</returns>
       /// <param name="payee">Payee.</param>
        public static string StoreTransferTokenRequest(TppMember payee)
        {
            // Create token request to be stored
            TokenRequest request = TokenRequest.TransferTokenRequestBuilder(100, "EUR")
                    .SetToMemberId(payee.MemberId())
                    .SetDescription("Book purchase") // optional description
                    .SetRedirectUrl("https://token.io/callback") // callback URL
                    .SetFromAlias(new Alias() { Value= "payer-alias@token.io",
                                                Type=Alias.Types.Type.Email
                                                })
                    .SetBankId("iron") // bank ID
                    .SetCsrfToken(Util.Nonce()) // nonce for CSRF check
                    .build();

            // Store token request
            return payee.StoreTokenRequestBlocking(request);
        }

      /// <summary>
      /// Stores the access token request.
      /// </summary>
      /// <returns>The access token request.</returns>
      /// <param name="grantee">Grantee.</param>
        public static string StoreAccessTokenRequest(TppMember grantee)
        {
            // Create token request to be stored
            TokenRequest request = TokenRequest.AccessTokenRequestBuilder(ResourceType.Accounts, ResourceType.Balances)
                    .SetToMemberId(grantee.MemberId())
                    .SetRedirectUrl("https://token.io/callback") // callback URL
                    .SetFromAlias(new Alias()
                    {
                        Value = "grantor-alias@token.io",
                        Type = Alias.Types.Type.Email
                    })
                    .SetBankId("iron") // bank ID
                    .SetCsrfToken(Util.Nonce()) // nonce for CSRF check
                    .build();

            return grantee.StoreTokenRequestBlocking(request);
        }

       /// <summary>
       /// Retrieves the token request.
       /// </summary>
       /// <returns>The token request.</returns>
       /// <param name="tokenClient">Token client.</param>
       /// <param name="requestId">Request identifier.</param>
        public static TokenRequest RetrieveTokenRequest(
                TokenClient tokenClient,
                string requestId)
        {
            return tokenClient.RetrieveTokenRequestBlocking(requestId);
        }



    }
}
