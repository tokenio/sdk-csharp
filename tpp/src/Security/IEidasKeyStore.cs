using System;
using Org.BouncyCastle.Math;
using Tokenio.Security;
using Org.BouncyCastle.X509;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Exceptions;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using System.Collections.Generic;

namespace Tokenio.Tpp.Security
{
    public interface IEidasKeyStore: IKeyStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        BigInteger GetCertificateSerialNumber();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        X509Certificate GetCertificate();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        KeyPair GetKey();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        new KeyPair GetById(string memberId, string keyId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memmberId"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        new KeyPair GetByLevel(string memmberId, Level level);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberId"></param>
        new IList<KeyPair> KeyList(string memberId);
    }
}
