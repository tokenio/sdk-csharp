using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Tokenio.Exceptions;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.SubmissionProtos;
using Tokenio.Proto.Common.SubscriberProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Rpc;
using Tokenio.Security;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.NotificationProtos.Notification.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Gateway.GetTransfersRequest.Types;
using static Tokenio.Proto.Gateway.ReplaceTokenRequest.Types;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;
using TokenAction = Tokenio.Proto.Common.TokenProtos.TokenSignature.Types.Action;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;
using System;

namespace Tokenio.User.Rpc
{
    /// <summary>
    /// An authenticated RPC client that is used to talk to Token gateway. The
    /// class is a thin wrapper on top of gRPC generated client. Makes the API
    /// easier to use.
    /// </summary>
    public sealed class Client : Tokenio.Rpc.Client
    {
        /// <summary>
        /// Creates a client instance.
        /// </summary>
        /// <param name = "memberId">member id</param>
        /// <param name = "cryptoEngine">the crypto engine used to sign for authentication, request, payloads, etc</param>
        /// <param name = "channel">managed channel</param>
        public Client(string memberId, ICryptoEngine cryptoEngine, ManagedChannel channel) : base(memberId, cryptoEngine, channel)
        {
        }

        /// <summary>
        /// Replaces a member's public profile.
        /// </summary>
        /// <param name = "profile">Profile to set</param>
        /// <returns>task that completes when request handled</returns>
        public Task<Profile> SetProfile(Profile profile)
        {
            var request = new SetProfileRequest
            {
                Profile = profile
            };
            return gateway(authenticationContext()).SetProfileAsync(request)
                    .ToTask(response =>
                            response.Profile);
        }

        /// <summary>
        /// Replaces a member's public profile picture.
        /// </summary>
        /// <param name = "payload">Picture data</param>
        /// <returns>task that completes when request handled</returns>
        public Task SetProfilePicture(Payload payload)
        {
            var request = new SetProfilePictureRequest
            {
                Payload = payload
            };
            return gateway(authenticationContext())
                    .SetProfilePictureAsync(request)
                    .ToTask();
        }

        /// <summary>
        /// Makes RPC to get default bank account for this member.
        /// </summary>
        /// <param name = "memberId">member id</param>
        /// <returns>the bank account</returns>
        public Task<ProtoAccount> GetDefaultAccount(string memberId)
        {
            var request = new GetDefaultAccountRequest
            {
                MemberId = memberId
            };
            return gateway(authenticationContext())
                    .GetDefaultAccountAsync(request)
                    .ToTask(response =>
                            response.Account);
        }

        /// <summary>
        /// Makes RPC to set default bank account.
        /// </summary>
        /// <param name = "accountId">the account id</param>
        /// <returns>task indicating if the default bank account was successfully set</returns>
        public Task SetDefaultAccount(string accountId)
        {
            var request = new SetDefaultAccountRequest
            {
                MemberId = MemberId,
                AccountId = accountId
            };
            return gateway(authenticationContext())
                    .SetDefaultAccountAsync(request)
                    .ToTask();
        }

        /// <summary>
        /// Looks up if this account is default.
        /// </summary>
        /// <param name = "accountId">the bank account id</param>
        /// <returns>true if the account is default; false otherwise</returns>
        public Task<bool> IsDefault(string accountId)
        {
            return GetDefaultAccount(MemberId)
                    .Map(account =>
                            account.Id.Equals(accountId));
        }

