using System;
using Io.Token.Proto.Common.Token;

namespace sdk.Exceptions
{
    public class TransferTokenException : Exception
    {
        public TransferTokenException(TransferTokenStatus status)
            : base("Failed to create token: " + status)
        {
            Status = status;
        }

        public TransferTokenStatus Status { get; }
    }
}
