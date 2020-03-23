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
    /// Creates a transfer token and endorses it to a payee.
    /// </summary>
    public static class CreateTransferTokenSample
    {
        /// <summary>
        /// Creates a transfer token and authorizes a money transfer from a payer to a payee.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeAlias">payee Token member alias</param>
        /// <param name="keyLevel">the level of signature to provide</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferToken(
            UserMember payer,
            Alias payeeAlias,
            Level keyLevel)
        {
            // We'll use this as a reference ID. Normally, a payer who
            // explicitly sets a reference ID would use an ID from a db.
            // E.g., a bill-paying service might use ID of a "purchase".
            // We don't have a db, so we fake it with a random string:
            string purchaseId = Util.Nonce();
            // Set the details of the token.
            TransferTokenBuilder builder = payer.CreateTransferTokenBuilder(
                    10.0, // amount
                    "EUR") // currency
                           // source account:
                .SetAccountId(payer.GetAccountsBlocking()[0].Id())
                // payee token alias:
                .SetToAlias(payeeAlias)
                // optional description:
                .SetDescription("Book purchase")
                // ref id (if not set, will get random ID)
                .SetRefId(purchaseId);
            PrepareTokenResult result = payer.PrepareTransferTokenBlocking(builder);
            // Create the token: Default behavior is to provide the member's signature
            // at the specified level. In other cases, it may be necessary to provide
            // additional signatures with payer.createToken(payload, signatures).
            Token transferToken = payer.CreateTokenBlocking(result.TokenPayload, keyLevel);
            return transferToken;
        }

        /// <summary>
        /// Creates a transfer token using some other options.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeId">payee Token member Id</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferTokenWithOtherOptions(
            UserMember payer,
            string payeeId)
        {
            long now = Util.CurrentMillis();
            // Set the details of the token.
            TransferTokenBuilder builder = payer.CreateTransferTokenBuilder(
                    120.0, // amount
                    "EUR") // currency
                           // source account:
                .SetAccountId(payer.GetAccountsBlocking()[0].Id())
                .SetToMemberId(payeeId)
                // effective in one second:
                .SetEffectiveAtMs(now + 1000)
                // expires in 300 seconds:
                .SetExpiresAtMs(now + (300 * 1000))
                .SetRefId("a713c8a61994a749")
                .SetChargeAmount(10.0)
                .SetDescription("Book purchase");
            // Get the token redemption policy and resolve the token payload.
            PrepareTokenResult result = payer.PrepareTransferTokenBlocking(builder);
            // Create the token, signing with the payer's STANDARD-level key
            Token transferToken = payer.CreateTokenBlocking(result.TokenPayload, Level.Standard);
            return transferToken;
        }

        /// <summary>
        /// Creates transfer token to a destination.
        /// </summary>
        /// <param name="payer">Payer who has no linked bank accounts</param>
        /// <param name="payeeAlias">Alias of payee member</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferTokenToDestination(
            UserMember payer,
            Alias payeeAlias)
        {
            // Set SEPA destination.
            TransferDestination sepaDestination = new TransferDestination
            {
                Sepa = new TransferDestination.Types.Sepa
                {
                    Bic = "XUIWC2489",
                    Iban = "DE89 3704 0044 0532 0130 00"
                }
            };
            // Set the destination and other details.
            TransferTokenBuilder builder =
                payer.CreateTransferTokenBuilder(
                        100.0, // amount
                        "EUR") // currency
                    .SetAccountId(payer.GetAccountsBlocking()[0].Id())
                    .SetToAlias(payeeAlias)
                    .AddDestination(sepaDestination);
            // Get the token redemption policy and resolve the token payload.
            PrepareTokenResult result = payer.PrepareTransferTokenBlocking(builder);
            // Create the token, signing with the payer's STANDARD-level key
            Token transferToken = payer.CreateTokenBlocking(result.TokenPayload, Level.Standard);
            return transferToken;
        }

        /// <summary>
        /// Creates a transfer token with a later execution date.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeAlias">payee Token member alias</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferTokenScheduled(
            UserMember payer,
            Alias payeeAlias)
        {
            // We'll use this as a reference ID. Normally, a payer who
            // explicitly sets a reference ID would use an ID from a db.
            // E.g., a bill-paying service might use ID of a "purchase".
            // We don't have a db, so we fake it with a random string:
            string purchaseId = Util.Nonce();
            // Set the details of the token.
            TransferTokenBuilder builder = payer.CreateTransferTokenBuilder(
                    10.0, // amount
                    "EUR") // currency
                           // source account:
                .SetAccountId(payer.GetAccountsBlocking()[0].Id())
                // payee token alias:
                .SetToAlias(payeeAlias)
                // optional description:
                .SetDescription("Book purchase")
                // ref id (if not set, will get random ID)
                .SetRefId(purchaseId)
                // set the transfer to execute in 30 days
                .SetExecutionDate(DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"));
            // Get the token redemption policy and resolve the token payload.
            PrepareTokenResult result = payer.PrepareTransferTokenBlocking(builder);
            // Create the token: Default behavior is to provide the member's signature
            // at the specified level. In other cases, it may be necessary to provide
            // additional signatures with payer.createToken(payload, signatures).
            Token transferToken = payer.CreateTokenBlocking(result.TokenPayload, Level.Standard);
            return transferToken;
        }
    }
}
