namespace Tokenio.Security {
    /// <summary>
    /// Creates {@link CryptoEngine} instances bound to a given member id.
    /// </summary>
    public interface ICryptoEngineFactory {
        /// <summary>
        /// Creates a new <see cref="ICryptoEngine"/> for a given member.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <returns>the crypto engine instance</returns>
        ICryptoEngine Create(string memberId);
    }
}
