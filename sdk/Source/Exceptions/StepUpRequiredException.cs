using System;

namespace sdk.Exceptions
{
    public class StepUpRequiredException : Exception
    {
        public StepUpRequiredException(string message) : base(message)
        {
        }
    }
}
