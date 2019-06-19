using System;
using System.Threading.Tasks;
using WebRequest = System.Net.WebRequest;

namespace Tokenio.User.Browser
{
    public interface IBrowser : IDisposable
    {

        /// <summary>
        /// Instructs the browser to load the given url.
        /// </summary>
        /// <param name="url">the url to be loaded</param>
        void GoTo(WebRequest url);

        /// <summary>
        /// Returns a url Task which will notify the user of hyperlinks and redirects.
        ///The new page will not be loaded unless the user calls goTo on that URL.
        /// </summary>
        ///<returns>a url task</returns>
        Task<WebRequest> Url();
        
    }
}
