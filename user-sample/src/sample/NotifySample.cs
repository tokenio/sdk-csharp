using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User.Utils;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;
namespace TokenioSample
{
    public static class NotifySample
    {

        /// <summary>
        /// Creates a payment request (a transfer token payload)
        /// and sends it to a potential payer.
        /// </summary>
        /// <param name="tokenClient">initialized SDK</param>
        /// <param name="payee">payee Token member</param>
        /// <param name="payerAlias">payer Token member alias</param>
        /// <returns>a transfer Token</returns>
        public static NotifyStatus NotifyPaymentRequest(
           TokenClient tokenClient,
           UserMember payee,
           Alias payerAlias)
        {
            // We'll use this as a reference ID. Normally, a payee who
            // explicitly sets a reference ID would use an ID from a db.
            // E.g., an online merchant might use the ID of a "shopping cart".
            // We don't have a db, so we fake it with a random string:
            string cartId = Util.Nonce();
            TokenPayload paymentRequest = new TokenPayload
            {

                Description = "Sample payment request",
                From = new TokenMember { Alias = payerAlias },
                To = new TokenMember { Alias = payee.GetFirstAliasBlocking() },
                Transfer = new TransferBody { Amount = "100.00", Currency = "EUR" },
                RefId = cartId

            };


            NotifyStatus status = tokenClient.NotifyPaymentRequestBlocking(paymentRequest);
            return status;
        }

    }

}

