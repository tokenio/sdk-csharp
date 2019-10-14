using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    public class LinkMemberAndBankSampleTest {
        [Fact]
        public void LinkMemberAndBankTest() {
            using(Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                LinkMemberAndBankSample.LinkBankAccounts(member);
                Assert.NotEmpty(member.GetAccountsBlocking());
            }
        }
    }
}
