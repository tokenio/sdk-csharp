using System;
using Google.Protobuf;
using Io.Token.Proto.Common.Member;
using NUnit.Framework;
using sdk;
using sdk.Api;
using static Io.Token.Proto.Common.Member.ProfilePictureSize;

namespace tests
{
    [TestFixture]
    public class ProfileTest
    {
        private static readonly TokenIO tokenIO = TestUtil.NewSdkInstance();
        
        private MemberSync member;

        [SetUp]
        public void Init()
        {
            member = tokenIO.CreateMember(TestUtil.Alias());
        }
        
        [Test]
        public void SetProfile()
        {
            var inProfile = new Profile
            {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };
            var backProfile = member.SetProfile(inProfile);
            var outProfile = member.GetProfile(member.MemberId());
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
            var backProfile = member.SetProfile(firstProfile);
            var outProfile = member.GetProfile(member.MemberId());
            Assert.AreEqual(backProfile, outProfile);
            
            var secondProfile = new Profile
            {
                DisplayNameFirst = "Katy"
            };
            backProfile = member.SetProfile(secondProfile);
            outProfile = member.GetProfile(member.MemberId());
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
            member.SetProfile(inProfile);

            var otherMember = tokenIO.CreateMember();
            var outProfile = otherMember.GetProfile(member.MemberId());
            Assert.AreEqual(inProfile, outProfile);
        }

        [Test]
        public void GetProfilePicture()
        {
            // "The tiniest gif ever" , a 1x1 gif
            // http://probablyprogramming.com/2009/03/15/the-tiniest-gif-ever
            var tinyGif = Convert.FromBase64String("R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");

            member.SetProfilePicture("image/gif", tinyGif);

            var otherMember = tokenIO.CreateMember();
            var blob = otherMember.GetProfilePicture(member.MemberId(), Original);
            var tinyGifString = ByteString.CopyFrom(tinyGif);
            Assert.AreEqual(tinyGifString, blob.Payload.Data);

            // Because our example picture is so small, asking for a "small" version
            // gets us the original
            var sameBlob = otherMember.GetProfilePicture(member.MemberId(), Small);
            Assert.AreEqual(tinyGifString, sameBlob.Payload.Data);
        }

        [Test]
        public void GetNoProfilePicture()
        {
            var blob = member.GetProfilePicture(member.MemberId(), Original);
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

            var otherMember = tokenIO.CreateMember();

            member.SetProfile(inProfile);
            member.SetProfilePicture("image/gif", tinyGif);
            var outProfile = otherMember.GetProfile(member.MemberId());
            
            Assert.IsNotEmpty(outProfile.OriginalPictureId);
            Assert.AreEqual(outProfile.DisplayNameFirst, inProfile.DisplayNameFirst);
            Assert.AreEqual(outProfile.DisplayNameLast, inProfile.DisplayNameLast);

            var tinyGifString = ByteString.CopyFrom(tinyGif);
            var blob = otherMember.GetBlob(outProfile.OriginalPictureId);
            Assert.AreEqual(tinyGifString, blob.Payload.Data);
        }
    }
}
