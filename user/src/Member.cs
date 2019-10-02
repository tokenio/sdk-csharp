using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using log4net;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.SubmissionProtos;
using Tokenio.Proto.Common.SubscriberProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.User.Browser;
using Tokenio.User.Rpc;
using Tokenio.User.Utils;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.NotificationProtos.Notification.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using Notification = Tokenio.Proto.Common.NotificationProtos.Notification;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;

namespace Tokenio.User
{
    /// <summary>
    /// Represents a Member in the Token system. Each member has an active secret
    /// and public key pair that is used to perform authentication.
    /// </summary>
    public class Member : Tokenio.Member
    {
        private static readonly ILog logger = LogManager
                .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Client client;
        private readonly IBrowserFactory browserFactory;

        /// <summary>
        /// Creates an instance of {@link Member}.
        /// </summary>
        /// <param name="memberId">member ID</param>
        /// <param name="client">RPC client used to perform operations against the server</param>
        /// <param name="tokenCluster">Token cluster, e.g. sandbox, production</param>
        /// <param name="partnerId">member ID of partner</param>
        /// <param name="realmId">realm ID</param>
        /// <param name="browserFactory">browser factory for displaying UI for linking</param>
        public Member(
                string memberId,
                Client client,
                TokenCluster tokenCluster,
                string partnerId,
                string realmId,
                IBrowserFactory browserFactory) : base(memberId, client, tokenCluster, partnerId, realmId)
        {
            this.client = client;
            this.browserFactory = browserFactory;
        }

        /// <summary>
        /// Links a funding bank account to Token and returns it to the caller.
        /// </summary>
        /// <returns>list of accounts</returns>
        public Task<IList<Account>> GetAccounts()
        {
            return GetAccountsImpl()
                    .Map(accounts =>
                            (IList<Account>)accounts.Select(account =>
                                   new Account(account, client, this))
                    .ToList());
        }

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>list of linked accounts</returns>
        public IList<Account> GetAccountsBlocking()
        {
            return GetAccounts().Result;
        }

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">account id</param>
        /// <returns>looked up account</returns>
        public Task<Account> GetAccount(string accountId)
        {
            return GetAccountImpl(accountId)
                    .Map(account =>
                            new Account(account, client, this));
        }

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">account id</param>
        /// <returns>looked up account</returns>
        public Account GetAccountBlocking(string accountId)
        {
            return GetAccount(accountId).Result;
        }

        /// <summary>
        /// Set the default bank account for this member.
        /// </summary>
        /// <param name="accountId">ID of default account to set</param>
        /// <returns>task</returns>
        public Task SetDefaultAccount(string accountId)
        {
            return client.SetDefaultAccount(accountId);
        }

        /// <summary>
        /// Set the default bank account for this member.
        /// </summary>
        /// <param name="accountId">ID of default account to set</param>
        public void SetDefaultAccountBlocking(string accountId)
        {
            client.SetDefaultAccount(accountId).Wait();
        }

        /// <summary>
        /// Get the default bank account for this member.
        /// </summary>
        /// <returns>task account</returns>
        public Task<Account> GetDefaultAccount()
        {
            return client
                    .GetDefaultAccount(MemberId())
                    .Map(account =>
                            new Account(this, account, client));
        }

        /// <summary>
        /// Get the default bank account.
        /// </summary>
        /// <returns>the default bank account</returns>
        public Account GetDefaultAccountBlocking()
        {
            return GetDefaultAccount().Result;
        }

        /// <summary>
        /// Looks up an existing token transfer.
        /// </summary>
        /// <param name="transferId">ID of the transfer record</param>
        /// <returns>transfer record</returns>
        public Task<Transfer> GetTransfer(string transferId)
        {
            return client.GetTransfer(transferId);
        }

        /// <summary>
        /// Looks up an existing token transfer.
        /// </summary>
        /// <param name="transferId">ID of the transfer record</param>
        /// <returns>transfer record</returns>
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
        /// <param name="tokenId">optional token id to restrict the search</param>
        /// <param name="offset">optional offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>transfer record</returns>
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
        /// <param name="tokenId">optional token id to restrict the search</param>
        /// <param name="offset">optional offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>transfer record</returns>
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
        /// Prepares a transfer token, returning the resolved token payload and policy.
        /// </summary>
        /// <param name="transferTokenBuilder">transfer token builder</param>
        /// <returns>resolved token payload and policy</returns>
		public Task<PrepareTokenResult> PrepareTransferToken(
                TransferTokenBuilder transferTokenBuilder)
        {
            transferTokenBuilder.From(MemberId());
            return client.PrepareToken(transferTokenBuilder.BuildPayload());
        }

