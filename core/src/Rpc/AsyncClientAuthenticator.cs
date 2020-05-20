using System;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using Tokenio.Utils;

namespace Tokenio.Rpc
{
    /// <summary>
    /// gRPC interceptor that performs Token authentication by signing the request
    /// with a member private key.
    /// </summary>
    public class AsyncClientAuthenticator : Interceptor
    {
        private readonly string memberId;
        private readonly ICryptoEngine crypto;
        private readonly AuthenticationContext authentication;

        private readonly string TOKEN_REALM = "token-realm";
        private readonly string TOKEN_SCHEME = "token-scheme";
        private readonly string TOKEN_KEY_ID = "token-key-id";
        private readonly string TOKEN_SIGNATURE = "token-signature";
        private readonly string TOKEN_CREATED_AT_MS = "token-created-at-ms";
        private readonly string TOKEN_MEMBER_ID = "token-member-id";
        private readonly string TOKEN_ON_BEHALF_OF = "token-on-behalf-of";
        private readonly string CUSTOMER_INITIATED = "customer-initiated";
        private readonly string CUSTOMER_IP_ADDRESS_KEY = "token-customer-ip-address";
        private readonly string CUSTOMER_GEO_LOCATION_KEY = "token-customer-geo-location";
        private readonly string CUSTOMER_DEVICE_ID_KEY = "token-customer-device-id";
        
        public AsyncClientAuthenticator(string memberId,
            ICryptoEngine crypto,
            AuthenticationContext authentication)
        {
            this.memberId = memberId;
            this.crypto = crypto;
            this.authentication = authentication;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var now = Util.EpochTimeMillis();
            var keyLevel = authentication.KeyLevel;
            var signer = crypto.CreateSigner(keyLevel);
            var payload = new GrpcAuthPayload
            {
                Request = ByteString.CopyFrom(((IMessage)request).ToByteArray()),
                CreatedAtMs = now
            };
            var signature = signer.Sign(payload);
            var metadata = context.Options.Headers ?? new Metadata();
            metadata.Add(TOKEN_REALM, "Token");
            metadata.Add(TOKEN_SCHEME, "Token-Ed25519-SHA512");
            metadata.Add(TOKEN_KEY_ID, signer.GetKeyId());
            metadata.Add(TOKEN_SIGNATURE, signature);
            metadata.Add(TOKEN_CREATED_AT_MS, now.ToString());
            metadata.Add(TOKEN_MEMBER_ID, memberId);
            
            var customer = authentication.CustomerTrackingMetadata;
            if (!string.IsNullOrEmpty(customer.IpAddress))
                metadata.Add(CUSTOMER_IP_ADDRESS_KEY,
                    customer.IpAddress);
            if (!string.IsNullOrEmpty(customer.GeoLocation))
                metadata.Add(CUSTOMER_GEO_LOCATION_KEY,
                    customer.GeoLocation);
            if (!string.IsNullOrEmpty(customer.DeviceId))
                metadata.Add(CUSTOMER_DEVICE_ID_KEY,
                    customer.DeviceId);

            if (authentication.OnBehalfOf != null)
            {
                metadata.Add(TOKEN_ON_BEHALF_OF, authentication.OnBehalfOf);
                metadata.Add(CUSTOMER_INITIATED, authentication.CustomerInitiated.ToString());
            }

            return continuation(request,
                new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host,
                    context.Options.WithHeaders(metadata)));
        }
    }
}
