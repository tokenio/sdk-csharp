using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User.Utils;
using RequestBodyCase = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.RequestBodyOneofCase;

namespace Tokenio.User {
	/// <summary>
	/// Helps building an access token payload.
	/// </summary>
	public sealed class AccessTokenBuilder {
		private static readonly int REF_ID_MAX_LENGTH = 18;
		private readonly TokenPayload payload;

		internal string tokenRequestId {
			get;
		}

		private AccessTokenBuilder() {
			payload = new TokenPayload {
				Version = "1.0",
				RefId = Util.Nonce(),
				Access = new AccessBody()
			};
		}

		private AccessTokenBuilder(TokenPayload tokenPayload, string tokenRequestId = null) {
			this.payload = tokenPayload;
			this.tokenRequestId = tokenRequestId;
		}

		/// <summary>
		/// Creates an instance of  AccessTokenBuilder.
		/// </summary>
		/// <returns>instance of link AccessTokenBuilder.</returns>
		/// <param name="redeemerMemberId">redeemerMemberId redeemer member id.</param>
		public static AccessTokenBuilder Create(string redeemerMemberId) {
			return new AccessTokenBuilder().To(redeemerMemberId);
		}

		/// <summary>
		/// Creates an instance of  AccessTokenBuilder.
		/// </summary>
		/// <returns>instance of link AccessTokenBuilder.</returns>
		/// <param name="redeemerAlias">redeemerAlias redeemer alias.</param>
		public static AccessTokenBuilder Create(Alias redeemerAlias) {
			return new AccessTokenBuilder().To(redeemerAlias);
		}

		/// <summary>
		/// To the specified redeemerMemberId.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="redeemerMemberId">Redeemer member identifier.</param>
		AccessTokenBuilder To(string redeemerMemberId) {
			payload.To = new TokenMember {
				Id = redeemerMemberId
			};
			return this;
		}

		/// <summary>
		/// To the specified redeemerAlias.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="redeemerAlias">Redeemer alias.</param>
		AccessTokenBuilder To(Alias redeemerAlias) {
			payload.To = new TokenMember {
				Alias = redeemerAlias
			};
			return this;
		}

		/// <summary>
		/// Froms the payload.
		/// </summary>
		/// <returns>The payload.</returns>
		/// <param name="payload">Payload.</param>
		public static AccessTokenBuilder FromPayload(TokenPayload payload) {
			TokenPayload builder = payload;
			builder.Access = new AccessBody() {
			};
			builder.RefId = Util.Nonce();
			return new AccessTokenBuilder(builder, null);
		}

		public static AccessTokenBuilder FromTokenRequest(TokenRequest tokenRequest) {
			if (!tokenRequest.RequestPayload.RequestBodyCase.Equals(RequestBodyCase.AccessBody)) {
				throw new ArgumentException("Require token request with access body.");
			}
			var builder = new TokenPayload {
				Version = "1.0",
				RefId = tokenRequest.RequestPayload.RefId,
				From = tokenRequest.RequestOptions.From,
				To = tokenRequest.RequestPayload.To,
				Description = tokenRequest.RequestPayload.Description,
				ReceiptRequested = tokenRequest.RequestOptions.ReceiptRequested
			};
			if (tokenRequest.RequestPayload.ActingAs != null) {
				builder.ActingAs = tokenRequest.RequestPayload.ActingAs;
			}
			return new AccessTokenBuilder(builder, null);
		}

		/// <summary>
		/// Sets the referenceId of the token.
		/// </summary>
		/// <returns>builder</returns>
		/// <param name="refId">refId the reference Id, at most 18 characters long.</param>
		public AccessTokenBuilder SetRefId(string refId) {
			if (refId.Length > REF_ID_MAX_LENGTH) {
				throw new ArgumentException($"The length of the refId is at most {REF_ID_MAX_LENGTH}, got: {refId.Length}");
			}
			payload.RefId = refId;
			return this;
		}

		/// <summary>
		/// Fors the address.
		/// </summary>
		/// <returns>The address.</returns>
		/// <param name="addressId">Address identifier.</param>
		public AccessTokenBuilder ForAddress(string addressId) {
			payload.Access.Resources.Add(
					new AccessBody.Types.Resource {
						Address = new AccessBody.Types.Resource.Types.Address {
							AddressId = addressId
						}
					});
			return this;
		}

		/// <summary>
		/// Fors the account.
		/// </summary>
		/// <returns>The account.</returns>
		/// <param name="accountId">Account identifier.</param>
		public AccessTokenBuilder ForAccount(string accountId) {
			var access = new AccessBody();
			access.Resources.Add(new AccessBody.Types.Resource {
				Account = new AccessBody.Types.Resource.Types.Account {
					AccountId = accountId
				}
			});
			payload.Access = access;
			return this;
		}

		/// <summary>
		/// Fors the account transactions.
		/// </summary>
		/// <returns>The account transactions.</returns>
		/// <param name="accountId">Account identifier.</param>
		public AccessTokenBuilder ForAccountTransactions(string accountId) {
			payload.Access.Resources.Add(new AccessBody.Types.Resource {
				Transactions = new AccessBody.Types.Resource.Types.AccountTransactions {
					AccountId = accountId
				}
			});
			return this;
		}

		/// <summary>
		/// Fors the account balances.
		/// </summary>
		/// <returns>The account balances.</returns>
		/// <param name="accountId">Account identifier.</param>
		public AccessTokenBuilder ForAccountBalances(string accountId) {
			payload.Access.Resources.Add(new AccessBody.Types.Resource {
				Balance = new AccessBody.Types.Resource.Types.AccountBalance {
					AccountId = accountId
				}
			});
			return this;
		}

		/// <summary>
		/// Fors the transfer destinations.
		/// </summary>
		/// <returns>The transfer destinations.</returns>
		/// <param name="accountId">Account identifier.</param>
		public AccessTokenBuilder ForTransferDestinations(string accountId) {
			payload.Access.Resources.Add(new AccessBody.Types.Resource {
				TransferDestinations = new AccessBody.Types.Resource.Types.TransferDestinations {
					AccountId = accountId
				}
			});
			return this;
		}

		/// <summary>
		/// Grants the ability to confirm whether the account has sufficient funds to cover
		/// a given charge.
		/// </summary>
		/// <returns>{@link AccessTokenBuilder}</returns>
		/// <param name="accountId">Account identifier.</param>
		public AccessTokenBuilder ForFundsConfirmation(string accountId) {
			payload.Access.Resources.Add(
					new AccessBody.Types.Resource {
						FundsConfirmation = new AccessBody.Types.Resource.Types.FundsConfirmation {
							AccountId = accountId
						}
					});
			return this;
		}

		/// <summary>
		/// From the specified memberId.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="memberId">Member identifier.</param>
		internal AccessTokenBuilder From(string memberId) {
			payload.From = new TokenMember {
				Id = memberId
			};
			return this;
		}

		/// <summary>
		/// Actings as.
		/// </summary>
		/// <returns>The as.</returns>
		/// <param name="actingAs">Acting as.</param>
		AccessTokenBuilder ActingAs(ActingAs actingAs) {
			payload.ActingAs = actingAs;
			return this;
		}

		/// <summary>
		/// Build this instance.
		/// </summary>
		/// <returns>The build.</returns>
		internal TokenPayload Build() {
			if (payload.Access.Resources.Count == 0) {
				throw new ArgumentException("At least one access resource must be set");
			}
			return payload;
		}
	}
}