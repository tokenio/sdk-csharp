using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.User;
using Tokenio.User.Utils;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    /// <summary>
    /// Creates a transfer token and endorses it to a payee.
    /// </summary>
    public static class CreateAndEndorseTransferTokenSample {
        /// <summary>
        /// Creates a transfer token and authorizes a money transfer from a payer to a payee.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeAlias">payee Token member alias</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferToken (
            UserMember payer,
            Alias payeeAlias) {
            // Create an access token for the grantee to access bank
            string purchaseId = Util.Nonce ();

            // Create a transfer token.
            TransferTokenBuilder builder = payer.CreateTransferTokenBuilder (
                    10.0, // amount
                    "EUR") // currency // source account:
                .SetAccountId (payer.GetAccountsBlocking () [0].Id ())
                // payee token alias:
                .SetToAlias (payeeAlias)
                // optional description:
                .SetDescription ("Book purchase")
                // ref id (if not set, will get random ID)
                .SetRefId (purchaseId);

            PrepareTokenResult result = payer.PrepareTransferTokenBlocking (builder);

            // Payer endorses a token to a payee by signing it
            // with her secure private key.
            Token transferToken = payer.CreateTokenBlocking (
                result.TokenPayload, Key.Types.Level.Low);

            return transferToken;
        }

        /// <summary>
        /// Creates a transfer token using some other options.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeId">payee Token member Id</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferTokenWithOtherOptions (
            UserMember payer,
            string payeeId) {
            long now = Util.CurrentMillis ();

            // Set the details of the token.
            TransferTokenBuilder builder = payer.CreateTransferTokenBuilder (
                    120.0, // amount
                    "EUR") // currency
                .SetAccountId (payer.GetAccountsBlocking () [0].Id ())
                .SetToMemberId (payeeId)
                .SetToMemberId (payeeId)
                // effective in one second:
                .SetEffectiveAtMs (now + 1000)
                // expires in 300 seconds:
                .SetExpiresAtMs (now + (300 * 1000))
                .SetRefId ("a713c8a61994a749")
                .SetChargeAmount (10.0)
                .SetDescription ("Book purchase");

            // Get the token redemption policy and resolve the token payload.
            PrepareTokenResult result = payer.PrepareTransferTokenBlocking (builder);

            // Create the token, signing with the payer's STANDARD-level key
            Token transferToken = payer.CreateTokenBlocking (result.TokenPayload, Level.Standard);

            return transferToken;
        }

        /// <summary>
        /// Creates transfer token to a destination.
        /// </summary>
        /// <param name="payer">Payer who has no linked bank accounts</param>
        /// <param name="payeeAlias">Alias of payee member</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferTokenToDestination (
            UserMember payer,
            Alias payeeAlias) {
            // Set SEPA destination.
            TransferDestination sepaDestination = new TransferDestination {
                Sepa = new TransferDestination.Types.Sepa {
                Bic = "XUIWC2489",
                Iban = "DE89 3704 0044 0532 0130 00"
                }
            };

            // Set the destination and other details.
            TransferTokenBuilder builder =
                payer.CreateTransferTokenBuilder (
                    100.0, // amount
                    "EUR") // currency
                .SetAccountId (payer.GetAccountsBlocking () [0].Id ())
                .SetToAlias (payeeAlias)
                .AddDestination (sepaDestination);

            // Get the token redemption policy and resolve the token payload.
            PrepareTokenResult result = payer.PrepareTransferTokenBlocking (builder);

            // Create the token, signing with the payer's STANDARD-level key
            Token transferToken = payer.CreateTokenBlocking (result.TokenPayload, Level.Standard);

            return transferToken;
        }
    }
}