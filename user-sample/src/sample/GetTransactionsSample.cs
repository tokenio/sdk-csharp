using System.Collections.Generic;
using System.Linq;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public static class GetTransactionsSample
    {
        /// <summary>
        /// Illustrate Member.getTransactions
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void getTransactionsSample(UserMember payer)
        {
            List<Tokenio.User.Account> accounts = payer.GetAccountsBlocking().ToList();
            string accountId = accounts[0].Id();
            foreach (Transaction transaction in payer.GetTransactionsBlocking(accountId, 10, Level.Standard, null).List)
            {
                DisplayTransaction(
                    transaction.Amount.Currency,
                    transaction.Amount.Value,
                    transaction.Type, // debit or credit
                    transaction.Status);
            }

        }

        /// <summary>
        /// Illustrate Member.getTransactions.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void GetTransactionsByDateSample(UserMember payer)
        {
            List<Tokenio.User.Account> accounts = payer.GetAccountsBlocking().ToList();
            string accountId = accounts[0].Id();
            foreach (Transaction transaction in payer
                .GetTransactionsBlocking(accountId, 10, Level.Standard, null, "2019-01-15", "2022-02-15").List)
            {
                DisplayTransaction(
                           transaction.Amount.Currency,
                           transaction.Amount.Value,
                           transaction.Type, // debit or credit
                           transaction.Status);
            }

        }

        /// <summary>
        /// Illustrate Member.getTransaction
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="transfer">recently-completed transfer</param>
        /// <returns>a Transaction</returns>
        public static Transaction GetTransactionSample(
          UserMember payer,
          Transfer transfer)
        {
            List<Tokenio.User.Account> accounts = payer.GetAccountsBlocking().ToList();
            string accountId = accounts[0].Id();

            string transactionId = transfer.TransactionId;
            Transaction transaction = payer.GetTransactionBlocking(accountId, transactionId, Level.Standard);
            return transaction;
        }

        /// <summary>
        /// Illustrate Account.getTransactions
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void AccountGetTransactionsSample(UserMember payer)
        {
            Tokenio.User.Account account = payer.GetAccountsBlocking()[0];
            foreach (Transaction transaction in account.GetTransactionsBlocking(10, Level.Standard, null).List)
            {
                DisplayTransaction(
                           transaction.Amount.Currency,
                           transaction.Amount.Value,
                           transaction.Type, // debit or credit
                           transaction.Status);
            }
        }

        /// <summary>
        /// Illustrate Account.getTransaction
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="transfer">recently-completed transfer</param>
        /// <returns>a Transaction</returns>
        public static Transaction AccountGetTransactionSample(
           UserMember payer,
           Transfer transfer)
        {
            Tokenio.User.Account account = payer.GetAccountsBlocking()[0];

            string txnId = transfer.TransactionId;
            Transaction transaction = account.GetTransactionBlocking(txnId, Level.Standard);
            return transaction;
        }


        private static void DisplayTransaction(
             string currency,
             string value,
             TransactionType debitOrCredit,
             TransactionStatus status)
        {
        }


    }
}
