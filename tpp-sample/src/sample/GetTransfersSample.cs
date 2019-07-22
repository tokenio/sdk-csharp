﻿using System;
using System.Collections.Generic;
using Tokenio.Proto.Common.TransferProtos;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;
using System.Linq;
using Tokenio.Proto.Common.TransactionProtos;
using Tokenio.Proto.Common.TokenProtos;

namespace  TokenioSample
{
    public class GetTransfersSample
    {
        /// <summary>
        /// Gets the transfer sample.
        /// </summary>
        /// <param name="payer">Payer.</param>
        public static void GetTransfers_Sample(TppMember payer)
        {
            var accounts = payer.GetAccountsBlocking();
            string accountId = accounts[0].Id();
            foreach (Transfer transfer in payer.GetTransfersBlocking(null,null,10).List)
            {

                DisplayTransfer(
                       transfer.Status,
                       transfer.Payload.Description);

            }
         
        }

        /// <summary>
        /// Gets the transfer tokens sample.
        /// </summary>
        /// <param name="payer">Payer.</param>
        public static void GetTransferTokensSample(
            TppMember payer)
        {

            foreach (Token token in payer.GetTransferTokensBlocking(null,10).List)
            {
                TransferBody transferBody = token.Payload.Transfer;
                DisplayTransferToken(
                       transferBody.Currency,
                       transferBody.LifetimeAmount);

            }

        }

        /// <summary>
        /// Gets the transfer sample.
        /// </summary>
        /// <returns>The transfer sample.</returns>
        /// <param name="payer">Payer.</param>
        /// <param name="transferId">Transfer identifier.</param>
        public static Transfer GetTransferSample(
            TppMember payer,
            string transferId)
        {
            Transfer transfer = payer.GetTransferBlocking(transferId);
            return transfer;
        }

        /// <summary>
        /// Displaies the transfer.
        /// </summary>
        /// <param name="status">Status.</param>
        /// <param name="description">Description.</param>
        private static void DisplayTransfer(
           TransactionStatus status,
           string description)
        {
        }

        /// <summary>
        /// Displaies the transfer token.
        /// </summary>
        /// <param name="currency">Currency.</param>
        /// <param name="value">Value.</param>
        private static void DisplayTransferToken(
           string currency,string value)
        {
        }
    }
}
