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
        public static Money RedeemAccessToken(MemberSync grantee, string tokenId)
        {
            // Specifies whether the request originated from a customer
            var customerInitiated = true;

            // Access grantor's account list by applying
            // access token to the grantee client.
            grantee.UseAccessToken(tokenId, customerInitiated);
            var grantorAccounts = grantee.GetAccounts();

            // Get the data we want
            var balance0 = grantorAccounts[0].GetCurrentBalance(Standard);
            // When done using access, clear token from grantee client.
            grantee.ClearAccessToken();
            return balance0;
        }
    }
}