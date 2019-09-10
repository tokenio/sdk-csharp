using Tokenio.Proto.Common.TokenProtos;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    /// <summary>
    /// Cancels a transfer token.
    /// </summary>
    public static class CancelTransferTokenSample
    {
        /// <summary>
        /// Cancels a transfer token.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="tokenId">token ID to cancel</param>
        /// <returns>operation result</returns>
        public static TokenOperationResult CancelTransferToken(UserMember payer, string tokenId)
        {
            // Retrieve a transfer token to cancel.
            Token transferToken = payer.GetTokenBlocking(tokenId);

            // Cancel transfer token.
            return payer.CancelTokenBlocking(transferToken);
        }
    }
}
