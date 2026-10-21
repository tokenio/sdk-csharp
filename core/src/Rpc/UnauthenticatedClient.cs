using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tokenio.Exceptions;
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

namespace Tokenio.Rpc
{
    /// <summary>
    /// Similar to <see cref="Client"/> but is only used for a handful of requests that
    /// don't require authentication. We use this client to create new member or getMember
    /// an existing one and switch to the authenticated <see cref="Client"/>.
    /// </summary>
    public class UnauthenticatedClient
    {
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
        /// Creates the member identifier.
        /// </summary>
        /// <returns>The member identifier.</returns>
        /// <param name="createMemberType">Create member type.</param>
        /// <param name="tokenRequestId">Token request identifier.</param>
        /// <param name="partnerId">Partner identifier.</param>
        /// <param name="realmId">Realm identifier.</param>
        public Task<string> CreateMemberId(CreateMemberType createMemberType,
            string tokenRequestId = null,
            string partnerId = null,
            string realmId = null)
        {
            var request = new CreateMemberRequest
            {
                Nonce = Util.Nonce(),
                MemberType = createMemberType,
                TokenRequestId = tokenRequestId ?? "",
                PartnerId = partnerId ?? "",
                RealmId = realmId ?? ""
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
        /// Returns a list of token enabled banks.
        /// </summary>
        /// <param name="ids">If specified, return banks whose 'id' matches any one of the given ids
        ///   (case-insensitive). Can be at most 1000.</param>
        /// <param name="search">If specified, return banks whose 'name' or 'identifier' contains the given
        ///     search string (case-insensitive)</param>
        /// <param name="country">If specified, return banks whose 'country' matches the given ISO 3166-1
        ///     alpha-2 country code(case-insensitive)</param>
        /// <param name="page">Result page to retrieve. Default to 1 if not specified.</param>
        /// <param name="perPage">Maximum number of records per page. Can be at most 200. Default to 200
        ///     if not specified.</param>
        /// <param name="sort">The key to sort the results. Could be one of: name, provider and country.
        ///     Defaults to name if not specified.</param>
        /// <param name="provider">If specified, return banks whose 'provider' matches the given provider
        ///     (case insensitive).</param>
        /// <param name="bankFeatures">If specified, return banks who meet the bank features requirement.</param>
        /// <returns>banks with paging information</returns>
        public Task<PagedBanks> GetBanks(
            IList<string> ids,
            string search,
            string country,
            int? page,
            int? perPage,
            string sort,
            string provider,
            BankFeatures bankFeatures)
        {
            var request = new GetBanksRequest()
            {
                Filter = new BankFilter(),
            };
            
            if (ids != null)
            {
                request.Filter.Ids.Add(ids);
            }

            if (search != null)
            {
                request.Filter.Search = search;
            }

            if (country != null)
            {
                request.Filter.Country = country;
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

            if (provider != null)
            {
                request.Filter.Provider = provider;
            }

            if (bankFeatures != null)
            {
                request.Filter.BankFeatures = bankFeatures;
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
