using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using log4net;
using System.Reflection;
using System.IO;


namespace TokenioTest.Common
{
    public class EnvConfig
    {
        private readonly bool useSsl ;
        private readonly string bankId;
        private readonly DnsEndPoint gateway;
        private readonly string devKey;

     

        public EnvConfig(IConfiguration config)
        {
            this.useSsl = bool.Parse(config["use-ssl"]);
            this.bankId = config["bank-id"];
            var gatewaySection = config.GetSection("gateway");
            this.gateway = new DnsEndPoint(gatewaySection["host"], int.Parse(gatewaySection["port"])); 
            this.devKey = config["dev-key"];

            FileInfo f = new FileInfo("log4net.config"); //please modify this line
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, f);
        }

        public bool UseSsl()
        {
            return useSsl;
        }

        public string GetBankId()
        {
            return bankId;
        }

        public DnsEndPoint GetGateway()
        {
            return gateway;
        }

        public string GetDevKey()
        {
            return devKey;
        }

        private static Regex GlobToPattern(string glob)
        {
            StringBuilder pattern = new StringBuilder();
            foreach (char c in glob)
            {
                switch (c)
                {
                    case '.':
                        pattern.Append("\\.");
                        break;
                    case '*':
                        pattern.Append(".*");
                        break;
                    default:
                        pattern.Append(c);
                        break;
                }
            }
            return new Regex(pattern.ToString());
        }

    }
}
