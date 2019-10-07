using System;
using System.Threading.Tasks;
using WebRequest = System.Net.WebRequest;

namespace Tokenio.User.Browser {
    /// <summary>
    /// A browser abstraction used by the SDK to interact with web content.
    /// 
    /// <p>Pages will only be displayed in the browser as a result of a call to GoTo(url).
    /// Hyperlinks and redirects will cause the url() Task to be notified
    /// but will not load the page unless goTo(url) is explicitly called by a Task.</p>
    /// </summary>
    public interface IBrowser : IDisposable {
        /// <summary>
        /// Instructs the browser to load the given url.
        /// </summary>
        /// <param name = "url">the url to be loaded</param>
        void GoTo (WebRequest url);

        /// <summary>
        /// Returns a url Task which will notify the user of hyperlinks and redirects.
        /// The new page will not be loaded unless the user calls GoTo on that URL.
        /// </summary>
        /// <returns>a url task</returns>

        Task<WebRequest> Url ();
    }
}