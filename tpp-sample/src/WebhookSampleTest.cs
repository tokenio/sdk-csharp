using System;
using Xunit;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    /// <summary>
    /// Tests the webhook samples.
    /// </summary>
    public class WebhookSampleTest
    {
        [Fact]
        public void WebhookTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember tpp = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());

                Assert.Throws<AggregateException>(() => WebhookSample.GetWebhookConfig(tpp));

                WebhookSample.SetWebhookConfig(tpp);
                Assert.NotNull(WebhookSample.GetWebhookConfig(tpp));

                WebhookSample.DeleteWebhookConfig(tpp);
                Assert.Throws<AggregateException>(() => WebhookSample.GetWebhookConfig(tpp));
            }
        }
    }
}
