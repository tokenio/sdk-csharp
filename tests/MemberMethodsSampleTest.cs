using Sample;
using Xunit;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Security;
using static Test.TestUtil;
using Member = Tokenio.Member;

namespace Test
{
    public class MemberMethodsSampleTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        private readonly Member member;

        public MemberMethodsSampleTest()
        {
            member = tokenClient.CreateMemberBlocking(Alias());
        }

        [Fact]
        public void keys()
        {
            IKeyStore keyStore = new InMemoryKeyStore();
            ICryptoEngine crypto = new TokenCryptoEngine("member-id", keyStore);
            MemberMethodsSample.keys(crypto, member);
        }

        [Fact]
        public void profiles()
        {
            Profile profile = MemberMethodsSample.profiles(member);

            Assert.NotEmpty(profile.DisplayNameFirst);
            Assert.NotEmpty(profile.DisplayNameLast);
        }
    }
}