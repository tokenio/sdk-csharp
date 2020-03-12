using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.ProviderSpecific;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.User.Utils;
using RequestBodyCase = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.RequestBodyOneofCase;

namespace Tokenio.User
{
    /// <summary>
    /// This class is used to build a transfer token.The required parameters are member, amount (which
    /// is the lifetime amount of the token), and currency.One source of funds must be set: either
    /// accountId or BankAuthorization. Finally, a redeemer must be set, specified by either alias
    /// or memberId.
    /// </summary>
    public sealed class StandingOrderTokenBuilder
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int REF_ID_MAX_LENGTH = 18;

        private readonly TokenPayload payload;

        /// <summary>
        /// Creates the builder object.
        /// </summary>
        /// <param name="member">payer of the token</param>
        /// <param name="amount">amount per charge of the standing order token</param>
        /// <param name="currency">currency of the token</param>
        /// <param name="frequency">ISO 20022 code for the frequency of the standing order:
        ///            DAIL, WEEK, TOWK, MNTH, TOMN, QUTR, SEMI, YEAR</param>
        /// <param name="startDate">start date of the standing order</param>
        /// <param name="endDate">end date of the standing order</param>
        public StandingOrderTokenBuilder(
            Member member,
            double amount,
            string currency,
            string frequency,
            DateTime startDate,
            DateTime? endDate = null)
        {
            this.payload = new TokenPayload
            {
                Version = "1.0",
                From = new TokenMember
                {
                    Id = member.MemberId()
                },
                StandingOrder = new StandingOrderBody
                {
                    Currency = currency,
                    Amount = amount.ToString(),
                    Frequency = frequency,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate == null ? "" : endDate.Value.ToString("yyyy-MM-dd")
                }
            };

            IList<Alias> aliases = member.GetAliasesBlocking();
            if (aliases.Count > 0)
            {
                payload.From.Alias = aliases[0];
            }
        }

        /// <summary>
        /// Creates the builder object from a token request.
        /// </summary>
        /// <param name="tokenRequest">token request</param>
        public StandingOrderTokenBuilder(TokenRequest tokenRequest)
        {
            if (tokenRequest.RequestPayload.RequestBodyCase != RequestBodyCase.StandingOrderBody)
            {
                throw new ArgumentException(
                    "Require token request with standing order body.");
            }

            if (tokenRequest.RequestPayload.To == null)
            {
                throw new ArgumentException("No payee on token request");
            }

            StandingOrderBody body = tokenRequest.RequestPayload
                .StandingOrderBody;
            this.payload = new TokenPayload
            {
                Version = "1.0",
                RefId = tokenRequest.RequestPayload.RefId,
                From = tokenRequest.RequestOptions.From,
                To = tokenRequest.RequestPayload.To,
                Description = tokenRequest.RequestPayload.Description,
                ReceiptRequested = tokenRequest.RequestOptions.ReceiptRequested,
                TokenRequestId = tokenRequest.Id,
                StandingOrder = body
            };
            if (tokenRequest.RequestPayload.ActingAs != null)
            {
                this.payload.ActingAs = tokenRequest.RequestPayload.ActingAs;
            }
        }

        /// <summary>
        /// Sets the expiration date.
        /// </summary>
        /// <param name="expiresAtMs">expiration date in ms.</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetExpiresAtMs(long expiresAtMs)
        {
            payload.ExpiresAtMs = expiresAtMs;
            return this;
        }

        /// <summary>
        /// Sets the effective date.
        /// </summary>
        /// <param name="effectiveAtMs"></param>
        /// <returns></returns>
        public StandingOrderTokenBuilder SetEffectiveAtMs(long effectiveAtMs)
        {
            payload.EffectiveAtMs = effectiveAtMs;
            return this;
        }

        /// <summary>
        /// Sets the time after which endorse is no longer possible.
        /// </summary>
        /// <param name="endorseUntilMs">endorse until, in milliseconds.</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetEndorseUntilMs(long endorseUntilMs)
        {
            payload.EndorseUntilMs = endorseUntilMs;
            return this;
        }

        /// <summary>
        /// Sets the description.
        /// </summary>
        /// <param name="description">description</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetDescription(string description)
        {
            payload.Description = description;
            return this;
        }

        /// <summary>
        /// Adds a transfer source.
        /// </summary>
        /// <param name="source">source</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetSource(TransferEndpoint source)
        {
            var instructions = payload.StandingOrder.Instructions;
            if (instructions == null)
            {
                instructions = new TransferInstructions();
            }

            instructions.Source = source;
            payload.StandingOrder.Instructions = instructions;
            return this;
        }

