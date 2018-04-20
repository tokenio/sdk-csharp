using System;
using System.Linq;
using System.Web;
using Io.Token.Proto.Common.Token;
using NUnit.Framework;
using sdk;
using sdk.Api;
using static Io.Token.Proto.Common.Security.Key.Types.Level;
using static tests.TestUtil;

namespace tests
{
    [TestFixture]
    public class AccessTokenTest
    {
        private static readonly int TOKEN_LOOKUP_TIMEOUT_MS = 15000;
        private static readonly int TOKEN_LOOKUP_POLL_FREQUENCY_MS = 1000;

        private static readonly TokenIO tokenIO = NewSdkInstance();

        private MemberSync member1;
        private MemberSync member2;

        [SetUp]
        public void Init()
        {
            member1 = tokenIO.CreateMember(Alias());
            member2 = tokenIO.CreateMember(Alias());
        }

        [Test]
        public void GetAccessToken()
        {
            var address = member1.AddAddress(Util.Nonce(), Address());
            var payload = AccessTokenBuilder.Create(member2.FirstAlias())
                .ForAddress(address.Id)
                .Build();
            var accessToken = member1.CreateAccessToken(payload);
            var result = member1.GetToken(accessToken.Id);
            Assert.AreEqual(accessToken, result);
        }

        [Test]
        public void GetAccessTokens()
        {
            var address = member1.AddAddress(Util.Nonce(), Address());
            var payload = AccessTokenBuilder.Create(member2.FirstAlias())
                .ForAddress(address.Id)
                .Build();
            var accessToken = member1.CreateAccessToken(payload);
            member1.EndorseToken(accessToken, Standard);

            WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS, () =>
            {
                var result = member1.GetAccessTokens(null, 2);
                Assert.True(result.List.Select(t => t.Id.Equals(accessToken.Id)).Any());
            }).Wait();
        }

        [Test]
        public void OnlyOneAccessTokenAllowed()
        {
            var address = member1.AddAddress(Util.Nonce(), Address());

            member1.CreateAccessToken(AccessTokenBuilder
                .Create(member1.FirstAlias())
                .ForAddress(address.Id)
                .Build());

            Assert.Throws<AggregateException>(() => member1.CreateAccessToken(AccessTokenBuilder
                .Create(member1.FirstAlias())
                .ForAddress(address.Id)
                .Build()));
        }

        [Test]
        public void CreateAccessTokenIdempotent()
        {
            var address = member1.AddAddress(Util.Nonce(), Address());

            var accessToken = AccessTokenBuilder
                .Create(member1.FirstAlias())
                .ForAddress(address.Id)
                .Build();

            member1.EndorseToken(member1.CreateAccessToken(accessToken), Standard);
            member1.EndorseToken(member1.CreateAccessToken(accessToken), Standard);

            WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS,
                    () => { Assert.AreEqual(member1.GetAccessTokens(null, 2).List.Count, 1); })
                .Wait();
        }

        [Test, RequiresThread]
        public void CreateAddressAccessToken()
        {
            var address1 = member1.AddAddress(Util.Nonce(), Address());
            var address2 = member1.AddAddress(Util.Nonce(), Address());
            var accessToken = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAddress(address1.Id)
                .Build());
            member1.EndorseToken(accessToken, Standard);
            member2.UseAccessToken(accessToken.Id);

            Assert.AreEqual(address1, member2.GetAddress(address1.Id));
            Assert.Throws<AggregateException>(() => member2.GetAddress(address2.Id));
        }

        [Test, RequiresThread]
        public void CreateAddressesAccessToken()
        {
            var accessToken = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAllAddresses()
                .Build());
            member1.EndorseToken(accessToken, Standard);
            var address1 = member1.AddAddress(Util.Nonce(), Address());
            var address2 = member1.AddAddress(Util.Nonce(), Address());
            
            member2.UseAccessToken(accessToken.Id);
            var result = member2.GetAddress(address2.Id);

            Assert.AreEqual(result, address2);
            Assert.AreNotEqual(result, address1);
        }

        [Test]
        public void GetAccessTokenId()
        {
            var address = member1.AddAddress(Util.Nonce(), Address());
            var payload = AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAddress(address.Id)
                .Build();

            var tokenRequestId = member2.StoreTokenRequest(new TokenRequest {Payload = payload});
            var accessToken = member1.CreateAccessToken(payload, tokenRequestId);

            member1.EndorseToken(accessToken, Standard);

            WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS, () =>
            {
                var tokenId = tokenIO.GetTokenId(tokenRequestId);
                Assert.AreEqual(accessToken.Id, tokenId);
            }).Wait();
        }

        [Test]
        public void AuthFlowTest()
        {
            var accessToken = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAll()
                .Build());
            var token = member1.GetToken(accessToken.Id);

            var requestId = Util.Nonce();
            var originalState = Util.Nonce();
            var csrfToken = Util.Nonce();

            var tokenRequestUrl = tokenIO.GenerateTokenRequestUrl(
                requestId,
                originalState,
                csrfToken);

            var stateParameter = HttpUtility
                .ParseQueryString(Util.GetQueryString(tokenRequestUrl))
                .Get("state");

            var signature = member1.RequestSignature(
                token.Id,
                stateParameter);

            var path = $"path?token-id={token.Id}&state={stateParameter}&signature={Util.ToJson(signature)}";

            var tokenRequestCallbackUrl = "http://localhost:80/" + path;

            var callback = tokenIO.ParseTokenRequestCallbackUrl(
                tokenRequestCallbackUrl,
                csrfToken);

            Assert.AreEqual(originalState, callback.State);
        }
    }
}
