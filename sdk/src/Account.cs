using System;
using System.Threading.Tasks;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Rpc;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;

namespace Tokenio
{
    /// <summary>
    /// Represents a funding account in the Token system.
    /// </summary>
    public class Account
    {
        private readonly Member member;
        private readonly ProtoAccount account;
        private readonly Client client;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="member">account owner</param>
        /// <param name="account">account information</param>
        /// <param name="client">RPC client used to perform operations against the server</param>
        internal Account(Member member, ProtoAccount account, Client client)
        {
            this.member = member;
            this.account = account;
            this.client = client;
        }

        /// <summary>
        /// Gets an account owner.
        /// </summary>
        /// <returns>account owner</returns>
        public Member Member()
        {
            return member;
        }

        /// <summary>
        /// Gets an account ID.
        /// </summary>
        /// <returns>account id</returns>
        public string Id()
        {
            return account.Id;
        }

        /// <summary>
        /// Gets an account name.
        /// </summary>
        /// <returns>account name</returns>
        public string Name()
        {
            return account.Name;
        }

        /// <summary>
        /// Looks up if this account is locked.
        /// </summary>
        /// <returns>true if this account is locked; false otherwise.</returns>
        public bool IsLocked()
        {
            return account.IsLocked;
        }

        /// <summary>
        /// Gets the bank ID.
        /// </summary>
        /// <returns>the bank ID</returns>
        public string BankId()
        {
            return account.BankId;
        }
        
        /// <summary>
        /// Sets this account as a member's default account.
        /// </summary>
        /// <returns>a task</returns>
        [Obsolete("SetAsDefault is deprecated.")]
        public Task SetAsDefault()
        {
            return client.SetDefaultAccount(Id());
        }
        
        /// <summary>
        /// Sets to be a default account for its member.
        /// Only 1 account can be default for each member.
        /// </summary>
        [Obsolete("SetAsDefaultBlocking is deprecated.")]
        public void SetAsDefaultBlocking()
        {
            SetAsDefault().Wait();
        }

        /// <summary>
        /// Looks up if this account is default.
        /// </summary>
        /// <returns>true if this account is default; false otherwise.</returns>
        [Obsolete("IsDefault is deprecated.")]
        public Task<bool> IsDefault()
        {
            return client.IsDefault(Id());
        }
        
        /// <summary>
        /// Checks if this account is default.
        /// </summary>
        /// <returns>true is the account is default; otherwise false</returns>
        [Obsolete("IsDefaultBlocking is deprecated.")]
        public bool IsDefaultBlocking()
        {
            return IsDefault().Result;
        }
        
        /// <summary>
        /// Looks up an account current balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the current balance</returns>
        [Obsolete("GetCurrentBalance is deprecated.")]
        public Task<Money> GetCurrentBalance(Level keyLevel)
        {
            return client.GetBalance(account.Id, keyLevel)
                .Map(balance => balance.Current);
        }
        
        /// <summary>
        /// Looks up an account current balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the current balance</returns>
        [Obsolete("GetCurrentBalanceBlocking is deprecated.")]
        public Money GetCurrentBalanceBlocking(Level keyLevel)
        {
            return GetCurrentBalance(keyLevel).Result;
        }

        /// <summary>
        /// Looks up an account available balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the available balance</returns>
        [Obsolete("GetAvailableBalance is deprecated.")]
        public Task<Money> GetAvailableBalance(Level keyLevel)
        {
            return client.GetBalance(account.Id, keyLevel)
                .Map(balance => balance.Available);
        }
        
        /// <summary>
        /// Looks up an account available balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the available balance</returns>
        [Obsolete("GetAvailableBalanceBlocking is deprecated.")]
        public Money GetAvailableBalanceBlocking(Level keyLevel)
        {
            return GetAvailableBalance(keyLevel).Result;
        }

        /// <summary>
        /// Looks up an account balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the account balance</returns>
        public Task<Balance> GetBalance(Level keyLevel)
        {
            return client.GetBalance(account.Id, keyLevel);
        }
        
        /// <summary>
        /// Looks up an account balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the account balance</returns>
        public Balance GetBalanceBlocking(Level keyLevel)
        {
            return GetBalance(keyLevel).Result;
        }

        /// <summary>
        /// Looks up transaction.
        /// </summary>
        /// <param name="transactionId">transaction id</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>the transaction</returns>
        public Task<Transaction> GetTransaction(
            string transactionId,
            Level keyLevel)
        {
            return client.GetTransaction(account.Id, transactionId, keyLevel);
        }
        
        /// <summary>
        /// Looks up transaction.
        /// </summary>
        /// <param name="transactionId">transaction id</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>the transaction</returns>
        public Transaction GetTransactionBlocking(
            string transactionId,
            Level keyLevel)
        {
            return GetTransaction(transactionId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up transactions.
        /// </summary>
        /// <param name="offset">nullable offset offset</param>
        /// <param name="limit">limit</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>a paged list of transactions</returns>
        public Task<PagedList<Transaction>> GetTransactions(
            string offset,
            int limit,
            Level keyLevel)
        {
            return client.GetTransactions(account.Id, limit, keyLevel, offset);
        }
        
        /// <summary>
        /// Returns ProtoAccount object
        /// </summary>
        /// <returns> the ProtoAccount object</returns>
        public ProtoAccount toProto()
        {
            return account;
        }
        
        /// <summary>
        /// Looks up transactions.
        /// </summary>
        /// <param name="offset">nullable offset offset</param>
        /// <param name="limit">limit</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>a paged list of transactions</returns>
        public PagedList<Transaction> GetTransactionsBlocking(
            string offset,
            int limit,
            Level keyLevel)
        {
            return GetTransactions(offset, limit, keyLevel).Result;
        }

        public override int GetHashCode()
        {
            return account.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType().IsInstanceOfType(this))
            {
                return ((Account) obj).account.Equals(account);
            }

            return false;
        }
    }
}
