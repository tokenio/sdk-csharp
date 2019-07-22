using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using static Tokenio.Proto.Common.MemberProtos.MemberOperationMetadata.Types;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;

namespace Tokenio.Utils
{
    /// <summary>
    /// Utility Methods
    /// </summary>
    public class Util
    {
        /// <summary>
        /// Generates a random string 
        /// </summary>
        /// <returns>Generated Random string.</returns>
        public static string Nonce()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 18);
        }

        public static string ToJson(IMessage message)
        {
            var json = JsonFormatter.Default.Format(message);
            return NormalizeJson(json);
        }

        public static string HashAlias(Alias alias)
        {
            var aliasClone = alias.Clone();
            aliasClone.Realm = "";
            return Base58.Encode(Sha256Hash(Encoding.UTF8.GetBytes(ToJson(aliasClone))));
        }

        public static string NormalizeAndHashAlias(Alias alias)
        {
            return HashAlias(alias.ToNormalized());
        }

        public static string HashProto(IMessage message)
        {
            return Base58.Encode(Sha256Hash(message.ToByteArray()));
        }

        public static string DoubleToString(double d)
        {
            return Convert.ToString(d, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Converts alias to AddAlias operation.
        /// </summary>
        /// <returns>member operation.</returns>
        /// <param name="alias">Alias : alias to add.</param>
        public static MemberOperation ToAddAliasOperation(Alias alias)
        {
            return new MemberOperation
            {
                AddAlias = new MemberAliasOperation
                {
                    AliasHash = NormalizeAndHashAlias(alias),
                    Realm = alias.Realm,
                    RealmId = alias.RealmId

                }
            };
        }

        public static MemberOperation ToRemoveAliasOperation(Alias alias)
        {
            return new MemberOperation
            {
                RemoveAlias = new MemberAliasOperation
                {
                    AliasHash = NormalizeAndHashAlias(alias),
                    Realm = alias.Realm
                }
            };
        }

        /// <summary>
        /// Converts alias to MemberOperationMetadata.
        /// </summary>
        /// <returns>member operation metadata</returns>
        /// <param name="alias">Alias : alias to add.</param>
        public static MemberOperationMetadata ToAddAliasMetadata(Alias alias)
        {
            return new MemberOperationMetadata
            {
                AddAliasMetadata = new AddAliasMetadata
                {
                    Alias = alias.ToNormalized(),
                    AliasHash = NormalizeAndHashAlias(alias)
                }
            };
        }

        /// <summary>
        /// Converts Key to AddKey operation.
        /// </summary>
        /// <returns>member operation.</returns>
        /// <param name="key">Key : key to add</param>
        public static MemberOperation ToAddKeyOperation(Key key)
        {
            return new MemberOperation
            {
                AddKey = new MemberAddKeyOperation
                {
                    Key = key
                }
            };
        }

        /// <summary>
        /// Converts agent id to AddKey operation.
        /// </summary>
        /// <returns>member operation.</returns>
        /// <param name="agentId">Agent identifier : agent id to add.</param>
        public static MemberOperation ToRecoveryAgentOperation(string agentId)
        {

            var MemberRecoveryRulesOperation = new MemberRecoveryRulesOperation()
            {

                RecoveryRule = new RecoveryRule()
                {

                    PrimaryAgent = agentId

                }
            };
            return new MemberOperation()
            {
                RecoveryRules = MemberRecoveryRulesOperation
            };

        }


        public static MemberOperation ToRemoveKeyOperation(string keyId)
        {
            return new MemberOperation
            {
                RemoveKey = new MemberRemoveKeyOperation
                {
                    KeyId = keyId
                }
            };
        }

        public static UpdateMemberRequest ToUpdateMemberRequest(
            ProtoMember member,
            IList<MemberOperation> operations,
            ISigner signer)
        {
            return ToUpdateMemberRequest(member, operations, signer, new List<MemberOperationMetadata>());
        }

        public static UpdateMemberRequest ToUpdateMemberRequest(
            ProtoMember member,
            IList<MemberOperation> operations,
            ISigner signer,
            IList<MemberOperationMetadata> metadata)
        {
            var update = new MemberUpdate
            {
                MemberId = member.Id,
                PrevHash = member.LastHash,
                Operations = { operations }
            };

            return new UpdateMemberRequest
            {
                Update = update,
                UpdateSignature = new Signature
                {
                    MemberId = member.Id,
                    KeyId = signer.GetKeyId(),
                    Signature_ = signer.Sign(update)
                },
                Metadata = { metadata }
            };
        }

        public static async Task<KeyValuePair<T1, T2>> TwoTasks<T1, T2>(Task<T1> task1, Task<T2> task2)
        {
            await Task.WhenAll(task1, task2);
            return new KeyValuePair<T1, T2>(task1.Result, task2.Result);
        }

        public static string HashString(string str)
        {
            return ToBigEndianHex(Sha256Hash(Encoding.ASCII.GetBytes(str)));
        }

        public static long EpochTimeMillis()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }


        public static string NormalizeJson(string json)
        {
            var jToken = JToken.Parse(json);
            Sort(jToken);
            return JsonConvert.SerializeObject(jToken, Formatting.None);
        }

        private static void Sort(JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.Object:
                    var jObj = (JObject)jToken;
                    var sortedProperties = jObj.Properties().OrderBy(p => p.Name).ToList();
                    jObj.RemoveAll();

                    foreach (var p in sortedProperties)
                    {
                        Sort(p.Value);
                        jObj.Add(p);
                    }

                    break;

                case JTokenType.Array:
                    foreach (var child in ((JArray)jToken).Children())
                    {
                        Sort(child);
                    }

                    break;

                default: return;
            }
        }

        private static string ToBigEndianHex(byte[] bytes)
        {
            var hexDigits = "0123456789abcdef";
            var sb = new StringBuilder(2 * bytes.Length);
            foreach (var b in bytes)
            {
                sb.Append(hexDigits[(b >> 4) & 0xf]).Append(hexDigits[b & 0xf]);
            }

            return sb.ToString();
        }

        private static byte[] Sha256Hash(byte[] payload)
        {
            return SHA256.Create().ComputeHash(payload);
        }

        public static long CurrentMillis()
        {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTime = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
            return currentTime;
        }
    }
}
