using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.User;
using Tokenio.User.Utils;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;
namespace TokenioSample
{
    public class CreateAndEndorseTransferTokenSample
    {
        public static Token CreateTransferToken(
            UserMember payer,
         Alias payeeAlias)
        {
            // Create an access token for the grantee to access bank
            string purchaseId = Util.Nonce();

          
            // Create a transfer token.
           TransferTokenBuilder builder = payer.CreateTransferToken(
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
                    result.TokenPayload, Level.Low);

            return transferToken;
        }
    }
    }
