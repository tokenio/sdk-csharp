using System.Runtime.CompilerServices;
using System.Text;
using Google.Protobuf;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Tokenio.Utils;

namespace Tokenio.Security
{
    public class Rs256Signer : ISigner
    {
        private readonly string keyId;
        private readonly Org.BouncyCastle.Crypto.ISigner signer;

        public Rs256Signer(string keyId, byte[] privateKey)
        {
            this.keyId = keyId;
            signer = SignerUtilities.GetSigner("SHA-256withRSA");
            signer.Init(true, PrivateKeyFactory.CreateKey(privateKey));
        }

        public string GetKeyId()
        {
            return keyId;
        }

        public string Sign(IMessage message)
        {
            return Sign(Util.ToJson(message));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string Sign(string payload)
        {
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            signer.Reset();
            signer.BlockUpdate(payloadBytes, 0, payloadBytes.Length);
            return Base64UrlEncoder.Encode(signer.GenerateSignature());
        }
    }
}
