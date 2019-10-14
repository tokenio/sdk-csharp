using System.Collections.Immutable;
using System.Linq;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    public static class ProvisionDeviceSample {
        /// <summary>
        /// Illustrate provisioning a new device for an already-existing member.
        /// </summary>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="alias">member's alias</param>
        /// <returns>key which we hope member will approve</returns>
        public static Key ProvisionDevice(Tokenio.User.TokenClient tokenClient, Alias alias) {
            // generate keys, storing (private and public) locally
            DeviceInfo deviceInfo = tokenClient.ProvisionDeviceBlocking(alias);
            Key lowKey = deviceInfo.Keys.FirstOrDefault(k => k.Level == Key.Types.Level.Low);
            // ask user (on "regular" device) to approve one of our keys
            NotifyStatus status = tokenClient.NotifyAddKeyBlocking(
                alias,
                (new [] { lowKey }).ToImmutableList(),
                new DeviceMetadata { Application = "SDK Sample" }
            );
            return lowKey;
        }

        /// <summary>
        /// Log in on provisioned device (assuming "remote" member approved key).
        /// </summary>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="alias">member's alias</param>
        /// <returns>Member</returns>
        public static UserMember UseProvisionedDevice(Tokenio.User.TokenClient tokenClient, Alias alias) {
            string memberId = tokenClient.GetMemberIdBlocking(alias);
            // Uses the key that remote member approved (we hope)
            UserMember member = tokenClient.GetMemberBlocking(memberId);
            return member;
        }
    }
}
