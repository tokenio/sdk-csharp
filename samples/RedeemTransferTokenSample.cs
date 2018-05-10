using Tokenio;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using static Tokenio.Proto.Common.AccountProtos.BankAccount.Types;

namespace samples
{
    public class RedeemTransferTokenSample
    {
        public static Transfer RedeemTransferToken(
            MemberSync payee,
            string accountId, // account ID of the payee
            string tokenId)
        {
            // ID of token to redeem
            // We'll use this as a reference ID. Normally, a payee who
            // explicitly sets a reference ID would use an ID from a db.
            // E.g., an online merchant might use the ID of a "shopping cart".
            // We don't have a db, so we fake it with a random string:
            var cartId = Util.Nonce();

            // Retrieve a transfer token to redeem.
            var transferToken = payee.GetToken(tokenId);

            // Payee redeems a transfer token.
            // Money is transferred to a payee bank account.
            var transfer = payee.RedeemToken(
                transferToken,
                new TransferEndpoint
                {
                    Account = new BankAccount
                    {
                        Token = new Token
                        {
                            MemberId = payee.MemberId(),
                            AccountId = accountId
                        }
                    }
                },
                // if refId not set, transfer will have random refID:
                cartId);

            return transfer;
        }
    }
}