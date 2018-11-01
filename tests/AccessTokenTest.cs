using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.TokenProtos;
using static Test.TestUtil;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace Test
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
            });
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
                () => { Assert.AreEqual(member1.GetAccessTokens(null, 2).List.Count, 1); });
        }

        [Test]
        public void CreateAddressAccessToken()
        {
            var address1 = member1.AddAddress(Util.Nonce(), Address());
            var address2 = member1.AddAddress(Util.Nonce(), Address());
            var accessToken = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAddress(address1.Id)
                .Build());
            member1.EndorseToken(accessToken, Standard);
            var representable = member2.ForAccessToken(accessToken.Id);

            Assert.AreEqual(address1, representable.GetAddress(address1.Id));
            Assert.Throws<AggregateException>(() => representable.GetAddress(address2.Id));
        }

        [Test]
        public void CreateAddressesAccessToken()
        {
            var accessToken = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAllAddresses()
                .Build());
            member1.EndorseToken(accessToken, Standard);
            var address1 = member1.AddAddress(Util.Nonce(), Address());
            var address2 = member1.AddAddress(Util.Nonce(), Address());

            var representable = member2.ForAccessToken(accessToken.Id);
            var result = representable.GetAddress(address2.Id);

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

            var signature = member1.SignTokenRequestState(tokenRequestId, accessToken.Id, Util.Nonce());
            Assert.IsNotEmpty(signature.Signature_);

            var result = tokenIO.GetTokenRequestResult(tokenRequestId);
            Assert.AreEqual(accessToken.Id, result.TokenId);
            Assert.AreEqual(signature.Signature_, result.Signature.Signature_);
        }

        [Test]
        public void UseAccessTokenConcurrently()
        {
            var address1 = member1.AddAddress(Util.Nonce(), Address());
            var address2 = member2.AddAddress(Util.Nonce(), Address());
            
            var user = tokenIO.CreateMember(Alias());
            var accessToken1 = member1.CreateAccessToken(AccessTokenBuilder
                .Create(user.FirstAlias())
                .ForAddress(address1.Id)
                .Build());
            var accessToken2 = member2.CreateAccessToken(AccessTokenBuilder
                .Create(user.FirstAlias())
                .ForAddress(address2.Id)
                .Build());

            member1.EndorseToken(accessToken1, Standard);
            member2.EndorseToken(accessToken2, Standard);
            
            var representable1 = user.Async().ForAccessToken(accessToken1.Id);
            var representable2 = user.Async().ForAccessToken(accessToken2.Id);

            Task<AddressRecord> t1 = representable1.GetAddress(address1.Id);
            Task<AddressRecord> t2 = representable2.GetAddress(address2.Id);
            Task<AddressRecord> t3 = representable1.GetAddress(address1.Id);
            Task.WhenAll(t1, t2, t3).Wait();
            
            Assert.AreEqual(t1.Result, address1);
            Assert.AreEqual(t2.Result, address2);
            Assert.AreEqual(t3.Result, address1);
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

            var signature = member1.SignTokenRequestState(
                requestId,
                token.Id,
                WebUtility.UrlEncode(stateParameter));

            var path = string.Format(
                "path?tokenId={0}&state={1}&signature={2}",
                token.Id,
                WebUtility.UrlEncode(stateParameter),
                Util.ToJson(signature));

            var tokenRequestCallbackUrl = "http://localhost:80/" + path;

            var callback = tokenIO.ParseTokenRequestCallbackUrl(
                tokenRequestCallbackUrl,
                csrfToken);

            Assert.AreEqual(originalState, callback.State);
        }

        [Test]
        public void RequestSignature()
        {
            var token = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAll()
                .Build());
            var signature = member1.SignTokenRequestState(Util.Nonce(), token.Id, Util.Nonce());
            Assert.IsNotEmpty(signature.Signature_);
        }

        [Test]
        public void AccessTokenBuilderSetTransferDestinations()
        {
            var payload = AccessTokenBuilder.Create(member2.FirstAlias())
                .ForAllTransferDestinations()
                .Build();
            var accessToken = member1.CreateAccessToken(payload);
            var result = member1.GetToken(accessToken.Id);
            Assert.AreEqual(accessToken, result);
        }
    }
}