        /// <summary>
        /// Prepares a transfer token, returning the resolved token payload and policy.
        /// </summary>
        /// <param name="transferTokenBuilder">transfer token builder</param>
        /// <returns>resolved token payload and policy</returns>
        public PrepareTokenResult PrepareTransferTokenBlocking(
                TransferTokenBuilder transferTokenBuilder)
        {
            return PrepareTransferToken(transferTokenBuilder).Result;
        }

        /// <summary>
        /// Prepares an access token, returning the resolved token payload and policy.
        /// </summary>
        /// <param name="accessTokenBuilder">access token builder</param>
        /// <returns>resolved token payload and policy</returns>
        public Task<PrepareTokenResult> PrepareAccessToken(
                AccessTokenBuilder accessTokenBuilder)
        {
            accessTokenBuilder.From(MemberId());
            return client.PrepareToken(accessTokenBuilder.Build());
        }

        /// <summary>
        /// Prepares a standing order token, returning the resolved token payload
        /// and policy.
        /// 
        /// </summary>
        /// <param name="builder">standing order token builder</param>
        /// <returns>resolved token payload and policy</returns>
        public Task<PrepareTokenResult> PrepareStandingOrderToken(
                StandingOrderTokenBuilder builder)
        {
            return client.PrepareToken(builder.BuildPayload());
        }

        /// <summary>
        /// Prepares a standing order token, returning the resolved token payload
        /// and policy.
        /// 
        /// </summary>
        /// <param name="builder">standing order token builder</param>
        /// <returns>resolved token payload and policy</returns>
        public PrepareTokenResult PrepareStandingOrderTokenBlocking(
                StandingOrderTokenBuilder builder)
        {
            return PrepareStandingOrderToken(builder).Result;
        }

        /// <summary>
        /// Prepares an access token, returning the resolved token payload and policy.
        /// </summary>
        /// <param name="accessTokenBuilder">access token builder</param>
        /// <returns>resolved token payload and policy</returns>
        public PrepareTokenResult PrepareAccessTokenBlocking(AccessTokenBuilder accessTokenBuilder)
        {
            return PrepareAccessToken(accessTokenBuilder).Result;
        }

        /// <summary>
        /// Creates a token directly from a resolved token payload and list of token signatures.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <param name="signatures">list of signatures</param>
        /// <returns>token returned by server</returns>
        public Task<Token> CreateToken(TokenPayload payload, IList<Signature> signatures)
        {
            return CreateToken(payload, signatures, null);
        }

        /// <summary>
        /// Creates a token directly from a resolved token payload and list of token signatures.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <param name="signatures">list of signatures</param>
        /// <param name="tokenRequestId">token request ID</param>
        /// <returns>token returned by server</returns>
        public Task<Token> CreateToken(
                TokenPayload payload,
                IList<Signature> signatures,
                string tokenRequestId)
        {
            return client.CreateToken(payload, tokenRequestId, signatures);
        }

        /// <summary>
        /// Creates a token with the member's own signature.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>token returned by server</returns>
        public Task<Token> CreateToken(TokenPayload payload, Level keyLevel)
        {
            return CreateToken(payload, null, keyLevel);
        }

        /// <summary>
        /// Creates a token with the member's own signature.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <param name="tokenRequestId">token request ID</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>token returned by the server</returns>
        public Task<Token> CreateToken(
                TokenPayload payload,
                string tokenRequestId,
                Level keyLevel)
        {
            IList<Signature> signatures = new List<Signature> {
                SignTokenPayload(payload, keyLevel)
            };
            return client.CreateToken(
                    payload,
                    tokenRequestId,
                    signatures);
        }

        /// <summary>
        /// Creates a token directly from a resolved token payload and list of token signatures.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <param name="signatures">list of signatures</param>
        /// <returns>token returned by server</returns>
        public Token CreateTokenBlocking(TokenPayload payload, IList<Signature> signatures)
        {
            return CreateToken(payload, signatures).Result;
        }

        /// <summary>
        /// Creates a token directly from a resolved token payload and list of token signatures.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <param name="signatures">list of signatures</param>
        /// <param name="tokenRequestId">token request ID</param>
        /// <returns>token returned by server</returns>
        public Token CreateTokenBlocking(
                TokenPayload payload,
                IList<Signature> signatures,
                string tokenRequestId)
        {
            return CreateToken(payload, signatures, tokenRequestId).Result;
        }

