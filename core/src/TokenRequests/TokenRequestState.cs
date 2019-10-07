using System;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Tokenio.TokenRequests {
    [Serializable]
    public class TokenRequestState {
        public static TokenRequestState Create (string csrfTokenHash, string state) {
            return new TokenRequestState {
                CsrfTokenHash = csrfTokenHash,
                    InnerState = state
            };
        }

        /// <summary>
        /// Parse a serialized state into a TokenRequestState instance.
        /// </summary>
        /// <returns>The from.</returns>
        /// <param name="serialized">Serialized.</param>
        public static TokenRequestState ParseFrom (string serialized) {
            //ToDo(RD-2410): Remove WebUtility.UrlEncode call. It's only for backward compatibility with the old Token Request Flow.
            var urlDecoded = WebUtility.UrlDecode (serialized);
            var json = Base64UrlEncoder.Decode (urlDecoded);
            return JsonConvert.DeserializeObject<TokenRequestState> (json);
        }

        public string CsrfTokenHash { get; set; }

        public string InnerState { get; set; }

        /// <summary>
        /// Serialize into JSON fomrat and encode.
        /// </summary>
        /// <returns>The serialize.</returns>
        public string Serialize () {
            var json = JsonConvert.SerializeObject (this);
            return Base64UrlEncoder.Encode (json);
        }
    }
}