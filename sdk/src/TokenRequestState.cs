using System;
using System.Net;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Tokenio
{
    [Serializable]
    public class TokenRequestState
    {
        public static TokenRequestState Create(string csrfTokenHash, string state)
        {
            return new TokenRequestState
            {
                CsrfTokenHash = csrfTokenHash,
                InnerState = state
            };
        }

        public static TokenRequestState ParseFrom(string serialized)
        {
            //ToDo: Remove Base64Encoder call. It's only for backward compatibility with the old Token flow.
            var json = Base64UrlEncoder.Decode(serialized);
            return JsonConvert.DeserializeObject<TokenRequestState>(json);
        }

        public string CsrfTokenHash { get; set; }

        public string InnerState { get; set; }

        public string Serialize()
        {
            //ToDo: Remove Base64Encoder call. It's only for backward compatibility with the old Token flow.
            var json =  JsonConvert.SerializeObject(this);
            return Base64UrlEncoder.Encode(json);
        }
    }
}
