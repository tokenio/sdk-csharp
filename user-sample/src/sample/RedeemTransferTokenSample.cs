using System;
using Tokenio.Proto.Common.TransferProtos;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;
using Tokenio.User.Utils;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.AccountProtos;

namespace TokenioSample
{
    /// <summary>
    /// Redeem transfer token sample.
    /// </summary>
    public class RedeemTransferTokenSample
    {

        /// <summary>
        /// Redeems a transfer token to transfer money from payer bank account to payee bank account.
        /// </summary>
        /// <returns>The transfer token.</returns>
        /// <param name="payee">Payee.</param>
        /// <param name="accountId">Account identifier.</param>
        /// <param name="tokenId">Token identifier.</param>
        public static Transfer RedeemTransferToken(
           UserMember payee,
           string accountId, // account ID of the payee
           string tokenId)
        { // ID of token to redeem
          // We'll use this as a reference ID. Normally, a payee who
          // explicitly sets a reference ID would use an ID from a db.
          // E.g., an online merchant might use the ID of a "shopping cart".
          // We don't have a db, so we fake it with a random string:
            string cartId = Util.Nonce();

            // Retrieve a transfer token to redeem.
            Token transferToken = payee.GetTokenBlocking(tokenId);

            // Set token destination
            TransferDestination tokenDestination = new TransferDestination()
            {
                Token = new TransferDestination.Types.Token()
                    {
                        MemberId = payee.MemberId(),
                        AccountId = accountId
                    }
                };

          

            // Payee redeems a transfer token.
            // Money is transferred to a payee bank account.
            Transfer transfer = payee.RedeemTokenBlocking(
                    transferToken,
                    tokenDestination,
                    // if refId not set, transfer will have random refID:
                    cartId);

            return transfer;
        }

    }
}
