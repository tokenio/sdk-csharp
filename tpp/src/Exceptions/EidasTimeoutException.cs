using System;
namespace Tokenio.Tpp.Exceptions
{
    public class EidasTimeoutException: Exception
    {
        private readonly string memberId;
        private readonly string verificationId;

        /// <summary>
        /// Create a timeout exception
        /// </summary>
        /// <param name="memberId">member id</param>
        /// <param name="verificationId">verification id</param>
        public EidasTimeoutException(string memberId, string verificationId)
            : base($"Eidas verification for member {memberId} has timed out. Verification ID: {verificationId}")
        {
            this.memberId = memberId;
            this.verificationId = verificationId;
        }

        public string GetMemberId()
        {
            return memberId;
        }

        public string GetVerificationId()
        {
            return verificationId;
        }
    }
}
