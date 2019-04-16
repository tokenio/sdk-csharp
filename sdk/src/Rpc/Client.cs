using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.AddressProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.BankProtos;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Exceptions;
using Tokenio.Security;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Gateway.GatewayService;
using static Tokenio.Proto.Gateway.GetTransfersRequest.Types;
using static Tokenio.Proto.Gateway.ReplaceTokenRequest.Types;
using TokenAction = Tokenio.Proto.Common.TokenProtos.TokenSignature.Types.Action;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;
using Grpc.Core.Interceptors;
using Tokenio.Proto.Common.NotificationProtos;

namespace Tokenio.Rpc
{
    /// <summary>
    /// An authenticated RPC client that is used to talk to Token gateway. The
    /// class is a thin wrapper on top of gRPC generated client. Makes the API
    /// easier to use.
    /// </summary>
    public class Client
    {
        private readonly ICryptoEngine cryptoEngine;
        private readonly ManagedChannel channel;
        private string onBehalfOf = null;
        private bool customerInitiated = false;
        private SecurityMetadata securityMetadata = new SecurityMetadata();

        /// <summary>
        /// Instantiates a client.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="cryptoEngine">the crypto engine used to sign for authentication, request
        /// payloads, etc</param>
        /// <param name="channel">managed channel</param>
        public Client(string memberId, ICryptoEngine cryptoEngine, ManagedChannel channel)
        {
            this.MemberId = memberId;
            this.cryptoEngine = cryptoEngine;
            this.channel = channel;
        }

        public string MemberId { get; }

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
        /// Sets the security metadata to be sent with each request.
        /// </summary>
        /// <param name="securityMetadata">security metadata</param>
        public void SetTrackingMetadata(SecurityMetadata securityMetadata)
        {
            this.securityMetadata = securityMetadata;
        }

        /// <summary>
        /// Looks up member information for the current user. The user is defined by
        /// the key used for authentication.
        /// </summary>
        /// <returns>the member</returns>
        public Task<ProtoMember> GetMember()
        {
            return GetMember(MemberId);
        }

        /// <summary>
        /// Looks up member information for a given user.
        /// </summary>
        /// <param name="memberId">the member id of the user</param>
        /// <returns>the member</returns>
        public Task<ProtoMember> GetMember(string memberId)
        {
            var request = new GetMemberRequest {MemberId = memberId};
            return gateway(authenticationContext()).GetMemberAsync(request)
                .ToTask(response => response.Member);
        }

        /// <summary>
        /// Updates member by applying the specified operations.
        /// </summary>
        /// <param name="operations">the operations to apply</param>
        /// <param name="metadata">the metadata associated with the operations</param>
        /// <returns>the updated member</returns>
        public Task<ProtoMember> UpdateMember(
            IList<MemberOperation> operations,
            IList<MemberOperationMetadata> metadata)
        {
            return GetMember().FlatMap(member => UpdateMember(member, operations, metadata));
        }

        /// <summary>
        /// Updates member by applying the specified operations that don't contain any add
        /// alias operation.
        /// </summary>
        /// <param name="operations">the operations to apply</param>
        /// <returns>the updated member</returns>
        public Task<ProtoMember> UpdateMember(IList<MemberOperation> operations)
        {
            return GetMember().FlatMap(member => UpdateMember(
                member,
                operations,
                new List<MemberOperationMetadata>()));
        }

        /// <summary>
        /// Updates member by applying the specified operations.
        /// </summary>
        /// <param name="member">the member to update, which should be the member that the
        /// client is bond to</param>
        /// <param name="operations">the operations to apply</param>
        /// <param name="metadata">the updated member</param>
        /// <returns></returns>
        public Task<ProtoMember> UpdateMember(
            ProtoMember member,
            IList<MemberOperation> operations,
            IList<MemberOperationMetadata> metadata)
        {
            var signer = cryptoEngine.CreateSigner(Level.Privileged);
            var request = Util.ToUpdateMemberRequest(member, operations, signer, metadata);

            return gateway(authenticationContext()).UpdateMemberAsync(request)
                .ToTask(response => response.Member);
        }

