using System;
using System.Collections.Generic;
using System.Linq;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security {
    public class InMemoryKeyStore : IKeyStore {
        private readonly IDictionary<Tuple<string, Level>, KeyPair> latestKeys;
        private readonly IDictionary<Tuple<string, string>, KeyPair> allKeys;

        public InMemoryKeyStore() {
            latestKeys = new Dictionary<Tuple<string, Level>, KeyPair>();
            allKeys = new Dictionary<Tuple<string, string>, KeyPair>();
        }

        public void Put(string memberId, KeyPair keyPair) {
            latestKeys[new Tuple<string, Level>(memberId, keyPair.Level)] = keyPair;
            allKeys[new Tuple<string, string>(memberId, keyPair.Id)] = keyPair;
        }

        public KeyPair GetByLevel(string memberId, Level level) {
            return latestKeys[new Tuple<string, Level>(memberId, level)];
        }

        public KeyPair GetById(string memberId, string keyId) {
            return allKeys[new Tuple<string, string>(memberId, keyId)];
        }

        public IList<KeyPair> KeyList(string memberId) {
            return allKeys.Where(entry => entry.Key.Item1 == memberId)
                .Select(entry => entry.Value)
                .ToList();
        }
    }
}
