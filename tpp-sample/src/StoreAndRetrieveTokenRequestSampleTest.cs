using Tokenio.TokenRequests;
using Xunit;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;

namespace TokenioSample
{
    /// <summary>
    /// Sample to show how to store and retrieve token requests.
    /// </summary>
    public class StoreAndRetrieveTokenRequestSampleTest
    {
        [Fact]
        public void StoreAndRetrieveTransferTokenTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember payee = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                string requestId = StoreAndRetrieveTokenRequestSample.StoreTransferTokenRequest(payee);
                TokenRequest request = tokenClient.RetrieveTokenRequestBlocking(requestId);
                Assert.NotNull(request);
            }
        }

        [Fact]
        public void StoreAndRetrieveAccessTokenTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember grantee = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                string requestId = StoreAndRetrieveTokenRequestSample.StoreTransferTokenRequest(grantee);
                TokenRequest request = tokenClient.RetrieveTokenRequestBlocking(requestId);
                Assert.NotNull(request);
            }
        }
    }
}
