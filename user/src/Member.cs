using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using log4net;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.MoneyProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.SubscriberProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using Tokenio.Proto.Common.TransferProtos;
using Tokenio.User.Browser;
using Tokenio.User.Rpc;
using Tokenio.Utils;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using ProtoAccount = Tokenio.Proto.Common.AccountProtos.Account;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using TokenType = Tokenio.Proto.Gateway.GetTokensRequest.Types.Type;

namespace Tokenio.User
{
    public class Member : Tokenio.Member
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Client client;
        private readonly IBrowserFactory browserFactory;

        public Member(string memberId,
            Client client,
            IBrowserFactory browserFactory)
            : base(memberId,client)
        {
            this.client = client;
            this.browserFactory = browserFactory;
        }

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of accounts</returns>
        public Task<IList<Account>> GetAccounts()
        {
            return GetAccountsImpl()
                .Map(accounts => (IList<Account>)accounts
                    .Select(account => new Account(account, client, this))
                    .ToList());
        }

        /// <summary>
        /// Looks up funding bank accounts linked to Token.
        /// </summary>
        /// <returns>a list of accounts</returns>
        public IList<Account> GetAccountsBlocking()
        {
            return GetAccounts().Result;
        }

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        public Task<Account> GetAccount(string accountId)
        {
            return GetAccountImpl(accountId)
                .Map(account => new Account(account, client, this));
        }

        /// <summary>
        /// Looks up a funding bank account linked to Token.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>the account</returns>
        public Account GetAccountBlocking(string accountId)
        {
            return GetAccount(accountId).Result;
        }

        /// <summary>
        /// Set the default bank account for this member.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <returns>a Task</returns>
        public Task SetDefaultAccount(string accountId)
        {
            return client.SetDefaultAccount(accountId);
        }

        /// <summary>
        /// Set the default bank account for this member.
        /// </summary>
        /// <param name="accountId">the account id</param>
        public void SetDefaultAccountBlocking(string accountId)
        {
            client.SetDefaultAccount(accountId).Wait();
        }

        /// <summary>
        /// Get the default bank account for this member.
        /// </summary>
        /// <returns>the account</returns>
        public Task<Account> GetDefaultAccount()
        {
            return client
                .GetDefaultAccount(MemberId())
                .Map(account => new Account(this, account, client));
        }

