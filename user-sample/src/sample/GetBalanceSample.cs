using System;
using System.Collections.Generic;
using System.Linq;
using Tokenio.User;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.TransactionProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace TokenioSample

{
    /// <summary>
    /// Get balance sample.
    /// </summary>
    public class GetBalanceSample
    {
        public static IDictionary<string, double> MemberGetBalanceSample(UserMember member)
        {
            Dictionary<string, double> sums = new Dictionary<string, double>();

            IList<Account> accounts = member.GetAccountsBlocking();

            foreach (Account account in accounts)
            {
                Money balance = member.GetCurrentBalanceBlocking(account.Id(), Level.Standard);

                sums[balance.Currency] = Double.Parse(balance.Value) + SampleExtensions.GetValueOrDefault(sums, balance.Currency, 0.0);
            }
            return sums;
        }

        /// <summary>
        /// Accounts the get balance sample.
        /// </summary>
        /// <returns>The get balance sample.</returns>
        /// <param name="member">Member.</param>
        public static IDictionary<string, double> AccountGetBalanceSample(UserMember member)
        {
            Dictionary<string, double> sums = new Dictionary<string, double>();

            IList<Account> accounts = member.GetAccountsBlocking();

            foreach (Account account in accounts)
            {
                Money balance = account.GetCurrentBalanceBlocking(Level.Standard);


                sums[balance.Currency] = Double.Parse(balance.Value) + SampleExtensions.GetValueOrDefault(sums, balance.Currency, 0.0);
            }
            return sums;

        }

        public static IList<Balance> MemberGetBalanceListSample(UserMember member)
        {
            List<string> accountIds = member
                    .GetAccountsBlocking().Select(acc => acc.Id()).ToList();

            var balances = member.GetBalancesBlocking(accountIds, Level.Standard);

            return balances;
        }

    }

}
