using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using log4net;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.EidasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SubmissionProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Tpp.Rpc;
using Tokenio.Utils;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using TokenRequest = Tokenio.TokenRequests.TokenRequest;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;

namespace Tokenio.Tpp
{
    /// <summary>
    /// Represents a Member in the Token system. Each member has an active secret
    /// and public key pair that is used to perform authentication.
    /// </summary>
    public class Member : Tokenio.Member, IRepresentable
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Client client;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Tokenio.Tpp.Member"/> class.
        /// </summary>
        /// <param name="memberId">member ID</param>
        /// <param name="client">RPC client used to perform operations against the server.</param>
        /// <param name="tokenCluster">Token cluster, e.g. sandbox, production.</param>
        /// <param name="partnerId">member ID of partner.</param>
        /// <param name="realmId">Realm identifier.</param>
        public Member(string memberId,
            Client client,
            TokenCluster tokenCluster,
            string partnerId = null,
            string realmId = null)
            : base(memberId, client, tokenCluster, partnerId, realmId)
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
        /// Replaces auth'd member's public profile picture.
        /// </summary>
        /// <param name="type">MIME type of picture</param>
        /// <param name="data">image data</param>
        /// <returns>task that indicates whether the operation finished or had an error</returns>
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
        /// Creates a {@link Representable} that acts as another member using the access token
        /// that was granted by that member.
        /// </summary>
        /// <param name="accessTokenId">the access token id to be used</param>
        /// <param name="customerInitiated">whether the customer initiated the call</param>
        /// <returns>the representable</returns>>
        public IRepresentable ForAccessToken(string accessTokenId, bool customerInitiated = false)
        {
            Client cloned = client.ForAccessToken(accessTokenId, customerInitiated);
            return new Member(memberId, cloned, tokenCluster, partnerId, realmId);
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
            else if (!string.IsNullOrEmpty(token.Payload.RefId))
            {
                payload.RefId = token.Payload.RefId;
            }
            else
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Util.Nonce();
            }

