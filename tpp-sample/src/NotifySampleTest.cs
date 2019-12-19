using System.Collections.Immutable;
using Tokenio.Proto.Common.NotificationProtos;
using Xunit;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    public class NotifySampleTest
    {
        [Fact]
        public void TriggerBalanceStepUpNotificationTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());

                NotifyStatus status = member.TriggerBalanceStepUpNotificationBlocking(
                    (new[] {"123", "456"}).ToImmutableList());

                Assert.NotNull(status);
            }
        }
    }
}