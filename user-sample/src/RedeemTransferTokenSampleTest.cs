using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class RedeemTransferTokenSampleTest
    {

        [Fact]
        public void RedeemPaymentTokenTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = LinkMemberAndBankSample.LinkBankAccounts(payee);
                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Level.Low);
                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);
                Assert.NotNull(transfer);
            }
        }

        [Fact]
        public void RedeemScheduledPaymentTokenTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                Account payeeAccount = LinkMemberAndBankSample.LinkBankAccounts(payee);
                Token token = CreateTransferTokenSample.CreateTransferTokenScheduled(payer, payeeAlias);
                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                    payee,
                    payeeAccount.Id(),
                    token.Id);
                Assert.NotNull(transfer);
                Assert.NotEmpty(transfer.ExecutionDate);
            }
        }
    }
}
