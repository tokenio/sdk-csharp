using System;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Exceptions
{
    public class CryptoKeyNotFoundException : Exception
    {
        public CryptoKeyNotFoundException(string message) : base(message)
        {
        }

        public CryptoKeyNotFoundException(Level level) : base("Key not found: " + level)
        {
        }
    }
}
