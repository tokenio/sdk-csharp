using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.User;
using Tokenio.User.Utils;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    /// <summary>
    /// Creates a standing order token to a payee.
    /// </summary>
    public static class CreateStandingOrderTokenSample
    {
        /// <summary>
        /// Creates a standing order  token and authorizes a money transfer from a payer to a payee.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeAlias">payee Token member alias</param>
        /// <param name="keyLevel">the level of signature to provide</param>
        /// <returns>a standing order Token</returns>
        public static Token CreateStandingOrderToken(
            UserMember payer,
            Alias payeeAlias,
            Level keyLevel)
        {
            // We'll use this as a reference ID. Normally, a payer who
            // explicitly sets a reference ID would use an ID from a db.
            // E.g., a bill-paying service might use ID of a "purchase".
            // We don't have a db, so we fake it with a random string:
            string purchaseId = Util.Nonce();
            // Set SEPA destination.
            TransferDestination sepaDestination = new TransferDestination
            {
                Sepa = new TransferDestination.Types.Sepa
                {
                    Bic = "XUIWC2489",
                    Iban = "DE89 3704 0044 0532 0130 00"
                },
                CustomerData = new CustomerData
                {
                    LegalNames = { "Southside" }
                }
            };
            // Set the details of the token.
            StandingOrderTokenBuilder builder = payer.CreateStandingOrderTokenBuilder(
                    10.0, // amount
                    "EUR", // currency
                    "DAIL", // frequency of the standing order
                    DateTime.Now, // start date
                    DateTime.Now.AddDays(7)) // end date
                                             // source account:
                .SetAccountId(payer.GetAccountsBlocking()[0].Id())
                // payee token alias:
                .SetToAlias(payeeAlias)
                // optional description:
                .SetDescription("Credit card statement payment")
                // ref id (if not set, will get random ID)
                .SetRefId(purchaseId)
                .AddDestination(sepaDestination);
            // Get the token redemption policy and resolve the token payload.
            PrepareTokenResult result = payer.PrepareStandingOrderTokenBlocking(builder);
            // Create the token: Default behavior is to provide the member's signature
            // at the specified level. In other cases, it may be necessary to provide
            // additional signatures with payer.createToken(payload, signatures).
            Token token = payer.CreateTokenBlocking(result.TokenPayload, keyLevel);
            return token;
        }
    }
}
