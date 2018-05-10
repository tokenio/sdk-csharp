using Tokenio;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;

namespace samples
{
    public class GetTransfersSample
    {
        /// <summary>
        /// Illustrate MemberSync.GetTransfers
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void GetTransfersSample_(MemberSync payer)
        {
            foreach (var transfer in payer.GetTransfers(null, 10, null).List)
            {
                DisplayTransfer(
                    transfer.Status,
                    transfer.Payload.Description);
            }
        }

        /// <summary>
        /// Illustrate MemberSync.GetTransferTokens
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void GetTransferTokensSample(MemberSync payer)
        {
            foreach (var token in payer.GetTransferTokens(null, 10).List)
            {
                var transferBody = token.Payload.Transfer;
                DisplayTransferToken(
                    transferBody.Currency,
                    transferBody.LifetimeAmount);
            }
        }

        /// <summary>
        /// Illustrate MemberSync.GetTransfer
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="transferId">id of a transfer</param>
        /// <returns></returns>
        public static Transfer GetTransferSample(
            MemberSync payer,
            string transferId)
        {
            var transfer = payer.GetTransfer(transferId);
            return transfer;
        }

        private static void DisplayTransfer(
            TransactionStatus status,
            string description)
        {
        }

        private static void DisplayTransferToken(
            string currency,
            string value)
        {
        }
    }
}