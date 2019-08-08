namespace Tokenio.User.Browser
{
    /// <summary>
    /// A browser factory.
    /// </summary>
    public interface IBrowserFactory
    {
        /// <summary>
        /// Creates a new browser.
        /// </summary>
        /// <returns>a new browser</returns>
        IBrowser Create();
    }
}