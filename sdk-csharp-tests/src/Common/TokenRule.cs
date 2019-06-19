using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Tokenio.Security;
using TokenioTest.Bank;
using Tokenio.Rpc;
using Tokenio;
using Io.Token.Proto.Gateway.Testing;
namespace TokenioTest.Common
{
    public abstract class TokenRule
    {
        protected static readonly long timeoutMs = 10*60*1000;

        protected readonly EnvConfig envConfig;
        protected readonly TokenClient tokenClient;
        protected readonly TestBank testBank;



        protected readonly ManagedChannel testingGatewayChannel;
        protected readonly  TestingGatewayService.TestingGatewayServiceClient testingGateway;
        //protected readonly MockServiceClient mockClient;

        public TokenRule()
        : this(new ConfigurationBuilder()
        //.SetBasePath(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName)
        .AddJsonFile("resources/sandbox.json")
        .Build())
        {
        }


        public TokenRule(string bankId)
        : this(new ConfigurationBuilder()
        .AddJsonFile(string.Format("resources/{0}-{1}.json", "sandbox", bankId))
        .Build())
        {

        }


        private TokenRule(IConfiguration config)
        {
            this.envConfig = new EnvConfig(config);
            this.testBank = TestBank.Create(config);
            this.tokenClient = NewSdkInstance();

            DnsEndPoint hostAndPort = this.envConfig.GetGateway();
            var channel = new Channel(hostAndPort.Host, hostAndPort.Port, this.envConfig.UseSsl() ? new SslCredentials() : ChannelCredentials.Insecure);
            Interceptor[] interceptors =
                {
                    new AsyncTimeoutInterceptor(timeoutMs)
                };
            channel.Intercept(interceptors);

            this.testingGatewayChannel = new ManagedChannel(channel, interceptors);
            this.testingGateway = new TestingGatewayService.TestingGatewayServiceClient(channel);
        }

        public abstract TokenClient NewSdkInstance(params string[] featureCodes);

        public abstract TokenClient NewSdkInstance(ICryptoEngineFactory crypto, params string[] featureCodes);

        public static bool GetEnvProperty(string name, string defaultValue)
        {
           return Enum.TryParse(Environment.GetEnvironmentVariable(name) ?? defaultValue,
                true,
                out Tokenio.TokenCluster.TokenEnv tokenEnv); ;
           
        }

    }
}