        /// <summary>
        /// Set Token as the recovery agent.
        /// </summary>
        /// <returns>a task</returns>
        public Task UseDefaultRecoveryRule()
        {
            return gateway(authenticationContext()).GetDefaultAgentAsync(new GetDefaultAgentRequest())
                .ToTask(response => new MemberOperation
                {
                    RecoveryRules = new MemberRecoveryRulesOperation
                    {
                        RecoveryRule = new RecoveryRule {PrimaryAgent = response.MemberId}
                    }
                })
                .FlatMap(opration => UpdateMember(new List<MemberOperation> {opration}));
        }

        /// <summary>
        /// Links a funding bank account to Token.
        /// </summary>
        /// <param name="authorization">an authorization to accounts, from the bank</param>
        /// <returns>a list of linked accounts</returns>
        public Task<IList<ProtoAccount>> LinkAccounts(BankAuthorization authorization)
        {
            var request = new LinkAccountsRequest {BankAuthorization = authorization};
            return gateway(authenticationContext()).LinkAccountsAsync(request)
                .ToTask(response => (IList<ProtoAccount>) response.Accounts);
        }

        /// <summary>
        /// Links a funding bank account to Token.
        /// </summary>
        /// <param name="authorization">an OAuth authorization for linking</param>
        /// <returns>a list of linked accounts</returns>
        /// <exception cref="BankAuthorizationRequiredException"></exception>
        public Task<IList<ProtoAccount>> LinkAccounts(OauthBankAuthorization authorization)
        {
            var request = new LinkAccountsOauthRequest {Authorization = authorization};
            return gateway(authenticationContext()).LinkAccountsOauthAsync(request)
                .ToTask(response =>
                {
                    if (response.Status == AccountLinkingStatus.FailureBankAuthorizationRequired)
                    {
                        throw new BankAuthorizationRequiredException();
                    }

                    return (IList<ProtoAccount>) response.Accounts;
                });
        }

        /// <summary>
        /// Unlinks token accounts.
        /// </summary>
        /// <param name="accountIds">the account ids to unlink</param>
        /// <returns>a task</returns>
        public Task UnlinkAccounts(IList<string> accountIds)
        {
            var request = new UnlinkAccountsRequest {AccountIds = {accountIds}};
            return gateway(authenticationContext()).UnlinkAccountsAsync(request).ToTask();
        }

        /// <summary>
        /// Looks up a linked funding account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account info</returns>
        public Task<ProtoAccount> GetAccount(string accountId)
        {
            var request = new GetAccountRequest {AccountId = accountId};
            return gateway(authenticateOnBehalfOf()).GetAccountAsync(request)
                .ToTask(response => response.Account);
        }

        /// <summary>
        /// Looks up all the linked funding accounts.
        /// </summary>
        /// <returns>a list of linked accounts</returns>
        public Task<IList<ProtoAccount>> GetAccounts()
        {
            return gateway(authenticateOnBehalfOf())
                .GetAccountsAsync(new GetAccountsRequest())
                .ToTask(response => (IList<ProtoAccount>) response.Accounts);
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
                Options = {options}
            };
            return gateway(authenticationContext()).StoreTokenRequestAsync(request)
                .ToTask(response => response.TokenRequest.Id);
        }

        /// <summary>
        /// Update an existing token request.
        /// </summary>
        /// <param name="requestId">token request ID</param>
        /// <param name="options">new token request options</param>
        /// <returns>a task</returns>
        public Task UpdateTokenRequest(
            string requestId,
            Proto.Common.TokenProtos.TokenRequestOptions options)
        {
            var request = new UpdateTokenRequestRequest
            {
                RequestId = requestId,
                RequestOptions = options
            };
            return gateway(authenticationContext()).UpdateTokenRequestAsync(request).ToTask();
        }

