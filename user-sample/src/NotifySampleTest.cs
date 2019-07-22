using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class NotifySampleTest
    {


        [Fact]
        public void NotifyPaymentRequestSampleTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                Alias payerAlias = TestUtil.RandomAlias();
                UserMember payer = tokenClient.CreateMemberBlocking(payerAlias);
                UserMember payee = TestUtil.CreateMemberAndLinkAccounts(tokenClient);


                LinkMemberAndBankSample.LinkBankAccounts(payer);

                NotifyStatus status = NotifySample.NotifyPaymentRequest(
                        tokenClient,
                        payee,
                        payerAlias);
                Assert.NotNull(status);
            }

        }
    }
}