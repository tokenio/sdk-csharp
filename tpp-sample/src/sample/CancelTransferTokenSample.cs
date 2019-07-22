using System;
using Tokenio.Proto.Common.TokenProtos;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;

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
         public static TokenOperationResult CancelTransferToken(TppMember payee, string tokenId) {
        // Retrieve a transfer token to cancel.
        Token transferToken = payee.GetTokenBlocking(tokenId);

        // Cancel transfer token.
        return payee.CancelTokenBlocking(transferToken);
    }
	}
}
