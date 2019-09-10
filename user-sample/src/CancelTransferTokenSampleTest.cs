using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class CancelTransferTokenSampleTest
    {
        [Fact]
        public void CancelTransferTokenByGrantorTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias granteeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateAndEndorseTransferTokenSample.CreateTransferToken(payer, granteeAlias);
                TokenOperationResult result = CancelTransferTokenSample.CancelTransferToken(payer, token.Id);
                Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);
            }
        }

    }
}
