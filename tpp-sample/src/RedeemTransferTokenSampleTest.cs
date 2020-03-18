using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Sample.User;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp
{
    public class RedeemTransferTokenSampleTest
    {

        [Fact]
        public void RedeemPaymentTokenTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateUserMember();
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

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
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateUserMember();
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

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
