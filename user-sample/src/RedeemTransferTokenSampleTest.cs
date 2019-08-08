using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class RedeemTransferTokenSampleTest
    {

        [Fact]
        public void RedeemPaymentTokenTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = LinkMemberAndBankSample.LinkBankAccounts(payee);

                Token token = CreateAndEndorseTransferTokenSample.CreateTransferToken(payer, payeeAlias);

                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);
                Assert.NotNull(transfer);
            }
        }
    }
}
