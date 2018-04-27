using Sodium;
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
            var keyPair = PublicKeyAuth.GenerateKeyPair().ToKeyPair(level);
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
    }
}
