using Tokenio.Proto.Common.AliasProtos;
using Xunit;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    /// <summary>
    /// Tests for member-recovery sample code.
    /// </summary>
    public class MemberRecoverySampleTest
    {
        [Fact]
        public void RecoveryDefault()
        { // "normal consumer" recovery using "shortcuts"
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                MemberRecoverySample mrs = new MemberRecoverySample();

                // set up
                Alias originalAlias = TestUtil.RandomAlias();
                UserMember originalMember = tokenClient.CreateMemberBlocking(originalAlias);
                mrs.SetUpDefaultRecoveryRule(originalMember);

                Tokenio.User.TokenClient otherTokenClient = TestUtil.CreateClient();
                UserMember recoveredMember = mrs.RecoverWithDefaultRule(
                        otherTokenClient,
                        originalAlias);
                Alias recoveredAlias = recoveredMember.GetFirstAliasBlocking();
                Assert.Equal(recoveredAlias, originalAlias);
            }
        }

        [Fact]
        public void RecoveryComplex()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
            {
                MemberRecoverySample mrs = new MemberRecoverySample();

                Tokenio.User.TokenClient agentTokenIO = TestUtil.CreateClient();
                Alias agentAlias = TestUtil.RandomAlias();
                UserMember agentMember = agentTokenIO.CreateMemberBlocking(agentAlias);

                mrs.agentMember = agentMember;

                // set up
                Alias originalAlias = TestUtil.RandomAlias();
                UserMember originalMember = tokenClient.CreateMemberBlocking(originalAlias);
                mrs.SetUpComplexRecoveryRule(originalMember, tokenClient, agentAlias);

                // recover
                Tokenio.User.TokenClient otherTokenClient = TestUtil.CreateClient();
                UserMember recoveredMember = mrs.RecoverWithComplexRule(
                        otherTokenClient,
                        originalAlias);
                // make sure it worked
                Alias recoveredAlias = recoveredMember.GetFirstAliasBlocking();
                Assert.Equal(recoveredAlias, originalAlias);
            }
        }
    }
}
