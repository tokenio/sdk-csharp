using System;

namespace Tokenio.Exceptions
{   
    /// <summary>
    /// Invalid state exception.
    /// </summary>
    public class InvalidStateException : Exception
    {
        public InvalidStateException(string csrfToken) 
            : base($"CSRF token {csrfToken} does not match CSRF token in state (hashed)")
        {
        }
    }
}
