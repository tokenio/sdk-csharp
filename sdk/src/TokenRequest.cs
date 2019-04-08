using System;
using System.Collections.Generic;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;

namespace Tokenio
{
    public class TokenRequest
    {
        private TokenRequestPayload tokenRequestPayload;
        private Proto.Common.TokenProtos.TokenRequestOptions tokenRequestOptions;

        public Proto.Common.TokenProtos.TokenRequestOptions GetTokenRequestOptions()
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
        public static AccessBuilder accessTokenRequestBuilder(
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
        public static TransferBuilder transferTokenRequestBuilder(double amount, string currency)
        {
            return new TransferBuilder(amount, currency);
        }

        private TokenRequest(
            TokenRequestPayload payload,
            Proto.Common.TokenProtos.TokenRequestOptions options)
        {
            this.tokenRequestOptions = options;
            this.tokenRequestPayload = payload;
        }

        /// <summary>
        /// Creates an instance from TokenRequestPayload and TokenRequestOptions protos.
        /// </summary>
        /// <param name="tokenRequestPayload">TokenRequestPayload</param>
        /// <param name="tokenRequestOptions">TokenRequestOptions</param>
        /// <returns></returns>
        public static TokenRequest Create(
            TokenRequestPayload tokenRequestPayload,
            Proto.Common.TokenProtos.TokenRequestOptions tokenRequestOptions)
        {
            return new TokenRequest(tokenRequestPayload, tokenRequestOptions);
        }

        public class Builder
        {
            protected TokenRequestPayload requestPayload;
            protected Proto.Common.TokenProtos.TokenRequestOptions requestOptions;
            protected string oauthState;
            protected string csrfToken;

            public Builder()
            {
                this.requestOptions = new Proto.Common.TokenProtos.TokenRequestOptions();
                this.requestPayload = new TokenRequestPayload();
            }

            /// <summary>
            /// Optional. Sets the bank ID in order to bypass the Token Bank selection UI.
            /// </summary>
            /// <param name="bankId">bank id</param>
            /// <returns>builder</returns>
            public Builder SetBankId(string bankId)
            {
                this.requestOptions.BankId = bankId;
                return this;
            }

            /// <summary>
            /// Optional. Sets the payer/grantor member ID in order to bypass Token email input UI.
            /// </summary>
            /// <param name="fromMemberId">payer/grantor member ID</param>
            /// <returns>builder</returns>
            public Builder SetFromMemberId(string fromMemberId)
            {
                this.requestOptions.From.Id = fromMemberId;
                return this;
            }

            /// <summary>
            /// Optional. Sets the payer/grantor alias in order to bypass the Token email input UI.
            /// </summary>
            /// <param name="fromAlias">payer/grantor alias</param>
            /// <returns>builder</returns>
            public Builder SetFromAlias(Alias fromAlias)
            {
                this.requestOptions.From.Alias = fromAlias;
                return this;
            }

            /// <summary>
            /// Optional. Sets the account ID of the source bank account.
            /// </summary>
            /// <param name="sourceAccountId">source bank account ID</param>
            /// <returns>builder</returns>
            public Builder SetSourceAccount(string sourceAccountId)
            {
                this.requestOptions.SourceAccountId = sourceAccountId;
                return this;
            }

            /// <summary>
            /// Optional. True if a receipt should be sent to the payee/grantee's default
            /// receipt email/SMS/etc.
            /// </summary>
            /// <param name="receiptRequested">receipt requested flag</param>
            /// <returns>builder</returns>
            public Builder SetReceiptRequested(bool receiptRequested)
            {
                this.requestOptions.ReceiptRequested = receiptRequested;
                return this;
            }

            /// <summary>
            /// Optional. Sets the ID used to track a member claimed by a TPP.
            /// </summary>
            /// <param name="refId">user ref ID</param>
            /// <returns>builder</returns>
            public Builder SetUserRefId(string refId)
            {
                this.requestPayload.UserRefId = refId;
                return this;
            }

            /// <summary>
            /// Optional. Sets the ID used to customize the UI of the web-app.
            /// </summary>
            /// <param name="customizationId">customization ID</param>
            /// <returns>builder</returns>
            public Builder SetCustomizationId(string customizationId)
            {
                this.requestPayload.CustomizationId = customizationId;
                return this;
            }

            /// <summary>
            /// Sets the callback URL to the server that will initiate redemption of the token.
            /// </summary>
            /// <param name="redirectUrl">redirect url</param>
            /// <returns>builder</returns>
            public Builder SetRedirectUrl(string redirectUrl)
            {
                this.requestPayload.RedirectUrl = redirectUrl;
                return this;
            }

            /// <summary>
            /// Sets the reference ID of the token.
            /// </summary>
            /// <param name="refId">token ref ID</param>
            /// <returns>builder</returns>
            public Builder SetRefId(string refId)
            {
                this.requestPayload.RefId = refId;
                return this;
            }

            /// <summary>
            /// Sets the alias of the payee/grantee.
            /// </summary>
            /// <param name="toAlias">to alias</param>
            /// <returns>builder</returns>
            public Builder SetToAlias(Alias toAlias)
            {
                this.requestPayload.To.Alias = toAlias;
                return this;
            }

            /// <summary>
            /// Sets the memberId of the payee/grantee.
            /// </summary>
            /// <param name="memberId">memberId</param>
            /// <returns>builder</returns>
            public Builder SetToMemberId(string memberId)
            {
                this.requestPayload.To.Id = memberId;
                return this;
            }

            /// <summary>
            /// Sets acting as on the token.
            /// </summary>
            /// <param name="actingAs">entity the redeemer is acting on behalf of</param>
            /// <returns>builder</returns>
            public Builder SetActingAs(ActingAs actingAs)
            {
                this.requestPayload.ActingAs = actingAs;
                return this;
            }

            /// <summary>
            /// Sets the description.
            /// </summary>
            /// <param name="description">description</param>
            /// <returns>builder</returns>
            public Builder SetDescription(string description)
            {
                this.requestPayload.Description = description;
                return this;
            }

            /// <summary>
            /// Sets a developer-specified string that allows state to be persisted
            /// between the the request and callback phases of the flow.
            /// </summary>
            /// <param name="state">state</param>
            /// <returns>builder</returns>
            public Builder SetState(string state)
            {
                this.oauthState = state;
                return this;
            }

            /// <summary>
            /// A nonce that will be verified in the callback phase of the flow.
            /// Used for CSRF attack mitigation.
            /// </summary>
            /// <param name="csrfToken">CSRF token</param>
            /// <returns>builder</returns>
            public Builder SetCsrfToken(string csrfToken)
            {
                this.csrfToken = csrfToken;
                return this;
            }

            /// <summary>
            /// Builds the Token payload
            /// </summary>
            /// <returns>TokenRequest instance</returns>
            public TokenRequest build()
            {
                string serializeState = TokenRequestState.Create(
                    this.csrfToken == null ? "" : Util.HashString(this.csrfToken),
                    oauthState ?? "").Serialize();
                requestPayload.CallbackState = serializeState;
                return new TokenRequest(
                    requestPayload,
                    requestOptions);
            }
            
        }

        public class AccessBuilder : Builder
        {
            public AccessBuilder(params TokenRequestPayload.Types.AccessBody.Types.ResourceType[] resources)
            {
                this.requestPayload.AccessBody = new TokenRequestPayload.Types.AccessBody
                {
                    Type = { resources }
                };
            }
        }

        public class TransferBuilder : Builder
        {
            public TransferBuilder(double amount, string currency)
            {
                this.requestPayload.TransferBody = new TokenRequestPayload.Types.TransferBody
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
                this.requestPayload.DestinationCountry = destinationCountry;
                return this;
            }

            /// <summary>
            /// Adds a transfer destination to a transfer token request.
            /// </summary>
            /// <param name="destination">destination</param>
            /// <returns>builder</returns>
            public TransferBuilder AddDestination(TransferEndpoint destination)
            {
                this.requestPayload.TransferBody.Destinations.Add(destination);
                return this;
            }

            /// <summary>
            /// Sets the maximum amount per charge on a transfer token request.
            /// </summary>
            /// <param name="chargeAmount">amount</param>
            /// <returns>builder</returns>
            public TransferBuilder SetChargeAmount(double chargeAmount)
            {
                this.requestPayload.TransferBody.Amount = chargeAmount.ToString("F");
                return this;
            }
        }
    }
}