using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using log4net;
using Tokenio.Exceptions;
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
using Tokenio.Rpc;
using Tokenio.Security;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;

namespace Tokenio
{
    [Obsolete("deprecated, use Member instead")]
    public class MemberAsync : IRepresentableAsync
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Client client;

        /// <summary>
        /// Creates an instance of <see cref="MemberAsync"/>
        /// </summary>
        /// <param name="client">the gRPC client</param>
        public MemberAsync(Client client)
        {
            this.client = client;
        }

        /// <summary>
        /// Gets a synchronous version of the API.
        /// </summary>
        /// <returns>an instance of <see cref="MemberSync"/></returns>
        public MemberSync Sync()
        {
            return new MemberSync(this);
        }

        /// <summary>
        /// Gets the member id.
        /// </summary>
        /// <returns>the member id</returns>
        public string MemberId()
        {
            return client.MemberId;
        }

        /// <summary>
        /// Gets the last hash.
        /// </summary>
        /// <returns>the last hash</returns>
        public Task<string> LastHash()
        {
            return client
                .GetMember()
                .Map(m => m.LastHash);
        }

        /// <summary>
        /// Gets all aliases owned by the member.
        /// </summary>
        /// <returns>a list of aliases</returns>
        public Task<IList<Alias>> Aliases()
        {
            return client.GetAliases();
        }

        /// <summary>
        /// Gets the fisrt alias owned by the user.
        /// </summary>
        /// <returns>the alias</returns>
        public Task<Alias> FirstAlias()
        {
            return Aliases().Map(aliases => aliases.Count > 0 ? aliases[0] : null);
        }

        /// <summary>
        /// Gets all public keys for this member.
        /// </summary>
        /// <returns>a list of public keys</returns>
        public Task<IList<Key>> Keys()
        {
            return client
                .GetMember()
                .Map(member => (IList<Key>) member.Keys.ToList());
        }

        /// <summary>
        /// Creates a representable that acts as another member.
        /// </summary>
        /// <param name="accessTokenId">the access token id to be used</param>
        /// <param name="customerInitiated">whether the customer initiated the call</param>
        /// <returns>the representable</returns>>
        public IRepresentableAsync ForAccessToken(string accessTokenId, bool customerInitiated = false)
        {
            return ForAccessTokenInternal(accessTokenId, customerInitiated);
        }

        internal MemberAsync ForAccessTokenInternal(string accessTokenId, bool customerInitiated)
        {
            var cloned = client.Clone();
            cloned.UseAccessToken(accessTokenId, customerInitiated);
            return new MemberAsync(cloned);
        }

        /// <summary>
        /// Sets the security metadata to be sent with each request.
        /// </summary>
        /// <param name="securityMetadata">security metadata</param>
        public void SetSecurityMetadata(SecurityMetadata securityMetadata)
        {
            client.SetSecurityMetadata(securityMetadata);
        }

        /// <summary>
        /// Adds new aliases for the member.
        /// </summary>
        /// <param name="aliases"></param>
        /// <returns>a task</returns>
        public Task AddAliases(IList<Alias> aliases)
        {
            return client.GetMember().FlatMap(member => {
                aliases = aliases.Select(alias => {
                    var partnerId = member.PartnerId;
                    if (!string.IsNullOrEmpty(partnerId) && !partnerId.Equals("token")) {
                        // Realm must equal member's partner ID if affiliated
                        if (!string.IsNullOrEmpty(alias.Realm) && !alias.Realm.Equals(partnerId)) {
                            throw new InvalidRealmException(alias.Realm, partnerId);
                        }
                        alias.Realm = partnerId;
                    }
                    return alias;
                }).ToList();

                var operations = aliases.Select(Util.ToAddAliasOperation).ToList();
                var metadata = aliases.Select(Util.ToAddAliasMetadata).ToList();
                return client.UpdateMember(operations, metadata);
            });
        }

        /// <summary>
        /// Adds a new alias for the member.
        /// </summary>
        /// <param name="alias">the alias</param>
        /// <returns>a task</returns>
        public Task AddAlias(Alias alias)
        {
            return AddAliases(new List<Alias> {alias});
        }

