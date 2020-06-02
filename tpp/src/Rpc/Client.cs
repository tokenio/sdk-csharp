using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Grpc.Core;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.EidasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.SubmissionProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Rpc;
using Tokenio.Security;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Gateway.GetTransfersRequest.Types;
using TokenAction = Tokenio.Proto.Common.TokenProtos.TokenSignature.Types.Action;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;
using WebhookConfig = Tokenio.Proto.Common.WebhookProtos.Webhook.Types.Config;

namespace Tokenio.Tpp.Rpc
{
    /// <summary>
    /// An authenticated RPC client that is used to talk to Token gateway. The
    /// class is a thin wrapper on top of gRPC generated client. Makes the API
    /// easier to use.
    /// </summary>
    public sealed class Client : Tokenio.Rpc.Client
    {
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

        /// <summary>
        /// Creates a new instance with On-Behalf-Of authentication set.
        /// </summary>
        /// <param name="tokenId">access token ID to be used</param>
        /// <param name="customerInitiated">whether the customer initiated the calls</param>
        /// <returns>new client instance</returns>
        public Client ForAccessToken(string tokenId, bool customerInitiated)
        {
            Client updated = new Client(MemberId, cryptoEngine, channel);
            updated.UseAccessToken(tokenId, customerInitiated);
            return updated;
        }

