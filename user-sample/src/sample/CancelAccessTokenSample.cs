using Tokenio.Proto.Common.TokenProtos;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class CancelAccessTokenSample
    {
        /// <summary>
        /// Cancels the access token.
        /// </summary>
        /// <returns>The access token.</returns>
        /// <param name="grantee">Grantee.</param>
        /// <param name="tokenId">Token identifier.</param>
        public static TokenOperationResult CancelAccessToken(UserMember grantor, string tokenId)
        {
            // Retrieve an access token to cancel.
            Token accessToken = grantor.GetTokenBlocking(tokenId);
            // Cancel access token.
            return grantor.CancelTokenBlocking(accessToken);

        }
    }
}
