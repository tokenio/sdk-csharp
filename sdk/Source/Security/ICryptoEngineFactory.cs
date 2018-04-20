namespace sdk.Security
{
    public interface ICryptoEngineFactory
    {
        /// <summary>
        /// Creates a new <see cref="ICryptoEngine"/> for a given member.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <returns>the crypto engine instance</returns>
        ICryptoEngine Create(string memberId);
    }
}
