using System;
using Google.Protobuf;
using NUnit.Framework;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using static Test.TestUtil;
using static Tokenio.Proto.Common.MemberProtos.ProfilePictureSize;

namespace Test
{
    [TestFixture]
    public class ProfileTest
    {
        private static readonly TokenClient tokenClient = NewSdkInstance();
        
        private Tokenio.Member member;

        [SetUp]
        public void Init()
        {
            member = tokenClient.CreateMemberBlocking(Alias());
        }
        
        [Test]
        public void SetProfileBlocking()
        {
            var inProfile = new Profile
            {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };
            var backProfile = member.SetProfileBlocking(inProfile);
            var outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.AreEqual(inProfile, backProfile);
            Assert.AreEqual(backProfile, outProfile);
        }

        [Test]
        public void UpdateProfile()
        {
            var firstProfile = new Profile
            {
                DisplayNameFirst = "Katy",
                DisplayNameLast = "Hudson"
            };
            var backProfile = member.SetProfileBlocking(firstProfile);
            var outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.AreEqual(backProfile, outProfile);
            
            var secondProfile = new Profile
            {
                DisplayNameFirst = "Katy"
            };
            backProfile = member.SetProfileBlocking(secondProfile);
            outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.AreEqual(backProfile, outProfile);
        }

        [Test]
        public void ReadProfile_notYours()
        {
            var inProfile = new Profile
            {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };
            member.SetProfileBlocking(inProfile);

            var otherMember = tokenClient.CreateMemberBlocking();
            var outProfile = otherMember.GetProfileBlocking(member.MemberId());
            Assert.AreEqual(inProfile, outProfile);
        }

        [Test]
        public void GetProfilePictureBlocking()
        {
            // "The tiniest gif ever" , a 1x1 gif
            // http://probablyprogramming.com/2009/03/15/the-tiniest-gif-ever
            var tinyGif = Convert.FromBase64String("R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");

            member.SetProfilePictureBlocking("image/gif", tinyGif);

            var otherMember = tokenClient.CreateMemberBlocking();
            var blob = otherMember.GetProfilePictureBlocking(member.MemberId(), Original);
            var tinyGifString = ByteString.CopyFrom(tinyGif);
            Assert.AreEqual(tinyGifString, blob.Payload.Data);

            // Because our example picture is so small, asking for a "small" version
            // gets us the original
            var sameBlob = otherMember.GetProfilePictureBlocking(member.MemberId(), Small);
            Assert.AreEqual(tinyGifString, sameBlob.Payload.Data);
        }

        [Test]
        public void GetNoProfilePicture()
        {
            var blob = member.GetProfilePictureBlocking(member.MemberId(), Original);
            Assert.AreEqual(blob.Id, string.Empty);
        }

        [Test]
        public void GetPictureProfile()
        {
            // "The tiniest gif ever" , a 1x1 gif
            // http://probablyprogramming.com/2009/03/15/the-tiniest-gif-ever
            var tinyGif = Convert.FromBase64String("R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");

            var inProfile = new Profile
            {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };

            var otherMember = tokenClient.CreateMemberBlocking();

            member.SetProfileBlocking(inProfile);
            member.SetProfilePictureBlocking("image/gif", tinyGif);
            var outProfile = otherMember.GetProfileBlocking(member.MemberId());
            
            Assert.IsNotEmpty(outProfile.OriginalPictureId);
            Assert.AreEqual(outProfile.DisplayNameFirst, inProfile.DisplayNameFirst);
            Assert.AreEqual(outProfile.DisplayNameLast, inProfile.DisplayNameLast);

            var tinyGifString = ByteString.CopyFrom(tinyGif);
            var blob = otherMember.GetBlobBlocking(outProfile.OriginalPictureId);
            Assert.AreEqual(tinyGifString, blob.Payload.Data);
        }
    }
}
