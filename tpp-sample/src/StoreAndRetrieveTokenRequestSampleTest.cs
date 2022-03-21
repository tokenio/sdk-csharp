﻿using Tokenio.TokenRequests;
using Xunit;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    /// <summary>
    /// Sample to show how to store and retrieve token requests.
    /// </summary>
    public class StoreAndRetrieveTokenRequestSampleTest
    {
        private static string setTransferDestinationsUrl = "https://tpp-sample.com/callback/"
                                                           + "transferDestinations";

        private static string setTransferDestinationsCallback = "https://tpp-sample.com/callback/"
                                                                + "transferDestinations?supportedTransferDestinationType=FASTER_PAYMENTS&"
                                                                + "supportedTransferDestinationType=SEPA&bankName=Iron&country=UK";

        [Fact]
        public void StoreAndRetrieveTransferTokenTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
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
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember grantee = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                string requestId = StoreAndRetrieveTokenRequestSample.StoreTransferTokenRequest(grantee);
                TokenRequest request = tokenClient.RetrieveTokenRequestBlocking(requestId);
                Assert.NotNull(request);
            }
        }
    }
}