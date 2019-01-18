using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.TokenProtos;

namespace samples
{
    public class NotifySample
    {
        /// <summary>
        /// Creates a payment request (a transfer token payload)
        /// and sends it to a potential payer.
        /// </summary>
        /// <param name="tokenClient">initialized SDK</param>
        /// <param name="payee">payer Token member</param>
        /// <param name="payerAlias">payee Token member alias</param>
        /// <returns>a transfer Token</returns>
        public static NotifyStatus NotifyPaymentRequest(
            TokenClient tokenClient,
            Member payee,
            Alias payerAlias)
        {
            // We'll use this as a reference ID. Normally, a payee who
            // explicitly sets a reference ID would use an ID from a db.
            // E.g., an online merchant might use the ID of a "shopping cart".
            // We don't have a db, so we fake it with a random string:
            var cartId = Util.Nonce();
            var paymentRequest = new TokenPayload
            {
                Description = "Sample payment request",
                From = new TokenMember
                {
                    Alias = payerAlias
                },
                To = new TokenMember
                {
                    Alias = payee.GetFirstAlias().Result
                },
                Transfer = new TransferBody
                {
                    Amount = "100.00",
                    Currency = "EUR"
                },
                RefId = cartId
            };

            var status = tokenClient.NotifyPaymentRequest(paymentRequest).Result;
            return status;
        }
    }
}
