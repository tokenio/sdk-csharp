using System;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;
using System.Linq;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using Tokenio.Proto.Common.NotificationProtos;
using System.Collections.Immutable;

namespace TokenioSample
{
    public class ProvisionDeviceSample
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenClient"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static Key ProvisionDevice(TokenClient tokenClient, Alias alias)
        {
            // generate keys, storing (private and public) locally
            DeviceInfo deviceInfo = tokenClient.ProvisionDeviceBlocking(alias);
            Key lowKey = deviceInfo.Keys.Where(k => k.Level == Level.Low).FirstOrDefault();


            // ask user (on "regular" device) to approve one of our keys
            NotifyStatus status = tokenClient.NotifyAddKeyBlocking(
                    alias,
                    (new[] { lowKey }).ToImmutableList(),
                    new DeviceMetadata() { Application = "SDK Sample" }
                   );

            return lowKey;
        }

        /**
         * Log in on provisioned device (assuming "remote" member approved key).
         * @param tokenClient SDK client
         * @param alias member's alias
         * @return Member
         */
        public static UserMember UseProvisionedDevice(TokenClient tokenClient, Alias alias)
        {
            string memberId = tokenClient.GetMemberIdBlocking(alias);
            // Uses the key that remote member approved (we hope)
            UserMember member = tokenClient.GetMemberBlocking(memberId);
            return member;
        }

    }
}
