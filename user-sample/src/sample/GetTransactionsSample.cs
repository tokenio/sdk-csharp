using System.Collections.Generic;
using System.Linq;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.User;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class GetTransactionsSample
    {
        /// <summary>
        /// Gets the transactions sample.
        /// </summary>
        /// <param name="payer">Payer.</param>
        public static void getTransactionsSample(UserMember payer)
        {
            List<Account> accounts = payer.GetAccountsBlocking().ToList();
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
        /// Gets the transaction sample.
        /// </summary>
        /// <returns>The transaction sample.</returns>
        /// <param name="payer">Payer.</param>
        /// <param name="transfer">Transfer.</param>
        public static Transaction GetTransactionSample(
          UserMember payer,
          Transfer transfer)
        {
            List<Account> accounts = payer.GetAccountsBlocking().ToList();
            string accountId = accounts[0].Id();

            string transactionId = transfer.TransactionId;
            Transaction transaction = payer.GetTransactionBlocking(accountId, transactionId, Level.Standard);
            return transaction;
        }

        /// <summary>
        /// Accounts the get transactions sample.
        /// </summary>
        /// <param name="payer">Payer.</param>
        public static void AccountGetTransactionsSample(UserMember payer)
        {
            Account account = payer.GetAccountsBlocking()[0];


            foreach (Transaction transaction in account.GetTransactionsBlocking(null, 10, Level.Standard).List)
            {
                DisplayTransaction(
                           transaction.Amount.Currency,
                           transaction.Amount.Value,
                           transaction.Type, // debit or credit
                           transaction.Status);
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <returns>The get transaction sample.</returns>
        /// <param name="payer">Payer.</param>
        /// <param name="transfer">Transfer.</param>
        public static Transaction AccountGetTransactionSample(
           UserMember payer,
           Transfer transfer)
        {
            Account account = payer.GetAccountsBlocking()[0];

            string txnId = transfer.TransactionId;
            Transaction transaction = account.GetTransactionBlocking(txnId, Level.Standard);
            return transaction;
        }


        /// <summary>
        /// Displaies the transaction.
        /// </summary>
        /// <param name="currency">Currency.</param>
        /// <param name="value">Value.</param>
        /// <param name="debitOrCredit">Debit or credit.</param>
        /// <param name="status">Status.</param>
        private static void DisplayTransaction(
            string currency,
            string value,
            TransactionType debitOrCredit,
            TransactionStatus status)
        {
        }


    }
}
