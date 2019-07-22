using Tokenio.Proto.Common.TokenProtos;
using TppMember = Tokenio.Tpp.Member;

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
        public static TokenOperationResult CancelAccessToken(TppMember grantee, string tokenId)
        {
            // Retrieve an access token to cancel.
            Token accessToken = grantee.GetTokenBlocking(tokenId);
            // Cancel access token.
            return grantee.CancelTokenBlocking(accessToken);

        }
    }
}
