using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.User;
using Tokenio.User.Utils;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    /// <summary>
    /// Creates a transfer token and endorses it to a payee.
    /// </summary>
    public static class CreateStandingOrderTokenSample
    {
        /// <summary>
        /// Creates a transfer token and authorizes a money transfer from a payer to a payee.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeAlias">payee Token member alias</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateStandingOrderToken(
            UserMember payer,
            Alias payeeAlias)
        {
            // Create an access token for the grantee to access bank
            string purchaseId = Util.Nonce();

            // Set SEPA destination.
            TransferDestination sepaDestination = new TransferDestination
            {
                Sepa = new TransferDestination.Types.Sepa
                {
                    Bic = "XUIWC2489",
                    Iban = "DE89 3704 0044 0532 0130 00"
                }
            };

            // Create a transfer token.
            StandingOrderTokenBuilder builder = payer.CreateStandingOrderTokenBuilder(
                    10.0, // amount
                    "EUR", // currency
                    "DAIL", // frequency of the standing order
                    DateTime.Now.AddDays(1), // start date of the standing order
                    DateTime.Now.AddDays(7)) // end date of the standing order
                .SetAccountId(payer.GetAccountsBlocking()[0].Id())
                // payee token alias:
                .SetToAlias(payeeAlias)
                // optional description:
                .SetDescription("Book purchase")
                // ref id (if not set, will get random ID)
                .SetRefId(purchaseId)
                .AddDestination(sepaDestination);

            PrepareTokenResult result = payer.PrepareStandingOrderTokenBlocking(builder);

            // Payer endorses a token to a payee by signing it
            // with her secure private key.
            Token token = payer.CreateTokenBlocking(result.TokenPayload, Key.Types.Level.Standard);

            return token;
        }
    }
}
