﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Tpp
{
    /// <summary>
    /// Represents the part of a token member that can be accessed through an access token.
    /// </summary>
    public interface IRepresentable
    {
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
            string offset = null);

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
            string offset = null);

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
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">optional offset to start at</param>
        /// <param name="startDate">inclusive lower bound of transaction booking date</param>
        /// <param name="endDate">inclusive upper bound of transaction booking date</param>
        /// <returns>a paged list of transaction records</returns>
        Task<PagedList<Transaction>> GetTransactions(
            string accountId,
            int limit,
            Level keyLevel,
            string offset = null,
            string startDate = null,
            string endDate = null);

        /// <summary>
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">optional offset to start at</param>
        /// <param name="startDate">inclusive lower bound of transaction booking date</param>
        /// <param name="endDate">inclusive upper bound of transaction booking date</param>
        /// <returns>a paged list of transaction records</returns>
        PagedList<Transaction> GetTransactionsBlocking(
            string accountId,
            int limit,
            Level keyLevel,
            string offset = null,
            string startDate = null,
            string endDate = null);

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
        /// Looks up an existing standing order for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="standingOrderId">ID of the standing order</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>standing order record</returns>
        Task<StandingOrder> GetStandingOrder(
                string accountId,
                string standingOrderId,
                Level keyLevel);

        /// <summary>
        /// Looks up an existing standing order for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="standingOrderId">ID of the standing order</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>standing order record</returns>
        StandingOrder GetStandingOrderBlocking(
                string accountId,
                string standingOrderId,
                Level keyLevel);

        /// <summary>
        /// Looks up standing orders for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>a paged list of standing order records</returns>
        Task<PagedList<StandingOrder>> GetStandingOrders(
                string accountId,
                int limit,
                Level keyLevel,
                string offset = null);

        /// <summary>
        /// Looks up standing orders for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>a paged list of standing order records</returns>
        PagedList<StandingOrder> GetStandingOrdersBlocking(
                string accountId,
                int limit,
                Level keyLevel,
                string offset = null);

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        Task<IList<TransferDestination>> ResolveTransferDestinations(string accountId);

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        IList<TransferDestination> ResolveTransferDestinationsBlocking(string accountId);

        /// <summary>
        /// Confirms the funds.
        /// </summary>
        /// <param name="accountId">account ID</param>
        /// <param name="amount">charge amount</param>
        /// <param name="currency">charge currency</param>
        /// <returns>true if the account has sufficient funds to cover the charge</returns>
        Task<bool> ConfirmFunds(string accountId, double amount, string currency);

        /// <summary>
        /// Confirm that the given account has sufficient funds to cover the charge.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <returns>true if the account has sufficient funds to cover the charge</returns>
        bool ConfirmFundsBlocking(string accountId, double amount, string currency);

    }
}
