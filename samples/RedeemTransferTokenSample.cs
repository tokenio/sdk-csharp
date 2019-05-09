using Tokenio;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using static Tokenio.Proto.Common.AccountProtos.BankAccount.Types;

namespace Sample
{
    public class RedeemTransferTokenSample
    {
        public static Transfer RedeemTransferToken(
            Member payee,
            string accountId, // account ID of the payee
            string tokenId)
        {
            
            var cartId = Util.Nonce();

            var transferToken = payee.GetToken(tokenId).Result;

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
                cartId).Result;

            return transfer;
        }
    }
}
