using System.Collections.Generic;
using System.Linq;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace samples
{
    public class ProvisionDeviceSample
    {
        /// <summary>
        /// Illustrate provisioning a new device for an already-existing member.
        /// </summary>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="alias">member's alias</param>
        /// <returns>key which we hope member will approve</returns>
        public static Key ProvisionDevice(TokenClient tokenClient, Alias alias)
        {
            // generate keys, storing (private and public) locally
            var deviceInfo = tokenClient.ProvisionDevice(alias).Result;
            var lowKey = deviceInfo.Keys
                .Where(k => k.Level.Equals(Low))
                .FirstOrDefault(null);
            // ask user (on "regular" device) to approve one of our keys
            IList<Key> keys = new List<Key>();
            keys.Add(lowKey);
            var status = tokenClient.NotifyAddKey(alias, keys, new DeviceMetadata()).Result;
            return lowKey;
        }

        /// <summary>
        /// Log in on provisioned device (assuming "remote" member approved key).
        /// </summary>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="alias">member's alias</param>
        /// <returns>Member</returns>
        public static Member UseProvisionedDevice(TokenClient tokenClient, Alias alias)
        {
            var memberId = tokenClient.GetMemberId(alias).Result;
            // Uses the key that remote member approved (we hope)
            var member = tokenClient.GetMember(memberId).Result;
            return member;
        }
    }
}
