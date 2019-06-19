using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.NotificationProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.SubscriberProtos;
using Tokenio.Proto.Common.TokenProtos;
using TokenioTest.Common;
using Xunit;
using Sample = TokenioTest.Testing.Sample.Sample;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;


namespace TokenioTest
{
    public abstract class NotificationsTestBase : IDisposable
    {
        public TokenUserRule rule = new TokenUserRule();
        public TokenTppRule tppRule = new TokenTppRule();

        internal TppMember payee;
        internal LinkedAccount payerAccount;
        internal UserMember payer;


        protected NotificationsTestBase()
        {
            this.payee = tppRule.Member();
            this.payerAccount = rule.LinkedAccount();
            this.payer = payerAccount.GetMember();
        }

        public void Dispose()
        {
            // Do "global" teardown here; Called after every test method.
        }
    }



    public class NotificationsTest : NotificationsTestBase
    {


        private static readonly int NOTIFICATION_TIMEOUT_MS = 15000;
        private static readonly string NOTIFICATION_HANDLER = "token";
        private static readonly string ANDROID = "ANDROID";
        private static readonly string FIREBASE_IOS = "FIREBASE_IOS";
        private static readonly string CREATE_AND_ENDORSE_TOKEN_INVALIDATED =
            "CREATE_AND_ENDORSE_TOKEN_INVALIDATED";


        //[Fact]
        //public void TestSubscribers()
        //{
        //    string target = Sample.RandomNumeric(15);
        //    Subscriber subscriber = payer.SubscribeToNotificationsBlocking(
        //        NOTIFICATION_HANDLER,
        //        Sample.HandlerInstructions(target, FIREBASE_IOS));

        //    Alias alias = payer.GetFirstAliasBlocking();
        //    DeviceInfo deviceInfo = rule.Token().ProvisionDeviceBlocking(alias);

        //    payer.SubscribeToNotificationsBlocking(
        //            NOTIFICATION_HANDLER,
        //            Sample.HandlerInstructions(target, FIREBASE_IOS));

        //    IList<Key> keys = new List<Key>
        //    {
        //        deviceInfo.Keys[0]
        //    };

        //    DeviceMetadata metadata = new DeviceMetadata
        //    {
        //        Application = "Chrome",
        //        ApplicationVersion = "52.0"
        //    };
        //    NotifyStatus res = rule.Token().NotifyAddKeyBlocking(
        //            alias,
        //            keys,
        //            metadata);
        //    Assert.Equal(res, NotifyStatus.Accepted);
        //    IList<Subscriber> subscriberList = payer.GetSubscribersBlocking();
        //    Assert.Equal(subscriberList.Count, 1);
        //    Assert.Equal(payer.GetNotificationsBlocking(null, 100).List.Count, 10);

        //    Polling.WaitUntil(
        //            NOTIFICATION_TIMEOUT_MS,
        //            () => {
        //        IList<IMessage> messages = rule.mockServiceClient()
        //                .getFirebaseMessages(target);
        //        assertThat(messages.size()).isEqualTo(1);
        //        assertThat(messages.get(0).getNotification().getClickAction())
        //                .isEqualTo(ADD_KEY.toString());
        //    });

        //    payer.unsubscribeFromNotificationsBlocking(subscriber.getId());
        //    List<Subscriber> subscriberList2 = payer.getSubscribersBlocking();
        //    assertThat(subscriberList2.size()).isEqualTo(0);
        //}

        [Fact]
        public void TestHandlerSubscriber()
        {
            payer.SubscribeToNotificationsBlocking("iron", new MapField<string, string>());
            IList<Subscriber> subscriberList = payer.GetSubscribersBlocking();

            Alias alias = payer.GetFirstAliasBlocking();
            DeviceInfo deviceInfo = rule.Token().ProvisionDeviceBlocking(alias);

            IList<Key> keys = new List<Key>
            {
                deviceInfo.Keys[0]
            };

            DeviceMetadata metadata = new DeviceMetadata
            {
                Application = "Chrome",
                ApplicationVersion = "52.0"
            };

            rule.Token().NotifyAddKeyBlocking(
                    alias,
                    keys,
                    metadata);

            Assert.Equal( 1, subscriberList.Count);
            Assert.True(payer.GetNotificationsBlocking(100,null).List.Count > 0);
        }

