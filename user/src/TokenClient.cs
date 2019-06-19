using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.User.Rpc;
using Tokenio.Security;
using Tokenio.TokenRequests;
using TokenRequest = Tokenio.TokenRequests.TokenRequest;
using Tokenio.User.Browser;
using Tokenio.User;
using Tokenio.User.Utils;
using WebUtility = System.Net.WebUtility;
using TokenRequestStatePayload = Tokenio.Proto.Common.TokenProtos.TokenRequestStatePayload;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;


namespace Tokenio.User
{
    public class TokenClient : Tokenio.TokenClient
    {
        /// <summary>
        /// The browser factory.
        /// </summary>
        private readonly IBrowserFactory browserFactory;



        /// <summary>
        /// Initializes a new instance of the <see cref="T:Tokenio.User.TokenClient"/> class.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="cryptoEngineFactory">Crypto engine factory.</param>
        /// <param name="tokenCluster">Token cluster.</param>
        /// <param name="browserFactory">Browser factory.</param>
        public TokenClient(
             Tokenio.Rpc.ManagedChannel channel,
             ICryptoEngineFactory cryptoEngineFactory,
             TokenCluster tokenCluster, IBrowserFactory browserFactory)
             : base(channel, cryptoEngineFactory, tokenCluster)
        {

            this.browserFactory = browserFactory;

        }


        /// <summary>
        /// Creates a newww <see cref="Builder"/> instance that is used to configure and
        /// </summary>
        /// <returns>the builder</returns>
        public static Builder NewBuilder()
        {
            return new Builder();
        }

        /// <summary>
        /// Create the specified cluster.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="cluster">Cluster.</param>
        public static TokenClient Create(TokenCluster cluster)
        {
            return (Tokenio.User.TokenClient)NewBuilder()
            .ConnectTo(cluster)
            .Build();
        }


        /// <summary>
        /// Creates a new instance of <see cref="TokenIO"/> that's configured to use
        /// the specified environment.
        /// </summary>
        /// <param name="cluster">the token cluster to connect to</param>
        /// <param name="developerKey">the developer key</param>
        /// <returns>an instance of <see cref="TokenIO"/></returns>
        public static TokenClient Create(TokenCluster cluster, string developerKey)
        {


            return (Tokenio.User.TokenClient)NewBuilder()
            .ConnectTo(cluster)
            .DeveloperKey(developerKey)
            .Build();
        }



        /// <summary>
        /// Creates a new Token member with a set of auto-generated keys, an alias, and member type.
        /// </summary>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias
        /// will be created with the member</param>
        /// <param name="createMemberType">the type of member to register</param>
        /// <returns>the created member</returns>
        public Task<Member> CreateMember(
            Alias alias ,string recoveryAgent=null)
        {
            return CreateMemberImpl(alias, CreateMemberType.Personal,recoveryAgent)
                .Map(member =>
                {
                    var crypto = cryptoEngineFactory.Create(member.MemberId());
                    var client = ClientFactory.Authenticated(channel, member.MemberId(), crypto);
                    return new Member(member.MemberId(),client,browserFactory);
                });
        }


        /// <summary>
        /// Creates a new Token member with a set of auto-generated keys, an alias, and member type.
        /// </summary>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias
        /// will be created with the member</param>
        /// <param name="createMemberType">the type of member to register</param>
        /// <returns>the created member</returns>
        public Member CreateMemberBlocking(
            Alias alias ,
           string recoveryAgent=null)
        {
            return CreateMember(alias, recoveryAgent).Result;
        }

        /// <summary>
        /// Sets up member.
        /// </summary>
        /// <returns>The up member.</returns>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="alias">Alias.</param>
        public Task<Member> SetUpMember(string memberId,
            Alias alias)
        {
            return SetUpMemberImpl(memberId,alias)
                .Map(member =>
                {
                    var crypto = cryptoEngineFactory.Create(member.MemberId());
                    var client = ClientFactory.Authenticated(channel, member.MemberId(), crypto);
                    return new Member(member.MemberId(),client, browserFactory);
                });
        }

