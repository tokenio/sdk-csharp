namespace Tokenio.User.Browser {
	/// <summary>
	/// A Browser factory.
	/// </summary>
	public interface IBrowserFactory {
		/// <summary>
		/// Creates a new Browser.
		/// </summary>
		/// <returns>A new browser</returns>
		IBrowser Create();
	}
}