        /// <summary>
        /// Creates a token with the member's own signature.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>token returned by server</returns>
        public Token CreateTokenBlocking(TokenPayload payload, Level keyLevel)
        {
            return CreateToken(payload, keyLevel).Result;
        }

        /// <summary>
        /// Creates a token with the member's own signature.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <param name="tokenRequestId">token request ID</param>
        /// <param name="keyLevel">key level</param>
        /// <returns>token returned by the server</returns>
        public Token CreateTokenBlocking(
                TokenPayload payload,
                string tokenRequestId,
                Level keyLevel)
        {
            return CreateToken(payload, tokenRequestId, keyLevel).Result;
        }

        /// <summary>
        /// Creates a new transfer token builder
        /// </summary>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">currency code, e.g. "USD"</param>
        /// <returns>transfer token builder</returns>
        public TransferTokenBuilder CreateTransferTokenBuilder(double amount, string currency)
        {
            return new TransferTokenBuilder(this, amount, currency);
        }

        /// <summary>
        /// Creates a new transfer token builder from a token request.
        /// </summary>
        /// <param name="tokenRequest">token request</param>
        /// <returns>transfer token builder</returns>
        public TransferTokenBuilder CreateTransferTokenBuilder(TokenRequest tokenRequest)
        {
            return new TransferTokenBuilder(this, tokenRequest);
        }

        /// <summary>
        /// Creates a new transfer token builder from a token payload.
        /// </summary>
        /// <param name="payload">token payload</param>
        /// <returns>transfer token builder</returns>
        public TransferTokenBuilder CreateTransferTokenBuilder(TokenPayload payload)
        {
            return new TransferTokenBuilder(this, payload);
        }
        
        /// <summary>
        /// Creates a new standing order token builder.
        /// </summary>
        /// <param name="amount">individual transfer amount</param>
        /// <param name="currency">currency code, e.g. "USD"</param>
        /// <param name="frequency">ISO 20022 code for the frequency of the standing order:
        ///              DAIL, WEEK, TOWK, MNTH, TOMN, QUTR, SEMI, YEAR</param>
        /// <param name="startDate">start date of the standing order</param>
        /// <param name="endDate">optional end date of the standing order</param>
        /// <returns>standing order token builder</returns>
        public StandingOrderTokenBuilder CreateStandingOrderTokenBuilder(
                double amount,
                string currency,
                string frequency,
                DateTime startDate,
                DateTime? endDate = null)
        {
            return new StandingOrderTokenBuilder(
                    this,
                    amount,
                    currency,
                    frequency,
                    startDate,
                    endDate);
        }

        /// <summary>
        /// Creates a new standing order token builder from a token request.
        /// </summary>
        /// <param name="tokenRequest">token request</param>
        /// <returns>transfer token builder</returns>
        public StandingOrderTokenBuilder CreateStandingOrderTokenBuilder(TokenRequest tokenRequest)
        {
            return new StandingOrderTokenBuilder(tokenRequest);
        }

        /// <summary>
        /// Creates an access token built from a given {@link AccessTokenBuilder}.
        /// </summary>
        /// <param name="accessTokenBuilder">an {@link AccessTokenBuilder} to create access token from</param>
        /// <returns>the access token created</returns>
        public Task<Token> CreateAccessToken(AccessTokenBuilder accessTokenBuilder)
        {
            return client.CreateAccessToken(
                    accessTokenBuilder.From(MemberId()).Build(),
                    accessTokenBuilder.tokenRequestId);
        }

        /// <summary>
        /// Creates an access token built from a given {@link AccessTokenBuilder}.
        /// </summary>
        /// <param name="accessTokenBuilder">an {@link AccessTokenBuilder} to create access token from</param>
        /// <returns>the access token created</returns>
        public Token CreateAccessTokenBlocking(AccessTokenBuilder accessTokenBuilder)
        {
            return CreateAccessToken(accessTokenBuilder).Result;
        }

        /// <summary>
        /// Endorses the token by signing it. The signature is persisted along
        /// with the token.
        ///
        /// <p>If the key's level is too low, the result's status is MORE_SIGNATURES_NEEDED
        /// and the system pushes a notification to the member prompting them to use a
        /// higher-privilege key.</p>
        /// </summary>
        /// <param name="token">token to endorse</param>
        /// <param name="keyLevel">key level to be used to endorse the token</param>
        /// <returns>result of endorse token</returns>
        public Task<TokenOperationResult> EndorseToken(Token token, Level keyLevel)
        {
            return client.EndorseToken(token, keyLevel);
        }

