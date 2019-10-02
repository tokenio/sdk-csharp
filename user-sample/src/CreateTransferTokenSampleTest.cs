using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class CreateTransferTokenSampleTest
    {
        [Fact]
        public void CreatePaymentTokenTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Key.Types.Level.Low);
                Assert.NotNull(token);
            }
        }

        [Fact]
        public void CreateScheduledPaymentTokenTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                Token token = CreateTransferTokenSample.CreateTransferTokenScheduled(payer, payeeAlias);
                Assert.NotNull(token);
                Assert.NotEmpty(token.Payload.Transfer.ExecutionDate);
            }
        }
    }
}
