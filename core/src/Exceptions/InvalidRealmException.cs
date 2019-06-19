using System;

namespace Tokenio.Exceptions
{
    public class InvalidRealmException : Exception
    {
        public InvalidRealmException(string actual, string expected) 
            : base($"Invalid realm {actual}; expected {expected}")
        {
        }
    }
}
