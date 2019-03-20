using Tokenio;
using Tokenio.Proto.Common.TokenProtos;

namespace samples
{
    /// <summary>
    /// Cancels an access token.
    /// </summary>
    public class CancelAccessTokenSample
    {
        public static TokenOperationResult CancelAccessToken(Member grantee, string tokenId)
        {
            // Retrieve an access token to cancel.
            var accessToken = grantee.GetToken(tokenId).Result;

            // Cancel access token.
            return grantee.CancelToken(accessToken).Result;
        }
    }
}
