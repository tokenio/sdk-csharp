using System;
using Google.Protobuf;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;

namespace Tokenio
{
    public class TokenRequest
    {
        private TokenRequestPayload tokenRequestPayload;
        private TokenRequestOptions tokenRequestOptions;

        public TokenRequestOptions GetTokenRequestOptions()
        {
            return tokenRequestOptions;
        }

        public TokenRequestPayload GetTokenRequestPayload()
        {
            return tokenRequestPayload;
        }

        /// <summary>
        /// Create a new Builder instance for an access token request.
        /// </summary>
        /// <param name="resources">access token resources</param>
        /// <returns>Builder instance</returns>
        public static AccessBuilder AccessTokenRequestBuilder(
            params TokenRequestPayload.Types.AccessBody.Types.ResourceType[] resources)
        {
            return new AccessBuilder(resources);
        }
        
        /// <summary>
        /// Create a new Builder instance for an transfer token request.
        /// </summary>
        /// <param name="amount">lifetime amount of the token request</param>
        /// <param name="currency">currency of the token request</param>
        /// <returns>Builder instance</returns>
        public static TransferBuilder TransferTokenRequestBuilder(double amount, string currency)
        {
            return new TransferBuilder(amount, currency);
        }

        private TokenRequest(
            TokenRequestPayload payload,
            TokenRequestOptions options)
        {
            tokenRequestOptions = options;
            tokenRequestPayload = payload;
        }

        /// <summary>
        /// Creates an instance from TokenRequestPayload and TokenRequestOptions protos.
        /// </summary>
        /// <param name="tokenRequestPayload">TokenRequestPayload</param>
        /// <param name="tokenRequestOptions">TokenRequestOptions</param>
        /// <returns></returns>
        public static TokenRequest fromProtos(
            TokenRequestPayload tokenRequestPayload,
            TokenRequestOptions tokenRequestOptions)
        {
            return new TokenRequest(tokenRequestPayload, tokenRequestOptions);
        }

        public class Builder<T> where T : Builder<T>
        {
            protected TokenRequestPayload requestPayload;
            protected TokenRequestOptions requestOptions;
            protected string oauthState;
            protected string csrfToken;

            public Builder()
            {
                requestOptions = new TokenRequestOptions();
                requestPayload = new TokenRequestPayload();
                requestOptions.From = new TokenMember();
                requestPayload.To = new TokenMember();
            }

