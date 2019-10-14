using System.Linq;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security {
    public class KeyPair {
        public KeyPair(
            string id,
            Level level,
            Algorithm algorithm,
            byte[] privateKey,
            byte[] publicKey) {
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

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            var other = (KeyPair) obj;

            return Id.Equals(other.Id)
                && Level.Equals(other.Level)
                && Algorithm.Equals(other.Algorithm)
                && PrivateKey.SequenceEqual(other.PrivateKey)
                && PublicKey.SequenceEqual(other.PublicKey);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }
    }
}
