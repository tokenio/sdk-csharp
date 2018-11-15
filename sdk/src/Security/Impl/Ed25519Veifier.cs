using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Tokenio.Security
{
    public class Ed25519Veifier : IVerifier
    {
        private readonly Org.BouncyCastle.Crypto.ISigner signer;

        public Ed25519Veifier(byte[] publicKey)
        {
            signer = SignerUtilities.GetSigner("Ed25519");
            signer.Init(false, new Ed25519PublicKeyParameters(publicKey, 0));
        }
        
        public Ed25519Veifier(string publicKey) : this(Base64UrlEncoder.DecodeBytes(publicKey))
        {
        }

        public void Verify(IMessage message, string signature)
        {
            Verify(Util.ToJson(message), signature);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Verify(string payload, string signature)
        {
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            signer.Reset();
            signer.BlockUpdate(payloadBytes, 0, payloadBytes.Length);
            if (!signer.VerifySignature(Base64UrlEncoder.DecodeBytes(signature)))
            {
                throw new CryptographicException("Failed to verify signature.");
            }
        }
    }
}
