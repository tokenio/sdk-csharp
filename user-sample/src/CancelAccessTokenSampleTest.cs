using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;

namespace Tokenio.Sample.User {
    public class CancelAccessTokenSampleTest {
        [Fact]
        public void CancelAccessTokenByGrantorTest () {

            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient ()) {
                Tokenio.User.Member grantor = tokenClient.CreateMemberBlocking (TestUtil.RandomAlias ());
                string accountId = grantor.CreateTestBankAccountBlocking (1000.0, "EUR").Id ();
                Alias granteeAlias = TestUtil.RandomAlias ();
                Tokenio.User.Member grantee = tokenClient.CreateMemberBlocking (granteeAlias);

                Token token = CreateAndEndorseAccessTokenSample.CreateAccessToken (grantor, accountId, granteeAlias);
                TokenOperationResult result = CancelAccessTokenSample.CancelAccessToken (grantor, token.Id);
                Assert.Equal (TokenOperationResult.Types.Status.Success, result.Status);

            }
        }
    }
}