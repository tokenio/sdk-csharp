using System;
using Io.Token.Proto.Common.Token;
using NUnit.Framework;
using sdk;
using sdk.Api;
using static sdk.Api.TokenRequestOptions;

namespace tests
{
    [TestFixture]
    public class TokenRequestTest
    {
        private static readonly string tokenUrl = "https://token.io";
        private static readonly TokenIO tokenIO = TestUtil.NewSdkInstance();

        private MemberSync memberSync;

        [SetUp]
        public void Init()
        {
            memberSync = tokenIO.CreateMember(TestUtil.Alias());
        }

        [Test]
        public void AddAndGetTransferTokenRequest()
        {
            var storedRequest = new TokenRequest
            {
                Payload = memberSync.CreateTransferToken(10.0, "EUR")
                    .SetToMemberId(memberSync.MemberId())
                    .BuildPayload(),
                Options = {{redirectUrl.ToString(), tokenUrl}}
            };
            var requestId = memberSync.StoreTokenRequest(storedRequest);
            Assert.IsNotEmpty(requestId);
            storedRequest.Id = requestId;
            var retrievedRequest = tokenIO.RetrieveTokenRequest(requestId);
            Assert.AreEqual(storedRequest, retrievedRequest);
        }


        [Test]
        public void AddAndGetAccessTokenRequest()
        {
            var storedRequest = new TokenRequest
            {
                Payload = new TokenPayload
                {
                    To = new TokenMember
                    {
                        Id = memberSync.MemberId()
                    },
                    Access = new AccessBody
                    {
                        Resources =
                        {
                            new AccessBody.Types.Resource
                            {
                                AllAddresses = new AccessBody.Types.Resource.Types.AllAddresses()
                            }
                        }
                    }
                },
                Options = {{redirectUrl.ToString(), tokenUrl}}
            };
            var requestId = memberSync.StoreTokenRequest(storedRequest);
            Assert.IsNotEmpty(requestId);
            storedRequest.Id = requestId;
            var retrievedRequest = tokenIO.RetrieveTokenRequest(requestId);
            Assert.AreEqual(storedRequest, retrievedRequest);
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
            var storedRequest = new TokenRequest
            {
                Payload = memberSync.CreateTransferToken(10.0, "EUR")
                    .SetToMemberId(tokenIO.CreateMember().MemberId())
                    .BuildPayload()
            };
            Assert.Throws<AggregateException>(() => memberSync.StoreTokenRequest(storedRequest));
        }
    }
}
