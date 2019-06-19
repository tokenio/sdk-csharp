using System;
using System.Threading;

namespace TokenioTest.Common
{
    public class Polling
    {
        public static void WaitUntil(long timeoutMs, Action function)
        {
            WaitUntil(timeoutMs, 1, 2, function);
        }

        public static void WaitUntil(long timeoutMs, long waitTimeMs, Action function)
        {
            WaitUntil(timeoutMs, waitTimeMs, 1, function);
        }

        public static void WaitUntil(
            long timeoutMs,
            long waitTimeMs,
            int backOffFactor,
            Action function)
        {

            for ( long start = CurrentMillis();  ; waitTimeMs *= backOffFactor)
            {
                try
                {
                    function.Invoke();
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    if (CurrentMillis() - start < timeoutMs)
                    {
                        SleepUninterruptibly(waitTimeMs);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
        }

        private static void SleepUninterruptibly(long sleepFor)
        {
            bool interrupted = false;
            try
            {
                long end =  CurrentMillis() + sleepFor;
                while (true)
                {
                    try
                    {
                        Thread.Sleep((int)sleepFor);
                        return;
                    }
                    catch (ThreadInterruptedException e)
                    {
                        interrupted = true;
                        sleepFor = end - CurrentMillis();
                    }
                }
            }
            finally
            {
                if (interrupted)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        private static long CurrentMillis()
        {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTime = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
            return currentTime;
        }
    }
}
