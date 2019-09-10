using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User;
using Tokenio.User.Utils;
using UserMember = Tokenio.User.Member;
namespace Tokenio.Sample.User
{
    /// <summary>
    /// Creates a transfer token and endorses it to a payee.
    /// </summary>
    public static class CreateAndEndorseTransferTokenSample
    {
        /// <summary>
        /// Creates a transfer token and authorizes a money transfer from a payer to a payee.
        /// </summary>
        /// <param name="payer">payer Token member</param>
        /// <param name="payeeAlias">payee Token member alias</param>
        /// <returns>a transfer Token</returns>
        public static Token CreateTransferToken(
            UserMember payer,
         Alias payeeAlias)
        {
            // Create an access token for the grantee to access bank
            string purchaseId = Util.Nonce();


            // Create a transfer token.
            TransferTokenBuilder builder = payer.CreateTransferTokenBuilder(
                     10.0, // amount
                     "EUR")  // currency // source account:
                     .SetAccountId(payer.GetAccountsBlocking()[0].Id())
                     // payee token alias:
                     .SetToAlias(payeeAlias)
                     // optional description:
                     .SetDescription("Book purchase")
                     // ref id (if not set, will get random ID)
                     .SetRefId(purchaseId);

            PrepareTokenResult result = payer.PrepareTransferTokenBlocking(builder);

            // Payer endorses a token to a payee by signing it
            // with her secure private key.
            Token transferToken = payer.CreateTokenBlocking(
                    result.TokenPayload, Key.Types.Level.Low);

            return transferToken;
        }
    }
}
