using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using log4net;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Tpp.Rpc;
using Tokenio.Utils;
using TokenRequest=Tokenio.TokenRequests.TokenRequest;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;
using Tokenio.Proto.Common.MoneyProtos;

namespace Tokenio.Tpp
{
    public class Member : Tokenio.Member, IRepresentable
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Client client;

        /// <summary>
        /// Creates an instance of <see cref="Member"/>
        /// </summary>
        /// <param name="client">the gRPC client</param>
        public Member(string memberId,
            Client client)
            : base(memberId, client)
        {
            this.client = client;
        }

        /// <summary>
        /// Replaces auth'd member's public profile.
        /// </summary>
        /// <param name="profile">the protile to set</param>
        /// <returns>the updated profile</returns>
        public Task<Profile> SetProfile(Profile profile)
        {
            return client.SetProfile(profile);
        }

        /// <summary>
        /// Replaces auth'd member's public profile.
        /// </summary>
        /// <param name="profile">the protile to set</param>
        /// <returns>the updated profile</returns>
        public Profile SetProfileBlocking(Profile profile)
        {
            return SetProfile(profile).Result;
        }

        /// <summary>
        /// Gets a member's public profile. Unlike setProfile, you can get another member's profile.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <returns>the profile</returns>
        public Task<Profile> GetProfile(string memberId)
        {
            return client.GetProfile(memberId);
        }

        /// <summary>
        /// Gets a member's public profile. Unlike setProfile, you can get another member's profile.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <returns>the profile</returns>
        public Profile GetProfileBlocking(string memberId)
        {
            return GetProfile(memberId).Result;
        }

        public Task SetProfilePicture(string type, byte[] data)
        {
            var payload = new Payload
            {
                OwnerId = MemberId(),
                Type = type,
                Name = "profile",
                Data = ByteString.CopyFrom(data),
                AccessMode = AccessMode.Public
            };
            return client.SetProfilePicture(payload);
        }

        /// <summary>
        /// Replaces auth'd member's public profile picture.
        /// </summary>
        /// <param name="type">MIME type of the picture</param>
        /// <param name="data">the image data</param>
        /// <returns>a task</returns>
        public void SetProfilePictureBlocking(string type, byte[] data)
        {
            SetProfilePicture(type, data).Wait();
        }

        public Task<Blob> GetProfilePicture(string memberId, ProfilePictureSize size)
        {
            return client.GetProfilePicture(memberId, size);
        }

        /// <summary>
        /// Gets a member's public profile picture. Unlike set, you can get another member's picture.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <param name="size">the desired size category (small, medium, large, original)</param>
        /// <returns>a blob with picture; empty if the member has no picture</returns>
        public Blob GetProfilePictureBlocking(string memberId, ProfilePictureSize size)
        {
            return GetProfilePicture(memberId, size).Result;
        }

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of accounts</returns>
        public Task<IList<Account>> GetAccounts()
        {

            return GetAccountsImpl()
            .Map(accounts => (IList<Account>)accounts
            .Select(account => new Account(this, account))
            .ToList());

        }

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of accounts</returns>
        public IList<Account> GetAccountsBlocking()
        {
            return GetAccounts().Result;
        }

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        public Task<Account> GetAccount(string accountId)
        {
            return GetAccountImpl(accountId)
                .Map(account => new Account(this, account));
        }

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        public Account GetAccountBlocking(string accountId)
        {
            return GetAccount(accountId).Result;
        }

        /// <summary>
        /// Retrieves a blob from the server.
        /// </summary>
        /// <param name="blobId">the blob id</param>
        /// <returns>the blob</returns>
        public Task<Blob> GetBlob(string blobId)
        {
            return client.GetBlob(blobId);
        }

        /// <summary>
        /// Retrieves a blob from the server.
        /// </summary>
        /// <param name="blobId">the blob id</param>
        /// <returns>the blob</returns>
        public Blob GetBlobBlocking(string blobId)
        {
            return GetBlob(blobId).Result;
        }

