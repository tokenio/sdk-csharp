using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using UserMember = Tokenio.User.Member;


namespace Tokenio.Sample.User
{
    public class CancelAccessTokenSampleTest
    {
        [Fact]
        public void CancelAccessTokenByGrantorTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember grantor = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                string accountId = grantor.CreateTestBankAccountBlocking(1000.0, "EUR").Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                UserMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateAndEndorseAccessTokenSample.CreateAccessToken(grantor, accountId, granteeAlias);
                TokenOperationResult result = CancelTokenSample.CancelAccessToken(grantor, token.Id);
                Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);
            }
        }

        [Fact]
        public void CancelTransferTokenByGrantorTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias granteeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateTransferTokenSample.CreateTransferToken(payer, granteeAlias, Key.Types.Level.Low);
                TokenOperationResult result = CancelTokenSample.CancelTransferToken(payer, token.Id);
                Assert.Equal(TokenOperationResult.Types.Status.Success, result.Status);
            }
        }
    }
}
