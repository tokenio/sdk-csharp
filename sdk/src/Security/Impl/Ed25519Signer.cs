using System.Runtime.CompilerServices;
using System.Text;
using Google.Protobuf;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Tokenio.Security {
    public class Ed25519Signer : ISigner {
        private readonly string keyId;
        private readonly Org.BouncyCastle.Crypto.ISigner signer;

        public Ed25519Signer(string keyId, byte[] privateKey) {
            this.keyId = keyId;
            signer = SignerUtilities.GetSigner("Ed25519");
            signer.Init(true, new Ed25519PrivateKeyParameters(privateKey, 0));
        }

        public string GetKeyId() {
            return keyId;
        }

        public string Sign(IMessage message) {
            return Sign(Util.ToJson(message));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string Sign(string payload) {
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            signer.Reset();
            signer.BlockUpdate(payloadBytes, 0, payloadBytes.Length);
            return Base64UrlEncoder.Encode(signer.GenerateSignature());
        }
    }
}
