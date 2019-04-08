using Tokenio;
using Tokenio.Proto.Common.MoneyProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace samples
{
    public class RedeemAccessTokenSample
    {
        /// <summary>
        /// Redeems access token to acquire access to the grantor's account balances.
        /// </summary>
        /// <param name="grantee">grantee Token member</param>
        /// <param name="tokenId">ID of the access token to redeem</param>
        /// <returns>balance of one of grantor's acounts</returns>
        public static Money RedeemAccessToken(Member grantee, string tokenId)
        {
            // Specifies whether the request originated from a customer
            var customerInitiated = true;

            // Access grantor's account list by applying
            // access token to the grantee client.
            // forAccessToken snippet begin
            var grantor = grantee.ForAccessToken(tokenId, customerInitiated);
            var accounts = grantor.GetAccounts().Result;

            // Get the data we want
            Money balance0 = accounts[0].GetBalanceBlocking(Standard).Current;
            // forAccessToken snippet end
            return balance0;
        }
    }
}
