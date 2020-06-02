using EventType = Tokenio.Proto.Common.WebhookProtos.EventType;
using TppMember = Tokenio.Tpp.Member;
using WebhookConfig = Tokenio.Proto.Common.WebhookProtos.Webhook.Types.Config;

namespace Tokenio.Sample.Tpp
{
    /// <summary>
    /// Manage the webhook.
    /// </summary>
    public static class WebhookSample
    {
        /// <summary>
        /// Set a webhook config.
        /// </summary>
        /// <param name="tpp">The TPP member</param>
        public static void SetWebhookConfig(TppMember tpp)
        {
            WebhookConfig config = new WebhookConfig
            {
                Url = "http://example.token.io/webhook"
            };
            config.Type.Add(EventType.TransferStatusChanged);

            tpp.SetWebhookConfigBlocking(config);
        }

        /// <summary>
        /// Get a webhook config.
        /// </summary>
        /// <param name="tpp">The TPP member</param>
        public static WebhookConfig GetWebhookConfig(TppMember tpp)
        {
            return tpp.GetWebhookConfigBlocking();
        }

        /// <summary>
        /// Delete a webhook config.
        /// </summary>
        /// <param name="tpp">The TPP member</param>
        public static void DeleteWebhookConfig(TppMember tpp)
        {
            tpp.DeleteWebhookConfigBlocking();
        }
    }
}
