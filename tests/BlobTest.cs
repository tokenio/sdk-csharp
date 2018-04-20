using System;
using Google.Protobuf;
using NUnit.Framework;
using sdk;
using sdk.Api;
using static Io.Token.Proto.Common.Blob.Blob.Types;
using static tests.TestUtil;

namespace tests
{
    [TestFixture]
    public class BlobTest
    {
        private static readonly string FILENAME = "file.json";
        private static readonly string FILETYPE = "application/json";

        private static readonly TokenIO tokenIO = NewSdkInstance();

        private MemberSync member;

        [SetUp]
        public void Init()
        {
            member = tokenIO.CreateMember(Alias());
        }

        [Test]
        public void CheckHash()
        {
            var randomData = new byte[100];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlob(member.MemberId(), FILETYPE, FILENAME, randomData);
            var blobPayload = new Payload
            {
                Data = ByteString.CopyFrom(randomData),
                Name = FILENAME,
                Type = FILETYPE,
                OwnerId = member.MemberId()
            };

            var hash = Util.HashProto(blobPayload);
            StringAssert.Contains(hash, attachment.BlobId);
        }

        [Test]
        public void Create()
        {
            var randomData = new byte[100];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlob(member.MemberId(), FILETYPE, FILENAME, randomData);
            Assert.AreEqual(attachment.Name, FILENAME);
            Assert.AreEqual(attachment.Type, FILETYPE);
            Assert.Greater(attachment.BlobId.Length, 5);
        }

        [Test]
        public void CreateIdempotent()
        {
            var randomData = new byte[100];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlob(member.MemberId(), FILETYPE, FILENAME, randomData);
            var attachment2 = member.CreateBlob(member.MemberId(), FILETYPE, FILENAME, randomData);

            Assert.AreEqual(attachment, attachment2);
        }

        [Test]
        public void Get()
        {
            var randomData = new byte[100];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlob(member.MemberId(), FILETYPE, FILENAME, randomData);
            var blob = member.GetBlob(attachment.BlobId);

            Assert.AreEqual(attachment.BlobId, blob.Id);
            Assert.AreEqual(randomData, blob.Payload.Data.ToByteArray());
            Assert.AreEqual(member.MemberId(), blob.Payload.OwnerId);
        }

        [Test]
        public void EmptyData()
        {
            var randomData = new byte[0];

            var attachment = member.CreateBlob(member.MemberId(), FILETYPE, FILENAME, randomData);
            var blob = member.GetBlob(attachment.BlobId);

            Assert.AreEqual(attachment.BlobId, blob.Id);
            Assert.AreEqual(randomData, blob.Payload.Data.ToByteArray());
            Assert.AreEqual(member.MemberId(), blob.Payload.OwnerId);
        }

        [Test]
        public void MediumSize()
        {
            var randomData = new byte[50000];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlob(member.MemberId(), FILETYPE, FILENAME, randomData);
            var blob = member.GetBlob(attachment.BlobId);

            Assert.AreEqual(attachment.BlobId, blob.Id);
            Assert.AreEqual(randomData, blob.Payload.Data.ToByteArray());
            Assert.AreEqual(member.MemberId(), blob.Payload.OwnerId);
        }

        [Test]
        public void PublicAccess()
        {
            var randomData = new byte[50];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlob(member.MemberId(), FILETYPE, FILENAME, randomData, AccessMode.Public);
            var otherMember = tokenIO.CreateMember();
            var blob1 = member.GetBlob(attachment.BlobId);
            var blob2 = otherMember.GetBlob(attachment.BlobId);
            Assert.AreEqual(blob1, blob2);
        }
    }
}
