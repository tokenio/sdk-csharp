using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.User.Utils;
using Tokenio;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;
using Tokenio.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TokenioSample
{
    public class PollNotificationsSample
    {



        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenClient"></param>
        /// <returns></returns>
        public static UserMember CreateMember(TokenClient tokenClient)
        {
            // Token members "in the real world" are set up to receive notifications:
            // The Token mobile app, having created a new member, subscribes to
            // notifications so they're delivered to the mobile device.
            //
            // When we create a member via createMember for testing, it's not
            // set up to receive notifications. If we try to send a notification
            // to this member, we would get a NO_SUBSCRIBERS error.
            // We set up a fake bank subscription for testing
            // (but we wouldn't do this in production for "real-world" members).
            Alias alias = new Alias()
            {
                Type = Alias.Types.Type.Email,
                Value = "test-" + Util.Nonce() + "+noverify@token.io"

            };

            UserMember member = tokenClient.CreateMemberBlocking(alias);
            member.SubscribeToNotificationsBlocking("iron");
            return member;
        }

        /**
	     * Poll for notifications.
	     *
	     * @param member Whose notifications to poll for
	     * @return a notification, maybe
	     */
        public static Notification Poll(UserMember member)
        {
            for (int retries = 0; retries < 5; retries++)
            {
                // getNotifications doc extract start:
                PagedList<Notification> pagedList =
                        member.GetNotificationsBlocking(10, null);
                IList<Notification> notifications = pagedList.List;
                if (notifications.Count != 0)
                {
                    Notification notification = notifications[0];

                    var bodyCase = Enum.Parse(typeof(NotifyBody.BodyOneofCase), notification.Content.Type.ToLower().Replace("_", ""), true);
                    switch (bodyCase)
                    {
                        
                        case NotifyBody.BodyOneofCase.PayeeTransferProcessed:
                            Console.WriteLine("Transfer processed:"+ notification);
                            break;
                        default:
                            Console.WriteLine("Got notification: "+ notification);
                            break;
                    }
                    if (notification == null)
                    {
                        return new Notification();
                    }
                    else
                    {
                        return notification;

                    }

                }
                //getNotifications doc extract end
                try
                {
                    Console.WriteLine("Don't see notifications yet. Sleeping...\n");
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException ie)
                {
                    throw new ArgumentException("", ie);
                }
            }
            // We waited a few seconds and still don't see any notification. Give up.
            return null;
        }

    }
}
