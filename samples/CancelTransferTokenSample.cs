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
        public static TokenOperationResult CancelTransferToken(Member payee, string tokenId) {
            // Retrieve a transfer token to cancel.
            var transferToken = payee.GetToken(tokenId).Result;

            // Cancel transfer token.
            return payee.CancelToken(transferToken).Result;
        }
    }
}
