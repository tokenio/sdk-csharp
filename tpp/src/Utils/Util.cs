using System;
using System.Linq;
using System.Collections.Generic;
using Google.Protobuf;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using Tokenio.Security.Crypto;
using System.Collections.Concurrent;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;
using System.Threading;
using System.Threading.Tasks;

namespace Tokenio.Tpp.Utils
{
    /// <summary>
    /// Utility Methods
    /// </summary>
    public class Util : Tokenio.Utils.Util
    {
        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <returns>The query string.</returns>
        /// <param name="url">URL.</param>
        public static string GetQueryString(string url)
        {
            if (url == null)
            {
                throw new ArgumentException("URL cannot be null");
            }
            var splitted = url.Split(new[] { '?' }, 2);
            return splitted.Length == 1 ? splitted[0] : splitted[1];
        }

        /// <summary>
        /// Verify the signature of the payload.
        /// </summary>
        /// <param name="member">Member.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="signature">Signature.</param>
        public static void VerifySignature(
            ProtoMember member,
            IMessage payload,
            Signature signature)
        {
            Key key;
            try
            {
                key = member.Keys.Single(k => k.Id.Equals(signature.KeyId));
            }
            catch (InvalidOperationException)
            {
                throw new CryptoKeyNotFoundException(signature.KeyId);
            }

            var verifier = new Ed25519Veifier(key.PublicKey);
            verifier.Verify(payload, signature.Signature_);
        }

        
            public static T RetryWithExponentialBackoff<T>(
                       long timeoutMs,
                       long waitTimeMs,
                       double backOffFactor,
                       long maxWaitTimeMs,
                       Func<T> function,
                       Predicate<T> retryIf)
            {
                if(timeoutMs < 0 || waitTimeMs < 0 || backOffFactor < 0 || maxWaitTimeMs < 0)
                {
                    throw new ArgumentException("All time arguments and the backOffFactor should be non-negative.");
                }
                long totalTime = 0;
                T result = function.Invoke();
                while(retryIf(result))
                {
                    if(totalTime >= timeoutMs) {
                        return result;
                    }
                    Console.WriteLine("Retry");
                    Console.WriteLine(totalTime + "" + result);
                    Thread.Sleep((int) waitTimeMs);
                    result = function.Invoke();
                    totalTime = totalTime + waitTimeMs;
                    waitTimeMs = Math.Min((long)(waitTimeMs * backOffFactor), maxWaitTimeMs);
                }
                Console.WriteLine("Last Retry");
                Console.WriteLine(totalTime + "" + result);
                return result;
            }

        public static T RetryWithExponentialBackoffNoThrow<T>(
            long timeOutMs,
            long waitTimeMs,
            double backOffFactor,
            long maxWaitTimeMs,
            Func<T> function,
            Predicate<T> retryIf)
        {
            try
            {
                return RetryWithExponentialBackoff(timeOutMs,
                    waitTimeMs,
                    backOffFactor,
                    maxWaitTimeMs,
                    function,
                    retryIf);
            }
            catch (ThreadInterruptedException e) {
                throw e;
            }
            catch(Exception e) {
                throw new Exception(e.ToString());
            }
        }
        }
}
