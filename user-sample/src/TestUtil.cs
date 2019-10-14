using System;
using System.Threading;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Utils;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User {
    public abstract class TestUtil {
        private static string DEV_KEY = "f3982819-5d8d-4123-9601-886df2780f42";
        private static string TOKEN_REALM = "token";

        /// <summary>
        /// Generates random user name to be used for testing.
        /// </summary>
        /// <returns>The alias.</returns>
        public static Alias RandomAlias() {
            return new Alias {
                Value = "alias-" + Util.Nonce().ToLower() + "+noverify@example.com",
                    Type = Alias.Types.Type.Email,
                    Realm = TOKEN_REALM
            };
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <returns>The client.</returns>
        public static Tokenio.User.TokenClient CreateClient() {
            return Tokenio.User.TokenClient.Create(Tokenio.TokenCluster.DEVELOPMENT, DEV_KEY);
        }

        /// <summary>
        /// Creates the member and link accounts.
        /// </summary>
        /// <returns>The member and link accounts.</returns>
        /// <param name="client">Client.</param>
        public static UserMember CreateMemberAndLinkAccounts(Tokenio.User.TokenClient client) {
            Alias alias = RandomAlias();
            UserMember member = client.CreateMemberBlocking(alias);
            LinkMemberAndBankSample.LinkBankAccounts(member);
            return member;
        }

        /// <summary>
        /// Randoms the numeric.
        /// </summary>
        /// <returns>The numeric.</returns>
        /// <param name="size">Size.</param>
        public static string RandomNumeric(int size) {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, size);
        }

        public static void waitUntil(Action function) {
            WaitUntil(60000, 500, 1, function);
        }

        /// <summary>
        /// Waits the until.
        /// </summary>
        /// <param name="timeoutMs">Timeout ms.</param>
        /// <param name="waitTimeMs">Wait time ms.</param>
        /// <param name="backOffFactor">Back off factor.</param>
        /// <param name="function">Function.</param>
        public static void WaitUntil(
            long timeoutMs,
            long waitTimeMs,
            int backOffFactor,
            Action function) {
            for (long start = CurrentMillis();; waitTimeMs *= backOffFactor) {
                try {
                    Thread newThread = new Thread(new ThreadStart(function));
                    newThread.Start();
                    return;
                } catch (InvalidOperationException ex) {
                    if (CurrentMillis() - start < timeoutMs) {
                        SleepUninterruptibly(waitTimeMs);
                    } else {
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sleepFor">Sleep for.</param>
        private static void SleepUninterruptibly(long sleepFor) {
            bool interrupted = false;
            try {
                long end = CurrentMillis() + sleepFor;
                while (true) {
                    try {
                        Thread.Sleep((int) sleepFor);
                        return;
                    } catch (ThreadInterruptedException) {
                        interrupted = true;
                        sleepFor = end - CurrentMillis();
                    }
                }
            } finally {
                if (interrupted) {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        /// <summary>
        /// Currents the millis.
        /// </summary>
        /// <returns>The millis.</returns>
        private static long CurrentMillis() {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTime = (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
            return currentTime;
        }
    }
}
