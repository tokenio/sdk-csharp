using System.Linq;
using Xunit;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Security;
using static Test.TestUtil;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace Test
{
    public class MemberRegistrationTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        [Fact]
        public void CreateMember()
        {
            var alias = Alias();
            var member = tokenClient.CreateMemberBlocking(alias);
            Assert.Equal(3, member.GetKeysBlocking().Count);
        }

        [Fact]
        public void CreateMember_noAlias()
        {
            var member = tokenClient.CreateMemberBlocking();
            Assert.Empty(member.GetAliasesBlocking());
            Assert.Equal(3, member.GetKeysBlocking().Count);
        }

        [Fact]
        public void LoginMember()
        {
            var member = tokenClient.CreateMemberBlocking(Alias());

            var loggedIn = tokenClient.GetMemberBlocking(member.MemberId());
            CollectionAssert.Equivalent(member.GetAliasesBlocking(), loggedIn.GetAliasesBlocking());
            CollectionAssert.Equivalent(member.GetKeysBlocking(), loggedIn.GetKeysBlocking());
        }

        [Fact]
        public void AddAlias()
        {
            var alias1 = Alias();
            var alias2 = Alias();

            var member = tokenClient.CreateMemberBlocking(alias1);
            CollectionAssert.Equivalent(
                new[] {alias1.ToNormalized()},
                member.GetAliasesBlocking());

            member.AddAliasBlocking(alias2);
            CollectionAssert.Equivalent(
                new[] {alias1.ToNormalized(), alias2.ToNormalized()}.AsEnumerable(),
                member.GetAliasesBlocking().AsEnumerable());
        }

        [Fact]
        public void RemoveAlias()
        {
            var alias1 = Alias();
            var alias2 = Alias();

            var member = tokenClient.CreateMemberBlocking(alias1);
            CollectionAssert.Equivalent(new[] {alias1.ToNormalized()}, member.GetAliasesBlocking());

            member.AddAliasBlocking(alias2);
            CollectionAssert.Equivalent(new[] {alias1.ToNormalized(), alias2.ToNormalized()}, member.GetAliasesBlocking());

            member.RemoveAliasBlocking(alias2);
            CollectionAssert.Equivalent(new[] {alias1.ToNormalized()}, member.GetAliasesBlocking());
        }

        [Fact]
        public void AliasDoesNotExist()
        {
            var alias = Alias();
            Assert.Null(tokenClient.ResolveAliasBlocking(alias));
        }

        [Fact]
        public void AliasExists()
        {
            var alias = Alias();
            tokenClient.CreateMemberBlocking(alias);
            Assert.NotNull(tokenClient.ResolveAliasBlocking(alias));
        }

        [Fact]
        public void Recovery()
        {
            var alias = Alias();
            var member = tokenClient.CreateMemberBlocking(alias);

            member.UseDefaultRecoveryRuleBlocking();
            var verificationId = tokenClient.BeginRecoveryBlocking(alias);
            var recovered = tokenClient.CompleteRecoveryWithDefaultRuleBlocking(
                member.MemberId(),
                verificationId,
                "code");

            Assert.Equal(member.MemberId(), recovered.MemberId());
            Assert.Equal(3, recovered.GetKeysBlocking().Count);
            Assert.Empty(recovered.GetAliasesBlocking());
            Assert.False(tokenClient.AliasExistsBlocking(alias));

            recovered.VerifyAliasBlocking(verificationId, "code");
            Assert.True(tokenClient.AliasExistsBlocking(alias));
            CollectionAssert.Equivalent(new[] {alias.ToNormalized()}, recovered.GetAliasesBlocking());
        }

        [Fact]
        public void Recovery_withSecondaryAgent()
        {
            var alias = Alias();
            var member = tokenClient.CreateMemberBlocking(alias);            
            var memberId = member.MemberId();
            var primaryAgentId = member.GetDefaultAgentBlocking();
            var secondaryAgent = tokenClient.CreateMemberBlocking(Alias());
            var unusedSecondaryAgent = tokenClient.CreateMemberBlocking(Alias());
            member.AddRecoveryRuleBlocking(new RecoveryRule
            {
                PrimaryAgent = primaryAgentId,
                SecondaryAgents = {secondaryAgent.MemberId(), unusedSecondaryAgent.MemberId()}
            });

            var cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
            var key = cryptoEngine.GenerateKey(Privileged);

            var verificationId = tokenClient.BeginRecoveryBlocking(alias);
            var authorization = new Authorization
            {
                MemberId = memberId,
                MemberKey = key,
                PrevHash = member.GetLastHashBlocking()
            };
            var signature = secondaryAgent.AuthorizeRecoveryBlocking(authorization);
            var op1 = tokenClient.GetRecoveryAuthorizationBlocking(verificationId, "code", key);
            var op2 = new MemberRecoveryOperation
            {
                Authorization = authorization,
                AgentSignature = signature
            };
            var recovered = tokenClient.CompleteRecoveryBlocking(
                memberId,
                new[] {op1, op2},
                key,
                cryptoEngine);

            Assert.Equal(member.MemberId(), recovered.MemberId());
            Assert.Equal(3, recovered.GetKeysBlocking().Count);
            Assert.Empty(recovered.GetAliasesBlocking());
            Assert.False(tokenClient.AliasExistsBlocking(alias));

            recovered.VerifyAliasBlocking(verificationId, "code");
            Assert.True(tokenClient.AliasExistsBlocking(alias));
            CollectionAssert.Equivalent(new[] {alias.ToNormalized()}, recovered.GetAliasesBlocking());
        }
    }
}
