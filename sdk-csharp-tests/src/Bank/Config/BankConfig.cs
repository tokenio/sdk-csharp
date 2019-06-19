using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace TokenioTest.Bank.Config
{
    public class BankConfig
    {
        private readonly string bankId;
        private readonly IList<BankAccountConfig> accounts;
        private readonly BankAccountConfig rejectAccount;

        public BankConfig(IConfiguration config)
        {
            this.bankId = config["bank-id"];
            var temp = config.GetSection("bank:accounts").GetChildren().AsEnumerable().ToList();
            foreach (var account in temp)
            {
                accounts.Add(new BankAccountConfig(account));
            }
            this.rejectAccount = new BankAccountConfig(config.GetSection("bank:reject-account"));
        }


        public string GetBankId()
        {
            return bankId;
        }

        public IList<BankAccountConfig> GetAccounts()
        {
            return accounts;
        }

        public BankAccountConfig GetRejectAccount()
        {
            return rejectAccount;
        }

    }
}
