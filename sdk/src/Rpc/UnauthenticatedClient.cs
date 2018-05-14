using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Rpc
{
    /// <summary>
    /// Similar to <see cref="Client"/> but is only used for a handful of requests that
    /// don't require authentication. We use this client to create new member or getMember
    /// an existing one and switch to the authenticated <see cref="Client"/>.
    /// </summary>
    public class UnauthenticatedClient
    {
        private static readonly Alias TOKEN = new Alias
        {
            Type = Domain,
            Value = "token.io"
        };

        private readonly GatewayService.GatewayServiceClient gateway;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="gateway">the gateway gRPC client</param>
        public UnauthenticatedClient(GatewayService.GatewayServiceClient gateway)
        {
            this.gateway = gateway;
        }

        /// <summary>
        /// Checks if a given alias already exists.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>true if alias exists, false otherwise</returns>
        public Task<bool> AliasExists(Alias alias)
        {
            var request = new ResolveAliasRequest {Alias = alias};
            return gateway.ResolveAliasAsync(request)
                .ToTask(response => response.Member != null);
        }

        /// <summary>
        /// Looks up member id for a given alias.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>member id if alias already exists, null otherwise</returns>
        public Task<string> GetMemberId(Alias alias)
        {
            var request = new ResolveAliasRequest {Alias = alias};
            return gateway.ResolveAliasAsync(request)
                .ToTask(response => response.Member?.Id);
        }

        /// <summary>
        /// Looks up member id for a given member ID.
        /// </summary>
        /// <param name="memberId">the member ID to check</param>
        /// <returns>the member</returns>
        public Task<Member> GetMember(string memberId)
        {
            var request = new GetMemberRequest {MemberId = memberId};
            return gateway.GetMemberAsync(request)
                .ToTask(response => response.Member);
        }

        /// <summary>
        /// Creates new member ID. After the method returns the ID is reserved on the server.
        /// </summary>
        /// <param name="memberType">the type of member to register</param>
        /// <returns>the created member ID</returns>
        public Task<string> CreateMemberId(MemberType memberType)
        {
            var request = new CreateMemberRequest {Nonce = Util.Nonce(), MemberType = memberType};
            return gateway.CreateMemberAsync(request)
                .ToTask(response => response.MemberId);
        }

        /// <summary>
        /// Creates a new token member.
        /// </summary>
        /// <param name="memberId">the member ID</param>
        /// <param name="operations">the operations to apply</param>
        /// <param name="metadata">the metadata of the operations</param>
        /// <param name="signer">the signer used to sign the request</param>
        /// <returns>the created member</returns>
        public Task<Member> CreateMember(
            string memberId,
            IList<MemberOperation> operations,
            IList<MemberOperationMetadata> metadata,
            ISigner signer)
        {
            var update = new MemberUpdate
            {
                MemberId = memberId,
                Operations = {operations}
            };
            var request = new UpdateMemberRequest
            {
                Update = update,
                UpdateSignature = new Signature
                {
                    MemberId = memberId,
                    KeyId = signer.GetKeyId(),
                    Signature_ = signer.Sign(update)
                },
                Metadata = {metadata}
            };

            return gateway.UpdateMemberAsync(request)
                .ToTask(response => response.Member);
        }

        /// <summary>
        /// Retrieves a transfer token request.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request</returns>
        public Task<TokenRequest> RetrieveTokenRequest(string tokenRequestId)
        {
            var request = new RetrieveTokenRequestRequest {RequestId = tokenRequestId};
            return gateway.RetrieveTokenRequestAsync(request)
                .ToTask(response => response.TokenRequest);
        }

        /// <summary>
        /// Notifies subscribed devices that accounts should be linked.
        /// </summary>
        /// <param name="alias">alias of the member</param>
        /// <param name="authorization">the bank authorization for the funding account</param>
        /// <returns>status of the notification</returns>
        public Task<NotifyStatus> NotifyLinkAccounts(
            Alias alias,
            BankAuthorization authorization)
        {
            var request = new NotifyRequest
            {
                Alias = alias,
                Body = new NotifyBody
                {
                    LinkAccounts = new LinkAccounts
                    {
                        BankAuthorization = authorization
                    }
                }
            };
            return gateway.NotifyAsync(request)
                .ToTask(response => response.Status);
        }

        /// <summary>
        /// Notifies subscribed devices that a key should be added.
        /// </summary>
        /// <param name="alias">alias of the member</param>
        /// <param name="name">device/client name, e.g. iPhone, Chrome Browser, etc</param>
        /// <param name="key">the that needs an approval</param>
        /// <returns>status of the notification</returns>
        public Task<NotifyStatus> NotifyAddKey(
            Alias alias,
            string name,
            Key key)
        {
            var request = new NotifyRequest
            {
                Alias = alias,
                Body = new NotifyBody
                {
                    AddKey = new AddKey
                    {
                        Name = name,
                        Key = key
                    }
                }
            };
            return gateway.NotifyAsync(request)
                .ToTask(response => response.Status);
        }

        /// <summary>
        /// Notifies subscribed devices that a key should be added.
        /// </summary>
        /// <param name="alias">alias of the member</param>
        /// <param name="authorization">the bank authorization for the funding account</param>
        /// <param name="name">device/client name, e.g. iPhone, Chrome Browser, etc</param>
        /// <param name="key">the that needs an approval</param>
        /// <returns>status of the notification</returns>
        public Task<NotifyStatus> NotifyLinkAccountsAndAddKey(
            Alias alias,
            BankAuthorization authorization,
            string name,
            Key key)
        {
            var request = new NotifyRequest
            {
                Alias = alias,
                Body = new NotifyBody
                {
                    LinkAccountsAndAddKey = new LinkAccountsAndAddKey
                    {
                        LinkAccounts = new LinkAccounts
                        {
                            BankAuthorization = authorization
                        },
                        AddKey = new AddKey
                        {
                            Name = name,
                            Key = key
                        }
                    }
                }
            };
            return gateway.NotifyAsync(request)
                .ToTask(response => response.Status);
        }

        /// <summary>
        /// Notifies subscribed devices of payment requests.
        /// </summary>
        /// <param name="tokenPayload">the payload of a token to be sent</param>
        /// <returns>status of the notification request</returns>
        public Task<NotifyStatus> NotifyPaymentRequest(TokenPayload tokenPayload)
        {
            var request = new RequestTransferRequest{TokenPayload = tokenPayload};
            return gateway.RequestTransferAsync(request)
                .ToTask(response => response.Status);
        }

        /// <summary>
        /// Begins account recovery.
        /// </summary>
        /// <param name="alias">the alias used to recover</param>
        /// <returns>the verification ID</returns>
        public Task<string> BeginRecovery(Alias alias)
        {
            var request = new BeginRecoveryRequest {Alias = alias.ToNormalized()};
            return gateway.BeginRecoveryAsync(request)
                .ToTask(response => response.VerificationId);
        }

        /// <summary>
        /// Create a recovery authorization for some agent to sign.
        /// </summary>
        /// <param name="memberId">the ID of the member we claim to be</param>
        /// <param name="privilegedKey">the new privileged key we want to use</param>
        /// <returns>the authorization</returns>
        public Task<Authorization> CreateRecoveryAuthorization(string memberId, Key privilegedKey)
        {
            var request = new GetMemberRequest {MemberId = memberId};
            return gateway.GetMemberAsync(request)
                .ToTask(response => new Authorization
                {
                    MemberId = memberId,
                    MemberKey = privilegedKey,
                    PrevHash = response.Member.LastHash
                });
        }

        /// <summary>
        /// Completes account recovery.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="recoveryOperations">the member recovery operations</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>the new member</returns>
        public Task<Member> CompleteRecovery(
            string memberId,
            IList<MemberRecoveryOperation> recoveryOperations,
            Key privilegedKey,
            ICryptoEngine cryptoEngine)
        {
            var operations = recoveryOperations.Select(re => new MemberOperation {Recover = re}).ToList();

            operations.Add(Util.ToAddKeyOperation(privilegedKey));
            operations.Add(Util.ToAddKeyOperation(cryptoEngine.GenerateKey(Level.Standard)));
            operations.Add(Util.ToAddKeyOperation(cryptoEngine.GenerateKey(Level.Low)));

            var signer = cryptoEngine.CreateSigner(Level.Privileged);

            return GetMember(memberId)
                .FlatMap(member =>
                {
                    var request = Util.ToUpdateMemberRequest(member, operations, signer);
                    return gateway.UpdateMemberAsync(request).ToTask(response => response.Member);
                });
        }

        /// <summary>
        /// Completes account recovery if the default recovery rule was set.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the code</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>the new member</returns>
        public Task<Member> CompleteRecoveryWithDefaultRule(
            string memberId,
            string verificationId,
            string code,
            ICryptoEngine cryptoEngine)
        {
            var privilegedKey = cryptoEngine.GenerateKey(Level.Privileged);
            var standardKey = cryptoEngine.GenerateKey(Level.Standard);
            var lowKey = cryptoEngine.GenerateKey(Level.Low);

            var signer = cryptoEngine.CreateSigner(Level.Privileged);

            var operations = new List<Key> {privilegedKey, standardKey, lowKey}
                .Select(Util.ToAddKeyOperation)
                .ToList();

            return Util.TwoTasks(
                    GetMember(memberId),
                    GetRecoveryAuthorization(verificationId, code, privilegedKey))
                .Map(memberAndEntry =>
                {
                    operations.Add(new MemberOperation {Recover = memberAndEntry.Value});
                    return Util.ToUpdateMemberRequest(memberAndEntry.Key, operations, signer);
                })
                .FlatMap(updateMember => gateway
                    .UpdateMemberAsync(updateMember)
                    .ToTask(response => response.Member));
        }

        /// <summary>
        /// Gets recovery authorization from Token.
        /// </summary>
        /// <param name="verificationId">the verification ID</param>
        /// <param name="code">the verification code</param>
        /// <param name="privilegedKey">the privileged key</param>
        /// <returns>the recovery entry</returns>
        public Task<MemberRecoveryOperation> GetRecoveryAuthorization(
            string verificationId,
            string code,
            Key privilegedKey)
        {
            var request = new CompleteRecoveryRequest
            {
                VerificationId = verificationId,
                Code = code,
                Key = privilegedKey
            };

            return gateway.CompleteRecoveryAsync(request).ToTask(response => response.RecoveryEntry);
        }

        public Task<PagedBanks> GetBanks(
            IList<string> ids,
            string search,
            string country,
            int? page,
            int? perPage,
            string sort)
        {
            var request = new GetBanksRequest();

            if (ids != null)
            {
                request.Ids.Add(ids);
            }

            if (search != null)
            {
                request.Search = search;
            }

            if (country != null)
            {
                request.Country = country;
            }

            if (page.HasValue)
            {
                request.Page = page.Value;
            }

            if (perPage.HasValue)
            {
                request.PerPage = perPage.Value;
            }

            if (sort != null)
            {
                request.Sort = sort;
            }

            return gateway.GetBanksAsync(request)
                .ToTask(response => new PagedBanks(response));
        }

        /// <summary>
        /// Returns the token member.
        /// </summary>
        /// <returns>the member</returns>
        public Task<Member> GetTokenMember()
        {
            return GetMemberId(TOKEN).FlatMap(GetMember);
        }

        /// <summary>
        /// Get a token ID based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token id</returns>
        public Task<string> GetTokenId(string tokenRequestId)
        {
            var request = new GetTokenIdRequest {TokenRequestId = tokenRequestId};
            return gateway.GetTokenIdAsync(request)
                .ToTask(response => response.TokenId);
        }
    }
}