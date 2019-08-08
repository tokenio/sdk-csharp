using Tokenio.Proto.Common.TokenProtos;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    /// <summary>
    /// Cancels an access token.
    /// </summary>
    public static class CancelAccessTokenSample
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
    }
}
