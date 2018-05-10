using Tokenio;
using Tokenio.Proto.Common.TokenProtos;

namespace samples
{
    /// <summary>
    /// Cancels a transfer token.
    /// </summary>
    public class CancelTransferTokenSample
    {
        /// <summary>
        /// Cancels a transfer token.
        /// </summary>
        /// <param name="grantor">grantor Token member</param>
        /// <param name="tokenId">token ID to cancel</param>
        /// <returns>operation result</returns>
        public static TokenOperationResult CancelTransferToken(MemberSync grantor, string tokenId) {
            // Retrieve a transfer token to cancel.
            var transferToken = grantor.GetToken(tokenId);

            // Cancel transfer token.
            return grantor.CancelToken(transferToken);
        }
    }
}