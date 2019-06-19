using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Google.Protobuf;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Exceptions;
using Tokenio.Security;
using ProtoMember = Tokenio.Proto.Common.MemberProtos.Member;

namespace Tokenio.Tpp.Utils
{
    public class Util : Tokenio.Utils.Util
    {
        public static string GetQueryString(string url)
        {
            if (url == null)
            {
                throw new ArgumentException("URL cannot be null");
            }
            var splitted = url.Split(new[] { '?' }, 2);
            return splitted.Length == 1 ? splitted[0] : splitted[1];
        }

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
    }
}
