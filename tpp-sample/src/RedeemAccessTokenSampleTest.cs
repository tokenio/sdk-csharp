using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp {
    public class RedeemAccessTokenSampleTest {

        [Fact]
        public void RedeemAccessTokenTest () {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient ()) {
                UserMember grantor = TestUtil.CreateUserMember ();
                string accountId = grantor.GetAccountsBlocking () [0].Id ();
                Alias granteeAlias = TestUtil.RandomAlias ();
                TppMember grantee = tokenClient.CreateMemberBlocking (granteeAlias);

                Token token = TestUtil.CreateAccessToken (grantor, accountId, granteeAlias);
                Money balance0 = RedeemAccessTokenSample.RedeemAccessToken (grantee, token.Id);

                Assert.True (Convert.ToDecimal (balance0.Value) > (decimal.One * 10));

            }

        }
    }
}