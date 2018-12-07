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
        private static readonly TokenIO tokenIO = NewSdkInstance();

        private MemberSync memberSync;

        [SetUp]
        public void Init()
        {
            memberSync = tokenIO.CreateMember(Alias());
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
                    Id = memberSync.MemberId()
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

            var requestId = memberSync.StoreTokenRequest(storedPayload, storedOptions);
            Assert.IsNotEmpty(requestId);
            var retrievedRequest = tokenIO.RetrieveTokenRequest(requestId);
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
                    Id = memberSync.MemberId()
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

            var requestId = memberSync.StoreTokenRequest(storedPayload, storedOptions);
            Assert.IsNotEmpty(requestId);

            var retrievedRequest = tokenIO.RetrieveTokenRequest(requestId);
            Assert.AreEqual(storedPayload, retrievedRequest.RequestPayload);
            Assert.AreEqual(storedOptions, retrievedRequest.RequestOptions);
        }

        
        [Test]
        public void AddAndGetTokenRequest_NotFound()
        {
            Assert.Throws<AggregateException>(() => tokenIO.RetrieveTokenRequest("bogus"));
            Assert.Throws<AggregateException>(() => tokenIO.RetrieveTokenRequest(memberSync.MemberId()));
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
                    Id = tokenIO.CreateMember().MemberId()
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
            Assert.Throws<AggregateException>(() => memberSync.StoreTokenRequest(storedPayload, storedOptions));
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
                    Id = memberSync.MemberId()
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
            memberSync.StoreTokenRequest(storedPayload, storedOptions);
            
            var requestId = memberSync.StoreTokenRequest(storedPayload, storedOptions);
            Assert.IsNotEmpty(requestId);
            var retrievedRequest1 = tokenIO.RetrieveTokenRequest(requestId);
            Assert.IsFalse(retrievedRequest1.RequestOptions.ReceiptRequested);
            
            var optionsUpdate = new Tokenio.Proto.Common.TokenProtos.TokenRequestOptions
            {
                ReceiptRequested = true
            };
            
            memberSync.UpdateTokenRequest(requestId, optionsUpdate);
            var retrievedRequest2 = tokenIO.RetrieveTokenRequest(requestId);
            Assert.IsTrue(retrievedRequest2.RequestOptions.ReceiptRequested);
        }
    }
}
