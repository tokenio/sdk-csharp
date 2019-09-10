using Tokenio.Proto.Common.TokenProtos;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    public static class GetTokensSample
    {
        /// <summary>
        /// Gets a token by ID.
        /// </summary>
        /// <param name="member">member represented by the token (payer/payee/grantor/grantee)</param>
        /// <param name="tokenId">token ID</param>
        /// <returns>token</returns>
        public static Token GetToken(UserMember member, string tokenId)
        {
            Token token = member.GetTokenBlocking(tokenId);

            // get token payload
            TokenPayload payload = token.Payload;

            // get signatures
            var signatures = token.PayloadSignatures;

            return token;
        }


        /// <summary>
        /// Gets a list of transfer tokens associated with a member.
        /// </summary>
        /// <param name="member">member</param>
        /// <returns>paged list of transfer tokens</returns>
        public static PagedList<Token> GetTransferTokens(UserMember member)
        {
            // last 10 tokens and offset that can be used to get the next 10
            PagedList<Token> pagedList = member.GetTransferTokensBlocking("", 10);

            return pagedList;
        }

        /// <summary>
        /// Gets a list of access tokens associated with the member.
        /// </summary>
        /// <param name="member">member</param>
        /// <returns>paged list of access tokens</returns>
        public static PagedList<Token> GetAccessTokens(UserMember member)
        {
            // last 10 tokens and offset that can be used to get the next 10
            PagedList<Token> pagedList = member.GetAccessTokensBlocking("", 10);

            return pagedList;
        }

    }
}
