using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class CancelTransferTokenSampleTest
    {
        [Fact]
        public void CancelTransferTokenByGrantorTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
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
