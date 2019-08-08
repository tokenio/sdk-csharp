using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User.Utils;
using RequestBodyCase = Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.RequestBodyOneofCase;

namespace Tokenio.User
{
    /// <summary>
    /// Helps building an access token payload.
    /// </summary>
    public sealed class AccessTokenBuilder
    {
        private static readonly int REF_ID_MAX_LENGTH = 18;
        private readonly TokenPayload payload;

        // Token request ID
        internal string tokenRequestId
        {
            get;
        }

        private AccessTokenBuilder()
        {
            payload = new TokenPayload
            {
                Version = "1.0",
                RefId = Util.Nonce(),
                Access = new AccessBody()
            };
        }

        private AccessTokenBuilder(TokenPayload tokenPayload, string tokenRequestId = null)
        {
            this.payload = tokenPayload;
            this.tokenRequestId = tokenRequestId;
        }

        /// <summary>
        /// Creates an instance of {@link AccessTokenBuilder}.
        /// </summary>
        /// <param name="redeemerMemberId">redeemer member id</param>
        /// <returns>instance of {@link AccessTokenBuilder}</returns>
        public static AccessTokenBuilder Create(string redeemerMemberId)
        {
            return new AccessTokenBuilder().To(redeemerMemberId);
        }

        /// <summary>
        /// Creates an instance of {@link AccessTokenBuilder}.
        /// </summary>
        /// <param name="redeemerAlias">redeemer alias</param>
        /// <returns>instance of {@link AccessTokenBuilder}</returns>
        public static AccessTokenBuilder Create(Alias redeemerAlias)
        {
            return new AccessTokenBuilder().To(redeemerAlias);
        }

        /// <summary>
        /// Sets "to" field on the payload.
        /// </summary>
        /// <param name="redeemerMemberId">redeemer member id</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        AccessTokenBuilder To(string redeemerMemberId)
        {
            payload.To = new TokenMember
            {
                Id = redeemerMemberId
            };
            return this;
        }

        /// <summary>
        /// Sets "to" field on the payload.
        /// </summary>
        /// <param name="redeemerAlias">redeemer alias</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        AccessTokenBuilder To(Alias redeemerAlias)
        {
            payload.To = new TokenMember
            {
                Alias = redeemerAlias
            };
            return this;
        }

        /// <summary>
        /// Creates an instance of {@link AccessTokenBuilder} from an existing token payload.
        /// </summary>
        /// <param name="payload">payload to initialize from</param>
        /// <returns>instance of {@link AccessTokenBuilder}</returns>
        public static AccessTokenBuilder FromPayload(TokenPayload payload)
        {
            var builder = payload;
            builder.Access = null;
            builder.RefId = Util.Nonce();
            return new AccessTokenBuilder(builder, null);
        }

        /// <summary>
        /// Creates an instance of {@link AccessTokenBuilder} from a token request.
        /// </summary>
        /// <param name="tokenRequest">token request</param>
        /// <returns>instance of {@link AccessTokenBuilder}</returns>
        public static AccessTokenBuilder FromTokenRequest(TokenRequest tokenRequest)
        {
            if (!tokenRequest.RequestPayload.RequestBodyCase.Equals(RequestBodyCase.AccessBody))
            {
                throw new ArgumentException("Require token request with access body.");
            }
            var builder = new TokenPayload
            {
                Version = "1.0",
                RefId = tokenRequest.RequestPayload.RefId,
                From = tokenRequest.RequestOptions.From,
                To = tokenRequest.RequestPayload.To,
                Description = tokenRequest.RequestPayload.Description,
                ReceiptRequested = tokenRequest.RequestOptions.ReceiptRequested
            };
            if (tokenRequest.RequestPayload.ActingAs != null)
            {
                builder.ActingAs = tokenRequest.RequestPayload.ActingAs;
            }
            return new AccessTokenBuilder(builder, null);
        }

        /// <summary>
        /// Sets the referenceId of the token.
        /// </summary>
        /// <param name="refId">the reference Id, at most 18 characters long</param>
        /// <returns>builder</returns>
        public AccessTokenBuilder SetRefId(string refId)
        {
            if (refId.Length > REF_ID_MAX_LENGTH)
            {
                throw new ArgumentException($"The length of the refId is at most {REF_ID_MAX_LENGTH}, got: {refId.Length}");
            }
            payload.RefId = refId;
            return this;
        }

