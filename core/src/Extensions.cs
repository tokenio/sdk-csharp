using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Grpc.Core;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio
{
    public static class Extensions
    {
        public static KeyPair ParseEd25519KeyPair(this AsymmetricCipherKeyPair ed25519KeyPair, Level level)
        {
            var publicKey = ((Ed25519PublicKeyParameters) ed25519KeyPair.Public).GetEncoded();
            var privateKey = ((Ed25519PrivateKeyParameters) ed25519KeyPair.Private).GetEncoded();
            var id = Base64UrlEncoder.Encode(SHA256.Create().ComputeHash(publicKey)).Substring(0, 16);
            return new KeyPair(
                id,
                level,
                Algorithm.Ed25519,
                privateKey,
                publicKey);
        }

        public static KeyPair ParseEd25519KeyPair(this AsymmetricCipherKeyPair ed25519KeyPair, Level level,long expiresAtMs)
        {
            var publicKey = ((Ed25519PublicKeyParameters)ed25519KeyPair.Public).GetEncoded();
            var privateKey = ((Ed25519PrivateKeyParameters)ed25519KeyPair.Private).GetEncoded();
            var id = Base64UrlEncoder.Encode(SHA256.Create().ComputeHash(publicKey)).Substring(0, 16);
            return new KeyPair(
                id,
                level,
                Algorithm.Ed25519,
                privateKey,
                publicKey, expiresAtMs);
        }

        public static Key ToKey(this KeyPair keyPair)
        {

            return new Key
            {
                Id = keyPair.Id,
                PublicKey = Base64UrlEncoder.Encode(keyPair.PublicKey),
                Level = keyPair.Level,
                Algorithm = keyPair.Algorithm,
                ExpiresAtMs= keyPair.ExpiresAtMs

            };
        }

        public static Alias ToNormalized(this Alias alias)
        {
            return new Alias {  Value = alias.Value.ToLower().Trim(), Type = alias.Type, Realm = alias.Realm };
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
