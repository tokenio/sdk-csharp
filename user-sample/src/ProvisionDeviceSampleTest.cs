using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class ProvisionDeviceSampleTest
    {

        [Fact]
        public void provisionDevice()
        {
            using (TokenClient remoteDevice = TestUtil.CreateClient())
            {
                Alias alias = TestUtil.RandomAlias();
                UserMember remoteMember = remoteDevice.CreateMemberBlocking(alias);
                remoteMember.SubscribeToNotifications("iron");

                TokenClient localDeviceClient = TestUtil.CreateClient();
                Key key = ProvisionDeviceSample.ProvisionDevice(localDeviceClient, alias);
                remoteMember.ApproveKeyBlocking(key);

                UserMember local = ProvisionDeviceSample.UseProvisionedDevice(localDeviceClient, alias);

                Assert.Equal(local.MemberId(), remoteMember.MemberId());
            }
        }
    }
}

