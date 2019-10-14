using System;
using System.Collections.Generic;
using System.Linq;
using Tokenio;
using Tokenio.Proto.Common.TransactionProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace Sample {
    /// <summary>
    /// Two ways to get balances of a member's bank accounts.
    /// </summary>
    public class GetBalanceSample {
        /// <summary>
        /// Get a member's balances.
        /// </summary>
        /// <param name="member">member</param>
        /// <returns>dictionary currency: total</returns>
        public static IDictionary<string, double> MemberGetBalanceSample(Member member) {
            var sums = new Dictionary<string, double>();

            var accounts = member.GetAccounts().Result;
            foreach (var account in accounts) {
                var balance = member.GetBalanceBlocking(account.Id(), Standard).Current;
                if (sums.ContainsKey(balance.Currency)) {
                    sums[balance.Currency] += Convert.ToDouble(balance.Value);
                } else {
                    sums[balance.Currency] = Convert.ToDouble(balance.Value);
                }
            }

            return sums;
        }

        /// <summary>
        /// Get a member's list of balances.
        /// </summary>
        /// <param name="member">member</param>
        /// <returns>a list of balances</returns>
        public static IList<Balance> MemberGetBalanceListSample(Member member) {
            var accountIds = member.GetAccounts()
                .Result
                .Select(account => account.Id())
                .ToList();

            var balances = member.GetBalances(accountIds, Standard).Result;

            return balances;
        }
    }
}
