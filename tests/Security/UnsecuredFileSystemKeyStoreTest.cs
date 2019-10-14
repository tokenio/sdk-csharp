using System;
using System.Collections.Generic;
using System.IO;
using Tokenio;
using Tokenio.Security;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;
using KeyPair = Tokenio.Security.KeyPair;

namespace Test.Security {
    public class UnsecuredFileSystemKeyStoreTest : IDisposable {
        private readonly string directory = "./testKeys";

        private readonly string memberId1 = Util.Nonce() + ":" + Util.Nonce();
        private readonly string memberId2 = Util.Nonce() + ":" + Util.Nonce();

        private readonly KeyPair privileged = TestUtil.GenerateKeyPair(Privileged);
        private readonly KeyPair standard = TestUtil.GenerateKeyPair(Standard);
        private readonly KeyPair lowOld = TestUtil.GenerateKeyPair(Low);
        private readonly KeyPair lowNew = TestUtil.GenerateKeyPair(Low);

        public UnsecuredFileSystemKeyStoreTest() {
            var keyStore = new UnsecuredFileSystemKeyStore(directory);
            keyStore.Put(memberId1, standard);
            keyStore.Put(memberId1, lowOld);
            keyStore.Put(memberId1, lowNew);
            keyStore.Put(memberId2, privileged);

            Assert.Equal(standard, keyStore.GetByLevel(memberId1, Standard));
            Assert.Equal(lowNew, keyStore.GetByLevel(memberId1, Low));
            CollectionAssert.Equivalent(
                keyStore.KeyList(memberId1),
                new List<KeyPair> { standard, lowNew, lowOld });

            Assert.Equal(privileged, keyStore.GetByLevel(memberId2, Privileged));
            Assert.Equal(privileged, keyStore.GetById(memberId2, privileged.Id));
        }

        public void Dispose() {
            Directory.Delete(directory, true);
        }

        [Fact]
        public void ReadFromFile() {
            var keyStore = new UnsecuredFileSystemKeyStore(directory);

            Assert.Equal(standard, keyStore.GetByLevel(memberId1, Standard));
            Assert.Equal(lowNew, keyStore.GetByLevel(memberId1, Low));
            CollectionAssert.Equivalent(
                keyStore.KeyList(memberId1),
                new List<KeyPair> { standard, lowNew, lowOld });

            Assert.Equal(privileged, keyStore.GetByLevel(memberId2, Privileged));
            Assert.Equal(privileged, keyStore.GetById(memberId2, privileged.Id));
        }
    }
}
