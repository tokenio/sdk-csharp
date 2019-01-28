using System.Collections.Generic;
using System.Threading.Tasks;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio
{
    /// <summary>
    /// Represents the part of a token member that can be accessed through an access token.
    /// </summary>
    public interface IRepresentable
    {
        /// <summary>
        /// Looks up member addresses.
        /// </summary>
        /// <returns>a list of addresses</returns>
        Task<IList<AddressRecord>> GetAddresses();
        
        /// <summary>
        /// Looks up member addresses.
        /// </summary>
        /// <returns>a list of addresses</returns>
        IList<AddressRecord> GetAddressesBlocking();

        /// <summary>
        /// Looks up an address by id.
        /// </summary>
        /// <param name="addressId">the address id</param>
        /// <returns>the address record</returns>
        Task<AddressRecord> GetAddress(string addressId);
        
        /// <summary>
        /// Looks up an address by id.
        /// </summary>
        /// <param name="addressId">the address id</param>
        /// <returns>the address record</returns>
        AddressRecord GetAddressBlocking(string addressId);

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of accounts</returns>
        Task<IList<Account>> GetAccounts();
        
        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of accounts</returns>
        IList<Account> GetAccountsBlocking();

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        Task<Account> GetAccount(string accountId);
        
        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        Account GetAccountBlocking(string accountId);

        /// <summary>
        /// Looks up account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        Task<Balance> GetBalance(string accountId, Level keyLevel);
        
        /// <summary>
        /// Looks up account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        Balance GetBalanceBlocking(string accountId, Level keyLevel);

        /// <summary>
        /// Looks up balances for a list of accounts.
        /// </summary>
        /// <param name="accountIds">the list of accounts</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>a list of balances</returns>
        Task<IList<Balance>> GetBalances(IList<string> accountIds, Level keyLevel);
        
        /// <summary>
        /// Looks up balances for a list of accounts.
        /// </summary>
        /// <param name="accountIds">the list of accounts</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>a list of balances</returns>
        IList<Balance> GetBalancesBlocking(IList<string> accountIds, Level keyLevel);

        /// <summary>
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">the key level</param>
        /// <param name="offset">the nullable offset to start at</param>
        /// <returns>a paged list of transactions</returns>
        Task<PagedList<Transaction>> GetTransactions(
            string accountId,
            int limit,
            Level keyLevel,
            string offset);
        
        /// <summary>
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">the key level</param>
        /// <param name="offset">the nullable offset to start at</param>
        /// <returns>a paged list of transactions</returns>
        PagedList<Transaction> GetTransactionsBlocking(
            string accountId,
            int limit,
            Level keyLevel,
            string offset);

        /// <summary>
        /// Looks up an existing transaction for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="transactionId">the transaction ID</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the transaction</returns>
        Task<Transaction> GetTransaction(
            string accountId,
            string transactionId,
            Level keyLevel);
        
        /// <summary>
        /// Looks up an existing transaction for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="transactionId">the transaction ID</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the transaction</returns>
        Transaction GetTransactionBlocking(
            string accountId,
            string transactionId,
            Level keyLevel);

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        Task<IList<TransferEndpoint>> ResolveTransferDestination(string accountId);
        
        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        IList<TransferEndpoint> ResolveTransferDestinationBlocking(string accountId);
    }
}
