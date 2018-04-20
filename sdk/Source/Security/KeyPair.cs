using Io.Token.Proto.Common.Security;

namespace sdk.Security
{
    public class KeyPair
    {
        public KeyPair(string id, Key.Types.Level level, Key.Types.Algorithm algorithm, byte[] privateKey, byte[] publicKey)
        {
            Id = id;
            Level = level;
            Algorithm = algorithm;
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        public string Id { get; }
        
        public Key.Types.Level Level { get; }

        public Key.Types.Algorithm Algorithm { get; }

        public byte[] PrivateKey { get; }
        
        public byte[] PublicKey { get; }
    }
}