            return client.CreateTransfer(payload);
        }

        /// <summary>
        /// Redeems the token.
        /// </summary>
        /// <returns>The token.</returns>
        /// <param name="token">Token.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="currency">Currency.</param>
        /// <param name="description">Description.</param>
        /// <param name="refId">Reference identifier.</param>
        public Task<Transfer> RedeemToken(
           Token token,
           double? amount,
           string currency = null,
           string description = null,
           string refId = null)
        {
            return RedeemTokenInternal(token, amount, currency, description, null, refId);
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
                payload.Amount = money;
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

        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount = null,
            string currency = null,
            string description = null,
            string refId = null)
        {
            return RedeemToken(token, amount, currency, description, refId).Result;
        }

        /// <summary>
        /// Redeems a standing order token.
        /// </summary>
        /// <param name="tokenId">ID of token to redeem</param>
        /// <returns>standing order submission</returns>
        public Task<StandingOrderSubmission> RedeemStandingOrderToken(string tokenId)
        {
            return client.CreateStandingOrder(tokenId);
        }

        /// <summary>
        /// Redeems a standing order token.
        /// </summary>
        /// <param name="tokenId">ID of token to redeem</param>
        /// <returns>standing order submission</returns>
        public StandingOrderSubmission RedeemStandingOrderTokenBlocking(string tokenId)
        {
            return RedeemStandingOrderToken(tokenId).Result;
        }

        /// <summary>
        /// Stores a token request.
        /// </summary>
        /// <param name="requestPayload">the token request payload (immutable fields)</param>
        /// <param name="requestOptions">the token request options (mutable with UpdateTokenRequest)</param>
        /// <returns>an id to reference the token request</returns>
        public Task<string> StoreTokenRequest(
            TokenRequestPayload requestPayload,
            TokenRequestOptions requestOptions)
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
            TokenRequestOptions requestOptions)
        {
            return StoreTokenRequest(requestPayload, requestOptions).Result;
        }

        /// <summary>
        /// Stores a token request. This can be retrieved later by the token request id.
        /// </summary>
        /// <param name="tokenRequest">the token request</param>
        /// <returns>an id to reference the token request</returns>
        public Task<string> StoreTokenRequest(TokenRequest tokenRequest)
        {
            return client.StoreTokenRequest(
                tokenRequest.GetTokenRequestPayload(),
                tokenRequest.GetTokenRequestOptions());
        }

        /// <summary>
        /// Stores a token request to be retrieved later (possibly by another member).
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
        /// Looks up an existing Token standing order submission.
        /// </summary>
        /// <param name="submissionId">ID of the standing orde submission</param>
        /// <returns>standing order submission</returns>
        public Task<StandingOrderSubmission> GetStandingOrderSubmission(string submissionId)
        {
            return client.GetStandingOrderSubmission(submissionId);
        }

        /// <summary>
        /// Looks up an existing Token standing order submission.
        /// </summary>
        /// <param name="submissionId">ID of the standing orde submission</param>
        /// <returns>standing order submission</returns>
        public StandingOrderSubmission GetStandingOrderSubmissionBlocking(string submissionId)
        {
            return GetStandingOrderSubmission(submissionId).Result;
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
        /// Looks up existing Token standing order submissions.
        /// </summary>
        /// <param name="limit">max number of submissions to return</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>standing order submissions</returns>
        public Task<PagedList<StandingOrderSubmission>> GetStandingOrderSubmissions(
                int limit,
                string offset = null)
        {
            return client.GetStandingOrderSubmissions(limit, offset);
        }

        /// <summary>
        /// Looks up existing Token standing order submissions.
        /// </summary>
        /// <param name="limit">max number of submissions to return</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>standing order submissions</returns>
        public PagedList<StandingOrderSubmission> GetStandingOrderSubmissionsBlocking(
                int limit,
                string offset = null)
        {
            return GetStandingOrderSubmissions(limit, offset).Result;
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
        /// Looks up transfer tokens owned by the member.
        /// </summary>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">the max number of records to return</param>
        /// <returns>a paged list of transfer tokens</returns>
        public Task<PagedList<Token>> GetTransferTokens(string offset, int limit)
        {
            return client.GetTokens(TokenType.Transfer, limit, offset);
        }

        /// <summary>
        /// Looks up tokens owned by the member.
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
        /// <param name="accountId">account ids</param>
        /// <returns>notification status</returns>
        public Task<NotifyStatus> TriggerTransactionStepUpNotification(string accountId)
        {
            return client.TriggerTransactionStepUpNotification(accountId);
        }

        /// <summary>
        /// Trigger a step up notification for transaction requests
        /// </summary>
        /// <param name="accountId">account ids</param>
        /// <returns>notification status</returns>
        public NotifyStatus TriggerTransactionStepUpNotificationBlocking(string accountId)
        {
            return TriggerTransactionStepUpNotification(accountId).Result;
        }


        /// <summary>
        /// Looks up a existing access token where the calling member is the grantor and given member is
        /// the grantee.
        /// </summary>
        /// <returns>token returned by the server.</returns>
        /// <param name="toMemberId">beneficiary of the active access token.</param>
        public Task<Token> GetActiveAccessToken(string toMemberId)
        {
            return client.GetActiveAccessToken(toMemberId);
        }


        /// <summary>
        /// Looks up a existing access token where the calling member is the grantor and given member is
        /// the grantee.
        /// </summary>
        /// <returns>The active access token blocking.</returns>
        /// <param name="toMemberId">token returned by the server.</param>
        public Token GetActiveAccessTokenBlocking(string toMemberId)
        {
            return GetActiveAccessToken(toMemberId).Result;
        }


        /// <summary>
        /// Creates a test bank account in a fake bank and links the account.
        /// </summary>
        /// <returns>The test bank account.</returns>
        /// <param name="balance">Balance.</param>
        /// <param name="currency">Currency  e.g. "EUR".</param>
        public Task<Account> CreateTestBankAccount(double balance, string currency)
        {
            return CreateTestBankAccountImpl(balance, currency)
                .Map(acc => new Account(this, acc));
        }


        /// <summary>
        ///  Creates a test bank account in a fake bank and links the account.
        /// </summary>
        /// <returns>The linked account.</returns>
        /// <param name="balance">account balance to set.</param>
        /// <param name="currency">currency code, e.g. "EUR".</param>
        public Account CreateTestBankAccountBlocking(double balance, string currency)
        {
            return CreateTestBankAccount(balance, currency).Result;
        }

        /// <summary>
        /// Verifies eIDAS alias with an eIDAS certificate, containing auth number equal to the value
        ///of the alias.
        ///An eIDAS-type alias containing auth number of the TPP should be added to the
        ///member before making this call.The member must be under the realm of a bank.
        /// </summary>
        /// <returns>The eidas.</returns>
        /// <param name="payload">payload payload containing the member id and the certificate in PEM format.</param>
        /// <param name="signature">signature the payload signed with a private key corresponding to the certificate.</param>
        public Task VerifyEidas(
            VerifyEidasPayload payload,
            string signature)
        {
            return client.VerifyEidas(payload, signature);
        }
    }
}