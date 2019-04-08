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

        public static AccessBuilder accessTokenRequestBuilder(
            params  TokenRequestPayload.Types.AccessBody.Types.ResourceType[] resources)
        {
            return new AccessBuilder(resources);
        }
        
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

            public Builder SetBankId(string bankId)
            {
                this.requestOptions.BankId = bankId;
                return this;
            }

            public Builder SetFromMemberId(string fromMemberId)
            {
                this.requestOptions.From.Id = fromMemberId;
                return this;
            }

            public Builder SetFromAlias(Alias fromAlias)
            {
                this.requestOptions.From.Alias = fromAlias;
                return this;
            }

            public Builder SetSourceAccount(string sourceAccountId)
            {
                this.requestOptions.SourceAccountId = sourceAccountId;
                return this;
            }

            public Builder SetReceiptRequested(bool receiptRequested)
            {
                this.requestOptions.ReceiptRequested = receiptRequested;
                return this;
            }

            public Builder SetUserRefId(string refId)
            {
                this.requestPayload.UserRefId = refId;
                return this;
            }

            public Builder SetCustomizationId(string customizationId)
            {
                this.requestPayload.CustomizationId = customizationId;
                return this;
            }

            public Builder SetRedirectUrl(string redirectUrl)
            {
                this.requestPayload.RedirectUrl = redirectUrl;
                return this;
            }

            public Builder SetRefId(string refId)
            {
                this.requestPayload.RefId = refId;
                return this;
            }

            public Builder SetToAlias(Alias alias)
            {
                this.requestPayload.To.Alias = alias;
                return this;
            }

            public Builder SetToMemberId(string memberId)
            {
                this.requestPayload.To.Id = memberId;
                return this;
            }

            public Builder SetActingAs(ActingAs actingAs)
            {
                this.requestPayload.ActingAs = actingAs;
                return this;
            }

            public Builder SetDescription(string description)
            {
                this.requestPayload.Description = description;
                return this;
            }

            public Builder SetState(string state)
            {
                this.oauthState = state;
                return this;
            }

            public Builder SetCsrfToken(string csrfToken)
            {
                this.csrfToken = csrfToken;
                return this;
            }

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

            public TransferBuilder SetDestinationCountry(string destinationCountry)
            {
                this.requestPayload.DestinationCountry = destinationCountry;
                return this;
            }

            public TransferBuilder AddDestination(TransferEndpoint destination)
            {
                this.requestPayload.TransferBody.Destinations.Add(destination);
                return this;
            }

            public TransferBuilder SetChargeAmount(double chargeAmount)
            {
                this.requestPayload.TransferBody.Amount = chargeAmount.ToString("F");
                return this;
            }
        }
    }
}