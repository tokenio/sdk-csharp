using System;
using Google.Protobuf;
using Tokenio;
using Tokenio.Proto.Common.MemberProtos;
using Xunit;
using static Test.TestUtil;
using static Tokenio.Proto.Common.MemberProtos.ProfilePictureSize;

namespace Test {
    public class ProfileTest {
        private static readonly TokenClient tokenClient = NewSdkInstance();

        private Tokenio.Member member;

        public ProfileTest() {
            member = tokenClient.CreateMemberBlocking(Alias());
        }

        [Fact]
        public void SetProfileBlocking() {
            var inProfile = new Profile {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };
            var backProfile = member.SetProfileBlocking(inProfile);
            var outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.Equal(inProfile, backProfile);
            Assert.Equal(backProfile, outProfile);
        }

        [Fact]
        public void UpdateProfile() {
            var firstProfile = new Profile {
                DisplayNameFirst = "Katy",
                DisplayNameLast = "Hudson"
            };
            var backProfile = member.SetProfileBlocking(firstProfile);
            var outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.Equal(backProfile, outProfile);

            var secondProfile = new Profile {
                DisplayNameFirst = "Katy"
            };
            backProfile = member.SetProfileBlocking(secondProfile);
            outProfile = member.GetProfileBlocking(member.MemberId());
            Assert.Equal(backProfile, outProfile);
        }

        [Fact]
        public void ReadProfile_notYours() {
            var inProfile = new Profile {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };
            member.SetProfileBlocking(inProfile);

            var otherMember = tokenClient.CreateMemberBlocking();
            var outProfile = otherMember.GetProfileBlocking(member.MemberId());
            Assert.Equal(inProfile, outProfile);
        }

        [Fact]
        public void GetProfilePictureBlocking() {
            // "The tiniest gif ever" , a 1x1 gif
            // http://probablyprogramming.com/2009/03/15/the-tiniest-gif-ever
            var tinyGif = Convert.FromBase64String("R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");

            member.SetProfilePictureBlocking("image/gif", tinyGif);

            var otherMember = tokenClient.CreateMemberBlocking();
            var blob = otherMember.GetProfilePictureBlocking(member.MemberId(), Original);
            var tinyGifString = ByteString.CopyFrom(tinyGif);
            Assert.Equal(tinyGifString, blob.Payload.Data);

            // Because our example picture is so small, asking for a "small" version
            // gets us the original
            var sameBlob = otherMember.GetProfilePictureBlocking(member.MemberId(), Small);
            Assert.Equal(tinyGifString, sameBlob.Payload.Data);
        }

        [Fact]
        public void GetNoProfilePicture() {
            var blob = member.GetProfilePictureBlocking(member.MemberId(), Original);
            Assert.Equal(blob.Id, string.Empty);
        }

        [Fact]
        public void GetPictureProfile() {
            // "The tiniest gif ever" , a 1x1 gif
            // http://probablyprogramming.com/2009/03/15/the-tiniest-gif-ever
            var tinyGif = Convert.FromBase64String("R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");

            var inProfile = new Profile {
                DisplayNameFirst = "Tomás",
                DisplayNameLast = "de Aquino"
            };

            var otherMember = tokenClient.CreateMemberBlocking();

            member.SetProfileBlocking(inProfile);
            member.SetProfilePictureBlocking("image/gif", tinyGif);
            var outProfile = otherMember.GetProfileBlocking(member.MemberId());

            Assert.NotEmpty(outProfile.OriginalPictureId);
            Assert.Equal(outProfile.DisplayNameFirst, inProfile.DisplayNameFirst);
            Assert.Equal(outProfile.DisplayNameLast, inProfile.DisplayNameLast);

            var tinyGifString = ByteString.CopyFrom(tinyGif);
            var blob = otherMember.GetBlobBlocking(outProfile.OriginalPictureId);
            Assert.Equal(tinyGifString, blob.Payload.Data);
        }
    }
}
