using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace samples
{
    /// <summary>
    /// Creates an information access token and endorses it to a grantee.
    /// </summary>
    public class CreateAndEndorseAccessTokenSample
    {
        /// <summary>
        /// Creates an information access token to allow a grantee to see all bank balances of a grantor.
        /// </summary>
        /// <param name="grantor"></param>
        /// <param name="granteeAlias"></param>
        /// <returns></returns>
        public static Token CreateAccessToken(MemberSync grantor, Alias granteeAlias)
        {
            // Create an access token for the grantee to access bank
            // account names of the grantor.
            var accessToken = grantor.CreateAccessToken(AccessTokenBuilder
                .Create(granteeAlias)
                .ForAllAccounts() // user can call getAccounts()
                .ForAllBalances() // for each account, can call getBalance()
                .Build());

            // Grantor endorses a token to a grantee by signing it
            // with her secure private key.
            accessToken = grantor.EndorseToken(accessToken, Standard).Token;

            return accessToken;
        }
    }
}
