using Tokenio.Proto.Common.BankProtos;

namespace Tokenio.Tpp
{
    public class ExternalMetadata
    {
        private readonly OpenBankingStandard openBankingStandard;
        private readonly string consent;

        /// <summary>
        /// Instantiates a new external metadata instance.
        /// </summary>
        /// <param name="openBankingStandard">openBankingStandard the open banking standard</param>
        /// <param name="consent">consent the consent</param>
        public ExternalMetadata(OpenBankingStandard openBankingStandard,
            string consent)
        {
            this.openBankingStandard = openBankingStandard;
            this.consent = consent;
        }

        public OpenBankingStandard GetOpenBankingStandard()
        {
            return openBankingStandard;
        }

        public string GetConsent()
        {
            return consent;
        }
    }
}
