using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;
namespace TokenioSample
{
    public  class CreateAndEndorseAccessTokenSample
    {

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
                        Level.Low).Token;

            return accessToken;
        }

    }
}
