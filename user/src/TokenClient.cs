using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Collections.Generic;
using System.Reflection;
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
using TokenRequest = Tokenio.TokenRequests.TokenRequest;

namespace Tokenio.User {
	public class TokenClient : Tokenio.TokenClient {
		/// <summary>
		/// The browser factory.
		/// </summary>
		private readonly IBrowserFactory browserFactory;

		/// <summary>
		/// Initializes a new instance of the <see cref = "T:Tokenio.User.TokenClient"/> class.
		/// </summary>
		/// <param name = "channel">Channel.</param>
		/// <param name="cryptoEngineFactory">Crypto engine factory.</param>
		/// <param name="tokenCluster">Token cluster.</param>
		/// <param name="browserFactory">Browser factory.</param>
		public TokenClient(
				 Tokenio.Rpc.ManagedChannel channel,
				 ICryptoEngineFactory cryptoEngineFactory,
				 TokenCluster tokenCluster, IBrowserFactory browserFactory) : base(channel, cryptoEngineFactory, tokenCluster) {
			this.browserFactory = browserFactory;
		}

		/// <summary>
		/// Creates a newww <see cref="Builder"/> instance that is used to configure and
		/// build a {@link TokenClient} instance.
		/// </summary>
		/// <returns>the builder</returns>
		public static Builder NewBuilder() {
			return new Builder();
		}

