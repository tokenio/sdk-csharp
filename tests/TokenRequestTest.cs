using System;
using NUnit.Framework;
using Tokenio;
using Tokenio.Proto.Common.TokenProtos;
using static Test.TestUtil;
using static Tokenio.Proto.Common.TokenProtos.AccessBody.Types;
using static Tokenio.Proto.Common.TokenProtos.AccessBody.Types.Resource.Types;
using TokenRequestOptions = Tokenio.TokenRequestOptions;

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
            var storedRequest = new TokenRequest
            {
                Payload = memberSync.CreateTransferToken(10.0, "EUR")
                    .SetToMemberId(memberSync.MemberId())
                    .BuildPayload(),
                Options = {{TokenRequestOptions.redirectUrl.ToString(), tokenUrl}}
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
                            new Resource
                            {
                                AllAddresses = new AllAddresses()
                            }
                        }
                    }
                },
                Options = {{TokenRequestOptions.redirectUrl.ToString(), tokenUrl}}
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
