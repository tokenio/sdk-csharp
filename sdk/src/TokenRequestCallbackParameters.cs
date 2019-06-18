using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Google.Protobuf;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.SecurityProtos;

namespace Tokenio
{
    public class TokenRequestCallbackParameters
    {
        private static readonly string TOKEN_ID_FIELD = "tokenId";
        private static readonly string STATE_FIELD = "state";
        private static readonly string SIGNATURE_FIELD = "signature";

        public static TokenRequestCallbackParameters Create(NameValueCollection parameters)
        {
            if (!parameters.AllKeys.Contains(TOKEN_ID_FIELD)
                || !parameters.AllKeys.Contains(STATE_FIELD)
                || !parameters.AllKeys.Contains(SIGNATURE_FIELD))
            {
                throw new InvalidTokenRequestQuery();
            }

            return new TokenRequestCallbackParameters
            {
                TokenId = parameters.Get(TOKEN_ID_FIELD),
                SerializedState = parameters.Get(STATE_FIELD),
                Signature = JsonParser.Default.Parse<Signature>(parameters.Get(SIGNATURE_FIELD))
            };
        }

        public string TokenId { get; private set; }

        public string SerializedState { get; private set; }

        public Signature Signature { get; private set; }
    }
}
