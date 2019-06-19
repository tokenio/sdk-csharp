using System;
using System.Collections.Generic;
using Tokenio.Proto.BankLink;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Security;
using Tokenio.User;
using TokenioTest.Bank;
using Tokenio.Proto.Gateway;
using Account = Tokenio.User.Account;
using BankProtos = Tokenio.Proto.Common.BankProtos.Bank;
using Member = Tokenio.User.Member;
using Io.Token.Proto.Gateway.Testing;
using Tokenio.User.Utils;
using TokenClient = Tokenio.User.TokenClient;
using Tokenio;
namespace TokenioTest.Common
{
    public class TokenUserRule : TokenRule
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TokenUserRule));
        public TokenUserRule()
        : base()
        {
        }

        public TokenUserRule(string bankId)
        : base(bankId)
        {
        }

        public override Tokenio.TokenClient NewSdkInstance(params string[] featureCodes)
        {
            return NewSdkInstance(null, featureCodes);
        }

        public override Tokenio.TokenClient NewSdkInstance(ICryptoEngineFactory crypto, params string[] featureCodes)
        {
            Enum.TryParse(
               Environment.GetEnvironmentVariable("TOKEN_ENV") ?? "sandbox",
               true,
               out Tokenio.TokenCluster.TokenEnv tokenEnv);

            return TokenClient.NewBuilder()
                    .ConnectTo(Tokenio.TokenCluster.GetCluster(tokenEnv))
                    .HostName(envConfig.GetGateway().Host)
                    .Port(envConfig.GetGateway().Port)
                    .Timeout(timeoutMs)
                    .WithCryptoEngine(crypto)
                    .DeveloperKey(envConfig.GetDevKey())
                    .Build();


        }

        public string GetBankId()
        {
            return envConfig.GetBankId();
        }

        public Member NoAliasMember()
        {
            return Member(CreateMemberType.Personal, null);
        }

        public Member Member()
        {
            return Member(CreateMemberType.Personal);
        }

        public Member Member(CreateMemberType type)
        {
            return Member(type, Sample.alias());
        }

        public Member Member(Alias alias)
        {
            return Member(CreateMemberType.Personal, alias);
        }

       

        public TokenClient Token()
        {
            return (Tokenio.User.TokenClient)tokenClient;
        }

        public Member Member(CreateMemberType type,Alias alias=null)
        {
            CreateTestMemberRequest request = new CreateTestMemberRequest() { 
            
                MemberType=type,
                Nonce = Util.Nonce(),
                PartnerId="",

            };
            string mmemberId = testingGateway.CreateTestMember(request).MemberId;
            return Token().SetUpMemberBlocking(mmemberId, alias);

            //Member mem = null; 
            //var memberId = Token().GetMemberIdBlocking(alias);
            //if (string.IsNullOrEmpty(memberId)){

            //    var email = "Testcsharp_noverify@example.com";
            //    var memberAlias = new Alias
            //    {
            //        Value = email,
            //        Type = Alias.Types.Type.Email
            //    };
            //    memberId = Token().GetMemberIdBlocking(memberAlias);
            //    if (string.IsNullOrEmpty(memberId))
            //    {
            //        mem = Token().CreateMemberBlocking(memberAlias);
            //        memberId = mem.MemberId();

            //    }

            //}

            //return mem;

        }



        public LinkedAccount LinkedAccount()
        {
            return LinkAccount(testBank.NextAccount(null));
        }

        private LinkedAccount LinkAccount(TestAccount testAccount)
        {
            return LinkAccount(testAccount, Member());
        }


        public LinkedAccount LinkedAccount(LinkedAccount counterParty=null)
        {
            return LinkAccount(testBank.NextAccount(counterParty.TestAccount()));
        }

        public LinkedAccount LinkedAccount(Member member)
        {
            return LinkAccount(testBank.NextAccount(null), member);
        }

        public LinkedAccount InvalidLinkedAccount()
        {
            return LinkAccount(testBank.InvalidAccount());
        }


        public LinkedAccount RejectLinkedAccount()
        {
            return LinkAccount(testBank.RejectAccount());
        }

        public TestAccount UnlinkedAccount(LinkedAccount counterParty = null)
        {
            return testBank.NextAccount(counterParty.TestAccount());
        }

        public LinkedAccount RelinkAccount(LinkedAccount account)
        {
            return LinkAccount(account.TestAccount());
        }

        public IList<BankProtos> GetBanks()
        {
            return tokenClient.GetBanksBlocking().Banks;
        }



        private LinkedAccount LinkAccount(TestAccount testAccount, Member member)
        {
            //Thread.Sleep(2*1000);
            BankAuthorization auth = testBank.AuthorizeAccount(
                    member.MemberId(),
                    new NamedAccount(testAccount.GetBankAccount(), testAccount.GetAccountName()));
            Account account = member
                    .LinkAccountsBlocking(auth)[0];
            return new LinkedAccount(testAccount, account);
        } 



    }
}
