using System.Threading.Tasks;
using Tokenio.Proto.Gateway;
using Tokenio.TokenRequests;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;

namespace Tokenio.Tpp.Rpc
{
    /// <summary>
    /// Similar to <see cref="Client"/> but is only used for a handful of requests that
    /// don't require authentication. We use this client to create new member or getMember
    /// an existing one and switch to the authenticated <see cref="Client"/>.
    /// </summary>
    public sealed class UnauthenticatedClient : Tokenio.Rpc.UnauthenticatedClient
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="gateway">the gateway gRPC client</param>
        public UnauthenticatedClient(GatewayService.GatewayServiceClient gateway)
            : base(gateway)
        {
        }

        /// <summary>
        /// Looks up member id for a given member ID. The user is defined by
        /// the key used for authentication.
        /// </summary>
        /// <param name="memberId">the member ID to check</param>
        /// <returns>the member</returns>
        public Task<ProtoMember> GetMember(string memberId)
        {
            var request = new GetMemberRequest { MemberId = memberId };
            return gateway.GetMemberAsync(request)
                .ToTask(response => response.Member);
        }

        /// <summary>
        /// Returns the token member.
        /// </summary>
        /// <returns>the member</returns>
        public Task<ProtoMember> GetTokenMember()
        {
            return GetMemberId(TOKEN).FlatMap(GetMember);
        }

        /// <summary>
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request result</returns>
        public Task<TokenRequestResult> GetTokenRequestResult(string tokenRequestId)
        {
            var request = new GetTokenRequestResultRequest { TokenRequestId = tokenRequestId };
            return gateway.GetTokenRequestResultAsync(request)
                .ToTask(response => new TokenRequestResult(response.TokenId, response.Signature));
        }

        /// <summary>
        /// Retrieves a transfer token request.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>token request that was stored with the request id</returns>
        public Task<Proto.Common.TokenProtos.TokenRequest> RetrieveTokenRequest(string tokenRequestId)
        {
            var request = new RetrieveTokenRequestRequest { RequestId = tokenRequestId };
            return gateway.RetrieveTokenRequestAsync(request)
                .ToTask(response => response.TokenRequest);
        }
    }
}