        /// <summary>
        /// Adds a linked source account to the token.
        /// </summary>
        /// <param name="accountId">source accountId</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetAccountId(string accountId)
        {
            if (string.IsNullOrEmpty(payload.From.Id))
            {
                throw new InvalidOperationException();
            }

            SetSource(new TransferEndpoint
            {
                Account = new BankAccount
                {
                    Token = new BankAccount.Types.Token
                    {
                        AccountId = accountId,
                        MemberId = payload.From.Id
                    }
                }
            });
            return this;
        }

        /// <summary>
        /// Adds a transfer destination.
        /// </summary>
        /// <param name="destination">destination</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder AddDestination(TransferDestination destination)
        {
            var instructions = payload.StandingOrder.Instructions;
            if (instructions == null)
            {
                instructions = new TransferInstructions { };
            }

            instructions.TransferDestinations.Add(destination);
            payload.StandingOrder.Instructions = instructions;
            return this;
        }

        /// <summary>
        /// Sets the alias of the payee.
        /// </summary>
        /// <param name="toAlias">alias</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetToAlias(Alias toAlias)
        {
            payload.To = new TokenMember
            {
                Alias = toAlias
            };
            return this;
        }

        /// <summary>
        /// Sets the memberId of the payee.
        /// </summary>
        /// <param name="toMemberId">memberId</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetToMemberId(string toMemberId)
        {
            payload.To = new TokenMember
            {
                Id = toMemberId
            };
            return this;
        }

        /// <summary>
        /// Sets the reference ID of the token.
        /// </summary>
        /// <param name="refId">the reference Id, at most 18 characters long</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetRefId(string refId)
        {
            if (refId.Length > REF_ID_MAX_LENGTH)
            {
                throw new ArgumentException(string.Format(
                    "The length of the refId is at most {0}, got: {1}",
                    REF_ID_MAX_LENGTH,
                    refId.Length));
            }

            payload.RefId = refId;
            return this;
        }

        /// <summary>
        /// Sets acting as on the token.
        /// </summary>
        /// <param name="actingAs">entity the redeemer is acting on behalf of</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetActingAs(ActingAs actingAs)
        {
            payload.ActingAs = actingAs;
            return this;
        }

        /// <summary>
        /// Sets the token request ID.
        /// </summary>
        /// <param name="tokenRequestId">token request id</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetTokenRequestId(string tokenRequestId)
        {
            payload.TokenRequestId = tokenRequestId;
            return this;
        }

        /// <summary>
        /// Sets the flag indicating whether a receipt is requested.
        /// </summary>
        /// <param name="receiptRequested">receipt requested flag</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetReceiptRequested(bool receiptRequested)
        {
            payload.ReceiptRequested = receiptRequested;
            return this;
        }

        /// <summary>
        ///  Sets provider transfer metadata.
        /// </summary>
        /// <param name="metadata">the metadata</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetProviderTransferMetadata(
            ProviderTransferMetadata metadata)
        {
            payload.StandingOrder
                .Instructions
                .Metadata
                .ProviderTransferMetadata = metadata;
            return this;
        }

        /// <summary>
        /// Sets the ultimate party to which the money is due.
        /// </summary>
        /// <param name="ultimateCreditor">the ultimate creditor</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetUltimateCreditor(string ultimateCreditor)
        {
            payload.StandingOrder
                .Instructions
                .Metadata
                .UltimateCreditor = ultimateCreditor;
            return this;
        }

        /// <summary>
        /// Sets ultimate party that owes the money to the (ultimate) creditor.
        /// </summary>
        /// <param name="ultimateDebtor">the ultimate debtor</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetUltimateDebtor(string ultimateDebtor)
        {
            payload.StandingOrder
                .Instructions
                .Metadata
                .UltimateDebtor = ultimateDebtor;
            return this;
        }

        /// <summary>
        /// Sets the purpose code. Refer to ISO 20022 external code sets.
        /// </summary>
        /// <param name="purposeCode">the purpose code</param>
        /// <returns>builder</returns>
        public StandingOrderTokenBuilder SetPurposeCode(string purposeCode)
        {
            payload.StandingOrder
                .Instructions
                .Metadata
                .PurposeCode = purposeCode;
            return this;
        }

        /// <summary>
        /// Builds a token payload, without uploading blobs or attachments.
        /// </summary>
        /// <returns>payload</returns>
        public TokenPayload BuildPayload()
        {
            if (string.IsNullOrEmpty(payload.RefId))
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Util.Nonce();
            }

            return payload;
        }
    }
}