using System.Collections.Generic;
using NUnit.Framework;
using sdk;
using sdk.Security;
using Sodium;
using static Io.Token.Proto.Common.Security.Key.Types.Level;
using KeyPair = sdk.Security.KeyPair;

namespace tests.Security
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
            keyStore.put(memberId, privileged);
            keyStore.put(memberId, standard);
            keyStore.put(memberId, low);
            Assert.AreEqual(privileged, keyStore.GetByLevel(memberId, Privileged));
            Assert.AreEqual(standard, keyStore.GetByLevel(memberId, Standard));
            Assert.AreEqual(low, keyStore.GetByLevel(memberId, Low));
        }

        [Test]
        public void GetById()
        {
            var key1 = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var key2 = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            keyStore.put(memberId, key1);
            keyStore.put(memberId, key2);
            Assert.AreEqual(key1, keyStore.GetById(memberId, key1.Id));
            Assert.AreEqual(key2, keyStore.GetById(memberId, key2.Id));
        }

        [Test]
        public void GetLatest()
        {
            var oldKey = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var newKey = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            keyStore.put(memberId, oldKey);
            keyStore.put(memberId, newKey);
            Assert.AreEqual(newKey, keyStore.GetByLevel(memberId, Privileged));
        }

        [Test]
        public void KeyList()
        {
            var privileged = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var standard = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Standard);
            var low = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Low);
            keyStore.put(memberId, privileged);
            keyStore.put(memberId, standard);
            keyStore.put(memberId, low);
            var keyList = new List<KeyPair> {privileged, standard, low};
            CollectionAssert.AreEquivalent(keyStore.KeyList(memberId), keyList);
        }

        [Test]
        public void DifferentMember()
        {
            var member2 = Util.Nonce();
            var key1 = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            var key2 = PublicKeyAuth.GenerateKeyPair().ToKeyPair(Privileged);
            keyStore.put(memberId, key1);
            keyStore.put(member2, key2);
            Assert.AreEqual(key1, keyStore.GetById(memberId, key1.Id));
            Assert.AreEqual(key2, keyStore.GetById(member2, key2.Id));
        }
    }
}
