using Tokenio.Proto.Common.NotificationProtos;

namespace Tokenio.User
{
    public class NotifyResult
    {
        /// <summary>
        /// Create token request.
        /// </summary>
        /// <param name = "notificationId">notification id</param>
        /// <param name = "notifyStatus">notify status</param>
        /// <returns>notify result</returns>
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