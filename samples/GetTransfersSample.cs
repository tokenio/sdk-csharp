using Tokenio;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;

namespace Sample
{
    public class GetTransfersSample
    {
        /// <summary>
        /// Illustrate Member.GetTransfers
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void GetTransfersSample_(Member payer)
        {
            foreach (var transfer in payer.GetTransfers(null, null, 10).Result.List)
            {
                DisplayTransfer(
                    transfer.Status,
                    transfer.Payload.Description);
            }
        }

        /// <summary>
        /// Illustrate Member.GetTransferTokens
        /// </summary>
        /// <param name="payer">payer Token member</param>
        public static void GetTransferTokensSample(Member payer)
        {
            foreach (var token in payer.GetTransferTokens(null, 10).Result.List)
            {
                var transferBody = token.Payload.Transfer;
                DisplayTransferToken(
                    transferBody.Currency,
                    transferBody.LifetimeAmount);
            }
        }

        /// <summary>
        /// Illustrate Member.GetTransfer
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="transferId">id of a transfer</param>
        /// <returns></returns>
        public static Transfer GetTransferSample(
            Member payer,
            string transferId)
        {
            var transfer = payer.GetTransfer(transferId).Result;
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
