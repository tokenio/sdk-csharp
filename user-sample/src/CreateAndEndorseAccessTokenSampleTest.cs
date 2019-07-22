using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;
namespace TokenioSample
{
    public class CreateAndEndorseAccessTokenSampleTest
    {
        [Fact]
        public void CreateAccessTokenTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember grantor = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                string accountId = grantor.CreateTestBankAccountBlocking(1000, "EUR").Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                UserMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateAndEndorseAccessTokenSample.CreateAccessToken(grantor, accountId, granteeAlias);
                Assert.NotNull(token);
            }
        }
    }
}
