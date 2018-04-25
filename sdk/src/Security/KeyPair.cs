using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security
{
    public class KeyPair
    {
        public KeyPair(
            string id,
            Level level,
            Algorithm algorithm,
            byte[] privateKey,
            byte[] publicKey)
        {
            Id = id;
            Level = level;
            Algorithm = algorithm;
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        public string Id { get; }
        
        public Level Level { get; }

        public Algorithm Algorithm { get; }

        public byte[] PrivateKey { get; }
        
        public byte[] PublicKey { get; }
    }
}
