using System.Collections.Generic;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security
{
    /// <summary>
    /// Provides key storage primitives.
    /// </summary>
    public interface IKeyStore
    {
        /// <summary>
        /// Puts a specified key pair into the storage.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="keyPair">the key paid</param>
        void Put(string memberId, KeyPair keyPair);

        /// <summary>
        /// Gets a key pair of a specific level.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="level">the level of the key pair</param>
        /// <returns>the key pair</returns>
        /// <exception cref="KeyNotFoundException"></exception>>
        KeyPair GetByLevel(string memberId, Level level);

        /// <summary>
        /// Gets a key pair of by its ID.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="keyId">the key id</param>
        /// <returns>the key pair</returns>
        /// <exception cref="KeyNotFoundException"></exception>>
        KeyPair GetById(string memberId, string keyId);

        /// <summary>
        /// Get all of a member's keys.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <returns>a list of key pairs</returns>
        IList<KeyPair> KeyList(string memberId);
    }
}