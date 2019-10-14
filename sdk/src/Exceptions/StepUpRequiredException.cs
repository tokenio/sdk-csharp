using System;

namespace Tokenio.Exceptions {
    public class StepUpRequiredException : Exception {
        public StepUpRequiredException(string message) : base(message) { }
    }
}
