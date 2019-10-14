using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    public class CreateTransferTokenSampleTest {
        [Fact]
        public void CreatePaymentTokenTest() {
            using(Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Level.Low);
                Assert.NotNull(token);
            }
        }

        [Fact]
        public void CreatePaymentTokenWithOtherOptionsTest() {
            using(Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                UserMember payee = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                Token token = CreateTransferTokenSample.CreateTransferTokenWithOtherOptions(payer, payee.MemberId());
                Assert.NotNull(token);
            }
        }

        [Fact]
        public void CreatePaymentTokenToDestinationTest() {
            using(Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                Token token = CreateTransferTokenSample.CreateTransferTokenToDestination(payer, payeeAlias);
                Assert.NotNull(token);
            }
        }

        [Fact]
        public void CreatePaymentTokenScheduledTest() {
            using(Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                Token token = CreateTransferTokenSample.CreateTransferTokenScheduled(payer, payeeAlias);
                Assert.NotNull(token);
            }
        }
    }
}
