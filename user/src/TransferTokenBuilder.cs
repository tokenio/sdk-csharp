using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.ProviderSpecific;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Utils;
using static Tokenio.Proto.Common.AccountProtos.BankAccount.Types;
using AccountCase = Tokenio.Proto.Common.AccountProtos.BankAccount.AccountOneofCase;
using ProtoToken = Tokenio.Proto.Common.TokenProtos.Token;
using TRANSFER = Tokenio.Proto.Common.TokenProtos.TokenPayload.BodyOneofCase;
using TRANSFER_BODY = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.RequestBodyOneofCase;

namespace Tokenio.User
{
    /// <summary>
    /// This class is used to build a transfer token. The required parameters are member, amount (which
    /// is the lifetime amount of the token), and currency.One source of funds must be set: either
    /// accountId or BankAuthorization. Finally, a redeemer must be set, specified by either alias
    /// or memberId.
    /// </summary>
    public sealed class TransferTokenBuilder
    {
        private static readonly ILog logger = LogManager
                .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly int REF_ID_MAX_LENGTH = 18;
        private readonly Member member;
        private readonly TokenPayload payload;
        // Token request ID
        private string tokenRequestId;

        /// <summary>
        /// Creates the builder object.
        /// </summary>
        /// <param name="member">payer of the token</param>
        /// <param name="amount">lifetime amount of the token</param>
        /// <param name="currency">currency of the token</param>
        public TransferTokenBuilder(
                Member member,
                double amount,
                string currency)
        {
            this.member = member;
            this.payload = new TokenPayload
            {
                Version = "1.0",
                Transfer = new TransferBody
                {
                    Currency = currency,
                    LifetimeAmount = amount.ToString()
                }
            };
            if (member != null)
            {
                From(member.MemberId());
                IList<Alias> aliases = member.GetAliasesBlocking();
                if (aliases.Count > 0)
                {
                    payload.From.Alias = aliases[0];
                }
            }
        }

        /// <summary>
        /// Creates the builder object from a token request.
        /// </summary>
        /// <param name="member">payer of the token</param>
        /// <param name="tokenRequest">token request</param>
        public TransferTokenBuilder(Member member, TokenRequest tokenRequest)
        {
            if (tokenRequest.RequestPayload.RequestBodyCase != TRANSFER_BODY.TransferBody)
            {
                throw new ArgumentException("Require token request with transfer body.");
            }
            if (tokenRequest.RequestPayload.To == null)
            {
                throw new ArgumentException("No payee on token request.");
            }
            var transferBody = tokenRequest.RequestPayload.TransferBody;
            var instructions = transferBody.Instructions;
            if (instructions == null)
            {
                instructions = new TransferInstructions();
                instructions.Destinations.Add(transferBody.Destinations);
            }
            this.member = member;
            this.payload = new TokenPayload
            {
                Version = "1.0",
                RefId = tokenRequest.RequestPayload.RefId,
                From = tokenRequest.RequestOptions.From,
                To = tokenRequest.RequestPayload.To,
                Description = tokenRequest.RequestPayload.Description,
                ReceiptRequested = tokenRequest.RequestOptions.ReceiptRequested,
                TokenRequestId = tokenRequest.Id,
                Transfer = new TransferBody
                {
                    LifetimeAmount = transferBody.LifetimeAmount,
                    Currency = transferBody.Currency,
                    Amount = transferBody.Amount,
                    Instructions = instructions
                }
            };
            if (tokenRequest.RequestPayload.ActingAs != null)
            {
                this.payload.ActingAs = tokenRequest.RequestPayload.ActingAs;
            }
            this.tokenRequestId = tokenRequest.Id;
        }

        /// <summary>
        /// Creates the builder object from a token payload
        /// </summary>
        /// <param name="member">payer of the token</param>
        /// <param name="tokenPayload">token payload</param>
        public TransferTokenBuilder(Member member, TokenPayload tokenPayload)
        {
            if (tokenPayload.BodyCase != TRANSFER.Transfer)
            {
                throw new ArgumentException("Require token payload with transfer body.");
            }
            if (tokenPayload.To == null)
            {
                throw new ArgumentException("No payee on token payload.");
            }
            this.member = member;
            this.payload = tokenPayload;

            if (this.payload.From == null)
            {
                this.payload.From = new TokenMember
                {
                    Id = member.MemberId()
                };
            }
        }

