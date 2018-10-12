using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tokenio;
using static Test.TestUtil;

namespace Test
{
    [TestFixture]
    public class TrustedBeneficiaryTest
    {
        private static readonly TokenIO tokenIO = NewSdkInstance();

        private MemberSync member1;
        private MemberSync member2;
        private MemberSync member3;

        [SetUp]
        public void Init()
        {
            member1 = tokenIO.CreateMember(Alias());
            member2 = tokenIO.CreateMember(Alias());
            member3 = tokenIO.CreateMember(Alias());
        }

        [Test]
        public void AddAndGetTrustedBeneficiary()
        {
            member1.AddTrustedBeneficiary(member2.MemberId());
            CollectionAssert.AreEquivalent(
                new List<string> {member2.MemberId()},
                member1.GetTrustedBeneficiaries()
                    .Select(a => a.Payload.MemberId));

            member1.AddTrustedBeneficiary(member3.MemberId());
            CollectionAssert.AreEquivalent(
                new List<string> {member2.MemberId(), member3.MemberId()},
                member1.GetTrustedBeneficiaries()
                    .Select(a => a.Payload.MemberId));
        }

        [Test]
        public void RemoveTrustedBeneficiary()
        {
            member1.AddTrustedBeneficiary(member2.MemberId());
            member1.AddTrustedBeneficiary(member3.MemberId());
            CollectionAssert.AreEquivalent(
                new List<string> {member2.MemberId(), member3.MemberId()},
                member1.GetTrustedBeneficiaries()
                    .Select(a => a.Payload.MemberId));

            member1.RemoveTrustedBeneficiary(member3.MemberId());
            CollectionAssert.AreEquivalent(
                new List<string> {member2.MemberId()},
                member1.GetTrustedBeneficiaries()
                   .Select(a => a.Payload.MemberId));

            member1.RemoveTrustedBeneficiary(member2.MemberId());
            Assert.IsEmpty(member1.GetTrustedBeneficiaries());
        }
    }
}