        /// <summary>
        /// Endorses the token by signing it. The signature is persisted along
        /// with the token.
        ///
        /// <p>If the key's level is too low, the result's status is MORE_SIGNATURES_NEEDED
        /// and the system pushes a notification to the member prompting them to use a
        /// higher-privilege key.</p>
        /// </summary>
        /// <param name="token">token to endorse</param>
        /// <param name="keyLevel">key level to be used to endorse the token</param>
        /// <returns>result of endorse token</returns>
        public TokenOperationResult EndorseTokenBlocking(Token token, Level keyLevel)
        {
            return EndorseToken(token, keyLevel).Result;
        }

        /// <summary>
        /// Cancels the token by signing it. The signature is persisted along
        /// with the token.
        /// </summary>
        /// <param name="token">token to cancel</param>
        /// <returns>result of cancel token</returns>
        public Task<TokenOperationResult> CancelToken(Token token)
        {
            return client.CancelToken(token);
        }

        /// <summary>
        /// Cancels the token by signing it. The signature is persisted along
        /// with the token.
        /// </summary>
        /// <param name="token">token to cancel</param>
        /// <returns>result of cancel token</returns>
        public TokenOperationResult CancelTokenBlocking(Token token)
        {
            return CancelToken(token).Result;
        }

        /// <summary>
        /// Cancels the existing access token and creates a replacement for it.
        /// </summary>
        /// <param name="tokenToCancel">old token to cancel</param>
        /// <param name="tokenToCreate">an {@link AccessTokenBuilder} to create new token from</param>
        /// <returns>result of the replacement operation</returns>
        public Task<TokenOperationResult> ReplaceAccessToken(
                Token tokenToCancel,
                AccessTokenBuilder tokenToCreate)
        {
            return client.ReplaceToken(
                    tokenToCancel,
                    tokenToCreate.From(MemberId())
                    .Build());
        }

        /// <summary>
        /// Cancels the existing access token, creates a replacement and optionally endorses it.
        /// </summary>
        /// <param name="tokenToCancel">old token to cancel</param>
        /// <param name="tokenToCreate">an {@link AccessTokenBuilder} to create new token from</param>
        /// <returns>result of the replacement operation</returns>
        public TokenOperationResult ReplaceAccessTokenBlocking(
                Token tokenToCancel,
                AccessTokenBuilder tokenToCreate)
        {
            return ReplaceAccessToken(tokenToCancel, tokenToCreate)
                    .Result;
        }

        /// <summary>
        /// Replaces the member's receipt contact.
        /// </summary>
        /// <param name="contact">receipt contact to set</param>
        /// <returns>task that indicates whether the operation finished or had an error</returns>
        public Task SetReceiptContact(ReceiptContact contact)
        {
            return client.SetReceiptContact(contact);
        }

        /// <summary>
        /// Replaces the member's receipt contact.
        /// </summary>
        /// <param name="receiptContact">receipt contact to set</param>
        public void SetReceiptContactBlocking(ReceiptContact receiptContact)
        {
            SetReceiptContact(receiptContact).Wait();
        }

        /// <summary>
        /// Gets the member's receipt email address.
        /// </summary>
        /// <returns>receipt contact</returns>
        public Task<ReceiptContact> GetReceiptContact()
        {
            return client.GetReceiptContact();
        }

        /// <summary>
        /// Gets the member's receipt contact.
        /// </summary>
        /// <returns>receipt contact</returns>
        public ReceiptContact GetReceiptContactBlocking()
        {
            return GetReceiptContact().Result;
        }

        /// <summary>
        /// Sets the app's callback url.
        /// </summary>
        /// <param name="appCallbackUrl">the app callback url to set</param>
        /// <returns>task</returns>
        public Task SetAppCallbackUrl(string appCallbackUrl)
        {
            return client.SetAppCallbackUrl(appCallbackUrl);
        }

        /// <summary>
        /// Sets the app's callback url.
        /// </summary>
        /// <param name="appCallbackUrl">the app callback url to set</param>
        public void SetAppCallbackUrlBlocking(string appCallbackUrl)
        {
            client.SetAppCallbackUrl(appCallbackUrl).Wait();
        }

        /// <summary>
        /// Looks up a existing access token where the calling member is the grantor and given member is
        /// the grantee.
        /// </summary>
        /// <param name="toMemberId">beneficiary of the active access token</param>
        /// <returns>access token returned by the server</returns>
        public Task<Token> GetActiveAccessToken(string toMemberId)
        {
            return client.GetActiveAccessToken(toMemberId);
        }

