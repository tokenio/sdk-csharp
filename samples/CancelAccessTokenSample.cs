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
            var accessToken = grantee.GetToken(tokenId).Result;
            return grantee.CancelToken(accessToken).Result;
        }
    }
}
