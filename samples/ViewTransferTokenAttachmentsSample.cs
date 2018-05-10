using Tokenio;

namespace samples
{
    public class ViewTransferTokenAttachmentsSample
    {
        /// <summary>
        /// Show how to download attachment data from a token
        /// </summary>
        /// <param name="payee">Payee member</param>
        /// <param name="tokenId">Token with attachments we want to show</param>
        public static void DisplayAttachmentFromTransferToken(
            MemberSync payee,
            string tokenId)
        {
            // Retrieve a transfer token to redeem.
            var transferToken = payee.GetToken(tokenId);

            var attachments = transferToken
                .Payload
                .Transfer
                .Attachments;
            foreach (var attachment in attachments)
            {
                // Attachment has some metadata (name, type)
                // but not the "file" contents.
                if (attachment.Type.StartsWith("image/"))
                {
                    // Download the contents for the attachment[s]
                    // we want:
                    var blob = payee.GetTokenBlob(tokenId, attachment.BlobId);
                    // Use the attachment data.
                    ShowImage(
                        blob.Payload.Name, // "invoice.jpg"
                        blob.Payload.Type, // "image/jpeg"
                        // byte[] of contents:
                        blob.Payload.Data.ToByteArray());
                }
            }
        }

        private static void ShowImage(string name, string mimeType, byte[] data)
        {
            // no-op fake to make example look plausible
        }
    }
}