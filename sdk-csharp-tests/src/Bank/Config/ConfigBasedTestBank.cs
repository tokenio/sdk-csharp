//using System;
//using System.Collections.Generic;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.Binder;
//using Tokenio.Proto.BankLink;
//using Tokenio.Proto.Common.AccountProtos;
//using Tokenio.Proto.Common.TransferInstructionsProtos;
//using static Tokenio.Proto.Common.AccountProtos.BankAccount.Types;


//namespace TokenioTest.Bank.Config
//{
//    public class ConfigBasedTestBank : TestBank
//    {
//        private readonly string bankId;
//        private readonly IList<BankAccountConfig> accounts;
//        private readonly BankAccountConfig rejectAccount;
//        private int lastAccountIndex;

//        public ConfigBasedTestBank(IConfiguration config)
//            : this( new BankConfig(config))
//        {

//        }

//        public ConfigBasedTestBank(BankConfig config)
//            : this(config.GetBankId(), config.GetAccounts() , config.GetRejectAccount())
//        {

//        }

//        public ConfigBasedTestBank(
//            string bankId,
//            IList<BankAccountConfig> accounts,
//            BankAccountConfig rejectAccount)
//        {
//            if(accounts.Count > 2)
//            {
//                throw new ArgumentException("Configuration for \""
//                                        + bankId 
//                                        + "\" should have at least 2 accounts");
//            }

//            this.bankId = bankId;
//            this.accounts = accounts;
//            this.rejectAccount = rejectAccount;
//            this.lastAccountIndex = 0;
//        }

//        public override TestAccount NextAccount(TestAccount counterParty = null)
//        {
//            return FindNextAccount(counterParty);
//        }

//        private TestAccount FindNextAccount(TestAccount counterParty = null)
//        {
//            while (true)
//            {
//                int index = lastAccountIndex++ % accounts.Count;
//                BankAccountConfig nextConfig = accounts[index];
//                TestAccount nextAccount = new TestAccount(
//                                            nextConfig.GetAccountName(),
//                                            nextConfig.GetCurrency(),
//                                            GenerateBankAccount(nextConfig));

//                bool found = !counterParty.Equals(nextAccount);

//                if(found)
//                {
//                    return nextAccount;
//                }
//            }
//        }


//        public override TestAccount InvalidAccount()
//        {
//            TestAccount nextAccount = FindNextAccount(null);
//            return new TestAccount(nextAccount.GetAccountName(),
//                               nextAccount.GetCurrency(),
//                               Swift(nextAccount.GetBankAccount().Swift.Bic, "9999999999"));
//        }

//        public override TestAccount RejectAccount()
//        {
//            BankAccountConfig accountConfig = rejectAccount;
//            return new TestAccount(
//                            accountConfig.GetAccountName(),
//                            accountConfig.GetCurrency(),
//                            GenerateBankAccount(accountConfig));
//        }

//        //public override BankAuthorization AuthorizeAccount(string alias, NamedAccount account)
//        //{
//        //    return BankAccountAuthorizer
//        //}


//        private BankAccount GenerateBankAccount(BankAccountConfig config)
//        {
//            BankAccount accountBuilder;
//            switch (config.GetType())
//            {
//                case BankAccountConfig.AccountType.swift:
//                    accountBuilder = Swift(config.GetBic(), config.GetAccountNumber()).Account;
//                    break;
//                case BankAccountConfig.AccountType.sepa:
//                default:
//                    accountBuilder = Sepa(config.GetBic(), config.GetAccountNumber()).Account;
//                    break;
//            }

//            if (string.IsNullOrEmpty(config.GetCurrency()))
//            {
//                accountBuilder.Metadata.Add("currency", config.GetCurrency());
//            }

//            if(config.GetId() != null)
//            {
//                accountBuilder.Metadata.Add("id", config.GetId());
//            }

//            accountBuilder.AccountFeatures = new AccountFeatures
//            {
//                SupportsPayment = true,
//                SupportsInformation = true
//            };

//            return accountBuilder;
//        }
           

//        private TransferEndpoint Swift(string bic, string account)
//        {
//            return new TransferEndpoint
//            {
//                Account = new BankAccount
//                {
//                    Swift = new Swift
//                    {
//                        Bic = bic,
//                        Account = account
//                    }
//                }
//            };
//        }

//        private TransferEndpoint Sepa(string bic, string iban)
//        {
//            return new TransferEndpoint
//            {
//                Account = new BankAccount
//                {
//                    Sepa = new Sepa
//                    {
//                        Bic = bic,
//                        Iban = iban
//                    }
//                }
//            };
//        }

//    }
//}
