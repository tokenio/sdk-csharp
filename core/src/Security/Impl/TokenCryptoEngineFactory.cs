namespace Tokenio.Security
{
    public class TokenCryptoEngineFactory : ICryptoEngineFactory
    {
        private readonly IKeyStore keyStore;

        public TokenCryptoEngineFactory(IKeyStore keyStore) {
            this.keyStore = keyStore;
        }
        
        public ICryptoEngine Create(string memberId)
        {
            return new TokenCryptoEngine(memberId, keyStore);
        }
    }
}
