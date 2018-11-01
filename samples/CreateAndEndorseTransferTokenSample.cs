using Tokenio;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.PricingProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using static Tokenio.Proto.Common.AccountProtos.BankAccount.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;
using Token = Tokenio.Proto.Common.TokenProtos.Token;

namespace samples
{
    public class CreateAndEndorseTransferTokenSample
    {
        /// <summary>
        /// Creates a transfer token and authorizes a money transfer from a payer to a payee.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeAlias">payee Token member alias</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferToken(
            MemberSync payer,
            Alias payeeAlias)
        {
            // We'll use this as a reference ID. Normally, a payer who
            // explicitly sets a reference ID would use an ID from a db.
            // E.g., a bill-paying service might use ID of a "purchase".
            // We don't have a db, so we fake it with a random string:
            var purchaseId = Util.Nonce();

            // Create a transfer token.
            var transferToken = payer.CreateTransferToken(
                    100.0, // amount
                    "EUR") // currency
                // source account:
                .SetAccountId(payer.GetAccounts()[0].Id())
                // payee token alias:
                .SetToAlias(payeeAlias)
                // optional description:
                .SetDescription("Book purchase")
                // ref id (if not set, will get random ID)
                .SetRefId(purchaseId)
                .Execute();

            // Payer endorses a token to a payee by signing it
            // with her secure private key.
            transferToken = payer.EndorseToken(transferToken, Standard).Token;

            return transferToken;
        }

        /// <summary>
        /// Creates a transfer token using some other options.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeId">payee Token member Id</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferTokenWithOtherOptions(
            MemberSync payer,
            string payeeId)
        {
            var now = Util.EpochTimeMillis();

            var srcQuote = new TransferQuote
            {
                Id = "b40d2555df187098da241e6b9079cf45",
                AccountCurrency = "EUR",
                FeesTotal = "0.02",
                Fees =
                {
                    new TransferQuote.Types.Fee
                    {
                        Amount = "0.02",
                        Description = "Transfer fee"
                    }
                }
            };

            var pricing = new Pricing {SourceQuote = srcQuote};

            // Create a transfer token.
            var transferToken = payer.CreateTransferToken(
                    120.0, // amount
                    "EUR") // currency
                // source account:
                .SetAccountId(payer.GetAccounts()[0].Id())
                .SetToMemberId(payeeId)
                // effective in one second:
                .SetEffectiveAtMs(now + 1000)
                // expires in 300 seconds:
                .SetExpiresAtMs(now + (300 * 1000))
                .SetRefId("a713c8a61994a749")
                .SetPricing(pricing)
                .SetChargeAmount(10.0)
                .SetDescription("Book purchase")
                .SetPurposeOfPayment(PurposeOfPayment.PersonalExpenses)
                .Execute();

            // Payer endorses a token to a payee by signing it
            // with her secure private key.
            transferToken = payer.EndorseToken(transferToken, Standard).Token;

            return transferToken;
        }

        /// <summary>
        /// Creates transfer token to a destination.
        /// </summary>
        /// <param name="payer">Payer who has no linked bank accounts</param>
        /// <param name="payeeAlias">Alias of payee member</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferTokenToDestination(
            MemberSync payer,
            Alias payeeAlias)
        {
            // Create a transfer token.
            var transferToken = payer.CreateTransferToken(
                    100.0, // amount
                    "EUR") // currency
                .SetAccountId(payer.GetAccounts()[0].Id())
                .SetToAlias(payeeAlias)
                .AddDestination(
                    new TransferEndpoint
                    {
                        Account = new BankAccount
                        {
                            Sepa = new Sepa
                            {
                                Bic = "XUIWC2489",
                                Iban = "DE89 3704 0044 0532 0130 00"
                            }
                        }
                    })
                .Execute();

            // Payer endorses a token to a payee by signing it with her secure private key.
            transferToken = payer.EndorseToken(transferToken, Standard).Token;

            return transferToken;
        }

        /// <summary>
        /// Creates a transfer token and authorizes a money transfer from a payer to a payee.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeAlias">payee Token member alias</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferTokenWithExistingAttachment(
            MemberSync payer,
            Alias payeeAlias)
        {
            // Create a transfer token.
            var transferToken = payer.CreateTransferToken(
                    100.0,
                    "EUR")
                .SetAccountId(payer.GetAccounts()[0].Id())
                .SetToAlias(payeeAlias)
                .SetDescription("Invoice payment")
                .AddAttachment(
                    payer.MemberId(),
                    "image/jpeg",
                    "invoice.jpg",
                    LoadImageByteArray("invoice.jpg"))
                .Execute();

            // Payer endorses a token to a payee by signing it
            // with her secure private key.
            transferToken = payer.EndorseToken(transferToken, Standard).Token;

            return transferToken;
        }

        private static byte[] LoadImageByteArray(string filename)
        {
            return new byte[0];
        }
    }
}
