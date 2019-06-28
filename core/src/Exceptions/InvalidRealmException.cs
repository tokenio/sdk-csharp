using System;

namespace Tokenio.Exceptions
{
    public class InvalidRealmException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Tokenio.Exceptions.InvalidRealmException"/> class.
        /// </summary>
        /// <param name="actual">Actual.</param>
        /// <param name="expected">Expected.</param>
        public InvalidRealmException(string actual, string expected) 
            : base($"Invalid realm {actual}; expected {expected}")
        {
        }
    }
}
