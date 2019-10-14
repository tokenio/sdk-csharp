using System.Collections.Generic;
using Tokenio;
using Tokenio.Security;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;
using KeyPair = Tokenio.Security.KeyPair;

namespace Test.Security {
    public class InMemoryKeyStoreTest {
        private IKeyStore keyStore;
        private string memberId;

        public InMemoryKeyStoreTest() {
            keyStore = new InMemoryKeyStore();
            memberId = Util.Nonce();
        }

        [Fact]
        public void GetByLevel() {
            var privileged = TestUtil.GenerateKeyPair(Privileged);
            var standard = TestUtil.GenerateKeyPair(Standard);
            var low = TestUtil.GenerateKeyPair(Low);
            keyStore.Put(memberId, privileged);
            keyStore.Put(memberId, standard);
            keyStore.Put(memberId, low);
            Assert.Equal(privileged, keyStore.GetByLevel(memberId, Privileged));
            Assert.Equal(standard, keyStore.GetByLevel(memberId, Standard));
            Assert.Equal(low, keyStore.GetByLevel(memberId, Low));
        }

        [Fact]
        public void GetById() {
            var key1 = TestUtil.GenerateKeyPair(Privileged);
            var key2 = TestUtil.GenerateKeyPair(Privileged);
            keyStore.Put(memberId, key1);
            keyStore.Put(memberId, key2);
            Assert.Equal(key1, keyStore.GetById(memberId, key1.Id));
            Assert.Equal(key2, keyStore.GetById(memberId, key2.Id));
        }

        [Fact]
        public void GetLatest() {
            var oldKey = TestUtil.GenerateKeyPair(Privileged);
            var newKey = TestUtil.GenerateKeyPair(Privileged);
            keyStore.Put(memberId, oldKey);
            keyStore.Put(memberId, newKey);
            Assert.Equal(newKey, keyStore.GetByLevel(memberId, Privileged));
        }

        [Fact]
        public void KeyList() {
            var privileged = TestUtil.GenerateKeyPair(Privileged);
            var standard = TestUtil.GenerateKeyPair(Standard);
            var low = TestUtil.GenerateKeyPair(Low);
            keyStore.Put(memberId, privileged);
            keyStore.Put(memberId, standard);
            keyStore.Put(memberId, low);
            var keyList = new List<KeyPair> { privileged, standard, low };
            CollectionAssert.Equivalent(keyStore.KeyList(memberId), keyList);
        }

        [Fact]
        public void DifferentMember() {
            var member2 = Util.Nonce();
            var key1 = TestUtil.GenerateKeyPair(Privileged);
            var key2 = TestUtil.GenerateKeyPair(Privileged);
            keyStore.Put(memberId, key1);
            keyStore.Put(member2, key2);
            Assert.Equal(key1, keyStore.GetById(memberId, key1.Id));
            Assert.Equal(key2, keyStore.GetById(member2, key2.Id));
        }
    }
}
