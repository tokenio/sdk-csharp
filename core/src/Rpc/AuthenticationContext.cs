using Tokenio.Proto.Common.SecurityProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Rpc
{
    public class AuthenticationContext
    {
        private readonly string onBehalfOf = null;
        private readonly Level keyLevel = Level.Low;
        private readonly bool customerInitiated = false;
        private readonly SecurityMetadata securityMetadata = new SecurityMetadata();

        public AuthenticationContext(
            string onBehalfOf,
            Level keyLevel,
            bool customerInitiated,
            SecurityMetadata securityMetadata)
        {
            this.onBehalfOf = onBehalfOf;
            this.keyLevel = keyLevel;
            this.customerInitiated = customerInitiated;
            this.securityMetadata = securityMetadata;
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

        public SecurityMetadata SecurityMetadata
        {
            get => securityMetadata;
        }
    }
}
