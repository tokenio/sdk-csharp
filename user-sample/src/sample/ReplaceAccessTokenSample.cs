using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class ReplaceAccessTokenSample
    {
        /// <summary>
        /// Finds the access token.
        /// </summary>
        /// <returns>The access token.</returns>
        /// <param name="tokenClient">Token client.</param>
        /// <param name="grantor">Grantor.</param>
        /// <param name="granteeAlias">Grantee alias.</param>
        public static Token FindAccessToken(
            TokenClient tokenClient,
            Member grantor,
            Alias granteeAlias)
        {
            string granteeMemberId = tokenClient.GetMemberIdBlocking(granteeAlias);
            return grantor.GetActiveAccessTokenBlocking(granteeMemberId);
        }



        /// <summary>
        /// Replaces the access token.
        /// </summary>
        /// <returns>The access token.</returns>
        /// <param name="grantor">Grantor.</param>
        /// <param name="granteeAlias">Grantee alias.</param>
        /// <param name="oldToken">Old token.</param>
        public static TokenOperationResult ReplaceAccessToken(
          UserMember grantor,
          Alias granteeAlias,
          Token oldToken)
        {
            string accountId = grantor.CreateTestBankAccountBlocking(1000.0, "EUR")
                    .Id();

            // Replace the old access token
            Token newToken = grantor.ReplaceAccessTokenBlocking(
                    oldToken,
                    AccessTokenBuilder
                            .FromPayload(oldToken.Payload)
                            .ForAccount(accountId))
                    .Token;

            // Endorse the new access token
            TokenOperationResult status = grantor.EndorseTokenBlocking(newToken, Level.Standard);

            return status;
        }

    }
}
