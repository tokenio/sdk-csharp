using System;
using System.Reflection;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Interceptors;
using log4net;

namespace sdk.Rpc
{
    public class ManagedChannel : IDisposable
    {
        private static readonly ILog logger = LogManager
            .GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private static readonly int SHUTDOWN_DURATION_MS = 10000;
        
        private readonly Channel channel;
        private readonly Interceptor[] interceptors;
        
        public ManagedChannel(Channel channel, Interceptor[] interceptors)
        {
            this.channel = channel;
            this.interceptors = interceptors;
        }

        public CallInvoker BuildInvoker()
        {
            return channel.Intercept(interceptors);
        }

        public void Dispose()
        {
            if (!channel.ShutdownAsync().Wait(SHUTDOWN_DURATION_MS))
            {
                logger.Error("Channel shutdown timed out! Interrupting thread...");
                Thread.CurrentThread.Interrupt();
            }
        }
    }
}
