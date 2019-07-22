using System;
using System.Threading.Tasks;
using WebRequest = System.Net.WebRequest;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Tokenio.User.Browser {
	/// <summary>
	/// A browser abstraction used by the SDK to interact with web content.
	/// Pages will only be displayed in the browser as a result of a call to goTo(url).
	/// Hyperlinks and redirects will cause the url() Observable to be notified but will not load the page unless goTo(url) is explicitly called by an Observer.
	/// </summary>
	public interface IBrowser : IDisposable {
		/// <summary>
		/// Instructs the browser to load the given url.
		/// </summary>
		/// <param name = "url">The url to be loaded</param>
		void GoTo(WebRequest url);

        /// <summary>
        /// Returns a url Task which will notify the user of hyperlinks and redirects.
        /// The new page will not be loaded unless the user calls goTo on that URL.
        /// </summary>
        /// <returns>A url task</returns>
      
       Task<WebRequest> Url();
	}
}