        /// <summary>
        /// Creates the builder object.
        /// </summary>
        /// <param name="amount">lifetime amount of the token</param>
        /// <param name="currency">currency of the token</param>
        public TransferTokenBuilder(double amount, string currency) : this(null, amount, currency)
        {
        }

        /// <summary>
        /// Adds a source accountId to the token.
        /// </summary>
        /// <param name="accountId">source accountId</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetAccountId(string accountId)
        {
            var source = new TransferEndpoint
            {
                Account = new BankAccount
                {
                    Token = new BankAccount.Types.Token
                    {
                        AccountId = accountId,
                        MemberId = member.MemberId()
                    }
                }
            };
            var instructions = payload.Transfer.Instructions;
            if (instructions == null)
            {
                instructions = new TransferInstructions();
            }
            instructions.Source = source;
            payload.Transfer.Instructions = instructions;
            return this;
        }

        /// <summary>
        /// Sets the source custom authorization.
        /// </summary>
        /// <param name="bankId">source bank ID</param>
        /// <param name="authorization">source custom authorization</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetCustomAuthorization(string bankId, string authorization)
        {
            payload.Transfer
                    .Instructions
                    .Source
                    .Account = new BankAccount
                    {
                        Custom = new Custom
                        {
                            BankId = bankId,
                            Payload = authorization
                        }
                    };
            return this;
        }

        /// <summary>
        /// Sets the expiration date.
        /// </summary>
        /// <param name="expiresAtMs">expiration date in ms.</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetExpiresAtMs(long expiresAtMs)
        {
            payload.ExpiresAtMs = expiresAtMs;
            return this;
        }

        /// <summary>
        /// Sets the effective date.
        /// </summary>
        /// <param name="effectiveAtMs">effective date in ms.</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetEffectiveAtMs(long effectiveAtMs)
        {
            payload.EffectiveAtMs = effectiveAtMs;
            return this;
        }

        /// <summary>
        /// Sets the time after which endorse is no longer possible.
        /// </summary>
        /// <param name="endorseUntilMs">endorse until, in milliseconds.</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetEndorseUntilMs(long endorseUntilMs)
        {
            payload.EndorseUntilMs = endorseUntilMs;
            return this;
        }

        /// <summary>
        /// Sets the maximum amount per charge.
        /// </summary>
        /// <param name="chargeAmount">amount</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetChargeAmount(double chargeAmount)
        {
            payload.Transfer.Amount = chargeAmount.ToString();
            return this;
        }

        /// <summary>
        /// Sets the description.
        /// </summary>
        /// <param name="description">description</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetDescription(string description)
        {
            payload.Description = description;
            return this;
        }

        /// <summary>
        /// Adds a transfer source.
        /// </summary>
        /// <param name="source">the source</param>
        /// <returns>builder</returns>
		public TransferTokenBuilder SetSource(TransferEndpoint source)
        {
            payload.Transfer
                    .Instructions
                    .Source = source;
            return this;
        }

        /// <summary>
        /// Adds a transfer destination.
        /// </summary>
        /// <param name="destination">destination</param>
        /// <returns>builder</returns>
        [Obsolete("AddDestination is Deprecated.")]
        public TransferTokenBuilder AddDestination(TransferEndpoint destination)
        {
            var instructions = payload.Transfer.Instructions;
            if (instructions == null)
            {
                instructions = new TransferInstructions();
            }
            instructions.Destinations.Add(destination);
            payload.Transfer.Instructions = instructions;
            return this;
        }

        /// <summary>
        /// Adds a transfer destination.
        /// </summary>
        /// <param name="destination">destination</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder AddDestination(TransferDestination destination)
        {
            var instructions = payload.Transfer.Instructions;
            if (instructions == null)
            {
                instructions = new TransferInstructions();
            }
            instructions.TransferDestinations.Add(destination);
            payload.Transfer.Instructions = instructions;
            return this;
        }

