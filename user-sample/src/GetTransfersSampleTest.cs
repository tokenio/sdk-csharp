using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class GetTransfersSampleTest
    {
        [Fact]
        public void GetTransfersTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = CreateAndEndorseTransferTokenSample.CreateTransferToken(payer, payeeAlias);

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
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = CreateAndEndorseTransferTokenSample.CreateTransferToken(payer, payeeAlias);

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
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = CreateAndEndorseTransferTokenSample.CreateTransferToken(payer, payeeAlias);

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
