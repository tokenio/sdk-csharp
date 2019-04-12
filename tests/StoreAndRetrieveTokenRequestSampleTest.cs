using System;
using NUnit.Framework;
using samples;
using Tokenio;
using Member = Tokenio.Member;
using static Test.TestUtil;

namespace Test
{
    [TestFixture]
    public class StoreAndRetrieveTokenRequestSampleTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        private Member member;
        
        [SetUp]
        public void Init()
        {
            member = tokenClient.CreateMemberBlocking(Alias());
        }

        [Test]
        public void storeAndRetrieveAccessToken()
        {
            var requestId = StoreAndRetrieveTokenRequestSample.storeAccessTokenRequest(member);
            TokenRequest tokenRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
            Assert.IsNotNull(tokenRequest);
        }
        
        [Test]
        public void storeAndRetrieveTransferToken()
        {
            var requestId = StoreAndRetrieveTokenRequestSample.StoreTransferTokenRequest(member);
            TokenRequest tokenRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
            Assert.IsNotNull(tokenRequest);
        }
    }
}