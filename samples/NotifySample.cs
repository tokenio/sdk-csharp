using System.Collections.Generic;
using Tokenio;
using Tokenio.Proto.Common.NotificationProtos;

namespace Sample
{
    public class NotifySample
    {
        /// <summary>
        /// Triggers a notification to step up the signature level when requesting balance information.
        /// </summary>
        /// <param name="member">member</param>
        /// <param name="accountIds">list of account ids</param>
        /// <returns>notification status</returns>
        public static NotifyStatus TriggerBalanceStepUpNotification(
            Member member,
            IList<string> accountIds)
        {
            return member.TriggerBalanceStepUpNotificationBlocking(accountIds);
        }
    }
}
