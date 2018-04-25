using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using Microsoft.IdentityModel.Tokens;
using Sodium;

namespace Tokenio.Security
{
    public class Ed25519Veifier : IVerifier
    {
        private readonly byte[] publicKey;

        public Ed25519Veifier(byte[] publicKey)
        {
            this.publicKey = publicKey;
        }
        
        public Ed25519Veifier(string publicKey)
        {
            this.publicKey = Base64UrlEncoder.DecodeBytes(publicKey);
        }

        public void Verify(IMessage message, string signature)
        {
            Verify(Util.ToJson(message), signature);
        }

        public void Verify(string payload, string signature)
        {
            var verified = PublicKeyAuth.VerifyDetached(
                Base64UrlEncoder.DecodeBytes(signature),
                Encoding.UTF8.GetBytes(payload),
                publicKey);
            
            if (!verified)
            {
                throw new CryptographicException("Failed to verify signature.");
            }
        }
    }
}
