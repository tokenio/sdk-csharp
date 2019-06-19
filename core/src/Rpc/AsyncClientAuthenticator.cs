using System;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using Tokenio.Utils;

namespace Tokenio.Rpc
{
    public class AsyncClientAuthenticator : Interceptor
    {
        private readonly string memberId;
        private readonly ICryptoEngine crypto;
        private readonly AuthenticationContext authentication;

        public AsyncClientAuthenticator(
            string memberId,
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
                Request = ByteString.CopyFrom(((IMessage) request).ToByteArray()),
                CreatedAtMs = now
            };
            var signature = signer.Sign(payload);
            var metadata = context.Options.Headers ?? new Metadata();
            metadata.Add("token-realm", "Token");
            metadata.Add("token-scheme", "Token-Ed25519-SHA512");
            metadata.Add("token-key-id", signer.GetKeyId());
            metadata.Add("token-signature", signature);
            metadata.Add("token-created-at-ms", now.ToString());
            metadata.Add("token-member-id", memberId);
            metadata.Add("token-security-metadata", encodeSecurityMetadata(authentication));

            if (authentication.OnBehalfOf != null)
            {
                metadata.Add("token-on-behalf-of", authentication.OnBehalfOf);
                metadata.Add("customer-initiated", authentication.CustomerInitiated.ToString());
            }

            return continuation(request,
                new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host,
                    context.Options.WithHeaders(metadata)));
        }

        private static string encodeSecurityMetadata(AuthenticationContext context)
        {
            var json = Util.ToJson(context.SecurityMetadata);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        }
    }
}
