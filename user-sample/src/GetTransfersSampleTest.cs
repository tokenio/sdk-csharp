using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class GetTransfersSampleTest
    {
        [Fact]
        public void GetTransfersTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Key.Types.Level.Low);

                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);

                GetTransfersSample.GetTransfers_Sample(payee);
            }
        }


        [Fact]
        public void GetTransferTokensTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Key.Types.Level.Low);

                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);

                GetTransfersSample.GetTransferTokensSample(payee);
            }
        }


        [Fact]
        public void GetTransferTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Key.Types.Level.Low);

                Transfer redeemedTransfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);

                Transfer gotTransfer = GetTransfersSample.GetTransferSample(
                        payee,
                        redeemedTransfer.Id);
                Assert.Equal(gotTransfer, redeemedTransfer);
            }

        }
    }
}
