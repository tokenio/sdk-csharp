using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    /// <summary>
    /// Working with existing access tokens: finding and replacing.
    /// </summary>
    public static class ReplaceAccessTokenSample {
        /// <summary>
        /// Finds a previously-created access token from grantor to grantee.
        /// </summary>
        /// <param name="tokenClient">initialized SDK</param>
        /// <param name="grantor">Token member granting access to her accounts</param>
        /// <param name="granteeAlias">Token member alias acquiring information access</param>
        /// <returns>an access Token</returns>
        public static Token FindAccessToken (
            Tokenio.User.TokenClient tokenClient,
            UserMember grantor,
            Alias granteeAlias) {
            string granteeMemberId = tokenClient.GetMemberIdBlocking (granteeAlias);
            return grantor.GetActiveAccessTokenBlocking (granteeMemberId);
        }

        /// <summary>
        /// Replaces a previously-created access token.
        /// </summary>
        /// <param name="grantor">Token member granting access to her accounts</param>
        /// <param name="granteeAlias">Token member alias acquiring information access</param>
        /// <param name="oldToken">token to replace</param>
        /// <returns>success or failure</returns>
        public static TokenOperationResult ReplaceAccessToken (
            UserMember grantor,
            Alias granteeAlias,
            Token oldToken) {
            string accountId = grantor.CreateTestBankAccountBlocking (1000.0, "EUR")
                .Id ();

            // Replace the old access token
            Token newToken = grantor.ReplaceAccessTokenBlocking (
                    oldToken,
                    AccessTokenBuilder
                    .FromPayload (oldToken.Payload)
                    .ForAccount (accountId))
                .Token;

            // Endorse the new access token
            TokenOperationResult status = grantor.EndorseTokenBlocking (newToken, Key.Types.Level.Standard);

            return status;
        }

    }
}