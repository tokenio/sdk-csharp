using System;
using Xunit;
using TokenioTest.Common;
using Tokenio.Tpp;
using Grpc.Core;
using UserMember = Tokenio.User.Member;
using Tokenio.Proto.Common.TokenProtos;
using AccessTokenBuilder =  Tokenio.User.AccessTokenBuilder;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using Tokenio.Security;


namespace TokenioTest
{
    public abstract class FeautureCodeTestBase : IDisposable
    {
        public TokenUserRule rule = new TokenUserRule();
        public TokenTppRule tppRule = new TokenTppRule();

        protected Member member1; // TPP (grantee) member
        protected UserMember member2; // user (grantor) member
        protected LinkedAccount grantorAccount;
        protected Account granteeAccount;

        protected FeautureCodeTestBase()
        {
            this.member1 = tppRule.Member();
            this.grantorAccount = rule.LinkedAccount();
            this.member2 = grantorAccount.GetMember();
            this.granteeAccount = member1.CreateTestBankAccountBlocking(1000000, "USD");
        }

        public void Dispose()
        {
            // Do "global" teardown here; Called after every test method.
        }
    }

    public class FeatureCodeTest : FeautureCodeTestBase
    {
        [Fact]
        public void TriggerException_Unknown()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                    .Create(member1.MemberId())
                    .ForAccount(grantorAccount.GetId())
                    .ForAccountBalances(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Member member1WithFeature = ApplyTppFeatureCodes(
                member1.MemberId(),
                tppRule.Token().GetCryptoEngineFactory(),
                "trigger_unknown");

            IRepresentable representable = member1WithFeature.ForAccessToken(accessToken.Id);
            var ex = Assert.Throws<AggregateException>(() =>
                representable.GetBalanceBlocking(grantorAccount.GetId(), Level.Low));

            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.Unknown, exception.StatusCode);
        }

        [Fact]
        public void TriggerException_Unknown_Extra_Codes()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                    .Create(member1.MemberId())
                    .ForAccount(grantorAccount.GetId())
                    .ForAccountBalances(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Member member1WithFeature = ApplyTppFeatureCodes(
                    member1.MemberId(),
                    tppRule.Token().GetCryptoEngineFactory(),
                    "extra_feature1",
                    "trigger_unknown",
                    "extra_feature2");

            IRepresentable representable = member1WithFeature.ForAccessToken(accessToken.Id);

            var ex = Assert.Throws<AggregateException>(() =>
                representable.GetBalanceBlocking(grantorAccount.GetId(), Level.Low));

            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.Unknown, exception.StatusCode);
        }

        [Fact]
        public void TriggerException_Unimplemented()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                    .Create(member1.MemberId())
                    .ForAccount(grantorAccount.GetId())
                    .ForAccountBalances(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Member member1WithFeature = ApplyTppFeatureCodes(
                    member1.MemberId(),
                    tppRule.Token().GetCryptoEngineFactory(),
                    "trigger_unimplemented");

            IRepresentable representable = member1WithFeature.ForAccessToken(accessToken.Id);

            var ex = Assert.Throws<AggregateException>(() =>
                representable.GetBalanceBlocking(grantorAccount.GetId(), Level.Low));

            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.Unimplemented, exception.StatusCode);
        }

        [Fact]
        public void TriggerException_Unauthenticated_Status()
        {
            Token token = grantorAccount
                    .CreateTransferToken(100, granteeAccount, member1);
            Token endorsed = member2.EndorseTokenBlocking(token, Level.Standard).Token;

            Member member1WithFeature = ApplyTppFeatureCodes(
                    member1.MemberId(),
                    tppRule.Token().GetCryptoEngineFactory(),
                    "trigger_unauthenticated");

            var ex = Assert.Throws<AggregateException>(() =>
                member1WithFeature.RedeemTokenBlocking(endorsed).Status);

            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
        }

        [Fact]
        public void TriggerException_Deadline_Exceeded()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                    .Create(member1.MemberId())
                    .ForAccount(grantorAccount.GetId())
                    .ForAccountTransactions(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Member member1WithFeature = ApplyTppFeatureCodes(
                    member1.MemberId(),
                    tppRule.Token().GetCryptoEngineFactory(),
                    "trigger_deadline_exceeded");

            IRepresentable representable = member1WithFeature.ForAccessToken(accessToken.Id);

            var ex = Assert.Throws<AggregateException>(() =>
                representable.GetTransactionsBlocking(grantorAccount.GetId(), 1, Level.Low,null));

            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.DeadlineExceeded, exception.StatusCode);
        }

        [Fact]
        public void TriggerException_Timeout()
        {
            Token accessToken = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                    .Create(member1.MemberId())
                    .ForAccount(grantorAccount.GetId())
                    .ForAccountTransactions(grantorAccount.GetId()));
            member2.EndorseTokenBlocking(accessToken, Level.Standard);

            Member member1WithFeature = ApplyTppFeatureCodes(
                    member1.MemberId(),
                    tppRule.Token().GetCryptoEngineFactory(),
                    "trigger_timeout");

            IRepresentable representable = member1WithFeature.ForAccessToken(accessToken.Id);

            var ex = Assert.Throws<AggregateException>(() =>
                representable.GetTransactionsBlocking(grantorAccount.GetId(), 1, Level.Low, null));

            RpcException exception = (RpcException)ex.InnerException;
            Assert.Equal(StatusCode.DeadlineExceeded, exception.StatusCode);
        }

        private Member ApplyTppFeatureCodes(
                string memberId,
                ICryptoEngineFactory crypto,
                params string[] codes)
        {
            TokenClient newClient = (TokenClient)tppRule.NewSdkInstance(crypto, codes);
            return newClient.GetMemberBlocking(memberId);
        }
    }
}
