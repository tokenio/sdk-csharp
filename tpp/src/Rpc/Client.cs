using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Exceptions;
using Tokenio.Security;
using Tokenio.Rpc;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Gateway.GetTransfersRequest.Types;
using TokenAction = Tokenio.Proto.Common.TokenProtos.TokenSignature.Types.Action;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;
using Tokenio.Proto.Common.NotificationProtos;

namespace Tokenio.Tpp.Rpc
{
    /// <summary>
    /// An authenticated RPC client that is used to talk to Token gateway. The
    /// class is a thin wrapper on top of gRPC generated client. Makes the API
    /// easier to use.
    /// </summary>
    public sealed class Client : Tokenio.Rpc.Client
    {
        private SecurityMetadata securityMetadata = new SecurityMetadata();

        /// <summary>
        /// Instantiates a client.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="cryptoEngine">the crypto engine used to sign for authentication, request
        /// payloads, etc</param>
        /// <param name="channel">managed channel</param>
        public Client(string memberId, ICryptoEngine cryptoEngine, ManagedChannel channel)
            : base(memberId, cryptoEngine, channel)
        {
        }

        /// <summary>
        /// Replaces a member's public profile.
        /// </summary>
        /// <param name="profile">the profile to set</param>
        /// <returns>the profile that was set</returns>
        public Task<Profile> SetProfile(Profile profile)
        {
            var request = new SetProfileRequest { Profile = profile };
            return gateway(authenticationContext()).SetProfileAsync(request)
                .ToTask(response => response.Profile);
        }

        /// <summary>
        /// Gets a member's public profile.
        /// </summary>
        /// <param name="memberId">the member id of the member</param>
        /// <returns>the profile</returns>
        public Task<Profile> GetProfile(string memberId)
        {
            var request = new GetProfileRequest { MemberId = memberId };
            return gateway(authenticationContext()).GetProfileAsync(request)
                .ToTask(response => response.Profile);
        }

        /// <summary>
        /// Replaces a member's public profile picture.
        /// </summary>
        /// <param name="payload">the blob payload</param>
        /// <returns>a task</returns>
        public Task SetProfilePicture(Payload payload)
        {
            var request = new SetProfilePictureRequest { Payload = payload };
            return gateway(authenticationContext()).SetProfilePictureAsync(request).ToTask();
        }

        /// <summary>
        /// Gets a member's public profile picture.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="size">the desired size(small, medium, large, original)</param>
        /// <returns>blob with picture; empty blob (no fields set) if has no picture</returns>
        public Task<Blob> GetProfilePicture(string memberId, ProfilePictureSize size)
        {
            var request = new GetProfilePictureRequest
            {
                MemberId = memberId,
                Size = size
            };
            return gateway(authenticationContext()).GetProfilePictureAsync(request)
                .ToTask(response => response.Blob);
        }

        /// <summary>
        /// Retrieves a blob from the server.
        /// </summary>
        /// <param name="blobId">the blob id</param>
        /// <returns>the blob</returns>
        public Task<Blob> GetBlob(string blobId)
        {
            var request = new GetBlobRequest { BlobId = blobId };
            return gateway(authenticationContext()).GetBlobAsync(request)
                .ToTask(response => response.Blob);
        }

        public Client ForAccessToken(string tokenId, bool customerInitiated)
        {
            Client updated = new Client(MemberId, cryptoEngine, channel);
            updated.UseAccessToken(tokenId, customerInitiated);
            updated.SetSecurityMetadata(securityMetadata);
            return updated;
        }

        /// <summary>
        /// Sets the On-Behalf-Of authentication value to be used with this client.
        /// The value must correspond to an existing Access Token ID issued for the
        /// client member. Sets customer initiated to false if not specified.
        /// </summary>
        /// <param name="accessTokenId">the access token id to be used</param>
        /// <param name="customerInitiated">whether the customer initiated the calls</param>
        public void UseAccessToken(string accessTokenId, bool customerInitiated = false)
        {
            this.onBehalfOf = accessTokenId;
            this.customerInitiated = customerInitiated;
        }

        /// <summary>
        /// Stores a transfer token request.
        /// </summary>
        /// <param name="payload">the token request payload (immutable fields)</param>
        /// <param name="options">the token request options (mutable fields)</param>
        /// <returns>an id to reference the token request</returns>
        public Task<string> StoreTokenRequest(
            TokenRequestPayload payload,
            Proto.Common.TokenProtos.TokenRequestOptions options)
        {
            var request = new StoreTokenRequestRequest
            {
                RequestPayload = payload,
                RequestOptions = options
            };

            return gateway(authenticationContext()).StoreTokenRequestAsync(request)
                .ToTask(response => response.TokenRequest.Id);
        }

        /// <summary>
        /// **DEPRECATED** Stores a transfer token request.
        /// </summary>
        /// <param name="payload">the transfer token payload</param>
        /// <param name="options">a map of options</param>
        /// <returns>an id to reference the token request</returns>
        [Obsolete("Deprecated. Use StoreTokenRequest(TokenRequestPayload, TokenRequestOptions) instead.")]
        public Task<string> StoreTokenRequest(
            TokenPayload payload,
            IDictionary<string, string> options)
        {
            var request = new StoreTokenRequestRequest
            {
                Payload = payload,
                Options = { options }
            };
            return gateway(authenticationContext()).StoreTokenRequestAsync(request)
                .ToTask(response => response.TokenRequest.Id);
        }

        public Task<string> CreateCustomization(
            Payload logo,
            MapField<string, string> colors,
            string consentText,
            string name,
            string appName)
        {
            var request = new CreateCustomizationRequest
            {
                Logo = logo,
                Colors = { colors },
                Name = name,
                ConsentText = consentText,
                AppName = appName
            };
            return gateway(authenticationContext())
                .CreateCustomizationAsync(request)
                .ToTask(response => response.CustomizationId);
        }

