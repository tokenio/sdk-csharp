using Tokenio.Proto.Common.NotificationProtos;
namespace Tokenio.User
{
    public class NotifyResult
    {   
        /// <summary>
        /// Create the specified notificationId and notifyStatus.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="notificationId">Notification identifier.</param>
        /// <param name="notifyStatus">Notify status.</param>
        public static NotifyResult Create(string notificationId, NotifyStatus notifyStatus)
        {
            return new NotifyResult
            {

                NotificationId = notificationId,
                NotifyStatus = notifyStatus
            };

        }
        public string NotificationId { get; private set; }

        public NotifyStatus NotifyStatus { get; private set; }
    }
}