        /// <summary>
        /// Sets the alias of the payee.
        /// </summary>
        /// <param name="toAlias">alias</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetToAlias(Alias toAlias)
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
        public TransferTokenBuilder SetToMemberId(string toMemberId)
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
        public TransferTokenBuilder SetRefId(string refId)
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
        /// Sets the purpose code. Refer to ISO 20022 External Code Sets.
        /// </summary>
        /// <param name="purposeCode">purpose of payment</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetPurposeCode(string purposeCode)
        {
            var instructions = payload.Transfer.Instructions;
            if (instructions == null)
            {
                instructions = new TransferInstructions { };
            }
            if (instructions.Metadata == null)
            {
                instructions.Metadata = new TransferInstructions.Types.Metadata { };
            }
            instructions.Metadata.PurposeCode = purposeCode;
            payload.Transfer.Instructions = instructions;
            return this;
        }

        /// <summary>
        /// Sets acting as on the token.
        /// </summary>
        /// <param name="actingAs">entity the redeemer is acting on behalf of</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetActingAs(ActingAs actingAs)
        {
            payload.ActingAs = actingAs;
            return this;
        }

        /// <summary>
        /// Sets the token request ID.
        /// </summary>
        /// <param name="tokenRequestId">token request id</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetTokenRequestId(string tokenRequestId)
        {
            payload.TokenRequestId = tokenRequestId;
            this.tokenRequestId = tokenRequestId;
            return this;
        }

        /// <summary>
        /// Sets the flag indicating whether a receipt is requested.
        /// </summary>
        /// <param name="receiptRequested">receipt requested flag</param>
        /// <returns>builder</returns>
        public TransferTokenBuilder SetReceiptRequested(bool receiptRequested)
        {
            payload.ReceiptRequested = receiptRequested;
            return this;
        }

        /// <summary>
        /// Sets provider transfer metadata.
        /// </summary>
        /// <param name="metadata">the metadata</param>
        /// <returns>the provider transfer metadata</returns>
        public TransferTokenBuilder SetProviderTransferMetadata(ProviderTransferMetadata metadata)
        {
            var instructions = payload.Transfer.Instructions;
            if (instructions == null)
            {
                instructions = new TransferInstructions { };
            }
            if (instructions.Metadata == null)
            {
                instructions.Metadata = new TransferInstructions.Types.Metadata { };
            }
            instructions.Metadata.ProviderTransferMetadata = metadata;
            payload.Transfer.Instructions = instructions;
            return this;
        }

        public TransferTokenBuilder From(string memberId)
        {
            payload.From = new TokenMember
            {
                Id = memberId
            };
            return this;
        }

        /// <summary>
        /// Builds a token payload, without uploading blobs or attachments.
        /// </summary>
        /// <returns>token payload</returns>
        public TokenPayload BuildPayload()
        {
            if (payload.RefId.Length == 0)
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Util.Nonce();
            }
            return payload;
        }

        /// <summary>
        /// DEPRECATED: Use {@link Member#prepareTransferToken(TransferTokenBuilder)} and
        /// {@link Member#createToken(TokenPayload, List)} instead.
        ///
        /// <p>Executes the request asynchronously.</p>
        /// </summary>
        /// <returns>Token</returns>
        [Obsolete("AddDestination is Deprecated. Use Member/PrepareTransferToken(TransferTokenBuilder) and Member/CreateToken(TokenPayload, List) instead.")]
        public Task<ProtoToken> Execute()
        {
            AccountCase sourceCase =
                    payload.Transfer.Instructions.Source.Account.AccountCase;
            IList<AccountCase> list = new List<AccountCase> { AccountCase.Token, AccountCase.Bank };
            if (!list.Contains(sourceCase))
            {
                throw new ArgumentException("No source on token");
            }
            if (payload.RefId.Length == 0)
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Util.Nonce();
            }
            return member.CreateTransferToken(
                    payload,
                    tokenRequestId != null ? tokenRequestId : "");
        }

        /// <summary>
        /// DEPRECATED: Use {@link Member#prepareTransferToken(TransferTokenBuilder)} and
        /// {@link Member#createToken(TokenPayload, List)} instead.
        ///
        /// <p>Executes the request, creating a token.</p>
        /// </summary>
        /// <returns>Token</returns>
        [Obsolete("AddDestination is Deprecated. Use Member/PrepareTransferToken(TransferTokenBuilder) and Member/CreateToken(TokenPayload, List) instead.")]
        public ProtoToken ExecuteBlocking()
        {
            return Execute().Result;
        }
    }
}
