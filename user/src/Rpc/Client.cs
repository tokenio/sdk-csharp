using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tokenio.Exceptions;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
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

namespace Tokenio.User.Rpc
{
    /// <summary>
    /// An authenticated RPC client that is used to talk to Token gateway.
    /// The class is a thin wrapper on top of gRPC generated client.
    /// Makes the API easier to use.
    /// </summary>
    public sealed class Client : Tokenio.Rpc.Client
    {
        /// <summary>
        /// Instantiates a client.
        /// </summary>
        /// <param name = "memberId">The member id</param>
        /// <param name = "cryptoEngine">The crypto engine used to sign for authentication, request, payloads, etc.</param>
        /// <param name = "channel">Managed channel</param>
        public Client(string memberId, ICryptoEngine cryptoEngine, ManagedChannel channel) : base(memberId, cryptoEngine, channel)
        {
        }

        /// <summary>
        /// Replaces a member's public profile.
        /// </summary>
        /// <param name = "profile">The profile to set</param>
        /// <returns>The profile that was set</returns>
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
        /// Gets a member's public profile.
        /// </summary>
        /// <param name = "memberId">The member id of the member</param>
        /// <returns>The profile</returns>
        public Task<Profile> GetProfile(string memberId)
        {
            var request = new GetProfileRequest
            {
                MemberId = memberId
            };
            return gateway(authenticationContext()).GetProfileAsync(request)
                    .ToTask(response =>
                            response.Profile);
        }

        /// <summary>
        /// Replaces a member's public profile picture.
        /// </summary>
        /// <param name = "payload">The blob payload</param>
        /// <returns>A task</returns>
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
        /// Gets a member's public profile picture.
        /// </summary>
        /// <param name = "memberId">The member id</param>
        /// <param name = "size">The desired size(small, medium, large, original)</param>
        /// <returns>Blob with picture, or an empty blob (no fields set) if has no picture</returns>
        public Task<Blob> GetProfilePicture(string memberId, ProfilePictureSize size)
        {
            var request = new GetProfilePictureRequest
            {
                MemberId = memberId,
                Size = size
            };
            return gateway(authenticationContext())
                    .GetProfilePictureAsync(request)
                    .ToTask(response =>
                            response.Blob);
        }

        /// <summary>
        /// Makes RPC to get default bank account for this member.
        /// </summary>
        /// <param name = "memberId">The member id</param>
        /// <returns>The bank account</returns>
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
        /// <param name = "accountId">The account id</param>
        /// <returns>A task</returns>
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
        /// <param name = "accountId">The account id</param>
        /// <returns>True if the account is default, otherwise False</returns>
        public Task<bool> IsDefault(string accountId)
        {
            return GetDefaultAccount(MemberId)
                    .Map(account =>
                            account.Id.Equals(accountId));
        }

        /// <summary>
        /// Looks up an existing transfer.
        /// </summary>
        /// <param name = "transferId">The transfer id</param>
        /// <returns>The transfer record</returns>
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
        /// Looks up a list of existing transfers.
        /// </summary>
        /// <param name = "tokenId">Nullable token id</param>
        /// <param name = "offset">Nullable offset to start at</param>
        /// <param name = "limit">Max number of records to return</param>
        /// <returns>Transfer record</returns>
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
        /// Prepares the token, resolving the payload and determining the policy.
        /// </summary>
        /// <param name = "payload">Token Payload.</param>
        /// <returns>Resolved payload and policy</returns>
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
        /// Creates the token.
        /// </summary>
        /// <param name = "payload">Payload.</param>
        /// <param name = "tokenRequestId">Token request identifier.</param>
        /// <param name = "signatures">Signatures.</param>
        /// <returns>The token.</returns>
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
        /// <param name = "payload">The transfer token payload</param>
        /// <returns>The transfer token returned by the server</returns>
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
        /// Creates a new transfer token with a token request id.
        /// </summary>
        /// <param name = "payload">The transfer token payload</param>
        /// <param name = "tokenRequestId">The token request id</param>
        /// <returns>The transfer payload</returns>
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
        /// Creates an access token.
        /// </summary>
        /// <param name = "payload">The access token payload</param>
        /// <returns>The access token</returns>
        public Task<Token> CreateAccessToken(TokenPayload payload)
        {
            payload.From = new TokenMember
            {
                Id = MemberId
            };
            var request = new CreateAccessTokenRequest
            {
                Payload = payload
            };
            return gateway(authenticationContext())
                    .CreateAccessTokenAsync(request)
                    .ToTask(response =>
                            response.Token);
        }

        /// <summary>
        /// Creates an access token with a token request id.
        /// </summary>
        /// <param name = "payload">The access token payload</param>
        /// <param name = "tokenRequestId">The token request id</param>
        /// <returns>The access token</returns>
        public Task<Token> CreateAccessToken(TokenPayload payload, string tokenRequestId)
        {
            payload.From = new TokenMember
            {
                Id = MemberId
            };
            var request = new CreateAccessTokenRequest
            {
                Payload = payload,
                TokenRequestId = tokenRequestId == null ? "" : tokenRequestId
            };
            return gateway(authenticationContext())
                    .CreateAccessTokenAsync(request)
                    .ToTask(response =>
                            response.Token);
        }

