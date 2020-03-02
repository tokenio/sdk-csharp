using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Tokenio.Proto.Common.SecurityProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;


namespace Tokenio.Security
{
    /// <summary>
    /// Token implementation of the {@link CryptoEngine}. The keys are persisted
    /// int he provided storage
    /// </summary>
    public class TokenCryptoEngine : ICryptoEngine
    {
        private readonly IKeyStore keys;
        private readonly string memberId;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Tokenio.Security.TokenCryptoEngine"/> class.
        /// </summary>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="keys">Keys.</param>
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

        public Key GenerateKey(Level level, long expiresAtMs)
        {
            var generator = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
            generator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
            var keyPair = generator.GenerateKeyPair().ParseEd25519KeyPair(level, expiresAtMs);
            keys.Put(memberId, keyPair);
            return keyPair.ToKey();

        }

        public ISigner CreateSigner(Level level)
        {
            var keyPair = keys.GetByLevel(memberId, level);
            return new Ed25519Signer(keyPair.Id, keyPair.PrivateKey);
        }

        public ISigner CreateSigner(string keyId)
        {
            var keyPair = keys.GetById(memberId, keyId);
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
                publicKeys.Add(secretKey.ToKey());
            }
            return publicKeys;
        }




    }
}
