using System.Collections.Immutable;
using Tokenio.Proto.Common.NotificationProtos;
using Xunit;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
namespace TokenioSample
{
    public class NotifySampleTest
    {
        [Fact]
        public void TriggerBalanceStepUpNotificationTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());

                NotifyStatus status = member.TriggerBalanceStepUpNotificationBlocking(
                        (new[] { "123", "456" }).ToImmutableList());

                Assert.NotNull(status);
            }
        }

    }
}
