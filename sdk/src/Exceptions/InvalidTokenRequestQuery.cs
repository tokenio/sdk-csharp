using System;

namespace Tokenio.Exceptions
{
    public class InvalidTokenRequestQuery : Exception
    {
        public InvalidTokenRequestQuery() : base ("Invalid or missing parameters in token request query.")
        {
        }
    }
}
