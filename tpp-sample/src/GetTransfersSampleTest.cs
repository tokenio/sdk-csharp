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
    public class GetTransfersSampleTest
    {
        [Fact]
        public void GetTransfersTest()
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

                GetTransfersSample.GetTransfers_Sample(payee);
            }
        }

        [Fact]
        public void GetTransferTokensTest()
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

                GetTransfersSample.GetTransferTokensSample(payee);
            }
        }

        [Fact]
        public void GetTransferTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateUserMember();
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Level.Low);

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