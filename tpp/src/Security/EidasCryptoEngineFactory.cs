using System;
using Tokenio.Security;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Tpp.Security
{
    public class EidasCryptoEngineFactory: ICryptoEngineFactory
    {
        private readonly IEidasKeyStore keyStore;

        public EidasCryptoEngineFactory(IEidasKeyStore keyStore)
        {
            this.keyStore = keyStore;
        }

        public ICryptoEngine Create(string memberId)
        {
            return new TokenCryptoEngine(memberId, keyStore, Algorithm.Rs256);
        }
    }
}
