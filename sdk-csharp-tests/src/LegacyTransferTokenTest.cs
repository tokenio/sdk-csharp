using System;
using TokenioTest.Common;
using Xunit;
using Tokenio.Proto.Common.TokenProtos;
using TokenioTest.Asserts;
using Grpc.Core;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using System.Collections.Generic;
using Tokenio.Proto.Common.MemberProtos;
using Member = Tokenio.User.Member;
using Sample = TokenioTest.Testing.Sample.Sample;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using Tokenio;
using System.Collections.Immutable;

namespace TokenioTest
{

    public abstract class LegacyTransferTokenTestBase : IDisposable
    {
        internal static readonly int TOKEN_LOOKUP_TIMEOUT_MS = 15000;
        internal static readonly int TOKEN_LOOKUP_POLL_FREQUENCY_MS = 1000;

        public TokenUserRule rule = new TokenUserRule();
        public TokenTppRule tppRule = new TokenTppRule();

        internal LinkedAccount payerAccount;
        internal Member payer;
        internal LinkedAccount payeeAccount;
        internal Member payee;

        protected LegacyTransferTokenTestBase()
        {
            this.payerAccount = rule.LinkedAccount();
            this.payer = payerAccount.GetMember();
            this.payeeAccount = rule.LinkedAccount(payerAccount);
            this.payee = payeeAccount.GetMember();
        }

        public void Dispose()
        {

        }

    }

    public class LegacyTransferTokenTest : LegacyTransferTokenTestBase
    {
        [Fact]
        public void CreateTransferToken()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            TokenAssertion.AssertThat(token).HasFrom(payer).HasAmount(100).HasCurrency(payeeAccount.GetCurrency()).HasNoSignatures();

        }

