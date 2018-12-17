using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace samples
{
    /// <summary>
    /// Working with existing access tokens: finding and replacing.
    /// </summary>
    public class ReplaceAccessTokenSample
    {
        /// <summary>
        /// Finds a previously-created access token from grantor to grantee.
        /// </summary>
        /// <param name="grantor">Token member granting access to her accounts</param>
        /// <param name="granteeAlias">Token member alias acquiring information access</param>
        /// <returns>the access Token</returns>
        public static Token FindAccessToken(MemberSync grantor, Alias granteeAlias)
        {
            foreach (var token in grantor.GetAccessTokens(null, 100).List)
            {
                var toAlias = token.Payload.To.Alias;
                if (toAlias.Equals(granteeAlias))
                {
                    return token;
                }
            }

            return null;
        }

        /// <summary>
        /// Replaces a previously-created access token.
        /// </summary>
        /// <param name="grantor">Token member granting access to her accounts</param>
        /// <param name="granteeAlias">Token member alias acquiring information access</param>
        /// <param name="oldToken">token to replace</param>
        /// <returns>success or failure</returns>
        public static TokenOperationResult ReplaceAccessToken(
            MemberSync grantor,
            Alias granteeAlias,
            Token oldToken)
        {
            // Replace the old access token
            var newToken = grantor.ReplaceAccessToken(
                oldToken,
                AccessTokenBuilder
                    .FromPayload(oldToken.Payload)
                    .ForAccount("12345678")
                    .ForAccountTransactions("12345678")
                    .Build())
               .Token;
            
            // Endorse the new access token
            var status = grantor.EndorseToken(newToken, Standard);
            
            return status;
        }
    }
}
