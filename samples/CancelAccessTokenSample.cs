using Tokenio;
using Tokenio.Proto.Common.TokenProtos;

namespace samples
{
    /// <summary>
    /// Cancels an access token.
    /// </summary>
    public class CancelAccessTokenSample
    {
        public static TokenOperationResult CancaelAccessToken(MemberSync grantor, string tokenId)
        {
            // Retrieve an access token to cancel.
            var accessToken = grantor.GetToken(tokenId);

            // Cancel access token.
            return grantor.CancelToken(accessToken);
        }
    }
}