        /// <summary>
        /// Creates a new transfer token.
        /// </summary>
        /// <param name="payload">the transfer token payload</param>
        /// <returns>the transfer token</returns>
        /// <exception cref="TransferTokenException"></exception>
        public Task<Token> CreateTransferToken(TokenPayload payload)
        {
            var request = new CreateTransferTokenRequest {Payload = payload};
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
            payload.From = new TokenMember {Id = MemberId};
            var request = new CreateAccessTokenRequest {Payload = payload};
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
            payload.From = new TokenMember {Id = MemberId};
            var request = new CreateAccessTokenRequest
            {
                Payload = payload,
                TokenRequestId = tokenRequestId
            };
            return gateway(authenticationContext()).CreateAccessTokenAsync(request)
                .ToTask(response => response.Token);
        }

        /// <summary>
        /// Looks up an existing token.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>the token</returns>
        public Task<Token> GetToken(string tokenId)
        {
            var request = new GetTokenRequest {TokenId = tokenId};
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
            return CancelAndReplace(tokenToCancel, new CreateToken {Payload = tokenToCreate});
        }

        /// <summary>
        /// Makes RPC to get default bank account for this member.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <returns>the bank account</returns>
        public Task<ProtoAccount> GetDefaultAccount(string memberId)
        {
            var request = new GetDefaultAccountRequest {MemberId = memberId};
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
        /// Look up account balance.
        /// </summary>
        /// <param name="acountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the account balance</returns>
        /// <exception cref="StepUpRequiredException"></exception>
        public Task<Balance> GetBalance(string acountId, Level keyLevel)
        {
            var request = new GetBalanceRequest {AccountId = acountId};
            return gateway(authenticateOnBehalfOf(keyLevel)).GetBalanceAsync(request)
                .ToTask(response =>
                {
                    if (response.Status.Equals(RequestStatus.SuccessfulRequest))
                    {
                        return response.Balance;
                    }

                    throw new StepUpRequiredException("Balance step up required.");
                });
        }

        /// <summary>
        /// Look up balances for a list of accounts.
        /// </summary>
        /// <param name="accountIds">a list of account ids</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>a list of balances</returns>
        public Task<IList<Balance>> GetBalances(IList<string> accountIds, Level keyLevel)
        {
            var request = new GetBalancesRequest
            {
                AccountId = {accountIds}
            };
            return gateway(authenticationContext(keyLevel)).GetBalancesAsync(request)
                .ToTask(response => (IList<Balance>) response.Response
                    .Where(res => res.Status.Equals(RequestStatus.SuccessfulRequest))
                    .Select(res => res.Balance)
                    .ToList());
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
        /// Looks up an existing transfer.
        /// </summary>
        /// <param name="transferId">the transfer id</param>
        /// <returns>the transfer record</returns>
        public Task<Transfer> GetTransfer(string transferId)
        {
            var request = new GetTransferRequest {TransferId = transferId};
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
                request.Filter = new TransferFilter {TokenId = tokenId};
            }

            if (offset != null)
            {
                request.Page.Offset = offset;
            }

            return gateway(authenticationContext()).GetTransfersAsync(request)
                .ToTask(response => new PagedList<Transfer>(response.Transfers, response.Offset));
        }

        /// <summary>
        /// Look up an existing transaction and return the response.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="transactionId">the transaction id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the transaction</returns>
        /// <exception cref="StepUpRequiredException">if further authentication is required</exception>
        public Task<Transaction> GetTransaction(
            string accountId,
            string transactionId,
            Level keyLevel)
        {
            var request = new GetTransactionRequest
            {
                AccountId = accountId,
                TransactionId = transactionId
            };
            return gateway(authenticateOnBehalfOf(keyLevel)).GetTransactionAsync(request)
                .ToTask(response =>
                {
                    if (response.Status.Equals(RequestStatus.SuccessfulRequest))
                    {
                        return response.Transaction;
                    }

                    throw new StepUpRequiredException("Transaction step up required.");
                });
        }

        /// <summary>
        /// Lookup transactions and return response.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="limt">the limit</param>
        /// <param name="keyLevel">the level</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <returns>a paged list of transactions</returns>
        /// <exception cref="StepUpRequiredException">if further authentication is required</exception>
        public Task<PagedList<Transaction>> GetTransactions(
            string accountId,
            int limt,
            Level keyLevel,
            string offset)
        {
            var request = new GetTransactionsRequest
            {
                AccountId = accountId,
                Page = new Page
                {
                    Limit = limt
                }
            };
            if (offset != null)
            {
                request.Page.Offset = offset;
            }

            return gateway(authenticateOnBehalfOf(keyLevel)).GetTransactionsAsync(request)
                .ToTask(response =>
                {
                    if (response.Status.Equals(RequestStatus.SuccessfulRequest))
                    {
                        return new PagedList<Transaction>(response.Transactions, response.Offset);
                    }

                    throw new StepUpRequiredException("Transactions step up required.");
                });
        }

        /// <summary>
        /// Creates and uploads a blob.
        /// </summary>
        /// <param name="payload">the blob payload</param>
        /// <returns>id of the blob</returns>
        public Task<string> CreateBlob(Payload payload)
        {
            var request = new CreateBlobRequest {Payload = payload};
            return gateway(authenticationContext()).CreateBlobAsync(request)
                .ToTask(response => response.BlobId);
        }

        /// <summary>
        /// Retrieves a blob from the server.
        /// </summary>
        /// <param name="blobId">the blob id</param>
        /// <returns>the blob</returns>
        public Task<Blob> GetBlob(string blobId)
        {
            var request = new GetBlobRequest {BlobId = blobId};
            return gateway(authenticationContext()).GetBlobAsync(request)
                .ToTask(response => response.Blob);
        }

        /// <summary>
        /// Retrieves a blob that is attached to a token.
        /// </summary>
        /// <param name="tokenId">the id of the token</param>
        /// <param name="blobId">the id of the blob</param>
        /// <returns></returns>
        public Task<Blob> GetTokenBlob(string tokenId, string blobId)
        {
            var request = new GetTokenBlobRequest
            {
                TokenId = tokenId,
                BlobId = blobId
            };
            return gateway(authenticationContext()).GetTokenBlobAsync(request)
                .ToTask(response => response.Blob);
        }

        /// <summary>
        /// Adds a new member address.
        /// </summary>
        /// <param name="name">the name of the address</param>
        /// <param name="address">the address json</param>
        /// <returns>the created address record</returns>
        public Task<AddressRecord> AddAddress(string name, Address address)
        {
            var signer = cryptoEngine.CreateSigner(Level.Low);
            var request = new AddAddressRequest
            {
                Name = name,
                Address = address,
                AddressSignature = new Signature
                {
                    MemberId = MemberId,
                    KeyId = signer.GetKeyId(),
                    Signature_ = signer.Sign(address)
                }
            };
            return gateway(authenticationContext()).AddAddressAsync(request)
                .ToTask(response => response.Address);
        }

        /// <summary>
        /// Looks up an address by id.
        /// </summary>
        /// <param name="addressId">the address id</param>
        /// <returns>the address record</returns>
        public Task<AddressRecord> GetAddress(string addressId)
        {
            var request = new GetAddressRequest {AddressId = addressId};
            return gateway(authenticateOnBehalfOf()).GetAddressAsync(request)
                .ToTask(response => response.Address);
        }

        /// <summary>
        /// Looks up member addresses.
        /// </summary>
        /// <returns>a list of addresses</returns>
        public Task<IList<AddressRecord>> GetAddresses()
        {
            return gateway(authenticateOnBehalfOf()).GetAddressesAsync(new GetAddressesRequest())
                .ToTask(response => (IList<AddressRecord>) response.Addresses);
        }

        /// <summary>
        /// Deletes a member address by its id.
        /// </summary>
        /// <param name="addressId">the id of the address</param>
        /// <returns>a task</returns>
        public Task DeleteAddress(string addressId)
        {
            var request = new DeleteAddressRequest {AddressId = addressId};
            return gateway(authenticationContext()).DeleteAddressAsync(request).ToTask();
        }

        /// <summary>
        /// Replaces a member's public profile.
        /// </summary>
        /// <param name="profile">the profile to set</param>
        /// <returns>the profile that was set</returns>
        public Task<Profile> SetProfile(Profile profile)
        {
            var request = new SetProfileRequest {Profile = profile};
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
            var request = new GetProfileRequest {MemberId = memberId};
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
            var request = new SetProfilePictureRequest {Payload = payload};
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
        /// Returns linking information for the specified bank id.
        /// </summary>
        /// <param name="bankId">the bank id</param>
        /// <returns>the bank linking information</returns>
        public Task<BankInfo> GetBankInfo(string bankId)
        {
            var request = new GetBankInfoRequest {BankId = bankId};
            return gateway(authenticationContext()).GetBankInfoAsync(request)
                .ToTask(response => response.Info);
        }

        /// <summary>
        /// Returns a list of aliases of the member.
        /// </summary>
        /// <returns>a list of aliases</returns>
        public Task<IList<Alias>> GetAliases()
        {
            return gateway(authenticationContext()).GetAliasesAsync(new GetAliasesRequest())
                .ToTask(response => (IList<Alias>) response.Aliases);
        }

        /// <summary>
        /// Retry alias verification.
        /// </summary>
        /// <param name="alias">the alias to be verified</param>
        /// <returns>the verification id</returns>
        public Task<string> RetryVerification(Alias alias)
        {
            var request = new RetryVerificationRequest
            {
                Alias = alias,
                MemberId = MemberId
            };
            return gateway(authenticationContext()).RetryVerificationAsync(request)
                .ToTask(response => response.VerificationId);
        }

        /// <summary>
        /// Authorizes recovery as a trusted agent.
        /// </summary>
        /// <param name="authorization">the authorization</param>
        /// <returns>the signature</returns>
        public Task<Signature> AuthorizeRecovery(Authorization authorization)
        {
            var signer = cryptoEngine.CreateSigner(Level.Privileged);
            return Task.FromResult(new Signature
            {
                MemberId = MemberId,
                KeyId = signer.GetKeyId(),
                Signature_ = signer.Sign(authorization)
            });
        }

        /// <summary>
        /// Gets the member id of the default recovery agent.
        /// </summary>
        /// <returns>the member id</returns>
        public Task<string> GetDefaultAgent()
        {
            return gateway(authenticationContext())
                .GetDefaultAgentAsync(new GetDefaultAgentRequest())
                .ToTask(response => response.MemberId);
        }

        /// <summary>
        /// Verifies a given alias.
        /// </summary>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the verification code</param>
        /// <returns>a task</returns>
        public Task VerifyAlias(string verificationId, string code)
        {
            var request = new VerifyAliasRequest
            {
                VerificationId = verificationId,
                Code = code
            };
            return gateway(authenticationContext()).VerifyAliasAsync(request).ToTask();
        }

        /// <summary>
        /// Apply SCA for the given list of account IDs.
        /// </summary>
        /// <param name="accountIds">the list of account ids</param>
        /// <returns>a task</returns>
        public Task ApplySca(IList<string> accountIds)
        {
            var request = new ApplyScaRequest {AccountId = {accountIds}};
            return gateway(authenticationContext(Level.Standard)).ApplyScaAsync(request).ToTask();
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
        /// Get a list of paired devices.
        /// </summary>
        /// <returns>the list</returns>
        public Task<IList<Device>> GetPairedDevices()
        {
            return gateway(authenticationContext())
                .GetPairedDevicesAsync(new GetPairedDevicesRequest())
                .ToTask(response => (IList<Device>) response.Devices);
        }

        /// <summary>
        /// Verifies an affiliated TPP.
        /// </summary>
        /// <param name="memberId">member ID of the TPP verify</param>
        /// <returns>a task</returns>
        public Task VerifyAffiliate(string memberId)
        {
            var request = new VerifyAffiliateRequest {MemberId = memberId};
            return gateway(authenticationContext()).VerifyAffiliateAsync(request).ToTask();
        }

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        public Task<IList<TransferEndpoint>> ResolveTransferDestination(string accountId)
        {
            var request = new ResolveTransferDestinationsRequest {AccountId = accountId};
            return gateway(authenticateOnBehalfOf()).ResolveTransferDestinationsAsync(request)
                .ToTask(response => (IList<TransferEndpoint>) response.Destinations);
        }

        /// <summary>
        /// Adds a trusted beneficiary for whom the SCA will be skipped.
        /// </summary>
        /// <param name="payload">the trusted beneficiary payload</param>
        /// <returns>a task</returns>
        public Task AddTrustedBeneficiary(TrustedBeneficiary.Types.Payload payload)
        {
            var signer = cryptoEngine.CreateSigner(Level.Standard);
            var request = new AddTrustedBeneficiaryRequest
            {
                TrustedBeneficiary = new TrustedBeneficiary
                {
                    Payload = payload,
                    Signature = new Signature
                    {
                        KeyId = signer.GetKeyId(),
                        MemberId = MemberId,
                        Signature_ = signer.Sign(payload)
                    }
                }
            };
            return gateway(authenticationContext()).AddTrustedBeneficiaryAsync(request).ToTask();
        }

        /// <summary>
        /// Removes a trusted beneficiary. 
        /// </summary>
        /// <param name="payload">the trusted beneficiary payload</param>
        /// <returns>a task</returns>
        public Task RemoveTrustedBeneficiary(TrustedBeneficiary.Types.Payload payload)
        {
            var signer = cryptoEngine.CreateSigner(Level.Standard);
            var request = new RemoveTrustedBeneficiaryRequest
            {
                TrustedBeneficiary = new TrustedBeneficiary
                {
                    Payload = payload,
                    Signature = new Signature
                    {
                        KeyId = signer.GetKeyId(),
                        MemberId = MemberId,
                        Signature_ = signer.Sign(payload)
                    }
                }
            };
            return gateway(authenticationContext()).RemoveTrustedBeneficiaryAsync(request).ToTask();
        }

        /// <summary>
        /// Gets a list of all trusted beneficiaries.
        /// </summary>
        /// <returns>the list</returns>
        public Task<IList<TrustedBeneficiary>> GetTrustedBeneficiaries()
        {
            var request = new GetTrustedBeneficiariesRequest();
            return gateway(authenticationContext()).GetTrustedBeneficiariesAsync(request)
                .ToTask(response => (IList<TrustedBeneficiary>) response.TrustedBeneficiaries);
        }

        internal Client Clone()
        {
            return new Client(MemberId, cryptoEngine, channel);
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
                        Signature_ = Stringify(tokenToCancel, TokenAction.Cancelled)
                    }
                },
                CreateToken = tokenToCreate
            };
            return gateway(authenticationContext()).ReplaceTokenAsync(request)
                .ToTask(response => response.Result);
        }

