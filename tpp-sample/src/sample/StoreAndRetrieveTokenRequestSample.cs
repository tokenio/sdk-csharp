using System.Collections.Generic;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.TokenRequests;
using Tokenio.Tpp.TokenRequests;
using Tokenio.Tpp.Utils;
using DestinationCase = Tokenio.Proto.Common.TransferInstructionsProtos.TransferDestination.DestinationOneofCase;
using ResourceType = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types.ResourceType;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
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
                .SetRefId(Util.Nonce())    
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
        /// Stores a transfer token without setting Transfer Destinations and instead providing
        /// a callback URL.
        /// </summary>
        /// <param name="payee">Payee Token member (the member requesting the transfer token be created)</param>
        /// <param name="setTransferDestinationsCallback">callback url.</param>
        /// <returns>token request id</returns>
        public static string StoreTransferTokenRequestWithDestinationsCallback(
            TppMember payee,
            string setTransferDestinationsCallback)
        {
            TokenRequest tokenRequest = TokenRequest.TransferTokenRequestBuilder(250, "EUR")
                .SetRefId(Util.Nonce())
                .SetToMemberId(payee.MemberId())
                .SetDescription("Book purchase")
                // This TPP provided url gets called by Token after the user selects bank and
                // country on the Token web app.
                .SetSetTransferDestinationsUrl(setTransferDestinationsCallback)
                // This TPP provided Redirect URL gets called after Token is ready
                // for redemption.
                .SetRedirectUrl("https://tpp-sample.com/callback")
                .SetFromAlias(new Alias
                {
                    Value = "payer-alias@token.io",
                    Type = Alias.Types.Type.Email
                })
                .SetBankId("iron") // bank ID
                .SetCsrfToken(Util.Nonce()) // nonce for CSRF check
                .Build();

            string requestId = payee.StoreTokenRequestBlocking(tokenRequest);

            return requestId;
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
                .SetRefId(Util.Nonce())
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
            Tokenio.Tpp.TokenClient tokenClient,
            string requestId)
        {
            return tokenClient.RetrieveTokenRequestBlocking(requestId);
        }

        /**
         * Sets transfer destinations for a given token request.
         *
         * @param payee Payee Token member (the member requesting the transfer token be created)
         * @param requestId token request id
         * @param tokenClient Token SDK client
         * @param setTransferDestinationsCallback callback url
         */

        /// <summary>
        /// Sets transfer destinations for a given token request.
        /// </summary>
        /// <param name="payee">Payee Token member (the member requesting the transfer token be created)</param>
        /// <param name="requestId">token request id</param>
        /// <param name="tokenClient">Token SDK client</param>
        /// <param name="setTransferDestinationsCallback">callback url</param>
        public static void SetTokenRequestTransferDestinations(
            TppMember payee,
            string requestId,
            Tokenio.Tpp.TokenClient tokenClient,
            string setTransferDestinationsCallback)
        {
            TokenRequestTransferDestinationsCallbackParameters parameters =
                tokenClient.ParseSetTransferDestinationsUrl(setTransferDestinationsCallback);

            IList<TransferDestination> transferDestinations = new List<TransferDestination>();
            if (parameters.SupportedTransferDestinationTypes
                .Contains(DestinationCase.FasterPayments))
            {
                TransferDestination destination = new TransferDestination
                {
                    FasterPayments = new TransferDestination.Types.FasterPayments
                    {
                        SortCode = Util.Nonce(),
                        AccountNumber = Util.Nonce()
                    }
                };
                transferDestinations.Add(destination);
            }
            else
            {
                transferDestinations.Add(new TransferDestination
                {
                    Sepa = new TransferDestination.Types.Sepa
                    {
                        Bic = Util.Nonce(),
                        Iban = Util.Nonce()
                    }
                });
            }

            payee.SetTokenRequestTransferDestinationsBlocking(requestId, transferDestinations);
        }
    }
}