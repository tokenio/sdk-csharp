using System;
using System.Threading;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Tokenio;
using Tokenio.Proto.Common.AddressProtos;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;

namespace Test
{
    public class TestUtil
    {
        private static readonly IAsymmetricCipherKeyPairGenerator generator;

        static TestUtil()
        {
            generator = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
            generator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
        }

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
            Enum.TryParse(
                Environment.GetEnvironmentVariable("TOKEN_ENV") ?? "development",
                true,
                out TokenCluster.TokenEnv tokenEnv);
            
            return TokenIO.NewBuilder()
                .ConnectTo(TokenCluster.GetCluster(tokenEnv))
                .Port(443)
                .Timeout(10 * 60 * 1_000) // Set high for easy debugging.
                .DeveloperKey("4qY7lqQw8NOl9gng0ZHgT4xdiDqxqoGVutuZwrUYQsI")
                .Build();
        }

        public static KeyPair GenerateKeyPair(Key.Types.Level level)
        {
            return generator.GenerateKeyPair().ParseEd25519KeyPair(level);
        }

        public static void WaitUntil(
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
                        Thread.Sleep(waitTimeMs);
                    } else {
                        throw caughtError;
                    }
                }
            }
        }
    }
}
