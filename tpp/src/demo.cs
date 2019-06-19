//using System;
//using Tokenio.Tpp;
//namespace Tokenio.Tpp.src
//{
//    public class demo
//    {
//        public demo()
//        {
           

//        }
//        public static void Main(string[] args)
//        {

//            Enum.TryParse(
//                Environment.GetEnvironmentVariable("TOKEN_ENV") ?? "development",
//                true,
//                out Tokenio.TokenCluster.TokenEnv tokenEnv);

//            TokenClient  client=Tokenio.Tpp.TokenClient.Create(Tokenio.TokenCluster.GetCluster(tokenEnv), "4qY7lqQw8NOl9gng0ZHgT4xdiDqxqoGVutuZwrUYQsI");

//            var bankId = client.GetBanksBlocking().Banks[0].Id;
//            var member = client.CreateMemberBlocking();
//            Console.WriteLine(member);
//        }
//    }
//}
