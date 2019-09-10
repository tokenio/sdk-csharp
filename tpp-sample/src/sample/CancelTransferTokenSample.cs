using Tokenio.Proto.Common.TokenProtos;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    /// <summary>
    /// Cancels a transfer token.
    /// </summary>
    public static class CancelTransferTokenSample
    {
        /// <summary>
        /// Cancels the transfer token.
        /// </summary>
        /// <param name="payee">payee Token member</param>
        /// <param name="tokenId">token ID to cancel</param>
        /// <returns>operation result</returns>
        public static TokenOperationResult CancelTransferToken(TppMember payee, string tokenId)
        {
            // Retrieve a transfer token to cancel.
            Token transferToken = payee.GetTokenBlocking(tokenId);

            // Cancel transfer token.
            return payee.CancelTokenBlocking(transferToken);
        }
    }
}