        /// <summary>
        /// Endorses a token.
        /// </summary>
        /// <param name = "token">The token</param>
        /// <param name = "level">The key level to be used to endorse the token</param>
        /// <returns>The result of the endorsement</returns>
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
        /// <param name = "token">The token to cancel</param>
        /// <returns>The result of the cancel operation, returned by the server.</returns>
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
        /// <param name = "tokenToCancel">The token to cancel</param>
        /// <param name = "tokenToCreate">The payload to create new token with</param>
        /// <returns>The result of the replacement opration</returns>
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
        /// Looks up a existing access token where the calling member is the grantor and given member is the grantee.
        /// </summary>
        /// <param name = "toMemberId">Beneficiary of the active access token</param>
        /// <returns>Token returned by the server</returns>
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
        /// <param name = "tokenId">Token id</param>
        /// <returns>Token returned by the server</returns>
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
        /// Looks up a existing access token where the calling member is the grantor and given member is the grantee.
        /// </summary>
        /// <param name = "type">The token type</param>
        /// <param name = "limit">The max number of records to return</param>
        /// <param name = "offset">Nullable offset to start at</param>
        /// <returns>The tokens in paged list</returns>
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
        /// Creates a transfer redeeming a transfer token.
        /// </summary>
        /// <param name = "payload">The transfer payload</param>
        /// <returns>Transfer record</returns>
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
        /// Links a funding bank account to Token.
        /// </summary>
        /// <param name = "authorization">An authorization to accounts, from the bank</param>
        /// <returns>A list of linked accounts</returns>
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
        /// <param name = "accountIds">The account ids to unlink</param>
        /// <returns>A task</returns>
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
        /// Unsubscribes from notifications.
        /// </summary>
        /// <param name = "subscriberId">Subscriber identifier.</param>
        /// <returns>The from notifications.</returns>
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
        /// Gets the notifications.
        /// </summary>
        /// <param name = "offset">Offset.</param>
        /// <param name = "limit">Limit.</param>
        /// <returns>The notifications.</returns>
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
        /// Gets the notification.
        /// </summary>
        /// <param name = "notificationId">Notification identifier.</param>
        /// <returns>The notification.</returns>
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
        /// <param name = "handler">Specify the handler of the notifications.</param>
        /// <param name = "handlerInstructions">Map of instructions for the handler</param>
        /// <returns>Notification Subscriber.</returns>
        public Task<Subscriber> SubscribeToNotifications(
                string handler,
                MapField<string, string> handlerInstructions)
        {
            var request = new SubscribeToNotificationsRequest
            {
                Handler = handler,
            };
            request.HandlerInstructions.Add(handlerInstructions);
            return gateway(authenticationContext())
                    .SubscribeToNotificationsAsync(request)
                    .ToTask(response =>
                            response.Subscriber);
        }

        /// <summary>
        /// Signs a token request state payload.
        /// </summary>
        /// <param name = "tokenRequestId">The token request id</param>
        /// <param name = "tokenId">The token id</param>
        /// <param name = "state">The state</param>
        /// <returns>The signature</returns>
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
        /// Gets the subscriber.
        /// </summary>
        /// <param name = "subscriberId">Subscriber identifier.</param>
        /// <returns>The subscriber.</returns>
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
        /// Gets the subscribers.
        /// </summary>
        /// <returns>List of notification subscribers.</returns>
        public Task<IList<Subscriber>> GetSubscribers()
        {
            var request = new GetSubscribersRequest();
            return gateway(authenticationContext())
                    .GetSubscribersAsync(request)
                    .ToTask(response =>
                            (IList<Subscriber>)response.Subscribers);
        }

        /// <summary>
        /// Apply SCA for the given list of account IDs.
        /// </summary>
        /// <param name = "accountIds">The list of account ids</param>
        /// <returns>A task</returns>
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
        /// <returns>A task</returns>
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
        /// <param name = "contact">Contact.</param>
        /// <returns>Completable that indicates whether the operation finished or had an error.</returns>
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

        /// <summary>
        /// Cancels the and replace.
        /// </summary>
        /// <returns>The and replace.</returns>
        /// <param name="tokenToCancel">Token to cancel.</param>
        /// <param name="tokenToCreate">Token to create.</param>
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
        /// Sets the app callback URL.
        /// </summary>
        /// <returns>The app callback URL.</returns>
        /// <param name="appCallbackUrl">App callback URL.</param>
        public Task SetAppCallbackUrl(string appCallbackUrl)
        {
            var request = new SetAppCallbackUrlRequest()
            {

                AppCallbackUrl = appCallbackUrl

            };
            return gateway(authenticationContext())
                    .SetAppCallbackUrlAsync(request).ToTask();
        }

        /// <summary>
        /// Updates the status of a notification.
        /// </summary>
        /// <param name="notificationId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Task UpdateNotificationStatus(string notificationId, Status status)
        {
            var request = new UpdateNotificationStatusRequest()
            {
                NotificationId = notificationId,
                Status = status

            };

            return gateway(authenticationContext())
                   .UpdateNotificationStatusAsync(request).ToTask();
        }

    }
}