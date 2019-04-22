using System;
using Google.Protobuf;
using NUnit.Framework;
using Tokenio;
using static Test.TestUtil;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;

namespace Test
{
    [TestFixture]
    public class BlobTest
    {
        private static readonly string FILENAME = "file.json";
        private static readonly string FILETYPE = "application/json";

        private static readonly TokenClient tokenClient = NewSdkInstance();

        private Member member;

        [SetUp]
        public void Init()
        {
            member = tokenClient.CreateMemberBlocking(Alias());
        }

        [Test]
        public void CheckHash()
        {
            var randomData = new byte[100];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlobBlocking(member.MemberId(), FILETYPE, FILENAME, randomData);
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

            var attachment = member.CreateBlobBlocking(member.MemberId(), FILETYPE, FILENAME, randomData);
            Assert.AreEqual(attachment.Name, FILENAME);
            Assert.AreEqual(attachment.Type, FILETYPE);
            Assert.Greater(attachment.BlobId.Length, 5);
        }

        [Test]
        public void CreateIdempotent()
        {
            var randomData = new byte[100];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlobBlocking(member.MemberId(), FILETYPE, FILENAME, randomData);
            var attachment2 = member.CreateBlobBlocking(member.MemberId(), FILETYPE, FILENAME, randomData);

            Assert.AreEqual(attachment, attachment2);
        }

        [Test]
        public void Get()
        {
            var randomData = new byte[100];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlobBlocking(member.MemberId(), FILETYPE, FILENAME, randomData);
            var blob = member.GetBlobBlocking(attachment.BlobId);

            Assert.AreEqual(attachment.BlobId, blob.Id);
            Assert.AreEqual(randomData, blob.Payload.Data.ToByteArray());
            Assert.AreEqual(member.MemberId(), blob.Payload.OwnerId);
        }

        [Test]
        public void EmptyData()
        {
            var randomData = new byte[0];

            var attachment = member.CreateBlobBlocking(member.MemberId(), FILETYPE, FILENAME, randomData);
            var blob = member.GetBlobBlocking(attachment.BlobId);

            Assert.AreEqual(attachment.BlobId, blob.Id);
            Assert.AreEqual(randomData, blob.Payload.Data.ToByteArray());
            Assert.AreEqual(member.MemberId(), blob.Payload.OwnerId);
        }

        [Test]
        public void MediumSize()
        {
            var randomData = new byte[50000];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlobBlocking(member.MemberId(), FILETYPE, FILENAME, randomData);
            var blob = member.GetBlobBlocking(attachment.BlobId);

            Assert.AreEqual(attachment.BlobId, blob.Id);
            Assert.AreEqual(randomData, blob.Payload.Data.ToByteArray());
            Assert.AreEqual(member.MemberId(), blob.Payload.OwnerId);
        }

        [Test]
        public void PublicAccess()
        {
            var randomData = new byte[50];
            new Random().NextBytes(randomData);

            var attachment = member.CreateBlobBlocking(member.MemberId(), FILETYPE, FILENAME, randomData, AccessMode.Public);
            var otherMember = tokenClient.CreateMemberBlocking();
            var blob1 = member.GetBlobBlocking(attachment.BlobId);
            var blob2 = otherMember.GetBlobBlocking(attachment.BlobId);
            Assert.AreEqual(blob1, blob2);
        }
    }
}
