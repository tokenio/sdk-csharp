using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.User.Utils;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    /// <summary>
    /// Redeems a transfer token.
    /// </summary>
    public static class RedeemTransferTokenSample
    {

        /// <summary>
        /// Redeems a transfer token to transfer money from payer bank account to payee bank account.
        /// </summary>
        /// <param name="payee">payee Token member</param>
        /// <param name="accountId">account id of the payee</param>
        /// <param name="tokenId">ID of the token to redeem</param>
        /// <returns>a transfer Transfer</returns>
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
            TransferDestination tokenDestination = new TransferDestination
            {
                Token = new TransferDestination.Types.Token
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
