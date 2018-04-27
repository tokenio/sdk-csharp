using NUnit.Framework;
using Tokenio;
using static Test.TestUtil;

namespace Test
{
    [TestFixture]
    public class BankInformationTest
    {
        private static readonly TokenIO tokenIO = NewSdkInstance();

        [Test]
        public void GetBanks()
        {
            Assert.IsNotEmpty(tokenIO.GetBanks().Banks);
        }
        
        [Test]
        public void GetBankInfo()
        {
            var bankId = tokenIO.GetBanks().Banks[0].Id;
            var member = tokenIO.CreateMember();
            Assert.IsNotNull(member.GetBankInfo(bankId));
        }
    }
}
