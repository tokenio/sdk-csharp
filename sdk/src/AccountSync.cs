using System;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.TransactionProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio
{
    /// <summary>
    /// Represents a funding account in the Token system.
    /// </summary>
    [Obsolete("deprecated, use Account instead")]
    public class AccountSync
    {
        private readonly AccountAsync async;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="async">real implementation that the calls are delegated to</param>
        internal AccountSync(AccountAsync async)
        {
            this.async = async;
        }

        /// <summary>
        /// Returns an async version of the API.
        /// </summary>
        /// <returns>asynchronous version of the account API</returns>
        public AccountAsync Async()
        {
            return async;
        }

        /// <summary>
        /// Gets a sync version of the MemberSync API.
        /// </summary>
        /// <returns>the account owner</returns>
        public MemberSync Member()
        {
            return async.Member().Sync();
        }

        /// <summary>
        /// Gets the account id.
        /// </summary>
        /// <returns>the account id</returns>
        public string Id()
        {
            return async.Id();
        }

        /// <summary>
        /// Sets to be a default account for its member.
        /// Only 1 account can be default for each member.
        /// </summary>
        public void SetAsDefault()
        {
            async.SetAsDefault().Wait();
        }

        /// <summary>
        /// Checks if this account is default.
        /// </summary>
        /// <returns>true is the account is default; otherwise false</returns>
        public bool IsDefault()
        {
            return async.IsDefault().Result;
        }

        /// <summary>
        /// Gets an account name.
        /// </summary>
        /// <returns>the account name</returns>
        public string Name()
        {
            return async.Name();
        }

        /// <summary>
        /// Gets the bank ID.
        /// </summary>
        /// <returns>the bank ID</returns>
        public string BankId()
        {
            return async.BankId();
        }

        /// <summary>
        /// Looks up if this account is locked.
        /// </summary>
        /// <returns>true if this account is locked; false otherwise.</returns>
        public bool IsLocked()
        {
            return async.IsLocked();
        }

        /// <summary>
        /// Looks up the account balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the balance</returns>
        public Balance GetBalance(Level keyLevel)
        {
            return async.GetBalance(keyLevel).Result;
        }

        /// <summary>
        /// Looks up an account current balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the balance</returns>
        public Money GetCurrentBalance(Level keyLevel)
        {
            return async.GetCurrentBalance(keyLevel).Result;
        }

        /// <summary>
        /// Looks up an account available balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the balance</returns>
        public Money GetAvailableBalance(Level keyLevel)
        {
            return async.GetAvailableBalance(keyLevel).Result;
        }

        /// <summary>
        /// Looks up transaction.
        /// </summary>
        /// <param name="transactionId">transaction id</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>the transaction</returns>
        public Transaction GetTransaction(
            string transactionId,
            Level keyLevel)
        {
            return async.GetTransaction(transactionId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up transactions.
        /// </summary>
        /// <param name="offset">nullable offset offset</param>
        /// <param name="limit">limit</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>a paged list of transactions</returns>
        public PagedList<Transaction> GetTransactions(
            string offset,
            int limit,
            Level keyLevel)
        {
            return async.GetTransactions(offset, limit, keyLevel).Result;
        }

        public override int GetHashCode()
        {
            return async.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            if (obj != null && obj.GetType().IsInstanceOfType(this))
            {
                return ((AccountSync) obj).async.Equals(async);
            }

            return false;
        }
    }
}
