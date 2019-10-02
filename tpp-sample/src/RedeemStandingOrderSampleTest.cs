using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SubmissionProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Sample.User;
using Xunit;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp
{
    public class RedeemStandingOrderTokenSampleTest
    {
        [Fact]
        public void RedeemStandingOrderTokenTest()
        {
            using (var tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateUserMember();
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Account payeeAccount = payee.CreateTestBankAccountBlocking(1000, "EUR");

                Token token = CreateStandingOrderTokenSample.CreateStandingOrderToken(payer, payeeAlias);

                StandingOrderSubmission submission = RedeemStandingOrderTokenSample
                    .RedeemStandingOrderToken(payee, token.Id);
                Assert.NotNull(submission);
            }
        }
    }
}