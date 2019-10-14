using System;

namespace Tokenio.Exceptions {
    /// <summary>
    /// Invalid token request query.
    /// </summary>
    public class InvalidTokenRequestQuery : Exception {
        public InvalidTokenRequestQuery() : base("Invalid or missing parameters in token request query.") { }
    }
}
