using System;

namespace sdk.Exceptions
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException(string csrfToken) 
            : base($"CSRF token {csrfToken} does not match CSRF token in state (hashed)")
        {
        }
    }
}
