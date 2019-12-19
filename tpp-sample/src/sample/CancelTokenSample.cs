using Tokenio.Proto.Common.TokenProtos;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    /// <summary>
    /// Cancels an access token.
    /// </summary>
    public static class CancelTokenSample
    {
        /// <summary>
        /// Cancels the access token.
        /// </summary>
        /// <param name="grantee">grantee Token member</param>
        /// <param name="tokenId">token ID to cancel</param>
        /// <returns>operation result</returns>
        public static TokenOperationResult CancelAccessToken(TppMember grantee, string tokenId)
        {
            // Retrieve an access token to cancel.
            Token accessToken = grantee.GetTokenBlocking(tokenId);
            // Cancel access token.
            return grantee.CancelTokenBlocking(accessToken);
        }

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