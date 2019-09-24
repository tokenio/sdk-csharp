using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Tokenio.Exceptions;
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
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using Tokenio.Utils;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Gateway.GatewayService;
using static Tokenio.Proto.Gateway.ReplaceTokenRequest.Types;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using TokenAction = Tokenio.Proto.Common.TokenProtos.TokenSignature.Types.Action;

namespace Tokenio.Rpc
{
    /// <summary>
    /// An authenticated RPC client that is used to talk to Token gateway. The
    /// class is a thin wrapper on top of gRPC generated client. Makes the API
    /// easier to use.
    /// </summary>
    public class Client
    {
        protected readonly ICryptoEngine cryptoEngine;
        protected readonly ManagedChannel channel;
        protected bool customerInitiated;
        private SecurityMetadata trackingMetadata = new SecurityMetadata();
        protected string onBehalfOf;

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
            var request = new GetMemberRequest { MemberId = memberId };
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

            ISigner signer = cryptoEngine.CreateSigner(Level.Privileged);
            return GetMember(MemberId)
                .FlatMap(member => gateway(authenticationContext())
                .GetDefaultAgentAsync(new GetDefaultAgentRequest())
                .ToTask(response =>
                {
                    var operations = new MemberOperation
                    {
                        RecoveryRules = new MemberRecoveryRulesOperation
                        {
                            RecoveryRule = new RecoveryRule { PrimaryAgent = response.MemberId }
                        }
                    };
                    return new MemberUpdate
                    {
                        PrevHash = member.LastHash,
                        MemberId = member.Id,
                        Operations = { operations }
                    };
                })
                .Map(update => gateway(authenticationContext())
                .UpdateMemberAsync(new UpdateMemberRequest
                {
                    Update = update,
                    UpdateSignature = new Signature
                    {
                        KeyId = signer.GetKeyId(),
                        MemberId = MemberId,
                        Signature_ = signer.Sign(update)
                    }
                })
                .ToTask())
                );
        }

        /// <summary>
        /// Signs a token payload.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="keyLevel"></param>
        /// <returns></returns>
        public Signature SignTokenPayload(TokenPayload payload, Level keyLevel)
        {
            ISigner signer = cryptoEngine.CreateSigner(keyLevel);
            return new Signature
            {
                KeyId = signer.GetKeyId(),
                MemberId = MemberId,
                Signature_ = signer.Sign(Stringify(payload, TokenAction.Endorsed))

            };
        }

        /// <summary>
        /// Looks up a linked funding account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account info</returns>
        public Task<ProtoAccount> GetAccount(string accountId)
        {
            var request = new GetAccountRequest { AccountId = accountId };
            return gateway(authenticateOnBehalfOf()).GetAccountAsync(request)
                .ToTask(response => response.Account);
        }

        /// <summary>
        /// Looks up all the linked funding accounts.
        /// </summary>
        /// <returns>a list of linked accounts</returns>
        public Task<IList<ProtoAccount>> GetAccounts()
        {
            var acc = gateway(authenticateOnBehalfOf())
                .GetAccountsAsync(new GetAccountsRequest())
                .ToTask(response => (IList<ProtoAccount>)response.Accounts);
            return acc;
        }

