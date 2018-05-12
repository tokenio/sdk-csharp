using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sodium;
using Tokenio;
using Tokenio.Security;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;
using KeyPair = Tokenio.Security.KeyPair;

namespace Test.Security
{
    [TestFixture]
    public class UnsecuredFileSystemKeyStoreTest
    {
        private readonly string directory = "./testKeys";
        
        private readonly string memberId1 = Util.Nonce() + ":" + Util.Nonce();
        private readonly string memberId2 = Util.Nonce() + ":" + Util.Nonce();
        
        private readonly KeyPair privileged = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
        private readonly KeyPair standard = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Standard);
        private readonly KeyPair lowOld = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Low);
        private readonly KeyPair lowNew = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Low);

        [SetUp]
        public void Setup()
        {
            var keyStore = new UnsecuredFileSystemKeyStore(directory);
            keyStore.Put(memberId1, standard);
            keyStore.Put(memberId1, lowOld);
            keyStore.Put(memberId1, lowNew);
            keyStore.Put(memberId2, privileged);

            Assert.AreEqual(standard, keyStore.GetByLevel(memberId1, Standard));
            Assert.AreEqual(lowNew, keyStore.GetByLevel(memberId1, Low));
            CollectionAssert.AreEquivalent(
                keyStore.KeyList(memberId1),
                new List<KeyPair> {standard, lowNew, lowOld});

            Assert.AreEqual(privileged, keyStore.GetByLevel(memberId2, Privileged));
            Assert.AreEqual(privileged, keyStore.GetById(memberId2, privileged.Id));
        }

        [Test]
        public void ReadFromFile()
        {
            var keyStore = new UnsecuredFileSystemKeyStore(directory);

            Assert.AreEqual(standard, keyStore.GetByLevel(memberId1, Standard));
            Assert.AreEqual(lowNew, keyStore.GetByLevel(memberId1, Low));
            CollectionAssert.AreEquivalent(
                keyStore.KeyList(memberId1),
                new List<KeyPair> {standard, lowNew, lowOld});

            Assert.AreEqual(privileged, keyStore.GetByLevel(memberId2, Privileged));
            Assert.AreEqual(privileged, keyStore.GetById(memberId2, privileged.Id));
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(directory, true);
        }
    }
}
