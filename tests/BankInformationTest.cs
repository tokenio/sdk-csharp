using NUnit.Framework;
using Tokenio;
using static Test.TestUtil;

namespace Test
{
    [TestFixture]
    public class BankInformationTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        [Test]
        public void GetBanks()
        {
            Assert.IsNotEmpty(tokenClient.GetBanksBlocking().Banks);
        }
        
        [Test]
        public void GetBankInfo()
        {
            var bankId = tokenClient.GetBanksBlocking().Banks[0].Id;
            var member = tokenClient.CreateMemberBlocking();
            Assert.IsNotNull(member.GetBankInfoBlocking(bankId));
        }
    }
}