        /// <summary>
        /// Look up account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the account balance</returns>
        /// <exception cref="StepUpRequiredException"></exception>
        public Task<Balance> GetBalance(string accountId, Level keyLevel)
        {
            var request = new GetBalanceRequest { AccountId = accountId };
            return gateway(authenticateOnBehalfOf(keyLevel)).GetBalanceAsync(request)
                .ToTask(response =>
                {
                    switch (response.Status)
                    {
                        case RequestStatus.SuccessfulRequest:
                            return response.Balance;
                        case RequestStatus.MoreSignaturesNeeded:
                            throw new StepUpRequiredException("Balance step up required.");
                        default:
                            throw new RequestException(response.Status);
                    }
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
                AccountId = { accountIds }
            };
            return gateway(authenticateOnBehalfOf(keyLevel)).GetBalancesAsync(request)
                .ToTask(response =>
                {
                    IList<Balance> balances = new List<Balance>();
                    foreach (GetBalanceResponse getBalanceResponse in response.Response)
                    {
                        switch (getBalanceResponse.Status)
                        {
                            case RequestStatus.SuccessfulRequest:
                                balances.Add(getBalanceResponse.Balance);
                                break;
                            case RequestStatus.MoreSignaturesNeeded:
                                throw new StepUpRequiredException("Balance step up required.");
                            default:
                                throw new RequestException(getBalanceResponse.Status);
                        }
                    }
                    return balances;
                });
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
                    switch (response.Status)
                    {
                        case RequestStatus.SuccessfulRequest:
                            return response.Transaction;
                        case RequestStatus.MoreSignaturesNeeded:
                            throw new StepUpRequiredException("Balance step up required.");
                        default:
                            throw new RequestException(response.Status);
                    }
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
            string offset = null)
        {
            var request = new GetTransactionsRequest
            {
                AccountId = accountId,
                Page = PageBuilder(limt, offset)
            };

            return gateway(authenticateOnBehalfOf(keyLevel)).GetTransactionsAsync(request)
                .ToTask(response =>
                {
                    switch (response.Status)
                    {
                        case RequestStatus.SuccessfulRequest:
                            return new PagedList<Transaction>(response.Transactions, response.Offset);
                        case RequestStatus.MoreSignaturesNeeded:
                            throw new StepUpRequiredException("Balance step up required.");
                        default:
                            throw new RequestException(response.Status);
                    }
                });
        }

        /// <summary>
        /// Look up an existing standing order and return the response.
        /// </summary>
        /// <param name="accountId">account ID</param>
        /// <param name="standingOrderId">standing order ID</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>transaction</returns>
        public Task<StandingOrder> GetStandingOrder(
            string accountId,
            string standingOrderId,
            Level keyLevel)
        {
            var request = new GetStandingOrderRequest
            {
                AccountId = accountId,
                StandingOrderId = standingOrderId
            };
            return gateway(authenticateOnBehalfOf(keyLevel))
                    .GetStandingOrderAsync(request)
                    .ToTask(response =>
                    {
                        switch (response.Status)
                        {
                            case RequestStatus.SuccessfulRequest:
                                return response.StandingOrder;
                            case RequestStatus.MoreSignaturesNeeded:
                                throw new StepUpRequiredException("Balance step up required.");
                            default:
                                throw new RequestException(response.Status);
                        }
                    });
        }

        /// <summary>
        /// Look up standing orders and return response.
        /// </summary>
        /// <param name="accountId">account ID</param>
        /// <param name="limit">limit</param>
        /// <param name="keyLevel">key level</param>
        /// <param name="offset">offset</param>
        /// <returns></returns>
        public Task<PagedList<StandingOrder>> GetStandingOrders(
                string accountId,
                int limit,
                Level keyLevel,
                string offset = null)
        {
            var request = new GetStandingOrdersRequest
            {
                AccountId = accountId,
                Page = PageBuilder(limit, offset)
            };
            return gateway(authenticateOnBehalfOf(keyLevel))
                    .GetStandingOrdersAsync(request)
                    .ToTask(response =>
                    {
                        switch (response.Status)
                        {
                            case RequestStatus.SuccessfulRequest:
                                return new PagedList<StandingOrder>(response.StandingOrders, response.Offset);
                            case RequestStatus.MoreSignaturesNeeded:
                                throw new StepUpRequiredException("Balance step up required.");
                            default:
                                throw new RequestException(response.Status);
                        }
                    });
        }

        /// <summary>
        /// Confirm that the given account has sufficient funds to cover the charge.
        /// </summary>
        /// <param name="accountId">account ID</param>
        /// <param name="amount">charge amount</param>
        /// <returns>true if the account has sufficient funds to cover the charge</returns>
        public Task<bool> ConfirmFunds(string accountId, Money amount)
        {
            var request = new ConfirmFundsRequest
            {
                AccountId = accountId,
                Amount = amount
            };

            return gateway(authenticateOnBehalfOf())
                .ConfirmFundsAsync(request)
                .ToTask(response => response.FundsAvailable);
        }

        /// <summary>
        /// Returns linking information for the specified bank id.
        /// </summary>
        /// <param name="bankId">the bank id</param>
        /// <returns>the bank linking information</returns>
        public Task<BankInfo> GetBankInfo(string bankId)
        {
            var request = new GetBankInfoRequest { BankId = bankId };
            return gateway(authenticationContext()).GetBankInfoAsync(request)
                .ToTask(response => response.Info);
        }

        /// <summary>
        /// Links a funding bank account to Token.
        /// </summary>
        /// <param name="authorization">an OAuth authorization for linking</param>
        /// <returns>a list of linked accounts</returns>
        /// <exception cref="BankAuthorizationRequiredException"></exception>
        public Task<IList<ProtoAccount>> LinkAccounts(OauthBankAuthorization authorization)
        {
            var request = new LinkAccountsOauthRequest { Authorization = authorization };
            return gateway(authenticationContext()).LinkAccountsOauthAsync(request)
                .ToTask(response =>
                {
                    if (response.Status == AccountLinkingStatus.FailureBankAuthorizationRequired)
                    {
                        throw new BankAuthorizationRequiredException();
                    }
                    return (IList<ProtoAccount>)response.Accounts;
                });
        }

        /// <summary>
        /// Creates a test bank account and links it.
        /// </summary>
        /// <param name="balance">account balance to set</param>
        /// <returns>linked account</returns>
        public Task<ProtoAccount> CreateAndLinkTestBankAccount(Money balance)
        {

            return CreateTestBankAuth(balance)
            .FlatMap(Authorization =>
            {
                return LinkAccounts(Authorization)
               .Map(accounts =>
               {
                   if (accounts.Count != 1)
                   {
                       throw new SystemException("Expected 1 account; found " + accounts.Count);
                   }
                   return accounts.ElementAt(0);
               });
            });
        }

        /// <summary>
        /// Returns a list of aliases of the member.
        /// </summary>
        /// <returns>a list of aliases</returns>
        public Task<IList<Alias>> GetAliases()
        {
            var request = new GetAliasesRequest();
            return gateway(authenticationContext()).GetAliasesAsync(request)
                .ToTask(response => (IList<Alias>)response.Aliases);
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
            var signer = cryptoEngine.CreateSigner(Level.Standard);
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
        /// Delete the member.
        /// </summary>
        /// <returns>Task</returns>
        public Task DeleteMember()
        {
            return gateway(authenticationContext(Level.Privileged))
                .DeleteMemberAsync(new DeleteMemberRequest())
                .ToTask();
        }

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        public Task<IList<TransferDestination>> ResolveTransferDestination(string accountId)
        {
            var request = new ResolveTransferDestinationsRequest { AccountId = accountId };
            return gateway(authenticateOnBehalfOf()).ResolveTransferDestinationsAsync(request)
                .ToTask(response => (IList<TransferDestination>)response.TransferDestinations);
        }

        /// <summary>
        /// Sets the security metadata to be sent with each request.
        /// </summary>
        /// <param name="trackingMetadata">security metadata</param>
        /// TODO: RD-2335: Change from SecurityMetaData to TrackingMetaData
        public void SetTrackingMetadata(SecurityMetadata trackingMetadata)
        {
            this.trackingMetadata = trackingMetadata;
        }

        /// <summary>
        /// Clears tracking metadata
        /// </summary>
        public void ClearTrackingMetaData()
        {
            this.trackingMetadata = new SecurityMetadata();
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
        /// Gets a member's public profile.
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public Task<Profile> GetProfile(string memberId)
        {
            var request = new GetProfileRequest
            {
                MemberId = memberId
            };
            return gateway(authenticationContext())
                .GetProfileAsync(request)
                .ToTask(response => response.Profile);
        }

        public Task<Blob> GetProfilePicture(string memberId, ProfilePictureSize size)
        {
            var request = new GetProfilePictureRequest
            {
                MemberId = memberId,
                Size = size
            };
            return gateway(authenticationContext())
                .GetProfilePictureAsync(request)
                .ToTask(response => response.Blob);
        }

        /// <summary>
        /// Creates and uploads a blob.
        /// </summary>
        /// <param name="payload">the blob payload</param>
        /// <returns>id of the blob</returns>
        public Task<string> CreateBlob(Payload payload)
        {
            var request = new CreateBlobRequest { Payload = payload };
            return gateway(authenticationContext()).CreateBlobAsync(request)
                .ToTask(response => response.BlobId);
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
            var request = new GetAddressRequest { AddressId = addressId };
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
                .ToTask(response => (IList<AddressRecord>)response.Addresses);
        }

        /// <summary>
        /// Deletes a member address by its id.
        /// </summary>
        /// <param name="addressId">the id of the address</param>
        /// <returns>a task</returns>
        public Task DeleteAddress(string addressId)
        {
            var request = new DeleteAddressRequest { AddressId = addressId };
            return gateway(authenticationContext()).DeleteAddressAsync(request).ToTask();
        }

        /// <summary>
        /// Get a list of paired devices.
        /// </summary>
        /// <returns>the list</returns>
        public Task<IList<Device>> GetPairedDevices()
        {
            return gateway(authenticationContext())
                .GetPairedDevicesAsync(new GetPairedDevicesRequest())
                .ToTask(response => (IList<Device>)response.Devices);
        }

        /// <summary>
        /// Verifies an affiliated TPP.
        /// </summary>
        /// <param name="memberId">member ID of the TPP verify</param>
        /// <returns>a task</returns>
        public Task VerifyAffiliate(string memberId)
        {
            var request = new VerifyAffiliateRequest { MemberId = memberId };
            return gateway(authenticationContext()).VerifyAffiliateAsync(request).ToTask();
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
                .ToTask(response => (IList<TrustedBeneficiary>)response.TrustedBeneficiaries);
        }

        protected Client Clone()
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
                        Signature_ = signer.Sign(Stringify(tokenToCancel, TokenAction.Cancelled))
                    }
                },
                CreateToken = tokenToCreate
            };
            return gateway(authenticationContext()).ReplaceTokenAsync(request)
                .ToTask(response => response.Result);
        }


        private Task<OauthBankAuthorization> CreateTestBankAuth(Money balance)
        {
            var request = new CreateTestBankAccountRequest { Balance = balance };
            return gateway(authenticationContext())
                .CreateTestBankAccountAsync(request)
                .ToTask(response => response.Authorization);
        }



        protected virtual string GetOnBehalfOf()
        {
            return onBehalfOf;
        }

        public ICryptoEngine GetCryptoEngine()
        {
            return cryptoEngine;
        }

        protected Page PageBuilder(int limit, string offset = null)
        {
            Page page = new Page { Limit = limit };
            if (offset != null)
            {
                page.Offset = offset;
            }

            return page;
        }


        protected string Stringify(Token token, TokenAction action)
        {
            return Stringify(token.Payload, action);
        }

        protected string Stringify(TokenPayload payload, TokenAction action)
        {
            return $"{Util.ToJson(payload)}.{action.ToString().ToLower()}";
        }

        protected virtual AuthenticationContext authenticationContext()
        {
            return authenticationContext(Level.Low);
        }
        protected AuthenticationContext authenticationContext(Level level)
        {
            return new AuthenticationContext(null, level, false, trackingMetadata);
        }

        protected AuthenticationContext authenticateOnBehalfOf(Level level = Level.Low)
        {
            return new AuthenticationContext(GetOnBehalfOf(), level, customerInitiated, trackingMetadata);
        }

        protected GatewayServiceClient gateway(AuthenticationContext authentication)
        {
            var intercepted = channel.BuildInvoker()
                .Intercept(new AsyncClientAuthenticator(MemberId, cryptoEngine, authentication));
            return new GatewayService.GatewayServiceClient(intercepted);
        }
    }
}