        /// <summary>
        /// Looks up a existing access token where the calling member is the grantor and given member is
        /// the grantee.
        /// </summary>
        /// <param name="toMemberId">beneficiary of the active access token</param>
        /// <returns>access token returned by the server</returns>
        public Token GetActiveAccessTokenBlocking(string toMemberId)
        {
            return GetActiveAccessToken(toMemberId).Result;
        }

        /// <summary>
        /// Looks up transfer tokens owned by the member.
        /// </summary>
        /// <param name="offset">optional offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>transfer tokens owned by the member</returns>
        public Task<PagedList<Token>> GetTransferTokens(
                string offset,
                int limit)
        {
            return client.GetTokens(TokenType.Transfer, limit, offset);

        }

        /// <summary>
        /// Looks up transfer tokens owned by the member.
        /// </summary>
        /// <param name="offset">optional offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>transfer tokens owned by the member</returns>
        public PagedList<Token> GetTransferTokensBlocking(string offset, int limit)
        {
            return GetTransferTokens(offset, limit).Result;
        }

        /// <summary>
        /// Looks up access tokens owned by the member.
        /// </summary>
        /// <param name="offset">optional offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>access tokens owned by the member</returns>
        public Task<PagedList<Token>> GetAccessTokens(
                string offset,
                int limit)
        {
            return client.GetTokens(TokenType.Access, limit, offset);
        }

        /// <summary>
        /// Looks up access tokens owned by the member.
        /// </summary>
        /// <param name="offset">optional offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>access tokens owned by the member</returns>
        public PagedList<Token> GetAccessTokensBlocking(string offset, int limit)
        {
            return GetAccessTokens(offset, limit).Result;
        }

        /// <summary>
        /// Looks up a existing token.
        /// </summary>
        /// <param name="tokenId">token id</param>
        /// <returns>token returned by the server</returns>
        public Task<Token> GetToken(string tokenId)
        {
            return client.GetToken(tokenId);
        }

