using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tokenio;
using Tokenio.Proto.Common.TokenProtos;
using static Test.TestUtil;
using static Tokenio.Proto.Common.TokenProtos.TokenRequestPayload.Types.AccessBody.Types;

namespace Test
{
    [TestFixture]
    public class TokenRequestTest
    {
        private static readonly string tokenUrl = "https://token.io";
        private static readonly TokenClient tokenClient = NewSdkInstance();

        private Member member;

        [SetUp]
        public void Init()
        {
            member = tokenClient.CreateMemberBlocking(Alias());
        }

        [Test]
        public void AddAndGetTransferTokenRequest()
        {
            var storedPayload = new TokenRequestPayload
            {
                UserRefId = Util.Nonce(),
                RedirectUrl = tokenUrl,
                To = new TokenMember
                {
                    Id = member.MemberId()
                },
                Description = Util.Nonce(),
                CallbackState = Util.Nonce(),
                TransferBody = new TokenRequestPayload.Types.TransferBody
                {
                    Amount = "10.0",
                    Currency = "EUR"
                }
            };

            var storedOptions = new Tokenio.Proto.Common.TokenProtos.TokenRequestOptions
            {
                BankId = "iron",
                ReceiptRequested = false
            };

            var requestId = member.StoreTokenRequestBlocking(storedPayload, storedOptions);
            Assert.IsNotEmpty(requestId);
            var retrievedRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
            Assert.AreEqual(storedPayload, retrievedRequest.RequestPayload);
            Assert.AreEqual(storedOptions, retrievedRequest.RequestOptions);
        }

        [Test]
        public void AddAndGetAccessTokenRequest()
        {
            IList<ResourceType> types = new List<ResourceType>();
            types.Add(ResourceType.Accounts);
            var storedPayload = new TokenRequestPayload
            {
                UserRefId = Util.Nonce(),
                RedirectUrl = Util.Nonce(),
                To = new TokenMember
                {
                    Id = member.MemberId()
                },
                Description = Util.Nonce(),
                CallbackState = Util.Nonce(),
                AccessBody = new TokenRequestPayload.Types.AccessBody
                {
                    Type = {types}
                }
            };

            var storedOptions = new Tokenio.Proto.Common.TokenProtos.TokenRequestOptions
            {
                BankId = "iron",
                ReceiptRequested = false
            };

            var requestId = member.StoreTokenRequestBlocking(storedPayload, storedOptions);
            Assert.IsNotEmpty(requestId);

            var retrievedRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
            Assert.AreEqual(storedPayload, retrievedRequest.RequestPayload);
            Assert.AreEqual(storedOptions, retrievedRequest.RequestOptions);
        }

        
        [Test]
        public void AddAndGetTokenRequest_NotFound()
        {
            Assert.Throws<AggregateException>(() => tokenClient.RetrieveTokenRequestBlocking("bogus"));
            Assert.Throws<AggregateException>(() => tokenClient.RetrieveTokenRequestBlocking(member.MemberId()));
        }
        
        [Test]
        public void AddAndGetTokenRequest_WrongMember()
        {
            var storedPayload = new TokenRequestPayload
            {
                UserRefId = Util.Nonce(),
                RedirectUrl = tokenUrl,
                To = new TokenMember
                {
                    Id = tokenClient.CreateMemberBlocking().MemberId()
                },
                Description = Util.Nonce(),
                CallbackState = Util.Nonce(),
                TransferBody = new TokenRequestPayload.Types.TransferBody
                {
                    Amount = "10.0",
                    Currency = "EUR"
                }
            };
            var storedOptions = new Tokenio.Proto.Common.TokenProtos.TokenRequestOptions
            {
                BankId = "iron",
                ReceiptRequested = false
            };
            Assert.Throws<AggregateException>(() => member.StoreTokenRequestBlocking(storedPayload, storedOptions));
        }

        [Test]
        public void UpdateTokenRequest()
        {
            var storedPayload = new TokenRequestPayload
            {
                UserRefId = Util.Nonce(),
                RedirectUrl = tokenUrl,
                To = new TokenMember
                {
                    Id = member.MemberId()
                },
                Description = Util.Nonce(),
                CallbackState = Util.Nonce(),
                TransferBody = new TokenRequestPayload.Types.TransferBody
                {
                    Amount = "10.0",
                    Currency = "EUR"
                }
            };
            var storedOptions = new Tokenio.Proto.Common.TokenProtos.TokenRequestOptions
            {
                BankId = "iron",
                ReceiptRequested = false
            };
            member.StoreTokenRequestBlocking(storedPayload, storedOptions);
            
            var requestId = member.StoreTokenRequestBlocking(storedPayload, storedOptions);
            Assert.IsNotEmpty(requestId);
            var retrievedRequest1 = tokenClient.RetrieveTokenRequestBlocking(requestId);
            Assert.IsFalse(retrievedRequest1.RequestOptions.ReceiptRequested);
            
            var optionsUpdate = new Tokenio.Proto.Common.TokenProtos.TokenRequestOptions
            {
                ReceiptRequested = true
            };
            
            member.UpdateTokenRequestBlocking(requestId, optionsUpdate);
            var retrievedRequest2 = tokenClient.RetrieveTokenRequestBlocking(requestId);
            Assert.IsTrue(retrievedRequest2.RequestOptions.ReceiptRequested);
        }
    }
}
