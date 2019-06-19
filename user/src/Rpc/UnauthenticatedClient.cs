using System.Threading.Tasks;
using Tokenio.Proto.Gateway;
using Tokenio.TokenRequests;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using ReceiptContact = Tokenio.Proto.Common.MemberProtos.ReceiptContact;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Proto.Common.AliasProtos;
using static Tokenio.Proto.Common.BlobProtos.Blob.Types;
using Tokenio.Proto.Common.BlobProtos;



namespace Tokenio.User.Rpc
{
    /// <summary>
    /// Similar to <see cref="Client"/> but is only used for a handful of requests that
    /// don't require authentication. We use this client to create new member or getMember
    /// an existing one and switch to the authenticated <see cref="Client"/>.
    /// </summary>
    public sealed class UnauthenticatedClient : Tokenio.Rpc.UnauthenticatedClient
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="gateway">the gateway gRPC client</param>
        public UnauthenticatedClient(GatewayService.GatewayServiceClient gateway)
            : base(gateway)
        {
        }



        /// <summary>
        /// Get the token request result based on a token's tokenRequestId.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request result</returns>
        public Task<TokenRequestResult> GetTokenRequestResult(string tokenRequestId)
        {
            var request = new GetTokenRequestResultRequest { TokenRequestId = tokenRequestId };
            return gateway.GetTokenRequestResultAsync(request)
                .ToTask(response => new TokenRequestResult(response.TokenId, response.Signature));
        }

        /// <summary>
        /// Retrieves a transfer token request.
        /// </summary>
        /// <param name="tokenRequestId">the token request id</param>
        /// <returns>the token request</returns>
        public Task<Proto.Common.TokenProtos.TokenRequest> RetrieveTokenRequest(string tokenRequestId)
        {
            var request = new RetrieveTokenRequestRequest { RequestId = tokenRequestId };
            return gateway.RetrieveTokenRequestAsync(request)
                .ToTask(response => response.TokenRequest);
        }


        /// <summary>
        /// Notifies subscribed devices that a key should be added.
        /// </summary>
        /// <param name="alias">alias of the member</param>
        /// <param name="addKey">AddKey payload to be sent</param>
        /// <returns></returns>
        public Task<NotifyStatus> NotifyAddKey(Alias alias, AddKey addKey)
        {
            var request = new NotifyRequest
            {
                Alias = alias,
                Body = new NotifyBody
                {
                    AddKey = addKey
                }
            };
            return gateway.NotifyAsync(request)
                .ToTask(response => response.Status);
        }



        /// <summary>
        /// Notifies subscribed devices of payment requests.
        /// </summary>
        /// <param name="tokenPayload">the payload of a token to be sent</param>
        /// <returns>status of the notification request</returns>
        public Task<NotifyStatus> NotifyPaymentRequest(TokenPayload tokenPayload)
        {
            var request = new RequestTransferRequest { TokenPayload = tokenPayload };
            return gateway.RequestTransferAsync(request)
                .ToTask(response => response.Status);
        }


        /// <summary>
        /// Notifies subscribed devices that a token should be created and endorsed.
        /// </summary>
        /// <returns> notify result of the notification request.</returns>
        /// <param name="tokenRequestId">tokenRequestId the token request ID to send.</param>
        /// <param name="addKey">addKey optional add key payload to send.</param>
        /// <param name="receiptContact">receiptContact optional receipt contact to send.</param>
        public Task<NotifyResult> NotifyCreateAndEndorseToken(
            string tokenRequestId,
            AddKey addKey,
            ReceiptContact receiptContact)
        {
            var request = new TriggerCreateAndEndorseTokenNotificationRequest
            {

                TokenRequestId = tokenRequestId

            };
            if (addKey != null)
            {
                request.AddKey = addKey;
            }

            if (receiptContact != null)
            {
                request.Contact = receiptContact;
            }

            return gateway.TriggerCreateAndEndorseTokenNotificationAsync(request)
                 .ToTask(response => NotifyResult.Create(response.NotificationId, response.Status));
        }

        /// <summary>
        /// Invalidates the notification.
        /// </summary>
        /// <returns>The notification.</returns>
        /// <param name="notificationId">Notification identifier.</param>
        public Task<NotifyStatus> InvalidateNotification(string notificationId)
        {

            var invalidateNotificationRequest = new InvalidateNotificationRequest() {
                NotificationId = notificationId
            };
            return gateway.InvalidateNotificationAsync(invalidateNotificationRequest)
                    .ToTask(response => response.Status);
        }

        /// <summary>
        /// Gets the BLOB.
        /// </summary>
        /// <returns>The BLOB.</returns>
        /// <param name="blobId">BLOB identifier.</param>
        public Task<Blob> GetBlob(string blobId)
        {
            var request = new GetBlobRequest { BlobId = blobId };
            return gateway.GetBlobAsync(request)
                .ToTask(response => response.Blob);
        }

        /// <summary>
        /// Updates the token request.
        /// </summary>
        /// <returns>The token request.</returns>
        /// <param name="requestId">Request identifier.</param>
        /// <param name="options">Options.</param>
        public Task UpdateTokenRequest(string requestId, TokenRequestOptions options)
        {
            var builder = new UpdateTokenRequestRequest()
            {
                RequestId = requestId,
                RequestOptions = options
            };
            return  gateway
                    .UpdateTokenRequestAsync(builder).ToTask();
        }



    }
}
