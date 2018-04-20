using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Io.Token.Proto.Banklink;
using Io.Token.Proto.Common.Account;
using Io.Token.Proto.Common.Alias;
using Io.Token.Proto.Common.Blob;
using Io.Token.Proto.Common.Pricing;
using Io.Token.Proto.Common.Token;
using Io.Token.Proto.Common.Transferinstructions;
using log4net;
using sdk.Exceptions;
using static Io.Token.Proto.Common.Account.BankAccount.Types;
using static Io.Token.Proto.Common.Blob.Blob.Types;
using static Io.Token.Proto.Common.Transferinstructions.TransferInstructions.Types;
using AccountType = Io.Token.Proto.Common.Account.BankAccount.AccountOneofCase;
using Token = Io.Token.Proto.Common.Token.Token;
using TokenAccount = Io.Token.Proto.Common.Account.BankAccount.Types.Token;

namespace sdk.Api
{
    public class TransferTokenBuilder
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IList<AccountType> sourceTypes = new List<AccountType>
        {
            AccountType.TokenAuthorization,
            AccountType.Token,
            AccountType.Bank
        };

        private readonly MemberAsync member;
        private readonly TokenPayload payload;

        // Used for attaching files / data to tokens
        private readonly IList<Payload> blobPayloads;

        /// <summary>
        /// Creates the builder object.
        /// </summary>
        /// <param name="member">the payer of the token</param>
        /// <param name="amount">the lifetime amount of the token</param>
        /// <param name="currency">the currency of the token</param>
        public TransferTokenBuilder(
            MemberAsync member,
            double amount,
            string currency)
        {
            this.member = member;
            this.payload = new TokenPayload
            {
                Version = "1.0",
                From = new TokenMember {Id = member.MemberId()},
                Transfer = new TransferBody
                {
                    Currency = currency,
                    LifetimeAmount = Convert.ToString(amount, CultureInfo.InvariantCulture),
                    Instructions = new TransferInstructions
                    {
                        Source = new TransferEndpoint(),
                        Metadata = new Metadata()
                    },
                    Redeemer = new TokenMember()
                },
                To = new TokenMember()
            };

            var alias = member.FirstAlias().Result;
            if (alias != null)
            {
                payload.From.Alias = alias;
            }

            blobPayloads = new List<Payload>();
        }

        /// <summary>
        /// Adds a source accountId to the token.
        /// </summary>
        /// <param name="accountId">the source accountId</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetAccountId(string accountId)
        {
            var sourceAccount = new BankAccount
            {
                Token = new TokenAccount
                {
                    AccountId = accountId,
                    MemberId = member.MemberId()
                }
            };
            payload.Transfer.Instructions.Source.Account = sourceAccount;
            return this;
        }

        /// <summary>
        /// Sets the Bank Authorization.
        /// </summary>
        /// <param name="bankAuthorization">the bank authorization</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetBankAuthorization(BankAuthorization bankAuthorization)
        {
            var sourceAccount = new BankAccount
            {
                TokenAuthorization = new TokenAuthorization
                {
                    Authorization = bankAuthorization
                }
            };
            payload.Transfer.Instructions.Source.Account = sourceAccount;
            return this;
        }

        /// <summary>
        /// Sets the expiration date.
        /// </summary>
        /// <param name="expiresAtMs">the expiration date in ms.</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetExpiresAtMs(long expiresAtMs)
        {
            payload.ExpiresAtMs = expiresAtMs;
            return this;
        }

        /// <summary>
        /// Sets the effective date.
        /// </summary>
        /// <param name="effectiveAtMs">the effective date in ms.</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetEffectiveAtMs(long effectiveAtMs)
        {
            payload.EffectiveAtMs = effectiveAtMs;
            return this;
        }

        /// <summary>
        /// Sets the time after which endorse is no longer possible.
        /// </summary>
        /// <param name="endorseUntilMs">endorse until, in milliseconds.</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetEndorseUntilMs(long endorseUntilMs)
        {
            payload.EndorseUntilMs = endorseUntilMs;
            return this;
        }

        /// <summary>
        /// Sets the maximum amount per charge.
        /// </summary>
        /// <param name="chargeAmount">the charge amount</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetChargeAmount(double chargeAmount)
        {
            payload.Transfer.Amount = Convert.ToString(chargeAmount, CultureInfo.InvariantCulture);
            return this;
        }

        /// <summary>
        /// Sets the description.
        /// </summary>
        /// <param name="description">the description</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetDescription(string description)
        {
            payload.Description = description;
            return this;
        }

        /// <summary>
        /// Adds a transfer source.
        /// </summary>
        /// <param name="source">the source</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetSource(TransferEndpoint source)
        {
            payload.Transfer.Instructions.Source = source;
            return this;
        }

