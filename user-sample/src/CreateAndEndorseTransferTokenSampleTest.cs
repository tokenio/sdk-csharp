using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    public class CreateAndEndorseTransferTokenSampleTest {
        [Fact]
        public void CreatePaymentTokenTest () {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient ()) {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts (tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias ();
                UserMember payee = tokenClient.CreateMemberBlocking (payeeAlias);
                Token token = CreateAndEndorseTransferTokenSample.CreateTransferToken (payer, payeeAlias);
                Assert.NotNull (token);
            }
        }
    }
}