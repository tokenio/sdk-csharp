using System;
using Tokenio.Proto.Common.NotificationProtos;
using Xunit;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;
using System.Linq;
using System.Collections.Immutable;
namespace TokenioSample
{
    public class NotifySampleTest
    {
        /// <summary>
        /// Triggers the balance step up notification test.
        /// </summary>
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
