using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Tpp;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp {
    /// <summary>
    /// Redeems an information access token.
    /// </summary>
    public static class RedeemAccessTokenSample {

        /// <summary>
        /// Redeems access token to acquire access to the grantor's account balances.
        /// </summary>
        /// <param name="grantee">grantee Token member</param>
        /// <param name="tokenId">ID of the access token to redeem</param>
        /// <returns>balance of one of grantor's acounts</returns>
        public static Money RedeemAccessToken (TppMember grantee, string tokenId) {
            // Specifies whether the request originated from a customer
            bool customerInitiated = true;

            // Access grantor's account list by applying
            // access token to the grantee client.
            // forAccessToken snippet begin
            IRepresentable grantor = grantee.ForAccessToken (tokenId, customerInitiated);
            var accounts = grantor.GetAccountsBlocking ();

            // Get the data we want
            Money balance0 = accounts[0].GetBalanceBlocking (Key.Types.Level.Standard).Current;
            // forAccessToken snippet end
            return balance0;
        }

    }
}