using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;

namespace Test {
    public static class TestUtil {
        private static readonly IAsymmetricCipherKeyPairGenerator ed255519KeyGen;

        static TestUtil() {
            ed255519KeyGen = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
            ed255519KeyGen.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
        }

        public static Alias Alias() {
            return new Alias {
                // use uppercase to test normalization
                Value = Util.Nonce().ToUpper() + "+noverify@example.com",
                    Type = Email,
                    Realm = "token"
            };
        }

        public static TokenClient NewSdkInstance() {
            Enum.TryParse(
                Environment.GetEnvironmentVariable("TOKEN_ENV") ?? "development",
                true,
                out TokenCluster.TokenEnv tokenEnv);

            return TokenClient.NewBuilder()
                .ConnectTo(TokenCluster.GetCluster(tokenEnv))
                .Port(443)
                .Timeout(10 * 60 * 1_000) // Set high for easy debugging.
                .DeveloperKey("4qY7lqQw8NOl9gng0ZHgT4xdiDqxqoGVutuZwrUYQsI")
                .Build();
        }

        public static KeyPair GenerateKeyPair(Key.Types.Level level) {
            return ed255519KeyGen.GenerateKeyPair().ParseEd25519KeyPair(level);
        }
    }
}
