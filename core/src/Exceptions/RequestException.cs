using System;
using Tokenio.Proto.Common.TransactionProtos;

namespace Tokenio.Exceptions {
    /// <summary>
    /// Generic request exception.
    /// </summary>
    public class RequestException : Exception {
        private readonly RequestStatus status;

        public RequestException(RequestStatus status) {
            this.status = status;
        }

        public RequestStatus GetStatus() {
            return status;
        }
    }
}
