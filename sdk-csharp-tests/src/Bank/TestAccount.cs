using Tokenio.Proto.Common.AccountProtos;


namespace TokenioTest.Bank
{
    public class TestAccount
    {
        private readonly string accountName;
        private readonly string currency;
        private readonly BankAccount bankAccount;

        public TestAccount(string accountName, string currency, BankAccount bankAccount)
        {
            this.accountName = accountName;
            this.currency = currency;
            this.bankAccount = bankAccount;
        }

        public string GetAccountName()
        {
            return accountName;
        }

        public string GetCurrency()
        {
            return currency;
        }

        public BankAccount GetBankAccount()
        {
            return bankAccount;
        }

        public NamedAccount ToNamedAccount()
        {
            return new NamedAccount(bankAccount, accountName);
        }
    }
}
