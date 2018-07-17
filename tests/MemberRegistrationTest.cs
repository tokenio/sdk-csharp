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
        private static readonly TokenIO tokenIO = NewSdkInstance();

        [Test]
        public void CreateMember()
        {
            var alias = Alias();
            var member = tokenIO.CreateMember(alias);
            Assert.AreEqual(3, member.Keys().Count);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                Assert.True(member.Aliases().Contains(alias)));
        }

        [Test]
        public void CreateMember_noAlias()
        {
            var member = tokenIO.CreateMember();
            CollectionAssert.IsEmpty(member.Aliases());
            Assert.AreEqual(3, member.Keys().Count);
        }

        [Test]
        public void LoginMember()
        {
            var alias = Alias();
            var member = tokenIO.CreateMember(alias);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                Assert.True(member.Aliases().Contains(alias)));

            var loggedIn = tokenIO.GetMember(member.MemberId());
            CollectionAssert.AreEquivalent(member.Aliases(), loggedIn.Aliases());
            CollectionAssert.AreEquivalent(member.Keys(), loggedIn.Keys());
        }

        [Test]
        public void ProvisionDevice()
        {
            var alias = Alias();
            var member = tokenIO.CreateMember(alias);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                Assert.True(member.Aliases().Contains(alias)));

            var secondDevice = NewSdkInstance();

            var deviceInfo = secondDevice.ProvisionDevice(member.FirstAlias());
            member.ApproveKeys(deviceInfo.Keys);

            var loggedIn = secondDevice.GetMember(deviceInfo.MemberId);

            CollectionAssert.AreEquivalent(member.Aliases(), loggedIn.Aliases());
            Assert.AreEqual(6, loggedIn.Keys().Count);
        }

        [Test]
        public void AddAlias()
        {
            var alias1 = Alias();
            var alias2 = Alias();

            var member = tokenIO.CreateMember(alias1);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                CollectionAssert.AreEquivalent(member.Aliases(), new[] {alias1}));

            member.AddAlias(alias2);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                CollectionAssert.AreEquivalent(member.Aliases(), new[] {alias1, alias2}));
        }

        [Test]
        public void RemoveAlias()
        {
            var alias1 = Alias();
            var alias2 = Alias();

            var member = tokenIO.CreateMember(alias1);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                CollectionAssert.AreEquivalent(member.Aliases(), new[] {alias1}));

            member.AddAlias(alias2);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                CollectionAssert.AreEquivalent(member.Aliases(), new[] {alias1, alias2}));

            member.RemoveAlias(alias2);
            CollectionAssert.AreEquivalent(member.Aliases(), new[] {alias1});
        }

        [Test]
        public void AliasDoesNotExist()
        {
            var alias = Alias();
            Assert.False(tokenIO.AliasExists(alias));
        }

        [Test]
        public void AliasExists()
        {
            var alias = Alias();
            tokenIO.CreateMember(alias);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                Assert.True(tokenIO.AliasExists(alias)));
        }

        [Test]
        public void Recovery()
        {
            var alias = Alias();
            var member = tokenIO.CreateMember(alias);
            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                Assert.True(member.Aliases().Contains(alias)));

            member.UseDefaultRecoveryRule();
            var verificationId = tokenIO.BeginRecovery(alias);
            var recovered = tokenIO.CompleteRecoveryWithDefaultRule(
                member.MemberId(),
                verificationId,
                "code");

            Assert.AreEqual(member.MemberId(), recovered.MemberId());
            Assert.AreEqual(3, recovered.Keys().Count);
            Assert.IsEmpty(recovered.Aliases());
            Assert.False(tokenIO.AliasExists(alias));

            recovered.VerifyAlias(verificationId, "code");
            Assert.True(tokenIO.AliasExists(alias));
            CollectionAssert.AreEquivalent(recovered.Aliases(), new[] {alias});
        }

        [Test]
        public void Recovery_withSecondaryAgent()
        {
            var alias = Alias();
            var member = tokenIO.CreateMember(alias);
            var memberId = member.MemberId();
            var primaryAgentId = member.GetDefaultAgent();
            var secondaryAgent = tokenIO.CreateMember(Alias());
            var unusedSecondaryAgent = tokenIO.CreateMember(Alias());

            WaitUntil(ALIAS_VERIFICATION_TIMEOUT_MS, ALIAS_VERIFICATION_POLL_FREQUENCY_MS, () =>
                Assert.True(member.Aliases().Contains(alias)));
            member.AddRecoveryRule(new RecoveryRule
            {
                PrimaryAgent = primaryAgentId,
                SecondaryAgents = {secondaryAgent.MemberId(), unusedSecondaryAgent.MemberId()}
            });

            var cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
            var key = cryptoEngine.GenerateKey(Privileged);

            var verificationId = tokenIO.BeginRecovery(alias);
            var authorization = new Authorization
            {
                MemberId = memberId,
                MemberKey = key,
                PrevHash = member.LastHash()
            };
            var signature = secondaryAgent.AuthorizeRecovery(authorization);
            var op1 = tokenIO.GetRecoveryAuthorization(verificationId, "code", key);
            var op2 = new MemberRecoveryOperation
            {
                Authorization = authorization,
                AgentSignature = signature
            };
            var recovered = tokenIO.CompleteRecovery(
                memberId,
                new[] {op1, op2},
                key,
                cryptoEngine);

            Assert.AreEqual(member.MemberId(), recovered.MemberId());
            Assert.AreEqual(3, recovered.Keys().Count);
            Assert.IsEmpty(recovered.Aliases());
            Assert.False(tokenIO.AliasExists(alias));

            recovered.VerifyAlias(verificationId, "code");
            Assert.True(tokenIO.AliasExists(alias));
            CollectionAssert.AreEquivalent(recovered.Aliases(), new[] {alias});
        }
    }
}
