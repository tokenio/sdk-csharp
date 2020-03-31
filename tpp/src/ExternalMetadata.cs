using Tokenio.Proto.Common.BankProtos;

namespace Tokenio.Tpp
{
    public class ExternalMetadata
    {
        private readonly OpenBankingStandard openBankingStandard;
        private readonly string consentId;
        private readonly string consent;


        public ExternalMetadata(
            OpenBankingStandard openBankingStandard,
            string consentId = null,
            string consent = null)
        {
            this.openBankingStandard = openBankingStandard;
            this.consentId = consentId ?? string.Empty;
            this.consent = consent ?? string.Empty;
        }

        public OpenBankingStandard GetOpenBankingStandard()
        {
            return openBankingStandard;
        }

        public string GetConsentId()
        {
            return consentId;
        }

        public string GetConsent()
        {
            return consent;
        }
    }
}