		/// <summary>
		/// Creates a new instance of {@link TokenClient} that's configured to use
		/// the specified environment.
		/// </summary>
		/// <returns>The create.</returns>
		/// <param name="cluster">Cluster.</param>
		public static TokenClient Create(TokenCluster cluster) {
			return (Tokenio.User.TokenClient) NewBuilder()
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
		public static TokenClient Create(TokenCluster cluster, string developerKey) {
			return (Tokenio.User.TokenClient) NewBuilder()
					.ConnectTo(cluster)
					.DeveloperKey(developerKey)
					.Build();
		}

        /// <summary>
        /// Creates a new Token member with a set of auto-generated keys, an alias, and member type.
        /// </summary>
        /// <returns>The member.</returns>
        /// <param name="alias">Alias.</param>
        /// <param name="recoveryAgent">Recovery agent.</param>
        /// <param name="realmId">Realm identifier.</param>
        public Task<Member> CreateMember(
				Alias alias, 
                string recoveryAgent = null,
                string realmId=null) {
			return CreateMemberImpl(alias, CreateMemberType.Personal, recoveryAgent,null,realmId)
					.Map(member => {
						var crypto = cryptoEngineFactory.Create(member.MemberId());
						var client = ClientFactory.Authenticated(channel, member.MemberId(), crypto);
						return new Member(member.MemberId(), client, tokenCluster,member.PartnerId()
                            ,member.RealmId(), browserFactory);
					});
		}

		/// <summary>
		/// Creates a new personal-use Token member for the Banks with a set of
		/// auto-generated keys, an alias and recovery agent set as the Bank..
		/// </summary>
		/// <param name="alias">nullable member alias to use, must be unique. If null, then no alias
		/// will be created with the member</param>
		/// <param name="createMemberType">memver id of the primary recovery agent</param>
		/// <returns>newly created member</returns>
		public Member CreateMemberBlocking(
				Alias alias,
			   string recoveryAgent = null) {
			return CreateMember(alias, recoveryAgent, null).Result;
		}


        /// <summary>
        /// Creates the member in realm blocking.
        /// </summary>
        /// <returns>The member in realm blocking.</returns>
        /// <param name="alias">Alias.</param>
        /// <param name="realmId">Realm identifier.</param>
        public Member CreateMemberInRealmBlocking(Alias alias, string realmId)
        {
            return CreateMember(alias, realmId, realmId).Result;
        }



        /// <summary>
        /// Sets up a member given a specific ID of a member that already exists in the system. If
        /// the member ID already has keys, this will not succeed.Used for testing since this
        /// gives more control over the member creation process.
        /// </summary>
        /// <returns>The up member.</returns>
        /// <param name="memberId">Member identifier.</param>
        /// <param name="alias">nullable member alias to use, must be unique. If null, then no alias will be created with the member</param>
        public Task<Member> SetUpMember(string memberId,
				Alias alias) {
			return SetUpMemberImpl(memberId, alias)
					.Map(member => {
						var crypto = cryptoEngineFactory.Create(member.MemberId());
						var client = ClientFactory.Authenticated(channel, member.MemberId(), crypto);
						return new Member(member.MemberId(), client,tokenCluster ,
                            member.PartnerId(),member.RealmId(),browserFactory);
					});
		}

		public Member SetUpMemberBlocking(string memberId,
				Alias alias = null) {
			return SetUpMember(memberId, alias).Result;
		}

		/// <summary>
		/// Return a Member set up to use some Token member's keys (assuming we have them).
		/// </summary>
		/// <param name="memberId">the member ID</param>
		/// <returns>the member</returns>
		public Task<Member> GetMember(string memberId) {
			var crypto = cryptoEngineFactory.Create(memberId);
			var client = ClientFactory.Authenticated(channel, memberId, crypto);
			return GetMemberImpl(memberId, client)
					.Map(member => {
						return new Member(member.MemberId(), client,tokenCluster ,
                            member.PartnerId(),
                            member.RealmId(),browserFactory);
					});
		}

		/// <summary>
		/// Return a Member set up to use some Token member's keys (assuming we have them).
		/// </summary>
		/// <param name="memberId">the member ID</param>
		/// <returns>the member</returns>
		public Member GetMemberBlocking(string memberId) {
			return GetMember(memberId).Result;
		}

		/// <summary>
		/// Returns a token request for a specified token request id that was previously stored.
		/// </summary>
		/// <param name="requestId">token request that was stored with the request id</param>
		/// <returns>token request that was stored with the request idt</returns>
		public Task<TokenRequest> RetrieveTokenRequest(string requestId) {
			var unauthenticated = ClientFactory.Unauthenticated(channel);
			return unauthenticated.RetrieveTokenRequest(requestId)
					.Map(tokenRequest =>
							TokenRequest.fromProtos(
									tokenRequest.RequestPayload,
									tokenRequest.RequestOptions));
		}

		/// <summary>
		/// Returns a token request for a specified token request id.
		/// </summary>
		/// <param name="requestId">the request id</param>
		/// <returns>token request that was stored with the request id</returns>
		public TokenRequest RetrieveTokenRequestBlocking(string requestId) {
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
				ICryptoEngine cryptoEngine) {
			return CompleteRecoveryImpl(memberId, recoveryOperations, privilegedKey, cryptoEngine)
					.Map(member => {
						var client = ClientFactory.Authenticated(channel, member.MemberId(), cryptoEngine);
						return new Member(member.MemberId(), client,tokenCluster ,
                            member.PartnerId(),
                            member.RealmId(),browserFactory);
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
				ICryptoEngine cryptoEngine) {
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
				ICryptoEngine cryptoEngine) {
			return CompleteRecoveryWithDefaultRuleImpl(memberId, verificationId, code, cryptoEngine)
					.Map(member => {
						var client = ClientFactory.Authenticated(channel, member.MemberId(), cryptoEngine);
						return new Member(member.MemberId(), client,tokenCluster ,
                            member.PartnerId(),
                            member.RealmId(),browserFactory);
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
				ICryptoEngine cryptoEngine) {
			return CompleteRecoveryWithDefaultRule(memberId, verificationId, code, cryptoEngine).Result;
		}

		/// <summary>
		/// Get the token request result based on a token's tokenRequestId.
		/// </summary>
		/// <param name="tokenRequestId">the token request id</param>
		/// <returns>the token request result</returns>
		public Task<TokenRequestResult> GetTokenRequestResult(string tokenRequestId) {
			var unauthenticated = ClientFactory.Unauthenticated(channel);
			return unauthenticated.GetTokenRequestResult(tokenRequestId);
		}

		/// <summary>
		/// Get the token request result based on a token's tokenRequestId.
		/// </summary>
		/// <param name="tokenRequestId">the token request id</param>
		/// <returns>the token request result</returns>
		public TokenRequestResult GetTokenRequestResultBlocking(string tokenRequestId) {
			return GetTokenRequestResult(tokenRequestId).Result;
		}

		/// <summary>
		/// Provisions a new device for an existing user. The call generates a set
		/// of keys that are returned back.The keys need to be approved by an
		/// existing device/keys.
		/// </summary>
		/// <returns>The device.</returns>
		/// <param name="alias">member id to provision the device for</param>
		public Task<DeviceInfo> ProvisionDevice(Alias alias) {
			var unauthenticated = ClientFactory.Unauthenticated(channel);
			return unauthenticated.GetMemberId(alias)
					.Map(memberId => {
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
		/// existing device/keys..
		/// </summary>
		/// <returns>The device blocking.</returns>
		/// <param name="alias">Alias.</param>
		public DeviceInfo ProvisionDeviceBlocking(Alias alias) {
			return ProvisionDevice(alias).Result;
		}

		/// <summary>
		/// Notifies the add key.
		/// </summary>
		/// <returns>The add key.</returns>
		/// <param name="alias">Alias.</param>
		/// <param name="keys">Keys.</param>
		/// <param name="deviceMetadata">Device metadata.</param>
		public Task<NotifyStatus> NotifyAddKey(Alias alias, IList<Key> keys, DeviceMetadata deviceMetadata) {
			var unauthenticated = ClientFactory.Unauthenticated(channel);
			var addKey = new AddKey {
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
				DeviceMetadata deviceMetadata) {
			return NotifyAddKey(alias, keys, deviceMetadata).Result;
		}

		/// <summary>
		/// Notifies the payment request.
		/// </summary>
		/// <returns>The payment request.</returns>
		/// <param name="tokenPayload">Token payload.</param>
		public Task<NotifyStatus> NotifyPaymentRequest(TokenPayload tokenPayload) {
			UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
			if (tokenPayload.RefId.Length == 0) {
				tokenPayload.RefId = Util.Nonce();
			}
			return unauthenticated.NotifyPaymentRequest(tokenPayload);
		}

		/// <summary>
		/// Notifies the payment request blocking.
		/// </summary>
		/// <returns>The payment request blocking.</returns>
		/// <param name="tokenPayload">Token payload.</param>
		public NotifyStatus NotifyPaymentRequestBlocking(TokenPayload tokenPayload) {
			return NotifyPaymentRequest(tokenPayload).Result;
		}

		/// <summary>
		/// Notifies subscribed devices that a token should be created and endorsed.
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
			  ReceiptContact receiptContact) {
			UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
			var addKey = new AddKey {
				DeviceMetadata = deviceMetadata
			};
			addKey.Keys.Add(keys);
			return unauthenticated.NotifyCreateAndEndorseToken(
					tokenRequestId,
					addKey,
					receiptContact);
		}

		/// <summary>
		/// Notifies subscribed devices that a token should be created and endorsed..
		/// </summary>
		/// <returns>notify result of the notification request.</returns>
		/// <param name="tokenRequestId">the token request ID to send.</param>
		/// <param name="keys">keys to be added.</param>
		/// <param name="deviceMetadata">device metadata of the keys.</param>
		/// <param name="receiptContact">optional receipt contact to send.</param>
		public NotifyResult NotifyCreateAndEndorseTokenBlocking(
				string tokenRequestId,
			   IList<Key> keys,
				DeviceMetadata deviceMetadata,
			   ReceiptContact receiptContact) {
			return NotifyCreateAndEndorseToken(tokenRequestId, keys, deviceMetadata, receiptContact).Result;
		}

		/// <summary>
		/// Invalidates the notification.
		/// </summary>
		/// <returns>The notification.</returns>
		/// <param name="notificationId">Notification identifier.</param>
		public Task<NotifyStatus> InvalidateNotification(string notificationId) {
			UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
			return unauthenticated.InvalidateNotification(notificationId);
		}

		/// <summary>
		/// Invalidates the notification blocking.
		/// </summary>
		/// <returns>The notification blocking.</returns>
		/// <param name="notificationId">Notification identifier.</param>
		public NotifyStatus InvalidateNotificationBlocking(string notificationId) {
			return InvalidateNotification(notificationId).Result;
		}

		/// <summary>
		/// Gets the BLOB.
		/// </summary>
		/// <returns>The BLOB.</returns>
		/// <param name="blobId">BLOB identifier.</param>
		public Task<Blob> GetBlob(string blobId) {
			UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
			return unauthenticated.GetBlob(blobId);
		}

		/// <summary>
		/// Gets the BLOB blocking.
		/// </summary>
		/// <returns>The BLOB blocking.</returns>
		/// <param name="blobId">BLOB identifier.</param>
		public Blob GetBlobBlocking(string blobId) {
			return GetBlob(blobId).Result;
		}

		/// <summary>
		/// Updates the token request.
		/// </summary>
		/// <returns>The token request.</returns>
		/// <param name="requestId">token request ID.</param>
		/// <param name="options">new token request options.</param>
		public Task UpdateTokenRequest(string requestId, TokenRequestOptions options) {
			UnauthenticatedClient unauthenticated = ClientFactory.Unauthenticated(channel);
			return unauthenticated.UpdateTokenRequest(requestId, options);
		}

		/// <summary>
		/// Updates the token request blocking.
		/// </summary>
		/// <param name="requestId">Request identifier.</param>
		/// <param name="options">Options.</param>
		public void UpdateTokenRequestBlocking(string requestId, TokenRequestOptions options) {
			UpdateTokenRequest(requestId, options).Wait();
		}

		public class Builder : Tokenio.TokenClient.Builder {
			private IBrowserFactory browserFactory;

			/// <summary>
			/// Sets the browser factory to be used with the SDK.
			/// </summary>
			/// <returns>The builder instance</returns>
			/// <param name="browserFactory">Browser factory.</param>
			public Builder withBrowserFactory(IBrowserFactory browserFactory) {
				this.browserFactory = browserFactory;
				return this;
			}

			/// <summary>
			/// Build this instance.
			/// </summary>
			/// <returns>The build.</returns>
			public override Tokenio.TokenClient Build() {
				var channel = new Channel(hostName, port, useSsl ? new SslCredentials() : ChannelCredentials.Insecure);
				Interceptor[] interceptors = {
					new Tokenio.Rpc.AsyncTimeoutInterceptor(timeoutMs),
					new Tokenio.Rpc.AsyncMetadataInterceptor(metadata =>
							{
								metadata.Add("token-sdk", "csharp");
								metadata.Add(
										"token-sdk-version",
										Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
								metadata.Add("token-dev-key", devKey);
								if(featureCodes != null) {
									featureCodes.ForEach(f =>
											metadata.Add(FEATURE_CODE_KEY,f));
								}
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