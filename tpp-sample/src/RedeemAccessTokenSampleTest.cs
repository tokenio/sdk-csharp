using System;
using System.Collections.Generic;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Sample.User;
using Xunit;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp {
    public class RedeemAccessTokenSampleTest {
        [Fact]
        public void RedeemBalanceAccessTokenTest() {
            using(var tokenClient = TestUtil.CreateClient()) {
                UserMember grantor = TestUtil.CreateUserMember();
                string accountId = grantor.GetAccountsBlocking() [0].Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                TppMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateAndEndorseAccessTokenSample.CreateBalanceAccessToken(grantor, accountId, granteeAlias);
                Money balance0 = RedeemAccessTokenSample.RedeemBalanceAccessToken(grantee, token.Id);

                Assert.True(Convert.ToDecimal(balance0.Value) > (decimal.One * 10));
            }
        }

        [Fact]
        public void RedeemTransactionsAccessTokenTest() {
            using(var tokenClient = TestUtil.CreateClient()) {
                UserMember grantor = TestUtil.CreateUserMember();
                string accountId = grantor.GetAccountsBlocking() [0].Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                TppMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                // make some transactions
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                var payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");
                for (int i = 0; i < 5; i++) {
                    Token token = CreateTransferTokenSample.CreateTransferToken(
                        grantor,
                        payeeAlias,
                        Key.Types.Level.Standard);
                    RedeemTransferTokenSample.RedeemTransferToken(
                        payee,
                        payeeAccount.Id(),
                        token.Id);
                }

                Token accessToken = CreateAndEndorseAccessTokenSample.CreateTransactionsAccessToken(grantor, accountId, granteeAlias);
                IList<Transaction> transactions = RedeemAccessTokenSample.RedeemTransactionsAccessToken(grantee, accessToken.Id);

                Assert.Equal(5, transactions.Count);
            }
        }

        [Fact]
        public void RedeemStandingOrdersAccessTokenTest() {
            using(var tokenClient = TestUtil.CreateClient()) {
                UserMember grantor = TestUtil.CreateUserMember();
                string accountId = grantor.GetAccountsBlocking() [0].Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                TppMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                // make some standing orders
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                for (int i = 0; i < 5; i++) {
                    Token token = CreateStandingOrderTokenSample.CreateStandingOrderToken(
                        grantor,
                        payeeAlias,
                        Key.Types.Level.Standard);
                    RedeemStandingOrderTokenSample.RedeemStandingOrderToken(
                        payee,
                        token.Id);
                }

                Token accessToken = CreateAndEndorseAccessTokenSample.CreateStandingOrdersAccessToken(grantor, accountId, granteeAlias);
                IList<StandingOrder> standingOrders = RedeemAccessTokenSample.RedeemStandingOrdersAccessToken(grantee, accessToken.Id);

                Assert.Equal(5, standingOrders.Count);
            }
        }
    }
}