        public Task<Token> GetToken(string tokenId)
        {
            var request = new GetTokenRequest { TokenId = tokenId };
            return gateway(authenticationContext()).GetTokenAsync(request)
                .ToTask(response => response.Token);
        }

        /// <summary>
        /// Looks up a list of existing token.
        /// </summary>
        /// <param name="type">the token type</param>
        /// <param name="limit">the max number of records to return</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <returns>the tokens in paged list</returns>
        public Task<PagedList<Token>> GetTokens(
            TokenType type,
            int limit,
            string offset)
        {
            var request = new GetTokensRequest
            {
                Type = type,
                Page = new Page
                {
                    Limit = limit
                }
            };
            if (offset != null)
            {
                request.Page.Offset = offset;
            }

            return gateway(authenticationContext()).GetTokensAsync(request)
                .ToTask(response => new PagedList<Token>(response.Tokens, response.Offset));
        }

        /// <summary>
        /// Looks up an existing transfer.
        /// </summary>
        /// <param name="transferId">the transfer id</param>
        /// <returns>the transfer record</returns>
        public Task<Transfer> GetTransfer(string transferId)
        {
            var request = new GetTransferRequest { TransferId = transferId };
            return gateway(authenticationContext()).GetTransferAsync(request)
                .ToTask(response => response.Transfer);
        }

        /// <summary>
        /// Looks up a list of existing transfers.
        /// </summary>
        /// <param name="tokenId">nullable token id</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns></returns>
        public Task<PagedList<Transfer>> GetTransfers(
            string tokenId,
            string offset,
            int limit)
        {
            var request = new GetTransfersRequest
            {
                Page = new Page
                {
                    Limit = limit
                }
            };
            if (tokenId != null)
            {
                request.Filter = new TransferFilter { TokenId = tokenId };
            }

            if (offset != null)
            {
                request.Page.Offset = offset;
            }

            return gateway(authenticationContext()).GetTransfersAsync(request)
                .ToTask(response => new PagedList<Transfer>(response.Transfers, response.Offset));
        }

        public void SetSecurityMetadata(SecurityMetadata securityMetadata)
        {
            this.securityMetadata = securityMetadata;
        }

        public void ClearSecurityMetaData()
        {
            this.securityMetadata = new SecurityMetadata();
        }

        /// <summary>
        /// Creates a new transfer token.
        /// </summary>
        /// <param name="payload">the transfer token payload</param>
        /// <returns>the transfer token</returns>
        /// <exception cref="TransferTokenException"></exception>
        public Task<Token> CreateTransferToken(TokenPayload payload)
        {
            var request = new CreateTransferTokenRequest { Payload = payload };
            return gateway(authenticationContext()).CreateTransferTokenAsync(request)
                .ToTask(response =>
                {
                    if (response.Status != TransferTokenStatus.Success)
                    {
                        throw new TransferTokenException(response.Status);
                    }

                    return response.Token;
                });
        }

        /// <summary>
        /// Creates a new transfer token with a token request id.
        /// </summary>
        /// <param name="payload">the transfer token payload</param>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the transfer payload</returns>
        /// <exception cref="TransferTokenException"></exception>
        public Task<Token> CreateTransferToken(TokenPayload payload, string tokenRequestId)
        {
            var request = new CreateTransferTokenRequest
            {
                Payload = payload,
                TokenRequestId = tokenRequestId
            };
            return gateway(authenticationContext()).CreateTransferTokenAsync(request)
                .ToTask(response =>
                {
                    if (response.Status != TransferTokenStatus.Success)
                    {
                        throw new TransferTokenException(response.Status);
                    }

                    return response.Token;
                });
        }

        /// <summary>
        /// Cancels a token.
        /// </summary>
        /// <param name="token">the token to cancel</param>
        /// <returns>the result of the cancel operation</returns>
        public Task<TokenOperationResult> CancelToken(Token token)
        {
            var signer = cryptoEngine.CreateSigner(Level.Low);
            var request = new CancelTokenRequest
            {
                TokenId = token.Id,
                Signature = new Signature
                {
                    MemberId = MemberId,
                    KeyId = signer.GetKeyId(),
                    Signature_ = signer.Sign(Stringify(token, TokenAction.Cancelled))
                }
            };
            return gateway(authenticationContext()).CancelTokenAsync(request)
                .ToTask(response => response.Result);
        }

        public Task<NotifyStatus> TriggerBalanceStepUpNotification(IList<string> accountIds)
        {
            var request = new TriggerStepUpNotificationRequest
            {
                BalanceStepUp = new BalanceStepUp
                {
                    AccountId = { accountIds }
                }
            };

            return gateway(authenticationContext())
                .TriggerStepUpNotificationAsync(request)
                .ToTask(response => response.Status);
        }

        public Task<NotifyStatus> TriggerTransactionStepUpNotification(string accountId)
        {
            var request = new TriggerStepUpNotificationRequest
            {
                TransactionStepUp = new TransactionStepUp
                {
                    AccountId = accountId
                }
            };

            return gateway(authenticationContext())
                .TriggerStepUpNotificationAsync(request)
                .ToTask(response => response.Status);
        }

        protected override  string GetOnBehalfOf()
        {
            return onBehalfOf;
        }

        public Task<Token> GetActiveAccessToken(string toMemberId)
        {
            var request = new GetActiveAccessTokenRequest
            {
                ToMemberId = toMemberId
            };

            return gateway(authenticationContext())
                .GetActiveAccessTokenAsync(request)
                .ToTask(response => response.Token);
        }

    }
}
