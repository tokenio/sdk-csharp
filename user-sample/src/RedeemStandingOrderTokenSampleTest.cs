using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SubmissionProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class RedeemStandingOrderTokenSampleTest
    {
        [Fact]
        public void RedeemStandingOrderTokenTest()
        {
            using (var tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias payeeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(payeeAlias);
                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");
                Token token =
                    CreateStandingOrderTokenSample.CreateStandingOrderToken(payer, payeeAlias, Level.Standard);
                StandingOrderSubmission submission = RedeemStandingOrderTokenSample
                    .RedeemStandingOrderToken(payee, token.Id);
                Assert.NotNull(submission);
            }
        }
    }
}