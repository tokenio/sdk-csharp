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
        private static readonly TokenClient tokenClient = NewSdkInstance();

        private Member member1;
        private Member member2;
        private Member member3;

        [SetUp]
        public void Init()
        {
            member1 = tokenClient.CreateMemberBlocking(Alias());
            member2 = tokenClient.CreateMemberBlocking(Alias());
            member3 = tokenClient.CreateMemberBlocking(Alias());
        }

        [Test]
        public void AddAndGetTrustedBeneficiary()
        {
            member1.AddTrustedBeneficiaryBlocking(member2.MemberId());
            CollectionAssert.AreEquivalent(
                new List<string> {member2.MemberId()},
                member1.GetTrustedBeneficiariesBlocking()
                    .Select(a => a.Payload.MemberId));

            member1.AddTrustedBeneficiaryBlocking(member3.MemberId());
            CollectionAssert.AreEquivalent(
                new List<string> {member2.MemberId(), member3.MemberId()},
                member1.GetTrustedBeneficiariesBlocking()
                    .Select(a => a.Payload.MemberId));
        }

        [Test]
        public void RemoveTrustedBeneficiary()
        {
            member1.AddTrustedBeneficiaryBlocking(member2.MemberId());
            member1.AddTrustedBeneficiaryBlocking(member3.MemberId());
            CollectionAssert.AreEquivalent(
                new List<string> {member2.MemberId(), member3.MemberId()},
                member1.GetTrustedBeneficiariesBlocking()
                    .Select(a => a.Payload.MemberId));

            member1.RemoveTrustedBeneficiaryBlocking(member3.MemberId());
            CollectionAssert.AreEquivalent(
                new List<string> {member2.MemberId()},
                member1.GetTrustedBeneficiariesBlocking()
                   .Select(a => a.Payload.MemberId));

            member1.RemoveTrustedBeneficiaryBlocking(member2.MemberId());
            Assert.IsEmpty(member1.GetTrustedBeneficiariesBlocking());
        }
    }
}
