using System;
using Tokenio.Proto.Common.AccountProtos;
using AccountCase = Tokenio.Proto.Common.AccountProtos.BankAccount.AccountOneofCase;

namespace TokenioTest.Bank
{
    public class NamedAccount
    {
        private BankAccount bankAccount;
        private readonly string displayName;

  
        public NamedAccount(BankAccount bankAccount, string displayName)
        {
            if(displayName.Length == 0)
            {
                throw new NullReferenceException(); 
            }   
            this.bankAccount = ValidateBankAccount(bankAccount);
            this.displayName = displayName;
        }

        public BankAccount GetBankAccount()
        {
            return bankAccount;
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        private static BankAccount ValidateBankAccount(BankAccount bankAccount)
        {
            AccountCase accountCase  = bankAccount.AccountCase;
            if (accountCase == AccountCase.Token || accountCase == AccountCase.TokenAuthorization)
            {
                throw new ArgumentException(
                        "Invalid account value. Token and TokenAuthorization are reserved types.");
            }
            return bankAccount;
        }
    }
}
