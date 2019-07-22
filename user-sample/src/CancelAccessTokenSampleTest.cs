using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;


namespace TokenioSample
{
    public class CancelAccessTokenSampleTest
    {
        /// <summary>
        /// Cancels the access token by grantee test.
        /// </summary>
        [Fact]
        public void CancelAccessTokenByGrantorTest()
        {

           using  (TokenClient tokenClient = TestUtil.CreateClient()) {
                Tokenio.User.Member grantor = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias()); ;
                string accountId = grantor.CreateTestBankAccountBlocking(1000.0, "EUR").Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                Tokenio.User.Member grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateAndEndorseAccessTokenSample.CreateAccessToken(grantor, accountId, granteeAlias);
                TokenOperationResult result = CancelAccessTokenSample.CancelAccessToken(grantor, token.Id);
                Assert.Equal(result.Status,TokenOperationResult.Types.Status.Success);

            }
        }


    }
}