        [Fact]
        public void TestHandlerSubscriberInstructions()
        {
            MapField<string, string> instructionsBank = new MapField<string, string>();
            instructionsBank.Add("sampleInstruction", "value");
            payer.SubscribeToNotificationsBlocking("iron", instructionsBank);
            IList<Subscriber> subscriberList = payer.GetSubscribersBlocking();

            Alias alias = payer.GetFirstAliasBlocking();
            DeviceInfo deviceInfo = rule.Token().ProvisionDeviceBlocking(alias);

            IList<Key> keys = new List<Key>
            {
                deviceInfo.Keys[0]
            };

            DeviceMetadata metadata = new DeviceMetadata
            {
                Application = "Chrome",
                ApplicationVersion = "52.0"
            };

            rule.Token().NotifyAddKeyBlocking(
                    alias,
                    keys,
                    metadata);

            Assert.Equal( 1, subscriberList.Count);
            Assert.True(payer.GetNotificationsBlocking(100, null).List.Count > 0);
        }

        [Fact]
        public void GetSubscriber()
        {
            Subscriber subscriber = payer.SubscribeToNotificationsBlocking(
                    NOTIFICATION_HANDLER,
                    Sample.HandlerInstructions(Sample.RandomNumeric(15), ANDROID));

            Subscriber subscriber2 = payer.GetSubscriberBlocking(subscriber.Id);
            Assert.Equal( subscriber2, subscriber);
        }

        //[Fact]
        //public void TriggerBalanceStepUpNotification()
        //{
        //    string target = Sample.RandomNumeric(15);
        //    payer.SubscribeToNotificationsBlocking(
        //            NOTIFICATION_HANDLER,
        //            Sample.HandlerInstructions(target, FIREBASE_IOS));
        //    payee.TriggerBalanceStepUpNotificationBlocking(ImmutableList.Create<string>(payerAccount.GetId()));

        //    Polling.WaitUntil(
        //            NOTIFICATION_TIMEOUT_MS,
        //            () =>
        //            { Assert.True(payer.GetNotificationsBlocking(100, null).List.Count == 1);

        //                    .extracting(new NotificationStatusExtractor())
        //                    .containsExactly(DELIVERED)
        //                    });

        //    waitUntil(
        //            NOTIFICATION_TIMEOUT_MS,
        //            ()-> {
        //        List<Message> messages = rule.mockServiceClient().getFirebaseMessages(target);
        //        assertThat(messages.size()).isEqualTo(1);
        //        assertThat(messages.get(0).getNotification().getClickAction())
        //                .isEqualTo(BALANCE_STEP_UP.toString());
        //    });
        //}

        [Fact]
        public void GetNotificationsEmpty()
        {
            Assert.True(payer.GetNotificationsBlocking(100, null).List.Count == 0);
        }

        [Fact]
        public void getNotificationFalse()
        {
            Assert.ThrowsAny<Exception>(() => payer.GetNotificationBlocking("123456789"));
        }

        [Fact]
        public void GetNotificationsPaging_stable()
        {
            string payerTarget = Sample.RandomNumeric(15);
            payer.SubscribeToNotificationsBlocking(
                    NOTIFICATION_HANDLER,
                    Sample.HandlerInstructions(payerTarget, FIREBASE_IOS));

            Alias alias = payer.GetFirstAliasBlocking();
            DeviceInfo deviceInfo = rule.Token().ProvisionDeviceBlocking(alias);

            for (int i = 0; i < 4; i++)
            {

                IList<Key> keys = new List<Key>
                {
                    deviceInfo.Keys[0]
                };

                DeviceMetadata metadata = new DeviceMetadata
                {
                    Application = "Chrome",
                    ApplicationVersion = "52.0"
                };

                rule.Token().NotifyAddKeyBlocking(
                        alias,
                        keys,
                        metadata);
            }

            PagedList<Notification> notifications = payer.GetNotificationsBlocking(2,null);
            Assert.Equal( 2, notifications.List.Count);

            var payload = new TokenPayload
            {
                Description = "Payment request",
                From = new TokenMember { Alias = payer.GetFirstAliasBlocking() },
                To = new TokenMember { Alias = payee.GetFirstAliasBlocking() },
                Transfer = new TransferBody { Amount = "100", Currency = "USD" }
            };

            rule.Token().NotifyPaymentRequestBlocking(payload);

            PagedList<Notification> notifications2 = payer.GetNotificationsBlocking(100, notifications.Offset);

            // the new notification should not affect what's past the offset
            Assert.Equal( 2, notifications2.List.Count);
        }
    }
}
