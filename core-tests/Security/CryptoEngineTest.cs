using System.Security.Cryptography;
using Tokenio.Exceptions;
using Xunit;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using Tokenio.Utils;
using Tokenio.Security.Crypto;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Test.Security
{
    public class CryptoEngineTest
    {
        private string memberId;
        private IKeyStore keyStore;
        private ICryptoEngine cryptoEngine;

        public CryptoEngineTest()
        {
            memberId = Util.Nonce();
            keyStore = new InMemoryKeyStore();
            cryptoEngine = new TokenCryptoEngine(memberId, keyStore);
        }

        [Fact]
        public void VerifierTest()
        {
            var signature = "tPmCXbpIf-lR2sOJrlB3wviI-mybLwKomo6Vh3Lxaf9RmS7FDiL5zdDxa8m5JvoVBMW4MnqHn5zUaKecESjjBQ";
            var payload = "{\"type\":\"EMAIL\",\"value\":\"123\"}";
            var verifier = new Ed25519Veifier("ypQFEgdQe-E8u1dtpmAhAE0EoaGdvP5lNc0P4wgY2DA");
            verifier.Verify(payload, signature);
        }

        [Fact]
        public void SignAndVerify_string()
        {
            cryptoEngine.GenerateKey(Privileged);
            var signer = cryptoEngine.CreateSigner(Privileged);
            var payload = Util.Nonce();
            var signature = signer.Sign(payload);
            var verifier = cryptoEngine.CreateVerifier(signer.GetKeyId());
            verifier.Verify(payload, signature);
        }

        [Fact]
        public void SignAndVerify_protobuf()
        {
            cryptoEngine.GenerateKey(Privileged);
            var signer = cryptoEngine.CreateSigner(Privileged);
            var payload = new Alias{Value = "bob@token.io", Type = Email};
            var signature = signer.Sign(payload);
            var verifier = cryptoEngine.CreateVerifier(signer.GetKeyId());
            verifier.Verify(payload, signature);
        }

        [Fact]
        public void WrongKey()
        {
            cryptoEngine.GenerateKey(Privileged);
            var signer = cryptoEngine.CreateSigner(Privileged);
            var newKey = cryptoEngine.GenerateKey(Privileged);
            var payload = Util.Nonce();
            var signature = signer.Sign(payload);
            var verifier = cryptoEngine.CreateVerifier(newKey.Id);
            Assert.Throws<CryptographicException>(() => verifier.Verify(payload, signature));
        }

        [Fact]
        public void useOldKey()
        {
            var oldKey = cryptoEngine.GenerateKey(Privileged);
            cryptoEngine.GenerateKey(Privileged);
            var signer = cryptoEngine.CreateSigner(oldKey.Id);
            var payload = Util.Nonce();
            var signature = signer.Sign(payload);
            var verifier = cryptoEngine.CreateVerifier(oldKey.Id);
            verifier.Verify(payload, signature);
        }

        [Fact]
        public void CreateCryptoEngine_cryptoType()
        {
            IKeyStore keyStore = new InMemoryKeyStore();
            ICryptoEngine cryptoEngineDefault = new TokenCryptoEngine(Util.Nonce(), keyStore);
            cryptoEngineDefault.GenerateKey(Low);
            // default crypto type
            Assert.Equal(Algorithm.Ed25519, cryptoEngineDefault.GetPublicKeys()[0].Algorithm);

            // RSA crypto type
            ICryptoEngine cryptoEngineRsa = new TokenCryptoEngine(Util.Nonce(), keyStore, Algorithm.Rs256);
            cryptoEngineRsa.GenerateKey(Low);
            Assert.Equal(Algorithm.Rs256, cryptoEngineRsa.GetPublicKeys()[0].Algorithm);


            // InvalidAlgo crypto type
            ICryptoEngine cryptoEngineInvalidAlgo = new TokenCryptoEngine(
                Util.Nonce(),
                keyStore,
                Algorithm.InvalidAlgorithm);
            cryptoEngineInvalidAlgo.GenerateKey(Low);
            Assert.Equal(Algorithm.InvalidAlgorithm, cryptoEngineInvalidAlgo.GetPublicKeys()[0].Algorithm);
        }

        [Fact]
        public void CreateSigner_forMinLeve()
        {
            IKeyStore keyStore = new InMemoryKeyStore();
            ICryptoEngine cryptoEngine = new TokenCryptoEngine("member-id", keyStore);

            Assert.Throws<CryptoKeyNotFoundException>(() => cryptoEngine.CreateSignerForLevelAtLeast(Low));
            var privileged = cryptoEngine.GenerateKey(Privileged);
            Assert.Equal(
                privileged.Id,
                cryptoEngine.CreateSignerForLevelAtLeast(Low)
                    .GetKeyId());
            Assert.Equal(
                privileged.Id,
                cryptoEngine.CreateSignerForLevelAtLeast(Standard)
                    .GetKeyId());
            Assert.Equal(
                privileged.Id,
                cryptoEngine.CreateSignerForLevelAtLeast(Privileged)
                    .GetKeyId());

            var low = cryptoEngine.GenerateKey(Low);
            Assert.Equal(
                cryptoEngine.CreateSignerForLevelAtLeast(Low)
                    .GetKeyId(),
                low.Id);
        }
    }
}
