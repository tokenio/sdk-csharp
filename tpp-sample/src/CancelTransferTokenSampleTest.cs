using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp
{
    public class CancelTransferTokenSampleTest
	{
    [Fact]
    public void CancelTransferTokenByGranteeTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember grantor = TestUtil.CreateUserMember();
                Alias granteeAlias = TestUtil.RandomAlias();
                TppMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = TestUtil.CreateTransferToken(grantor, granteeAlias);
                TokenOperationResult result = CancelTransferTokenSample.CancelTransferToken(grantee, token.Id);
                Assert.Equal(result.Status, TokenOperationResult.Types.Status.Success);
            }
        }

    }
}
