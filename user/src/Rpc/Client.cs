using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SubscriberProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Exceptions;
using Tokenio.Security;
using Tokenio.Rpc;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Gateway.GetTransfersRequest.Types;
using static Tokenio.Proto.Gateway.ReplaceTokenRequest.Types;
using TokenAction = Tokenio.Proto.Common.TokenProtos.TokenSignature.Types.Action;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;

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
        /// Makes RPC to get default bank account for this member.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <returns>the bank account</returns>
        public Task<ProtoAccount> GetDefaultAccount(string memberId)
        {
            var request = new GetDefaultAccountRequest { MemberId = memberId };
            return gateway(authenticationContext()).GetDefaultAccountAsync(request)
                .ToTask(response => response.Account);
        }

        /// <summary>
        /// Makes RPC to set default bank account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a task</returns>
        public Task SetDefaultAccount(string accountId)
        {
            var request = new SetDefaultAccountRequest
            {
                MemberId = MemberId,
                AccountId = accountId
            };
            return gateway(authenticationContext()).SetDefaultAccountAsync(request).ToTask();
        }

        /// <summary>
        /// Looks up if this account is default.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>true if the account is default; false otherwise</returns>
        public Task<bool> IsDefault(string accountId)
        {
            return GetDefaultAccount(MemberId)
                .Map(account => account.Id.Equals(accountId));
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

        public Task<PrepareTokenResult> PrepareToken(TokenPayload payload)
        {
            var request = new PrepareTokenRequest
            {
                Payload = payload
            };
            return gateway(authenticationContext())
                .PrepareTokenAsync(request)
                .ToTask(response => PrepareTokenResult.Create(response.ResolvedPayload, response.Policy));
        }

        public Task<Token> CreateToken(
           TokenPayload payload,
           string tokenRequestId,
           IList<Signature> signatures)
        {
            var request = new CreateTokenRequest { Payload = payload };
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
                .ToTask(response => response.Token);
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
        /// Creates an access token.
        /// </summary>
        /// <param name="payload">the access token payload</param>
        /// <returns>the access token</returns>
        public Task<Token> CreateAccessToken(TokenPayload payload)
        {
            payload.From = new TokenMember { Id = MemberId };
            var request = new CreateAccessTokenRequest { Payload = payload };
            return gateway(authenticationContext()).CreateAccessTokenAsync(request)
                .ToTask(response => response.Token);
        }

        /// <summary>
        /// Creates an access token with a token request id.
        /// </summary>
        /// <param name="payload">the access token payload</param>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the access token</returns>
        public Task<Token> CreateAccessToken(TokenPayload payload, string tokenRequestId)
        {
            payload.From = new TokenMember { Id = MemberId };
            var request = new CreateAccessTokenRequest
            {
                Payload = payload,
                TokenRequestId = tokenRequestId == null ? "" : tokenRequestId
            };
            return gateway(authenticationContext()).CreateAccessTokenAsync(request)
                .ToTask(response => response.Token);
        }

        /// <summary>
        /// Endorses a token.
        /// </summary>
        /// <param name="token">the token</param>
        /// <param name="level">the key level to be used to endorse the token</param>
        /// <returns>the result of the endorsement</returns>
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
            return gateway(authenticationContext()).EndorseTokenAsync(request)
                .ToTask(response => response.Result);
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

        /// <summary>
        /// Cancels the existing token and creates a replacement for it. Supported
        /// only for access tokens.
        /// </summary>
        /// <param name="tokenToCancel">the token to cancel</param>
        /// <param name="tokenToCreate">the payload to create new token with</param>
        /// <returns>the result of the replacement opration</returns>
        public Task<TokenOperationResult> ReplaceToken(
            Token tokenToCancel,
            TokenPayload tokenToCreate)
        {
            return CancelAndReplace(tokenToCancel, new CreateToken { Payload = tokenToCreate });
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
        /// Links a funding bank account to Token.
        /// </summary>
        /// <param name="authorization">an authorization to accounts, from the bank</param>
        /// <returns>a list of linked accounts</returns>
        public Task<IList<ProtoAccount>> LinkAccounts(BankAuthorization authorization)
        {
            var request = new LinkAccountsRequest { BankAuthorization = authorization };
            return gateway(authenticationContext()).LinkAccountsAsync(request)
                .ToTask(response => (IList<ProtoAccount>)response.Accounts);
        }

        /// <summary>
        /// Unlinks token accounts.
        /// </summary>
        /// <param name="accountIds">the account ids to unlink</param>
        /// <returns>a task</returns>
        public Task UnlinkAccounts(IList<string> accountIds)
        {
            var request = new UnlinkAccountsRequest { AccountIds = { accountIds } };
            return gateway(authenticationContext()).UnlinkAccountsAsync(request).ToTask();
        }

        /// <summary>
        /// Unsubscribes from notifications.
        /// </summary>
        /// <returns>The from notifications.</returns>
        /// <param name="subscriberId">Subscriber identifier.</param>
        public Task UnsubscribeFromNotifications(string subscriberId)
        {
            var request = new UnsubscribeFromNotificationsRequest
            {
                SubscriberId = subscriberId
            };
            return gateway(authenticationContext())
                .UnsubscribeFromNotificationsAsync(request).ToTask(); 
        }

        /// <summary>
        /// Gets the notifications.
        /// </summary>
        /// <returns>The notifications.</returns>
        /// <param name="offset">Offset.</param>
        /// <param name="limit">Limit.</param>
        public Task<PagedList<Notification>> GetNotifications(
            int limit,
            string offset = null)
        {
            var request = new GetNotificationsRequest
            {
                Page = PageBuilder(limit, offset)
            };

            return gateway(authenticationContext())
                .GetNotificationsAsync(request) 
                .ToTask(response => new PagedList<Notification>(response.Notifications, response.Offset));
        }

        /// <summary>
        /// Gets the notification.
        /// </summary>
        /// <returns>The notification.</returns>
        /// <param name="notificationId">Notification identifier.</param>
        public Task<Notification> GetNotification(string notificationId)
        {
            var request = new GetNotificationRequest
            {
                NotificationId = notificationId
            };
            return gateway(authenticationContext())
                .GetNotificationAsync(request)
                .ToTask(response => response.Notification);
        }

        /// <summary>
        /// Subscribes to notifications.
        /// </summary>
        /// <returns>The to notifications.</returns>
        /// <param name="handler">Handler.</param>
        /// <param name="handlerInstructions">Handler instructions.</param>
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
                .ToTask(response => response.Subscriber);
        }

        /// <summary>
        /// Signs a token request state payload.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <param name="tokenId">the token id</param>
        /// <param name="state">the state</param>
        /// <returns>the signature</returns>
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
            return gateway(authenticationContext()).SignTokenRequestStateAsync(request)
                .ToTask(response => response.Signature);
        }

        /// <summary>
        /// Gets the subscriber.
        /// </summary>
        /// <returns>The subscriber.</returns>
        /// <param name="subscriberId">Subscriber identifier.</param>
        public Task<Subscriber> GetSubscriber(string subscriberId)
        {
            var request = new GetSubscriberRequest
            {
                SubscriberId = subscriberId
            };
            return gateway(authenticationContext())
                .GetSubscriberAsync(request)
                .ToTask(response => response.Subscriber);
        }

        /// <summary>
        /// Gets the subscribers.
        /// </summary>
        /// <returns>The subscribers.</returns>
        public Task<IList<Subscriber>> GetSubscribers()
        {
            var request = new GetSubscribersRequest();
            return gateway(authenticationContext())
                .GetSubscribersAsync(request)
                .ToTask(response => (IList < Subscriber>)response.Subscribers);
        }

        /// <summary>
        /// Apply SCA for the given list of account IDs.
        /// </summary>
        /// <param name="accountIds">the list of account ids</param>
        /// <returns>a task</returns>
        public Task ApplySca(IList<string> accountIds)
        {
            var request = new ApplyScaRequest { AccountId = { accountIds } };
            return gateway(authenticationContext(Level.Standard)).ApplyScaAsync(request).ToTask();
        }

        /// <summary>
        /// Gets a member's receipt contact.
        /// </summary>
        /// <returns>a task</returns>
        public Task<ReceiptContact> GetReceiptContact()
        {
            var request = new GetReceiptContactRequest();
            return gateway(authenticationContext())
                .GetReceiptContactAsync(request)
                .ToTask(response => response.Contact);
        }

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
                        Signature_ =Stringify(tokenToCancel, TokenAction.Cancelled)
                    }
                },
                CreateToken = tokenToCreate
            };
            return gateway(authenticationContext()).ReplaceTokenAsync(request)
                .ToTask(response => response.Result);
        }
    }
}
