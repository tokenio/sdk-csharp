﻿using Grpc.Core.Interceptors;
using Tokenio.Proto.Gateway;
using Tokenio.Security;

namespace Tokenio.Rpc
{
    public static class ClientFactory
    {
        public static UnauthenticatedClient Unauthenticated(ManagedChannel channel)
        {
            return new UnauthenticatedClient(new GatewayService.GatewayServiceClient(channel.BuildInvoker()));
        }

        /// <summary>
        /// Creates authenticated client backed by the specified channel. The supplied
        /// signer is used to authenticate the caller for every call.
        /// </summary>
        /// <param name="channel">the RPC channel to use</param>
        /// <param name="memberId">the member id</param>
        /// <param name="crypto">the engine to use for signing requests, tokens, etc</param>
        /// <returns>the created client</returns>
        public static Client Authenticated(
            ManagedChannel channel,
            string memberId,
            ICryptoEngine crypto)
        {
            return new Client(memberId, crypto, channel);
        }
    }
}
