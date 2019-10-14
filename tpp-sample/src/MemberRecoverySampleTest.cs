using Tokenio.Proto.Common.AliasProtos;
using Xunit;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp {
    /// <summary>
    /// Tests for member-recovery sample code.
    /// </summary>
    public class MemberRecoverySampleTest {
        [Fact]
        public void RecoveryDefault() { // "normal consumer" recovery using "shortcuts"
            using(Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient()) {
                MemberRecoverySample mrs = new MemberRecoverySample();

                // set up
                Alias originalAlias = TestUtil.RandomAlias();
                TppMember originalMember = tokenClient.CreateMemberBlocking(originalAlias);
                mrs.SetUpDefaultRecoveryRule(originalMember);

                Tokenio.Tpp.TokenClient otherTokenClient = TestUtil.CreateClient();
                TppMember recoveredMember = mrs.RecoverWithDefaultRule(
                    otherTokenClient,
                    originalAlias);
                Alias recoveredAlias = recoveredMember.GetFirstAliasBlocking();
                Assert.Equal(recoveredAlias, originalAlias);
            }
        }

        [Fact]
        public void RecoveryComplex() {
            using(Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient()) {
                MemberRecoverySample mrs = new MemberRecoverySample();

                Tokenio.Tpp.TokenClient agentTokenIO = TestUtil.CreateClient();
                Alias agentAlias = TestUtil.RandomAlias();
                TppMember agentMember = agentTokenIO.CreateMemberBlocking(agentAlias);

                mrs.agentMember = agentMember;

                // set up
                Alias originalAlias = TestUtil.RandomAlias();
                TppMember originalMember = tokenClient.CreateMemberBlocking(originalAlias);
                mrs.SetUpComplexRecoveryRule(originalMember, tokenClient, agentAlias);

                // recover
                Tokenio.Tpp.TokenClient otherTokenClient = TestUtil.CreateClient();
                TppMember recoveredMember = mrs.RecoverWithComplexRule(
                    otherTokenClient,
                    originalAlias);
                // make sure it worked
                Alias recoveredAlias = recoveredMember.GetFirstAliasBlocking();
                Assert.Equal(recoveredAlias, originalAlias);
            }
        }
    }
}
