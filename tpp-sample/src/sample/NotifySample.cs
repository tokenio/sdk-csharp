using System.Collections.Generic;
using Tokenio.Proto.Common.NotificationProtos;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    public static class NotifySample
    {
        /// <summary>
        /// Triggers a notification to step up the signature level when requesting balance information.
        /// </summary>
        /// <param name="member">member</param>
        /// <param name="accountIds">list of account id</param>
        /// <returns>notification status</returns>
        public static NotifyStatus TriggerBalanceStepUpNotification(
            TppMember member,
            IList<string> accountIds)
        {
            return member.TriggerBalanceStepUpNotificationBlocking(accountIds);
        }
    }
}