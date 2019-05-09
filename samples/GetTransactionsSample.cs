using Tokenio;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace Sample
{
    public class GetTransactionsSample
    {
        /// <summary>
        /// Illustrate Member.GetTransactions
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void GetTransactionsSample_(Member payer)
        {
            var accounts = payer.GetAccounts().Result;
            var accountId = accounts[0].Id();
            foreach (var transaction in payer.GetTransactions(accountId, 10, Standard, null).Result.List)
            {
                DisplayTransaction(
                    transaction.Amount.Currency,
                    transaction.Amount.Value,
                    transaction.Type, // debit or credit
                    transaction.Status);
            }
        }

        /// <summary>
        /// Illustrate Member.GetTransaction
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="transfer">recently-completed transfer</param>
        /// <returns>a transaction</returns>
        public static Transaction GetTransactionSample(
            Member payer,
            Transfer transfer)
        {
            var accounts = payer.GetAccounts().Result;
            var accountId = accounts[0].Id();

            var transactionId = transfer.TransactionId;
            var transaction = payer.GetTransaction(accountId, transactionId, Standard).Result;
            return transaction;
        }

        /// <summary>
        /// Illustrate Account.GetTransactions
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void AccountGetTransactionsSample(Member payer)
        {
            var account = payer.GetAccounts().Result[0];

            foreach (var transaction in account.GetTransactions(null, 10, Standard).Result.List)
            {
                DisplayTransaction(
                    transaction.Amount.Currency,
                    transaction.Amount.Value,
                    transaction.Type, // debit or credit
                    transaction.Status);
            }
        }

        /// <summary>
        /// Illustrate Account.GetTransaction
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="transfer">recently-completed transfer</param>
        /// <returns>a Transaction</returns>
        public static Transaction AccountGetTransactionSample(
            Member payer,
            Transfer transfer)
        {
            var account = payer.GetAccounts().Result[0];

            var txnId = transfer.TransactionId;
            var transaction = account.GetTransaction(txnId, Standard).Result;
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