        /// <summary>
        /// Adds a transfer destination.
        /// </summary>
        /// <param name="destination">the destination</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder AddDestination(TransferEndpoint destination)
        {
            payload.Transfer.Instructions.Destinations.Add(destination);
            return this;
        }

        /// <summary>
        /// Sets the alias of the redeemer.
        /// </summary>
        /// <param name="redeemerAlias">the alias</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetRedeemerAlias(Alias redeemerAlias)
        {
            payload.Transfer.Redeemer.Alias = redeemerAlias;
            return this;
        }

        /// <summary>
        /// Sets the memberId of the redeemer.
        /// </summary>
        /// <param name="redeemerMemberId">the member id</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetRedeemerMemberId(string redeemerMemberId)
        {
            payload.Transfer.Redeemer.Id = redeemerMemberId;
            return this;
        }

        /// <summary>
        /// Adds an attachment to the token.
        /// </summary>
        /// <param name="attachment">the attachment</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder AddAttachment(Attachment attachment)
        {
            payload.Transfer.Attachments.Add(attachment);
            return this;
        }

        /// <summary>
        /// Adds an attachment by filename (reads file, uploads it, and attaches it).
        /// </summary>
        /// <param name="ownerId">the owner id</param>
        /// <param name="type">the MIME type of file</param>
        /// <param name="name">the name of the file</param>
        /// <param name="data">file binary data</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder AddAttachment(
            string ownerId,
            string type,
            string name,
            byte[] data)
        {
            blobPayloads.Add(new Payload
            {
                OwnerId = ownerId,
                Type = type,
                Name = name,
                Data = ByteString.CopyFrom(data)
            });
            return this;
        }

        /// <summary>
        /// Sets the alias of the payee.
        /// </summary>
        /// <param name="toAlias">the alias</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetToAlias(Alias toAlias)
        {
            payload.To.Alias = toAlias;
            return this;
        }

        /// <summary>
        /// Sets the memberId of the payee.
        /// </summary>
        /// <param name="toMemberId">the member id</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetToMemberId(string toMemberId)
        {
            payload.To.Id = toMemberId;
            return this;
        }

        /// <summary>
        /// Sets the referenceId of the token.
        /// </summary>
        /// <param name="refId">the reference ID</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetRefId(string refId)
        {
            payload.RefId = refId;
            return this;
        }

        /// <summary>
        /// Sets the pricing (fees/fx) on the token.
        /// </summary>
        /// <param name="pricing">the pricing</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetPricing(Pricing pricing)
        {
            payload.Transfer.Pricing = pricing;
            return this;
        }

        /// <summary>
        /// Sets the purpose of payment.
        /// </summary>
        /// <param name="purposeOfPayment">the purpose of payment</param>
        /// <returns>the builder</returns>
        public TransferTokenBuilder SetPurposeOfPayment(PurposeOfPayment purposeOfPayment)
        {
            payload.Transfer.Instructions.Metadata.TransferPurpose = purposeOfPayment;
            return this;
        }

        /// <summary>
        /// Builds a token payload, without uploading blobs or attachments.
        /// </summary>
        /// <returns>the token payload</returns>
        /// <exception cref="TokenArgumentsException"></exception>
        public TokenPayload BuildPayload()
        {
            if (payload.To.Id == null && payload.To.Alias == null)
            {
                throw new TokenArgumentsException("No payee on token request");
            }

            return payload;
        }

        /// <summary>
        /// Executes the request, creating a token.
        /// </summary>
        /// <returns>the token</returns>
        public Token Execute()
        {
            return ExecuteAsync().Result;
        }

        /// <summary>
        /// Executes the request asynchronously.
        /// </summary>
        /// <returns>the token</returns>
        /// <exception cref="TokenArgumentsException"></exception>
        public Task<Token> ExecuteAsync()
        {
            var sourceCase = payload.Transfer.Instructions.Source.Account.AccountCase;
            if (!sourceTypes.Contains(sourceCase))
            {
                throw new TokenArgumentsException("No source on token");
            }

            if (payload.Transfer.Redeemer.Id == null && payload.Transfer.Redeemer.Alias == null)
            {
                throw new TokenArgumentsException("No redeemer on token");
            }

            if (payload.RefId == null)
            {
                logger.Warn("refId is not set. A random ID will be used.");
                payload.RefId = Util.Nonce();
            }

            var attachmentUploads = blobPayloads.Select(payload => member.CreateBlob(
                payload.OwnerId,
                payload.Type,
                payload.Name,
                payload.Data.ToByteArray())).ToList();
            return Task.WhenAll(attachmentUploads)
                .FlatMap(attachments =>
                {
                    payload.Transfer.Attachments.AddRange(attachments);
                    return member.CreateTransferToken(payload);
                });
        }
    }
}
