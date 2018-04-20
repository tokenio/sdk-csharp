using System;

namespace sdk.Exceptions
{
    public class CryptoKeyNotFoundException : Exception
    {
        public CryptoKeyNotFoundException(string keyId) : base("Key not found: " + keyId)
        {
        }
    }
}
