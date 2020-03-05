using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tokenio.Proto.Common.EidasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using Tokenio.TokenRequests;
using Tokenio.Utils;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
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
        public UnauthenticatedClient(GatewayService.GatewayServiceClient gateway) : base(gateway)
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
            var request = new GetMemberRequest {MemberId = memberId};
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
            var request = new GetTokenRequestResultRequest {TokenRequestId = tokenRequestId};
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
            var request = new RetrieveTokenRequestRequest {RequestId = tokenRequestId};
            return gateway.RetrieveTokenRequestAsync(request)
                .ToTask(response => response.TokenRequest);
        }
        
        /// <summary>
        /// Recovers an eIDAS-verified member with eidas payload.
        /// 
        /// </summary>
        /// <param name="payload">a payload containing member id, the certificate and a new key to add to the member</param>
        /// <param name="signature">a payload signature with the private key corresponding to the certificate</param>
        /// <param name="cryptoEngine">a crypto engine that must contain the privileged key that is included in
        ///      the payload(if it does not contain keys for other levels they will be generated)</param>
        /// <returns>a task of a new member</returns>
        public Task<ProtoMember> RecoverEidasMember(
                EidasRecoveryPayload payload,
                string signature,
                ICryptoEngine cryptoEngine)
        {
            Key privilegedKey = payload.Key;
            Key standardKey = GetOrGenerateKeyForLevel(cryptoEngine, Level.Standard);
            Key lowKey = GetOrGenerateKeyForLevel(cryptoEngine, Level.Low);

            IList<Key> keys = new List<Key> { privilegedKey, standardKey, lowKey };
            // TODO(RD-3764): createSigner by keyId (to make sure it's for the key in the payload)
            ISigner signer = cryptoEngine.CreateSigner(Level.Privileged);
            string memberId = payload.MemberId;

            var request = new RecoverEidasRequest
            {
                Payload = payload,
                Signature = signature
            };
            var memberRequest = new GetMemberRequest { MemberId = memberId };

            return Util.TwoTasks(
                    gateway.GetMemberAsync(memberRequest)
                        .ToTask(response => response.Member),
                    gateway.RecoverEidasMemberAsync(request)
                        .ToTask(response => response.RecoveryEntry))
                .Map(memberAndEntry =>
                {
                    IList<MemberOperation> operations = new List<MemberOperation>();
                    operations.Add(new MemberOperation { Recover = memberAndEntry.Value });

                    var list = keys
                        .Select(key => new MemberOperation
                        {
                            AddKey = new MemberAddKeyOperation
                            {
                                Key = key
                            }
                        }).ToList();
                    foreach (var operation in list)
                        operations.Add(operation);
                    return Util.ToUpdateMemberRequest(memberAndEntry.Key, operations, signer);
                })
                .FlatMap(updateMember => gateway
                    .UpdateMemberAsync(updateMember)
                    .ToTask(response => response.Member));
        }

        private static Key GetOrGenerateKeyForLevel(
                ICryptoEngine cryptoEngine,
                Level level)
        {
            var keys = cryptoEngine
                .GetPublicKeys()
                .ToList()
                .Where(k => k.Level.Equals(level))
                .ToList();

            if (keys.Count == 0)
            {
                return cryptoEngine.GenerateKey(level);
            }
            return keys[0];
        }
    }
}