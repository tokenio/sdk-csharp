using System.Security.Cryptography;
using Google.Protobuf;
using Sodium.Exceptions;

namespace sdk.Security
{
    public interface IVerifier
    {
        /// <summary>
        /// Verifies the protobuf payload signature.
        /// </summary>
        /// <param name="message">the payload to sign</param>
        /// <param name="signature">the signature to verify</param>
        /// <exception cref="KeyOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        void Verify(IMessage message, string signature);

        /// <summary>
        /// Verifies the protobuf payload signature.
        /// </summary>
        /// <param name="payload">the payload to sign</param>
        /// <param name="signature">the signature to verify</param>
        /// <exception cref="KeyOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        void Verify(string payload, string signature);
    }
}
