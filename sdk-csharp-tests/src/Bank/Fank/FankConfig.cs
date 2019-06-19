using System;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace TokenioTest.Bank.Fank
{
    public sealed class FankConfig
    {
        private readonly IConfiguration config;

        public FankConfig(IConfiguration config)
        {
            this.config = config;
        }

        public bool UseSsl()
        {
            return bool.Parse(config["use-ssl"]);
        }

        public string GetBic()
        {
            return config["bank-bic"];
        }

        public DnsEndPoint GetFank()
        {
            var fank = config.GetSection("fank");
            return new DnsEndPoint(
                    fank["host"],
                    int.Parse(fank["port"]));
        }
    }
}
