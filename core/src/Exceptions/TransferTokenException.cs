using System;
using Tokenio.Proto.Common.TokenProtos;

namespace Tokenio.Exceptions {
    public class TransferTokenException : Exception {
        public TransferTokenException (TransferTokenStatus status) : base ("Failed to create token: " + status) {
            Status = status;
        }

        public TransferTokenStatus Status { get; }
    }
}