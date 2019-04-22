using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using static Tokenio.Proto.Common.TokenProtos.AccessBody.Types;
using static Tokenio.Proto.Common.TokenProtos.AccessBody.Types.Resource.Types;

namespace Tokenio
{
    /// <summary>
    /// Helps building an access token payload.
    /// </summary>
    public class AccessTokenBuilder
    {
        private readonly TokenPayload payload;

        private AccessTokenBuilder()
        {
            payload = new TokenPayload
            {
                Version = "1.0",
                RefId = Util.Nonce(),
                Access = new AccessBody(),
                From = new TokenMember(),
                To = new TokenMember()
            };
        }

        private AccessTokenBuilder(TokenPayload payload)
        {
            this.payload = payload;
        }

        /// <summary>
        /// Creates an instance of <see cref="AccessTokenBuilder"/>
        /// </summary>
        /// <param name="redeemerAlias">the redeemer alias</param>
        /// <returns>an instance of <see cref= "AccessTokenBuilder"/></returns>
        public static AccessTokenBuilder Create(Alias redeemerAlias)
        {
            return new AccessTokenBuilder().To(redeemerAlias);
        }

        /// <summary>
        /// Creates an instance of <see cref="AccessTokenBuilder"/>.
        /// </summary>
        /// <param name="redeemerMemberId">the redeemer member id</param>
        /// <returns></returns>
        public static AccessTokenBuilder Create(string redeemerMemberId)
        {
            return new AccessTokenBuilder().To(redeemerMemberId);
        }

        /// <summary>
        /// Creates an instance of <see cref="AccessTokenBuilder"/>
        /// </summary>
        /// <param name="payload">the payload to initialize from</param>
        /// <returns>an instance of <see cref= "AccessTokenBuilder"/></returns>
        public static AccessTokenBuilder FromPayload(TokenPayload payload)
        {
            payload.Access = new AccessBody();
            payload.RefId = Util.Nonce();
            return new AccessTokenBuilder(payload);
        }

        /// <summary>
        /// Grants access to all addresses.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllAddresses()
        {
            payload.Access.Resources.Add(new Resource
            {
                AllAddresses = new AllAddresses()
            });
            return this;
        }

        /// <summary>
        /// Grants access to a given address id.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder ForAddress(string addressId)
        {
            payload.Access.Resources.Add(new Resource
            {
                Address = new Address
                {
                    AddressId = addressId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to all accounts.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllAccounts()
        {
            payload.Access.Resources.Add(new Resource
            {
                AllAccounts = new AllAccounts()
            });
            return this;
        }

        /// <summary>
        /// Grants access to all accounts at the given bank.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllAccountsAtBank(string bankId)
        {
            payload.Access.Resources.Add(new Resource
            {
                AllAccountsAtBank = new AllAccountsAtBank
                {
                    BankId = bankId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to a given account id.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder ForAccount(string accountId)
        {
            payload.Access.Resources.Add(new Resource
            {
                Account = new Resource.Types.Account
                {
                    AccountId = accountId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to all transactions.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllTransactions()
        {
            payload.Access.Resources.Add(new Resource
            {
                AllTransactions = new AllAccountTransactions()
            });
            return this;
        }

        /// <summary>
        /// Grants access to transactions for all accounts at a given bank.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllTransactionsAtBank(string bankId)
        {
            payload.Access.Resources.Add(new Resource
            {
                AllTransactionsAtBank = new AllTransactionsAtBank
                {
                    BankId = bankId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to transactions of a given account.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder ForAccountTransactions(string accountId)
        {
            payload.Access.Resources.Add(new Resource
            {
                Transactions = new AccountTransactions
                {
                    AccountId = accountId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to all balances.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllBalances()
        {
            payload.Access.Resources.Add(new Resource
            {
                AllBalances = new AllAccountBalances()
            });
            return this;
        }

        /// <summary>
        /// Grants access to balances for all accounts at the given bank.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllBalancesAtBank(string bankId)
        {
            payload.Access.Resources.Add(new Resource
            {
                AllBalancesAtBank = new AllBalancesAtBank
                {
                    BankId = bankId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to balances of a given account.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder ForAccountBalances(string accountId)
        {
            payload.Access.Resources.Add(new Resource
            {
                Balance = new AccountBalance
                {
                    AccountId = accountId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to all transfer destinations.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllTransferDestinations()
        {
            payload.Access.Resources.Add(new Resource
            {
                AllTransferDestinations = new AllTransferDestinations()
            });
            return this;
        }

        /// <summary>
        /// Grants access to all transfer destinations at the given bank.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAllTransferDestinationsAtBank(string bankId)
        {
            payload.Access.Resources.Add(new Resource
            {
                AllTransferDestinationsAtBank = new AllTransferDestinationsAtBank
                {
                    BankId = bankId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to a transfer destinations for the given account.
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder ForTransferDestination(string accountId)
        {
            payload.Access.Resources.Add(new Resource
            {
                TransferDestinations = new TransferDestinations
                {
                    AccountId = accountId
                }
            });
            return this;
        }

        /// <summary>
        /// Grants access to ALL resources (aka wildcard permissions).
        /// </summary>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        [Obsolete("Access token permissions of type 'ALL' will be removed.")]
        public AccessTokenBuilder ForAll()
        {
            return ForAllAccounts()
                .ForAllAddresses()
                .ForAllBalances()
                .ForAllTransactions()
                .ForAllTransferDestinations();
        }

        /// <summary>
        /// Sets "from" field on the payload.
        /// </summary>
        /// <param name="memberId">the token member ID to set</param>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder From(string memberId)
        {
            payload.From.Id = memberId;
            return this;
        }

        /// <summary>
        /// Sets "to" field on the payload.
        /// </summary>
        /// <param name="redeemerAlias">the redeemer's alias</param>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder To(Alias redeemerAlias)
        {
            payload.To.Alias = redeemerAlias;
            return this;
        }

        /// <summary>
        /// Sets "to" field on the payload.
        /// </summary>
        /// <param name="redeemerMemberId">the redeemer's member id</param>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder To(string redeemerMemberId)
        {
            payload.To.Id = redeemerMemberId;
            return this;
        }

        /// <summary>
        /// Sets "acting as" field on the payload.
        /// </summary>
        /// <param name="actingAs">entity the redeemer is acting on behalf of</param>
        /// <returns><see cref= "AccessTokenBuilder"/></returns>
        public AccessTokenBuilder ActingAs(ActingAs actingAs)
        {
            payload.ActingAs = actingAs;
            return this;
        }

        /// <summary>
        /// Builds the <see cref= "TokenPayload"/> with all specified settings.
        /// </summary>
        /// <returns>an instance of <see cref= "TokenPayload"/></returns>
        /// <exception cref="ArgumentException"></exception>
        public TokenPayload Build()
        {
            if (payload.Access.Resources.Count == 0)
            {
                throw new ArgumentException("At least one access resource must be set");
            }

            return payload;
        }
    }
}
