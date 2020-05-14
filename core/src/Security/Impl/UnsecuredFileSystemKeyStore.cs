using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Tokenio.Exceptions;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security
{
    /// <summary>
    /// A key store that uses the local file system for persistent storage.
    /// Keys are stored in a single root directory, with a subdirectory containing each member's keys.
    /// No support is provided for security of key files.
    /// </summary>
    public class UnsecuredFileSystemKeyStore : IKeyStore
    {
        private readonly IDictionary<string, IList<KeyPair>> keys;
        private readonly string directory;

        /// <summary>
        /// Creates a new key store.
        /// </summary>
        /// <param name="directory">Directory.</param>
        public UnsecuredFileSystemKeyStore(string directory)
        {
            this.directory = directory;
            keys = new Dictionary<string, IList<KeyPair>>();

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var files = Directory.GetFiles(directory);

            foreach (var file in files)
            {
                var memberId = Path.GetFileName(file).Replace('_', ':');
                var content = File.ReadAllText(file);
                var memberKeys = JsonConvert.DeserializeObject<List<KeyPair>>(content);
                keys[memberId] = memberKeys;
            }
        }

        public void Put(string memberId, KeyPair keyPair)
        {
            if (keyPair.IsExpired())
            {
                throw new ArgumentException("Key " + keyPair.Id + " has expired");
            }
            var filePath = Path.Combine(directory, memberId.Replace(':', '_'));

            if (keys.ContainsKey(memberId))
            {
                keys[memberId].Add(keyPair);
            }
            else
            {
                keys[memberId] = new List<KeyPair> { keyPair };
                var newFile = File.Create(filePath);
                newFile.Close();
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(keys[memberId]));
        }

        public KeyPair GetByLevel(string memberId, Level level)
        {
            try
            {
                var key = keys[memberId].Last(k => k.Level.Equals(level));
                if (key.IsExpired())
                    throw new CryptoKeyNotFoundException("Key not found for level: " + level);
                return key;
            }
            catch (Exception)
            {
                throw new CryptoKeyNotFoundException(level);
            }
        }

        public KeyPair GetById(string memberId, string keyId)
        {
            try
            {
                var key = keys[memberId].First(k => k.Id.Equals(keyId));
                if (key == null)
                    throw new CryptoKeyNotFoundException("Key not found for id: " + keyId);
                if (key.IsExpired())
                    throw new CryptoKeyNotFoundException("Key with id: " + keyId + "has expired");
                return key;
            }
            catch (Exception)
            {
                throw new CryptoKeyNotFoundException("Key not found for id: " + keyId);
            }
        }

        /// <summary>
        /// Get all of a member's keys.
        /// </summary>
        /// <returns>The keys.</returns>
        /// <param name="memberId">Member identifier.</param>
        public IList<KeyPair> KeyList(string memberId)
        {
            return keys[memberId].Where(key => !key.IsExpired()).ToList();

        }
    }
}
