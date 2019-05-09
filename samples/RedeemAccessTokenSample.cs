using Tokenio;
using Tokenio.Proto.Common.MoneyProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace Sample
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
            var customerInitiated = true;

            var grantor = grantee.ForAccessToken(tokenId, customerInitiated);
            var accounts = grantor.GetAccounts().Result;

            Money balance0 = accounts[0].GetBalance(Standard).Result.Current;
            return balance0;
        }
    }
}