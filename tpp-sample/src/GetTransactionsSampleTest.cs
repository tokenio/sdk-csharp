using System;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class GetTransactionsSampleTest
    {
        [Fact]
        public void GetTransactionsTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {

                UserMember payer = TestUtil.CreateUserMember();
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = TestUtil.CreateTransferToken(payer, payeeAlias);

                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);

                string transactionId = transfer.TransactionId;
                Transaction transaction = payer.GetTransactionBlocking(
                        payer.GetAccountsBlocking()[0].Id(),
                        transactionId,
                        Level.Standard);
                Assert.Equal(transaction.TokenId, token.Id);
            }
        }

        [Fact]
        public void AccountGetTransactionsTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateUserMember();
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = TestUtil.CreateTransferToken(payer, payeeAlias);

                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);

                Account account = payer.GetAccountsBlocking()[0];
                Transaction transaction = account.GetTransactionBlocking(
                        transfer.TransactionId,
                        Level.Standard);
                Assert.Equal(transaction.TokenId, token.Id);
            }

        }
    }
}
