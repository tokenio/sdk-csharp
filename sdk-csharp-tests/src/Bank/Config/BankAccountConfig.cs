using System;
using Microsoft.Extensions.Configuration;

namespace TokenioTest.Bank.Config
{
    public class BankAccountConfig
    {
        private readonly string accountName;
        private readonly string bic;
        private readonly string accountNumber;
        private readonly string currency;
        private readonly AccountType type;
        private readonly string id;

        public BankAccountConfig(IConfiguration config) 
            : this(config["name"],
                   config["bic"],
                   config["number"], 
                   config["currency"],
                   (AccountType)Enum.Parse(typeof(AccountType), config["type"]),
                   config["id"] ?? null)
        {

        }


        public BankAccountConfig(
            string accountName,
            string bic,
            string accountNumber,
            string currency,
            AccountType type,
            string id)
        {
            this.accountName = accountName;
            this.bic = bic;
            this.accountNumber = accountNumber;
            this.currency = currency;
            this.type = type;
            this.id = id;
        }

        public string GetAccountName()
        {
            return accountName;
        }

        public string GetBic()
        {
            return bic;
        }

        public string GetAccountNumber()
        {
            return accountNumber;
        }

        public string GetCurrency()
        {
            return currency;
        }

        public AccountType GetType()
        {
            return type;
        }

        public string GetId()
        {
            return id;
        }

        public enum AccountType
        {
            sepa, swift, token, ach
        }

    }
}
