using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.BankProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using Tokenio.Utils;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using Tokenio.Exceptions;

namespace Tokenio.Rpc
{
    /// <summary>
    /// Similar to <see cref="Client"/> but is only used for a handful of requests that
    /// don't require authentication. We use this client to create new member or getMember
    /// an existing one and switch to the authenticated <see cref="Client"/>.
    /// </summary>
    public class UnauthenticatedClient
    {
        protected static readonly Alias TOKEN = new Alias
        {
            Type = Domain,
            Value = "token.io"
        };

        protected readonly GatewayService.GatewayServiceClient gateway;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="gateway">the gateway gRPC client</param>
        public UnauthenticatedClient(GatewayService.GatewayServiceClient gateway)
        {
            this.gateway = gateway;
        }

        /// <summary>
        /// Gets the default agent request.
        /// </summary>
        /// <returns>The default agent request.</returns>
        public ResolveAliasRequest GetDefaultAgentRequest()
        {

            return new ResolveAliasRequest()
            {

                Alias = new Alias()
                {
                    Type = Alias.Types.Type.Domain,
                    Value = "token.io"
                }

            };
        }

        /// <summary>
        /// Gets the default agent.
        /// </summary>
        /// <returns>The default agent.</returns>
        public Task<string> GetDefaultAgent()
        {
            return gateway.ResolveAliasAsync(GetDefaultAgentRequest())
                 .ToTask(response => response.Member?.Id);
        }


        /// <summary>
        /// Resolve an alias to a TokenMember object, containing member ID and
        /// the alias with the correct type.
        /// </summary>
        /// <param name="alias">alias to resolve</param>
        /// <returns>TokenMember</returns>
        public Task<TokenMember> ResolveAlias(Alias alias)
        {
            var request = new ResolveAliasRequest { Alias = alias };
            return gateway.ResolveAliasAsync(request)
                .ToTask(response => response.Member);
        }

        /// <summary>
        /// Looks up member id for a given alias.
        /// </summary>
        /// <param name="alias">the alias to check</param>
        /// <returns>member id if alias already exists, null otherwise</returns>
        public Task<string> GetMemberId(Alias alias)
        {
            var request = new ResolveAliasRequest { Alias = alias };
            return gateway.ResolveAliasAsync(request)
                .ToTask(response => response.Member!=null?
                response.Member.Id:throw new  MemberNotFoundException(alias));
        }

        /// <summary>
        /// Creates the member identifier.
        /// </summary>
        /// <returns>The member identifier.</returns>
        /// <param name="createMemberType">Create member type.</param>
        /// <param name="tokenRequestId">Token request identifier.</param>
        /// <param name="partnerId">Partner identifier.</param>
        /// <param name="realmId">Realm identifier.</param>
        public Task<string> CreateMemberId(CreateMemberType createMemberType,
            string tokenRequestId = null,
            string partnerId=null,
            string realmId=null)
        {
            var request = new CreateMemberRequest
            {
                Nonce = Util.Nonce(),
                MemberType = createMemberType,
                TokenRequestId= tokenRequestId??"",
                PartnerId=partnerId??"",
                RealmId=realmId??""
            };
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
        public Task<ProtoMember> CreateMember(
            string memberId,
            IList<MemberOperation> operations,
            IList<MemberOperationMetadata> metadata,
            ISigner signer)
        {
            var update = new MemberUpdate
            {
                MemberId = memberId,
                Operations = { operations }
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
                Metadata = { metadata }
            };

            return gateway.UpdateMemberAsync(request)
                .ToTask(response => response.Member);
        }

        /// <summary>
        /// Begins account recovery.
        /// </summary>
        /// <param name="alias">the alias used to recover</param>
        /// <returns>the verification ID</returns>
        public Task<string> BeginRecovery(Alias alias)
        {
            var request = new BeginRecoveryRequest { Alias = alias.ToNormalized() };
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
            var request = new GetMemberRequest { MemberId = memberId };
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
        public Task<ProtoMember> CompleteRecovery(
            string memberId,
            IList<MemberRecoveryOperation> recoveryOperations,
            Key privilegedKey,
            ICryptoEngine cryptoEngine)
        {
            var operations = recoveryOperations.Select(re => new MemberOperation { Recover = re }).ToList();
          
            operations.Add(Util.ToAddKeyOperation(privilegedKey));
            operations.Add(Util.ToAddKeyOperation(cryptoEngine.GenerateKey(Level.Standard)));
            operations.Add(Util.ToAddKeyOperation(cryptoEngine.GenerateKey(Level.Low)));

            var signer = cryptoEngine.CreateSigner(Level.Privileged);
            var memberRequest = new GetMemberRequest { MemberId = memberId };
            return gateway.GetMemberAsync(memberRequest)
                .ToTask(response => response.Member)
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
        public Task<ProtoMember> CompleteRecoveryWithDefaultRule(
            string memberId,
            string verificationId,
            string code,
            ICryptoEngine cryptoEngine)
        {
            var privilegedKey = cryptoEngine.GenerateKey(Level.Privileged);
            var standardKey = cryptoEngine.GenerateKey(Level.Standard);
            var lowKey = cryptoEngine.GenerateKey(Level.Low);

            var signer = cryptoEngine.CreateSigner(Level.Privileged);

            var operations = new List<Key> { privilegedKey, standardKey, lowKey }
                .Select(Util.ToAddKeyOperation)
                .ToList();

            var memberRequest = new GetMemberRequest { MemberId = memberId };
            return Util.TwoTasks(
                    gateway.GetMemberAsync(memberRequest)
                    .ToTask(response => response.Member),
                    GetRecoveryAuthorization(verificationId, code, privilegedKey))
                .Map(memberAndEntry =>
                {
                    operations.Add(new MemberOperation { Recover = memberAndEntry.Value });
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
        /// Returns a list of countries with Token-enabled banks.
        /// </summary>
        /// <param name="provider">If specified, return banks whose 'provider' matches the given provider</param>
        /// <returns>a list if country codes</returns>
        public Task<IList<string>> GetCountries(string provider)
        {
            var request = new GetBanksCountriesRequest();
            if (provider != null)
            {
                var filter = new BankFilter
                {
                    Provider = provider
                };
                request.Filter = filter;
            }

            return gateway.GetBanksCountriesAsync(request)
                .ToTask(response => (IList<string>)response.Countries);
        }


    }
}
