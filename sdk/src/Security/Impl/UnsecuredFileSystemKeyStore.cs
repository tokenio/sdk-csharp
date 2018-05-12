using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Security
{
    public class UnsecuredFileSystemKeyStore : IKeyStore
    {
        private readonly IDictionary<string, IList<KeyPair>> keys;
        private readonly string filePath;

        public UnsecuredFileSystemKeyStore(string filePath)
        {
            this.filePath = filePath;

            if (File.Exists(filePath))
            {
                var content = File.ReadAllText(filePath);
                keys = JsonConvert.DeserializeObject<IDictionary<string, IList<KeyPair>>>(content);
            }
            else
            {
                keys = new Dictionary<string, IList<KeyPair>>();
                var stream = File.Create(filePath);
                stream.Close();
            }
        }

        public void Put(string memberId, KeyPair keyPair)
        {
            if (keys.ContainsKey(memberId))
            {
                keys[memberId].Add(keyPair);
            }
            else
            {
                keys[memberId] = new List<KeyPair> {keyPair};
                File.WriteAllText(filePath, JsonConvert.SerializeObject(keys));
            }
        }

        public KeyPair GetByLevel(string memberId, Level level)
        {
            return keys[memberId].Last(key => key.Level.Equals(level));
        }

        public KeyPair GetById(string memberId, string keyId)
        {
            return keys[memberId].First(key => key.Id.Equals(keyId));
        }

        public IList<KeyPair> KeyList(string memberId)
        {
            return keys[memberId];
        }
    }
}