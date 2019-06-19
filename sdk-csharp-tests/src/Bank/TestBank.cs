using System;
using Microsoft.Extensions.Configuration;
using Tokenio.Proto.BankLink;
using TokenioTest.Bank.Fank;


namespace TokenioTest.Bank
{
    public abstract class TestBank
    {
        public static TestBank Create(IConfiguration config)
        {
            if (config.GetSection("fank").Exists())
            {
                return new FankTestBank(config);
            }
            //else if (config["bank"]!=null)
            //{
            //    //return new ConfigBasedTestBank(config);
            //}
            throw new InvalidOperationException("Not supported configuration");
        }


        public abstract TestAccount NextAccount(TestAccount counterParty = null);

        public abstract TestAccount InvalidAccount();

        public abstract TestAccount RejectAccount();

        public abstract BankAuthorization AuthorizeAccount(string alias, NamedAccount account);
    }
}