        /// <summary>
        /// Retries alias verification.
        /// </summary>
        /// <param name="alias">the alias to be verified</param>
        /// <returns>the verification id</returns>
        public Task<string> RetryVerification(Alias alias)
        {
            return client.RetryVerification(alias);
        }

        /// <summary>
        /// Adds the recovery rule.
        /// </summary>
        /// <param name="rule">the recovery rule</param>
        /// <returns>the updated member</returns>
        public Task<ProtoMember> AddRecoveryRule(RecoveryRule rule)
        {
            return client.UpdateMember(new List<MemberOperation>
            {
                new MemberOperation
                {
                    RecoveryRules = new MemberRecoveryRulesOperation
                    {
                        RecoveryRule = rule
                    }
                }
            });
        }

        /// <summary>
        /// Set Token as the recovery agent.
        /// </summary>
        /// <returns>a task</returns>
        public Task UseDefaultRecoveryRule()
        {
            return client.UseDefaultRecoveryRule();
        }

        /// <summary>
        /// Authorizes recovery as a trusted agent.
        /// </summary>
        /// <param name="authorization">the authorization</param>
        /// <returns>the signature</returns>
        public Task<Signature> AuthorizeRecovery(Authorization authorization)
        {
            return client.AuthorizeRecovery(authorization);
        }

        /// <summary>
        /// Gets the member id of the default recovery agent.
        /// </summary>
        /// <returns>the member id</returns>
        public Task<string> GetDefaultAgent()
        {
            return client.GetDefaultAgent();
        }

        /// <summary>
        /// Verifies a given alias.
        /// </summary>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the verification code</param>
        /// <returns>a task</returns>
        public Task VerifyAlias(string verificationId, string code)
        {
            return client.VerifyAlias(verificationId, code);
        }

        /// <summary>
        /// Removes an alias for the member.
        /// </summary>
        /// <param name="aliases">the aliases to remove</param>
        /// <returns>a task</returns>
        public Task RemoveAliases(IList<Alias> aliases)
        {
            var operations = aliases.Select(Util.ToRemoveAliasOperation).ToList();
            return client.UpdateMember(operations);
        }

        /// <summary>
        /// Removes an alias for the member.
        /// </summary>
        /// <param name="alias">the alias to remove</param>
        /// <returns>a task</returns>
        public Task RemoveAlias(Alias alias)
        {
            return RemoveAliases(new List<Alias> {alias});
        }

        /// <summary>
        /// Approves public keys owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="keys">the keys to add</param>
        /// <returns>a task</returns>
        public Task ApproveKeys(IList<Key> keys)
        {
            var operations = keys.Select(Util.ToAddKeyOperation).ToList();
            return client.UpdateMember(operations);
        }

        /// <summary>
        /// Approves a public key owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="key">the key to add</param>
        /// <returns>a task</returns>
        public Task ApproveKey(Key key)
        {
            return ApproveKeys(new List<Key> {key});
        }

        /// <summary>
        /// Approves a key owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="keyPair">the keypair to add</param>
        /// <returns>a task</returns>
        public Task ApproveKey(KeyPair keyPair)
        {
            return ApproveKey(keyPair.ToKey());
        }

        /// <summary>
        /// Removes some public keys owned by this member.
        /// </summary>
        /// <param name="keyIds">the IDs of the keys to remove</param>
        /// <returns>a task</returns>
        public Task RemoveKeys(IList<string> keyIds)
        {
            var operations = keyIds.Select(Util.ToRemoveKeyOperation).ToList();
            return client.UpdateMember(operations);
        }

        /// <summary>
        /// Removes a public key owned by this member.
        /// </summary>
        /// <param name="keyId">the key id</param>
        /// <returns>a task</returns>
        public Task RemoveKey(string keyId)
        {
            return RemoveKeys(new List<string> {keyId});
        }

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of accounts</returns>
        public Task<IList<AccountAsync>> GetAccounts()
        {
            return client
                .GetAccounts()
                .Map(accounts => (IList<AccountAsync>) accounts
                    .Select(account => new AccountAsync(this, account, client))
                    .ToList());
        }

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        public Task<AccountAsync> GetAccount(string accountId)
        {
            return client
                .GetAccount(accountId)
                .Map(account => new AccountAsync(this, account, client));
        }

