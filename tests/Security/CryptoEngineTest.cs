using System.Security.Cryptography;
using Io.Token.Proto.Common.Alias;
using NUnit.Framework;
using sdk;
using sdk.Security;
using static Io.Token.Proto.Common.Alias.Alias.Types.Type;
using static Io.Token.Proto.Common.Security.Key.Types.Level;

namespace tests.Security
{
    [TestFixture]
    public class CryptoEngineTest
    {
        private string memberId;
        private IKeyStore keyStore;
        private ICryptoEngine cryptoEngine;

        [SetUp]
        public void Setup()
        {
            memberId = Util.Nonce();
            keyStore = new InMemoryKeyStore();
            cryptoEngine = new TokenCryptoEngine(memberId, keyStore);
        }
        
        [Test]
        public void VerifierTest()
        {
            var signature = "tPmCXbpIf-lR2sOJrlB3wviI-mybLwKomo6Vh3Lxaf9RmS7FDiL5zdDxa8m5JvoVBMW4MnqHn5zUaKecESjjBQ";
            var payload = "{\"type\":\"EMAIL\",\"value\":\"123\"}";
            var verifier = new Ed25519Veifier("ypQFEgdQe-E8u1dtpmAhAE0EoaGdvP5lNc0P4wgY2DA");
            verifier.Verify(payload, signature);
        }

        [Test]
        public void SignAndVerify_string()
        {
            cryptoEngine.GenerateKey(Privileged);
            var signer = cryptoEngine.CreateSigner(Privileged);
            var payload = Util.Nonce();
            var signature = signer.Sign(payload);
            var verifier = cryptoEngine.CreateVerifier(signer.GetKeyId());
            verifier.Verify(payload, signature);
        }
        
        [Test]
        public void SignAndVerify_protobuf()
        {
            cryptoEngine.GenerateKey(Privileged);
            var signer = cryptoEngine.CreateSigner(Privileged);
            var payload = new Alias{Value = "bob@token.io", Type = Email};
            var signature = signer.Sign(payload);
            var verifier = cryptoEngine.CreateVerifier(signer.GetKeyId());
            verifier.Verify(payload, signature);
        }

        [Test]
        public void WrongKey()
        {
            var oldKey = cryptoEngine.GenerateKey(Privileged);
            cryptoEngine.GenerateKey(Privileged);
            var signer = cryptoEngine.CreateSigner(Privileged);
            var payload = Util.Nonce();
            var signature = signer.Sign(payload);
            var verifier = cryptoEngine.CreateVerifier(oldKey.Id);
            Assert.Throws<CryptographicException>(() => verifier.Verify(payload, signature));
        }
    }
}
