using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using Tokenio.Utils;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;

namespace Test
{
    public static class TestUtil
    {
        private static readonly IAsymmetricCipherKeyPairGenerator ed255519KeyGen;

        static TestUtil()
        {
            ed255519KeyGen = GeneratorUtilities.GetKeyPairGenerator("Ed25519");
            ed255519KeyGen.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
        }

        public static Alias Alias()
        {
            return new Alias
            {
                // use uppercase to test normalization
                Value = Util.Nonce().ToUpper() + "+noverify@example.com",
                Type = Email,
                Realm = "token"
            };
        }
        
        public static KeyPair GenerateKeyPair(Key.Types.Level level)
        {
            return ed255519KeyGen.GenerateKeyPair().ParseEd25519KeyPair(level,Key.Types.Algorithm.Ed25519);
        }
    }
}
