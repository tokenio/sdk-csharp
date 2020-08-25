using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using Tokenio.Security.Keystore;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Tokenio.Tpp.Security
{
    public class InMemoryEidasKeyStore : IEidasKeyStore
    {
        private readonly KeyPair key;
        private readonly X509Certificate eidasCertificate;
        public InMemoryEidasKeyStore(X509Certificate eidasCertificate, AsymmetricCipherKeyPair keyPair)
        {
            this.eidasCertificate = eidasCertificate;
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(eidasCertificate.GetPublicKey());
            var publicKey = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            var privateKey = privateKeyInfo.ToAsn1Object().GetDerEncoded();
            this.key = new KeyPair(eidasCertificate.SerialNumber.ToString(),
                Level.Privileged, Algorithm.Rs256, privateKey, publicKey);
        }

        public InMemoryEidasKeyStore(string eidasCertificate, AsymmetricCipherKeyPair keyPair)
            : this(ExtractCertificate(eidasCertificate), keyPair)
        {

        }

        public static X509Certificate ExtractCertificate(string certString)
        {
            byte[] bytes = Convert.FromBase64String(certString);
            var cert = new X509Certificate2(bytes);
            X509CertificateParser parser = new X509CertificateParser();
            var obj1 = parser.ReadCertificate(cert.GetRawCertData());
            return obj1;
        }

        public KeyPair GetKey()
        {
            return key;
        }

        public KeyPair GetByLevel(string memberId, Key.Types.Level level)
        {
            if(key.Level == level)
            {
                return key;
            }
            throw new CryptoKeyNotFoundException(level);
        }

        public X509Certificate GetCertificate()
        {
            return eidasCertificate;
        }

        public BigInteger GetCertificateSerialNumber()
        {
            return eidasCertificate.SerialNumber;
        }

        public IList<KeyPair> KeyList(string memberId)
        {
            return (IList<KeyPair>)GetKey();
        }

        public void Put(string memberId, KeyPair keyPair)
        {
            throw new NotImplementedException("This key store does not accept new keys - "
                + "it stores the only key provided at the moment of the store creation");
        }

        public KeyPair GetById(string memberId, string keyId)
        {
            if (GetKey().Id != keyId)
            {
                throw new CryptoKeyNotFoundException("Key not found for id: " + keyId);
            }
            return GetKey();
        }
    }
}