        [Fact]
        public void CreateTransferToken_noDestination()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).SetDescription("book purchase").ExecuteBlocking();
            TokenAssertion.AssertThat(token).HasFrom(payer).HasAmount(100).HasCurrency(payeeAccount.GetCurrency()).HasNoSignatures();
        }

        [Fact]
        public void CreateTransferToken_idempotentRefId()
        {
            string refId = RandomNumeric(15);

            Token token1 = payerAccount.TransferTokenBuilder(100.0, payeeAccount)
                                        .SetRefId(refId)
                                        .SetDescription("book purchase 1")
                                        .ExecuteBlocking();

            Token token2 = payerAccount.TransferTokenBuilder(200.0, payeeAccount)
                                        .SetRefId(refId)
                                        .SetDescription("book purchase 2")
                                        .ExecuteBlocking();

            TokenAssertion.AssertThat(token1).HasFrom(payer).HasAmount(100).HasCurrency(payeeAccount.GetCurrency()).HasNoSignatures();
            Assert.Equal(token1, token2);
        }

        [Fact]
        public void CreateTransferToken_sameRefIdDifferentPayer()
        {
            string refId = RandomNumeric(15);

            Token token1 = payerAccount.TransferTokenBuilder(100.0, payeeAccount)
                                        .SetRefId(refId)
                                        .SetDescription("book purchase 1")
                                        .ExecuteBlocking();

            Token token2 = payeeAccount.TransferTokenBuilder(200.0, payerAccount)
                                        .SetRefId(refId)
                                        .SetDescription("book purchase 2")
                                        .ExecuteBlocking();

            TokenAssertion.AssertThat(token1).HasFrom(payer).HasAmount(100.0).HasCurrency(payeeAccount.GetCurrency()).HasNoSignatures();
            TokenAssertion.AssertThat(token2).HasFrom(payee).HasAmount(200.0).HasCurrency(payerAccount.GetCurrency()).HasNoSignatures();

        }


        [Fact]
        public void CreateTransferTokenWithUnlinkedAccount()
        {
            IList<string> list = new List<string> { payeeAccount.GetId() };
            payer.UnlinkAccountsBlocking(list);
            var ex = Assert.Throws<AggregateException>(() => payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking());

            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
            Assert.Equal("Account not found", exception.Status.Detail);

        }

        [Fact]
        public void CreateTransferToken_invalidAccount_source()
        {
            var ex = Assert.Throws<AggregateException>(() => payerAccount.TransferTokenBuilder(100.0, payeeAccount)
                                                                .SetAccountId(payeeAccount.GetId())
                                                                .SetDescription("book purchase")
                                                                .ExecuteBlocking());
            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.PermissionDenied, exception.StatusCode);
            Assert.Equal("Account not found", exception.Status.Detail);

        }

        [Fact]
        public void GetTransferToken()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            TokenAssertion.AssertThat(payer.GetTokenBlocking(token.Id))
                            .HasFrom(payer)
                            .HasAmount(100.0)
                            .HasCurrency(payeeAccount.GetCurrency())
                            .HasNoSignatures();

        }

        [Fact]
        public void GetTransferTokens()
        {
            Token token1 = payerAccount.TransferTokenBuilder(123.45, payeeAccount).ExecuteBlocking();
            Token token2 = payerAccount.TransferTokenBuilder(678.90, payeeAccount).ExecuteBlocking();
            Token token3 = payerAccount.TransferTokenBuilder(100.99, payeeAccount).ExecuteBlocking();

            payer.EndorseTokenBlocking(token1, Level.Standard);
            payer.EndorseTokenBlocking(token2, Level.Standard);
            payer.EndorseTokenBlocking(token3, Level.Standard);

           
            

            Polling.WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS, 
                        () => {
                            PagedList<Token> res = payer.GetTransferTokensBlocking(null, 10);
                            List<string> list = new List<string> { token1.Id, token2.Id, token3.Id };
                            IList<Token> reslist = res.List;
                            for (int i =0; i<list.Count; i++)
                            {
                                bool temp = list.Contains(reslist[i].Id);
                                Assert.True(temp);
                            }
                            Assert.NotEmpty(res.Offset);
                        });
        }

        [Fact]
        public void GetTransferTokens_payee()
        {
            Token token1 = payerAccount.TransferTokenBuilder(123.45, payeeAccount).ExecuteBlocking();
            Token token2 = payerAccount.TransferTokenBuilder(678.90, payeeAccount).ExecuteBlocking();
            Token token3 = payerAccount.TransferTokenBuilder(100.99, payeeAccount).ExecuteBlocking();

            payer.EndorseTokenBlocking(token1, Level.Standard);
            payer.EndorseTokenBlocking(token2, Level.Standard);
            payer.EndorseTokenBlocking(token3, Level.Standard);


            Polling.WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS,
                                    () => {
                                        PagedList<Token> res = payee.GetTransferTokensBlocking(null, 3);
                                        IList<string> list = new List<string> { token1.Id, token2.Id, token3.Id };
                                        IList<Token> reslist = res.List;
                                        for (int i = 0; i < reslist.Count; i++)
                                        {
                                            Assert.True(list.Contains(reslist[i].Id));
                                        }
                                        Assert.NotEmpty(res.Offset);
                                        Assert.Equal(list.Count, reslist.Count);
                                    });
        }

        [Fact]
        public void GetTransferTokensPaged()
        {
            int num = 10;
            for (int i = 0; i < num; i++)
            {
                Token token = payerAccount.TransferTokenBuilder(100 + i, payeeAccount)
                                                .ExecuteBlocking();
                payer.EndorseTokenBlocking(token, Level.Standard);
            }

            int limit = 2;

            //var builder = ImmutableHashSet.CreateBuilder<Token>();
            //PagedList<Token> res = payer.GetTransferTokensBlocking(null, limit);

            Polling.WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS,
                                                () => {
                                                    //var builder = ImmutableHashSet.CreateBuilder<Token>();
                                                    IList<Token> builder = new List<Token>();
                                                    PagedList<Token> res = payer.GetTransferTokensBlocking(null, limit);
                                                    for (int i = 0; i < num/limit; i++)
                                                    {
                                                        foreach(var a in res.List)
                                                        {
                                                            if (!builder.Contains(a))
                                                            {
                                                                builder.Add(a);
                                                            }
                                                        }
                                                        res = payer.GetTransferTokensBlocking(res.Offset, limit);
                                                    }
                                                    Assert.Equal(num, builder.Count);
                                                });
        }

        [Fact]
        public void EndorseTransferToken()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            TokenOperationResult result = payer.EndorseTokenBlocking(token, Level.Standard);
            Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);

            TokenAssertion.AssertThat(result.Token).HasNSignatures(2)
                                .IsEndorsedBy(payer, Level.Standard)
                                .HasFrom(payer)
                                .HasAmount(100.0)
                                .HasCurrency(payeeAccount.GetCurrency());

        }

        [Fact]
        public void EndorseTransferTokenWithUnlinkedAccount()
        {
            IList<string> list = new List<string> { payerAccount.GetId() };
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            payer.UnlinkAccountsBlocking(list);

            var ex = Assert.Throws<AggregateException>(() => payer.EndorseTokenBlocking(token, Level.Standard));
            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);

        }


        [Fact]
        public void CancelTransferToken()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            TokenOperationResult result = payer.CancelTokenBlocking(token);
            Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);

            TokenAssertion.AssertThat(result.Token).HasNSignatures(2).IsCancelledBy(payer, Level.Low)
                                .HasFrom(payer)
                                .HasAmount(100.0)
                                .HasCurrency(payeeAccount.GetCurrency());

        }

        [Fact]
        public void CancelTransferToken_transientMember()
        {
            LinkedAccount payerAccount = rule.LinkedAccount(rule.Member(CreateMemberType.Transient));
            Member transientPayer = payerAccount.GetMember();
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();

            TokenOperationResult result = payee.CancelTokenBlocking(token);
            Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);

            TokenAssertion.AssertThat(result.Token).HasNSignatures(2).IsCancelledBy(payee, Level.Low)
                                    .HasFrom(transientPayer)
                                    .HasAmount(100.0)
                                    .HasCurrency(payeeAccount.GetCurrency());

            var ex = Assert.Throws<AggregateException>(() => transientPayer.GetFirstAliasBlocking());
            RpcException exception = (RpcException)ex.InnerException;

        }

        [Fact]
        public void EndorseTransferTokenMoreSignaturesNeeded_amountExceeded()
        {
            Token tokenReset = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            payer.EndorseTokenBlocking(tokenReset, Level.Low);
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            TokenOperationResult result = payer.EndorseTokenBlocking(token, Level.Low);

            Assert.Equal(TokenOperationResult.Types.Status.MoreSignaturesNeeded, result.Status);
            TokenAssertion.AssertThat(result.Token).HasNSignatures(1).IsEndorsedBy(payer, Level.Low)
                                            .HasFrom(payer)
                                            .HasAmount(100.0)
                                            .HasCurrency(payeeAccount.GetCurrency());

        }


        [Fact]
        public void EndorseTransferTokenMoreSignaturesNeeded_aggregateAmountExceeded()
        {
            Token tokenReset = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            payer.EndorseTokenBlocking(tokenReset, Level.Standard);

            Token token;
            TokenOperationResult result;
            for (int i = 0; i < 3; ++i)
            {
                token = payerAccount.TransferTokenBuilder(29.0, payeeAccount).ExecuteBlocking();
                result = payer.EndorseTokenBlocking(token, Level.Low);
                Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);
            }

            token = payerAccount.TransferTokenBuilder(29.0, payeeAccount)
                .ExecuteBlocking();
            result = payer.EndorseTokenBlocking(token, Level.Low);
            Assert.Equal(TokenOperationResult.Types.Status.MoreSignaturesNeeded, result.Status);

        }

        [Fact]
        public void EndorseTransferTokenMoreSignaturesNeeded_countExceeded()
        {
            Token tokenReset = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            payer.EndorseTokenBlocking(tokenReset, Level.Standard);
            Token token;
            TokenOperationResult result;
            for (int i = 0; i < 5; ++i)
            {
                token = payerAccount.TransferTokenBuilder(1.0, payeeAccount).ExecuteBlocking();
                result = payer.EndorseTokenBlocking(token, Level.Low);
                Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);

            }

            token = payerAccount.TransferTokenBuilder(1.0, payeeAccount).ExecuteBlocking();
            result = payer.EndorseTokenBlocking(token, Level.Low);

            Assert.Equal(TokenOperationResult.Types.Status.MoreSignaturesNeeded, result.Status);

        }

        [Fact]
        public void EndorseTransferTokenUnicode()
        {
            string description = "\\u263A\\uFE0Fs\\uD83D\\uDC69\\u200D\\uD83D\\uDC69\\u200D"
                    + "\\uD83D\\uDC67\\u200D\\uD83D\\uDC67\\uD83D\\uDC69\\u200D\\uD83D\\uDC69"
                    + "\\u200D\\uD83D\\uDC67\\uD83D\\uDC69\\u200D\\u2764\\uFE0F\\u200D\\uD83D"
                    + "\\uDC8B\\u200D\\uD83D\\uDC69\\uD83D\\uDC70\\uD83C\\uDFFF\\uD83C\\uDDE6"
                    + "\\uD83C\\uDDFC\\uD83C\\uDDE7\\uD83C\\uDDED";
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount)
                    .SetDescription(description)
                    .ExecuteBlocking();
            TokenOperationResult result = payer.EndorseTokenBlocking(token, Level.Standard);
            Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);
            TokenAssertion.AssertThat(result.Token).HasDescription(description);
            TokenAssertion.AssertThat(payee.GetTokenBlocking(token.Id)).HasDescription(description);

        }

        [Fact]
        public void GetTransferTokenRequestResult()
        {
            Tokenio.Tpp.Member payee = tppRule.Member();
            var tokenMember = new Tokenio.Proto.Common.TokenProtos.TokenMember
            {
                Id = payee.MemberId(),

            };

            Tokenio.TokenRequests.TokenRequest tokenRequest = Sample.TransferTokenRequest(tokenMember,
                                            "https://token.io",
                                            "100.00",
                                            "EUR",
                                            RandomNumeric(15),
                                            null,
                                            null,
                                            null,
                                            payerAccount.GetId());

            string tokenRequestId = payee.StoreTokenRequestBlocking(tokenRequest);

            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount)
                                        .SetTokenRequestId(tokenRequestId)
                                        .ExecuteBlocking();

            payerAccount.GetMember().EndorseTokenBlocking(token, Level.Standard);

            var ex = Assert.Throws<AggregateException>(() => rule.Token().GetTokenRequestResultBlocking(tokenRequestId));
            RpcException exception = (RpcException)ex.InnerException;

        }

        [Fact]
        public void SignTokenRequestState()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();

            Signature signature = payer.SignTokenRequestStateBlocking(
                    RandomNumeric(15),
                    token.Id,
                    RandomNumeric(15));
            Assert.NotEmpty(signature.Signature_);
        }


        [Fact]
        public void SignTokenRequestState_invalidTokenId()
        {
            Assert.Throws<AggregateException>(() => payer.SignTokenRequestStateBlocking(RandomNumeric(15), RandomNumeric(15), RandomNumeric(15))); 
        }

        [Fact]
        public void SignTokenRequestState_notFromCorrectMember()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();

            Assert.Throws<AggregateException>(() => payee.SignTokenRequestStateBlocking(RandomNumeric(15), token.Id, RandomNumeric(15)));
        }

        [Fact]
        public void GetTransferTokenWithPayer_lockedOutPayer()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();

            ICryptoEngine cryptoEngine = new TokenCryptoEngine(
                    payerAccount.GetMember().MemberId(),
                    new InMemoryKeyStore());
            string verificationId = rule.Token()
                                .BeginRecoveryBlocking(payerAccount.GetMember().GetFirstAliasBlocking());
            Member recoveredMember = rule.Token().CompleteRecoveryWithDefaultRuleBlocking(
                    payerAccount.GetMember().MemberId(),
                    verificationId,
                    "code");

            var ex = Assert.Throws<AggregateException>(() => recoveredMember.GetTokenBlocking(token.Id));
            RpcException exception = (RpcException)ex.InnerException;

            Assert.Contains("is locked", exception.ToString());
        }   

        [Fact]
        public void GetTransferTokenWithPayee_lockedOutPayer()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();

            string verificationId = rule.Token()
                    .BeginRecoveryBlocking(payerAccount.GetMember().GetFirstAliasBlocking());

            rule.Token().CompleteRecoveryWithDefaultRuleBlocking(
                    payerAccount.GetMember().MemberId(),
                    verificationId,
                    "code");

            TokenAssertion.AssertThat(payee.GetTokenBlocking(token.Id))
                                .HasFrom(payer)
                                .HasAmount(100.0)
                                .HasCurrency(payeeAccount.GetCurrency())
                                .HasNoSignatures();
                                
        }

        [Fact]
        public void GetTransferToken_deletedPayee()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();
            payee.DeleteMemberBlocking();

            TokenAssertion.AssertThat(payer.GetTokenBlocking(token.Id))
                                .HasFrom(payer)
                                .HasAmount(100.0)
                                .HasCurrency(payeeAccount.GetCurrency())
                                .HasNoSignatures();
        }

        [Fact]
        public void GetTransferToken_deletedPayer()
        {
            Token token = payerAccount.TransferTokenBuilder(100.0, payeeAccount).ExecuteBlocking();

            payer.DeleteMemberBlocking();
            TokenAssertion.AssertThat(payee.GetTokenBlocking(token.Id))
                                .HasFrom(payer)
                                .HasAmount(100.0)
                                .HasCurrency(payeeAccount.GetCurrency())
                                .HasNoSignatures();
        }

        private string RandomNumeric(int size)
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, size);
        }
    }
}
