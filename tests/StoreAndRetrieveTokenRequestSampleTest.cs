using Sample;
using Xunit;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using Member = Tokenio.Member;
using static Test.TestUtil;
using TokenRequest = Tokenio.TokenRequest;

namespace Test
{
    public class StoreAndRetrieveTokenRequestSampleTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        private Member member;

        public StoreAndRetrieveTokenRequestSampleTest()
        {
            member = tokenClient.CreateMemberBlocking(DomainAlias(), CreateMemberType.Business);
        }

        [Fact]
        public void StoreAndRetrieveAccessToken()
        {
            var requestId = StoreAndRetrieveTokenRequestSample.StoreAccessTokenRequest(member);
            TokenRequest tokenRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
            Assert.NotNull(tokenRequest);
        }

        [Fact]
        public void StoreAndRetrieveTransferToken()
        {
            var requestId = StoreAndRetrieveTokenRequestSample.StoreTransferTokenRequest(member);
            TokenRequest tokenRequest = tokenClient.RetrieveTokenRequestBlocking(requestId);
            Assert.NotNull(tokenRequest);
        }
    }
}