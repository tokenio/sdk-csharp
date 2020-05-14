using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security
{
    /// <summary>
    /// Creates {@link CryptoEngine} instances bound to a given member id.
    /// Uses a provided key store to persist keys.
    /// </summary>
    public class TokenCryptoEngineFactory : ICryptoEngineFactory
    {
        private readonly Algorithm cryptoType;
        private readonly IKeyStore keyStore;

        /// <summary>
        /// Creates a new instance of the factory that uses supplied store
        /// to persist the keys.
        /// </summary>
        /// <param name="keyStore">Key store.</param>
        public TokenCryptoEngineFactory(IKeyStore keyStore, Algorithm cryptoType = Algorithm.Ed25519)
        {
            this.keyStore = keyStore;
            this.cryptoType = cryptoType;
        }

        /// <summary>
        /// Creates a new {@link CryptoEngine} for the given member.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="memberId">Member identifier.</param>
        public ICryptoEngine Create(string memberId)
        {
            return new TokenCryptoEngine(memberId, keyStore, cryptoType);
        }
    }
}
