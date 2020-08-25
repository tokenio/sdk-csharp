using System;
using Xunit;
using static Tokenio.Tpp.Utils.Util;

namespace Tokenio.Sample.Tpp
{
    public class UtilTest
    {
        [Fact]
        public void RetryWithExponentialBackoffTest()
        {
            long startTime = CurrentMillis();
            Console.WriteLine("Inside Exponential back off 1");
            Assert.True(RetryWithExponentialBackoff(
                300,
                10,
                2,
                50,
                () => ReturnTrueAfter(startTime + 200),
                b => !b));
        }

        [Fact]
        public void RetryWithExponentialBackoff_timeoutTest()
        {
            long startTime = CurrentMillis();
            Console.WriteLine("Inside Exponential back off 2");
            Assert.False(RetryWithExponentialBackoff(
                100,
                10,
                2,
                50,
                () => ReturnTrueAfter(startTime + 1000),
                b => !b));
        }

        /// <summary>
        /// Currents the millis.
        /// </summary>
        /// <returns>The millis.</returns>
        private static long CurrentMillis()
        {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTime = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
            return currentTime;
        }

        private bool ReturnTrueAfter(long timeMs)
        {
            long curTime = CurrentMillis();
            if(curTime < timeMs)
            {
                return false;
            }
            return true;
        }
    }
}
