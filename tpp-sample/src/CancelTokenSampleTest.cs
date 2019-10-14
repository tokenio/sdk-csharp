using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Sample.User;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp {
    public class CancelTokenSampleTest {
        [Fact]
        public void CancelAccessTokenByGranteeTest() {
            using(Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient()) {
                Tokenio.User.Member grantor = TestUtil.CreateUserMember();
                string accountId = grantor.GetAccountsBlocking() [0].Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                TppMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateAndEndorseAccessTokenSample.CreateBalanceAccessToken(grantor, accountId, granteeAlias);
                TokenOperationResult result = CancelTokenSample.CancelAccessToken(grantee, token.Id);
                Assert.Equal(result.Status, TokenOperationResult.Types.Status.Success);
            }
        }

        [Fact]
        public void CancelTransferTokenByGranteeTest() {
            using(Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember grantor = TestUtil.CreateUserMember();
                Alias granteeAlias = TestUtil.RandomAlias();
                TppMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateTransferTokenSample.CreateTransferToken(grantor, granteeAlias, Level.Low);
                TokenOperationResult result = CancelTokenSample.CancelTransferToken(grantee, token.Id);
                Assert.Equal(result.Status, TokenOperationResult.Types.Status.Success);
            }
        }
    }
}
