using System.Collections.Generic;
using System.Threading.Tasks;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Security;
using Tokenio.TokenRequests;
using Tokenio.User.Browser;
using Tokenio.User.Rpc;
using Tokenio.User.Utils;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using ManagedChannel = Tokenio.Rpc.ManagedChannel;
using TokenRequest = Tokenio.TokenRequests.TokenRequest;

namespace Tokenio.User
{
    public class TokenClient : Tokenio.TokenClient
    {
        private readonly IBrowserFactory browserFactory;

        /// <summary>
        /// Creates an instance of a Token SDK.
        /// </summary>
        /// <param name = "channel">GRPC channel</param>
        /// <param name="cryptoEngineFactory">crypto factory instance</param>
        /// <param name="tokenCluster">token cluster</param>
        /// <param name="browserFactory">browser factory</param>
        public TokenClient(
                 ManagedChannel channel,
                 ICryptoEngineFactory cryptoEngineFactory,
                 TokenCluster tokenCluster, IBrowserFactory browserFactory) : base(channel, cryptoEngineFactory, tokenCluster)
        {
            this.browserFactory = browserFactory;
        }

        /// <summary>
        /// Creates a new {@link Builder} instance that is used to configure and
        /// build a {@link TokenClient} instance.
        /// </summary>
        /// <returns>builder</returns>
        public static Builder NewBuilder()
        {
            return new Builder();
        }

        /// <summary>
        /// Creates a new instance of {@link TokenClient} that's configured to use
        /// the specified environment.
        /// </summary>
        /// <param name="cluster">token cluster to connect to</param>
        /// <returns>{@link TokenClient} instance</returns>
        public static TokenClient Create(TokenCluster cluster)
        {
            return (Tokenio.User.TokenClient)NewBuilder()
                    .ConnectTo(cluster)
                    .Build();
        }

        /// <summary>
        /// Creates a new instance of {@link TokenClient} that's configured to use
        /// the specified environment.
        /// </summary>
        /// <param name="cluster">token cluster to connect to</param>
        /// <param name="developerKey">developer key</param>
        /// <returns>{@link TokenClient} instance</returns>
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
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias will
        ///     be created with the member.</param>
        /// <param name="recoveryAgent">member id of the primary recovery agent.</param>
        /// <param name="realmId">member id of an existing Member to whose realm this new member belongs.</param>
        /// <returns>newly created member</returns>
        public Task<Member> CreateMember(
                Alias alias,
                string recoveryAgent = null,
                string realmId = null)
        {
            return CreateMemberImpl(alias, CreateMemberType.Personal, recoveryAgent, null, realmId)
                    .Map(member =>
                    {
                        var crypto = cryptoEngineFactory.Create(member.MemberId());
                        var client = ClientFactory.Authenticated(channel, member.MemberId(), crypto);
                        return new Member(member.MemberId(), client, tokenCluster, member.PartnerId()
                            , member.RealmId(), browserFactory);
                    });
        }

        /// <summary>
        /// Creates a new personal-use Token member for the Banks with a set of
        /// auto-generated keys, an alias and recovery agent set as the Bank..
        /// </summary>
        /// <param name="alias">alias to associate with member</param>
        /// <param name="recoveryAgent">memver id of the primary recovery agent.</param>
        /// <returns>newly created member</returns>
        public Member CreateMemberBlocking(
                Alias alias,
               string recoveryAgent = null)
        {
            return CreateMember(alias, recoveryAgent, null).Result;
        }


        /// <summary>
        /// Creates a new Token member in the provided realm with a set of auto-generated keys, an alias,
        /// and member type.
        /// </summary>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias will
        ///     be created with the member.</param>
        /// <param name="realmId">member id of an existing Member to whose realm this new member belongs.</param>
        /// <returns>newly created member</returns>
        public Member CreateMemberInRealmBlocking(Alias alias, string realmId)
        {
            return CreateMember(alias, realmId, realmId).Result;
        }



        /// <summary>
        /// Sets up a member given a specific ID of a member that already exists in the system. If
        /// the member ID already has keys, this will not succeed.Used for testing since this
        /// gives more control over the member creation process.
        ///
        /// <p>Adds an alias and a set of auto-generated keys to the member.</p>
        /// </summary>
        /// <param name="memberId">member id</param>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias will
        ///     be created with the member.</param>
        /// <returns>newly created member</returns>
        public Task<Member> SetUpMember(string memberId,
                Alias alias)
        {
            return SetUpMemberImpl(memberId, alias)
                    .Map(member =>
                    {
                        var crypto = cryptoEngineFactory.Create(member.MemberId());
                        var client = ClientFactory.Authenticated(channel, member.MemberId(), crypto);
                        return new Member(member.MemberId(), client, tokenCluster,
                            member.PartnerId(), member.RealmId(), browserFactory);
                    });
        }

