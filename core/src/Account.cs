using System.Threading.Tasks;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Rpc;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;

namespace Tokenio {
    /// <summary>
    /// Represents a funding account in the Token system.
    /// </summary>
    public class Account {
        protected readonly Member member;
        protected readonly ProtoAccount account;
        protected readonly Client client;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Tokenio.Account"/> class.
        /// </summary>
        /// <param name="account">Account.</param>
        public Account(Account account) {
            this.member = account.member;
            this.account = account.account;
            this.client = account.client;
        }

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="member">account owner</param>
        /// <param name="account">account information</param>
        /// <param name="client">RPC client used to perform operations against the server</param>
        public Account(Member member, ProtoAccount account, Client client) {
            this.member = member;
            this.account = account;
            this.client = client;
        }

        /// <summary>
        /// Gets an account owner.
        /// </summary>
        /// <returns>account owner</returns>
        public virtual Member Member() {
            return member;
        }

        /// <summary>
        /// Gets an account ID.
        /// </summary>
        /// <returns>account id</returns>
        public string Id() {
            return account.Id;
        }

        /// <summary>
        /// Gets an account name.
        /// </summary>
        /// <returns>account name</returns>
        public string Name() {
            return account.Name;
        }

        /// <summary>
        /// Looks up if this account is locked.
        /// </summary>
        /// <returns>true if this account is locked; false otherwise.</returns>
        public bool IsLocked() {
            return account.IsLocked;
        }

        /// <summary>
        /// Gets the bank ID.
        /// </summary>
        /// <returns>the bank ID</returns>
        public string BankId() {
            return account.BankId;
        }

        /// <summary>
        /// Looks up an account balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the account balance</returns>
        public Task<Balance> GetBalance(Level keyLevel) {
            return client.GetBalance(account.Id, keyLevel);
        }

        /// <summary>
        /// Looks up an account balance.
        /// </summary>
        /// <param name="keyLevel">key level</param>
        /// <returns>the account balance</returns>
        public Balance GetBalanceBlocking(Level keyLevel) {
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
            Level keyLevel) {
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
            Level keyLevel) {
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
            Level keyLevel) {
            return GetTransactions(limit, keyLevel, offset, null, null);
        }

        public Task<PagedList<Transaction>> GetTransactions(
            int limit,
            Level keyLevel,
            string offset = null,
            string startDate = null,
            string endDate = null) {
            return client.GetTransactions(account.Id, limit, keyLevel, offset, startDate, endDate);
        }

        /// <summary>
        /// Returns ProtoAccount object
        /// </summary>
        /// <returns> the ProtoAccount object</returns>
        public ProtoAccount toProto() {
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
            int limit,
            Level keyLevel,
            string offset) {
            return GetTransactions(offset, limit, keyLevel).Result;
        }

        /// <summary>
        /// Looks up transactions.
        /// </summary>
        /// <param name="limit">limit</param>
        /// <param name="keyLevel">keyLevel</param>
        /// <param name="offset">offset</param>
        /// <param name="startDate">inclusive lower bound of transaction booking date</param>
        /// <param name="endDate">inclusive upper bound of transaction booking date</param>
        /// <returns>paged list of transactions</returns>
        public PagedList<Transaction> GetTransactionsBlocking(
            int limit,
            Level keyLevel,
            string offset = null,
            string startDate = null,
            string endDate = null) {
            return GetTransactions(limit, keyLevel, offset, startDate, endDate).Result;
        }

        /// <summary>
        /// Looks up an existing standing order for a given account.
        /// </summary>
        /// <param name="standingOrderId">ID of the standing order</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>standing order record</returns>
        public Task<StandingOrder> GetStandingOrder(
            string standingOrderId,
            Level keyLevel) {
            return client.GetStandingOrder(account.Id, standingOrderId, keyLevel);
        }

        /// <summary>
        /// Looks up an existing standing order for a given account.
        /// </summary>
        /// <param name="standingOrderId">ID of the standing order</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>standing order record</returns>
        public StandingOrder GetStandingOrderBlocking(
            string standingOrderId,
            Level keyLevel) {
            return GetStandingOrder(standingOrderId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up standing orders for a given account.
        /// </summary>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>a paged list of standing order records</returns>
        public Task<PagedList<StandingOrder>> GetStandingOrders(
            int limit,
            Level keyLevel,
            string offset = null) {
            return client.GetStandingOrders(account.Id, limit, keyLevel, offset);
        }

        /// <summary>
        /// Looks up standing orders for a given account.
        /// </summary>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>a paged list of standing order records</returns>
        public PagedList<StandingOrder> GetStandingOrdersBlocking(
            int limit,
            Level keyLevel,
            string offset = null) {
            return GetStandingOrders(limit, keyLevel, offset).Result;
        }

        public override int GetHashCode() {
            return account.Id.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj != null && obj.GetType().IsInstanceOfType(this)) {
                return ((Account) obj).account.Equals(account);
            }

            return false;
        }

        public ProtoAccount GetAccount() {
            return account;
        }
    }
}