        public Member SetUpMemberBlocking(string memberId,
            Alias alias = null)
        {
            return SetUpMember(memberId, alias).Result;
        }

        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">the member ID</param>
        /// <returns>the member</returns>
        public Task<Member> GetMember(string memberId)
        {
            var crypto = cryptoEngineFactory.Create(memberId);
            var client = ClientFactory.Authenticated(channel, memberId, crypto);

            return GetMemberImpl(memberId,client)
             .Map(member =>
             {
                
                 return new Member(member.MemberId(),client, browserFactory);
             });
        }

        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">the member ID</param>
        /// <returns>the member</returns>
        public Member GetMemberBlocking(string memberId)
        {
            return GetMember(memberId).Result;
        }

        /// <summary>
        /// Returns a token request for a specified token request id.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <returns>the token request</returns>
        public Task<TokenRequest> RetrieveTokenRequest(string requestId)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.RetrieveTokenRequest(requestId)
                .Map(tokenRequest => TokenRequest.fromProtos(
                    tokenRequest.RequestPayload,
                    tokenRequest.RequestOptions));
        }

        /// <summary>
        /// Returns a token request for a specified token request id.
        /// </summary>
        /// <param name="requestId">the request id</param>
        /// <returns>the token request</returns>
        public TokenRequest RetrieveTokenRequestBlocking(string requestId)
        {
            return RetrieveTokenRequest(requestId).Result;
        }



        /// <summary>
        /// Completes account recovery.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="recoveryOperations">the member recovery operations</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>the new member</returns>
        public Task<Member> CompleteRecovery(
            string memberId,
            IList<MemberRecoveryOperation> recoveryOperations,
            Key privilegedKey,
            ICryptoEngine cryptoEngine)
        {
            return CompleteRecoveryImpl(memberId, recoveryOperations, privilegedKey, cryptoEngine)
                .Map(member =>
                {
                    var client = ClientFactory.Authenticated(channel, member.MemberId(), cryptoEngine);
                    return new Member(member.MemberId(),client, browserFactory);
                });
        }

        /// <summary>
        /// Completes account recovery.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="recoveryOperations">the member recovery operations</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>the new member</returns>
        public Member CompleteRecoveryBlocking(
            string memberId,
            IList<MemberRecoveryOperation> recoveryOperations,
            Key privilegedKey,
            ICryptoEngine cryptoEngine)
        {
            return CompleteRecovery(memberId, recoveryOperations, privilegedKey, cryptoEngine).Result;
        }

        /// <summary>
        /// Completes account recovery if the default recovery rule was set.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the code</param>
        /// <returns>the new member</returns>
        public Task<Member> CompleteRecoveryWithDefaultRule(
            string memberId,
            string verificationId,
            string code)
        {
            var cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());

