using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Tokenio.Proto.Gateway;
using Tokenio.Security;

namespace Tokenio.Rpc
{
    public class AsyncClientAuthenticator : Interceptor
    {
        private readonly string memberId;
        private readonly ICryptoEngine crypto;

        public AsyncClientAuthenticator(string memberId, ICryptoEngine crypto)
        {
            this.memberId = memberId;
            this.crypto = crypto;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var now = Util.EpochTimeMillis();
            var keyLevel = AuthenticationContext.ResetKeyLevel();
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

            if (AuthenticationContext.OnBehalfOf != null)
            {
                metadata.Add("token-on-behalf-of", AuthenticationContext.OnBehalfOf);
                metadata.Add("customer-initiated", AuthenticationContext.CustomerInitiated.ToString());
                AuthenticationContext.ClearAccessToken();
            }

            return continuation(request,
                new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host,
                    context.Options.WithHeaders(metadata)));
        }
    }
}
