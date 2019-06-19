using System;
using Tokenio.Security;
using Tokenio.Tpp;
using Tokenio.Proto.Common.MemberProtos;
using Member = Tokenio.Tpp.Member;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Tpp.Utils;
using Io.Token.Proto.Gateway.Testing;
using TokenClient = Tokenio.Tpp.TokenClient;
using Tokenio;



namespace TokenioTest.Common
{
    public class TokenTppRule : TokenRule
    {
      
        public  override Tokenio.TokenClient NewSdkInstance(params string[] featureCodes)
        {
            return NewSdkInstance(null, featureCodes);
        }


        public override Tokenio.TokenClient NewSdkInstance(ICryptoEngineFactory crypto, params string[] featureCodes)
        {
            Enum.TryParse(
               Environment.GetEnvironmentVariable("TOKEN_ENV") ?? "development",
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



        public TokenClient Token()
        {
            return (Tokenio.Tpp.TokenClient)tokenClient;
        }


        public Member Member()
        {
            return Member(Sample.alias());
        }


        public Member Member(Alias alias)
        {
            return Member(CreateMemberType.Business, alias);
        }


        public Member Member(CreateMemberType type, Alias alias = null)
        {

            CreateTestMemberRequest request = new CreateTestMemberRequest()
            {

                MemberType = type,
                Nonce = Util.Nonce(),
                PartnerId = "",

            };
            string mmemberId = testingGateway.CreateTestMember(request).MemberId;
            return Token().SetUpMemberBlocking(mmemberId, alias);


            //Member mem = null;
            //var memberId = Token().GetMemberIdBlocking(alias);
            //if (string.IsNullOrEmpty(memberId))
            //{

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



        //public static Tokenio.TokenCluster.TokenEnv GetEnvProperty(string name, string defaultValue)
        //{
        //    Enum.TryParse(
        //    Environment.GetEnvironmentVariable("TOKEN_ENV") ?? "sandbox",
        //    true,
        //    out Tokenio.TokenCluster.TokenEnv tokenEnv);

        //    return tokenEnv;
        //}
    }
}
