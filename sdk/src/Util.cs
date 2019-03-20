using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Gateway;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tokenio.Exceptions;
using Tokenio.Security;
using static Tokenio.Proto.Common.MemberProtos.MemberOperationMetadata.Types;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;

namespace Tokenio
{
    public static class Util
    {
        public static string Nonce()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 18);
        }

        public static string ToJson(IMessage message)
        {
            var json = JsonFormatter.Default.Format(message);
            return NormalizeJson(json);
        }

        public static string NormalizeAndHash(Alias alias)
        {
            return Base58.Encode(Sha256Hash(Encoding.UTF8.GetBytes(ToJson(alias.ToNormalized()))));
        }

        public static string HashProto(IMessage message)
        {
            return Base58.Encode(Sha256Hash(message.ToByteArray()));
        }

        public static string DoubleToString(double d)
        {
            return Convert.ToString(d, CultureInfo.InvariantCulture);
        }

        public static MemberOperation ToAddAliasOperation(Alias alias)
        {
            return new MemberOperation
            {
                AddAlias = new MemberAliasOperation
                {
                    AliasHash = NormalizeAndHash(alias)
                }
            };
        }

        public static MemberOperation ToRemoveAliasOperation(Alias alias)
        {
            return new MemberOperation
            {
                RemoveAlias = new MemberAliasOperation
                {
                    AliasHash = NormalizeAndHash(alias)
                }
            };
        }

        public static MemberOperationMetadata ToAddAliasMetadata(Alias alias)
        {
            return new MemberOperationMetadata
            {
                AddAliasMetadata = new AddAliasMetadata
                {
                    Alias = alias.ToNormalized(),
                    AliasHash = NormalizeAndHash(alias)
                }
            };
        }

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
                Operations = {operations}
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
                Metadata = {metadata}
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

        public static void VerifySignature(
            ProtoMember member,
            IMessage payload,
            Signature signature)
        {
            Key key;
            try
            {
                key = member.Keys.Single(k => k.Id.Equals(signature.KeyId));
            }
            catch (InvalidOperationException)
            {
                throw new CryptoKeyNotFoundException(signature.KeyId);
            }

            var verifier = new Ed25519Veifier(key.PublicKey);
            verifier.Verify(payload, signature.Signature_);
        }

        public static long EpochTimeMillis()
        {
            return (long) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static string GetQueryString(string url)
        {
            if (url == null)
            {
                throw new ArgumentException("URL cannot be null");
            }
            var splitted = url.Split(new[] {'?'}, 2);
            return splitted.Length == 1 ? splitted[0] : splitted[1];
        }

        private static string NormalizeJson(string json)
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
                     var jObj = (JObject) jToken;
                     var sortedProperties = jObj.Properties().OrderBy(p => p.Name).ToList();
                     jObj.RemoveAll();

                     foreach (var p in sortedProperties)
                     {
                         Sort(p.Value);
                         jObj.Add(p);
                     }

                     break;

                 case JTokenType.Array:
                     foreach (var child in ((JArray) jToken).Children())
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
    }
}
