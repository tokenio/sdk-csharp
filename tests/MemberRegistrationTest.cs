using Io.Token.Proto.Common.Member;
using NUnit.Framework;
using sdk.Api;
using sdk.Security;
using static Io.Token.Proto.Common.Member.MemberRecoveryOperation.Types;
using static Io.Token.Proto.Common.Security.Key.Types.Level;

namespace tests
{
    [TestFixture]
    public class MemberRegistrationTest
    {
        private static readonly TokenIO tokenIO = TestUtil.NewSdkInstance();

        [Test]
        public void CreateMember()
        {
            var alias = TestUtil.Alias();
            var member = tokenIO.CreateMember(alias);
            CollectionAssert.Contains(member.Aliases(), alias);
            Assert.AreEqual(3, member.Keys().Count);
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
            var alias = TestUtil.Alias();
            var member = tokenIO.CreateMember(alias);
            var loggedIn = tokenIO.GetMember(member.MemberId());
            CollectionAssert.AreEqual(member.Aliases(), loggedIn.Aliases());
            CollectionAssert.AreEqual(member.Keys(), loggedIn.Keys());
        }

        [Test]
        public void ProvisionDevice()
        {
            var alias = TestUtil.Alias();
            var member = tokenIO.CreateMember(alias);

            var secondDevice = TestUtil.NewSdkInstance();

            var deviceInfo = secondDevice.ProvisionDevice(member.FirstAlias());
            member.ApproveKeys(deviceInfo.Keys);

            var loggedIn = secondDevice.GetMember(deviceInfo.MemberId);

            CollectionAssert.AreEqual(member.Aliases(), loggedIn.Aliases());
            Assert.AreEqual(6, loggedIn.Keys().Count);
        }

        [Test]
        public void AddAlias()
        {
            var alias1 = TestUtil.Alias();
            var alias2 = TestUtil.Alias();
            var alias3 = TestUtil.Alias();

            var member = tokenIO.CreateMember(alias1);
            member.AddAlias(alias2);
            member.AddAlias(alias3);

            CollectionAssert.AreEquivalent(member.Aliases(), new[] {alias1, alias2, alias3});
        }

        [Test]
        public void RemoveAlias()
        {
            var alias1 = TestUtil.Alias();
            var alias2 = TestUtil.Alias();

            var member = tokenIO.CreateMember(alias1);

            member.AddAlias(alias2);
            CollectionAssert.AreEquivalent(member.Aliases(), new[] {alias1, alias2});

            member.RemoveAlias(alias2);
            CollectionAssert.AreEqual(member.Aliases(), new[] {alias1});
        }

        [Test]
        public void AliasDoesNotExist()
        {
            var alias = TestUtil.Alias();
            Assert.False(tokenIO.AliasExists(alias));
        }

        [Test]
        public void AliasExists()
        {
            var alias = TestUtil.Alias();
            tokenIO.CreateMember(alias);
            Assert.True(tokenIO.AliasExists(alias));
        }

        [Test]
        public void Recovery()
        {
            var alias = TestUtil.Alias();
            var member = tokenIO.CreateMember(alias);
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
            CollectionAssert.AreEqual(recovered.Aliases(), new[] {alias});
        }

        [Test]
        public void Recovery_withSecondaryAgent()
        {
            var alias = TestUtil.Alias();
            var member = tokenIO.CreateMember(alias);
            var memberId = member.MemberId();
            var primaryAgentId = member.GetDefaultAgent();
            var secondaryAgent = tokenIO.CreateMember(TestUtil.Alias());
            var unusedSecondaryAgent = tokenIO.CreateMember(TestUtil.Alias());
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
            CollectionAssert.AreEqual(recovered.Aliases(), new[] {alias});
        }
    }
}
