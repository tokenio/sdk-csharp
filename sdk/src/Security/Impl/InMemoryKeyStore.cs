using System;
using System.Collections.Generic;
using System.Linq;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security
{
    public class InMemoryKeyStore : IKeyStore
    {
        private readonly IDictionary<Tuple<string, Level>, KeyPair> LatestKeys;
        private readonly IDictionary<Tuple<string, string>, KeyPair> AllKeys;

        public InMemoryKeyStore()
        {
            LatestKeys = new Dictionary<Tuple<string, Level>, KeyPair>();
            AllKeys = new Dictionary<Tuple<string, string>, KeyPair>();
        }

        public void Put(string memberId, KeyPair keyPair)
        {
            LatestKeys[new Tuple<string, Level>(memberId, keyPair.Level)] = keyPair;
            AllKeys[new Tuple<string, string>(memberId, keyPair.Id)] = keyPair;
        }

        public KeyPair GetByLevel(string memberId, Level level)
        {
            return LatestKeys[new Tuple<string, Level>(memberId, level)];
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
