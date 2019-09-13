using Google.Protobuf;

namespace Tokenio.Security
{
    public interface ISigner
    {
        /// <summary>
        /// Returns the Key ID used for signing.
        /// </summary>
        /// <returns>The key id.</returns>
        string GetKeyId();

        /// <summary>
        /// Signs protobuf message. The message is converted to normalized json and the json
        /// gets signed.
        /// </summary>
        /// <param name="message">the payload to sign</param>
        /// <returns>the signature as a hex encoded string</returns>
        string Sign(IMessage message);

        /// <summary>
        /// Signs the payload with the test key.
        /// </summary>
        /// <param name="payload">the payload to sign</param>
        /// <returns>the signature as hex encoded string</returns>
        /// <remarks>this method is for testing purpose</remarks>
        string Sign(string payload);
    }
}
