using System;
using System;
using System.Collections.Generic;
using System.Linq;
using Tokenio.Tpp;
using TppMember = Tokenio.Tpp.Member;
using Tokenio.Proto.Common.MoneyProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using Tokenio.Proto.Common.TransactionProtos;

namespace TokenioSample
 
{
    /// <summary>
    /// Get balance sample.
    /// </summary>
    public class GetBalanceSample
    {
        public static IDictionary<string, double> MemberGetBalanceSample(TppMember member)
        {
            Dictionary<string, double> sums = new Dictionary<string, double>();

            IList<Account> accounts = member.GetAccountsBlocking();

            foreach (Account account in accounts)
            {
                Money balance = member.GetBalanceBlocking(account.Id(), Level.Standard)
                           .Current;

                sums[balance.Currency] = Double.Parse(balance.Value) + SampleExtensions.GetValueOrDefault(sums,balance.Currency,0.0);
            }
            return sums;
        }

        /// <summary>
        /// Accounts the get balance sample.
        /// </summary>
        /// <returns>The get balance sample.</returns>
        /// <param name="member">Member.</param>
        public static IDictionary<string, double> AccountGetBalanceSample(TppMember member)
        {
            Dictionary<string, double> sums = new Dictionary<string, double>();

            IList<Account> accounts = member.GetAccountsBlocking();

            foreach (Account account in accounts)
            {
                Money balance = account.GetBalanceBlocking(Level.Standard)
                           .Current;

                sums[balance.Currency] = Double.Parse(balance.Value) + SampleExtensions.GetValueOrDefault(sums, balance.Currency, 0.0);
            }
            return sums;

        }

        public static IList<Balance> memberGetBalanceListSample(TppMember member)
        {
            List<string> accountIds = member
                    .GetAccountsBlocking().Select(acc=> acc.Id()).ToList();

            var balances = member.GetBalancesBlocking(accountIds, Level.Standard);

            return balances;
        }

    }

}