        /// <summary>
        /// Looks up a existing token.
        /// </summary>
        /// <param name="tokenId">token id</param>
        /// <returns>token returned by the server</returns>
        public Token GetTokenBlocking(string tokenId)
        {
            return GetToken(tokenId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">transfer token to redeem</param>
        /// <returns>transfer record</returns>
        public Task<Transfer> RedeemToken(Token token)
        {
            return RedeemTokenInternal(token, null, null, null, null, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">transfer token to redeem</param>
        /// <param name="refId">transfer reference id</param>
        /// <returns>transfer record</returns>
        public Task<Transfer> RedeemToken(Token token, string refId)
        {
            return RedeemTokenInternal(token, null, null, null, null, refId);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">transfer token to redeem</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <returns>transfer record</returns>
        public Task<Transfer> RedeemToken(Token token, TransferDestination destination)
        {
            return RedeemToken(token, null, null, null, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">transfer token to redeem</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <param name="refId">transfer reference id</param>
        /// <returns>transfer record</returns>
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
        /// <param name="token">transfer token to redeem</param>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">transfer currency code, e.g. "EUR"</param>
        /// <param name="description">transfer description</param>
        /// <returns>transfer record</returns>
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
        /// <param name="token">transfer token to redeem</param>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">transfer currency code, e.g. "EUR"</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <returns>transfer record</returns>
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
        /// <param name="token">transfer token to redeem</param>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">transfer currency code, e.g. "EUR"</param>
        /// <param name="description">transfer description</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <returns>transfer record</returns>
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
        /// <param name="token">transfer token to redeem</param>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">transfer currency code, e.g. "EUR"</param>
        /// <param name="description">transfer description</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">transfer reference id</param>
        /// <returns>transfer record</returns>
        public Task<Transfer> RedeemToken(
                Token token,
                double? amount,
                string currency,
                string description,
                TransferDestination destination,
                string refId)
        {
            if (token.Payload.Transfer == null)
            {
                throw new ArgumentException("Expected transfer token; found "
                        + token.Payload.BodyCase);
            }
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
            else
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Util.Nonce();
            }
            return client.CreateTransfer(payload);
        }

        // Remove when deprecated TransferEndpoint methods are removed.
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
                Money money = new Money
                {
                    Value = Util.DoubleToString(amount.Value)
                };
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
        /// <param name="token">transfer token to redeem</param>
        /// <returns>transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token)
        {
            return RedeemToken(token).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">transfer token to redeem</param>
        /// <param name="refId">transfer reference id</param>
        /// <returns>transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token, string refId)
        {
            return RedeemToken(token, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">transfer token to redeem</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <returns>transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token, TransferDestination destination)
        {
            return RedeemToken(token, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">transfer token to redeem</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <param name="refId">transfer reference id</param>
        /// <returns>transfer record</returns>
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
        /// <param name="token">transfer token to redeem</param>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">transfer currency code, e.g. "EUR"</param>
        /// <param name="description">transfer description</param>
        /// <returns>transfer record</returns>
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
        /// <param name="token">transfer token to redeem</param>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">transfer currency code, e.g. "EUR"</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <returns>transfer record</returns>
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
        /// <param name="token">transfer token to redeem</param>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">transfer currency code, e.g. "EUR"</param>
        /// <param name="description">transfer description</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <returns>transfer record</returns>
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
        /// <param name="token">transfer token to redeem</param>
        /// <param name="amount">transfer amount</param>
        /// <param name="currency">transfer currency code, e.g. "EUR"</param>
        /// <param name="description">transfer description</param>
        /// <param name="destination">transfer instruction destination</param>
        /// <param name="refId">transfer reference id</param>
        /// <returns>transfer record</returns>
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
        /// Links a funding bank accounts to Token and returns it to the caller.
        /// </summary>
        /// <param name="authorization">an authorization to accounts, from the bank</param>
        /// <returns>list of linked accounts</returns>
        public Task<IList<Account>> LinkAccounts(
                BankAuthorization authorization)
        {
            return ToAccountList(client.LinkAccounts(authorization));
        }

        /// <summary>
        /// Links a funding bank accounts to Token and returns them to the caller.
        /// </summary>
        /// <param name="bankId">bank id</param>
        /// <param name="accessToken">OAuth access token</param>
        /// <returns>list of linked accounts</returns>
        public Task<IList<Account>> LinkAccounts(string bankId, string accessToken)
        {
            var authorization = new OauthBankAuthorization
            {
                BankId = bankId,
                AccessToken = accessToken
            };
            return ToAccountList(client.LinkAccounts(authorization));
        }

        /// <summary>
        /// Links a funding bank accounts to Token and returns it to the caller.
        /// </summary>
        /// <param name="authorization">an authorization to accounts, from the bank</param>
        /// <returns>list of linked accounts</returns>
        public IList<Account> LinkAccountsBlocking(BankAuthorization authorization)
        {
            return LinkAccounts(authorization).Result;
        }

        /// <summary>
        /// Links a funding bank accounts to Token and returns them to the caller.
        /// </summary>
        /// <param name="bankId">bank id</param>
        /// <param name="accessToken">OAuth access token</param>
        /// <returns>list of linked accounts</returns>
        public IList<Account> LinkAccountsBlocking(string bankId, string accessToken)
        {
            return LinkAccounts(bankId, accessToken).Result;
        }

        /// <summary>
        /// Unlinks bank accounts previously linked via LinkAccounts call.
        /// </summary>
        /// <param name="accountIds">account ids to unlink</param>
        /// <returns>task</returns>
        public Task UnlinkAccounts(IList<string> accountIds)
        {
            return client.UnlinkAccounts(accountIds);
        }

        /// <summary>
        /// Unlinks bank accounts previously linked via LinkAccounts call.
        /// </summary>
        /// <param name="accountIds">list of account ids to unlink</param>
        public void UnlinkAccountsBlocking(IList<string> accountIds)
        {
            UnlinkAccounts(accountIds).Wait();
        }

        /// <summary>
        /// Removes all public keys that do not have a corresponding private key stored on
        /// the current device from the member.
        /// </summary>
        /// <returns>task that indicates whether the operation finished or had an error</returns>
        public Task RemoveNonStoredKeys()
        {
            IList<Key> storedKeys = client.GetCryptoEngine().GetPublicKeys();
            return client.GetMember(MemberId())
                    .FlatMap(member =>
                    {
                        IList<string> toRemoveIds = new List<string>();
                        foreach (Key key in member.Keys.ToList())
                        {
                            if (!storedKeys.Contains(key))
                            {
                                toRemoveIds.Add(key.Id);
                            }
                        }
                        return (Task<ProtoMember>)RemoveKeys(toRemoveIds);
                    });
        }

        /// <summary>
        /// Removes all public keys that do not have a corresponding private key stored on
        /// the current device from the member.
        /// </summary>
        public void RemoveNonStoredKeysBlocking()
        {
            RemoveNonStoredKeys().Wait();
        }

        /// <summary>
        /// Replaces the authenticated member's public profile.
        /// </summary>
        /// <param name="profile">protile to set</param>
        /// <returns>updated profile</returns>
        public Task<Profile> SetProfile(Profile profile)
        {
            return client.SetProfile(profile);
        }

        /// <summary>
        /// Replaces the authenticated member's public profile.
        /// </summary>
        /// <param name="profile">protile to set</param>
        /// <returns>updated profile</returns>
        public Profile SetProfileBlocking(Profile profile)
        {
            return SetProfile(profile).Result;
        }

        /// <summary>
        /// Replaces authenticated member's public profile picture.
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
        /// Replaces authenticated member's public profile picture.
        /// </summary>
        /// <param name="type">MIME type of picture</param>
        /// <param name="data">image data</param>
        public void SetProfilePictureBlocking(string type, byte[] data)
        {
            SetProfilePicture(type, data).Wait();
        }

        /// <summary>
        /// Gets the notifications.
        /// </summary>
        /// <param name="offset">offset to start</param>
        /// <param name="limit">how many notifications to get</param>
        /// <returns>list of notifications</returns>
        public Task<PagedList<Notification>> GetNotifications(
                int limit,
                string offset = null)
        {
            return client.GetNotifications(limit, offset);
        }

        /// <summary>
        /// Gets the notifications.
        /// </summary>
        /// <param name="offset">offset to start</param>
        /// <param name="limit">how many notifications to get</param>
        /// <returns>list of notifications</returns>
        public PagedList<Notification> GetNotificationsBlocking(
                int limit,
                string offset = null)
        {
            return GetNotifications(limit, offset).Result;
        }

        /// <summary>
        /// Gets a notification by id.
        /// </summary>
        /// <param name="notificationId">Id of the notification</param>
        /// <returns>notification</returns>
        public Task<Notification> GetNotification(string notificationId)
        {
            return client.GetNotification(notificationId);
        }

        /// <summary>
        /// Gets a notification by id.
        /// </summary>
        /// <param name="notificationId">Id of the notification</param>
        /// <returns>notification</returns>
        public Notification GetNotificationBlocking(string notificationId)
        {
            return GetNotification(notificationId).Result;
        }

        /// <summary>
        /// Removes a subscriber.
        /// </summary>
        /// <param name="subscriberId">subscriberId</param>
        /// <returns>task that indicates whether the operation finished or had an error</returns>
        public Task UnsubscribeFromNotifications(string subscriberId)
        {
            return client
                    .UnsubscribeFromNotifications(subscriberId);
        }

        /// <summary>
        /// Removes a subscriber.
        /// </summary>
        /// <param name="subscriberId">subscriberId</param>
        public void UnsubscribeFromNotificationsBlocking(string subscriberId)
        {
            UnsubscribeFromNotifications(subscriberId).Wait();
        }

        /// <summary>
        /// Creates a subscriber to push notifications.
        /// </summary>
        /// <param name="handler">specify the handler of the notifications</param>
        /// <param name="handlerInstructions">map of instructions for the handler</param>
        /// <returns>Subscriber</returns>
        public Task<Subscriber> SubscribeToNotifications(
                string handler,
                MapField<string, string> handlerInstructions)
        {
            return client.SubscribeToNotifications(handler, handlerInstructions);
        }

        /// <summary>
        /// Subscribes a device to receive push notifications.
        /// </summary>
        /// <param name="handler">specify the handler of the notifications</param>
        /// <returns>Subscriber</returns>
        public Task<Subscriber> SubscribeToNotifications(string handler)
        {
            return SubscribeToNotifications(handler, new MapField<string, string>());
        }

        /// <summary>
        /// Subscribes a device to receive push notifications.
        /// </summary>
        /// <param name="handler">specify the handler of the notifications</param>
        /// <param name="handlerInstructions">map of instructions for the handler</param>
        /// <returns>Subscriber</returns>
        public Subscriber SubscribeToNotificationsBlocking(
                string handler,
                MapField<string, string> handlerInstructions)
        {
            return SubscribeToNotifications(handler, handlerInstructions).Result;
        }

        /// <summary>
        /// Subscribes a device to receive push notifications.
        /// </summary>
        /// <param name="handler">specify the handler of the notifications</param>
        /// <returns>Subscriber</returns>
        public Subscriber SubscribeToNotificationsBlocking(string handler)
        {
            return SubscribeToNotifications(handler).Result;
        }

        /// <summary>
        /// Gets subscribers.
        /// </summary>
        /// <returns>subscribers</returns>
        public Task<IList<Subscriber>> GetSubscribers()
        {
            return client.GetSubscribers();
        }

        /// <summary>
        /// Gets a list of all subscribers.
        /// </summary>
        /// <returns>subscribers</returns>
        public IList<Subscriber> GetSubscribersBlocking()
        {
            return GetSubscribers().Result;
        }

        /// <summary>
        /// Gets a subscriber by id.
        /// </summary>
        /// <param name="subscriberId">Id of the subscriber</param>
        /// <returns>subscriber</returns>
        public Task<Subscriber> GetSubscriber(string subscriberId)
        {
            return client.GetSubscriber(subscriberId);
        }

        /// <summary>
        /// Gets a subscriber by Id.
        /// </summary>
        /// <param name="subscriberId">subscriberId</param>
        /// <returns>subscriber</returns>
        public Subscriber GetSubscriberBlocking(string subscriberId)
        {
            return GetSubscriber(subscriberId).Result;
        }

        /// <summary>
        /// Sign with a Token signature a token request state payload.
        /// </summary>
        /// <param name="tokenRequestId">token request id</param>
        /// <param name="tokenId">token id</param>
        /// <param name="state">state</param>
        /// <returns>signature</returns>
        public Task<Signature> SignTokenRequestState(
                string tokenRequestId,
                string tokenId,
                string state)
        {
            return client.SignTokenRequestState(tokenRequestId, tokenId, state);
        }

        /// <summary>
        /// Sign with a Token signature a token request state payload.
        /// </summary>
        /// <param name="tokenRequestId">token request id</param>
        /// <param name="tokenId">token id</param>
        /// <param name="state">state</param>
        /// <returns>signature</returns>
        public Signature SignTokenRequestStateBlocking(
                string tokenRequestId,
                string tokenId,
                string state)
        {
            return SignTokenRequestState(tokenRequestId, tokenId, state).Result;
        }

        /// <summary>
        /// Apply SCA for the given list of account IDs.
        /// </summary>
        /// <param name="accountIds">list of account ids</param>
        /// <returns>task</returns>
		public Task ApplySca(IList<string> accountIds)
        {
            return client.ApplySca(accountIds);
        }

        /// <summary>
        /// Apply SCA for the given list of account IDs.
        /// </summary>
        /// <param name="accountIds">list of account ids</param>
        /// <returns>task</returns>
        public void ApplyScaBlocking(IList<string> accountIds)
        {
            ApplySca(accountIds).Wait();
        }

        /// <summary>
        /// Creates a test bank account in a fake bank and links the account.
        /// </summary>
        /// <param name="balance">account balance to set</param>
        /// <param name="currency">currency code, e.g. "EUR"</param>
        /// <returns>the linked account</returns>
        public Task<Account> CreateTestBankAccount(double balance, string currency)
        {
            return CreateTestBankAccountImpl(balance, currency)
                   .Map(account =>
                            new Account(account, client, this));
        }

        /// <summary>
        /// Creates a test bank account in a fake bank and links the account.
        /// </summary>
        /// <param name="balance">account balance to set</param>
        /// <param name="currency">currency code, e.g. "EUR"</param>
        /// <returns>the linked account</returns>
        public Account CreateTestBankAccountBlocking(double balance, string currency)
        {
            return CreateTestBankAccount(balance, currency).Result;
        }

        private Task<IList<Account>> ToAccountList(
                Task<IList<ProtoAccount>> accounts)
        {
            return accounts.Map(account => (IList<Account>)account
                    .Select(acc =>
                            new Account(this, acc, client))
                    .ToList());
        }

        /// <summary>
        /// Updates the status of a notification.
        /// </summary>
        /// <param name="notificationId">the notification id to update</param>
        /// <param name="status">the status to update</param>
        /// <returns>task</returns>
        public Task UpdateNotificationStatus(string notificationId, Status status)
        {
            return client.UpdateNotificationStatus(notificationId, status);
        }

        /// <summary>
        /// Updates the status of a notification.
        /// </summary>
        /// <param name="notificationId">the notification id to update</param>
        /// <param name="status">the status to update</param>
        public void UpdateNotificationStatusBlocking(string notificationId, Status status)
        {
            UpdateNotificationStatus(notificationId, status).Wait();
        }

    }
}
