using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public static class GetTransfersSample
    {
        /// <summary>
        /// Illustrate Member.getTransfers
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void GetTransfers_Sample(UserMember payer)
        {
            var accounts = payer.GetAccountsBlocking();
            string accountId = accounts[0].Id();
            foreach (Transfer transfer in payer.GetTransfersBlocking(null, null, 10).List)
            {

                DisplayTransfer(
                       transfer.Status,
                       transfer.Payload.Description);

            }

        }

        /// <summary>
        /// Illustrate Member.getTransferTokens
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void GetTransferTokensSample(
            UserMember payer)
        {

            foreach (Token token in payer.GetTransferTokensBlocking(null, 10).List)
            {
                TransferBody transferBody = token.Payload.Transfer;
                DisplayTransferToken(
                       transferBody.Currency,
                       transferBody.LifetimeAmount);

            }

        }

        /// <summary>
        /// Illustrate Member.getTransfer
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="transferId">id of a transfer</param>
        /// <returns>a Transfer</returns>
        public static Transfer GetTransferSample(
            UserMember payer,
            string transferId)
        {
            Transfer transfer = payer.GetTransferBlocking(transferId);
            return transfer;
        }

        private static void DisplayTransfer(
           TransactionStatus status,
           string description)
        {
        }

        private static void DisplayTransferToken(
           string currency, string value)
        {
        }
    }
}
