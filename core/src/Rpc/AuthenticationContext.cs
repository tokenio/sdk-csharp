using Tokenio.Proto.Common.SecurityProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Rpc
{
    /// <summary>
    /// Authentication context. Stores the values of On-Behalf-Of and Key-Level to be used for request
    /// authentication and signing.
    /// </summary>
    public class AuthenticationContext
    {
        private readonly string onBehalfOf;
        private readonly Level keyLevel = Level.Low;
        private readonly bool customerInitiated;
        private readonly CustomerTrackingMetadata customerTrackingMetadata = new CustomerTrackingMetadata();

        public AuthenticationContext(
            string onBehalfOf,
            Level keyLevel,
            bool customerInitiated,
            CustomerTrackingMetadata securityMetadata)
        {
            this.onBehalfOf = onBehalfOf;
            this.keyLevel = keyLevel;
            this.customerInitiated = customerInitiated;
            this.customerTrackingMetadata = securityMetadata;
        }

        public string OnBehalfOf
        {
            get => onBehalfOf;
        }

        public Level KeyLevel
        {
            get => keyLevel;
        }

        public bool CustomerInitiated
        {
            get => customerInitiated;
        }

        public CustomerTrackingMetadata CustomerTrackingMetadata
        {
            get => customerTrackingMetadata;
        }
    }
}
