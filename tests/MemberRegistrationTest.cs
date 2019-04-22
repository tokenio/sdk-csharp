using NUnit.Framework;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Security;
using static Test.TestUtil;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace Test
{
    [TestFixture]
    public class MemberRegistrationTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        [Test]
        public void CreateMember()
        {
            var alias = Alias();
            var member = tokenClient.CreateMemberBlocking(alias);
            Assert.AreEqual(3, member.GetKeysBlocking().Count);
        }

        [Test]
        public void CreateMember_noAlias()
        {
            var member = tokenClient.CreateMemberBlocking();
            CollectionAssert.IsEmpty(member.GetAliasesBlocking());
            Assert.AreEqual(3, member.GetKeysBlocking().Count);
        }

        [Test]
        public void LoginMember()
        {
            var member = tokenClient.CreateMemberBlocking(Alias());

            var loggedIn = tokenClient.GetMemberBlocking(member.MemberId());
            CollectionAssert.AreEquivalent(member.GetAliasesBlocking(), loggedIn.GetAliasesBlocking());
            CollectionAssert.AreEquivalent(member.GetKeysBlocking(), loggedIn.GetKeysBlocking());
        }

        [Test]
        public void ProvisionDevice()
        {
            var member = tokenClient.CreateMemberBlocking(Alias());

            var secondDevice = NewSdkInstance();

            var deviceInfo = secondDevice.ProvisionDeviceBlocking(member.GetFirstAliasBlocking());
            member.ApproveKeysBlocking(deviceInfo.Keys);

            var loggedIn = secondDevice.GetMemberBlocking(deviceInfo.MemberId);

            CollectionAssert.AreEquivalent(member.GetAliasesBlocking(), loggedIn.GetAliasesBlocking());
            Assert.AreEqual(6, loggedIn.GetKeysBlocking().Count);
        }

        [Test]
        public void AddAlias()
        {
            var alias1 = Alias();
            var alias2 = Alias();

            var member = tokenClient.CreateMemberBlocking(alias1);
            CollectionAssert.AreEquivalent(
                new[] {alias1.ToNormalized()},
                member.GetAliasesBlocking());

            member.AddAliasBlocking(alias2);
            CollectionAssert.AreEquivalent(
                new[] {alias1.ToNormalized(), alias2.ToNormalized()},
                member.GetAliasesBlocking());
        }

        [Test]
        public void RemoveAlias()
        {
            var alias1 = Alias();
            var alias2 = Alias();

            var member = tokenClient.CreateMemberBlocking(alias1);
            CollectionAssert.AreEquivalent(new[] {alias1.ToNormalized()}, member.GetAliasesBlocking());

            member.AddAliasBlocking(alias2);
            CollectionAssert.AreEquivalent(new[] {alias1.ToNormalized(), alias2.ToNormalized()}, member.GetAliasesBlocking());

            member.RemoveAliasBlocking(alias2);
            CollectionAssert.AreEquivalent(new[] {alias1.ToNormalized()}, member.GetAliasesBlocking());
        }

        [Test]
        public void AliasDoesNotExist()
        {
            var alias = Alias();
            Assert.IsNull(tokenClient.ResolveAliasBlocking(alias));
        }

        [Test]
        public void AliasExists()
        {
            var alias = Alias();
            tokenClient.CreateMemberBlocking(alias);
            Assert.IsNotNull(tokenClient.ResolveAliasBlocking(alias));
        }

        [Test]
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

            Assert.AreEqual(member.MemberId(), recovered.MemberId());
            Assert.AreEqual(3, recovered.GetKeysBlocking().Count);
            Assert.IsEmpty(recovered.GetAliasesBlocking());
            Assert.False(tokenClient.AliasExistsBlocking(alias));

            recovered.VerifyAliasBlocking(verificationId, "code");
            Assert.True(tokenClient.AliasExistsBlocking(alias));
            CollectionAssert.AreEquivalent(new[] {alias.ToNormalized()}, recovered.GetAliasesBlocking());
        }

        [Test]
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

            Assert.AreEqual(member.MemberId(), recovered.MemberId());
            Assert.AreEqual(3, recovered.GetKeysBlocking().Count);
            Assert.IsEmpty(recovered.GetAliasesBlocking());
            Assert.False(tokenClient.AliasExistsBlocking(alias));

            recovered.VerifyAliasBlocking(verificationId, "code");
            Assert.True(tokenClient.AliasExistsBlocking(alias));
            CollectionAssert.AreEquivalent(new[] {alias.ToNormalized()}, recovered.GetAliasesBlocking());
        }
    }
}