        private string Stringify(Token token, TokenAction action)
        {
            return Stringify(token.Payload, action);
        }

        private string Stringify(TokenPayload payload, TokenAction action)
        {
            return $"{Util.ToJson(payload)}.{action.ToString().ToLower()}";
        }

        private AuthenticationContext authenticationContext(Level level = Level.Low)
        {
            return new AuthenticationContext(null, level, false, securityMetadata);
        }

        private AuthenticationContext authenticateOnBehalfOf(Level level = Level.Low)
        {
            return new AuthenticationContext(onBehalfOf, level, customerInitiated, securityMetadata);
        }

        private GatewayServiceClient gateway(AuthenticationContext authentication)
        {
            var intercepted = channel.BuildInvoker()
                .Intercept(new AsyncClientAuthenticator(MemberId, cryptoEngine, authentication));
            return new GatewayService.GatewayServiceClient(intercepted);
        }
        
        public Task DeleteMember()
        {
            return gateway(authenticationContext(Level.Privileged))
                .DeleteMemberAsync(new DeleteMemberRequest())
                .ToTask();
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
                Colors = {colors},
                Name = name,
                ConsentText = consentText,
                AppName = appName
            };
            return gateway(authenticationContext())
                .CreateCustomizationAsync(request)
                .ToTask(response => response.CustomizationId);
        }

        public void ClearTrackingMetaData()
        {
            this.securityMetadata = new SecurityMetadata();
        }

        public Task<NotifyStatus> TriggerBalanceStepUpNotification(IList<string> accountIds)
        {
            var request = new TriggerStepUpNotificationRequest
            {
                BalanceStepUp = new BalanceStepUp
                {
                    AccountId = {accountIds}
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
    }
}
