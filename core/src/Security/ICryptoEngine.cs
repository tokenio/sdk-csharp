using Tokenio.Proto.Common.SecurityProtos;
using System.Collections.Generic;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security
{
    public interface ICryptoEngine
    {
        /// <summary>
        /// Generates keys of the specified level. If the key with the specified level
        /// already exists, it is replaced. Old key is still kept around because it could be
        /// used for signature verification later.
        /// </summary>
        /// <param name="level">the key level</param>
        /// <returns>the generated key</returns>
        Key GenerateKey(Level level);

        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <returns>The key.</returns>
        /// <param name="level">Level.</param>
        /// <param name="expiresAtMs">Expires at ms.</param>
        Key GenerateKey(Level level,long expiresAtMs);

        /// <summary>
        /// Create a signer that signs data with the latest generated key of the specified level.
        /// </summary>
        /// <param name="level">the key level</param>
        /// <returns>the signer</returns>
        ISigner CreateSigner(Level level);
        
        /// <summary>
        /// Create a verifier that verifies signatures with a specific key.
        /// </summary>
        /// <param name="keyId">the key id</param>
        /// <returns>the verifier</returns>
        IVerifier CreateVerifier(string keyId);

        /// <summary>
        /// Returns public keys that the CryptoEngine can use to sign.
        /// </summary>
        /// <returns>The public keys.</returns>
        IList<Key> GetPublicKeys();
    }
}

