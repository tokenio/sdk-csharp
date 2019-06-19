using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Collections.Generic;
using Tokenio.Proto.Common.SecurityProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
namespace Tokenio.Security
{
    public class TokenCryptoEngine : ICryptoEngine
    {
        private readonly IKeyStore keys;
        private readonly string memberId;

        public TokenCryptoEngine(string memberId, IKeyStore keys)
        {
            this.keys = keys;
            this.memberId = memberId;
        }

        public Key GenerateKey(Level level)
        {
            var generator = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
            generator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
            var keyPair = generator.GenerateKeyPair().ParseEd25519KeyPair(level);
            keys.Put(memberId, keyPair);
            return keyPair.ToKey();
        }

        public ISigner CreateSigner(Level level)
        {
            var keyPair = keys.GetByLevel(memberId, level);
            return new Ed25519Signer(keyPair.Id, keyPair.PrivateKey);
        }

        public IVerifier CreateVerifier(string keyId)
        {
            var keyPair = keys.GetById(memberId, keyId);
            return new Ed25519Veifier(keyPair.PublicKey);
        }

        public IList<Key> GetPublicKeys()
        {
            IList<Key> publicKeys = new List<Key>();
            IList<KeyPair> secretKeys = keys.KeyList(memberId);
            foreach (KeyPair secretKey in secretKeys)
            {
                publicKeys.Add(ToPublicKey(secretKey));
            }
            return publicKeys;
        }

        private Key ToPublicKey(KeyPair secretKey)
        {
            return new Key
            {
                Id = secretKey.Id,
                Algorithm = secretKey.Algorithm,
                Level = secretKey.Level,
                PublicKey = secretKey.PublicKey.ToString()
            };
        }
    }
}
