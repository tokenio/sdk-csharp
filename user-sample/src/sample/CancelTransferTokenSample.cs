using System;
using Tokenio.Proto.Common.TokenProtos;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
	public class CancelTransferTokenSample
	{
		/// <summary>
        /// Cancels the transfer token.
        /// </summary>
        /// <returns>The transfer token.</returns>
        /// <param name="payee">Payee.</param>
        /// <param name="tokenId">Token identifier.</param>
         public static TokenOperationResult CancelTransferToken(UserMember payer, string tokenId) {
        // Retrieve a transfer token to cancel.
        Token transferToken = payer.GetTokenBlocking(tokenId);

        // Cancel transfer token.
        return payer.CancelTokenBlocking(transferToken);
    }
	}
}
