using System;
using System.Collections.Generic;
using Tokenio.Proto.Common.TransactionProtos;
using Xunit;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class GetBalanceSampleTest
    {

        [Fact]
        public void MemberGetBalanceSampleTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                member.CreateTestBankAccountBlocking(1000.0, "EUR");

                var sums = GetBalanceSample.MemberGetBalanceSample(member);
                Assert.Equal(sums["EUR"], 1000.0);
            }
        }

        [Fact]
        public void AccountGetBalanceSampleTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                member.CreateTestBankAccountBlocking(1000.0, "EUR");

                var sums = GetBalanceSample.AccountGetBalanceSample(member);
                Assert.Equal(sums["EUR"], 1000.0);
            }
        }

        [Fact]
        public void MemberGetBalancesSampleTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                member.CreateTestBankAccountBlocking(1000.0, "EUR");

                member.CreateTestBankAccountBlocking(500.0, "EUR");
                List<Balance> balances = (List<Balance>)GetBalanceSample.memberGetBalanceListSample(member);

                Assert.Equal(balances.Count, 2);
                Assert.True(balances.Exists(b => Double.Parse(b.Current.Value) == 500.0));
                Assert.True(balances.Exists(b => Double.Parse(b.Current.Value) == 1000.0));

            }


        }
    }
}