        /// <summary>
        /// Creates a representable that acts as another member.
        /// </summary>
        /// <param name="accessTokenId">the access token id to be used</param>
        /// <param name="customerInitiated">whether the customer initiated the call</param>
        /// <returns>the representable</returns>>
        public IRepresentable ForAccessToken(string accessTokenId, bool customerInitiated = false)
        {
            Client cloned = client.ForAccessToken(accessTokenId, customerInitiated);
            return new Member(accessTokenId,cloned);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(Token token)
        {
            return RedeemTokenInternal(token, null, null, null, null, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(Token token, string refId)
        {
            return RedeemTokenInternal(token, null, null, null, null, refId);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(Token token, TransferDestination destination)
        {
            return RedeemToken(token, null, null, null, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Task<Transfer> RedeemToken(Token token, TransferEndpoint destination)
        {
            return RedeemToken(token, null, null, null, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Task<Transfer> RedeemToken(
            Token token,
            TransferEndpoint destination,
            string refId)
        {
            return RedeemToken(token, null, null, null, destination, refId);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(
            Token token,
            TransferDestination destination,
            string refId)
        {
            return RedeemToken(token, null, null, null, destination, refId);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description)
        {
            return RedeemTokenInternal(token, amount, currency, description, null, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(
            Token token,
            double? amount,
            string currency,
            TransferDestination destination)
        {
            return RedeemToken(token, amount, currency, null, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Task<Transfer> RedeemToken(
            Token token,
            double? amount,
            string currency,
            TransferEndpoint destination)
        {
            return RedeemToken(token, amount, currency, null, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Task<Transfer> RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination)
        {
            return RedeemToken(token, amount, currency, description, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferDestination destination)
        {
            return RedeemToken(token, amount, currency, description, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        /// <remarks>amount, currency, description, destination and refId are nullable</remarks>>
        public Task<Transfer> RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferDestination destination,
            string refId)
        {
            var payload = new TransferPayload
            {
                TokenId = token.Id,
                Description = token.Payload.Description
            };
            if (destination != null)
            {
                payload.TransferDestinations.Add(destination);
            }

            if (amount.HasValue)
            {
                payload.Amount.Value = Util.DoubleToString(amount.Value);
            }

            if (currency != null)
            {
                payload.Amount.Currency = currency;
            }

            if (description != null)
            {
                payload.Description = description;
            }

            if (refId != null)
            {
                payload.RefId = refId;
            }
            else
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Util.Nonce();
            }

            return client.CreateTransfer(payload);
        }

        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Task<Transfer> RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination,
            string refId)
        {
            return RedeemTokenInternal(token, amount, currency, description, destination, refId);
        }


        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        /// <remarks>amount, currency, description, destination and refId are nullable</remarks>>
        public Task<Transfer> RedeemTokenInternal(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination,
            string refId)
        {
            var payload = new TransferPayload
            {
                TokenId = token.Id,
                Description = token.Payload.Description
            };
            if (destination != null)
            {
                payload.Destinations.Add(destination);
            }

            if (amount.HasValue)
            {
                var money = new Money { Value = Util.DoubleToString(amount.Value) };
                payload.Amount = money ;
            }

            if (currency != null)
            {
                payload.Amount.Currency = currency;
            }

            if (description != null)
            {
                payload.Description = description;
            }

            if (refId != null)
            {
                payload.RefId = refId;
            }
            else
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Util.Nonce();
            }

            return client.CreateTransfer(payload);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token)
        {
            return RedeemToken(token).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token, string refId)
        {
            return RedeemToken(token, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token, TransferDestination destination)
        {
            return RedeemToken(token, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(Token token, TransferEndpoint destination)
        {
            return RedeemToken(token, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(
            Token token,
            TransferDestination destination,
            string refId)
        {
            return RedeemToken(token, destination, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(
            Token token,
            TransferEndpoint destination,
            string refId)
        {
            return RedeemToken(token, destination, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            string description)
        {
            return RedeemToken(token, amount, currency, description).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            TransferDestination destination)
        {
            return RedeemToken(token, amount, currency, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            TransferEndpoint destination)
        {
            return RedeemToken(token, amount, currency, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferDestination destination)
        {
            return RedeemToken(token, amount, currency, description, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination)
        {
            return RedeemToken(token, amount, currency, description, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        /// <remarks>amount, currency, description, destination and refId are nullable</remarks>>
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferDestination destination,
            string refId)
        {
            return RedeemToken(token, amount, currency, description, destination, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        /// <remarks>amount, currency, description, destination and refId are nullable</remarks>>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination,
            string refId)
        {
            return RedeemToken(token, amount, currency, description, destination, refId).Result;
        }

        /// <summary>
        /// Stores a token request.
        /// </summary>
        /// <param name="requestPayload">the token request payload (immutable fields)</param>
        /// <param name="requestOptions">the token request options (mutable with UpdateTokenRequest)</param>
        /// <returns>an id to reference the token request</returns>
        public Task<string> StoreTokenRequest(
            TokenRequestPayload requestPayload,
            Proto.Common.TokenProtos.TokenRequestOptions requestOptions)
        {
            return client.StoreTokenRequest(requestPayload, requestOptions);
        }

        /// <summary>
        /// Stores a token request.
        /// </summary>
        /// <param name="requestPayload">the token request payload (immutable fields)</param>
        /// <param name="requestOptions">the token request options (mutable with UpdateTokenRequest)</param>
        /// <returns>an id to reference the token request</returns>
        public string StoreTokenRequestBlocking(
            TokenRequestPayload requestPayload,
            Proto.Common.TokenProtos.TokenRequestOptions requestOptions)
        {
            return StoreTokenRequest(requestPayload, requestOptions).Result;
        }

        /// <summary>
        /// Stores a token request.
        /// </summary>
        /// <param name="tokenRequest">the token request</param>
        /// <returns>an id to reference the token request</returns>
        [Obsolete("Deprecated. Use StoreTokenRequest(Tokenio/TokenRequest) instead.")]
        public Task<string> StoreTokenRequest(Proto.Common.TokenProtos.TokenRequest tokenRequest)
        {
            return client.StoreTokenRequest(
                tokenRequest.Payload,
                tokenRequest.Options);
        }

        /// Stores a token request.
        /// </summary>
        /// <param name="tokenRequest">the token request</param>
        /// <returns>an id to reference the token request</returns>
        [Obsolete("Deprecated. Use StoreTokenRequestBlocking(Tokenio/TokenRequest) instead.")]
        public string StoreTokenRequestBlocking(Proto.Common.TokenProtos.TokenRequest tokenRequest)
        {
            return StoreTokenRequest(tokenRequest).Result;
        }


        /// <summary>
        /// Stores a token request.
        /// </summary>
        /// <param name="tokenRequest">the token request</param>
        /// <returns>an id to reference the token request</returns>
        public Task<string> StoreTokenRequest(TokenRequest tokenRequest)
        {
            return client.StoreTokenRequest(
                tokenRequest.GetTokenRequestPayload(),
                tokenRequest.GetTokenRequestOptions());
        }

        /// Stores a token request.
        /// </summary>
        /// <param name="tokenRequest">the token request</param>
        /// <returns>an id to reference the token request</returns>
        public string StoreTokenRequestBlocking(TokenRequest tokenRequest)
        {
            return StoreTokenRequest(tokenRequest).Result;
        }

        /// <summary>
        /// Creates a customization
        /// </summary>
        /// <param name="logo">logo</param>
        /// <param name="colors">map of ARGB colors #AARRGGBB</param>
        /// <param name="consentText">consent text</param>
        /// <param name="name">display name</param>
        /// <param name="appName">corresponding app name</param>
        /// <returns>customization id</returns>
        public Task<string> CreateCustomization(
            Payload logo,
            MapField<string, string> colors,
            string consentText,
            string name,
            string appName)
        {
            return client.CreateCustomization(logo, colors, consentText, name, appName);
        }

        /// <summary>
        /// Creates a customization
        /// </summary>
        /// <param name="logo">logo</param>
        /// <param name="colors">map of ARGB colors #AARRGGBB</param>
        /// <param name="consentText">consent text</param>
        /// <param name="name">display name</param>
        /// <param name="appName">corresponding app name</param>
        /// <returns>customization id</returns>
        public string CreateCustomizationBlocking(
            Payload logo,
            MapField<string, string> colors,
            string consentText,
            string name,
            string appName)
        {
            return CreateCustomization(logo, colors, consentText, name, appName).Result;
        }

        /// <summary>
        /// Looks up an existing token transfer.
        /// </summary>
        /// <param name="transferId">the transfer id</param>
        /// <returns>the transfer record</returns>
        public Task<Transfer> GetTransfer(string transferId)
        {
            return client.GetTransfer(transferId);
        }

        /// <summary>
        /// Looks up an existing token transfer.
        /// </summary>
        /// <param name="transferId">the transfer id</param>
        /// <returns>the transfer record</returns>
        public Transfer GetTransferBlocking(string transferId)
        {
            return GetTransfer(transferId).Result;
        }

        /// <summary>
        /// Looks up existing token transfers.
        /// </summary>
        /// <param name="tokenId">nullable token id</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>a paged list of transfers</returns>
        public Task<PagedList<Transfer>> GetTransfers(
            string tokenId,
            string offset,
            int limit)
        {
            return client.GetTransfers(tokenId, offset, limit);
        }

        /// <summary>
        /// Looks up existing token transfers.
        /// </summary>
        /// <param name="tokenId">nullable token id</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>a paged list of transfers</returns>
        public PagedList<Transfer> GetTransfersBlocking(
            string tokenId,
            string offset,
            int limit)
        {
            return GetTransfers(tokenId, offset, limit).Result;
        }

        /// <summary>
        /// Looks up existing access tokens.
        /// </summary>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">the max number of records to return</param>
        /// <returns>a paged list of access tokens</returns>
        public Task<PagedList<Token>> GetAccessTokens(string offset, int limit)
        {
            return client.GetTokens(TokenType.Access, limit, offset);
        }

        /// <summary>
        /// Looks up existing access tokens.
        /// </summary>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">the max number of records to return</param>
        /// <returns>a paged list of access tokens</returns>
        public PagedList<Token> GetAccessTokensBlocking(string offset, int limit)
        {
            return GetAccessTokens(offset, limit).Result;
        }

        /// <summary>
        /// Looks up exsiting transfer tokens.
        /// </summary>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">the max number of records to return</param>
        /// <returns>a paged list of transfer tokens</returns>
        public Task<PagedList<Token>> GetTransferTokens(string offset, int limit)
        {
            return client.GetTokens(TokenType.Transfer, limit, offset);
        }

        /// <summary>
        /// Looks up exsiting transfer tokens.
        /// </summary>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">the max number of records to return</param>
        /// <returns>a paged list of transfer tokens</returns>
        public PagedList<Token> GetTransferTokensBlocking(string offset, int limit)
        {
            return GetTransferTokens(offset, limit).Result;
        }

        /// <summary>
        /// Looks up an existing token.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>the token</returns>
        public Task<Token> GetToken(string tokenId)
        {
            return client.GetToken(tokenId);
        }

        /// <summary>
        /// Looks up an existing token.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>the token</returns>
        public Token GetTokenBlocking(string tokenId)
        {
            return GetToken(tokenId).Result;
        }

        /// <summary>
        /// Cancels a token.
        /// </summary>
        /// <param name="token">the token to cancel</param>
        /// <returns>the result of the cancel operation</returns>
        public Task<TokenOperationResult> CancelToken(Token token)
        {
            return client.CancelToken(token);
        }

        /// <summary>
        /// Cancels a token.
        /// </summary>
        /// <param name="token">the token to cancel</param>
        /// <returns>the result of the cancel operation</returns>
        public TokenOperationResult CancelTokenBlocking(Token token)
        {
            return CancelToken(token).Result;
        }

        /// <summary>
        /// Trigger a step up notification for balance requests
        /// </summary>
        /// <param name="accountIds">list of account ids</param>
        /// <returns>notification status</returns>
        public Task<NotifyStatus> TriggerBalanceStepUpNotification(IList<string> accountIds)
        {
            return client.TriggerBalanceStepUpNotification(accountIds);
        }

        /// <summary>
        /// Trigger a step up notification for balance requests
        /// </summary>
        /// <param name="accountIds">list of account ids</param>
        /// <returns>notification status</returns>
        public NotifyStatus TriggerBalanceStepUpNotificationBlocking(IList<string> accountIds)
        {
            return TriggerBalanceStepUpNotification(accountIds).Result;
        }

        /// <summary>
        /// Trigger a step up notification for transaction requests
        /// </summary>
        /// <param name="accountIds">account ids</param>
        /// <returns>notification status</returns>
        public Task<NotifyStatus> TriggerTransactionStepUpNotification(string accountId)
        {
            return client.TriggerTransactionStepUpNotification(accountId);
        }

        /// <summary>
        /// Trigger a step up notification for transaction requests
        /// </summary>
        /// <param name="accountIds">account ids</param>
        /// <returns>notification status</returns>
        public NotifyStatus TriggerTransactionStepUpNotificationBlocking(string accountId)
        {
            return TriggerTransactionStepUpNotification(accountId).Result;
        }

        public Task<Token> GetActiveAccessToken(string toMemberId)
        {
            return client.GetActiveAccessToken(toMemberId);
        }

        public Token GetActiveAccessTokenBlocking(string toMemberId)
        {
            return GetActiveAccessToken(toMemberId).Result;
        }

        public Task<Account> CreateTestBankAccount(double balance, string currency)
        {
            return CreateTestBankAccountImpl(balance, currency)
                .Map(acc => new Account(this, acc));
        }

        public Account CreateTestBankAccountBlocking(double balance, string currency)
        {
            return CreateTestBankAccount(balance, currency).Result;
        }
    }
}