            /// <summary>
            /// Optional. Sets the bank ID in order to bypass the Token Bank selection UI.
            /// </summary>
            /// <param name="bankId">bank id</param>
            /// <returns>builder</returns>
            public T SetBankId(string bankId)
            {
                requestOptions.BankId = bankId;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the payer/grantor member ID in order to bypass Token email input UI.
            /// </summary>
            /// <param name="fromMemberId">payer/grantor member ID</param>
            /// <returns>builder</returns>
            public T SetFromMemberId(string fromMemberId)
            {
                requestOptions.From.Id = fromMemberId;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the payer/grantor alias in order to bypass the Token email input UI.
            /// </summary>
            /// <param name="fromAlias">payer/grantor alias</param>
            /// <returns>builder</returns>
            public T SetFromAlias(Alias fromAlias)
            {
                requestOptions.From.Alias = fromAlias;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the account ID of the source bank account.
            /// </summary>
            /// <param name="sourceAccountId">source bank account ID</param>
            /// <returns>builder</returns>
            public T SetSourceAccount(string sourceAccountId)
            {
                requestOptions.SourceAccountId = sourceAccountId;
                return (T)this;
            }

            /// <summary>
            /// Optional. True if a receipt should be sent to the payee/grantee's default
            /// receipt email/SMS/etc.
            /// </summary>
            /// <param name="receiptRequested">receipt requested flag</param>
            /// <returns>builder</returns>
            public T SetReceiptRequested(bool receiptRequested)
            {
                requestOptions.ReceiptRequested = receiptRequested;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the ID used to track a member claimed by a TPP.
            /// </summary>
            /// <param name="refId">user ref ID</param>
            /// <returns>builder</returns>
            public T SetUserRefId(string refId)
            {
                requestPayload.UserRefId = refId;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the ID used to customize the UI of the web-app.
            /// </summary>
            /// <param name="customizationId">customization ID</param>
            /// <returns>builder</returns>
            public T SetCustomizationId(string customizationId)
            {
                requestPayload.CustomizationId = customizationId;
                return (T)this;
            }

            /// <summary>
            /// Sets the callback URL to the server that will initiate redemption of the token.
            /// </summary>
            /// <param name="redirectUrl">redirect url</param>
            /// <returns>builder</returns>
            public T SetRedirectUrl(string redirectUrl)
            {
                requestPayload.RedirectUrl = redirectUrl;
                return (T)this;
            }

            /// <summary>
            /// Sets the reference ID of the token.
            /// </summary>
            /// <param name="refId">token ref ID</param>
            /// <returns>builder</returns>
            public T SetRefId(string refId)
            {
                requestPayload.RefId = refId;
                return (T)this;
            }

            /// <summary>
            /// Sets the alias of the payee/grantee.
            /// </summary>
            /// <param name="toAlias">to alias</param>
            /// <returns>builder</returns>
            public T SetToAlias(Alias toAlias)
            {
                requestPayload.To.Alias = toAlias;
                return (T)this;
            }

            /// <summary>
            /// Sets the memberId of the payee/grantee.
            /// </summary>
            /// <param name="memberId">memberId</param>
            /// <returns>builder</returns>
            public T SetToMemberId(string memberId)
            {
                requestPayload.To.Id = memberId;
                return (T)this;
            }

            /// <summary>
            /// Sets acting as on the token.
            /// </summary>
            /// <param name="actingAs">entity the redeemer is acting on behalf of</param>
            /// <returns>builder</returns>
            public T SetActingAs(ActingAs actingAs)
            {
                requestPayload.ActingAs = actingAs;
                return (T)this;
            }

            /// <summary>
            /// Sets the description.
            /// </summary>
            /// <param name="description">description</param>
            /// <returns>builder</returns>
            public T SetDescription(string description)
            {
                requestPayload.Description = description;
                return (T)this;
            }

            /// <summary>
            /// Sets a developer-specified string that allows state to be persisted
            /// between the the request and callback phases of the flow.
            /// </summary>
            /// <param name="state">state</param>
            /// <returns>builder</returns>
            public T SetState(string state)
            {
                oauthState = state;
                return (T)this;
            }

            /// <summary>
            /// A nonce that will be verified in the callback phase of the flow.
            /// Used for CSRF attack mitigation.
            /// </summary>
            /// <param name="csrfToken">CSRF token</param>
            /// <returns>builder</returns>
            public T SetCsrfToken(string csrfToken)
            {
                this.csrfToken = csrfToken;
                return (T)this;
            }

            /// <summary>
            /// Builds the Token payload
            /// </summary>
            /// <returns>TokenRequest instance</returns>
            public TokenRequest build()
            {
                string serializeState = TokenRequestState.Create(
                    csrfToken == null ? "" : Util.HashString(csrfToken),
                    oauthState ?? "").Serialize();
                requestPayload.CallbackState = serializeState;
                return new TokenRequest(
                    requestPayload,
                    requestOptions);
            }
            
        }

        public class AccessBuilder : Builder<AccessBuilder>
        {
            public AccessBuilder(params TokenRequestPayload.Types.AccessBody.Types.ResourceType[] resources)
            {
                requestPayload.AccessBody = new TokenRequestPayload.Types.AccessBody
                {
                    Type = { resources }
                };
            }
        }

        public class TransferBuilder : Builder<TransferBuilder>
        {
            public TransferBuilder(double amount, string currency)
            {
                requestPayload.TransferBody = new TokenRequestPayload.Types.TransferBody
                {
                    LifetimeAmount = amount.ToString("F"),
                    Currency = currency
                };
            }

            /// <summary>
            /// Optional. Sets the destination country in order to narrow down
            /// the country selection in the web-app UI.
            /// </summary>
            /// <param name="destinationCountry">destination country</param>
            /// <returns>builder</returns>
            public TransferBuilder SetDestinationCountry(string destinationCountry)
            {
                requestPayload.DestinationCountry = destinationCountry;
                return this;
            }

            /// <summary>
            /// Adds a transfer destination to a transfer token request.
            /// </summary>
            /// <param name="destination">destination</param>
            /// <returns>builder</returns>
            public TransferBuilder AddDestination(TransferDestination destination)
            {
                if (requestPayload.TransferBody.Instructions == null)
                {
                    requestPayload.TransferBody.Instructions = new TransferInstructions();                    
                }             
                requestPayload.TransferBody.Instructions.TransferDestinations.Add(destination);
                return this;
            }

            /// <summary>
            /// Adds a transfer destination to a transfer token request.
            /// </summary>
            /// <param name="destination">destination</param>
            /// <returns>builder</returns>
            [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
            public TransferBuilder AddDestination(TransferEndpoint destination)
            {
                requestPayload.TransferBody.Destinations.Add(destination);
                return this;
            }

            /// <summary>
            /// Sets the maximum amount per charge on a transfer token request.
            /// </summary>
            /// <param name="chargeAmount">amount</param>
            /// <returns>builder</returns>
            public TransferBuilder SetChargeAmount(double chargeAmount)
            {
                requestPayload.TransferBody.Amount = chargeAmount.ToString("F");
                return this;
            }

            /// <summary>
            /// Optional. Set the bearer for any Foreign Exchange fees incurred on the transfer.
            /// </summary>
            /// <param name="chargeBearer">Bearer of the charges for any Fees related to the transfer.</param>
            /// <returns>builder</returns>
            public TransferBuilder SetChargeBearer(ChargeBearer chargeBearer)
            {
                if (requestPayload.TransferBody.Instructions == null)
                {
                    requestPayload.TransferBody.Instructions = new TransferInstructions();
                }

                if (requestPayload.TransferBody.Instructions.Metadata == null)
                {
                    requestPayload.TransferBody.Instructions.Metadata = new TransferInstructions.Types.Metadata();
                }

                requestPayload.TransferBody.Instructions.Metadata.ChargeBearer = chargeBearer;
                return this;
            }
        }
    }
}