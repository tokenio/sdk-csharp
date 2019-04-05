using System;
using System.Collections.Generic;
using Tokenio;
using Tokenio.Proto.Common.TokenProtos;

namespace samples
{
    public class GetTokensSample
    {
        /// <summary>
        /// Illustrate Member.GetToken
        /// </summary>
        /// <param name="member">Token member</param>
        public static Token GetToken(Member member, String tokenId)
        {
            Token token = member.GetTokenBlocking(tokenId);
            
            // get token payload
            TokenPayload payload = token.Payload;
            
            // get signatures
            IList<TokenSignature> signatures = token.PayloadSignatures;

            return token;
        }

        /// <summary>
        /// Illustrate Member.GetTransferTokens
        /// </summary>
        /// <param name="member">Token member</param>
        /// <returns></returns>
        public static PagedList<Token> GetTransferTokens(Member member)
        {
            // last 10 tokens and offset that can be used to get the next 10
            PagedList<Token> pagedList = member.GetTransferTokensBlocking("", 10);

            return pagedList;
        }

        /// <summary>
        /// Illustrate Member.GetAccessTokens
        /// </summary>
        /// <param name="member">Token member</param>
        /// <returns></returns>
        public static PagedList<Token> GetAccessTokens(Member member)
        {
            // last 10 tokens and offset that can be used to get the next 10
            PagedList<Token> pagedList = member.GetAccessTokensBlocking("", 10);

            return pagedList;
        }
    }
}
