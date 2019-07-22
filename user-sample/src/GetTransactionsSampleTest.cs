﻿using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
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

                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = LinkMemberAndBankSample.LinkBankAccounts(payee);

                Token token = CreateAndEndorseTransferTokenSample.CreateTransferToken(payer, payeeAlias);

                Transfer transfer = RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);


                GetTransactionsSample.getTransactionsSample(payer);
                Transaction transaction = GetTransactionsSample.GetTransactionSample(payer, transfer);
                Assert.Equal(transaction.TokenId, token.Id);
            }
        }

        [Fact]
        public void AccountGetTransactionsTest()
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

                GetTransactionsSample.AccountGetTransactionSample(payer, transfer);
                Transaction transaction = GetTransactionsSample.AccountGetTransactionSample(payer, transfer);

                Assert.Equal(transaction.TokenId, token.Id);
            }

        }
    }
}
