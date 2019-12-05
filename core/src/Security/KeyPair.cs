using System.Linq;
using Tokenio.Utils;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security
{
    /// <summary>
    /// Encapsulates Key pair.
    /// </summary>
    public class KeyPair
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Tokenio.Security.KeyPair"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="level">Level.</param>
        /// <param name="algorithm">Algorithm.</param>
        /// <param name="privateKey">Private key.</param>
        /// <param name="publicKey">Public key.</param>
        /// <param name="expiresAtMs">Expires at ms.</param>
        public KeyPair(
           string id,
           Level level,
           Algorithm algorithm,
           byte[] privateKey,
           byte[] publicKey,
           long expiresAtMs = 0)
        {
            Id = id;
            Level = level;
            Algorithm = algorithm;
            PrivateKey = privateKey;
            PublicKey = publicKey;
            ExpiresAtMs = expiresAtMs;
        }

        public string Id { get; }

        public Level Level { get; }

        public Algorithm Algorithm { get; }

        public byte[] PrivateKey { get; }

        public byte[] PublicKey { get; }

        public long ExpiresAtMs { get; }

        /// <summary>
        /// Checks whether a key has expired.
        /// </summary>
        /// <returns><c>true</c>, if expired was ised, <c>false</c> otherwise.</returns>
        public bool IsExpired()
        {

            return ExpiresAtMs != 0 && ExpiresAtMs < Util.CurrentMillis();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = (KeyPair)obj;

            return Id.Equals(other.Id)
                   && Level.Equals(other.Level)
                   && Algorithm.Equals(other.Algorithm)
                   && PrivateKey.SequenceEqual(other.PrivateKey)
                   && PublicKey.SequenceEqual(other.PublicKey);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
