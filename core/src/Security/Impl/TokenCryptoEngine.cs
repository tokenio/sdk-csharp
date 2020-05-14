using System.Collections.Generic;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security.Crypto;
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
        public static readonly Algorithm DefaultCryptoType = Algorithm.Ed25519;
        public readonly Algorithm cryptoType;

        /// <summary>
        ///  Creates an instance of a crypto engine for the default crypto type (EDDSA).
        /// </summary>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="keys">Keys.</param>
        public TokenCryptoEngine(string memberId, IKeyStore keys) : this(memberId, keys, DefaultCryptoType)
        {
        }

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="memberId">member ID</param>
        /// <param name="keys">key store</param>
        /// <param name="cryptoType">algorithm</param>
        public TokenCryptoEngine(string memberId, IKeyStore keys, Algorithm cryptoType)
        {
            this.keys = keys;
            this.memberId = memberId;
            this.cryptoType = cryptoType;
        }

        public Key GenerateKey(Level level)
        {
            KeyPair keyPair = null;
            switch (cryptoType)
            {
            case Algorithm.Ed25519:
            {
                var generator = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
                generator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
                keyPair = generator.GenerateKeyPair()
                    .ParseEd25519KeyPair(level,cryptoType);
                break;
            }
            case Algorithm.Rs256:
            case Algorithm.InvalidAlgorithm:
            {
                var generator = GeneratorUtilities.GetKeyPairGenerator("RSA");
                generator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
                keyPair = generator.GenerateKeyPair()
                    .ParseRsaKeyPair(level,cryptoType);
                break;
            }
            }
            keys.Put(memberId, keyPair);
            return keyPair.ToKey();
        }

        public Key GenerateKey(Level level, long expiresAtMs)
        {
            KeyPair keyPair = null;
            switch (cryptoType)
            {
            case Algorithm.Ed25519:
            {
                var generator = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
                generator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
                keyPair = generator.GenerateKeyPair()
                    .ParseEd25519KeyPair(level, expiresAtMs, cryptoType);
                break;
            }
            case Algorithm.Rs256:
            case Algorithm.InvalidAlgorithm:
            {
                var generator = GeneratorUtilities.GetKeyPairGenerator("RSA");
                generator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
                keyPair = generator.GenerateKeyPair()
                    .ParseRsaKeyPair(level, expiresAtMs, cryptoType);
                break;
            }
            }
            keys.Put(memberId, keyPair);
            return keyPair.ToKey();
        }

        public ISigner CreateSigner(Level level)
        {
            ISigner signer = null;
            switch (cryptoType)
            {
            case Algorithm.Ed25519:
            {
                var keyPair = keys.GetByLevel(memberId, level);
                signer = new Ed25519Signer(keyPair.Id, keyPair.PrivateKey);
                break;
            }
            case Algorithm.Rs256:
            {
                var keyPair = keys.GetByLevel(memberId, level);
                signer = new Rs256Signer(keyPair.Id, keyPair.PrivateKey);
                break;
            }
            }
            return signer;
        }

        public ISigner CreateSigner(string keyId)
        {
            ISigner signer = null;
            switch (cryptoType)
            {
            case Algorithm.Ed25519:
            {
                var keyPair = keys.GetById(memberId, keyId);
                signer = new Ed25519Signer(keyPair.Id, keyPair.PrivateKey);
                break;
            }
            case Algorithm.Rs256:
            {
                var keyPair = keys.GetById(memberId, keyId);
                signer = new Rs256Signer(keyPair.Id, keyPair.PrivateKey);
                break;
            }
            }
            return signer;
        }

        public IVerifier CreateVerifier(string keyId)
        {
            IVerifier verifier = null;
            switch (cryptoType)
            {
            case Algorithm.Ed25519:
            {
                var keyPair = keys.GetById(memberId, keyId);
                verifier = new Ed25519Veifier(keyPair.PublicKey);
                break;
            }
            case Algorithm.Rs256:
            {
                var keyPair = keys.GetById(memberId, keyId);
                verifier = new Rs256Verifier(keyPair.PublicKey);
                break;
            }
            }
            return verifier;
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

        public ISigner CreateSignerForLevelAtLeast(Level minKeyLevel)
        {
            var keyLevel = minKeyLevel;
            try
            {
                return CreateSigner(keyLevel);
            }
            catch (CryptoKeyNotFoundException exception)
            {
                // try a key for the next level
                keyLevel = Level.Standard;
                try
                {
                    return CreateSigner(keyLevel);
                }
                catch (CryptoKeyNotFoundException expStandardLevel)
                {
                    keyLevel = Level.Privileged;
                    try
                    {
                        return CreateSigner(keyLevel);
                    }
                    catch (CryptoKeyNotFoundException expPrivilegedLevel)
                    {
                        throw new CryptoKeyNotFoundException(keyLevel);
                    }
                }
            }
        }
    }
}
