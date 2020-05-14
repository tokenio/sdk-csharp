using System.Collections.Generic;
using Tokenio.Proto.Common.SecurityProtos;
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
        Key GenerateKey(Level level, long expiresAtMs);

        /// <summary>
        /// Create a signer that signs data with the latest generated key of the specified level.
        /// </summary>
        /// <param name="level">the key level</param>
        /// <returns>the signer</returns>
        ISigner CreateSigner(Level level);

        /// <summary>
        /// Create a signer that signs data with a specific key.
        /// </summary>
        /// <param name="keyId">the key id</param>
        /// <returns>the signer</returns>
        ISigner CreateSigner(string keyId);

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

        /// <summary>
        /// Creates a new signer that uses a key of specified level or higher
        /// (if no key of the specified level can be found).<br>
        /// Note, that if there are several same-level keys, a random one is used to create a signer.
        /// If you need to create a signer for a specific key, create a signer using the key id.
        /// </summary>
        /// <param name="minKeyLevel">minimum level of the key to use</param>
        /// <returns>signer that is used to generate digital signatures</returns>
        ISigner CreateSignerForLevelAtLeast(Level minKeyLevel);
    }
}

