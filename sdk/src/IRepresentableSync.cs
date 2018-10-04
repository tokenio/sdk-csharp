using System.Collections.Generic;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio
{
    /// <summary>
    /// Represents the part of a token member that can be accessed through an access token.
    /// </summary>
    public interface IRepresentableSync
    {
        /// <summary>
        /// Looks up member addresses.
        /// </summary>
        /// <returns>a list of addresses</returns>
        IList<AddressRecord> GetAddresses();

        /// <summary>
        /// Looks up an address by id.
        /// </summary>
        /// <param name="addressId">the address id</param>
        /// <returns>the address record</returns>
        AddressRecord GetAddress(string addressId);

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of linked accounts</returns>
        IList<AccountSync> GetAccounts();

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        AccountSync GetAccount(string accountId);

        /// <summary>
        /// Looks up account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        Balance GetBalance(string accountId, Level keyLevel);

        /// <summary>
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">the key level</param>
        /// <param name="offset">the nullable offset to start at</param>
        /// <returns>a paged list of transactions</returns>
        PagedList<Transaction> GetTransactions(
            string accountId,
            string offset,
            int limit,
            Level keyLevel);

        /// <summary>
        /// Looks up an existing transaction for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="transactionId">the transaction ID</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the transaction</returns>
        Transaction GetTransaction(
            string accountId,
            string transactionId,
            Level keyLevel);

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        IList<TransferEndpoint> ResolveTransferDestination(string accountId);
    }
}
