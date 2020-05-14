using System;
using System.Buffers.Text;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.EidasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using Tokenio.Security.Utils;
using Xunit;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp
{
    public class EidasMethodsSampleTest
    {
        private static readonly String directBankId = "gold";

        [Fact]
        public void VerifyEidasTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                var tppAuthNumber = RandomNumeric(15);
                var keyPair = GenerateKeyPair();
                string certificate = GenerateCert(keyPair, tppAuthNumber);
                TppMember verifiedTppMember = EidasMethodsSample.VerifyEidas(
                    tokenClient,
                    tppAuthNumber,
                    certificate,
                    directBankId,
                    keyPair.ParseRsaKeyPair()
                        .PrivateKey);
                IList<Alias> verifiedAliases = verifiedTppMember.GetAliasesBlocking();
                Assert.Equal(1, verifiedAliases.Count);
                Assert.Equal(tppAuthNumber, verifiedAliases[0].Value);
                Assert.Equal(Alias.Types.Type.Eidas, verifiedAliases[0].Type);
                GetEidasCertificateStatusResponse eidasInfo = verifiedTppMember.GetEidasCertificateStatus()
                    .Result;
                Assert.Equal(certificate, eidasInfo.Certificate);
                Assert.Equal(EidasCertificateStatus.CertificateValid, eidasInfo.Status);
            }
        }

        [Fact]
        public void RecoverEidasTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            using (Tokenio.Tpp.TokenClient anotherTokenClient = TestUtil.CreateClient()) {
                var tppAuthNumber = RandomNumeric(15);
                var keyPair = GenerateKeyPair();
                string certificate = GenerateCert(keyPair, tppAuthNumber);


                // create and verify member first
                TppMember verifiedTppMember = EidasMethodsSample.VerifyEidas(
                    tokenClient,
                    tppAuthNumber,
                    certificate,
                    directBankId,
                    keyPair.ParseRsaKeyPair()
                        .PrivateKey);

                // now pretend we lost the keys and need to recover the member
                TppMember recoveredMember = EidasMethodsSample.RecoverEidas(
                    anotherTokenClient,
                    verifiedTppMember.MemberId(),
                    tppAuthNumber,
                    certificate,
                    keyPair.ParseRsaKeyPair()
                        .PrivateKey);

                IList<Alias> verifiedAliases = recoveredMember.GetAliasesBlocking();

                Assert.Equal(1, verifiedAliases.Count);
                Assert.Equal(tppAuthNumber, verifiedAliases[0].Value);
                Assert.Equal(Alias.Types.Type.Eidas, verifiedAliases[0].Type);
            }
        }

        private static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var generator = GeneratorUtilities.GetKeyPairGenerator("RSA");
            generator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            return generator.GenerateKeyPair();
        }

        private static string GenerateCert(AsymmetricCipherKeyPair keyPair, string tppAuthNumber)
        {
            long now = Utils.Util.CurrentMillis();
            DateTime startDate = DateTime.UtcNow;

            IList<DerObjectIdentifier> oids = new List<DerObjectIdentifier>();
            oids.Add(new DerObjectIdentifier("2.5.4.3"));
            oids.Add(new DerObjectIdentifier("2.5.4.97"));

            IDictionary<DerObjectIdentifier, string> names = new Dictionary<DerObjectIdentifier, string>();
            names.Add(oids[0], "Token.io");
            names.Add(oids[1], tppAuthNumber);
            X509Name dnName = new X509Name((System.Collections.IList) oids, (System.Collections.IDictionary) names);
            BigInteger certSerialNumber = new BigInteger(now.ToString());
            DateTime endDate = startDate.Add(TimeSpan.FromDays(365));
            string signatureAlgorithm = "SHA256WithRSA";
            var generator = new X509V3CertificateGenerator();

            generator.SetSerialNumber(certSerialNumber);
            generator.SetNotBefore(startDate);
            generator.SetNotAfter(endDate);
            generator.SetSubjectDN(dnName);
            generator.SetIssuerDN(dnName);
            generator.SetPublicKey(keyPair.Public);

            ISignatureFactory contentSigner = new Asn1SignatureFactory(signatureAlgorithm, keyPair.Private);

            // Extensions --------------------------
            BasicConstraints basicConstraints = new BasicConstraints(true);
            generator.AddExtension(
                new DerObjectIdentifier("2.5.29.19"),
                true,
                basicConstraints);
            // -------------------------------------
            X509Certificate certificate = generator.Generate(contentSigner);
            return Convert.ToBase64String(certificate.GetEncoded());
        }

        private static string RandomNumeric(int size)
        {
            return Guid.NewGuid()
                .ToString()
                .Replace("-", string.Empty)
                .Substring(0, size);
        }

        [Fact]
        public void RegisterWithEidasTest()
        {
            IKeyStore keyStore = new InMemoryKeyStore();
            var factory = new TokenCryptoEngineFactory(keyStore, Key.Types.Algorithm.Rs256);
            using (var tokenClient = TestUtil.CreateClient(factory))
            {
                var authNumber = RandomNumeric(15);
                var rsaKeyPair = GenerateKeyPair();

                var certificate = GenerateCert(rsaKeyPair, authNumber);
                Member member = EidasMethodsSample.RegisterWithEidas(
                    tokenClient,
                    keyStore,
                    directBankId,
                    rsaKeyPair.ParseRsaKeyPair(),
                    certificate);
                var keys = member.GetKeys()
                    .Result;
                Assert.Equal(keys[0].Level, Key.Types.Level.Privileged);
                Assert.Equal(
                    keys[0].PublicKey,
                    Base64UrlEncoder.Encode(
                        rsaKeyPair.ParseRsaKeyPair()
                            .PublicKey));
                Assert.Equal(
                    member.GetAliases()
                        .Result[0].Value,
                    authNumber);
            }
        }
    }
}
