using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.TokenProtos;
using static Test.TestUtil;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;
using static Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types;
using static Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types.ResourceType;
using TokenRequestOptions = Tokenio.Proto.Common.TokenProtos.TokenRequestOptions;

namespace Test
{
    [TestFixture]
    public class AccessTokenTest
    {
        private static readonly int TOKEN_LOOKUP_TIMEOUT_MS = 15000;
        private static readonly int TOKEN_LOOKUP_POLL_FREQUENCY_MS = 1000;

        private static readonly TokenClient tokenClient = NewSdkInstance();

        private Tokenio.Member member1;
        private Tokenio.Member member2;

        [SetUp]
        public void Init()
        {
            member1 = tokenClient.CreateMemberBlocking(Alias());
            member2 = tokenClient.CreateMemberBlocking(Alias());
        }

        [Test]
        public void GetAccessToken()
        {
            var address = member1.AddAddressBlocking(Util.Nonce(), Address());
            var payload = AccessTokenBuilder.Create(member2.GetFirstAliasBlocking())
                .ForAddress(address.Id)
                .Build();
            var accessToken = member1.CreateAccessTokenBlocking(payload);
            var result = member1.GetTokenBlocking(accessToken.Id);
            Assert.AreEqual(accessToken, result);
        }

        [Test]
        public void GetAccessTokens()
        {
            var address = member1.AddAddressBlocking(Util.Nonce(), Address());
            var payload = AccessTokenBuilder.Create(member2.GetFirstAliasBlocking())
                .ForAddress(address.Id)
                .Build();
            var accessToken = member1.CreateAccessTokenBlocking(payload);
            member1.EndorseToken(accessToken, Standard);

            WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS, () =>
            {
                var result = member1.GetAccessTokensBlocking(null, 2);
                Assert.True(result.List.Select(t => t.Id.Equals(accessToken.Id)).Any());
            });
        }

        [Test]
        public void OnlyOneAccessTokenAllowed()
        {
            var address = member1.AddAddressBlocking(Util.Nonce(), Address());

            member1.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(member1.GetFirstAliasBlocking())
                .ForAddress(address.Id)
                .Build());

