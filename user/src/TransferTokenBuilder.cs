using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Utils;
using static Tokenio.Proto.Common.AccountProtos.BankAccount.Types;
using AccountCase = Tokenio.Proto.Common.AccountProtos.BankAccount.AccountOneofCase;
using ProtoToken = Tokenio.Proto.Common.TokenProtos.Token;
using TRANSFER_BODY = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.RequestBodyOneofCase;
using TRANSFER = Tokenio.Proto.Common.TokenProtos.TokenPayload.BodyOneofCase;
using Tokenio.Proto.Common.ProviderSpecific;
namespace Tokenio.User {
	/// <summary>
	/// This class is used to build a transfer token. The required parameters are member, amount (which
	/// is the lifetime amount of the token), and currency.One source of funds must be set: either
	/// accountId or BankAuthorization. Finally, a redeemer must be set, specified by either alias
	/// or memberId.
	/// </summary>
	public sealed class TransferTokenBuilder {
		private static readonly ILog logger = LogManager
				.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int REF_ID_MAX_LENGTH = 18;
		private readonly Member member;
		private readonly TokenPayload payload;
		private string tokenRequestId;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Tokenio.User.TransferTokenBuilder"/> class.
		/// </summary>
		/// <param name="member">Member.</param>
		/// <param name="amount">Amount.</param>
		/// <param name="currency">Currency.</param>
		public TransferTokenBuilder(
				Member member,
				double amount,
				string currency) {
			this.member = member;
			this.payload = new TokenPayload {
				Version = "1.0",
				Transfer = new TransferBody {
					Currency = currency,
					LifetimeAmount = amount.ToString()
				}
			};
			if (member != null) {
				From(member.MemberId());
				IList<Alias> aliases = member.GetAliasesBlocking();
				if (aliases.Count > 0) {
					payload.From.Alias = aliases[0];
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Tokenio.User.TransferTokenBuilder"/> class.
		/// </summary>
		/// <param name="member">Member.</param>
		/// <param name="tokenRequest">Token request.</param>
		public TransferTokenBuilder(Member member, TokenRequest tokenRequest) {
			if (tokenRequest.RequestPayload.RequestBodyCase != TRANSFER_BODY.TransferBody) {
				throw new ArgumentException("Require token request with transfer body.");
			}
            if (tokenRequest.RequestPayload.To==null)
            {
                throw new ArgumentException("No payee on token request.");
            }
            var transferBody = tokenRequest.RequestPayload.TransferBody;
            var instructions = transferBody.Instructions;
            if (instructions == null)
            { instructions = new TransferInstructions();
                instructions.Destinations.Add(transferBody.Destinations);
            }
            this.member = member;
			this.payload = new TokenPayload {
				Version = "1.0",
				RefId = tokenRequest.RequestPayload.RefId,
				From = tokenRequest.RequestOptions.From,
				To = tokenRequest.RequestPayload.To,
				Description = tokenRequest.RequestPayload.Description,
				ReceiptRequested = tokenRequest.RequestOptions.ReceiptRequested,
                TokenRequestId= tokenRequest.Id,
                Transfer = new TransferBody {
					LifetimeAmount = transferBody.LifetimeAmount,
					Currency = transferBody.Currency,
					Amount = transferBody.Amount,
					Instructions = instructions
				}
			};
			if (tokenRequest.RequestPayload.ActingAs != null) {
				this.payload.ActingAs = tokenRequest.RequestPayload.ActingAs;
			}
			this.tokenRequestId = tokenRequest.Id;
		}


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
        /// Initializes a new instance of the <see cref="T:Tokenio.User.TransferTokenBuilder"/> class.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <param name="currency">Currency.</param>
        public TransferTokenBuilder(double amount, string currency) : this(null, amount, currency) {
		}

		/// <summary>
		/// Sets the account identifier.
		/// </summary>
		/// <returns>The account identifier.</returns>
		/// <param name="accountId">Account identifier.</param>
		public TransferTokenBuilder SetAccountId(string accountId) {
			var source = new TransferEndpoint {
				Account = new BankAccount {
					Token = new BankAccount.Types.Token {
						AccountId = accountId,
						MemberId = member.MemberId()
					}
				}
			};
			var instructions = payload.Transfer.Instructions;
			if (instructions == null) {
				instructions = new TransferInstructions();
			}
			instructions.Source = source;
			payload.Transfer.Instructions = instructions;
			return this;
		}

		/// <summary>
		/// Sets the custom authorization.
		/// </summary>
		/// <returns>The custom authorization.</returns>
		/// <param name="bankId">Bank identifier.</param>
		/// <param name="authorization">Authorization.</param>
		public TransferTokenBuilder SetCustomAuthorization(string bankId, string authorization) {
			payload.Transfer
					.Instructions
					.Source
					.Account = new BankAccount {
						Custom = new Custom {
							BankId = bankId,
							Payload = authorization
						}
					};
			return this;
		}

		/// <summary>
		/// Sets the expires at ms.
		/// </summary>
		/// <returns>The expires at ms.</returns>
		/// <param name="expiresAtMs">Expires at ms.</param>
		public TransferTokenBuilder SetExpiresAtMs(long expiresAtMs) {
			payload.ExpiresAtMs = expiresAtMs;
			return this;
		}

		/// <summary>
		/// Sets the effective at ms.
		/// </summary>
		/// <returns>The effective at ms.</returns>
		/// <param name="effectiveAtMs">Effective at ms.</param>
		public TransferTokenBuilder SetEffectiveAtMs(long effectiveAtMs) {
			payload.EffectiveAtMs = effectiveAtMs;
			return this;
		}

		/// <summary>
		/// Sets the endorse until ms.
		/// </summary>
		/// <returns>The endorse until ms.</returns>
		/// <param name="endorseUntilMs">Endorse until ms.</param>
		public TransferTokenBuilder SetEndorseUntilMs(long endorseUntilMs) {
			payload.EndorseUntilMs = endorseUntilMs;
			return this;
		}

		/// <summary>
		/// Sets the charge amount.
		/// </summary>
		/// <returns>The charge amount.</returns>
		/// <param name="chargeAmount">Charge amount.</param>
		public TransferTokenBuilder SetChargeAmount(double chargeAmount) {
			payload.Transfer.Amount = chargeAmount.ToString();
			return this;
		}

		/// <summary>
		/// Sets the description.
		/// </summary>
		/// <returns>The description.</returns>
		/// <param name="description">Description.</param>
		public TransferTokenBuilder SetDescription(string description) {
			payload.Description = description;
			return this;
		}

		public TransferTokenBuilder SetSource(TransferEndpoint source) {
			payload.Transfer
					.Instructions
					.Source = source;
			return this;
		}

		/// <summary>
		/// Adds the destination.
		/// </summary>
		/// <returns>The destination.</returns>
		/// <param name="destination">Destination.</param>
		[Obsolete("AddDestination is Deprecated.")]
		public TransferTokenBuilder AddDestination(TransferEndpoint destination) {
			var instructions = payload.Transfer.Instructions;
			if (instructions == null) {
				instructions = new TransferInstructions();
			}
			instructions.Destinations.Add(destination);
			payload.Transfer.Instructions = instructions;
			return this;
		}

		/// <summary>
		/// Adds the destination.
		/// </summary>
		/// <returns>The destination.</returns>
		/// <param name="destination">Destination.</param>
		public TransferTokenBuilder AddDestination(TransferDestination destination) {
			var instructions = payload.Transfer.Instructions;
			if (instructions == null) {
				instructions = new TransferInstructions();
			}
			instructions.TransferDestinations.Add(destination);
			payload.Transfer.Instructions = instructions;
			return this;
		}

		/// <summary>
		/// Sets to alias.
		/// </summary>
		/// <returns>The to alias.</returns>
		/// <param name="toAlias">To alias.</param>
		public TransferTokenBuilder SetToAlias(Alias toAlias) {
            payload.To = new TokenMember
            {
                Alias= toAlias
            };
			return this;
		}

		/// <summary>
		/// Sets to member identifier.
		/// </summary>
		/// <returns>The to member identifier.</returns>
		/// <param name="toMemberId">To member identifier.</param>
		public TransferTokenBuilder SetToMemberId(string toMemberId) {
			payload.To = new TokenMember {
				Id = toMemberId
			};
			return this;
		}

		/// <summary>
		/// Sets the reference identifier.
		/// </summary>
		/// <returns>The reference identifier.</returns>
		/// <param name="refId">Reference identifier.</param>
		public TransferTokenBuilder SetRefId(string refId) {
			if (refId.Length > REF_ID_MAX_LENGTH) {
				throw new ArgumentException(string.Format(
						"The length of the refId is at most {0}, got: {1}",
						REF_ID_MAX_LENGTH,
						refId.Length));
			}
			payload.RefId = refId;
			return this;
		}

		/// <summary>
		/// Sets the purpose of payment.
		/// </summary>
		/// <returns>The purpose of payment.</returns>
		/// <param name="purposeOfPayment">Purpose of payment.</param>
		public TransferTokenBuilder SetPurposeOfPayment(PurposeOfPayment purposeOfPayment) {
			var instructions = payload.Transfer.Instructions;
			if (instructions == null) {
				instructions = new TransferInstructions { };
			}
			if (instructions.Metadata == null) {
				instructions.Metadata = new TransferInstructions.Types.Metadata { };
			}
			instructions.Metadata.TransferPurpose = purposeOfPayment;
            payload.Transfer.Instructions = instructions;
            return this;
		}

		/// <summary>
		/// Sets the acting as.
		/// </summary>
		/// <returns>The acting as.</returns>
		/// <param name="actingAs">Acting as.</param>
		public TransferTokenBuilder SetActingAs(ActingAs actingAs) {
			payload.ActingAs = actingAs;
			return this;
		}

		/// <summary>
		/// Sets the token request ID.
		/// </summary>
		/// <param name="tokenRequestId">token request id</param>
		/// <returns>builder</returns>
		public TransferTokenBuilder SetTokenRequestId(string tokenRequestId) {
            payload.TokenRequestId = tokenRequestId;
            this.tokenRequestId = tokenRequestId;
			return this;
		}

		/// <summary>
		/// Sets the flag indicating whether a receipt is requested.
		/// </summary>
		/// <param name="receiptRequested">receipt requested flag</param>
		/// <returns>builder</returns>
		public TransferTokenBuilder SetReceiptRequested(bool receiptRequested) {
			payload.ReceiptRequested = receiptRequested;
			return this;
		}

        /// <summary>
        /// Sets the provider transfer metadata.
        /// </summary>
        /// <returns>The provider transfer metadata.</returns>
        /// <param name="metadata">Metadata.</param>
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
            instructions.Metadata.ProviderTransferMetadata= metadata;
            payload.Transfer.Instructions = instructions;
            return this;
        }

        /// <summary>
        /// From the specified memberId.
        /// </summary>
        /// <returns>The from.</returns>
        /// <param name="memberId">Member identifier.</param>
        public TransferTokenBuilder From(string memberId) {
			payload.From = new TokenMember {
				Id = memberId
			};
			return this;
		}

		/// <summary>
		/// Builds the payload.
		/// </summary>
		/// <returns>The payload.</returns>
		public TokenPayload BuildPayload() {
			if (payload.RefId.Length == 0) {
				logger.Warn("refId is not set. A random ID will be used.");
				payload.RefId = Util.Nonce();
			}
			return payload;
		}

		/// <summary>
		/// Execute this instance.
		/// </summary>
		/// <returns>The execute.</returns>
		[Obsolete("AddDestination is Deprecated. Use Member/PrepareTransferToken(TransferTokenBuilder) and Member/CreateToken(TokenPayload, List) instead.")]
		public Task<ProtoToken> Execute() {
			AccountCase sourceCase =
					payload.Transfer.Instructions.Source.Account.AccountCase;
			IList<AccountCase> list = new List<AccountCase> { AccountCase.Token, AccountCase.Bank };
			if (!list.Contains(sourceCase)) {
				throw new ArgumentException("No source on token");
			}
			if (payload.RefId.Length == 0) {
				logger.Warn("refId is not set. A random ID will be used.");
				payload.RefId = Util.Nonce();
			}
			return member.CreateTransferToken(
					payload,
					tokenRequestId != null ? tokenRequestId : "");
		}

		/// <summary>
		/// Executes the blocking.
		/// </summary>
		/// <returns>The blocking.</returns>
		[Obsolete("AddDestination is Deprecated. Use Member/PrepareTransferToken(TransferTokenBuilder) and Member/CreateToken(TokenPayload, List) instead.")]
		public ProtoToken ExecuteBlocking() {
			return Execute().Result;
		}
	}
}