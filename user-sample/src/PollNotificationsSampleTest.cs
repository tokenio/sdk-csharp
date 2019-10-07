using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.User;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    public class PollNotificationsSampleTest {

        [Fact]
        public void NotifyPaymentRequestSampleTest () {

            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient ()) {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts (tokenClient);

                UserMember payee = PollNotificationsSample.CreateMember (tokenClient);
                Alias payeeAlias = payee.GetFirstAliasBlocking ();
                Tokenio.User.Account account = LinkMemberAndBankSample.LinkBankAccounts (payer);
                LinkMemberAndBankSample.LinkBankAccounts (payee);

                TransferDestination tokenDestination = new TransferDestination {
                    Token = new TransferDestination.Types.Token {
                    MemberId = payee.MemberId ()
                    }
                };

                TransferTokenBuilder builder = payer.CreateTransferTokenBuilder (100.00, "EUR")
                    .SetAccountId (account.Id ())
                    .SetToAlias (payeeAlias)
                    .AddDestination (tokenDestination);

                PrepareTokenResult result = payer.PrepareTransferTokenBlocking (builder);
                Token token = payer.CreateTokenBlocking (result.TokenPayload, Key.Types.Level.Standard);
                Transfer transfer = payee.RedeemTokenBlocking (token);

                var notification = PollNotificationsSample.Poll (payee);

                Assert.NotNull (notification);
            }
        }
    }
}