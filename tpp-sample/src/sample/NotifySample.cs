using System;
using System.Collections.Generic;
using Tokenio.Proto.Common.NotificationProtos;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;
namespace TokenioSample
{
    public class NotifySample
    {

     
        /// <summary>
        /// Triggers a notification to step up the signature level when requesting balance information
        /// </summary>
        /// <returns>The balance step up notification.</returns>
        /// <param name="member">Member.</param>
        /// <param name="accountIds">Account identifiers.</param>
        public static NotifyStatus TriggerBalanceStepUpNotification(
                TppMember member,
                IList<string> accountIds)
        {
            return member.TriggerBalanceStepUpNotificationBlocking(accountIds);
        }

    }
}
