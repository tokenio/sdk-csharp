using System;
using System.Linq;
using Google.Protobuf;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using Tokenio.Security.Crypto;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;

namespace Tokenio.Tpp.Utils {
    /// <summary>
    /// Utility Methods
    /// </summary>
    public class Util : Tokenio.Utils.Util {
        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <returns>The query string.</returns>
        /// <param name="url">URL.</param>
        public static string GetQueryString(string url) {
            if (url == null) {
                throw new ArgumentException("URL cannot be null");
            }
            var splitted = url.Split(new [] { '?' }, 2);
            return splitted.Length == 1 ? splitted[0] : splitted[1];
        }

        /// <summary>
        /// Verify the signature of the payload.
        /// </summary>
        /// <param name="member">Member.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="signature">Signature.</param>
        public static void VerifySignature(
            ProtoMember member,
            IMessage payload,
            Signature signature) {
            Key key;
            try {
                key = member.Keys.Single(k => k.Id.Equals(signature.KeyId));
            } catch (InvalidOperationException) {
                throw new CryptoKeyNotFoundException(signature.KeyId);
            }

            var verifier = new Ed25519Veifier(key.PublicKey);
            verifier.Verify(payload, signature.Signature_);
        }
    }
}
