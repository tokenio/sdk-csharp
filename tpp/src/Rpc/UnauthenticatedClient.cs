using Tokenio.Proto.Gateway;

namespace Tokenio.Tpp.Rpc
{
    /// <summary>
    /// Similar to <see cref="Client"/> but is only used for a handful of requests that
    /// don't require authentication. We use this client to create new member or getMember
    /// an existing one and switch to the authenticated <see cref="Client"/>.
    /// </summary>
    public sealed class UnauthenticatedClient : Tokenio.Rpc.UnauthenticatedClient
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="gateway">the gateway gRPC client</param>
        public UnauthenticatedClient(GatewayService.GatewayServiceClient gateway)
            : base(gateway)
        {
        }
    }
}
