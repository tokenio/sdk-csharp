using System.Collections.Generic;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Tpp;
using TppMember = Tokenio.Tpp.Member;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;


namespace Tokenio.Sample.Tpp
{
    /// <summary>
    /// Redeems an information access token.
    /// </summary>
    public static class RedeemAccessTokenSample
    {

        /// <summary>
        /// Redeems access token to acquire access to the grantor's account balances.
        /// </summary>
        /// <param name="grantee">grantee Token member</param>
        /// <param name="tokenId">ID of the access token to redeem</param>
        /// <returns>balance of one of grantor's acounts</returns>
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
            Money balance0 = accounts[0].GetBalanceBlocking(Key.Types.Level.Standard).Current;
            // forAccessToken snippet end
            return balance0;
        }

        /// <summary>
        /// Redeems access token to acquire access to the grantor's standing orders at an account.
        /// </summary>
        /// <param name="grantee">grantee Token member</param>
        /// <param name="tokenId">the access token to redeem</param>
        /// <returns>standing orders of one of grantor's accounts</returns>
        public static IList<StandingOrder> RedeemStandingOrdersAccessToken(
            TppMember grantee,
            string tokenId)
        {
            // Specifies whether the request originated from a customer
            bool customerInitiated = true;

            // Access grantor's account list by applying
            // access token to the grantee client.
            // forAccessToken snippet begin
            IRepresentable grantor = grantee.ForAccessToken(tokenId, customerInitiated);
            var accounts = grantor.GetAccountsBlocking();

            // Get the first 5 standing orders
            PagedList<StandingOrder> standingOrders = accounts[0]
                .GetStandingOrdersBlocking(5, Level.Standard, null);

            // Pass this offset to the next getStandingOrders
            // call to fetch the next page of standing orders.
            string nextOffset = standingOrders.Offset;

            return standingOrders.List;
        }

    }
}
