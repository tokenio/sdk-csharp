﻿using System;
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
            var visibleAccount = member1.CreateAndLinkTestBankAccount(visibleBalance);
            var hiddenAccount = member1.CreateAndLinkTestBankAccount(hiddenBalance);
            var accessToken = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAccount(visibleAccount.Id)
                .ForAccountBalances(visibleAccount.Id)
                .Build());
            member1.EndorseToken(accessToken, Standard);
            var representable = member2.ForAccessToken(accessToken.Id);

            var balanceResult = representable.GetBalance(visibleAccount.Id, Standard).Current;
            Assert.AreEqual(Convert.ToDouble(visibleBalance.Value), Convert.ToDouble(balanceResult.Value));
            Assert.AreEqual(visibleBalance.Currency, balanceResult.Currency);
            Assert.Throws<AggregateException>(() => representable.GetBalance(hiddenAccount.Id, Standard));
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

            var tokenRequestId = member2.StoreTokenRequest(payload, options);

            var balance = new Money
            {
                Value = "1000.00",
                Currency = "EUR"
            };
            var account = member1.CreateAndLinkTestBankAccount(balance);
            var accessTokenPayload = AccessTokenBuilder.Create(member2.MemberId())
                .ForAccount(account.Id)
                .To(member2.MemberId())
                .From(member1.MemberId())
                .Build();
            var accessToken = member1.CreateAccessToken(accessTokenPayload, tokenRequestId);

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
            var balance = new Money
            {
                Value = "100.00",
                Currency = "EUR"
            };
            var account = member1.CreateAndLinkTestBankAccount(balance);
            var accessToken = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAccount(account.Id)
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
            var balance = new Money
            {
                Value = "100.00",
                Currency = "EUR"
            };
            var account = member1.CreateAndLinkTestBankAccount(balance);
            var token = member1.CreateAccessToken(AccessTokenBuilder
                .Create(member2.FirstAlias())
                .ForAccount(account.Id)
                .Build());
            var signature = member1.SignTokenRequestState(Util.Nonce(), token.Id, Util.Nonce());
            Assert.IsNotEmpty(signature.Signature_);
        }
    }
}