        /// <summary>
        /// Grants access to a given {@code addressId}.
        /// </summary>
        /// <param name="addressId">address ID to grant access to</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        public AccessTokenBuilder ForAddress(string addressId)
        {
            var Access = payload.Access;
            if (Access == null)
            {
                Access = new AccessBody();
            }
            Access.Resources.Add(
                    new AccessBody.Types.Resource
                    {
                        Address = new AccessBody.Types.Resource.Types.Address
                        {
                            AddressId = addressId
                        }
                    });
            payload.Access = Access;
            return this;
        }

        /// <summary>
        /// Grants access to a given {@code accountId}.
        /// </summary>
        /// <param name="accountId">account ID to grant access to</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        public AccessTokenBuilder ForAccount(string accountId)
        {
            var Access = payload.Access;
            if (Access == null)
            {
                Access = new AccessBody();
            }
            Access.Resources.Add(new AccessBody.Types.Resource
            {
                Account = new AccessBody.Types.Resource.Types.Account
                {
                    AccountId = accountId
                }
            });
            payload.Access = Access;
            return this;
        }

        /// <summary>
        /// Grants access to a given account transactions.
        /// </summary>
        /// <param name="accountId">account ID to grant access to transactions</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        public AccessTokenBuilder ForAccountTransactions(string accountId)
        {
            var Access = payload.Access;
            if (Access == null)
            {
                Access = new AccessBody();
            }
            Access.Resources.Add(new AccessBody.Types.Resource
            {
                Transactions = new AccessBody.Types.Resource.Types.AccountTransactions
                {
                    AccountId = accountId
                }
            });
            payload.Access = Access;

            return this;
        }

        /// <summary>
        /// Grants access to a given account balances.
        /// </summary>
        /// <param name="accountId">account ID to grant access to balances</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        public AccessTokenBuilder ForAccountBalances(string accountId)
        {
            var Access = payload.Access;
            if (Access == null)
            {
                Access = new AccessBody();
            }
            Access.Resources.Add(new AccessBody.Types.Resource
            {
                Balance = new AccessBody.Types.Resource.Types.AccountBalance
                {
                    AccountId = accountId
                }
            });
            payload.Access = Access;

            return this;
        }

        /// <summary>
        /// Grants access to all transfer destinations at the given account.
        /// </summary>
        /// <param name="accountId">account id</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        public AccessTokenBuilder ForTransferDestinations(string accountId)
        {
            var Access = payload.Access;
            if (Access == null)
            {
                Access = new AccessBody();
            }
            Access.Resources.Add(new AccessBody.Types.Resource
            {
                TransferDestinations = new AccessBody.Types.Resource.Types.TransferDestinations
                {
                    AccountId = accountId
                }
            });
            payload.Access = Access;

            return this;
        }

        /// <summary>
        /// Grants the ability to confirm whether the account has sufficient funds to cover
        /// a given charge.
        /// </summary>
        /// <param name="accountId">account ID</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        public AccessTokenBuilder ForFundsConfirmation(string accountId)
        {
            var Access = payload.Access;
            if (Access == null)
            {
                Access = new AccessBody();
            }
            Access.Resources.Add(
                    new AccessBody.Types.Resource
                    {
                        FundsConfirmation = new AccessBody.Types.Resource.Types.FundsConfirmation
                        {
                            AccountId = accountId
                        }
                    });
            payload.Access = Access;

            return this;
        }

        /// <summary>
        /// Sets "from" field on the payload.
        /// </summary>
        /// <param name="memberId">token member ID to set</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        internal AccessTokenBuilder From(string memberId)
        {
            payload.From = new TokenMember
            {
                Id = memberId
            };
            return this;
        }

        /// <summary>
        /// Sets "acting as" field on the payload.
        /// </summary>
        /// <param name="actingAs">entity the redeemer is acting on behalf of</param>
        /// <returns>{@link AccessTokenBuilder}</returns>
        AccessTokenBuilder ActingAs(ActingAs actingAs)
        {
            payload.ActingAs = actingAs;
            return this;
        }

        /// <summary>
        /// Build this instance.
        /// </summary>
        /// <returns>The build.</returns>
        internal TokenPayload Build()
        {
            if (payload.Access.Resources.Count == 0)
            {
                throw new ArgumentException("At least one access resource must be set");
            }
            return payload;
        }
    }
}
