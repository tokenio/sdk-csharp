using Tokenio.Proto.Common.SecurityProtos;

namespace Tokenio {
    public class TokenRequestResult {
        /// <summary>
        /// Creates an instance of <see cref="TokenRequestResult"/>.
        /// </summary>
        /// <param name="tokenId">token id</param>
        /// <param name="signature">token request state signature</param>
        public TokenRequestResult(string tokenId, Signature signature) {
            TokenId = tokenId;
            Signature = signature;
        }

        /// <summary>
        /// Gets the token ID.
        /// </summary>
        public string TokenId { get; }

        /// <summary>
        /// Gets the signature.
        /// </summary>
        public Signature Signature { get; }
    }
}
