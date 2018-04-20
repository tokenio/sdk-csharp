using NUnit.Framework;
using sdk;
using sdk.Api;
using static tests.TestUtil;

namespace tests
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
