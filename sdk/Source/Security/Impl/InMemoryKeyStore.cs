using System;
using System.Collections.Generic;
using System.Linq;
using Io.Token.Proto.Common.Security;

namespace sdk.Security
{
    public class InMemoryKeyStore : IKeyStore
    {
        private readonly IDictionary<Tuple<string, Key.Types.Level>, KeyPair> LatestKeys;
        private readonly IDictionary<Tuple<string, string>, KeyPair> AllKeys;

        public InMemoryKeyStore()
        {
            LatestKeys = new Dictionary<Tuple<string, Key.Types.Level>, KeyPair>();
            AllKeys = new Dictionary<Tuple<string, string>, KeyPair>();
        }

        public void put(string memberId, KeyPair keyPair)
        {
            LatestKeys[new Tuple<string, Key.Types.Level>(memberId, keyPair.Level)] = keyPair;
            AllKeys[new Tuple<string, string>(memberId, keyPair.Id)] = keyPair;
        }

        public KeyPair GetByLevel(string memberId, Key.Types.Level level)
        {
            return LatestKeys[new Tuple<string, Key.Types.Level>(memberId, level)];
        }

        public KeyPair GetById(string memberId, string keyId)
        {
            return AllKeys[new Tuple<string, string>(memberId, keyId)];
        }

        public IList<KeyPair> KeyList(string memberId)
        {
            return AllKeys.Where(entry => entry.Key.Item1 == memberId)
                .Select(entry => entry.Value)
                .ToList();
        }
    }
}
