using System;
using Tokenio.Proto.Common.MoneyProtos;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;
using Tokenio.Tpp;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace TokenioSample
{
    public class RedeemAccessTokenSample
    {

        /// <summary>
        /// Redeems the access token.
        /// </summary>
        /// <returns>The access token.</returns>
        /// <param name="grantee">Grantee.</param>
        /// <param name="tokenId">Token identifier.</param>
        public static Money RedeemAccessToken(TppMember grantee, string tokenId)
        {
            // Specifies whether the request originated from a customer
            bool customerInitiated = true;

            // Access grantor's account list by applying
            // access token to the grantee client.
            // forAccessToken snippet begin
            IRepresentable grantor = grantee.ForAccessToken(tokenId, customerInitiated);
            var accounts = grantor.GetAccountsBlocking();

            // Get the data we want
            Money balance0 = accounts[0].GetBalanceBlocking(Level.Standard).Current;
            // forAccessToken snippet end
            return balance0;
        }


    }
}
