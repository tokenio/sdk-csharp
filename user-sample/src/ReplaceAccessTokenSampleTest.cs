using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class ReplaceAccessTokenSampleTest
    {
        [Fact]
        public void GetAccessTokensTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember grantor = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                string accountId = grantor.CreateTestBankAccountBlocking(1000, "EUR").Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                UserMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);
                Token createdToken =
                    CreateAndEndorseAccessTokenSample.CreateBalanceAccessToken(grantor, accountId, granteeAlias);
                Token foundToken = ReplaceAccessTokenSample.FindAccessToken(tokenClient, grantor, granteeAlias);
                Assert.Equal(foundToken.Id, createdToken.Id);
            }
        }

        [Fact]
        public void ReplaceAccessTokenTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember grantor = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                string accountId = grantor.CreateTestBankAccountBlocking(1000, "EUR").Id();
                Alias granteeAlias = TestUtil.RandomAlias();
                UserMember grantee = tokenClient.CreateMemberBlocking(granteeAlias);
                CreateAndEndorseAccessTokenSample.CreateBalanceAccessToken(grantor, accountId, granteeAlias);
                Token activeToken = ReplaceAccessTokenSample.FindAccessToken(tokenClient, grantor, granteeAlias);
                ReplaceAccessTokenSample.ReplaceAccessToken(grantor, granteeAlias, activeToken);
                Assert.NotEqual(ReplaceAccessTokenSample.FindAccessToken(tokenClient, grantor, granteeAlias).Id,
                    activeToken.Id);
            }
        }
    }
}