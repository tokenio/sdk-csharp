using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Linq;
using Grpc.Core;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.TokenRequests;
using Tokenio.Tpp.TokenRequests;
using Tokenio.User;
using Tokenio.Utils;
using TokenioTest.Common;
using Xunit;
using Tokenio;
using Tokenio.Exceptions;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Common.TokenProtos.TokenOperationResult.Types;
using Account = Tokenio.Account;
using Member = Tokenio.User.Member;
using ProtoToken = Tokenio.Proto.Common.TokenProtos.TokenRequest;

namespace TokenioTest
{
    public abstract class AccessTokenTestBase : IDisposable
    {
        internal static readonly int TOKEN_LOOKUP_TIMEOUT_MS = 15000;
        internal static readonly int TOKEN_LOOKUP_POLL_FREQUENCY_MS = 1000;
        public TokenUserRule rule = new TokenUserRule();
        public TokenTppRule tppRule = new TokenTppRule();

        internal Tokenio.Tpp.Member member1;
        internal Member member2;
        internal LinkedAccount grantorAccount;
        internal Tokenio.Tpp.Account granteeAccount;

        protected AccessTokenTestBase()
        {
            this.member1 = tppRule.Member();
            this.grantorAccount = rule.LinkedAccount();
            this.member2 = grantorAccount.GetMember();
            this.granteeAccount = member1.CreateTestBankAccountBlocking(1000000, "USD");
        }

        public void Dispose()
        {

        }
    }

    public class AccessTokenTest : AccessTokenTestBase
    {
        [Fact]
        public void GetAccessToken()
        {

            AccessTokenBuilder accessTokenBuilder = AccessTokenBuilder
                                .Create(member1.MemberId())
                                .ForAccount(grantorAccount.GetId());
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                .Create(member1.MemberId())
                                .ForAccount(grantorAccount.GetId()));
            Token result = member1.GetTokenBlocking(accessToken.Id);
            Assert.Equal(result, accessToken);
        }

