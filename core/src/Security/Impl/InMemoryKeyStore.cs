using System;
using System.Collections.Generic;
using System.Linq;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security {
    public class InMemoryKeyStore : IKeyStore {
        private readonly IDictionary<Tuple<string, string>, KeyPair> allKeys;

        public InMemoryKeyStore () {
            allKeys = new Dictionary<Tuple<string, string>, KeyPair> ();
        }

        public void Put (string memberId, KeyPair keyPair) {
            if (keyPair.IsExpired ()) {
                throw new ArgumentException ("Key " + keyPair.Id + " has expired");
            }
            allKeys[new Tuple<string, string> (memberId, keyPair.Id)] = keyPair;

        }

        public KeyPair GetByLevel (string memberId, Level level) {
            var keys = allKeys.Where (entry => entry.Key.Item1 == memberId).ToList ();
            var keyByLevel = keys.Select (entry => entry.Value).First (Key => Key.Level == level);
            if (keyByLevel.IsExpired ()) {
                throw new ArgumentException ("Key not found for level: " + level);
            }
            return keyByLevel;
        }

        public KeyPair GetById (string memberId, string keyId) {
            var key = allKeys[new Tuple<string, string> (memberId, keyId)];
            if (key == null) {
                throw new ArgumentException ("Key not found for id: " + keyId);
            }
            if (key.IsExpired ()) {
                throw new ArgumentException ("Key with id: " + keyId + "has expired");
            }
            return key;

        }

        public IList<KeyPair> KeyList (string memberId) {
            return allKeys.Where (entry => entry.Key.Item1 == memberId)
                .Select (entry => entry.Value).Where (key => !key.IsExpired ())
                .ToList ();
        }
    }
}