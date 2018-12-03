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
        /// <param name="tokenIO">SDK client</param>
        /// <param name="alias">member's alias</param>
        /// <returns>key which we hope member will approve</returns>
        public static Key ProvisionDevice(TokenIO tokenIO, Alias alias)
        {
            // generate keys, storing (private and public) locally
            var deviceInfo = tokenIO.ProvisionDevice(alias);
            var lowKey = deviceInfo.Keys
                .Where(k => k.Level.Equals(Low))
                .FirstOrDefault(null);
            // ask user (on "regular" device) to approve one of our keys
            IList<Key> keys = new List<Key>();
            keys.Add(lowKey);
            var status = tokenIO.NotifyAddKey(alias, keys, new DeviceMetadata());
            return lowKey;
        }

        /// <summary>
        /// Log in on provisioned device (assuming "remote" member approved key).
        /// </summary>
        /// <param name="tokenIO">SDK client</param>
        /// <param name="alias">member's alias</param>
        /// <returns>Member</returns>
        public static MemberSync UseProvisionedDevice(TokenIO tokenIO, Alias alias)
        {
            var memberId = tokenIO.GetMemberId(alias);
            // Uses the key that remote member approved (we hope)
            var member = tokenIO.GetMember(memberId);
            return member;
        }
    }
}