            Assert.Throws<AggregateException>(() => member1.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(member1.GetFirstAliasBlocking())
                .ForAddress(address.Id)
                .Build()));
        }

        [Test]
        public void CreateAccessTokenIdempotent()
        {
            var address = member1.AddAddressBlocking(Util.Nonce(), Address());

            var accessToken = AccessTokenBuilder
                .Create(member1.GetFirstAliasBlocking())
                .ForAddress(address.Id)
                .Build();

            member1.EndorseTokenBlocking(member1.CreateAccessTokenBlocking(accessToken), Standard);
            member1.EndorseTokenBlocking(member1.CreateAccessTokenBlocking(accessToken), Standard);

            WaitUntil(TOKEN_LOOKUP_TIMEOUT_MS, TOKEN_LOOKUP_POLL_FREQUENCY_MS,
                () => { Assert.AreEqual(member1.GetAccessTokensBlocking(null, 2).List.Count, 1); });
        }

        [Test]
        public void CreateBalanceAccessToken()
        {
            var visibleBalance = new Money
            {
                Value = "100.00",
                Currency = "EUR"
            };
            var hiddenBalance = new Money
            {
                Value = "1000.00",
                Currency = "EUR"
            };
            var visibleAccount = member1.CreateAndLinkTestBankAccountBlocking(visibleBalance);
            var hiddenAccount = member1.CreateAndLinkTestBankAccountBlocking(hiddenBalance);
            var accessToken = member1.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(member2.GetFirstAliasBlocking())
                .ForAccount(visibleAccount.Id)
                .ForAccountBalances(visibleAccount.Id)
                .Build());
            member1.EndorseTokenBlocking(accessToken, Standard);
            var representable = member2.ForAccessToken(accessToken.Id);

            var balanceResult = representable.GetBalanceBlocking(visibleAccount.Id, Standard).Current;
            Assert.AreEqual(Convert.ToDouble(visibleBalance.Value), Convert.ToDouble(balanceResult.Value));
            Assert.AreEqual(visibleBalance.Currency, balanceResult.Currency);
            Assert.Throws<AggregateException>(() => representable.GetBalanceBlocking(hiddenAccount.Id, Standard));
        }

        [Test]
        public void GetAccessTokenId()
        {
            IList<ResourceType> types = new List<ResourceType>();
            types.Add(Accounts);

            var payload = new TokenRequestPayload
            {
                UserRefId = Util.Nonce(),
                RedirectUrl = Util.Nonce(),
                To = new TokenMember
                {
                    Id = member2.MemberId()
                },
                Description = Util.Nonce(),
                CallbackState = Util.Nonce(),
                AccessBody = new TokenRequestPayload.Types.AccessBody
                {
                    Type = {types}
                }
            };

            var options = new TokenRequestOptions
            {
                BankId = "iron",
                ReceiptRequested = false
            };

            var tokenRequestId = member2.StoreTokenRequestBlocking(payload, options);

            var balance = new Money
            {
                Value = "1000.00",
                Currency = "EUR"
            };
            var account = member1.CreateAndLinkTestBankAccountBlocking(balance);
            var accessTokenPayload = AccessTokenBuilder.Create(member2.MemberId())
                .ForAccount(account.Id)
                .To(member2.MemberId())
                .From(member1.MemberId())
                .Build();
            var accessToken = member1.CreateAccessTokenBlocking(accessTokenPayload, tokenRequestId);

            member1.EndorseTokenBlocking(accessToken, Standard);

            var signature = member1.SignTokenRequestStateBlocking(tokenRequestId, accessToken.Id, Util.Nonce());
            Assert.IsNotEmpty(signature.Signature_);

            var result = tokenClient.GetTokenRequestResultBlocking(tokenRequestId);
            Assert.AreEqual(accessToken.Id, result.TokenId);
            Assert.AreEqual(signature.Signature_, result.Signature.Signature_);
        }

        [Test]
        public void UseAccessTokenConcurrently()
        {
            var address1 = member1.AddAddressBlocking(Util.Nonce(), Address());
            var address2 = member2.AddAddressBlocking(Util.Nonce(), Address());
            
            var user = tokenClient.CreateMemberBlocking(Alias());
            var accessToken1 = member1.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(user.GetFirstAliasBlocking())
                .ForAddress(address1.Id)
                .Build());
            var accessToken2 = member2.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(user.GetFirstAliasBlocking())
                .ForAddress(address2.Id)
                .Build());

            member1.EndorseTokenBlocking(accessToken1, Standard);
            member2.EndorseTokenBlocking(accessToken2, Standard);
            
            var representable1 = user.ForAccessToken(accessToken1.Id);
            var representable2 = user.ForAccessToken(accessToken2.Id);

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
            var balance = new Money
            {
                Value = "100.00",
                Currency = "EUR"
            };
            var account = member1.CreateAndLinkTestBankAccountBlocking(balance);
            var accessToken = member1.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(member2.GetFirstAliasBlocking())
                .ForAccount(account.Id)
                .Build());
            var token = member1.GetTokenBlocking(accessToken.Id);

            var requestId = Util.Nonce();
            var originalState = Util.Nonce();
            var csrfToken = Util.Nonce();

            var tokenRequestUrl = tokenClient.GenerateTokenRequestUrlBlocking(
                requestId,
                originalState,
                csrfToken);

            var stateParameter = HttpUtility
                .ParseQueryString(Util.GetQueryString(tokenRequestUrl))
                .Get("state");

            var signature = member1.SignTokenRequestStateBlocking(
                requestId,
                token.Id,
                WebUtility.UrlEncode(stateParameter));

            var path = string.Format(
                "path?tokenId={0}&state={1}&signature={2}",
                token.Id,
                WebUtility.UrlEncode(stateParameter),
                Util.ToJson(signature));

            var tokenRequestCallbackUrl = "http://localhost:80/" + path;

            var callback = tokenClient.ParseTokenRequestCallbackUrlBlocking(
                tokenRequestCallbackUrl,
                csrfToken);

            Assert.AreEqual(originalState, callback.State);
        }

        [Test]
        public void RequestSignature()
        {
            var balance = new Money
            {
                Value = "100.00",
                Currency = "EUR"
            };
            var account = member1.CreateAndLinkTestBankAccountBlocking(balance);
            var token = member1.CreateAccessTokenBlocking(AccessTokenBuilder
                .Create(member2.GetFirstAliasBlocking())
                .ForAccount(account.Id)
                .Build());
            var signature = member1.SignTokenRequestStateBlocking(Util.Nonce(), token.Id, Util.Nonce());
            Assert.IsNotEmpty(signature.Signature_);
        }
    }
}
