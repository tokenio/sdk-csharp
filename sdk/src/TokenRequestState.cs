using System;
using System.Net;
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
            var json = WebUtility.UrlDecode(serialized);
            return JsonConvert.DeserializeObject<TokenRequestState>(json);
        }

        public string CsrfTokenHash { get; set; }

        public string InnerState { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
