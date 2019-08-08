using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class LinkMemberAndBankSampleTest
    {
        [Fact]
        public void LinkMemberAndBankTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());

                LinkMemberAndBankSample.LinkBankAccounts(member);

                Assert.NotEmpty(member.GetAccountsBlocking());

            }
        }
    }
}