        /// <summary>
        /// Sets up a member given a specific ID of a member that already exists in the system. If
        /// the member ID already has keys, this will not succeed.Used for testing since this
        /// gives more control over the member creation process.
        ///
        /// <p>Adds an alias and a set of auto-generated keys to the member.</p>
        /// </summary>
        /// <param name="memberId">member id</param>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias will
        ///     be created with the member.</param>
        /// <returns>newly created member</returns>
        public Member SetUpMemberBlocking(string memberId,
                Alias alias = null)
        {
            return SetUpMember(memberId, alias).Result;
        }

        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">member id</param>
        /// <returns>member</returns>
        public Task<Member> GetMember(string memberId)
        {
            var crypto = cryptoEngineFactory.Create(memberId);
            var client = ClientFactory.Authenticated(channel, memberId, crypto);
            return GetMemberImpl(memberId, client)
                    .Map(member =>
                    {
                        return new Member(member.MemberId(), client, tokenCluster,
                            member.PartnerId(),
                            member.RealmId(), browserFactory);
                    });
        }

        /// <summary>
        /// Return a Member set up to use some Token member's keys (assuming we have them).
        /// </summary>
        /// <param name="memberId">member id</param>
        /// <returns>member</returns>
        public Member GetMemberBlocking(string memberId)
        {
            return GetMember(memberId).Result;
        }

        /// <summary>
        /// Return a TokenRequest that was previously stored.
        /// </summary>
        /// <param name="requestId">request id</param>
        /// <returns>token request that was stored with the request id</returns>
        public Task<TokenRequest> RetrieveTokenRequest(string requestId)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.RetrieveTokenRequest(requestId)
                    .Map(tokenRequest =>
                            TokenRequest.fromProtos(
                                    tokenRequest.RequestPayload,
                                    tokenRequest.RequestOptions));
        }

