using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Interceptors;
using log4net;
namespace Tokenio.Rpc
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

        /// <summary>
        /// Creates a new <see cref="Builder"/> instance that is used to configure
        /// </summary>
        /// <returns>the builder</returns>
        public static Builder NewBuilder(string hostName, int port, bool useSsl)
        {
            return new Builder(hostName, port, useSsl);
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


        public class Builder
        {

            protected readonly int port;
            protected readonly bool useSsl;
            protected readonly string hostName;
            protected bool keepAlive;
            protected int keepAliveTimeMs;
            protected long timeout;
            protected Metadata metadata;

            public Builder(string hostName, int port, bool useSsl)
            {
                this.hostName = hostName;
                this.port = port;
                this.useSsl = useSsl;
            }

            /// <summary>
            /// Sets whether the connection will allow keep-alive pings.
            /// </summary>
            /// <param name="keepAlive">whether keep-alive is enabled</param>
            /// <returns>this builder instance</returns>
            public Builder UseKeepAlive(bool keepAlive)
            {
                this.keepAlive = keepAlive;
                return this;
            }

            /// <summary>
            /// Sets the keep-alive time in milliseconds.
            /// </summary>
            /// <param name="keepAliveTimeMs">keep-alive time in milliseconds</param>
            /// <returns>this builder instance</returns>
            public Builder WithKeepAliveTime(int keepAliveTimeMs)
            {
                this.keepAliveTimeMs = keepAliveTimeMs;
                return this;
            }

            /// <summary>
            /// Sets the timeout in milliseconds.
            /// </summary>
            /// <param name="timeout"></param>
            /// <returns></returns>
            public Builder WithTimeout(long timeout)
            {
                this.timeout = timeout;
                return this;
            }

            /// <summary>
            /// Sets the metadata.
            /// </summary>
            /// <param name="metadata"></param>
            /// <returns></returns>
            public Builder WithMetadata(Metadata metadata)
            {
                this.metadata = metadata;
                return this;
            }

            public ManagedChannel Build()
            {
                var channelOptions = new List<ChannelOption>();
                channelOptions.Add(new ChannelOption("grpc.keepalive_permit_without_calls", keepAlive ? 1 : 0));
                channelOptions.Add(new ChannelOption("grpc.keepalive_time_ms", keepAliveTimeMs));
                var channel = new Channel(
                    hostName,
                    port,
                    useSsl ? new SslCredentials() : ChannelCredentials.Insecure,
                    channelOptions);

                Interceptor[] interceptors =
                {
                    new AsyncTimeoutInterceptor(timeout),
                    new AsyncMetadataInterceptor(metadata => {
                        foreach(Metadata.Entry entry in this.metadata)
                        {
                            metadata.Add(entry);
                        }
                        return metadata;
                    })
                };

                return new ManagedChannel(channel, interceptors);
            }
        }
    }
}
