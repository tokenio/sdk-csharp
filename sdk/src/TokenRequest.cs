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
        public static TokenRequest fromProtos(
            TokenRequestPayload tokenRequestPayload,
            Proto.Common.TokenProtos.TokenRequestOptions tokenRequestOptions)
        {
            return new TokenRequest(tokenRequestPayload, tokenRequestOptions);
        }

        public class Builder<T> where T : Builder<T>
        {
            protected TokenRequestPayload requestPayload;
            protected Proto.Common.TokenProtos.TokenRequestOptions requestOptions;
            protected string oauthState;
            protected string csrfToken;

            public Builder()
            {
                this.requestOptions = new Proto.Common.TokenProtos.TokenRequestOptions();
                this.requestPayload = new TokenRequestPayload();
                this.requestOptions.From = new TokenMember();
                this.requestPayload.To = new TokenMember();
            }

            /// <summary>
            /// Optional. Sets the bank ID in order to bypass the Token Bank selection UI.
            /// </summary>
            /// <param name="bankId">bank id</param>
            /// <returns>builder</returns>
            public T SetBankId(string bankId)
            {
                this.requestOptions.BankId = bankId;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the payer/grantor member ID in order to bypass Token email input UI.
            /// </summary>
            /// <param name="fromMemberId">payer/grantor member ID</param>
            /// <returns>builder</returns>
            public T SetFromMemberId(string fromMemberId)
            {
                this.requestOptions.From.Id = fromMemberId;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the payer/grantor alias in order to bypass the Token email input UI.
            /// </summary>
            /// <param name="fromAlias">payer/grantor alias</param>
            /// <returns>builder</returns>
            public T SetFromAlias(Alias fromAlias)
            {
                this.requestOptions.From.Alias = fromAlias;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the account ID of the source bank account.
            /// </summary>
            /// <param name="sourceAccountId">source bank account ID</param>
            /// <returns>builder</returns>
            public T SetSourceAccount(string sourceAccountId)
            {
                this.requestOptions.SourceAccountId = sourceAccountId;
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
                this.requestOptions.ReceiptRequested = receiptRequested;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the ID used to track a member claimed by a TPP.
            /// </summary>
            /// <param name="refId">user ref ID</param>
            /// <returns>builder</returns>
            public T SetUserRefId(string refId)
            {
                this.requestPayload.UserRefId = refId;
                return (T)this;
            }

            /// <summary>
            /// Optional. Sets the ID used to customize the UI of the web-app.
            /// </summary>
            /// <param name="customizationId">customization ID</param>
            /// <returns>builder</returns>
            public T SetCustomizationId(string customizationId)
            {
                this.requestPayload.CustomizationId = customizationId;
                return (T)this;
            }

            /// <summary>
            /// Sets the callback URL to the server that will initiate redemption of the token.
            /// </summary>
            /// <param name="redirectUrl">redirect url</param>
            /// <returns>builder</returns>
            public T SetRedirectUrl(string redirectUrl)
            {
                this.requestPayload.RedirectUrl = redirectUrl;
                return (T)this;
            }

            /// <summary>
            /// Sets the reference ID of the token.
            /// </summary>
            /// <param name="refId">token ref ID</param>
            /// <returns>builder</returns>
            public T SetRefId(string refId)
            {
                this.requestPayload.RefId = refId;
                return (T)this;
            }

            /// <summary>
            /// Sets the alias of the payee/grantee.
            /// </summary>
            /// <param name="toAlias">to alias</param>
            /// <returns>builder</returns>
            public T SetToAlias(Alias toAlias)
            {
                this.requestPayload.To.Alias = toAlias;
                return (T)this;
            }

            /// <summary>
            /// Sets the memberId of the payee/grantee.
            /// </summary>
            /// <param name="memberId">memberId</param>
            /// <returns>builder</returns>
            public T SetToMemberId(string memberId)
            {
                this.requestPayload.To.Id = memberId;
                return (T)this;
            }

            /// <summary>
            /// Sets acting as on the token.
            /// </summary>
            /// <param name="actingAs">entity the redeemer is acting on behalf of</param>
            /// <returns>builder</returns>
            public T SetActingAs(ActingAs actingAs)
            {
                this.requestPayload.ActingAs = actingAs;
                return (T)this;
            }

            /// <summary>
            /// Sets the description.
            /// </summary>
            /// <param name="description">description</param>
            /// <returns>builder</returns>
            public T SetDescription(string description)
            {
                this.requestPayload.Description = description;
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
                this.oauthState = state;
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
                    this.csrfToken == null ? "" : Util.HashString(this.csrfToken),
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
                this.requestPayload.AccessBody = new TokenRequestPayload.Types.AccessBody
                {
                    Type = { resources }
                };
            }
        }

        public class TransferBuilder : Builder<TransferBuilder>
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