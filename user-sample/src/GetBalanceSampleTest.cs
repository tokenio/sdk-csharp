using System.Collections.Generic;
using Tokenio.Proto.Common.TransactionProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
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
                UserMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                member.CreateTestBankAccountBlocking(1000.0, "EUR");

                var sums = GetBalanceSample.MemberGetBalanceSample(member);
                Assert.Equal(1000.0, sums["EUR"]);
            }
        }

        [Fact]
        public void AccountGetBalanceSampleTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                member.CreateTestBankAccountBlocking(1000.0, "EUR");

                var sums = GetBalanceSample.AccountGetBalanceSample(member);
                Assert.Equal(1000.0, sums["EUR"]);
            }
        }

        [Fact]
        public void MemberGetBalancesSampleTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                member.CreateTestBankAccountBlocking(1000.0, "EUR");

                member.CreateTestBankAccountBlocking(500.0, "EUR");
                var balances = (List<Balance>)GetBalanceSample.MemberGetBalanceListSample(member);

                Assert.Equal(2, balances.Count);
                Assert.True(balances.Exists(b => double.Parse(b.Current.Value).Equals(500.0)));
                Assert.True(balances.Exists(b => double.Parse(b.Current.Value).Equals(1000.0)));

            }
        }
    }
}