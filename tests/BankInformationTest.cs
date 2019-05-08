using Tokenio;
using Xunit;
using static Test.TestUtil;

namespace Test
{
    public class BankInformationTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        [Fact]
        public void GetBanks()
        {
            Assert.NotEmpty(tokenClient.GetBanksBlocking().Banks);
        }
        
        [Fact]
        public void GetBankInfo()
        {
            var bankId = tokenClient.GetBanksBlocking().Banks[0].Id;
            var member = tokenClient.CreateMemberBlocking();
            Assert.NotNull(member.GetBankInfoBlocking(bankId));
        }
    }
}
