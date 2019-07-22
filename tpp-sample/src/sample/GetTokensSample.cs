using System;
using Tokenio.Proto.Common.TokenProtos;
using TppMember = Tokenio.Tpp.Member;
using Tokenio.Proto.Common.MoneyProtos;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using System.Collections.Generic;
using Tokenio;

namespace TokenioSample
{
    public class GetTokensSample
    {
        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <returns>The token.</returns>
        /// <param name="member">Member.</param>
        /// <param name="tokenId">Token identifier.</param>
        public static Token GetToken(TppMember member, string tokenId)
        {
            Token token = member.GetTokenBlocking(tokenId);

            // get token payload
            TokenPayload payload = token.Payload;

            // get signatures
            var signatures = token.PayloadSignatures;

            return token;
        }


        /// <summary>
        /// Gets the transfer tokens.
        /// </summary>
        /// <returns>The transfer tokens.</returns>
        /// <param name="member">Member.</param>
        public static PagedList<Token> GetTransferTokens(TppMember member)
        {
            // last 10 tokens and offset that can be used to get the next 10
            PagedList<Token> pagedList = member.GetTransferTokensBlocking("", 10);

            return pagedList;
        }

        /// <summary>
        /// Gets the access tokens.
        /// </summary>
        /// <returns>The access tokens.</returns>
        /// <param name="member">Member.</param>
        public static PagedList<Token> GetAccessTokens(TppMember member)
        {
            // last 10 tokens and offset that can be used to get the next 10
            PagedList<Token> pagedList = member.GetAccessTokensBlocking("", 10);

            return pagedList;
        }

    }
}