        /// <summary>
        /// Creates a new instance with On-Behalf-Of authentication set.
        /// </summary>
        /// <param name="tokenId">access token ID to be used</param>
        /// <param name="customerTrackingMetadata">customer tracking metadata</param>
        /// <returns>new client instance</returns>
        public Client ForAccessToken(
            string tokenId,
            CustomerTrackingMetadata customerTrackingMetadata)
        {
            if (customerTrackingMetadata.Equals(new CustomerTrackingMetadata()))
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument,
                        "User tracking metadata is empty. "
                            + "Use forAccessToken(String, boolean) instead."));
            var updated = new Client(MemberId, cryptoEngine, channel);
            updated.UseAccessToken(tokenId, customerTrackingMetadata);
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
        /// Sets the On-Behalf-Of authentication value to be used
        /// with this client.The value must correspond to an existing
        /// Access Token ID issued for the client member.Uses the given customer
        /// initiated flag.
        ///
        /// </summary>
        /// <param name="accessTokenId">the access token id to be used</param>
        /// <param name="customerTrackingMetadata">the tracking metadata of the customer</param>
        private void UseAccessToken(
            string accessTokenId,
            CustomerTrackingMetadata customerTrackingMetadata)
        {
            onBehalfOf = accessTokenId;
            customerInitiated = true;
            this.customerTrackingMetadata = customerTrackingMetadata;
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
        /// Sets destination accounts for once if it hasn't been set.
        /// </summary>
        /// <param name="tokenRequestId">token request Id</param>
        /// <param name="transferDestinations">destination accounts</param>
        /// <returns>Task that completes when request handled</returns>
        public Task SetTokenRequestTransferDestinations(
            string tokenRequestId,
            IList<TransferDestination> transferDestinations)
        {
            return gateway(authenticationContext())
                .SetTokenRequestTransferDestinationsAsync(
                    new SetTokenRequestTransferDestinationsRequest
                    {
                        TokenRequestId = tokenRequestId,
                        TransferDestinations = { transferDestinations }
                    })
                .ToTask();
        }

        /// <summary>
        /// Creates the customization.
        /// </summary>
        /// <returns>The customization.</returns>
        /// <param name="logo">Logo.</param>
        /// <param name="colors">map of ARGB colors #AARRGGBB.</param>
        /// <param name="consentText">Consent text.</param>
        /// <param name="name">Display Name.</param>
        /// <param name="appName">Corresponding App name.</param>
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

        /// <summary>
        /// Looks up a existing token.
        /// </summary>
        /// <returns>The token returned by server.</returns>
        /// <param name="tokenId">Token id</param>
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
        /// Looks up an existing bulk transfer.
        /// </summary>
        /// <param name="bulkTransferId">bulk transfer ID</param>
        /// <returns>bulk transfer record</returns>
        public Task<BulkTransfer> GetBulkTransfer(string bulkTransferId)
        {
            return gateway(authenticationContext())
                .GetBulkTransferAsync(new GetBulkTransferRequest
                {
                    BulkTransferId = bulkTransferId
                })
                .ToTask(response => response.BulkTransfer);
        }

        /// <summary>
        /// Looks up an existing Token standing order submission.
        /// </summary>
        /// <param name="submissionId">submission ID</param>
        /// <returns>standing order submission record</returns>
        public Task<StandingOrderSubmission> GetStandingOrderSubmission(string submissionId)
        {
            return gateway(authenticationContext())
                   .GetStandingOrderSubmissionAsync(new GetStandingOrderSubmissionRequest
                   {
                       SubmissionId = submissionId
                   }).ToTask(response => response.Submission);
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

        /// <summary>
        /// Looks up a list of existing standing order submissions.
        /// </summary>
        /// <param name="limit">max number of records to return</param>
        /// <param name="offset">optional offset to start at</param>
        /// <returns>standing order submissions</returns>
        public Task<PagedList<StandingOrderSubmission>> GetStandingOrderSubmissions(
                int limit,
                string offset = null)
        {
            GetStandingOrderSubmissionsRequest request = new GetStandingOrderSubmissionsRequest
            {
                Page = PageBuilder(limit, offset)
            };

            return gateway(authenticationContext())
                .GetStandingOrderSubmissionsAsync(request)
                .ToTask(response => new PagedList<StandingOrderSubmission>(
                    response.Submissions,
                    response.Offset));
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
        /// <returns>the result of the cancel operation, , returned by the server</returns>
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


        /// <summary>
        /// Trigger a step up notification for balance requests.
        /// </summary>
        /// <returns>list of account ids.</returns>
        /// <param name="accountIds">Account identifiers.</param>
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


        /// <summary>
        /// Trigger a step up notification for transaction requests.
        /// </summary>
        /// <returns>notification setup.</returns>
        /// <param name="accountId">account id</param>
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

        protected override string GetOnBehalfOf()
        {
            return onBehalfOf;
        }


        /// <summary>
        /// Looks up a existing access token where the calling member is the grantor and given member is
        /// the grantee.
        /// </summary>
        /// <returns>The active access token.</returns>
        /// <param name="toMemberId">beneficiary of the active access token.</param>
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

        /// <summary>
        /// Verifies eIDAS certificate.
        /// </summary>
        /// <returns>The eidas.</returns>
        /// <param name="payload">payload payload containing member id and the certificate.</param>
        /// <param name="signature">signature payload signed with the private key corresponding to the certificate.</param>
        public Task<VerifyEidasResponse> VerifyEidas(
            VerifyEidasPayload payload,
            string signature)
        {
            var request = new VerifyEidasRequest
            {
                Payload = payload,
                Signature = signature
            };
            return gateway(authenticationContext())
                .VerifyEidasAsync(request)
                .ToTask(response => response);
        }

        /// <summary>
        /// Creates a transfer redeeming a transfer token.
        /// </summary>
        /// <param name="payload">the transfer payload</param>
        /// <returns></returns>
        public Task<Transfer> CreateTransfer(TransferPayload payload)
        {
            var signer = cryptoEngine.CreateSigner(Level.Low);
            var request = new CreateTransferRequest
            {
                Payload = payload,
                PayloadSignature = new Signature
                {
                    MemberId = MemberId,
                    KeyId = signer.GetKeyId(),
                    Signature_ = signer.Sign(payload)
                }
            };
            return gateway(authenticationContext()).CreateTransferAsync(request)
                .ToTask(response => response.Transfer);
        }

        /// <summary>
        /// Redeems a bulk transfer token.
        /// </summary>
        /// <param name="tokenId"> ID of token to redeem</param>
        /// <returns>bulk transfer record</returns>
        public Task<BulkTransfer> CreateBulkTransfer(string tokenId)
        {
            return gateway(authenticationContext())
                .CreateBulkTransferAsync(new CreateBulkTransferRequest
                {
                    TokenId = tokenId
                })
                .ToTask(response => response.Transfer);
        }

        /// <summary>
        /// Redeems a standing order token.
        /// </summary>
        /// <param name="tokenId">ID of token to redeem</param>
        /// <returns>standing order submission</returns>
        public Task<StandingOrderSubmission> CreateStandingOrder(string tokenId)
        {
            return gateway(authenticationContext())
                    .CreateStandingOrderAsync(new CreateStandingOrderRequest
                    {
                        TokenId = tokenId
                    }).ToTask(response => response.Submission);
        }

        /// <summary>
        /// Get url to bank authorization page for a token request.
        /// </summary>
        /// <param name="bankId">bank ID</param>
        /// <param name="tokenRequestId">token request ID</param>
        /// <returns>url</returns>
        public Task<string> GetBankAuthUrl(string bankId, string tokenRequestId)
        {
            return gateway(authenticationContext())
                .GetBankAuthUrlAsync(new GetBankAuthUrlRequest
                {
                    BankId = bankId,
                    TokenRequestId = tokenRequestId
                }).ToTask(response => response.Url);
        }

        /// <summary>
        /// Forward the callback from the bank (after user authentication) to Token.
        /// </summary>
        /// <param name="bankId">bank ID</param>
        /// <param name="query">HTTP query string</param>
        /// <returns>token request ID</returns>
        public Task<string> OnBankAuthCallback(string bankId, string query)
        {
            return gateway(authenticationContext())
                .OnBankAuthCallbackAsync(new OnBankAuthCallbackRequest
                {
                    BankId = bankId,
                    Query = query
                }).ToTask(response => response.TokenRequestId);
        }

        /// <summary>
        /// Get the raw consent from the bank associated with a token.
        /// </summary>
        /// <param name="tokenId">token ID</param>
        /// <returns>raw consent</returns>
        public Task<GetExternalMetadataResponse> GetExternalMetadata(string tokenRequestId)
        {
            return gateway(authenticationContext())
                .GetExternalMetadataAsync(new GetExternalMetadataRequest
                {
                    TokenRequestId = tokenRequestId,
                })
                .ToTask(res => res);
        }

        /// <summary>
        /// Set a webhook config.
        /// </summary>
        /// <param name="config">the webhook config</param>
        /// <returns>a task</returns>
        public Task SetWebhookConfig(WebhookConfig config)
        {
            var request = new SetWebhookConfigRequest { Config = config };
            return gateway(authenticationContext()).SetWebhookConfigAsync(request).ToTask();
        }

        /// <summary>
        /// Get the webhook config.
        /// </summary>
        /// <returns>the webhook config</returns>
        public Task<WebhookConfig> GetWebhookConfig()
        {
            return gateway(authenticationContext())
                .GetWebhookConfigAsync(new GetWebhookConfigRequest())
                .ToTask(res => res.Config);
        }

        /// <summary>
        /// Delete a webhook config.
        /// </summary>
        /// <returns>a task</returns>
        public Task DeleteWebhookConfig()
        {
            return gateway(authenticationContext())
                .DeleteWebhookConfigAsync(new DeleteWebhookConfigRequest())
                .ToTask();
        }
    }
}
