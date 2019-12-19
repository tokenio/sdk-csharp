namespace Tokenio.Tpp.TokenRequests
{
    /// <summary>
    /// Represents callback in Token Request Flow. Contains tokenID and state.
    /// </summary>
    public class TokenRequestCallback
    {
        public static TokenRequestCallback Create(
            string tokenId,
            string state)
        {
            return new TokenRequestCallback
            {
                TokenId = tokenId,
                State = state
            };
        }

        /// <summary>
        /// Get the token ID returned at the end of the Token Request Flow.
        /// </summary>
        /// <value>The token identifier.</value>
        public string TokenId { get; private set; }

        /// <summary>
        /// Get the state returned at the end of the Token Request Flow. This corresponds to the state
        /// set at the beginning of the flow.
        /// </summary>
        /// <value>The state.</value>
        public string State { get; private set; }
    }
}