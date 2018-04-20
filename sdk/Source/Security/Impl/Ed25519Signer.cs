using Google.Protobuf;
using Microsoft.IdentityModel.Tokens;
using Sodium;

namespace sdk.Security
{
    public class Ed25519Signer : ISigner
    {
        private readonly string keyId;
        private readonly byte[] privateKey;

        public Ed25519Signer(string keyId,  byte[] privateKey)
        {
            this.keyId = keyId;
            this.privateKey = privateKey;
        }

        public string GetKeyId()
        {
            return keyId;
        }

        public string Sign(IMessage message)
        {
            return Sign(Util.ToJson(message));
        }

        public string Sign(string payload)
        {
            var signature = PublicKeyAuth.SignDetached(payload, privateKey);
            return Base64UrlEncoder.Encode(signature);
        }
    }
}
