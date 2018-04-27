namespace Tokenio
{
    public class TokenRequestCallback
    {
        public static TokenRequestCallback Create(string tokenId, string state)
        {
            return new TokenRequestCallback
            {
                TokenId = tokenId,
                State = state
            };
        }

        public string TokenId { get; private set; }

        public string State { get; private set; }
    }
}
