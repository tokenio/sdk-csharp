using System;
using Grpc.Core;
using Tokenio.TokenRequests;
using Tokenio.User;
using Tokenio.Utils;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using Token = Tokenio.Proto.Common.TokenProtos.Token;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

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
        public void GetTokenRequestResultTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember payee = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                string requestId = StoreAndRetrieveTokenRequestSample.StoreTransferTokenRequest(payee);

                UserMember payer = TestUtil.CreateUserMember();
                TokenRequest request = tokenClient.RetrieveTokenRequestBlocking(requestId);
                var storedRequest = new Tokenio.Proto.Common.TokenProtos.TokenRequest
                {
                    Id = requestId,
                    RequestPayload = request.GetTokenRequestPayload(),
                    RequestOptions = request.GetTokenRequestOptions()
                };
                TransferTokenBuilder builder = payer.CreateTransferTokenBuilder(storedRequest)
                    .SetAccountId(payer.GetAccountsBlocking()[0].Id());
                PrepareTokenResult prepared = payer.PrepareTransferTokenBlocking(builder);
                Token token = payer.CreateTokenBlocking(prepared.TokenPayload, Level.Standard);

                // no result until the user signs the token request state
                AggregateException notFound = Assert.Throws<AggregateException>(
                    () => StoreAndRetrieveTokenRequestSample.GetTokenRequestResult(payee, requestId));
                Assert.Equal(StatusCode.NotFound, ((RpcException) notFound.InnerException).StatusCode);

                payer.SignTokenRequestStateBlocking(requestId, token.Id, Util.Nonce());

                TokenRequestResult result =
                    StoreAndRetrieveTokenRequestSample.GetTokenRequestResult(payee, requestId);
                Assert.Equal(token.Id, result.TokenId);
                Assert.NotEmpty(result.Signature.Signature_);
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