using System;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace sdk.Rpc
{
    public class AsyncTimeoutInterceptor : Interceptor
    {
        private readonly long timeoutMs;
        
        public AsyncTimeoutInterceptor(long timeoutMs)
        {
            this.timeoutMs = timeoutMs;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            
            return continuation(
                request,
                new ClientInterceptorContext<TRequest, TResponse>(
                    context.Method,
                    context.Host,
                    context.Options.WithDeadline(deadline)));
        }
    }
}
