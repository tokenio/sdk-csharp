using System;
using System.Threading.Tasks;
using Tokenio;
using Tokenio.Proto.Common.AddressProtos;
using Tokenio.Proto.Common.AliasProtos;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;

namespace Test
{
    public class TestUtil
    {
        public static Alias Alias()
        {
            return new Alias
            {
                Value = Util.Nonce() + "+noverify@example.com",
                Type = Email
            };
        }

        public static Address Address()
        {
            return new Address
            {
                HouseNumber = "425",
                Street = "Broadway",
                City = "Redwood City",
                PostCode = "94063",
                Country = "US"
            };
        }

        public static TokenIO NewSdkInstance()
        {
            var tokenEnv = Environment.GetEnvironmentVariable("TOKEN_ENV") ?? "development";
            return TokenIO.NewBuilder()
                .ConnectTo(TokenCluster.DEVELOPMENT)
                .HostName("api-grpc.dev.token.io")
                .Port(443)
                .Timeout(10 * 60 * 1_000) // Set high for easy debugging.
                .DeveloperKey("4qY7lqQw8NOl9gng0ZHgT4xdiDqxqoGVutuZwrUYQsI")
                .Build();
        }
        
        public static async Task WaitUntil(
            int timeoutMs,
            int waitTimeMs,
            Action action) {
            for (var start = Util.EpochTimeMillis(); ;) {
                try {
                    action.Invoke();
                    return;
                } catch (Exception caughtError) {
                    if (Util.EpochTimeMillis() - start < timeoutMs)
                    {
                        await Task.Delay(waitTimeMs);
                    } else {
                        throw caughtError;
                    }
                }
            }
        }
    }
}
