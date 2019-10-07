using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Security;
using Xunit;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp {
    public class MemberMethodsSampleTest {
        [Fact]
        public void KeysTest () {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient ()) {
                IKeyStore keyStore = new InMemoryKeyStore ();
                ICryptoEngine cryptoEngine = new TokenCryptoEngine ("member-id", keyStore);

                TppMember member = tokenClient.CreateMemberBlocking (TestUtil.RandomAlias ());
                MemberMethodsSample.Keys (cryptoEngine, member);
            }
        }

        [Fact]
        public void ProfilesTest () {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient ()) {
                TppMember member = tokenClient.CreateMemberBlocking (TestUtil.RandomAlias ());
                Profile profile = MemberMethodsSample.Profiles (member);

                Assert.NotEmpty (profile.DisplayNameFirst);
                Assert.NotEmpty (profile.DisplayNameLast);

            }
        }

    }
}