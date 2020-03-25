using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class CreateStandingOrderTokenSampleTest
    {
        [Fact]
        public void CreateStandingOrderTokenTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                Token token =
                    CreateStandingOrderTokenSample.CreateStandingOrderToken(payer, payeeAlias, Level.Standard);
                Assert.NotNull(token);
            }
        }
    }
}
