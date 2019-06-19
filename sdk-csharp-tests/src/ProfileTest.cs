using System;
using TokenioTest.Common;
using Xunit;
using Tokenio.Proto.Common.MemberProtos;
using Member = Tokenio.User.Member;
using Tokenio.Proto.Common.BlobProtos;
using Google.Protobuf;

namespace TokenioTest  
{
    public class ProfileTest
    {
        public TokenUserRule rule = new TokenUserRule();
        public TokenTppRule tppRule = new TokenTppRule();

        [Fact]
        public void SetProfile()
        {
            Member member = rule.Member();
            Profile inProfile = new Profile
            {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };

            Profile backProfile = member.SetProfileBlocking(inProfile);
            Profile outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.Equal(inProfile, backProfile);
            Assert.Equal(inProfile, outProfile);
        }

        [Fact]
        public void UpdateProfile()
        {
            Member member = rule.Member();
            Profile firstProfile = new Profile
            {
                DisplayNameFirst = "Katy",
                DisplayNameLast = "Hudson"
            };
            Profile backProfile = member.SetProfileBlocking(firstProfile);
            Profile outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.Equal(firstProfile, backProfile);
            Assert.Equal(firstProfile, outProfile);

            Profile secondProfile = new Profile
            {
                DisplayNameFirst = "Katy",
                DisplayNameLast = "Perry"
            };
            backProfile = member.SetProfileBlocking(secondProfile);
            outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.Equal(secondProfile, backProfile);
            Assert.Equal(secondProfile, outProfile);
        }

        [Fact]
        public void UpdateToMononym()
        {
            Member member = rule.Member();
            Profile firstProfile = new Profile
            {
                DisplayNameFirst = "Paul",
                DisplayNameLast = "Hewson"
            };
            Profile backProfile = member.SetProfileBlocking(firstProfile);
            Profile outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.Equal(firstProfile, backProfile);
            Assert.Equal(firstProfile, outProfile);

            Profile secondProfile = new Profile
            {
                DisplayNameFirst = "Bono",
            };
            backProfile = member.SetProfileBlocking(secondProfile);
            outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.Equal(secondProfile, backProfile);
            Assert.Equal(secondProfile, outProfile);
        }

        [Fact]
        public void ReadProfile_notYours()
        {
            Member member = rule.Member();
            Profile inProfile = new Profile
            {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };
            member.SetProfileBlocking(inProfile);

            Member otherMember = rule.Member();
            Profile outProfile = otherMember.GetProfileBlocking(member.MemberId());
            Assert.Equal(inProfile, outProfile);
        }

        [Fact]
        public void SetProfilePicture()
        {
            byte[] tinyGif = Convert.FromBase64String("R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");

            Member member = rule.Member();
            member.SetProfilePicture("image/gif", tinyGif);

            Member otherMember = rule.Member();
            Blob blob = otherMember.GetProfilePictureBlocking(member.MemberId(), ProfilePictureSize.Original);
            ByteString tinyGifString = ByteString.CopyFrom(tinyGif);

            Assert.Equal(blob.Payload.Data, tinyGifString);

            Blob sameBlob = otherMember.GetProfilePictureBlocking(member.MemberId(), ProfilePictureSize.Small);
            Assert.Equal(sameBlob.Payload.Data, tinyGifString);
        }


        [Fact]
        public void GetNoProfilePicture()
        {
            Member member = rule.Member();
            Blob blob = member.GetProfilePictureBlocking(member.MemberId(), ProfilePictureSize.Original);
            Assert.Equal(blob, new Blob());
        }

        [Fact]
        public void GetProfilePicture()
        {
            byte[] tinyGif = Convert.FromBase64String("R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");
            Profile inProfile = new Profile
            {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };

            Member member = rule.Member();
            Tokenio.Tpp.Member otherMember = tppRule.Member();

            member.SetProfileBlocking(inProfile);
            member.SetProfilePictureBlocking("image/gif", tinyGif);
            Profile outProfile = otherMember.GetProfileBlocking(member.MemberId());

            Assert.NotEmpty(outProfile.OriginalPictureId);
            Assert.Equal(inProfile.DisplayNameFirst, outProfile.DisplayNameFirst);
            Assert.Equal(inProfile.DisplayNameLast, outProfile.DisplayNameLast);

            ByteString tinyGifString = ByteString.CopyFrom(tinyGif);
            Blob blob = otherMember.GetBlobBlocking(outProfile.OriginalPictureId);
            Assert.Equal(blob.Payload.Data, tinyGifString);
        }

    }
}
