using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
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
                Account payeeAccount = LinkMemberAndBankSample.LinkBankAccounts(payee);
                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Level.Low);
                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                    payee,
                    payeeAccount.Id(),
                    token.Id);
                GetTransfersSample.GetTransfers_Sample(payer);
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
                Account payeeAccount = LinkMemberAndBankSample.LinkBankAccounts(payee);
                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Level.Low);
                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                    payee,
                    payeeAccount.Id(),
                    token.Id);
                GetTransfersSample.GetTransferTokensSample(payer);
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
                Account payeeAccount = LinkMemberAndBankSample.LinkBankAccounts(payee);
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