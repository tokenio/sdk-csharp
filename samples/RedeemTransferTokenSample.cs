using Tokenio;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;

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

            var transferDestination = new TransferDestination
            {
                Token = new TransferDestination.Types.Token
                {
                    MemberId = payee.MemberId(),
                    AccountId = accountId
                }
            };

            var transfer = payee.RedeemToken(
                transferToken,
                transferDestination,
                cartId).Result;

            return transfer;
        }
    }
}