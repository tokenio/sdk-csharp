
namespace Tokenio.User.Browser
{
    public interface IBrowserFactory
    {
        /// <summary>
        /// Creates a new Browser.
        /// </summary>
        /// <returns>a new browser</returns>
        IBrowser Create();
    }
}
