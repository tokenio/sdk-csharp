using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp {
    public class CancelAccessTokenSampleTest {
        [Fact]
        public void CancelAccessTokenByGranteeTest () {

            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient ()) {
                Tokenio.User.Member grantor = TestUtil.CreateUserMember ();
                string accountId = grantor.GetAccountsBlocking () [0].Id ();
                Alias granteeAlias = TestUtil.RandomAlias ();
                TppMember grantee = tokenClient.CreateMemberBlocking (granteeAlias);

                Token token = TestUtil.CreateAccessToken (grantor, accountId, granteeAlias);
                TokenOperationResult result = CancelAccessTokenSample.CancelAccessToken (grantee, token.Id);
                Assert.Equal (result.Status, TokenOperationResult.Types.Status.Success);

            }
        }

    }
}