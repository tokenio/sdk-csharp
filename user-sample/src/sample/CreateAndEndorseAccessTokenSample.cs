using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using UserMember = Tokenio.User.Member;
namespace Tokenio.Sample.User
{
    /// <summary>
    /// Creates an information access token and endorses it to a grantee.
    /// </summary>
    public static class CreateAndEndorseAccessTokenSample
    {
        /// <summary>
        /// Creates an information access token to allow a grantee to see the balance
        /// of one of the grantor's accounts.
        /// </summary>
        /// <param name="grantor">Token member granting access to her account</param>
        /// <param name="accountId">ID of account to grant access to.</param>
        /// <param name="granteeAlias">Token member alias acquiring information access</param>
        /// <returns>an access Token</returns>
        public static Token CreateAccessToken(
             UserMember grantor,
          string accountId,
          Alias granteeAlias)
        {
            // Create an access token for the grantee to access bank
            // account names of the grantor.
            Token accessToken = grantor.CreateAccessTokenBlocking(
                    Tokenio.User.AccessTokenBuilder
                            .Create(granteeAlias)
                            .ForAccount(accountId)
                            .ForAccountBalances(accountId));

            // Grantor endorses a token to a grantee by signing it
            // with her secure private key.
            accessToken = grantor.EndorseTokenBlocking(
                    accessToken,
                        Key.Types.Level.Low).Token;

            return accessToken;
        }

    }
}
