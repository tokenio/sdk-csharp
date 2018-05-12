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
        private readonly string directory;

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
            var filePath = Path.Combine(directory, memberId.Replace(':', '_'));

            if (keys.ContainsKey(memberId))
            {
                keys[memberId].Add(keyPair);
            }
            else
            {
                keys[memberId] = new List<KeyPair> {keyPair};
                var newFile = File.Create(filePath);
                newFile.Close();
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(keys[memberId]));
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