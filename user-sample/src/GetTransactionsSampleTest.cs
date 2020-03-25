using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class GetTransactionsSampleTest
    {
        [Fact]
        public void GetTransactionsTest()
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


                GetTransactionsSample.getTransactionsSample(payer);
                GetTransactionsSample.GetTransactionsByDateSample(payer);
                Transaction transaction = GetTransactionsSample.GetTransactionSample(payer, transfer);
                Assert.Equal(transaction.TokenId, token.Id);
            }
        }

        [Fact]
        public void AccountGetTransactionsTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = LinkMemberAndBankSample.LinkBankAccounts(payee);

                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias,Level.Low);

                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);

                GetTransactionsSample.AccountGetTransactionSample(payer, transfer);
                Transaction transaction = GetTransactionsSample.AccountGetTransactionSample(payer, transfer);

                Assert.Equal(transaction.TokenId, token.Id);
            }
        }
    }
}
