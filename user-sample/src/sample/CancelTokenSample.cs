using Tokenio.Proto.Common.TokenProtos;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    /// <summary>
    /// Cancels an access token.
    /// </summary>
    public static class CancelTokenSample
    {
        /// <summary>
        /// Cancels an access token.
        /// </summary>
        /// <param name="grantor">grantor Token member</param>
        /// <param name="tokenId">token ID to cancel</param>
        /// <returns>operation result</returns>
        public static TokenOperationResult CancelAccessToken(UserMember grantor, string tokenId)
        {
            // Retrieve an access token to cancel.
            Token accessToken = grantor.GetTokenBlocking(tokenId);
            // Cancel access token.
            return grantor.CancelTokenBlocking(accessToken);
        }

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