            return CompleteRecoveryWithDefaultRuleImpl(memberId, verificationId, code, cryptoEngine)
                .Map(member =>
                {
                    var client = ClientFactory.Authenticated(channel, member.MemberId(), cryptoEngine);
                    return new Member(member.MemberId(),client, browserFactory);
                });
        }

        /// <summary>
        /// Completes account recovery if the default recovery rule was set.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="verificationId">the verification id</param>
        /// <param name="code">the code</param>
        /// <returns>the new member</returns>
        public Member CompleteRecoveryWithDefaultRuleBlocking(
            string memberId,
            string verificationId,
            string code)
        {
            return CompleteRecoveryWithDefaultRule(memberId, verificationId, code).Result;


        }




        /// <summary>
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request result</returns>
        public Task<TokenRequestResult> GetTokenRequestResult(string tokenRequestId)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetTokenRequestResult(tokenRequestId);
        }

        /// <summary>
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request result</returns>
        public TokenRequestResult GetTokenRequestResultBlocking(string tokenRequestId)
        {
            return GetTokenRequestResult(tokenRequestId).Result;
        }

        /// <summary>
        /// Provisions the device.
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="alias">Alias.</param>
        public Task<DeviceInfo> ProvisionDevice(Alias alias)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetMemberId(alias)
                .Map(memberId =>
                {
                    //var cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
                    var cryptoEngine = cryptoEngineFactory.Create(memberId);
                    var Keys = new List<Key> {
                                            cryptoEngine.GenerateKey(Level.Privileged),
                                            cryptoEngine.GenerateKey(Level.Standard),
                                            cryptoEngine.GenerateKey(Level.Low)
                                        };
                    return new DeviceInfo(memberId, Keys);
                    ;
                });

        }

        /// <summary>
        /// Provisions the device blocking.
        /// </summary>
        /// <returns>The device blocking.</returns>
        /// <param name="alias">Alias.</param>
        public DeviceInfo ProvisionDeviceBlocking(Alias alias)
        {
            return ProvisionDevice(alias).Result;
        }

        /// <summary>
        /// Notifies the add key.
        /// </summary>
        /// <returns>The add key.</returns>
        /// <param name="alias">Alias.</param>
        /// <param name="keys">Keys.</param>
        /// <param name="deviceMetadata">Device metadata.</param>
        public Task<NotifyStatus> NotifyAddKey(Alias alias, IList<Key> keys, DeviceMetadata deviceMetadata)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            var addKey = new AddKey
            {
                DeviceMetadata = deviceMetadata

            };
            addKey.Keys.Add(keys);
            return unauthenticated.NotifyAddKey(alias, addKey);
        }

        /// <summary>
        /// Notifies the add key blocking.
        /// </summary>
        /// <returns>The add key blocking.</returns>
        /// <param name="alias">Alias.</param>
        /// <param name="keys">Keys.</param>
        /// <param name="deviceMetadata">Device metadata.</param>
        public NotifyStatus NotifyAddKeyBlocking(
            Alias alias,
            IList<Key> keys,
            DeviceMetadata deviceMetadata)
        {
            return NotifyAddKey(alias, keys, deviceMetadata).Result;
        }


        /// <summary>
        /// Notifies the payment request.
        /// </summary>
        /// <returns>The payment request.</returns>
        /// <param name="tokenPayload">Token payload.</param>
        public Task<NotifyStatus> NotifyPaymentRequest(TokenPayload tokenPayload)
        {
            UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
            if (tokenPayload.RefId.Length == 0)
            {
                tokenPayload.RefId = Util.Nonce();
            }
            return unauthenticated.NotifyPaymentRequest(tokenPayload);
        }

        /// <summary>
        /// Notifies the payment request blocking.
        /// </summary>
        /// <returns>The payment request blocking.</returns>
        /// <param name="tokenPayload">Token payload.</param>
        public NotifyStatus NotifyPaymentRequestBlocking(TokenPayload tokenPayload)
        {
            return NotifyPaymentRequest(tokenPayload).Result;
        }


        /// <summary>
        /// Notifies the create and endorse token.
        /// </summary>
        /// <returns>The create and endorse token.</returns>
        /// <param name="tokenRequestId">Token request identifier.</param>
        /// <param name="keys">Keys.</param>
        /// <param name="deviceMetadata">Device metadata.</param>
        /// <param name="receiptContact">Receipt contact.</param>
        public Task<NotifyResult> NotifyCreateAndEndorseToken(
           string tokenRequestId,
          IList<Key> keys,
           DeviceMetadata deviceMetadata,
          ReceiptContact receiptContact)
        {
            UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);

            var addKey = new AddKey
            {
                DeviceMetadata = deviceMetadata

            };
            addKey.Keys.Add(keys);
            return unauthenticated.NotifyCreateAndEndorseToken(
                    tokenRequestId,
                    addKey,
                    receiptContact);
        }

        /// <summary>
        /// Notifies the create and endorse token blocking.
        /// </summary>
        /// <returns>The create and endorse token blocking.</returns>
        /// <param name="tokenRequestId">Token request identifier.</param>
        /// <param name="keys">Keys.</param>
        /// <param name="deviceMetadata">Device metadata.</param>
        /// <param name="receiptContact">Receipt contact.</param>
        public NotifyResult NotifyCreateAndEndorseTokenBlocking(
            string tokenRequestId,
           IList<Key> keys,
            DeviceMetadata deviceMetadata,
           ReceiptContact receiptContact)
        {
            return NotifyCreateAndEndorseToken(tokenRequestId, keys, deviceMetadata, receiptContact)
                    .Result;
        }

        /// <summary>
        /// Invalidates the notification.
        /// </summary>
        /// <returns>The notification.</returns>
        /// <param name="notificationId">Notification identifier.</param>
        public Task<NotifyStatus> InvalidateNotification(string notificationId)
        {
            UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.InvalidateNotification(notificationId);
        }

        /// <summary>
        /// Invalidates the notification blocking.
        /// </summary>
        /// <returns>The notification blocking.</returns>
        /// <param name="notificationId">Notification identifier.</param>
        public NotifyStatus InvalidateNotificationBlocking(string notificationId)
        {
            return InvalidateNotification(notificationId).Result;
        }

        /// <summary>
        /// Gets the BLOB.
        /// </summary>
        /// <returns>The BLOB.</returns>
        /// <param name="blobId">BLOB identifier.</param>
        public Task<Blob> GetBlob(string blobId)
        {
            UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetBlob(blobId);

        }

        /// <summary>
        /// Gets the BLOB blocking.
        /// </summary>
        /// <returns>The BLOB blocking.</returns>
        /// <param name="blobId">BLOB identifier.</param>
        public Blob GetBlobBlocking(string blobId)
        {
            return GetBlob(blobId).Result;
        }

        /// <summary>
        /// Updates the token request.
        /// </summary>
        /// <returns>The token request.</returns>
        /// <param name="requestId">Request identifier.</param>
        /// <param name="options">Options.</param>
        public Task UpdateTokenRequest(string requestId, TokenRequestOptions options)
        {
            UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.UpdateTokenRequest(requestId, options);
        }

        /// <summary>
        /// Updates the token request blocking.
        /// </summary>
        /// <param name="requestId">Request identifier.</param>
        /// <param name="options">Options.</param>
        public void UpdateTokenRequestBlocking(string requestId, TokenRequestOptions options)
        {
            UpdateTokenRequest(requestId, options).Wait();
        }

        public class Builder : Tokenio.TokenClient.Builder
        {
            private IBrowserFactory browserFactory;

            /// <summary>
            /// Withs the browser factory.
            /// </summary>
            /// <returns>The browser factory.</returns>
            /// <param name="browserFactory">Browser factory.</param>
            public Builder withBrowserFactory(IBrowserFactory browserFactory)
            {
                this.browserFactory = browserFactory;
                return this;
            }

            /// <summary>
            /// Build this instance.
            /// </summary>
            /// <returns>The build.</returns>
            public override Tokenio.TokenClient Build()
            {
                var channel = new Channel(hostName, port, useSsl ? new SslCredentials() : ChannelCredentials.Insecure);
                Interceptor[] interceptors =
                {
                            new Tokenio.Rpc.AsyncTimeoutInterceptor(timeoutMs),
                            new Tokenio.Rpc.AsyncMetadataInterceptor(metadata =>
                            {
                                metadata.Add("token-sdk", "csharp");
                                metadata.Add(
                                    "token-sdk-version",
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
                                metadata.Add("token-dev-key", devKey);
                                return metadata;
                            })
                        };
                var newChannel = new Tokenio.Rpc.ManagedChannel(channel, interceptors);

                return new TokenClient(
                    newChannel,
                    cryptoEngine ?? new TokenCryptoEngineFactory(new InMemoryKeyStore()),
                    tokenCluster ?? TokenCluster.SANDBOX, browserFactory);
            }


        }



    }



}
