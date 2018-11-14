using System.Collections.Generic;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Tokenio;
using Tokenio.Security;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;
using KeyPair = Tokenio.Security.KeyPair;

namespace Test.Security
{
    [TestFixture]
    public class InMemoryKeyStoreTest
    {
        private IKeyStore keyStore;
        private string memberId;
        private IAsymmetricCipherKeyPairGenerator generator;

        [SetUp]
        public void Setup()
        {
            keyStore = new InMemoryKeyStore();
            memberId = Util.Nonce();
            generator = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
            generator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
        }

        [Test]
        public void GetByLevel()
        {
            var privileged = generator.GenerateKeyPair().ParseEd25519KeyPair(Privileged);
            var standard = generator.GenerateKeyPair().ParseEd25519KeyPair(Standard);
            var low = generator.GenerateKeyPair().ParseEd25519KeyPair(Low);
            keyStore.Put(memberId, privileged);
            keyStore.Put(memberId, standard);
            keyStore.Put(memberId, low);
            Assert.AreEqual(privileged, keyStore.GetByLevel(memberId, Privileged));
            Assert.AreEqual(standard, keyStore.GetByLevel(memberId, Standard));
            Assert.AreEqual(low, keyStore.GetByLevel(memberId, Low));
        }

        [Test]
        public void GetById()
        {
            var key1 = generator.GenerateKeyPair().ParseEd25519KeyPair(Privileged);
            var key2 = generator.GenerateKeyPair().ParseEd25519KeyPair(Privileged);
            keyStore.Put(memberId, key1);
            keyStore.Put(memberId, key2);
            Assert.AreEqual(key1, keyStore.GetById(memberId, key1.Id));
            Assert.AreEqual(key2, keyStore.GetById(memberId, key2.Id));
        }

        [Test]
        public void GetLatest()
        {
            var oldKey = generator.GenerateKeyPair().ParseEd25519KeyPair(Privileged);
            var newKey = generator.GenerateKeyPair().ParseEd25519KeyPair(Privileged);
            keyStore.Put(memberId, oldKey);
            keyStore.Put(memberId, newKey);
            Assert.AreEqual(newKey, keyStore.GetByLevel(memberId, Privileged));
        }

        [Test]
        public void KeyList()
        {
            var privileged = generator.GenerateKeyPair().ParseEd25519KeyPair(Privileged);
            var standard = generator.GenerateKeyPair().ParseEd25519KeyPair(Standard);
            var low = generator.GenerateKeyPair().ParseEd25519KeyPair(Low);
            keyStore.Put(memberId, privileged);
            keyStore.Put(memberId, standard);
            keyStore.Put(memberId, low);
            var keyList = new List<KeyPair> {privileged, standard, low};
            CollectionAssert.AreEquivalent(keyStore.KeyList(memberId), keyList);
        }

        [Test]
        public void DifferentMember()
        {
            var member2 = Util.Nonce();
            var key1 = generator.GenerateKeyPair().ParseEd25519KeyPair(Privileged);
            var key2 = generator.GenerateKeyPair().ParseEd25519KeyPair(Privileged);
            keyStore.Put(memberId, key1);
            keyStore.Put(member2, key2);
            Assert.AreEqual(key1, keyStore.GetById(memberId, key1.Id));
            Assert.AreEqual(key2, keyStore.GetById(member2, key2.Id));
        }
    }
}
