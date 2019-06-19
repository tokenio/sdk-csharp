using System;
using System.Collections.Generic;
using System.Net;
using Io.Token.Proto.Bankapi;
using Microsoft.Extensions.Configuration;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.AccountProtos;
using Tokenio.Proto.Common.TransferInstructionsProtos;
using static Tokenio.Proto.Common.AccountProtos.BankAccount.Types;

namespace TokenioTest.Bank.Fank
{
    public class FankTestBank : TestBank
    {
        private static readonly string CURRENCY = "EUR";
        private static readonly string CLIENT_ID_KEY = "CLIENT_ID";
        private readonly DateTime clock = DateTime.UtcNow;
        private readonly FankClient fank;
        private string bic;

        public FankTestBank(IConfiguration config)
            : this(new FankConfig(config))
        {
        }

        public FankTestBank(FankConfig fankConfig)
        : this(fankConfig.GetBic(), fankConfig.GetFank(), fankConfig.UseSsl())
        { 
        }

        public FankTestBank(string bic, DnsEndPoint fank, bool useSsl)
        {
            this.bic = bic;
            this.fank = new FankClient(
                    fank.Host,
                    fank.Port,
                    useSsl);
        }

        public override TestAccount NextAccount(TestAccount counterParty = null)
        {
            string accountName = "Test Account";
            string bankAccountNumber = "iban:" + (long)(clock - new DateTime(1970, 1, 1)).TotalMilliseconds + RandomNumeric(7);
            Client client = NewClient();
            fank.AddAccount(
                    client,
                    accountName,
                    bic,
                    bankAccountNumber,
                    1000000.00,
                    CURRENCY);
            return new TestAccount(
                    accountName,
                    CURRENCY,
                    SwiftAccount(bankAccountNumber, client.Id));
        }

        public override TestAccount InvalidAccount()
        {
            string accountName = "Invalid Account";
            string bankAccountNumber = "invalid:" + (long)(clock - new DateTime(1970, 1, 1)).TotalMilliseconds + RandomNumeric(7);
            Client client = NewClient();
            fank.AddAccount(
                    client,
                    accountName,
                    bic,
                    bankAccountNumber,
                    1000000.00,
                    CURRENCY);
            return new TestAccount(
                    accountName,
                    CURRENCY,
                    SwiftAccount(bankAccountNumber, client.Id));
        }

        public override TestAccount RejectAccount()
        {
            string accountName = "Reject Account";
            string bankAccountNumber = "reject:" + (long)(clock - new DateTime(1970, 1, 1)).TotalMilliseconds + RandomNumeric(7);
            Client client = NewClient();
            fank.AddAccount(
                    client,
                    accountName,
                    bic,
                    bankAccountNumber,
                    1000000.00,
                    CURRENCY);
            return new TestAccount(
                    accountName,
                    CURRENCY,
                    SwiftAccount(bankAccountNumber, client.Id));
        }


        public override BankAuthorization AuthorizeAccount(string alias, NamedAccount account)
        {
            string clientId = account.GetBankAccount()
                .Metadata[CLIENT_ID_KEY];

            IList<string> list = new List<string>();
            list.Add(account.GetBankAccount().Swift.Account);

            return fank.StartAccountsLinking(
                    alias,
                    clientId,
                    account.GetBankAccount().Swift.Bic,
                    list).Result;
        }

        private Client NewClient()
        {
            return fank.AddClient(bic, "Test " + RandomNumeric(15), "Testoff").Result;
        }

        private BankAccount SwiftAccount(string bankAccountNumber, string clientId)
        {
            var account = Swift(bic, bankAccountNumber).Account;
            account.Metadata.Add(CLIENT_ID_KEY, clientId);
            return account;
        }

        private TransferEndpoint Swift(string bic, string account)
        {
            return new TransferEndpoint
            {
                Account = new BankAccount
                {
                    Swift = new Swift
                    {
                        Bic = bic,
                        Account = account
                    }
                }
            };
        }

        private string RandomNumeric(int size) 
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, size);
        }
    }
}