        /// <summary>
        /// Return a TokenRequest that was previously stored.
        /// </summary>
        /// <param name="requestId">request id</param>
        /// <returns>token request that was stored with the request id</returns>
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
        /// <returns>a task of the updated member</returns>
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
                        return new Member(member.MemberId(), client, tokenCluster,
                            member.PartnerId(),
                            member.RealmId(), browserFactory);
                    });
        }

        /// <summary>
        /// Completes account recovery.
        /// </summary>
        /// <param name="memberId">the member id</param>
        /// <param name="recoveryOperations">the member recovery operations</param>
        /// <param name="privilegedKey">the privileged public key in the member recovery operations</param>
        /// <param name="cryptoEngine">the new crypto engine</param>
        /// <returns>a task of the updated member</returns>
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
                string code,
                ICryptoEngine cryptoEngine)
        {
            return CompleteRecoveryWithDefaultRuleImpl(memberId, verificationId, code, cryptoEngine)
                    .Map(member =>
                    {
                        var client = ClientFactory.Authenticated(channel, member.MemberId(), cryptoEngine);
                        return new Member(member.MemberId(), client, tokenCluster,
                            member.PartnerId(),
                            member.RealmId(), browserFactory);
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
                string code,
                ICryptoEngine cryptoEngine)
        {
            return CompleteRecoveryWithDefaultRule(memberId, verificationId, code, cryptoEngine).Result;
        }

        /// <summary>
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">token request id</param>
        /// <returns>token request result</returns>
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
        /// Provisions a new device for an existing user. The call generates a set
        /// of keys that are returned back.The keys need to be approved by an
        /// existing device/keys.
        /// </summary>
        /// <param name="alias">member id to provision the device for</param>
        /// <returns>device information</returns>
        public Task<DeviceInfo> ProvisionDevice(Alias alias)
        {
            var unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetMemberId(alias)
                    .Map(memberId =>
                    {
                        var cryptoEngine = cryptoEngineFactory.Create(memberId);
                        var Keys = new List<Key> {
                                    cryptoEngine.GenerateKey(Level.Privileged),
                                    cryptoEngine.GenerateKey(Level.Standard),
                                    cryptoEngine.GenerateKey(Level.Low)
                                };
                        return new DeviceInfo(memberId, Keys);
                    });
        }

        /// <summary>
        /// Provisions a new device for an existing user. The call generates a set
        /// of keys that are returned back.The keys need to be approved by an
        /// existing device/keys.
        /// </summary>
        /// <param name="alias">member id to provision the device for</param>
        /// <returns>device information</returns>
        public DeviceInfo ProvisionDeviceBlocking(Alias alias)
        {
            return ProvisionDevice(alias).Result;
        }

        /// <summary>
        /// Notifies to add a key.
        /// </summary>
        /// <param name="alias">alias alias to notify</param>
        /// <param name="keys">keys that need approval</param>
        /// <param name="deviceMetadata">device metadata of the keys</param>
        /// <returns>status of the notification</returns>
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
        /// Notifies to add a key.
        /// </summary>
        /// <param name="alias">alias alias to notify</param>
        /// <param name="keys">keys that need approval</param>
        /// <param name="deviceMetadata">device metadata of the keys</param>
        /// <returns>status of the notification</returns>
        public NotifyStatus NotifyAddKeyBlocking(
                Alias alias,
                IList<Key> keys,
                DeviceMetadata deviceMetadata)
        {
            return NotifyAddKey(alias, keys, deviceMetadata).Result;
        }

        /// <summary>
        /// Sends a notification to request a payment.
        /// </summary>
        /// <param name="tokenPayload">the payload of a token to be sent</param>
        /// <returns>status of the notification request</returns>
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
        /// Sends a notification to request a payment.
        /// </summary>
        /// <param name="tokenPayload">the payload of a token to be sent</param>
        /// <returns>status of the notification request</returns>
        public NotifyStatus NotifyPaymentRequestBlocking(TokenPayload tokenPayload)
        {
            return NotifyPaymentRequest(tokenPayload).Result;
        }

        /// <summary>
        /// Notifies subscribed devices that a token should be created and endorsed.
        /// </summary>
        /// <param name="tokenRequestId">the token request ID to send</param>
        /// <param name="keys">keys to be added</param>
        /// <param name="deviceMetadata">device metadata of the keys</param>
        /// <param name="receiptContact">optional receipt contact to send</param>
        /// <returns>notify result of the notification request</returns>
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
        /// Notifies subscribed devices that a token should be created and endorsed.
        /// </summary>
        /// <param name="tokenRequestId">the token request ID to send</param>
        /// <param name="keys">keys to be added</param>
        /// <param name="deviceMetadata">device metadata of the keys</param>
        /// <param name="receiptContact">optional receipt contact to send</param>
        /// <returns>notify result of the notification request</returns>
        public NotifyResult NotifyCreateAndEndorseTokenBlocking(
                string tokenRequestId,
               IList<Key> keys,
                DeviceMetadata deviceMetadata,
               ReceiptContact receiptContact)
        {
            return NotifyCreateAndEndorseToken(tokenRequestId, keys, deviceMetadata, receiptContact).Result;
        }

        /// <summary>
        /// Invalidate a notification.
        /// </summary>
        /// <param name="notificationId">notification id to invalidate</param>
        /// <returns>status of the invalidation request</returns>
        public Task<NotifyStatus> InvalidateNotification(string notificationId)
        {
            UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.InvalidateNotification(notificationId);
        }

        /// <summary>
        /// Invalidate a notification.
        /// </summary>
        /// <param name="notificationId">notification id to invalidate</param>
        /// <returns>status of the invalidation request</returns>
        public NotifyStatus InvalidateNotificationBlocking(string notificationId)
        {
            return InvalidateNotification(notificationId).Result;
        }

        /// <summary>
        /// Retrieves a blob from the server.
        /// </summary>
        /// <param name="blobId">id of the blob</param>
        /// <returns>Blob</returns>
        public Task<Blob> GetBlob(string blobId)
        {
            UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.GetBlob(blobId);
        }

        /// <summary>
        /// Retrieves a blob from the server.
        /// </summary>
        /// <param name="blobId">id of the blob</param>
        /// <returns>Blob</returns>
        public Blob GetBlobBlocking(string blobId)
        {
            return GetBlob(blobId).Result;
        }

        /// <summary>
        /// Updates an existing token request.
        /// </summary>
        /// <param name="requestId">token request ID</param>
        /// <param name="options">new token request options</param>
        /// <returns>task</returns>
        public Task UpdateTokenRequest(string requestId, TokenRequestOptions options)
        {
            UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
            return unauthenticated.UpdateTokenRequest(requestId, options);
        }

        /// <summary>
        /// Updates an existing token request.
        /// </summary>
        /// <param name="requestId">token request ID</param>
        /// <param name="options">new token request options</param>
        /// <returns>task</returns>
        public void UpdateTokenRequestBlocking(string requestId, TokenRequestOptions options)
        {
            UpdateTokenRequest(requestId, options).Wait();
        }

        public class Builder : Tokenio.TokenClient.Builder
        {
            private IBrowserFactory browserFactory;

            /// <summary>
            /// Sets the browser factory to be used with the SDK.
            /// </summary>
            /// <param name="browserFactory">browser factory</param>
            /// <returns>this builder instance</returns>
            public Builder withBrowserFactory(IBrowserFactory browserFactory)
            {
                this.browserFactory = browserFactory;
                return this;
            }

            public override Tokenio.TokenClient Build()
            {
                var metadata = GetHeaders();
                var newChannel = ManagedChannel.NewBuilder(hostName, port, useSsl)
                    .WithTimeout(timeoutMs)
                    .WithMetadata(metadata)
                    .UseKeepAlive(keepAlive)
                    .WithKeepAliveTime(keepAliveTimeMs)
                    .Build();

                return new TokenClient(
                        newChannel,
                        cryptoEngine ?? new TokenCryptoEngineFactory(new InMemoryKeyStore()),
                        tokenCluster ?? TokenCluster.SANDBOX, browserFactory);
            }
        }
    }
}
