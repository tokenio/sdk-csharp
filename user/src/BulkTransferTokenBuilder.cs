using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using static Tokenio.Proto.Common.TokenProtos.TokenRequestPayload;

namespace Tokenio.User
{
    public class BulkTransferTokenBuilder
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int REF_ID_MAX_LENGTH = 18;

        private readonly TokenPayload payload;

        public BulkTransferTokenBuilder(
                Member member,
                IList<BulkTransferBody.Types.Transfer> transfers,
                double totalAmount,
                TransferEndpoint source)
        {
            this.payload = new TokenPayload
            {
                Version = "1.0",
                From = new TokenMember
                {
                    Id = member.MemberId()
                },
                BulkTransfer = new BulkTransferBody
                {
                    TotalAmount = totalAmount.ToString(),
                    Source = source,
                    Transfers = { transfers }
                }
            };
            IList<Alias> aliases = member.GetAliases().Result;

            if (aliases == null)
            {
                payload.From.Alias = aliases[0];
            }
        }


        public BulkTransferTokenBuilder(TokenRequest tokenRequest)
        {
            if (tokenRequest.RequestPayload.RequestBodyCase != RequestBodyOneofCase.BulkTransferBody)
            {
                throw new ArgumentException(
                "Require token request with bulk transfer body.");
            }

            if (tokenRequest.RequestPayload.To != null)
            {
                throw new ArgumentException("No payee on token request");
            }

            BulkTransferBody body = tokenRequest.RequestPayload.BulkTransferBody;

            this.payload = new TokenPayload
            {
                Version = "1.0",
                RefId = tokenRequest.RequestPayload.RefId,
                From = tokenRequest.RequestOptions.From,
                To = tokenRequest.RequestPayload.To,
                Description = tokenRequest.RequestPayload.Description,
                ReceiptRequested = tokenRequest.RequestOptions.ReceiptRequested,
                TokenRequestId = tokenRequest.Id,
                BulkTransfer = body
            };

            if (tokenRequest.RequestPayload.ActingAs != null)
            {
                this.payload.ActingAs = tokenRequest.RequestPayload.ActingAs;
            }
        }

        /// <summary>
        /// Sets the expiration date.
        /// </summary>
        /// <param name="expiresAtMs">expiration date in ms</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetExpiresAtMs(long expiresAtMs)
        {
            payload.ExpiresAtMs = expiresAtMs;
            return this;
        }

        /// <summary>
        /// Sets the effective date.
        /// </summary>
        /// <param name="effectiveAtMs">effective date in ms.</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetEffectiveAtMs(long effectiveAtMs)
        {
            payload.EffectiveAtMs = effectiveAtMs;
            return this;
        }

        /// <summary>
        /// Sets the time after which endorse is no longer possible.
        /// </summary>
        /// <param name="endorseUntilMs">endorse until, in milliseconds.</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetEndorseUntilMs(long endorseUntilMs)
        {
            payload.EndorseUntilMs = endorseUntilMs;
            return this;
        }


        /// <summary>
        /// Sets the description.
        /// </summary>
        /// <param name="description">description</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetDescription(string description)
        {
            payload.Description = description;
            return this;
        }

        /// <summary>
        /// Adds a transfer source.
        /// </summary>
        /// <param name="source">the source</param>
        /// <returns></returns>
        public BulkTransferTokenBuilder SetSource(TransferEndpoint source)
        {
            payload.BulkTransfer.Source = source;
            return this;
        }

        /// <summary>
        /// Adds a linked source account to the token.
        /// </summary>
        /// <param name="accountId">source accountId</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetAccountId(string accountId)
        {
            if (payload.From.Id == null)
            {
                throw new ArgumentNullException();
            }
            SetSource(new TransferEndpoint
            {
                Account = new Proto.Common.AccountProtos.BankAccount
                {
                    Token = new Proto.Common.AccountProtos.BankAccount.Types.Token
                    {
                        AccountId = accountId,
                        MemberId = payload.From.Id
                    }
                }
            });
            return this;
        }

        /// <summary>
        /// Sets the alias of the payee.
        /// </summary>
        /// <param name="toAlias">alias</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetToAlias(Alias toAlias)
        {
            payload.To.Alias = toAlias;
            return this;
        }

        /// <summary>
        /// Sets the memberId of the payee.
        /// </summary>
        /// <param name="toMemberId">memberId</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetToMemberId(string toMemberId)
        {
            var x = payload.To;
            if (x == null)
            {
                x = new TokenMember { };
            }
            x.Id = toMemberId;
            payload.To = x;
            return this;
        }

        /// <summary>
        /// Sets the reference ID of the token.
        /// </summary>
        /// <param name="refId">the reference Id, at most 18 characters long</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetRefId(string refId)
        {
            if (refId.Length > REF_ID_MAX_LENGTH)
            {
                throw new ArgumentException(string.Format(
                        "The length of the refId is at most {0}, got: {1}",
                        REF_ID_MAX_LENGTH,
                        refId.Length));
            }
            payload.RefId = refId;
            return this;
        }

        /// <summary>
        /// Sets acting as on the token.
        /// </summary>
        /// <param name="actingAs">entity the redeemer is acting on behalf of</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetActingAs(ActingAs actingAs)
        {
            payload.ActingAs = actingAs;
            return this;
        }

        /// <summary>
        /// Sets the token request ID.
        /// </summary>
        /// <param name="tokenRequestId">token request id</param>
        /// <returns></returns>
        public BulkTransferTokenBuilder SetTokenRequestId(string tokenRequestId)
        {
            payload.TokenRequestId = tokenRequestId;
            return this;
        }


        /// <summary>
        /// Sets the flag indicating whether a receipt is requested.
        /// </summary>
        /// <param name="receiptRequested">receipt requested flag</param>
        /// <returns>builder</returns>
        public BulkTransferTokenBuilder SetReceiptRequested(bool receiptRequested)
        {
            payload.ReceiptRequested = receiptRequested;
            return this;
        }

        /// <summary>
        /// Builds a token payload, without uploading blobs or attachments.
        /// </summary>
        /// <returns>token payload</returns>
        public TokenPayload BuildPayload()
        {
            if (payload.RefId != null)
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Tokenio.Utils.Util.Nonce();
            }
            return payload;
        }
    }
}
