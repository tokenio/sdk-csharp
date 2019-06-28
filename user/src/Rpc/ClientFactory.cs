using Tokenio.Proto.Gateway;
using Tokenio.Rpc;
using Tokenio.Security;

namespace Tokenio.User.Rpc {
	/// <summary>
	/// A factory class that is used to create {@link Client} and {@link UnauthenticatedClient} instances.
	/// </summary>
	public static class ClientFactory {
		/// <summary>
		/// Creates new unauthenticated client backed by the specified channel.
		/// </summary>
		/// <param name = "channel">RPC channel to use.</param>
		/// <returns>Newly created client</returns>
		public static UnauthenticatedClient Unauthenticated(ManagedChannel channel) {
			return new UnauthenticatedClient(new GatewayService.GatewayServiceClient(channel.BuildInvoker()));
		}

		/// <summary>
		/// Creates authenticated client backed by the specified channel.
		/// The supplied signer is used to authenticate the caller for every call.
		/// </summary>
		/// <param name = "channel">The RPC channel to use</param>
		/// <param name = "memberId">The member id</param>
		/// <param name = "crypto">The engine to use for signing requests, tokens, etc.</param>
		/// <returns>Newly created client</returns>
		public static Client Authenticated(
				ManagedChannel channel,
				string memberId,
				ICryptoEngine crypto) {
			return new Client(memberId, crypto, channel);
		}
	}
}