using NUnit.Framework;
using samples;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Security;
using static Test.TestUtil;
using Member = Tokenio.Member;

namespace Test
{
    [TestFixture]
    public class MemberMethodsSampleTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        private Member member;

        [SetUp]
        public void Init()
        {
            member = tokenClient.CreateMemberBlocking(Alias());
        }
        
        [Test]
        public void keys()
        {
            IKeyStore keyStore = new InMemoryKeyStore();
            ICryptoEngine crypto = new TokenCryptoEngine("member-id", keyStore);
            MemberMethodsSample.keys(crypto, member);
        }

        [Test]
        public void profiles()
        {
            Profile profile = MemberMethodsSample.profiles(member);
            
            Assert.IsNotEmpty(profile.DisplayNameFirst);
            Assert.IsNotEmpty(profile.DisplayNameLast);
        }
    }
}
