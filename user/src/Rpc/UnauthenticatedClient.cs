using System.Threading.Tasks;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.BlobProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Gateway;
using Tokenio.TokenRequests;
using ReceiptContact = Tokenio.Proto.Common.MemberProtos.ReceiptContact;

namespace Tokenio.User.Rpc {
	/// <summary>
	/// Similar to <see cref = "Client"/> but is only used for a handful of requests that don't require authentication.
	/// We use this client to create new member or getMember an existing one and switch to the authenticated <see cref = "Client"/>.
	/// </summary>
	public sealed class UnauthenticatedClient : Tokenio.Rpc.UnauthenticatedClient {
		/// <summary>
		/// Creates an instance.
		/// </summary>
		/// <param name = "gateway">The gateway gRPC client</param>
		public UnauthenticatedClient(GatewayService.GatewayServiceClient gateway) : base(gateway) {
		}

		/// <summary>
		/// Get the token request result based on a token's tokenRequestId.
		/// </summary>
		/// <param name = "tokenRequestId">The token request id</param>
		/// <returns>The token request result</returns>
		public Task<TokenRequestResult> GetTokenRequestResult(string tokenRequestId) {
			var request = new GetTokenRequestResultRequest {
				TokenRequestId = tokenRequestId
			};
			return gateway.GetTokenRequestResultAsync(request)
					.ToTask(response =>
							new TokenRequestResult(response.TokenId, response.Signature));
		}

		/// <summary>
		/// Retrieves a transfer token request.
		/// </summary>
		/// <param name = "tokenRequestId">The token request id</param>
		/// <returns>The token request</returns>
		public Task<Proto.Common.TokenProtos.TokenRequest> RetrieveTokenRequest(string tokenRequestId) {
			var request = new RetrieveTokenRequestRequest {
				RequestId = tokenRequestId
			};
			return gateway.RetrieveTokenRequestAsync(request)
					.ToTask(response =>
							response.TokenRequest);
		}

		/// <summary>
		/// Notifies subscribed devices that a key should be added.
		/// </summary>
		/// <param name = "alias">Alias of the member</param>
		/// <param name = "addKey">AddKey payload to be sent</param>
		/// <returns>Status of the notification</returns>
		public Task<NotifyStatus> NotifyAddKey(Alias alias, AddKey addKey) {
			var request = new NotifyRequest {
				Alias = alias,
				Body = new NotifyBody {
					AddKey = addKey
				}
			};
			return gateway.NotifyAsync(request)
					.ToTask(response =>
							response.Status);
		}

		/// <summary>
		/// Notifies subscribed devices of payment requests.
		/// </summary>
		/// <param name = "tokenPayload">The payload of a token to be sent</param>
		/// <returns>Status of the notification request</returns>
		public Task<NotifyStatus> NotifyPaymentRequest(TokenPayload tokenPayload) {
			var request = new RequestTransferRequest {
				TokenPayload = tokenPayload
			};
			return gateway.RequestTransferAsync(request)
					.ToTask(response =>
							response.Status);
		}

		/// <summary>
		/// Notifies subscribed devices that a token should be created and endorsed.
		/// </summary>
		/// <param name = "tokenRequestId">TokenRequestId the token request ID to send.</param>
		/// <param name = "addKey">AddKey optional add key payload to send.</param>
		/// <param name = "receiptContact">ReceiptContact optional receipt contact to send.</param>
		/// <returns>Result of the notification request.</returns>
		public Task<NotifyResult> NotifyCreateAndEndorseToken(
				string tokenRequestId,
				AddKey addKey,
				ReceiptContact receiptContact) {
			var request = new TriggerCreateAndEndorseTokenNotificationRequest {
				TokenRequestId = tokenRequestId
			};
			if (addKey != null) {
				request.AddKey = addKey;
			}
			if (receiptContact != null) {
				request.Contact = receiptContact;
			}
			return gateway.TriggerCreateAndEndorseTokenNotificationAsync(request)
					 .ToTask(response =>
							NotifyResult.Create(response.NotificationId, response.Status));
		}

		/// <summary>
		/// Invalidates the notification.
		/// </summary>
		/// <param name = "notificationId">Notification id to invalidate.</param>
		/// <returns>Status of the invalidation request.</returns>
		public Task<NotifyStatus> InvalidateNotification(string notificationId) {
			var invalidateNotificationRequest = new InvalidateNotificationRequest() {
				NotificationId = notificationId
			};
			return gateway.InvalidateNotificationAsync(invalidateNotificationRequest)
					.ToTask(response =>
							response.Status);
		}

		/// <summary>
		/// Retrieves a blob from the server.
		/// </summary>
		/// <param name="blobId">BLOB identifier.</param>
		/// <returns>The BLOB.</returns>
		public Task<Blob> GetBlob(string blobId) {
			var request = new GetBlobRequest {
				BlobId = blobId
			};
			return gateway.GetBlobAsync(request)
					.ToTask(response =>
							response.Blob);
		}

		/// <summary>
		/// Updates an existing token request.
		/// </summary>
		/// <param name = "requestId">Request identifier.</param>
		/// <param name = "options">Options.</param>
		/// <returns>The token request.</returns>
		public Task UpdateTokenRequest(string requestId, TokenRequestOptions options) {
			var builder = new UpdateTokenRequestRequest() {
				RequestId = requestId,
				RequestOptions = options
			};
			return gateway
					.UpdateTokenRequestAsync(builder).ToTask();
		}
	}
}