using System.Collections.Generic;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.TokenProtos;

namespace samples
{
    public class NotifySample
    {
        public static NotifyStatus TriggerBalanceStepUpNotification(
            Member member,
            IList<string> accountIds)
        {
            return member.TriggerBalanceStepUpNotificationBlocking(accountIds);
        }
    }
}
