using System;
using Tokenio.Proto.Common.AliasProtos;
using Xunit;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    public class MemberRecoverySampleTest
    {
        [Fact]
    public void RecoveryDefault()
        { // "normal consumer" recovery using "shortcuts"
            using (TokenClient tokenClient = TestUtil.CreateClient()) {
                MemberRecoverySample mrs = new MemberRecoverySample();

                // set up
                Alias originalAlias = TestUtil.RandomAlias();
                TppMember originalMember = tokenClient.CreateMemberBlocking(originalAlias);
                mrs.SetUpDefaultRecoveryRule(originalMember);

                TokenClient otherTokenClient = TestUtil.CreateClient();
                TppMember recoveredMember = mrs.RecoverWithDefaultRule(
                        otherTokenClient,
                        originalAlias);
                Alias recoveredAlias = recoveredMember.GetFirstAliasBlocking();
                Assert.Equal(recoveredAlias, originalAlias);
            }
            }

        [Fact]
        public void RecoveryComplex()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                MemberRecoverySample mrs = new MemberRecoverySample();

                TokenClient agentTokenIO = TestUtil.CreateClient();
                Alias agentAlias = TestUtil.RandomAlias();
                TppMember agentMember = agentTokenIO.CreateMemberBlocking(agentAlias);

                mrs.agentMember = agentMember;

                // set up
                Alias originalAlias = TestUtil.RandomAlias();
                TppMember originalMember = tokenClient.CreateMemberBlocking(originalAlias);
                mrs.SetUpComplexRecoveryRule(originalMember, tokenClient, agentAlias);

                // recover
                TokenClient otherTokenClient = TestUtil.CreateClient();
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
