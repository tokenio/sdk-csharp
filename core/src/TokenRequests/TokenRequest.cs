using System;
using System.Collections.Generic;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.ProviderSpecific;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Utils;
using static Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types;
using static Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types.AccountResourceList.Types;

namespace Tokenio.TokenRequests
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
            params ResourceType[] resources)
        {
            return new AccessBuilder(resources);
        }
        
        /// <summary>
        /// Create a new Builder instance for an access token request with account-specific resources.
        /// </summary>
        /// <param name="list">list of account-specific access token resources</param>
        /// <returns>Builder instance</returns>
        public static AccessBuilder AccessTokenRequestBuilder(AccountResourceList list)
        {
            return new AccessBuilder(list);
        }
        
        /// <summary>
        /// Create a Builder instance for a funds confirmation request.
        /// </summary>
        /// <param name="bankId">bank ID</param>
        /// <param name="account">the user's account</param>
        /// <returns>Builder instance</returns>
        public static AccessBuilder FundsConfirmationRequestBuilder(
                string bankId,
                BankAccount account)
        {
            return FundsConfirmationRequestBuilder(bankId, account, null);
        }
        
        /// <summary>
        /// Create a Builder instance for a funds confirmation request.
        /// </summary>
        /// <param name="bankId">bank ID</param>
        /// <param name="account">the user's account</param>
        /// <param name="data">optional customer data</param>
        /// <returns>Builder instance</returns>
        public static AccessBuilder FundsConfirmationRequestBuilder(
                string bankId,
                BankAccount account,
                CustomerData data)
        {
            AccountResource builder = new AccountResource
            {
                BankAccount = account,
                Type = AccountResourceType.AccountFundsConfirmation
            };
            if (data != null)
            {
                builder.CustomerData = data;
            }
            var list = new AccountResourceList();
            list.Resources.Add(builder);
            return new AccessBuilder(list)
                    .SetBankId(bankId);
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

        /// <summary>
        /// Create a new Builder instance for a standing order token request.
        /// 
        /// </summary>
        /// <param name="amount">amount per charge</param>
        /// <param name="currency">currency per charge</param>
        /// <param name="frequency">frequency of the standing order. ISO 20022: DAIL, WEEK, TOWK,
        ///                     MNTH, TOMN, QUTR, SEMI, YEAR </param>
        /// <param name="startDate">start date of the standing order. ISO 8601: YYYY-MM-DD or YYYYMMDD.</param>
        /// <param name="endDate">end date of the standing order. ISO 8601: YYYY-MM-DD or YYYYMMDD.</param>
        /// <param name="destinations">destination account of the standing order</param>
        /// <returns>Builder instance</returns>
        public static StandingOrderBuilder StandingOrderRequestBuilder(
            double amount,
            string currency,
            string frequency,
            string startDate,
            string endDate,
            IList<TransferDestination> destinations)
        {
            return new StandingOrderBuilder(
                amount,
                currency,
                frequency,
                startDate,
                endDate,
                destinations);
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
        public static TokenRequest FromProtos(
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

            protected Builder()
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
            public TokenRequest Build()
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
            public AccessBuilder(params ResourceType[] resources)
            {
                var resourcesList = new ResourceTypeList();
                resourcesList.Resources.Add(resources);
                requestPayload.AccessBody = new TokenRequestPayload.Types.AccessBody
                {
                    ResourceTypeList = resourcesList
                };
            }

            public AccessBuilder(AccountResourceList list)
            {
                this.requestPayload.AccessBody = new TokenRequestPayload.Types.AccessBody
                {
                    AccountResourceList = list
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
                    Currency = currency,
                    Instructions = new TransferInstructions
                    {
                        Metadata = new TransferInstructions.Types.Metadata()
                    }
                };
            }

            /// <summary>
            /// Optional. Sets the source account to bypass account selection. May be required for
            /// some banks.
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public TransferBuilder SetSource(TransferEndpoint source)
            {
                requestPayload.TransferBody
                    .Instructions
                    .Source = source;
                return this;
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
            public TransferBuilder AddDestination(TransferEndpoint destination)
            {
                requestPayload.TransferBody.Destinations.Add(destination);
                return this;
            }

            /// <summary>
            /// Adds a transfer destination to a transfer token request.
            /// </summary>
            /// <param name="destination">destination</param>
            /// <returns>builder</returns>
            public TransferBuilder AddDestination(TransferDestination destination)
            {
                requestPayload.TransferBody.Instructions.TransferDestinations.Add(destination);
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
            /// Sets the execution date of the transfer. Used for future-dated payments.
            /// </summary>
            /// <param name="executionDate">execution date : ISO 8601 YYYY-MM-DD</param>
            /// <returns>builder</returns>
            public TransferBuilder SetExecutionDate(String  executionDate)
            {
                this.requestPayload.TransferBody
                    .ExecutionDate = executionDate;
                return this;
            }

            /// <summary>
            /// Optional. Adds metadata for a specific provider.
            /// </summary>
            /// <param name="metadata">provider-specific metadata</param>
            /// <returns>builder</returns>
            public TransferBuilder SetProviderMetadata(ProviderTransferMetadata metadata)
            {
                requestPayload.TransferBody.Instructions.Metadata.ProviderTransferMetadata = metadata;
                return this;
            }

            /// <summary>
            /// Optional. Set the bearer for any Foreign Exchange fees incurred on the transfer.
            /// </summary>
            /// <param name="chargeBearer">Bearer of the charges for any Fees related to the transfer.</param>
            /// <returns>builder</returns>
            public TransferBuilder SetChargeBearer(ChargeBearer chargeBearer)
            {
                this.requestPayload.TransferBody
                        .Instructions
                        .Metadata
                        .ChargeBearer = chargeBearer;
                return this;
            }

            /// <summary>
            /// Optional. In the scenario where TPP wishes to know the user's selection of country and
            /// bank, TPP should provide this url so that Token can make a call with relevant
            /// information as parameters. TPP can use that information to set transfer destination.
            /// </summary>
            /// <param name="url">URL</param>
            /// <returns>builder</returns>
            public TransferBuilder SetSetTransferDestinationsUrl(string url)
            {
                this.requestPayload.TransferBody
                    .SetTransferDestinationsUrl = url;
                return this;
            }

            /// <summary>
            /// Optional. Sets the ultimate party to which the money is due.
            /// </summary>
            /// <param name="ultimateCreditor">the ultimate creditor</param>
            /// <returns>builder</returns>
            public TransferBuilder SetUltimateCreditor(string ultimateCreditor)
            {
                this.requestPayload.TransferBody
                        .Instructions
                        .Metadata
                        .UltimateCreditor = ultimateCreditor;
                return this;
            }

            /// <summary>
            /// Optional. Sets ultimate party that owes the money to the (ultimate) creditor.
            /// </summary>
            /// <param name="ultimateDebtor">the ultimate debtor</param>
            /// <returns>builder</returns>
            public TransferBuilder SetUltimateDebtor(string ultimateDebtor)
            {
                this.requestPayload.TransferBody
                        .Instructions
                        .Metadata
                        .UltimateCreditor = ultimateDebtor;
                return this;
            }

            /// <summary>
            /// Optional. Sets the purpose code. Refer to ISO 20022 external code sets.
            /// </summary>
            /// <param name="purposeCode">the purpose code</param>
            /// <returns>builder</returns>
            public TransferBuilder SetPurposeCode(string purposeCode)
            {
                this.requestPayload.TransferBody
                        .Instructions
                        .Metadata
                        .PurposeCode = purposeCode;
                return this;
            }
            
            /// <summary>
            /// Optional. Sets whether CAF should be attempted before transfer.
            /// </summary>
            /// <param name="confirmFunds"> whether to attempt CAF before transfer</param>
            /// <returns>builder</returns>
            public TransferBuilder SetConfirmFunds(bool confirmFunds)
            {
                this.requestPayload.TransferBody
                        .ConfirmFunds = confirmFunds;
                return this;
            }
        }

        public class StandingOrderBuilder : Builder<StandingOrderBuilder>
        {
            internal StandingOrderBuilder()
            {
                this.requestPayload.StandingOrderBody = new StandingOrderBody();
            }

            internal StandingOrderBuilder(
                double amount,
                string currency,
                string frequency,
                string startDate,
                string endDate,
                IList<TransferDestination> destinations)
            {
                this.requestPayload.StandingOrderBody = new StandingOrderBody
                {
                    Amount = amount.ToString(),
                    Currency = currency,
                    Frequency = frequency,
                    StartDate = startDate,
                    EndDate = endDate,
                    Instructions = new TransferInstructions
                    {
                        TransferDestinations = { destinations }
                    }
                };
            }

            /// <summary>
            /// Sets the amount per charge of the standing order.
            /// </summary>
            /// <param name="amount">amount per individual charge</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetAmount(double amount)
            {
                this.requestPayload.StandingOrderBody
                    .Amount = amount.ToString();
                return this;
            }

            /// <summary>
            /// Sets the currency for each charge in the standing order.
            /// </summary>
            /// <param name="currency">currency</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetCurrency(string currency)
            {
                this.requestPayload.StandingOrderBody
                    .Currency = currency;
                return this;
            }

            /// <summary>
            /// Sets the frequency of the standing order. ISO 20022: DAIL, WEEK, TOWK,
            /// MNTH, TOMN, QUTR, SEMI, YEAR
            /// </summary>
            /// <param name="frequency">frequency of the standing order</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetFrequency(string frequency)
            {
                this.requestPayload.StandingOrderBody
                    .Frequency = frequency;
                return this;
            }

            /// <summary>
            /// Sets the start date of the standing order. ISO 8601: YYYY-MM-DD or YYYYMMDD.
            /// </summary>
            /// <param name="startDate">start date of the standing order</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetStartDate(string startDate)
            {
                this.requestPayload.StandingOrderBody
                    .StartDate = startDate;
                return this;
            }

            /// <summary>
            /// Sets the end date of the standing order. ISO 8601: YYYY-MM-DD or YYYYMMDD.
            /// If not specified, the standing order will occur indefinitely.
            /// </summary>
            /// <param name="endDate">end date of the standing order</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetEndDate(string endDate)
            {
                this.requestPayload.StandingOrderBody
                    .EndDate = endDate;
                return this;
            }

            /// <summary>
            /// Adds a destination account to a standing order token request.
            /// </summary>
            /// <param name="destination">destination</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder AddDestination(TransferDestination destination)
            {
                this.requestPayload.StandingOrderBody.Instructions
                    .TransferDestinations.Add(destination);
                return this;
            }

            /// <summary>
            /// Optional. Sets the destination country in order to narrow down
            /// the country selection in the web-app UI.
            /// </summary>
            /// <param name="destinationCountry">destination country</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetDestinationCountry(string destinationCountry)
            {
                this.requestPayload.DestinationCountry = destinationCountry;
                return this;
            }

            /// <summary>
            /// Optional. Sets the source account to bypass account selection.
            /// </summary>
            /// <param name="source">source</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetSource(TransferEndpoint source)
            {
                this.requestPayload.StandingOrderBody
                    .Instructions
                    .Source = source;
                return this;
            }

            /// <summary>
            /// Optional. Adds metadata for a specific provider.
            /// </summary>
            /// <param name="metadata">provider-specific metadata</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetProviderTransferMetadata(ProviderTransferMetadata metadata)
            {
                this.requestPayload.StandingOrderBody
                    .Instructions
                    .Metadata
                    .ProviderTransferMetadata = metadata;
                return this;
            }

            /// <summary>
            /// Optional. Sets the ultimate party to which the money is due.
            /// </summary>
            /// <param name="ultimateCreditor">the ultimate creditor</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetUltimateCreditor(string ultimateCreditor)
            {
                this.requestPayload.StandingOrderBody
                    .Instructions
                    .Metadata
                    .UltimateCreditor = ultimateCreditor;
                return this;
            }

            /// <summary>
            /// Optional. Sets ultimate party that owes the money to the (ultimate) creditor.
            /// </summary>
            /// <param name="ultimateDebtor">the ultimate debtor</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetUltimateDebtor(string ultimateDebtor)
            {
                this.requestPayload.StandingOrderBody
                    .Instructions
                    .Metadata
                    .UltimateCreditor = ultimateDebtor;
                return this;
            }

            /// <summary>
            /// Optional. Sets the purpose code. Refer to ISO 20022 external code sets.
            /// </summary>
            /// <param name="purposeCode">the purpose code</param>
            /// <returns>builder</returns>
            public StandingOrderBuilder SetPurposeCode(string purposeCode)
            {
                this.requestPayload.StandingOrderBody
                    .Instructions
                    .Metadata
                    .PurposeCode = purposeCode;
                return this;
            }
        }
    }
}