using System;

namespace sdk.Exceptions
{
    public class InvalidTokenRequestQuery : Exception
    {
        public InvalidTokenRequestQuery() : base ("Invalid or missing parameters in token request query.")
        {
        }
    }
}
