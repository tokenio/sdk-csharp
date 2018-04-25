using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Grpc.Core;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Microsoft.IdentityModel.Tokens;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio
{
    public static class Extensions
    {
        public static KeyPair ToKeyPair(this Sodium.KeyPair keyPair, Level level)
        {
            var id = Base64UrlEncoder.Encode(SHA256.Create().ComputeHash(keyPair.PublicKey)).Substring(0, 16);
            return new KeyPair(id, level, Algorithm.Ed25519, keyPair.PrivateKey, keyPair.PublicKey);
        }

        public static Key ToKey(this KeyPair keyPair)
        {
            return new Key
            {
                Id = keyPair.Id,
                PublicKey = Base64UrlEncoder.Encode(keyPair.PublicKey),
                Level = keyPair.Level,
                Algorithm = keyPair.Algorithm
            };
        }

        public static Alias ToNormalized(this Alias alias)
        {
            switch (alias.Type)
            {
                case Domain:
                case Email:
                    return new Alias {Value = alias.Value.ToLower(), Type = alias.Type};
                default:
                    return alias;
            }
        }

        public static async Task<TResult> Map<TSource, TResult>(
            this Task<TSource> sourceTask,
            Func<TSource, TResult> func)
        {
            return func.Invoke(await sourceTask);
        }
        
        public static async Task<TResult> FlatMap<TSource, TResult>(
            this Task<TSource> sourceTask,
            Func<TSource, Task<TResult>> func)
        {
            return await func.Invoke(await sourceTask);
        }
        
        public static async Task ToTask<TSource>(this AsyncUnaryCall<TSource> sourceAsync)
        {
            await sourceAsync.ResponseAsync;
        }
        
        public static async Task<TResult> ToTask<TSource, TResult>(
            this AsyncUnaryCall<TSource> sourceAsync,
            Func<TSource, TResult> func)
        {
            var source = await sourceAsync.ResponseAsync;
            return func.Invoke(source);
        }
    }
}
