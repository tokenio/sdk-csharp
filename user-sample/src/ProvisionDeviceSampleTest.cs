using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public class ProvisionDeviceSampleTest
    {
        [Fact]
        public void ProvisionDevice()
        {
            using (Tokenio.User.TokenClient remoteDevice = TestUtil.CreateClient())
            {
                Alias alias = TestUtil.RandomAlias();
                UserMember remoteMember = remoteDevice.CreateMemberBlocking(alias);
                remoteMember.SubscribeToNotifications("iron");
                Tokenio.User.TokenClient localDeviceClient = TestUtil.CreateClient();
                Key key = ProvisionDeviceSample.ProvisionDevice(localDeviceClient, alias);
                remoteMember.ApproveKeyBlocking(key);
                UserMember local = ProvisionDeviceSample.UseProvisionedDevice(localDeviceClient, alias);
                Assert.Equal(local.MemberId(), remoteMember.MemberId());
            }
        }
    }
}