        [Fact]
        public void GetAccessTokens()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                                .Create(member1.MemberId())
                                                .ForAccount(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Polling.WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS, () => {
                PagedList<Token> result = member1.GetAccessTokensBlocking(null, 2);
                Assert.Contains(result.List.First().Id, accessToken.Id);
            });
        }

            [Fact]
        public void OnlyOneAccessTokenAllowed()
        {
            member2.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(member1.GetFirstAliasBlocking())
                .ForAccount(grantorAccount.GetId()));
            Assert.Throws<AggregateException>(() =>
            member2.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(member1.GetFirstAliasBlocking())
                .ForAccount(grantorAccount.GetId()))
                );
        }

        [Fact]
        public void CreateAccessTokenIdempotent()
        {
            AccessTokenBuilder builder = AccessTokenBuilder
                                            .Create(member1.MemberId())
                                            .ForAccount(grantorAccount.GetId());

            member2.EndorseTokenBlocking(member2.CreateAccessTokenBlocking(builder), Level.Standard);
            member2.EndorseTokenBlocking(member2.CreateAccessTokenBlocking(builder), Level.Standard);

            Polling.WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS, () => {
                Assert.Equal(1, member1.GetAccessTokensBlocking(null, 2).List.Count);
            });
        }

        [Fact]
        public void AccountAccessToken_failNonEndorsed()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                                    .Create(member1.MemberId())
                                                    .ForAccount(grantorAccount.GetId()));

            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id);
            Assert.Throws<AggregateException>(() => representable.GetAccountBlocking(grantorAccount.GetId()));

        }

        [Fact]
        public void AccountAccessToken()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                                    .Create(member1.MemberId())
                                                    .ForAccount(grantorAccount.GetId()));
            TokenOperationResult res = member2.EndorseTokenBlocking(accessToken, Level.Standard);
            Assert.Equal(res.Status, TokenOperationResult.Types.Status.Success);
            Assert.Throws<AggregateException>(() => member1.GetAccountBlocking(grantorAccount.GetId()));

            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id);
            Account result = representable.GetAccountBlocking(grantorAccount.GetId());

            Assert.Equal(result.GetAccount(), grantorAccount.GetAccount().GetAccount());
            Assert.Throws<AggregateException>(() => member1.GetAccountBlocking(grantorAccount.GetId()));
        }

        [Fact]
        public void AccountAccessToken_canceled()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(member1.MemberId())
                .ForAccount(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id);
            Account result = representable.GetAccountBlocking(grantorAccount.GetId());

            Assert.Equal(result.Id(), grantorAccount.GetId());

            TokenOperationResult res = member2.CancelTokenBlocking(accessToken);
            Assert.Equal(res.Status, TokenOperationResult.Types.Status.Success);

            Assert.Throws<AggregateException>(() => representable.GetAccountBlocking(grantorAccount.GetId()));
        }

        [Fact]
        public void AccountAccessToken_canceled_transient()
        {
            Member transientMember = rule.Member(Tokenio.Proto.Common.MemberProtos.CreateMemberType.Transient);
            Account transientMemberAccount = transientMember.CreateTestBankAccountBlocking(100, "USD");
            Token accessToken = transientMember.CreateAccessTokenBlocking(AccessTokenBuilder
                                                       .Create(member1.MemberId())
                                                       .ForAccount(transientMemberAccount.Id()));
            transientMember.EndorseTokenBlocking(accessToken, Level.Standard);

            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id);
            Account result = representable.GetAccountBlocking(transientMemberAccount.Id());
            Assert.Equal(result.GetAccount(), transientMemberAccount.GetAccount());

            TokenOperationResult res = member1.CancelTokenBlocking(accessToken);
            Assert.Equal(res.Status, TokenOperationResult.Types.Status.Success);

            Assert.Throws<AggregateException>(() => representable.GetAccountBlocking(transientMemberAccount.Id()));
            Assert.Throws<AggregateException>(() => transientMember.GetFirstAliasBlocking());

        }

        [Fact]
        public void CreateAccountsAccessToken_getAccounts()
        {
            LinkedAccount account1 = rule.LinkedAccount(member2);
            LinkedAccount account2 = rule.LinkedAccount(member2);

            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                            .Create(member1.GetFirstAliasBlocking())
                                            .ForAccount(account1.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);
            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id);
            IList<Tokenio.Tpp.Account> result = representable.GetAccountsBlocking();
            var list = result.Select(acc => acc.GetAccount()).ToList();

            Assert.Contains(account1.GetAccount().GetAccount(), list);
        }

        [Fact]
        public void CreateAccountBalancesAccessToken()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                            .Create(member1.GetFirstAliasBlocking())
                                            .ForAccount(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Assert.Throws<AggregateException>(() => member1.GetAccountBlocking(grantorAccount.GetId()));
            IList<string> list = new List<string> { grantorAccount.GetId(), "invalidId" };
            Assert.Throws<AggregateException>(() => member1.GetBalancesBlocking(list, Level.Standard));
            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id);

            Money balance = representable.GetBalanceBlocking(grantorAccount.GetId(), Level.Standard).Current;
            Assert.Equal(balance, grantorAccount.GetAccount().GetCurrentBalanceBlocking(Level.Standard));

        }

        [Fact]
        public void CreateAccountBalancesAccessToken_getBalances()
        {
            TokenUserRule goldBank = new TokenUserRule("gold");
            LinkedAccount goldAccount = goldBank.LinkedAccount(member2);

            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                            .Create(member1.MemberId())
                                            .ForAccountBalances(goldAccount.GetId())
                                            .ForAccountBalances(grantorAccount.GetId()));

            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id, true);
            IList<string> requestIds = new List<string> { goldAccount.GetId(), grantorAccount.GetId() };
            Assert.Contains(grantorAccount.GetAccount().GetBalanceBlocking(Level.Standard), representable.GetBalancesBlocking(requestIds, Level.Standard));
            Assert.Contains(goldAccount.GetAccount().GetBalanceBlocking(Level.Standard), representable.GetBalancesBlocking(requestIds, Level.Standard));
        }

        [Fact]
        public void CreateAccountBalancesAccessToken_getUnauthorizedBalance()
        {
            TokenUserRule goldBank = new TokenUserRule("gold");
            LinkedAccount goldAccount = goldBank.LinkedAccount(member2);

            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                                            .Create(member1.MemberId())
                                                            .ForAccountBalances(goldAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id, true);

            IList<string> requestIds = new List<string> { goldAccount.GetId(), grantorAccount.GetId() };
            Assert.Throws<AggregateException>(() => representable.GetBalancesBlocking(requestIds, Level.Standard));
        }

        [Fact]
        public void CreateAccountTransactionsAccessToken()
        {
            Transaction transaction = GetTransaction(grantorAccount, granteeAccount, member1);
            Assert.Throws<AggregateException>(() => member1.GetTransactionBlocking(grantorAccount.GetId(),
                                                                transaction.Id,
                                                                Level.Standard));

            Member payer = grantorAccount.GetMember();
            Token accessToken = payer.CreateAccessTokenBlocking(AccessTokenBuilder
                                                .Create(member1.MemberId())
                                                .ForAccountTransactions(grantorAccount.GetId()));

            payer.EndorseTokenBlocking(accessToken, Level.Standard);

            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id, true);
            Transaction result = representable.GetTransactionBlocking(grantorAccount.GetId(),
                                                    transaction.Id,
                                                    Level.Standard);

            Assert.Equal(result, transaction);
        }

        //[Fact]
        //public void AccountAccess_getTransactionsPaged()
        //{
        //    Member payer = grantorAccount.GetMember();
        //    Token accessToken = payer.CreateAccessTokenBlocking(AccessTokenBuilder
        //                                        .Create(member1.MemberId())
        //                                        .ForAccountTransactions(grantorAccount.GetId()));
        //    payer.EndorseTokenBlocking(accessToken, Level.Standard);

        //    int num = 10;
        //    for( int i=0; i < num; i++)
        //    {
        //        GetTransaction(grantorAccount, granteeAccount, member1);
        //    }

        //    int limit = 2;
        //    IImmutableSet<Transaction> builder = IImmutableSet<Transaction>();


        //}


        [Fact]
        public void ReplaceAccessToken()
        {
            LinkedAccount account = rule.LinkedAccount(grantorAccount);
            Member accountMember = account.GetMember();
            Token accessToken = accountMember.CreateAccessTokenBlocking(AccessTokenBuilder
                                                    .Create(member1.MemberId())
                                                    .ForAccount(account.GetId())
                                                    .ForAccountBalances(account.GetId()));
            accountMember.EndorseTokenBlocking(accessToken, Level.Standard);
            Tokenio.Tpp.IRepresentable representable = member1.ForAccessToken(accessToken.Id, true);
            Money balance = representable.GetBalanceBlocking(account.GetId(), Level.Standard).Current;

            Assert.Equal(balance, account.GetAccount().GetCurrentBalanceBlocking(Level.Standard));

            AccessTokenBuilder builder = AccessTokenBuilder.FromPayload(accessToken.Payload)
                                                .ForAccount(account.GetId())
                                                .ForAccountBalances(account.GetId());

            TokenOperationResult result = accountMember.ReplaceAccessTokenBlocking(
                                                                accessToken,
                                                                builder);

            Assert.Equal(result.Status, TokenOperationResult.Types.Status.MoreSignaturesNeeded);
            Assert.True(result.Token.PayloadSignatures.Any());
            Assert.NotEqual(accessToken.Payload.RefId, result.Token.Payload.RefId);

        }


        [Fact]
        public void GetAccessTokenRequestResult()
        {
            var toMember = new TokenMember
            {
                Id = member1.MemberId()
            };
            Tokenio.TokenRequests.TokenRequest tokenRequest = Testing.Sample.Sample.AccessTokenRequest(
                                                toMember,
                                                "https://token.io",
                                                RandomNumeric(15),
                                                null,
                                                null,
                                                null);

            string tokenRequestId = member1.StoreTokenRequestBlocking(tokenRequest);

            Token accessToken = member2.CreateAccessTokenBlocking(
                                    AccessTokenBuilder.FromTokenRequest(
                                        new ProtoToken
                                        {
                                            RequestPayload = tokenRequest.GetTokenRequestPayload(),
                                            RequestOptions = tokenRequest.GetTokenRequestOptions(),
                                            Id = tokenRequestId
                                        })
                                        .ForAccount(grantorAccount.GetId())
                                        .ForAccountBalances(grantorAccount.GetId()));

            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            var ex = Assert.Throws<AggregateException>(() => tppRule.Token().GetTokenRequestResultBlocking(tokenRequestId));

            RpcException Exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.NotFound, Exception.StatusCode);

            member2.SignTokenRequestStateBlocking(tokenRequestId, accessToken.Id, RandomNumeric(15));

            TokenRequestResult result = tppRule.Token().GetTokenRequestResultBlocking(tokenRequestId);

            Assert.Equal(result.TokenId, accessToken.Id);
            Assert.NotEmpty(result.Signature.Signature_);

        }


        [Fact]
        public void AuthFlowTest()
        {
            try
            {
                Token token = GetAccountAccessToken();
                string requestId = RandomNumeric(15);
                string originalState = RandomNumeric(15);

                string csrfToken = Util.Nonce();
                string tokenRequestUrl = tppRule.Token().GenerateTokenRequestUrlBlocking(
                        requestId,
                        originalState,
                        csrfToken);

                string encodedStateParameter = ExtractStateParameter(new Uri(tokenRequestUrl));

                Signature signature = member2.SignTokenRequestStateBlocking(
                        requestId,
                        token.Id,
                        encodedStateParameter);

                string path = string.Format(
                        "path?tokenId={0}&state={1}&signature={2}",
                        WebUtility.UrlEncode(token.Id),
                        encodedStateParameter,
                        WebUtility.UrlEncode(Util.ToJson(signature)));

                string tokenRequestCallbackUrl = "http://localhost:80/" + path;

                TokenRequestCallback callback = tppRule.Token().ParseTokenRequestCallbackUrlBlocking(
                        tokenRequestCallbackUrl,
                        csrfToken);

                Assert.Equal(callback.State, originalState);
            }
            catch (AggregateException e)
            {
                throw e;
            }
        }


        [Fact]
        public void AuthFlowTest_missingParameter()
        {
            string path = string.Format(
                    "path?state={0}&signature={1}",
                    RandomNumeric(15),
                    RandomNumeric(15));
            string callbackUrlA = "http://localhost:80/" + path;

            Assert.Throws<AggregateException>(() => tppRule.Token().ParseTokenRequestCallbackUrlBlocking(
                                                callbackUrlA,
                                                RandomNumeric(15)));


            path = string.Format(
                "path?tokenId={0}&signature={1}",
                RandomNumeric(15),
                RandomNumeric(15));

            string callbackUrlB = "http://localhost:80/" + path;

            Assert.Throws<AggregateException>(() => tppRule.Token().ParseTokenRequestCallbackUrlBlocking(
                                                                        callbackUrlB,
                                                                        RandomNumeric(15)));

            path = string.Format(
                        "path?tokenId={0}&signature={1}",
                        RandomNumeric(15),
                        RandomNumeric(15));

            string callbackUrlC = "http://localhost:80/" + path;

            Assert.Throws<AggregateException>(() => tppRule.Token().ParseTokenRequestCallbackUrlBlocking(
                                                                        callbackUrlC,
                                                                        RandomNumeric(15)));

        }


        [Fact]
        public void AuthFlowTest_invalidNonce()
        {
            try
            {
                Token token = GetAccountAccessToken();
                string requestId = RandomNumeric(15);
                string originalState = RandomNumeric(15);
                string csrfToken = Util.Nonce();

                string tokenRequestUrl = tppRule.Token().GenerateTokenRequestUrlBlocking(
                        requestId,
                        originalState,
                        csrfToken);

                string encodedStateParameter = ExtractStateParameter(new Uri(tokenRequestUrl));

                Signature signature = member2.SignTokenRequestStateBlocking(
                        requestId,
                        token.Id,
                        encodedStateParameter);

                string path = string.Format(
                        "path?tokenId={0}&state={1}&signature={2}",
                        WebUtility.UrlEncode(token.Id),
                        encodedStateParameter,
                        WebUtility.UrlEncode(Util.ToJson(signature)));

                string callbackUrl = "http://localhost:80/" + path;
                
                Assert.Throws<AggregateException>(() =>
                    tppRule.Token().ParseTokenRequestCallbackUrlBlocking(callbackUrl, RandomNumeric(15)));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Fact]
        public void AuthFlowTest_invalidTokenId()
        {
            try
            {
                Token token = GetAccountAccessToken();
                string requestId = RandomNumeric(15);
                string originalState = RandomNumeric(15);
                string csrfToken = Util.Nonce();

                string tokenRequestUrl = tppRule.Token().GenerateTokenRequestUrlBlocking(
                        requestId,
                        originalState,
                        csrfToken);

                string encodedStateParameter = ExtractStateParameter(new Uri(tokenRequestUrl));

                Signature signature = member2.SignTokenRequestStateBlocking(
                        requestId,
                        token.Id,
                        encodedStateParameter);

                string path = string.Format(
                        "path?tokenId={0}&state={1}&signature={2}",
                        WebUtility.UrlEncode(RandomNumeric(15)),
                        encodedStateParameter,
                        WebUtility.UrlEncode(Util.ToJson(signature)));

                string tokenRequestCallbackUrl = "http://localhost:80/" + path;
                TokenRequestCallback callback =null;
                Assert.Throws<AggregateException>(() => {
                    //callback = tppRule.Token().ParseTokenRequestCallbackUrlBlocking(
                    //            tokenRequestCallbackUrl,
                    //            csrfToken);
                    Assert.Equal(callback.State, originalState);

                });

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Fact]
        public void AuthFlowTest_invalidSignature()
        {
            try
            {
                Token token = GetAccountAccessToken();
                string requestId = RandomNumeric(15);
                string originalState = RandomNumeric(15);
                string csrfToken = Util.Nonce();

                string tokenRequestUrl = tppRule.Token().GenerateTokenRequestUrlBlocking(
                        requestId,
                        originalState,
                        csrfToken);

                string encodedStateParameter = ExtractStateParameter(new Uri(tokenRequestUrl));

                Signature signature = member2.SignTokenRequestStateBlocking(
                        requestId,
                        token.Id,
                        encodedStateParameter);
                signature.KeyId = RandomNumeric(15);

                string path = string.Format(
                        "path?tokenId={0}&state={1}&signature={2}",
                        WebUtility.UrlEncode(token.Id),
                        encodedStateParameter,
                        WebUtility.UrlEncode(Util.ToJson(signature)));

                string callbackUrl = "http://localhost:80/" + path;
                Assert.Throws<AggregateException>(() =>tppRule.Token().ParseTokenRequestCallbackUrlBlocking(callbackUrl, csrfToken));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Fact]
        public void AuthFlowTest_keyNotFound()
        {
            try
            {
                Token token = GetAccountAccessToken();
                string requestId = RandomNumeric(15);
                string originalState = RandomNumeric(15);
                string csrfToken = Util.Nonce();

                string tokenRequestUrl = tppRule.Token().GenerateTokenRequestUrlBlocking(
                        requestId,
                        originalState,
                        csrfToken);

                string encodedStateParameter = ExtractStateParameter(new Uri(tokenRequestUrl));

                Signature signature = member2.SignTokenRequestStateBlocking(
                        requestId,
                        token.Id,
                        encodedStateParameter);

                signature.KeyId = RandomNumeric(15);

                string path = string.Format(
                        "path?tokenId={0}&state={1}&signature={2}",
                        WebUtility.UrlEncode(token.Id),
                        encodedStateParameter,
                        WebUtility.UrlEncode(Util.ToJson(signature)));

                string callbackUrl = "http://localhost:80/" + path;
                Assert.Throws<AggregateException>(() => tppRule.Token().ParseTokenRequestCallbackUrlBlocking(callbackUrl, csrfToken));

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [Fact]
        public void SignTokenRequestState()
        {
            Token token = GetAccountAccessToken();
            Signature signature = member2.SignTokenRequestStateBlocking(
                                                RandomNumeric(15),
                                                token.Id,
                                                RandomNumeric(15));
            Assert.NotEmpty(signature.Signature_);

        }

        [Fact]
        public void SignTokenRequestState_invalidTokenId()
        {
            Assert.Throws<AggregateException>(() => member2.SignTokenRequestStateBlocking(RandomNumeric(15), RandomNumeric(15), RandomNumeric(15)));
        }

        [Fact]
        public void SignTokenRequestState_notFromCorrectMember()
        {
            Token token = GetAccountAccessToken();
            Assert.Throws<AggregateException>(() => rule.Member().SignTokenRequestStateBlocking(RandomNumeric(15), token.Id, RandomNumeric(15)));
        }

        [Fact]
        public void CreateAccessToken_transferDestinations()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                                    .Create(member1.MemberId())
                                                    .ForTransferDestinations(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            IList<TransferDestination> destinations = member1.ForAccessToken(accessToken.Id)
                                                   .ResolveTransferDestinationsBlocking(grantorAccount.GetId());

            Assert.NotEmpty(destinations);
            Assert.True(member2.ResolveTransferDestinationsBlocking(grantorAccount.GetId()).SequenceEqual(destinations));
        }

        //[Fact]
        //public void CreateAccessToken_fundsConfirmation()
        //{
        //    Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
        //                                        .Create(member1.MemberId())
        //                                        .ForFundsConfirmation(grantorAccount.GetId()));
        //    member2.EndorseTokenBlocking(accessToken, Level.Standard);

        //    Money bal = member2.GetBalanceBlocking(grantorAccount.GetId(), Level.Low).Available;

        //    Assert.True(member1.ForAccessToken(accessToken.Id).);
        //}

        //[Fact]
        //public void CreateAccessToken_fundsConfirmation_invalidCurrency()
        //{

        //}

        //[Fact]
        //public void CreateAccessToken_fundsConfirmation_failNonEndorsed()
        //{

        //}


        //[Fact]
        //public void CreateAccessToken_fundsConfirmation_noToken()
        //{

        //}








        private Transaction GetTransaction(
                                LinkedAccount payerAccount,
                                Tokenio.Tpp.Account payeeAcount,
                                Tokenio.Tpp.Member payee)
        {
            Member payer = payerAccount.GetMember();
            Token token = payerAccount.CreateTransferToken(10, payeeAcount, payee);
            token = payer.EndorseTokenBlocking(token, Level.Standard).Token;
            Transfer transfer = payee.RedeemTokenBlocking(
                                            token,
                                            1.0,
                                            payerAccount.GetCurrency(),
                                            "one");

            return payerAccount.GetTransaction(transfer.TransactionId, Level.Standard);
        }

        private string ExtractStateParameter(Uri url)
        {
            string query = url.Query;
            query = query.Remove(0, 1);
            string[] par = query.Split("&");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (string param in par)
            {
                string name = param.Split("=")[0];
                string value = param.Split("=")[1];
                parameters.Add(name, value);
            }

            return parameters["state"];
        }

        private Token GetAccountAccessToken()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                                                        .Create(member1.MemberId())
                                                        .ForAccount(grantorAccount.GetId()));

            return member1.GetTokenBlocking(accessToken.Id);
        }

        private string RandomNumeric(int size)
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, size);
        }

    }
}
