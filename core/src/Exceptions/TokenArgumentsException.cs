using System;

namespace Tokenio.Exceptions
{
    /// <summary>
    ///  Thrown when the Token SDK version is no longer supported by the server. Any Token SDK callers
    /// are required to update the Token SDK to the latest version to continue.
    /// </summary>
    public class TokenArgumentsException : Exception
    {
        public TokenArgumentsException(string message) : base(message)
        {
        }
    }
}
