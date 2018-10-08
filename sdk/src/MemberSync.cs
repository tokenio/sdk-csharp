using System.Collections.Generic;
using System.Linq;
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
using Tokenio.Security;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio
{
    public class MemberSync : IRepresentableSync
    {
        private readonly MemberAsync async;

        public MemberSync(MemberAsync async)
        {
            this.async = async;
        }

        /// <summary>
        /// Gets an asynchronous version of the API.
        /// </summary>
        /// <returns>an instance of <see cref="MemberSync"/></returns>
        public MemberAsync Async()
        {
            return async;
        }

        /// <summary>
        /// Gets the member id.
        /// </summary>
        /// <returns>the member id</returns>
        public string MemberId()
        {
            return async.MemberId();
        }

        /// <summary>
        /// Gets the last hash.
        /// </summary>
        /// <returns>the last hash</returns>
        public string LastHash()
        {
            return async.LastHash().Result;
        }

        /// <summary>
        /// Gets the fisrt alias owned by the user.
        /// </summary>
        /// <returns>the alias</returns>
        public Alias FirstAlias()
        {
            return async.FirstAlias().Result;
        }

        /// <summary>
        /// Gets all aliases owned by the member.
        /// </summary>
        /// <returns>a list of aliases</returns>
        public IList<Alias> Aliases()
        {
            return async.Aliases().Result;
        }

        /// <summary>
        /// Gets all public keys for this member.
        /// </summary>
        /// <returns>a list of public keys</returns>
        public IList<Key> Keys()
        {
            return async.Keys().Result;
        }
        
        /// <summary>
        /// Creates a representable that acts as another member.
        /// </summary>
        /// <param name="accessTokenId">the access token id to be used</param>
        /// <param name="customerInitiated">whether the customer initiated the call</param>
        /// <returns>the representable</returns>
        public IRepresentableSync ForAccessToken(string accessTokenId, bool customerInitiated = false)
        {
            var newAsync = async.ForAccessTokenInternal(accessTokenId, customerInitiated);
            return new MemberSync(newAsync);
        }

        /// <summary>
        /// Adds a new alias for the member.
        /// </summary>
        /// <param name="alias">the alias</param>
        /// <returns>a task</returns>
        public void AddAlias(Alias alias)
        {
            async.AddAlias(alias).Wait();
        }

        /// <summary>
        /// Adds new aliases for the member.
        /// </summary>
        /// <param name="aliases"></param>
        /// <returns>a task</returns>
        public void AddAliases(IList<Alias> aliases)
        {
            async.AddAliases(aliases).Wait();
        }

        /// <summary>
        /// Retries alias verification.
        /// </summary>
        /// <param name="alias">the alias to be verified</param>
        /// <returns>the verification id</returns>
        public string RetryVerification(Alias alias)
        {
            return async.RetryVerification(alias).Result;
        }

        /// <summary>
        /// Adds the recovery rule.
        /// </summary>
        /// <param name="rule">the recovery rule</param>
        /// <returns>the updated member</returns>
        public Member AddRecoveryRule(RecoveryRule rule)
        {
            return async.AddRecoveryRule(rule).Result;
        }

        /// <summary>
        /// Set Token as the recovery agent.
        /// </summary>
        /// <returns>a task</returns>
        public void UseDefaultRecoveryRule()
        {
            async.UseDefaultRecoveryRule().Wait();
        }

        /// <summary>
        /// Authorizes recovery as a trusted agent.
        /// </summary>
        /// <param name="authorization">the authorization</param>
        /// <returns>the signature</returns>
        public Signature AuthorizeRecovery(Authorization authorization)
        {
            return async.AuthorizeRecovery(authorization).Result;
        }

        /// <summary>
        /// Gets the member id of the default recovery agent.
        /// </summary>
        /// <returns>the member id</returns>
        public string GetDefaultAgent()
        {
            return async.GetDefaultAgent().Result;
        }

        /// <summary>
        /// Verifies a given alias.
        /// </summary>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the verification code</param>
        /// <returns>a task</returns>
        public void VerifyAlias(string verificationId, string code)
        {
            async.VerifyAlias(verificationId, code).Wait();
        }

        /// <summary>
        /// Removes an alias for the member.
        /// </summary>
        /// <param name="alias">the alias to remove</param>
        /// <returns>a task</returns>
        public void RemoveAlias(Alias alias)
        {
            async.RemoveAlias(alias).Wait();
        }

        /// <summary>
        /// Removes an alias for the member.
        /// </summary>
        /// <param name="aliases">the aliases to remove</param>
        /// <returns>a task</returns>
        public void RemoveAliases(IList<Alias> aliases)
        {
            async.RemoveAliases(aliases).Wait();
        }

        /// <summary>
        /// Approves a key owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="key">the keypair to add</param>
        /// <returns>a task</returns>
        public void ApproveKey(KeyPair key)
        {
            async.ApproveKey(key).Wait();
        }

        /// <summary>
        /// Approves a public key owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="key">the key to add</param>
        /// <returns>a task</returns>
        public void ApproveKey(Key key)
        {
            async.ApproveKey(key).Wait();
        }

        /// <summary>
        /// Approves public keys owned by this member. The key is added to the list
        /// of valid keys for the member.
        /// </summary>
        /// <param name="keys">the keys to add</param>
        /// <returns>a task</returns>
        public void ApproveKeys(IList<Key> keys)
        {
            async.ApproveKeys(keys).Wait();
        }

        /// <summary>
        /// Removes a public key owned by this member.
        /// </summary>
        /// <param name="keyId">the key id</param>
        /// <returns>a task</returns>
        public void RemoveKey(string keyId)
        {
            async.RemoveKey(keyId).Wait();
        }

        /// <summary>
        /// Removes some public keys owned by this member.
        /// </summary>
        /// <param name="keyIds">the IDs of the keys to remove</param>
        /// <returns>a task</returns>
        public void RemoveKeys(IList<string> keyIds)
        {
            async.RemoveKeys(keyIds).Wait();
        }

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of linked accounts</returns>
        public IList<AccountSync> GetAccounts()
        {
            return async.GetAccounts()
                .Map(accounts => (IList<AccountSync>) accounts
                    .Select(account => account.Sync()))
                .Result;
        }

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        public AccountSync GetAccount(string accountId)
        {
            return async
                .GetAccount(accountId)
                .Map(account => account.Sync())
                .Result;
        }

        /// <summary>
        /// Gets the default bank account for this member.
        /// </summary>
        /// <returns>the default bank account id</returns>
        public AccountSync GetDefaultAccount()
        {
            return async
                .GetDefaultAccount()
                .Map(account => account.Sync())
                .Result;
        }

        /// <summary>
        /// Looks up an existing token transfer.
        /// </summary>
        /// <param name="transferId">the transfer id</param>
        /// <returns>the transfer record</returns>
        public Transfer GetTransfer(string transferId)
        {
            return async.GetTransfer(transferId).Result;
        }

        /// <summary>
        /// Looks up existing token transfers.
        /// </summary>
        /// <param name="tokenId">nullable token id</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>a paged list of transfers</returns>
        public PagedList<Transfer> GetTransfers(
            string offset,
            int limit,
            string tokenId)
        {
            return async.GetTransfers(tokenId, offset, limit).Result;
        }

        /// <summary>
        /// Creates a new member address.
        /// </summary>
        /// <param name="name">the name of the address</param>
        /// <param name="address">the address</param>
        /// <returns>the created address record</returns>
        public AddressRecord AddAddress(string name, Address address)
        {
            return async.AddAddress(name, address).Result;
        }

        /// <summary>
        /// Looks up an address by id.
        /// </summary>
        /// <param name="addressId">the address id</param>
        /// <returns>the address record</returns>
        public AddressRecord GetAddress(string addressId)
        {
            return async.GetAddress(addressId).Result;
        }

        /// <summary>
        /// Looks up member addresses.
        /// </summary>
        /// <returns>a list of addresses</returns>
        public IList<AddressRecord> GetAddresses()
        {
            return async.GetAddresses().Result;
        }

        /// <summary>
        /// Deletes a member address by its id.
        /// </summary>
        /// <param name="addressId">the address id</param>
        /// <returns>a task</returns>
        public void DeleteAddress(string addressId)
        {
            async.DeleteAddress(addressId).Wait();
        }

        /// <summary>
        /// Replaces auth'd member's public profile.
        /// </summary>
        /// <param name="profile">the protile to set</param>
        /// <returns>the updated profile</returns>
        public Profile SetProfile(Profile profile)
        {
            return async.SetProfile(profile).Result;
        }

        /// <summary>
        /// Gets a member's public profile. Unlike setProfile, you can get another member's profile.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <returns>the profile</returns>
        public Profile GetProfile(string memberId)
        {
            return async.GetProfile(memberId).Result;
        }

        /// <summary>
        /// Replaces auth'd member's public profile picture.
        /// </summary>
        /// <param name="type">MIME type of the picture</param>
        /// <param name="data">the image data</param>
        /// <returns>a task</returns>
        public void SetProfilePicture(string type, byte[] data)
        {
            async.SetProfilePicture(type, data).Wait();
        }

        /// <summary>
        /// Gets a member's public profile picture. Unlike set, you can get another member's picture.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <param name="size">the desired size category (small, medium, large, original)</param>
        /// <returns>a blob with picture; empty if the member has no picture</returns>
        public Blob GetProfilePicture(string memberId, ProfilePictureSize size)
        {
            return async.GetProfilePicture(memberId, size).Result;
        }

        /// <summary>
        /// Stores a transfer token request.
        /// </summary>
        /// <param name="tokenRequest">the token request</param>
        /// <returns>an id to reference the token request</returns>
        public string StoreTokenRequest(TokenRequest tokenRequest)
        {
            return async.StoreTokenRequest(tokenRequest).Result;
        }

        /// <summary>
        /// Creates a new transfer token builder.
        /// </summary>
        /// <param name="amount">the transfer amount</param>
        /// <param name="currency">the currency code, e.g. "USD"</param>
        /// <returns>the transfer token builder</returns>
        public TransferTokenBuilder CreateTransferToken(double amount, string currency)
        {
            return new TransferTokenBuilder(async, amount, currency);
        }

        /// <summary>
        /// Creates an access token.
        /// </summary>
        /// <param name="payload">the access token payload</param>
        /// <returns>the access token</returns>
        public Token CreateAccessToken(TokenPayload payload)
        {
            return async.CreateAccessToken(payload).Result;
        }

        /// <summary>
        /// Creates an access token with a token request id.
        /// </summary>
        /// <param name="payload">the access token payload</param>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the access token</returns>
        public Token CreateAccessToken(TokenPayload payload, string tokenRequestId)
        {
            return async.CreateAccessToken(payload, tokenRequestId).Result;
        }

        /// <summary>
        /// Looks up an existing token.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <returns>the token</returns>
        public Token GetToken(string tokenId)
        {
            return async.GetToken(tokenId).Result;
        }

        /// <summary>
        /// Looks up exsiting transfer tokens.
        /// </summary>
        /// <param name="limit">the max number of records to return</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <returns>a paged list of transfer tokens</returns>
        public PagedList<Token> GetTransferTokens(string offset, int limit)
        {
            return async.GetTransferTokens(limit, offset).Result;
        }

        /// <summary>
        /// Looks up existing access tokens.
        /// </summary>
        /// <param name="limit">the max number of records to return</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <returns>a paged list of access tokens</returns>
        public PagedList<Token> GetAccessTokens(string offset, int limit)
        {
            return async.GetAccessTokens(limit, offset).Result;
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
        public TokenOperationResult EndorseToken(Token token, Level keyLevel)
        {
            return async.EndorseToken(token, keyLevel).Result;
        }

        /// <summary>
        /// Cancels a token.
        /// </summary>
        /// <param name="token">the token to cancel</param>
        /// <returns>the result of the cancel operation</returns>
        public TokenOperationResult CancelToken(Token token)
        {
            return async.CancelToken(token).Result;
        }

        /// <summary>
        /// Cancels the existing access token and creates a replacement for it.
        /// </summary>
        /// <param name="tokenToCancel">the token to cancel</param>
        /// <param name="tokenToCreate">the payload to create new token with</param>
        /// <returns>the result of the replacement opration</returns>
        public TokenOperationResult ReplaceAccessToken(
            Token tokenToCancel,
            TokenPayload tokenToCreate)
        {
            return async.ReplaceAccessToken(tokenToCancel, tokenToCreate).Result;
        }

        /// <summary>
        /// Cancels the existing token, creates a replacement and endorses it.
        /// Supported only for access tokens.
        /// </summary>
        /// <param name="tokenToCancel">the token to cancel</param>
        /// <param name="tokenToCreate">the payload to create new token with</param>
        /// <returns>the result of the replacement opration</returns>
        public TokenOperationResult ReplaceAndEndorseAccessToken(
            Token tokenToCancel,
            TokenPayload tokenToCreate)
        {
            return async.ReplaceAndEndorseAccessToken(tokenToCancel, tokenToCreate).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemToken(Token token)
        {
            return async.RedeemToken(token).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemToken(Token token, string refId)
        {
            return async.RedeemToken(token, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemToken(Token token, TransferEndpoint destination)
        {
            return async.RedeemToken(token, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemToken(Token token, TransferEndpoint destination, string refId)
        {
            return async.RedeemToken(token, destination, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description)
        {
            return async.RedeemToken(token, amount, currency, description, null, null).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemToken(
            Token token,
            double? amount,
            string currency,
            TransferEndpoint destination)
        {
            return async.RedeemToken(token, amount, currency, null, destination, null).Result;
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
        public Transfer RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination)
        {
            return async.RedeemToken(token, amount, currency, description, destination, null).Result;
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
        public Transfer RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination,
            string refId)
        {
            return async.RedeemToken(token, amount, currency, description, destination, refId).Result;
        }

        /// <summary>
        /// Looks up an existing transaction for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="transactionId">the transaction ID</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the transaction</returns>
        public Transaction GetTransaction(
            string accountId,
            string transactionId,
            Level keyLevel)
        {
            return async.GetTransaction(accountId, transactionId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up transactions for a given account.
        /// </summary>
        /// <param name="accountId">the account ID</param>
        /// <param name="limit">max number of records to return</param>
        /// <param name="keyLevel">the key level</param>
        /// <param name="offset">the nullable offset to start at</param>
        /// <returns>a paged list of transactions</returns>
        public PagedList<Transaction> GetTransactions(
            string accountId,
            string offset,
            int limit,
            Level keyLevel)
        {
            return async.GetTransactions(accountId, limit, keyLevel, offset).Result;
        }

        /// <summary>
        /// Looks up account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        public Balance GetBalance(string accountId, Level keyLevel)
        {
            return async.GetBalance(accountId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up available account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        public Money GetAvailableBalance(string accountId, Level keyLevel)
        {
            return async.GetAvailableBalance(accountId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up current account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        public Money GetCurrentBalance(string accountId, Level keyLevel)
        {
            return async.GetCurrentBalance(accountId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up balances for a list of accounts.
        /// </summary>
        /// <param name="accountIds">the list of accounts</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>a list of balances</returns>
        public IList<Balance> GetBalances(
            IList<string> accountIds,
            Level keyLevel)
        {
            return async.GetBalances(accountIds, keyLevel).Result;
        }

        /// <summary>
        /// Returns linking information for a specified bank id.
        /// </summary>
        /// <param name="bankId">the bank id</param>
        /// <returns>the bank linking information</returns>
        public BankInfo GetBankInfo(string bankId)
        {
            return async.GetBankInfo(bankId).Result;
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
        public Attachment CreateBlob(
            string ownerId,
            string type,
            string name,
            byte[] data,
            AccessMode accessMode)
        {
            return async.CreateBlob(ownerId, type, name, data, accessMode).Result;
        }

        /// <summary>
        /// Creates and uploads a blob.
        /// </summary>
        /// <param name="ownerId">the id of the owner of the blob</param>
        /// <param name="type">the MIME type of the file</param>
        /// <param name="name">the name of the file</param>
        /// <param name="data">the file data</param>
        /// <returns>an attachment</returns>
        public Attachment CreateBlob(
            string ownerId,
            string type,
            string name,
            byte[] data)
        {
            return async
                .CreateBlob(ownerId, type, name, data)
                .Result;
        }

        /// <summary>
        /// Retrieves a blob from the server.
        /// </summary>
        /// <param name="blobId">the blob id</param>
        /// <returns>the blob</returns>
        public Blob GetBlob(string blobId)
        {
            return async.GetBlob(blobId).Result;
        }

        /// <summary>
        /// Retrieves a blob that is attached to a transfer token.
        /// </summary>
        /// <param name="tokenId">the token id</param>
        /// <param name="blobId">the blob id</param>
        /// <returns>the blob</returns>
        public Blob GetTokenBlob(string tokenId, string blobId)
        {
            return async.GetTokenBlob(tokenId, blobId).Result;
        }

        /// <summary>
        /// Applies SCA for the given a list of accounts.
        /// </summary>
        /// <param name="accountIds">the list of accounts</param>
        /// <returns>a task</returns>
        public void ApplySca(IList<string> accountIds)
        {
            async.ApplySca(accountIds).Wait();
        }

        /// <summary>
        /// Signs a token request state payload.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <param name="tokenId">the token id</param>
        /// <param name="state">the state</param>
        /// <returns>the signature</returns>
        public Signature SignTokenRequestState(
            string tokenRequestId,
            string tokenId,
            string state)
        {
            return async.SignTokenRequestState(tokenRequestId, tokenId, state).Result;
        }

        /// <summary>
        /// Gets all paired devices.
        /// </summary>
        /// <returns>a list of devices</returns>
        public IList<Device> GetPairedDevices()
        {
            return async.GetPairedDevices().Result;
        }

        /// <summary>
        /// Verifies an affiliated TPP.
        /// </summary>
        /// <param name="memberId">member ID of the TPP verify</param>
        public void VerifyAffiliate(string memberId)
        {
            async.VerifyAffiliate(memberId).Wait();
        }

        /// <summary>
        /// Resolves transfer destinations for the given account.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a list of transfer endpoints</returns>
        public IList<TransferEndpoint> ResolveTransferDestination(string accountId)
        {
            return async.ResolveTransferDestination(accountId).Result;
        }
        
        /// <summary>
        /// Adds a trusted beneficiary for whom the SCA will be skipped.
        /// </summary>
        /// <param name="memberId">the member id of the beneficiary</param>
        public void AddTrustedBeneficiary(string memberId)
        {
            async.AddTrustedBeneficiary(memberId).Wait();
        }

        /// <summary>
        /// Removes a trusted beneficiary. 
        /// </summary>
        /// <param name="memberId">the member id of the beneficiary</param>
        public void RemoveTrustedBeneficiary(string memberId)
        {
            async.RemoveTrustedBeneficiary(memberId).Wait();
        }

        /// <summary>
        /// Gets a list of all trusted beneficiaries.
        /// </summary>
        /// <returns>the list</returns>
        public IList<TrustedBeneficiary> GetTrustedBeneficiaries()
        {
            return async.GetTrustedBeneficiaries().Result;
        }
    }
}
