using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Utils;

namespace Tokenio.Rpc {
    public class AsyncMetadataInterceptor : Interceptor {
        private readonly Func<Metadata, Metadata> interceptor;

        public AsyncMetadataInterceptor (Func<Metadata, Metadata> interceptor) {
            this.interceptor = GrpcPreconditions.CheckNotNull (interceptor, nameof (interceptor));
        }

        private ClientInterceptorContext<TRequest, TResponse> GetNewContext<TRequest, TResponse> (
            ClientInterceptorContext<TRequest, TResponse> context) where TRequest : class where TResponse : class {
            var options = context.Options;
            var metadata = options.Headers ?? new Metadata ();
            return new ClientInterceptorContext<TRequest, TResponse> (
                context.Method,
                context.Host,
                options.WithHeaders (interceptor (metadata)));
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse> (
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation) {
            return continuation (request, GetNewContext (context));
        }
    }
}