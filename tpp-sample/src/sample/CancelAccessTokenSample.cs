using Tokenio.Proto.Common.TokenProtos;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    /// <summary>
    /// Cancels an access token.
    /// </summary>
    public static class CancelAccessTokenSample
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
    }
}