        /// <summary>
        /// Looks up an existing transfer.
        /// </summary>
        /// <param name = "transferId">transfer id</param>
        /// <returns>transfer record</returns>
        public Task<Transfer> GetTransfer(string transferId)
        {
            var request = new GetTransferRequest
            {
                TransferId = transferId
            };
            return gateway(authenticationContext())
                    .GetTransferAsync(request)
                    .ToTask(response =>
                            response.Transfer);
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
                })
                .ToTask(response => response.Submission);
        }

        /// <summary>
        /// Looks up a list of existing transfers.
        /// </summary>
        /// <param name = "tokenId">optional token id to restrict the search</param>
        /// <param name = "offset">optional offset to start at</param>
        /// <param name = "limit">max number of records to return</param>
        /// <returns>transfer records</returns>
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
                request.Filter = new TransferFilter
                {
                    TokenId = tokenId
                };
            }
            if (offset != null)
            {
                request.Page.Offset = offset;
            }
            return gateway(authenticationContext())
                    .GetTransfersAsync(request)
                    .ToTask(response =>
                            new PagedList<Transfer>(response.Transfers, response.Offset));
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
        /// Prepares the token, resolving the payload and determining the policy.
        /// </summary>
        /// <param name = "payload">token payload</param>
        /// <returns>resolved payload and policy</returns>
        public Task<PrepareTokenResult> PrepareToken(TokenPayload payload)
        {
            var request = new PrepareTokenRequest
            {
                Payload = payload
            };
            return gateway(authenticationContext())
                    .PrepareTokenAsync(request)
                    .ToTask(response =>
                            PrepareTokenResult.Create(response.ResolvedPayload, response.Policy));
        }

        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name = "payload">token payload</param>
        /// <param name = "tokenRequestId">token request ID</param>
        /// <param name = "signatures">list of token payload signatures</param>
        /// <returns>token returned by server</returns>
        public Task<Token> CreateToken(
                TokenPayload payload,
                string tokenRequestId,
                IList<Signature> signatures)
        {
            var request = new CreateTokenRequest
            {
                Payload = payload
            };
            if (tokenRequestId != null)
            {
                request.TokenRequestId = tokenRequestId;
            }
            if (signatures.Count > 0)
            {
                request.Signatures.Add(signatures);
            }
            return gateway(authenticationContext())
                    .CreateTokenAsync(request)
                    .ToTask(response =>
                            response.Token);
        }

        /// <summary>
        /// Creates a new transfer token.
        /// </summary>
        /// <param name = "payload">transfer token payload</param>
        /// <returns>transfer token returned by the server</returns>
        public Task<Token> CreateTransferToken(TokenPayload payload)
        {
            var request = new CreateTransferTokenRequest
            {
                Payload = payload
            };
            return gateway(authenticationContext())
                    .CreateTransferTokenAsync(request)
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
        /// Creates a new transfer token.
        /// </summary>
        /// <param name = "payload">transfer token payload</param>
        /// <param name = "tokenRequestId">token request id</param>
        /// <returns>transfer token returned by the server</returns>
        public Task<Token> CreateTransferToken(TokenPayload payload, string tokenRequestId)
        {
            var request = new CreateTransferTokenRequest
            {
                Payload = payload,
                TokenRequestId = tokenRequestId
            };
            return gateway(authenticationContext())
                    .CreateTransferTokenAsync(request)
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
        /// Creates a new access token.
        /// </summary>
        /// <param name = "payload">token payload</param>
        /// <param name="tokenRequestId">token request id</param>
        /// <returns>token returned by server</returns>
        public Task<Token> CreateAccessToken(TokenPayload payload, string tokenRequestId = null)
        {
            payload.From = new TokenMember
            {
                Id = MemberId
            };
            var request = new CreateAccessTokenRequest
            {
                Payload = payload,
                TokenRequestId = tokenRequestId ?? ""
            };
            return gateway(authenticationContext())
                    .CreateAccessTokenAsync(request)
                    .ToTask(response =>
                            response.Token);
        }

        /// <summary>
        /// Endorses a token.
        /// </summary>
        /// <param name = "token">token to endorse</param>
        /// <param name = "level">key level to be used to endorse the token</param>
        /// <returns>result of the endorse operation, returned by the server</returns>
        public Task<TokenOperationResult> EndorseToken(Token token, Level level)
        {
            var signer = cryptoEngine.CreateSigner(level);
            var request = new EndorseTokenRequest
            {
                TokenId = token.Id,
                Signature = new Signature
                {
                    MemberId = MemberId,
                    KeyId = signer.GetKeyId(),
                    Signature_ = signer.Sign(Stringify(token, TokenAction.Endorsed))
                }
            };
            return gateway(authenticationContext())
                    .EndorseTokenAsync(request)
                    .ToTask(response =>
                            response.Result);
        }

        /// <summary>
        /// Cancels a token.
        /// </summary>
        /// <param name = "token">token to cancel</param>
        /// <returns>result of the cancel operation, returned by the server</returns>
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
            return gateway(authenticationContext())
                    .CancelTokenAsync(request)
                    .ToTask(response =>
                        response.Result);
        }

        /// <summary>
        /// Cancels the existing token and creates a replacement for it.
        /// Supported only for access tokens.
        /// </summary>
        /// <param name = "tokenToCancel">old token to cancel</param>
        /// <param name = "tokenToCreate">new token to create</param>
        /// <returns>result of the replacement operation, returned by the server</returns>
        public Task<TokenOperationResult> ReplaceToken(
                Token tokenToCancel,
                TokenPayload tokenToCreate)
        {
            return CancelAndReplace(tokenToCancel, new CreateToken
            {
                Payload = tokenToCreate
            });
        }

        /// <summary>
        /// Looks up a existing access token where the calling member is the grantor and given member is
        /// the grantee.
        /// </summary>
        /// <param name = "toMemberId">beneficiary of the active access token</param>
        /// <returns>token returned by the server</returns>
        public Task<Token> GetActiveAccessToken(string toMemberId)
        {
            var request = new GetActiveAccessTokenRequest
            {
                ToMemberId = toMemberId
            };
            return gateway(authenticationContext())
                    .GetActiveAccessTokenAsync(request)
                    .ToTask(response =>
                            response.Token);
        }

        /// <summary>
        /// Looks up a existing token.
        /// </summary>
        /// <param name = "tokenId">token id</param>
        /// <returns>token returned by the server</returns>
        public Task<Token> GetToken(string tokenId)
        {
            var request = new GetTokenRequest
            {
                TokenId = tokenId
            };
            return gateway(authenticationContext())
                    .GetTokenAsync(request)
                    .ToTask(response =>
                            response.Token);
        }

        /// <summary>
        /// Looks up a list of existing token.
        /// </summary>
        /// <param name = "type">token type</param>
        /// <param name = "limit">max number of records to return</param>
        /// <param name = "offset">optional offset to start at</param>
        /// <returns>token returned by the server</returns>
        public Task<PagedList<Token>> GetTokens(
                TokenType type,
                int limit,
                string offset)
        {
            var request = new GetTokensRequest
            {
                Type = type,
                Page = PageBuilder(limit, offset)
            };
            return gateway(authenticationContext())
                    .GetTokensAsync(request)
                    .ToTask(response =>
                            new PagedList<Token>(response.Tokens, response.Offset));
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name = "payload">transfer parameters, such as amount, currency, etc</param>
        /// <returns>transfer record</returns>
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
            return gateway(authenticationContext())
                    .CreateTransferAsync(request)
                    .ToTask(response =>
                            response.Transfer);
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
                    })
                    .ToTask(response => response.Submission);
        }


        /// <summary>
        /// Links a funding bank account to Token.
        /// </summary>
        /// <param name = "authorization">an authorization to accounts, from the bank</param>
        /// <returns>list of linked accounts</returns>
        public Task<IList<ProtoAccount>> LinkAccounts(BankAuthorization authorization)
        {
            var request = new LinkAccountsRequest
            {
                BankAuthorization = authorization
            };
            return gateway(authenticationContext())
                    .LinkAccountsAsync(request)
                    .ToTask(response =>
                            (IList<ProtoAccount>)response.Accounts);
        }

        /// <summary>
        /// Unlinks token accounts.
        /// </summary>
        /// <param name = "accountIds">account ids to unlink</param>
        /// <returns>task</returns>
        public Task UnlinkAccounts(IList<string> accountIds)
        {
            var request = new UnlinkAccountsRequest
            {
                AccountIds = {
                    accountIds
                }
            };
            return gateway(authenticationContext())
                    .UnlinkAccountsAsync(request)
                    .ToTask();
        }

        /// <summary>
        /// Removes a subscriber, to stop receiving notifications.
        /// </summary>
        /// <param name = "subscriberId">id of the subscriber</param>
        /// <returns>task</returns>
        public Task UnsubscribeFromNotifications(string subscriberId)
        {
            var request = new UnsubscribeFromNotificationsRequest
            {
                SubscriberId = subscriberId
            };
            return gateway(authenticationContext())
                    .UnsubscribeFromNotificationsAsync(request)
                    .ToTask();
        }

        /// <summary>
        /// Gets a list of the member's notifications.
        /// </summary>
        /// <param name = "offset">offset to start</param>
        /// <param name = "limit">how many notifications to get</param>
        /// <returns>list of notifications</returns>
        public Task<PagedList<Notification>> GetNotifications(
                int limit,
                string offset = null)
        {
            var page = PageBuilder(limit, offset);
            var request = new GetNotificationsRequest
            {
                Page = page
            };
            return gateway(authenticationContext())
                    .GetNotificationsAsync(request)
                    .ToTask(response =>
                            new PagedList<Notification>(response.Notifications, response.Offset));
        }

        /// <summary>
        /// Gets a notification.
        /// </summary>
        /// <param name = "notificationId">id of the notification</param>
        /// <returns>notification</returns>
        public Task<Notification> GetNotification(string notificationId)
        {
            var request = new GetNotificationRequest
            {
                NotificationId = notificationId
            };
            return gateway(authenticationContext())
                    .GetNotificationAsync(request)
                    .ToTask(response =>
                            response.Notification);
        }

        /// <summary>
        /// Creates a subscriber to receive push notifications.
        /// </summary>
        /// <param name = "handler">specify the handler of the notifications</param>
        /// <param name = "handlerInstructions">map of instructions for the handler</param>
        /// <returns>n  otification subscriber</returns>
        public Task<Subscriber> SubscribeToNotifications(
                string handler,
                MapField<string, string> handlerInstructions)
        {
            var request = new SubscribeToNotificationsRequest
            {
                Handler = handler
            };
            request.HandlerInstructions.Add(handlerInstructions);
            return gateway(authenticationContext())
                    .SubscribeToNotificationsAsync(request)
                    .ToTask(response =>
                            response.Subscriber);
        }

        /// <summary>
        /// Sign with a Token signature a token request state payload.
        /// </summary>
        /// <param name = "tokenRequestId">token request id</param>
        /// <param name = "tokenId">token id</param>
        /// <param name = "state">state</param>
        /// <returns>signature</returns>
        public Task<Signature> SignTokenRequestState(
                string tokenRequestId,
                string tokenId,
                string state)
        {
            var request = new SignTokenRequestStateRequest
            {
                Payload = new TokenRequestStatePayload
                {
                    TokenId = tokenId,
                    State = state
                },
                TokenRequestId = tokenRequestId
            };
            return gateway(authenticationContext())
                    .SignTokenRequestStateAsync(request)
                    .ToTask(response =>
                            response.Signature);
        }

        /// <summary>
        /// Gets a subscriber by Id.
        /// </summary>
        /// <param name = "subscriberId">subscriber id</param>
        /// <returns>notification subscriber</returns>
        public Task<Subscriber> GetSubscriber(string subscriberId)
        {
            var request = new GetSubscriberRequest
            {
                SubscriberId = subscriberId
            };
            return gateway(authenticationContext())
                    .GetSubscriberAsync(request)
                    .ToTask(response =>
                            response.Subscriber);
        }

        /// <summary>
        /// Gets all subscribers for the member.
        /// </summary>
        /// <returns>list of notification subscribers</returns>
        public Task<IList<Subscriber>> GetSubscribers()
        {
            var request = new GetSubscribersRequest();
            return gateway(authenticationContext())
                    .GetSubscribersAsync(request)
                    .ToTask(response =>
                            (IList<Subscriber>)response.Subscribers);
        }

        /// <summary>
        /// Stores a linking request.
        /// </summary>
        /// <param name="callbackUrl">callback URL</param>
        /// <param name="tokenRequestId">token request ID</param>
        /// <returns>linking request ID</returns>
        public Task<string> StoreLinkingRequest(
            string callbackUrl,
            string tokenRequestId)
        {
            var request = new StoreLinkingRequestRequest
            {
                CallbackUrl = callbackUrl,
                TokenRequestId = tokenRequestId
            };
            return gateway(authenticationContext())
                .StoreLinkingRequestAsync(request)
                .ToTask(response => response.LinkingRequestId);
        }
        
        /// <summary>
        /// Apply SCA for the given list of account IDs.
        /// </summary>
        /// <param name = "accountIds">list of account ids</param>
        /// <returns>task</returns>
        public Task ApplySca(IList<string> accountIds)
        {
            var request = new ApplyScaRequest
            {
                AccountId = {
                    accountIds
                }
            };
            return gateway(authenticationContext(Level.Standard))
                    .ApplyScaAsync(request)
                    .ToTask();
        }

        /// <summary>
        /// Gets a member's receipt contact.
        /// </summary>
        /// <returns>receipt contact</returns>
        public Task<ReceiptContact> GetReceiptContact()
        {
            var request = new GetReceiptContactRequest();
            return gateway(authenticationContext())
                    .GetReceiptContactAsync(request)
                    .ToTask(response =>
                            response.Contact);
        }

        /// <summary>
        /// Replaces member's recipt contact.
        /// </summary>
        /// <param name = "contact">receipt contact to set</param>
        /// <returns>task that indicates whether the operation finished or had an error</returns>
        public Task SetReceiptContact(ReceiptContact contact)
        {
            var request = new SetReceiptContactRequest
            {
                Contact = contact
            };
            return gateway(authenticationContext())
                    .SetReceiptContactAsync(request)
                    .ToTask();
        }

        private Task<TokenOperationResult> CancelAndReplace(
                Token tokenToCancel,
                CreateToken tokenToCreate)
        {
            var signer = cryptoEngine.CreateSigner(Level.Low);
            var request = new ReplaceTokenRequest
            {
                CancelToken = new CancelToken
                {
                    TokenId = tokenToCancel.Id,
                    Signature = new Signature
                    {
                        MemberId = MemberId,
                        KeyId = signer.GetKeyId(),
                        Signature_ = signer.Sign(Stringify(tokenToCancel, TokenAction.Cancelled))
                    }
                },
                CreateToken = tokenToCreate
            };
            return gateway(authenticationContext())
                    .ReplaceTokenAsync(request)
                    .ToTask(response =>
                            response.Result);
        }

        /// <summary>
        /// Sets the app's callback url.
        /// </summary>
        /// <param name="appCallbackUrl">the app callback url to set</param>
        /// <returns>task</returns>
        public Task SetAppCallbackUrl(string appCallbackUrl)
        {
            var request = new SetAppCallbackUrlRequest
            {

                AppCallbackUrl = appCallbackUrl

            };
            return gateway(authenticationContext())
                    .SetAppCallbackUrlAsync(request).ToTask();
        }

        /// <summary>
        /// Updates the status of a notification.
        /// </summary>
        /// <param name="notificationId">the notification id to update</param>
        /// <param name="status">the status to update</param>
        /// <returns>task</returns>
        public Task UpdateNotificationStatus(string notificationId, Status status)
        {
            var request = new UpdateNotificationStatusRequest
            {
                NotificationId = notificationId,
                Status = status

            };

            return gateway(authenticationContext())
                   .UpdateNotificationStatusAsync(request).ToTask();
        }

    }
}
