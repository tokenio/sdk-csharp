using System;
using System.Collections.Generic;
using System.Linq;
using Tokenio.Exceptions;
using DestinationCase = Tokenio.Proto.Common.TransferInstructionsProtos.TransferDestination.DestinationOneofCase;

namespace Tokenio.Tpp.TokenRequests
{
    public class TokenRequestTransferDestinationsCallbackParameters
    {
        private static readonly string COUNTRY_FIELD = "country";
        private static readonly string BANK_NAME_FIELD = "bankName";

        private static readonly string SUPPORTED_TRANSFER_DESTINATION_TYPES_FIELD
            = "supportedTransferDestinationType";

        /// <summary>
        /// Parses url parameters such as country, bank and state for the use case
        /// to allow TPP to set transfer destinations for cross border payment.
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>TokenRequestTransferDestinationsCallbackParameters instance</returns>
        public static TokenRequestTransferDestinationsCallbackParameters Create(
            IDictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey(COUNTRY_FIELD)
                || !parameters.ContainsKey(BANK_NAME_FIELD)
                || !parameters.ContainsKey(SUPPORTED_TRANSFER_DESTINATION_TYPES_FIELD))
            {
                throw new InvalidTokenRequestQuery();
            }

            IList<string> countries = parameters[SUPPORTED_TRANSFER_DESTINATION_TYPES_FIELD].Split(',').ToList();
            IList<string> bankNames = parameters[SUPPORTED_TRANSFER_DESTINATION_TYPES_FIELD].Split(',').ToList();
            IList<string> destinations = parameters[SUPPORTED_TRANSFER_DESTINATION_TYPES_FIELD].Split(',').ToList();

            IList<DestinationCase> destinationCases = destinations
                .Select(destination =>
                    (DestinationCase)Enum.Parse(typeof(DestinationCase), destination.ToLower().Replace("_", ""), true))
                .ToList();

            return new TokenRequestTransferDestinationsCallbackParameters
            {
                Country = countries[0],
                BankName = bankNames[0],
                SupportedTransferDestinationTypes = destinationCases
            };
        }

        public string Country { get; private set; }

        public string BankName { get; private set; }

        public IList<DestinationCase> SupportedTransferDestinationTypes { get; private set; }
    }
}