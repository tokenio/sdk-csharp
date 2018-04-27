using System.Collections.Generic;
using NUnit.Framework;
using Sodium;
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

        [SetUp]
        public void Setup()
        {
            keyStore = new InMemoryKeyStore();
            memberId = Util.Nonce();
        }

        [Test]
        public void GetByLevel()
        {
            var privileged = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var standard = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Standard);
            var low = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Low);
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
            var key1 = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var key2 = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            keyStore.Put(memberId, key1);
            keyStore.Put(memberId, key2);
            Assert.AreEqual(key1, keyStore.GetById(memberId, key1.Id));
            Assert.AreEqual(key2, keyStore.GetById(memberId, key2.Id));
        }

        [Test]
        public void GetLatest()
        {
            var oldKey = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var newKey = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            keyStore.Put(memberId, oldKey);
            keyStore.Put(memberId, newKey);
            Assert.AreEqual(newKey, keyStore.GetByLevel(memberId, Privileged));
        }

        [Test]
        public void KeyList()
        {
            var privileged = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var standard = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Standard);
            var low = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Low);
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
            var key1 = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var key2 = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            keyStore.Put(memberId, key1);
            keyStore.Put(member2, key2);
            Assert.AreEqual(key1, keyStore.GetById(memberId, key1.Id));
            Assert.AreEqual(key2, keyStore.GetById(member2, key2.Id));
        }
    }
}
