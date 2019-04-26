using Tokenio;
using Tokenio.Proto.Common.TokenProtos;

namespace samples
{
    public class GetTokensSample
    {
        /// <summary>
        /// Gets a token by ID
        /// </summary>
        /// <param name="member">member represented by the token (payer/payee/grantor/grantee)</param>
        /// <param name="tokenId">token ID</param>
        /// <returns>token</returns>
        public static Token GetToken(Member member, string tokenId)
        {
            var token = member.GetTokenBlocking(tokenId);

            // get token payload
            var payload = token.Payload;

            // get signatures
            var signatures = token.PayloadSignatures;

            return token;
        }


        /// <summary>
        /// Gets a list of transfer tokens associated with a member
        /// </summary>
        /// <param name="member">member</param>
        /// <returns>paged list of transfer tokens</returns>
        public static PagedList<Token> getTransferTokens(Member member)
        {
           var pagedList =  member.GetTransferTokensBlocking("", 10);

           return pagedList;
        }

        /// <summary>
        /// Gets a list of access tokens associated with the member.
        /// </summary>
        /// <param name="member">member</param>
        /// <returns>paged list of access tokens</returns>
        public static PagedList<Token> getAccessTokens(Member member)
        {
            var pagedList = member.GetAccessTokensBlocking("", 10);

            return pagedList;
        }
    }
}