        /// <summary>
        /// Get the default bank account for this member.
        /// </summary>
        /// <returns>the account</returns>
        public Task<AccountAsync> GetDefaultAccount()
        {
            return client
                .GetDefaultAccount(MemberId())
                .Map(account => new AccountAsync(this, account, client));
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
        /// Creates and uploads a blob.
        /// </summary>
        /// <param name="ownerId">the id of the owner of the blob</param>
        /// <param name="type">the MIME type of the file</param>
        /// <param name="name">the name of the file</param>
        /// <param name="data">the file data</param>
        /// <param name="accessMode">the access mode, normal or public</param>
        /// <returns>an attachment</returns>
        public Task<Attachment> CreateBlob(
            string ownerId,
            string type,
            string name,
            byte[] data,
            AccessMode accessMode = AccessMode.Default)
        {
            var payload = new Payload
            {
                OwnerId = ownerId,
                Type = type,
                Name = name,
                Data = ByteString.CopyFrom(data),
                AccessMode = accessMode
            };
            return client.CreateBlob(payload)
                .Map(id => new Attachment
                {
                    BlobId = id,
                    Name = name,
                    Type = type
                });
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
        /// Retrieves a blob that is attached to a transfer token.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="blobId">the blob id</param>
        /// <returns>the blob</returns>
        public Task<Blob> GetTokenBlob(string tokenId, string blobId)
        {
            return client.GetTokenBlob(tokenId, blobId);
        }

        /// <summary>
        /// Creates a new member address.
        /// </summary>
        /// <param name="name">the name of the address</param>
        /// <param name="address">the address</param>
        /// <returns>the created address record</returns>
        public Task<AddressRecord> AddAddress(string name, Address address)
        {
            return client.AddAddress(name, address);
        }

        /// <summary>
        /// Looks up an address by id.
        /// </summary>
        /// <param name="addressId">the address id</param>
        /// <returns>the address record</returns>
        public Task<AddressRecord> GetAddress(string addressId)
        {
            return client.GetAddress(addressId);
        }

        /// <summary>
        /// Looks up member addresses.
        /// </summary>
        /// <returns>a list of addresses</returns>
        public Task<IList<AddressRecord>> GetAddresses()
        {
            return client.GetAddresses();
        }

        /// <summary>
        /// Deletes a member address by its id.
        /// </summary>
        /// <param name="addressId">the address id</param>
        /// <returns>a task</returns>
        public Task DeleteAddress(string addressId)
        {
            return client.DeleteAddress(addressId);
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
        /// Gets a member's public profile. Unlike setProfile, you can get another member's profile.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <returns>the profile</returns>
        public Task<Profile> GetProfile(string memberId)
        {
            return client.GetProfile(memberId);
        }

        /// <summary>
        /// Replaces auth'd member's public profile picture.
        /// </summary>
        /// <param name="type">MIME type of the picture</param>
        /// <param name="data">the image data</param>
        /// <returns>a task</returns>
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
        /// Gets a member's public profile picture. Unlike set, you can get another member's picture.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <param name="size">the desired size category (small, medium, large, original)</param>
        /// <returns>a blob with picture; empty if the member has no picture</returns>
        public Task<Blob> GetProfilePicture(string memberId, ProfilePictureSize size)
        {
            return client.GetProfilePicture(memberId, size);
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
        /// <param name="tokenRequest">the token request</param>
        /// <returns>an id to reference the token request</returns>
        [Obsolete("Deprecated. Use StoreTokenRequest(TokenRequestPayload, TokenRequestOptions) instead.")]
        public Task<string> StoreTokenRequest(TokenRequest tokenRequest)
        {
            return client.StoreTokenRequest(tokenRequest.Payload, tokenRequest.Options);
        }

        /// <summary>
        /// Update an existing token request.
        /// </summary>
        /// <param name="requestId">token request ID</param>
        /// <param name="options">new token request options</param>
        /// <returns>a task</returns>
        public Task UpdateTokenRequest(string requestId, Proto.Common.TokenProtos.TokenRequestOptions options)
        {
            return client.UpdateTokenRequest(requestId, options);
        }

        /// <summary>
        /// Creates a new transfer token.
        /// </summary>
        /// <param name="payload">the transfer token payload</param>
        /// <returns>the transfer token</returns>
        public Task<Token> CreateTransferToken(TokenPayload payload)
        {
            return client.CreateTransferToken(payload);
        }

        /// <summary>
        /// Creates a new transfer token builder.
        /// </summary>
        /// <param name="amount">the transfer amount</param>
        /// <param name="currency">the currency code, e.g. "USD"</param>
        /// <returns>the transfer token builder</returns>
        public TransferTokenBuilder CreateTransferToken(double amount, string currency)
        {
            return new TransferTokenBuilder(this, amount, currency);
        }

        /// <summary>
        /// Creates an access token.
        /// </summary>
        /// <param name="payload">the access token payload</param>
        /// <returns>the access token</returns>
        public Task<Token> CreateAccessToken(TokenPayload payload)
        {
            return client.CreateAccessToken(payload);
        }

        /// <summary>
        /// Creates an access token with a token request id.
        /// </summary>
        /// <param name="payload">the access token payload</param>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the access token</returns>
        public Task<Token> CreateAccessToken(TokenPayload payload, string tokenRequestId)
        {
            return client.CreateAccessToken(payload, tokenRequestId);
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
        /// Looks up exsiting transfer tokens.
        /// </summary>
        /// <param name="limit">the max number of records to return</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <returns>a paged list of transfer tokens</returns>
        public Task<PagedList<Token>> GetTransferTokens(int limit, string offset)
        {
            return client.GetTokens(TokenType.Transfer, limit, offset);
        }

        /// <summary>
        /// Looks up existing access tokens.
        /// </summary>
        /// <param name="limit">the max number of records to return</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <returns>a paged list of access tokens</returns>
        public Task<PagedList<Token>> GetAccessTokens(int limit, string offset)
        {
            return client.GetTokens(TokenType.Access, limit, offset);
        }

        /// <summary>
        /// Endorses the token by signing it. The signature is persisted along with
        /// the token
        /// If the key's level is too low, the result's status is MORE_SIGNATURES_NEEDED
        /// and the system pushes a notification to the member prompting them to use a
        /// higher-privilege key.
        /// </summary>
        /// <param name="token">the token to endorse</param>
        /// <param name="keyLevel">the key level to be used to endorse the token</param>
        /// <returns>the result of the endorsement</returns>
        public Task<TokenOperationResult> EndorseToken(Token token, Level keyLevel)
        {
            return client.EndorseToken(token, keyLevel);
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
        /// Cancels the existing access token and creates a replacement for it.
        /// </summary>
        /// <param name="tokenToCancel">the token to cancel</param>
        /// <param name="tokenToCreate">the payload to create new token with</param>
        /// <returns>the result of the replacement opration</returns>
        public Task<TokenOperationResult> ReplaceAccessToken(
            Token tokenToCancel,
            TokenPayload tokenToCreate)
        {
            tokenToCreate.From.Id = MemberId();
            return client.ReplaceToken(
                tokenToCancel,
                tokenToCreate);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(Token token)
        {
            return RedeemToken(token, null, null, null, null, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(Token token, string refId)
        {
            return RedeemToken(token, null, null, null, null, refId);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
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
            return RedeemToken(token, amount, currency, description, null, null);
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
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        /// <remarks>amount, currency, description, destination and refId are nullable</remarks>>
        public Task<Transfer> RedeemToken(
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

        /// <summary>
        /// Looks up an existing transaction for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="transactionId">the transaction ID</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the transaction</returns>
        public Task<Transaction> GetTransaction(
            string accountId,
            string transactionId,
            Level keyLevel)
        {
            return client.GetTransaction(accountId, transactionId, keyLevel);
        }

        /// <summary>
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">the key level</param>
        /// <param name="offset">the nullable offset to start at</param>
        /// <returns>a paged list of transactions</returns>
        public Task<PagedList<Transaction>> GetTransactions(
            string accountId,
            int limit,
            Level keyLevel,
            string offset)
        {
            return client.GetTransactions(accountId, limit, keyLevel, offset);
        }

        /// <summary>
        /// Looks up account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        public Task<Balance> GetBalance(string accountId, Level keyLevel)
        {
            return client.GetBalance(accountId, keyLevel);
        }

        /// <summary>
        /// Looks up current account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        public Task<Money> GetCurrentBalance(string accountId, Level keyLevel)
        {
            return client.GetBalance(accountId, keyLevel)
                .Map(response => response.Current);
        }

        /// <summary>
        /// Looks up available account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        public Task<Money> GetAvailableBalance(string accountId, Level keyLevel)
        {
            return client.GetBalance(accountId, keyLevel)
                .Map(response => response.Available);
        }

        /// <summary>
        /// Looks up balances for a list of accounts.
        /// </summary>
        /// <param name="accountIds">the list of accounts</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>a list of balances</returns>
        public Task<IList<Balance>> GetBalances(IList<string> accountIds, Level keyLevel)
        {
            return client.GetBalances(accountIds, keyLevel);
        }

        /// <summary>
        /// Returns linking information for a specified bank id.
        /// </summary>
        /// <param name="bankId">the bank id</param>
        /// <returns>the bank linking information</returns>
        public Task<BankInfo> GetBankInfo(string bankId)
        {
            return client.GetBankInfo(bankId);
        }

        /// <summary>
        /// Applies SCA for the given a list of accounts.
        /// </summary>
        /// <param name="accountIds">the list of accounts</param>
        /// <returns>a task</returns>
        public Task ApplySca(IList<string> accountIds)
        {
            return client.ApplySca(accountIds);
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
            return client.SignTokenRequestState(tokenRequestId, tokenId, state);
        }

        /// <summary>
        /// Gets all paired devices.
        /// </summary>
        /// <returns>a list of devices</returns>
        public Task<IList<Device>> GetPairedDevices()
        {
            return client.GetPairedDevices();
        }

        /// <summary>
        /// Verifies an affiliated TPP.
        /// </summary>
        /// <param name="memberId">member ID of the TPP verify</param>
        /// <returns>a task</returns>
        public Task VerifyAffiliate(string memberId)
        {
            return client.VerifyAffiliate(memberId);
        }

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        public Task<IList<TransferEndpoint>> ResolveTransferDestination(string accountId)
        {
            return client.ResolveTransferDestination(accountId);
        }

        /// <summary>
        /// Adds a trusted beneficiary for whom the SCA will be skipped.
        /// </summary>
        /// <param name="memberId">the member id of the beneficiary</param>
        /// <returns>a task</returns>
        public Task AddTrustedBeneficiary(string memberId)
        {
            var payload = new TrustedBeneficiary.Types.Payload
            {
                MemberId = memberId,
                Nonce = Util.Nonce()
            };
            return client.AddTrustedBeneficiary(payload);
        }

        /// <summary>
        /// Removes a trusted beneficiary. 
        /// </summary>
        /// <param name="memberId">the member id of the beneficiary</param>
        /// <returns>a task</returns>
        public Task RemoveTrustedBeneficiary(string memberId)
        {
            var payload = new TrustedBeneficiary.Types.Payload
            {
                MemberId = memberId,
                Nonce = Util.Nonce()
            };
            return client.RemoveTrustedBeneficiary(payload);
        }

        /// <summary>
        /// Gets a list of all trusted beneficiaries.
        /// </summary>
        /// <returns>the list</returns>
        public Task<IList<TrustedBeneficiary>> GetTrustedBeneficiaries()
        {
            return client.GetTrustedBeneficiaries();
        }

        /// <summary>
        /// **For testing purposes only**
        /// Creates a linked test bank account.
        /// </summary>
        /// <param name="balance">the account balance to set</param>
        /// <returns>the OAuth bank authorization</returns>
        public Task<ProtoAccount> CreateAndLinkTestBankAccount(Money balance)
        {
            return client.CreateAndLinkTestBankAccount(balance);
        }

        internal Member toMember()
        {
            return new Member(client);
        }
    }
}
