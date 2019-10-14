using System.Collections.Generic;
using System.Linq;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Security;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    public class MemberMethodsSampleTest {
        [Fact]
        public void KeysTest() {
            using(Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient()) {
                IKeyStore keyStore = new InMemoryKeyStore();
                ICryptoEngine cryptoEngine = new TokenCryptoEngine("member-id", keyStore);
                UserMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                MemberMethodsSample.Keys(cryptoEngine, member);
            }
        }

        [Fact]
        public void ProfilesTest() {
            using(Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                Profile profile = MemberMethodsSample.Profiles(member);
                Assert.NotEmpty(profile.DisplayNameFirst);
                Assert.NotEmpty(profile.DisplayNameLast);
            }
        }

        [Fact]
        public void AliasesTest() {
            using(Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());
                MemberMethodsSample.Aliases(tokenClient, member);
                List<Alias> aliases = member.GetAliasesBlocking().ToList();
                Assert.Equal(1, aliases.Count);
                Assert.Contains("alias4", aliases[0].Value);
            }
        }
    }
}
