using Tokenio.Proto.Common.SubmissionProtos;
using Tokenio.Proto.Common.TokenProtos;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    /// <summary>
    /// Redeems a standing order token.
    /// </summary>
    public static class RedeemStandingOrderTokenSample
    {
        /// <summary>
        /// Redeems a standing order token to make a series of transfers from payer bank account
        /// to payee bank account.
        /// </summary>
        /// <param name="payee">payee Token member</param>
        /// <param name="tokenId">ID of the token to redeem</param>
        /// <returns>standing order submission record</returns>
        public static StandingOrderSubmission RedeemStandingOrderToken(
            UserMember payee,
            string tokenId) // ID of token to redeem
        {
            // Retrieve a standing order token to redeem.
            Token token = payee.GetTokenBlocking(tokenId);
            // Payee redeems a standing order token.
            // Money is transferred in many installments to a payee bank account.
            StandingOrderSubmission submission = payee.RedeemStandingOrderTokenBlocking(token.Id);
            return submission;
        }
    }
}