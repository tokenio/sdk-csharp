using Tokenio.Proto.Common.SubmissionProtos;
using Tokenio.Proto.Common.TokenProtos;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    /// <summary>
    /// Redeems a transfer token.
    /// </summary>
    public static class RedeemStandingOrderTokenSample
    {
        /// <summary>
        /// Redeems a transfer token to transfer money from payer bank account to payee bank account.
        /// </summary>
        /// <param name="payee">payee Token member</param>
        /// <param name="tokenId">ID of the token to redeem</param>
        /// <returns>a transfer Transfer</returns>
        public static StandingOrderSubmission RedeemStandingOrderToken(
            TppMember payee,
            string tokenId)
        {
            // Retrieve a transfer token to redeem.
            Token standingOrderToken = payee.GetTokenBlocking(tokenId);

            // Payee redeems a standing order token.
            // Money is transferred to a payee bank account.
            StandingOrderSubmission submission = payee.RedeemStandingOrderTokenBlocking(
                standingOrderToken.Id);

            return submission;
        }
    }
}