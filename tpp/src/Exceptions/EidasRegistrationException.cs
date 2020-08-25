using System;
using Tokenio.Proto.Common.EidasProtos;

namespace Tokenio.Tpp.Exceptions
{
    public class EidasRegistrationException: Exception
    {
        private readonly EidasVerificationStatus status;

        public EidasRegistrationException(EidasVerificationStatus status, string message)
            : base($"{status} : {message}")
        {
            this.status = status;
        }

        public EidasVerificationStatus GetStatus()
        {
            return status;
        }

        public static EidasRegistrationException RegistrationException(EidasVerificationStatus status, string message)
        {
            return new EidasRegistrationException(status, message);
        }
    }
}