        /// <summary>
        /// Get the default bank account for this member.
        /// </summary>
        /// <returns>the account</returns>
        public Account GetDefaultAccountBlocking()
        {
            return GetDefaultAccount().Result;
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
        /// Looks up an existing token transfer.
        /// </summary>
        /// <param name="transferId">the transfer id</param>
        /// <returns>the transfer record</returns>
        public Transfer GetTransferBlocking(string transferId)
        {
            return GetTransfer(transferId).Result;
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
        /// Looks up existing token transfers.
        /// </summary>
        /// <param name="tokenId">nullable token id</param>
        /// <param name="offset">nullable offset to start at</param>
        /// <param name="limit">max number of records to return</param>
        /// <returns>a paged list of transfers</returns>
        public PagedList<Transfer> GetTransfersBlocking(
            string tokenId,
            string offset,
            int limit)
        {
            return GetTransfers(tokenId, offset, limit).Result;
        }

        public Task<PrepareTokenResult> PrepareTransferToken(
            TransferTokenBuilder transferTokenBuilder)
        {
            transferTokenBuilder.From(MemberId());
            return client.PrepareToken(transferTokenBuilder.BuildPayload());
        }

        /**
         * Prepares a transfer token, returning the resolved token payload and policy.
         *
         * @param transferTokenBuilder transfer token builder
         * @return resolved token payload and policy
         */
        public PrepareTokenResult PrepareTransferTokenBlocking(
                TransferTokenBuilder transferTokenBuilder)
        {
            return PrepareTransferToken(transferTokenBuilder).Result;
        }

        /// <summary>
        /// Prepares an access token, returning the resolved token payload and policy.
        /// </summary>
        /// <returns>resolved token payload and policy</returns>
        /// <param name="accessTokenBuilder">Access token builder.</param>
        public Task<PrepareTokenResult> PrepareAccessToken(
            AccessTokenBuilder accessTokenBuilder)
        {
            accessTokenBuilder.From(MemberId());
            return client.PrepareToken(accessTokenBuilder.Build());
        }

        /// <summary>
        /// Prepares an access token, returning the resolved token payload and policy.
        /// </summary>
        /// <returns>resolved token payload and policy</returns>
        /// <param name="accessTokenBuilder">Access token builder.</param>
        public PrepareTokenResult PrepareAccessTokenBlocking(AccessTokenBuilder accessTokenBuilder)
        {
            return PrepareAccessToken(accessTokenBuilder).Result;
        }

        /// <summary>
        /// Creates a token directly from a resolved token payload and list of token signatures.
        /// </summary>
        /// <returns>token returned by server</returns>
        /// <param name="payload">Payload.</param>
        /// <param name="signatures">list of Signatures.</param>
        public Task<Token> CreateToken(TokenPayload payload, IList<Signature> signatures)
        {
            return CreateToken(payload, signatures, null);
        }


        /// <summary>
        /// Creates a token directly from a resolved token payload and list of token signatures.
        /// </summary>
        /// <returns>The token returned by server</returns>
        /// <param name="payload">Payload.</param>
        /// <param name="signatures">Signatures.</param>
        /// <param name="tokenRequestId">Token request identifier.</param>
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
        /// <returns>The token returned by server</returns>
        /// <param name="payload">Payload.</param>
        /// <param name="keyLevel">Key level.</param>
        public Task<Token> CreateToken(TokenPayload payload, Level keyLevel)
        {
            return CreateToken(payload, null, keyLevel);
        }

        /// <summary>
        /// Creates a token with the member's own signature.
        /// </summary>
        /// <returns>The token.</returns>
        /// <param name="payload">Payload.</param>
        /// <param name="tokenRequestId">Token request identifier.</param>
        /// <param name="keyLevel">Key level.</param>
        public Task<Token> CreateToken(
            TokenPayload payload,
            string tokenRequestId,
            Level keyLevel)
        {
            IList<Signature> signatures = new List<Signature> { SignTokenPayload(payload, keyLevel) };
            return client.CreateToken(
                    payload,
                    tokenRequestId,
                    signatures);
        }

        /// <summary>
        /// Creates a token directly from a resolved token payload and list of token signatures.
        /// </summary>
        /// <returns>token returned by server</returns>
        /// <param name="payload">Payload.</param>
        /// <param name="signatures">list of Signatures.</param>
        public Token CreateTokenBlocking(TokenPayload payload, IList<Signature> signatures)
        {
            return CreateToken(payload, signatures).Result;
        }

        /// <summary>
        /// Creates a token directly from a resolved token payload and list of token signatures.
        /// </summary>
        /// <returns>The token returned by server</returns>
        /// <param name="payload">Payload.</param>
        /// <param name="signatures">Signatures.</param>
        /// <param name="tokenRequestId">Token request identifier.</param>
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
        /// <returns>The token returned by server</returns>
        /// <param name="payload">Payload.</param>
        /// <param name="keyLevel">Key level.</param>
        public Token CreateTokenBlocking(TokenPayload payload, Level keyLevel)
        {
            return CreateToken(payload, keyLevel).Result;
        }

        /// <summary>
        /// Creates a token with the member's own signature.
        /// </summary>
        /// <returns>The token.</returns>
        /// <param name="payload">Payload.</param>
        /// <param name="tokenRequestId">Token request identifier.</param>
        /// <param name="keyLevel">Key level.</param>
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
        /// <param name="amount">amount</param>
        /// <param name="currency">currency code, e.g. "USD"</param>
        /// <returns>transfer token builder</returns>
        [Obsolete("CreateTransferToken is deprecated.")]
        public TransferTokenBuilder CreateTransferToken(double amount, string currency)
        {
            return new TransferTokenBuilder(this, amount, currency);
        }

        /// <summary>
        /// Creates a new transfer token builder from a token request.
        /// </summary>
        /// <param name="tokenRequest">token request</param>
        /// <returns>transfer token builder</returns>
        public TransferTokenBuilder CreateTransferToken(Proto.Common.TokenProtos.TokenRequest tokenRequest)
        {
            return new TransferTokenBuilder(this, tokenRequest);
        }

        /// <summary>
        /// Creates a new transfer token from a token payload.
        /// </summary>
        /// <param name="payload">token request</param>
        /// <returns>transfer token returned by the server</returns>
        [Obsolete("CreateTransferToken is deprecated. Use CreateToken(TokenPayload,IList)} instead.")]
        public Task<Token> CreateTransferToken(TokenPayload payload)
        {
            return client.CreateTransferToken(payload);
        }

        /// <summary>
        /// Creates a new transfer token from a token payload.
        /// </summary>
        /// <param name="payload">token request</param>
        ///  /// <param name="tokenRequestId">token request</param>
        /// <returns>transfer token returned by the server</returns>
        [Obsolete("CreateTransferToken is deprecated. Use CreateToken(TokenPayload, IList, string) instead.")]
        public Task<Token> CreateTransferToken(TokenPayload payload, string tokenRequestId)
        {
            return client.CreateTransferToken(payload, tokenRequestId);
        }

        /// <summary>
        /// Creates a new transfer token builder from a token request.
        /// </summary>
        /// <param name="tokenRequest">token request</param>
        /// <returns>transfer token builder</returns>
        public TransferTokenBuilder CreateTransferTokenBlocking(Proto.Common.TokenProtos.TokenRequest tokenRequest)
        {
            return CreateTransferToken(tokenRequest);
        }

        /// <summary>
        /// Creates a new transfer token from a token payload.
        /// </summary>
        /// <param name="payload">token request</param>
        /// <returns>transfer token returned by the server</returns>
        [Obsolete("CreateTransferToken is deprecated. Use CreateToken(TokenPayload, IList) instead.")]
        public Token CreateTransferTokenBlocking(TokenPayload payload)
        {
            return CreateTransferToken(payload).Result;
        }

        /// <summary>
        /// Creates a new transfer token from a token payload.
        /// </summary>
        /// <param name="payload">token request</param>
        ///  /// <param name="tokenRequestId">token request</param>
        /// <returns>transfer token returned by the server</returns>
        [Obsolete("CreateTransferToken is deprecated. Use CreateToken(TokenPayload, IList, string) instead.")]
        public Token CreateTransferTokenBlocking(TokenPayload payload, string tokenRequestId)
        {
            return CreateTransferToken(payload, tokenRequestId).Result;
        }

        /// <summary>
        /// Creates an access token built from a given AccessTokenBuilder.
        /// </summary>
        /// <param name="accessTokenBuilder">an AccessTokenBuilder to create access token from</param>
        /// <returns>the access token created</returns>
        public Task<Token> CreateAccessToken(AccessTokenBuilder accessTokenBuilder)
        {
            return client.CreateAccessToken(
                    accessTokenBuilder.From(MemberId()).Build(),
                    accessTokenBuilder.tokenRequestId);
        }

        /// <summary>
        /// Creates an access token built from a given AccessTokenBuilder.
        /// </summary>
        /// <param name="accessTokenBuilder">an AccessTokenBuilder to create access token from</param>
        /// <returns>the access token created</returns>
        public Token CreateAccessTokenBlocking(AccessTokenBuilder accessTokenBuilder)
        {
            return CreateAccessToken(accessTokenBuilder).Result;
        }

        /// <summary>
        /// Endorses the token by signing it. The signature is persisted along
        /// with the token.
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
        /// <param name="token">token to endorse</param>
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
        /// Cancels the existing access token, creates a replacement and optionally endorses it.
        /// </summary>
        /// <param name="tokenToCancel">token to cancel</param>
        /// <param name="tokenToCreate">token to create</param>
        /// <returns>result of the replacement operation</returns>
        public Task<TokenOperationResult> ReplaceAccessToken(
            Token tokenToCancel,
            AccessTokenBuilder tokenToCreate)
        {
            return client.ReplaceToken(
                    tokenToCancel,
                    tokenToCreate.From(MemberId()).Build());
        }

        /// <summary>
        /// Cancels the existing access token, creates a replacement and optionally endorses it.
        /// </summary>
        /// <param name="tokenToCancel">token to cancel</param>
        /// <param name="tokenToCreate">token to create</param>
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
        /// <returns>Task that indicates whether the operation finished or had an error</returns>
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
        /// Gets the member's receipt email address.
        /// </summary>
        /// <returns>receipt contact</returns>
        public ReceiptContact GetReceiptContactBlocking()
        {
            return GetReceiptContact().Result;
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
        public Task<Token> GetToken(String tokenId)
        {
            return client.GetToken(tokenId);
        }

        /// <summary>
        /// Looks up a existing token.
        /// </summary>
        /// <param name="tokenId">token id</param>
        /// <returns>token returned by the server</returns>
        public Token GetTokenBlocking(String tokenId)
        {
            return GetToken(tokenId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(Token token)
        {
            return RedeemTokenInternal(token, null, null, null, null, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(Token token, string refId)
        {
            return RedeemTokenInternal(token, null, null, null, null, refId);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Task<Transfer> RedeemToken(Token token, TransferDestination destination)
        {
            return RedeemToken(token, null, null, null, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
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
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
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
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
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
            return RedeemTokenInternal(token, amount, currency, description, null, null);
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
            TransferDestination destination)
        {
            return RedeemToken(token, amount, currency, null, destination, null);
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
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
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
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
        /// <returns>a transfer record</returns>
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
            TransferDestination destination,
            string refId)
        {
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

        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Task<Transfer> RedeemToken(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination,
            string refId)
        {
            return RedeemTokenInternal(token, amount, currency, description, destination, refId);
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
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token)
        {
            return RedeemToken(token).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token, string refId)
        {
            return RedeemToken(token, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        public Transfer RedeemTokenBlocking(Token token, TransferDestination destination)
        {
            return RedeemToken(token, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(Token token, TransferEndpoint destination)
        {
            return RedeemToken(token, destination).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
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
        /// <param name="token">the transfer token</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <param name="refId">the reference id of the transfer</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(
            Token token,
            TransferEndpoint destination,
            string refId)
        {
            return RedeemToken(token, destination, refId).Result;
        }

        /// <summary>
        /// Redeems a transfer token.
        /// </summary>
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <returns>a transfer record</returns>
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
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
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
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            TransferEndpoint destination)
        {
            return RedeemToken(token, amount, currency, destination).Result;
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
        /// <param name="token">the transfer token</param>
        /// <param name="amount">the amount to transfer</param>
        /// <param name="currency">the currency</param>
        /// <param name="description">the description of the transfer</param>
        /// <param name="destination">the transfer instruction destination</param>
        /// <returns>a transfer record</returns>
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination)
        {
            return RedeemToken(token, amount, currency, description, destination).Result;
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
        [Obsolete("Use TransferDestination instead of TransferEndpoint.")]
        public Transfer RedeemTokenBlocking(
            Token token,
            double? amount,
            string currency,
            string description,
            TransferEndpoint destination,
            string refId)
        {
            return RedeemToken(token, amount, currency, description, destination, refId).Result;
        }

        /// <summary>
        /// Links a funding bank accounts to Token and returns it to the caller.
        /// </summary>
        /// <param name="authorization">the transfer token</param>
        /// <returns>list of linked accounts</returns>
        public Task<IList<Account>> LinkAccounts(
            BankAuthorization authorization)
        {
            return ToAccountList(client.LinkAccounts(authorization));
        }

        /// <summary>
        /// Links a funding bank accounts to Token and returns it to the caller.
        /// </summary>
        /// <param name="bankId">the transfer token</param>
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
        /// <param name="authorization">the transfer token</param>
        /// <returns>list of linked accounts</returns>
        public IList<Account> LinkAccountsBlocking(BankAuthorization authorization)
        {
            return LinkAccounts(authorization).Result;
        }

        /// <summary>
        /// Links a funding bank accounts to Token and returns it to the caller.
        /// </summary>
        /// <param name="bankId">the transfer token</param>
        /// <param name="accessToken">OAuth access token</param>
        /// <returns>list of linked accounts</returns>
        public IList<Account> LinkAccountsBlocking(string bankId, string accessToken)
        {
            return LinkAccounts(bankId, accessToken).Result;
        }

        /// <summary>
        /// Unlinks bank accounts previously linked via linkAccounts call.
        /// </summary>
        /// <param name="accountIds">account ID</param>
        /// <returns>nothing</returns>
        public Task UnlinkAccounts(IList<string> accountIds)
        {
            return client.UnlinkAccounts(accountIds);
        }

        /// <summary>
        /// Unlinks bank accounts previously linked via linkAccounts call.
        /// </summary>
        /// <param name="accountIds">account ID</param>
        /// <returns>nothing</returns>
        public void UnlinkAccountsBlocking(IList<string> accountIds)
        {
            UnlinkAccounts(accountIds).Wait();
        }

        /// <summary>
        /// Looks up current account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        [Obsolete("GetCurrentBalance is deprecated. Use GetBalance(accountId, keyLevel) instead.")]
        public Task<Money> GetCurrentBalance(string accountId, Level keyLevel)
        {
            return client.GetBalance(accountId, keyLevel)
                .Map(response => response.Current);
        }

        /// <summary>
        /// Looks up current account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        [Obsolete("GetCurrentBalanceBlocking is deprecated. Use GetBalanceBlocking(accountId, keyLevel).Current instead.")]
        public Money GetCurrentBalanceBlocking(string accountId, Level keyLevel)
        {
            return GetCurrentBalance(accountId, keyLevel).Result;
        }

        /// <summary>
        /// Looks up available account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        [Obsolete("GetAvailableBalance is deprecated. Use GetBalance(accountId, keyLevel) instead.")]
        public Task<Money> GetAvailableBalance(string accountId, Level keyLevel)
        {
            return client.GetBalance(accountId, keyLevel)
                .Map(response => response.Available);
        }

        /// <summary>
        /// Looks up available account balance.
        /// </summary>
        /// <param name="accountId">the account id</param>
        /// <param name="keyLevel">the key level</param>
        /// <returns>the balance</returns>
        [Obsolete("GetAvailableBalanceBlocking is deprecated. Use GetBalanceBlocking(accountId, keyLevel).Available instead.")]
        public Money GetAvailableBalanceBlocking(string accountId, Level keyLevel)
        {
            return GetAvailableBalance(accountId, keyLevel).Result;
        }


        public Task RemoveNonStoredKeys()
        {
            IList<Key> storedKeys = client.GetCryptoEngine().GetPublicKeys();
            return client.GetMember(MemberId())
                .FlatMap(member =>
                    {
                        IList<string> toRemoveIds = new List<string>();
                        foreach (Key key in member.Keys)
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
        /// Replaces auth'd member's public profile.
        /// </summary>
        /// <param name="profile">the protile to set</param>
        /// <returns>the updated profile</returns>
        public Task<Profile> SetProfile(Profile profile)
        {
            return client.SetProfile(profile);
        }

        /// <summary>
        /// Replaces auth'd member's public profile.
        /// </summary>
        /// <param name="profile">the protile to set</param>
        /// <returns>the updated profile</returns>
        public Profile SetProfileBlocking(Profile profile)
        {
            return SetProfile(profile).Result;
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
        /// Gets a member's public profile. Unlike setProfile, you can get another member's profile.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <returns>the profile</returns>
        public Profile GetProfileBlocking(string memberId)
        {
            return GetProfile(memberId).Result;
        }

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
        /// Replaces auth'd member's public profile picture.
        /// </summary>
        /// <param name="type">MIME type of the picture</param>
        /// <param name="data">the image data</param>
        /// <returns>a task</returns>
        public void SetProfilePictureBlocking(string type, byte[] data)
        {
            SetProfilePicture(type, data).Wait();
        }

        public Task<Blob> GetProfilePicture(string memberId, ProfilePictureSize size)
        {
            return client.GetProfilePicture(memberId, size);
        }

        /// <summary>
        /// Gets a member's public profile picture. Unlike set, you can get another member's picture.
        /// </summary>
        /// <param name="memberId">the ID of the desired member</param>
        /// <param name="size">the desired size category (small, medium, large, original)</param>
        /// <returns>a blob with picture; empty if the member has no picture</returns>
        public Blob GetProfilePictureBlocking(string memberId, ProfilePictureSize size)
        {
            return GetProfilePicture(memberId, size).Result;
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
            return client.GetNotifications(limit, offset);
        }

        /// <summary>
        /// Gets the notifications blocking.
        /// </summary>
        /// <returns>The notifications blocking.</returns>
        /// <param name="offset">Offset.</param>
        /// <param name="limit">Limit.</param>
        public PagedList<Notification> GetNotificationsBlocking(
                int limit,
                string offset = null)
        {
            return GetNotifications(limit, offset).Result;
        }

        /// <summary>
        /// Gets the notification.
        /// </summary>
        /// <returns>The notification.</returns>
        /// <param name="notificationId">Notification identifier.</param>
        public Task<Notification> GetNotification(string notificationId)
        {
            return client.GetNotification(notificationId);
        }

        /// <summary>
        /// Gets the notification blocking.
        /// </summary>
        /// <returns>The notification blocking.</returns>
        /// <param name="notificationId">Notification identifier.</param>
        public Notification GetNotificationBlocking(string notificationId)
        {
            return GetNotification(notificationId).Result;
        }

        /// <summary>
        /// Unsubscribes from notifications.
        /// </summary>
        /// <returns>The from notifications.</returns>
        /// <param name="subscriberId">Subscriber identifier.</param>
        public Task UnsubscribeFromNotifications(string subscriberId)
        {
            return client
                    .UnsubscribeFromNotifications(subscriberId);
        }

        /// <summary>
        /// Unsubscribes from notifications blocking.
        /// </summary>
        /// <param name="subscriberId">Subscriber identifier.</param>
        public void UnsubscribeFromNotificationsBlocking(string subscriberId)
        {
            UnsubscribeFromNotifications(subscriberId).Wait();
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
            return client.SubscribeToNotifications(handler, handlerInstructions);
        }

       /// <summary>
       /// Subscribes to notifications.
       /// </summary>
       /// <returns>The to notifications.</returns>
       /// <param name="handler">Handler.</param>
        public Task<Subscriber> SubscribeToNotifications(string handler)
        {
            return SubscribeToNotifications(handler, new MapField<string, string>());
        }

        /// <summary>
        /// Subscribes to notifications blocking.
        /// </summary>
        /// <returns>The to notifications blocking.</returns>
        /// <param name="handler">Handler.</param>
        /// <param name="handlerInstructions">Handler instructions.</param>
        public Subscriber SubscribeToNotificationsBlocking(
                string handler,
                MapField<string, string> handlerInstructions)
        {
            return SubscribeToNotifications(handler, handlerInstructions).Result;
        }

        /// <summary>
        /// Subscribes to notifications blocking.
        /// </summary>
        /// <returns>The to notifications blocking.</returns>
        /// <param name="handler">Handler.</param>
        public Subscriber SubscribeToNotificationsBlocking(String handler)
        {
            return SubscribeToNotifications(handler).Result;
        }

        /// <summary>
        /// Gets the subscribers.
        /// </summary>
        /// <returns>The subscribers.</returns>
        public Task<IList<Subscriber>> GetSubscribers()
        {
            return client.GetSubscribers();
        }

        /// <summary>
        /// Gets the subscribers blocking.
        /// </summary>
        /// <returns>The subscribers blocking.</returns>
        public IList<Subscriber> GetSubscribersBlocking()
        {
            return GetSubscribers().Result;
        }

        /// <summary>
        /// Gets the subscriber.
        /// </summary>
        /// <returns>The subscriber.</returns>
        /// <param name="subscriberId">Subscriber identifier.</param>
        public Task<Subscriber> GetSubscriber(string subscriberId)
        {
            return client.GetSubscriber(subscriberId);
        }

        /// <summary>
        /// Gets the subscriber blocking.
        /// </summary>
        /// <returns>The subscriber blocking.</returns>
        /// <param name="subscriberId">Subscriber identifier.</param>
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

        public Task ApplySca(IList<string> accountIds)
        {
            return client.ApplySca(accountIds);
        }

        public void ApplyScaBlocking(IList<string> accountIds)
        {
            ApplySca(accountIds).Wait();
        }

        /// <summary>
        /// Creates the test bank account.
        /// </summary>
        /// <returns>The test bank account.</returns>
        /// <param name="balance">Balance.</param>
        /// <param name="currency">Currency.</param>
        public Task<Account> CreateTestBankAccount(double balance, string currency)
        {
            return CreateTestBankAccountImpl(balance, currency)
                   .Map(account => new Account(account, client, this));
        }

        /// <summary>
        /// Creates the test bank account
        /// </summary>
        /// <returns>The test bank account blocking.</returns>
        /// <param name="balance">Balance.</param>
        /// <param name="currency">Currency.</param>
        public Account CreateTestBankAccountBlocking(double balance, string currency)
        {
            return CreateTestBankAccount(balance, currency).Result;
        }

        /// <summary>
        /// Tos the account list.
        /// </summary>
        /// <returns>The account list.</returns>
        /// <param name="accounts">Accounts.</param>
        private Task<IList<Account>> ToAccountList(
            Task<IList<ProtoAccount>> accounts)
        {
            return accounts.Map(account => (IList<Account>)account
                .Select(acc => new Account(this, acc, client))
                .ToList()); 
        }
    }
}
