using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.EidasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using Tokenio.Security.Crypto;
using Tokenio.Tpp.Exceptions;
using Tokenio.Tpp.Security;
using Tokenio.Utils;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TppMember = Tokenio.Tpp.Member;


namespace Tokenio.Sample.Tpp
{
    public class EidasMethodsSample
    {
        /// <summary>
        /// Creates a TPP member and verifies it using eIDAS certificate.
        /// 
        /// </summary>
        /// <param name="client">token client</param>
        /// <param name="tppAuthNumber">authNumber of the TPP</param>
        /// <param name="certificate">base64 encoded eIDAS certificate (a single line, no header and footer)</param>
        /// <param name="bankId">id of the bank the TPP trying to get access to</param>
        /// <param name="privateKey">private key corresponding to the public key in the certificate</param>
        /// <returns>verified business member</returns>
        public static TppMember VerifyEidas(Tokenio.Tpp.TokenClient client,
            string tppAuthNumber,
            string certificate,
            string bankId,
            byte[] privateKey)
        {
            Algorithm signingAlgorithm = Algorithm.Rs256;
            ISigner signer = new Rs256Signer("eidas", privateKey);

            // resolve memberId of the bank TPP is trying to get access to
            string bankMemberId = client
                .ResolveAliasBlocking(new Alias { Value = bankId, Type = Alias.Types.Type.Bank })
                .Id;
            // create an eIDAS alias under realm of the target bank
            Alias eidasAlias = new Alias
            {
                Value = tppAuthNumber.Trim(),
                RealmId = bankMemberId,
                Type = Alias.Types.Type.Eidas
            };
            // create a member under realm of the bank with eIDAS alias
            Tokenio.Tpp.Member tpp = client.CreateMember(eidasAlias, null, bankMemberId).Result;
            // construct a payload with all the required data
            VerifyEidasPayload payload = new VerifyEidasPayload
            {
                Algorithm = signingAlgorithm,
                Alias = eidasAlias,
                Certificate = certificate,
                MemberId = tpp.MemberId()
            };

            // verify eIDAS
            VerifyEidasResponse response = tpp
                .VerifyEidas(payload, signer.Sign(payload))
                .Result;
            return tpp;
        }

        /// <summary>
        /// Recovers a TPP member and verifies its EIDAS alias using eIDAS certificate.
        /// 
        /// </summary>
        /// <param name="client">token client</param>
        /// <param name="memberId">id of the member to be recovered</param>
        /// <param name="tppAuthNumber">authNumber of the TPP</param>
        /// <param name="certificate">base64 encoded eIDAS certificate (a single line, no header and footer)</param>
        /// <param name="certificatePrivateKey">private key corresponding to the public key in the certificate</param>
        /// <returns>verified business member</returns>
        public static TppMember RecoverEidas(Tokenio.Tpp.TokenClient client,
            string memberId,
            string tppAuthNumber,
            string certificate,
            byte[] certificatePrivateKey)
        {
            // create a signer using the certificate private key
            Algorithm signingAlgorithm = Algorithm.Rs256;
            ISigner payloadSigner = new Rs256Signer("eidas", certificatePrivateKey);

            // generate a new privileged key to add to the member
            ICryptoEngine cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
            Key newKey = cryptoEngine.GenerateKey(Level.Privileged);

            // construct a payload with all the required data
            EidasRecoveryPayload payload = new EidasRecoveryPayload
            {
                MemberId = memberId,
                Certificate = certificate,
                Algorithm = signingAlgorithm,
                Key = newKey
            };

            Tokenio.Tpp.Member recoveredMember = client
                    .RecoverEidasMember(payload, payloadSigner.Sign(payload), cryptoEngine)
                    .Result;

            // the eidas alias becomes unverified after the recovery, so we need to verify it again
            Alias eidasAlias = new Alias
            {
                Value = tppAuthNumber.Trim(),
                RealmId = recoveredMember.RealmId(),
                Type = Alias.Types.Type.Eidas
            };

            VerifyEidasPayload verifyPayload = new VerifyEidasPayload
            {
                MemberId = memberId,
                Alias = eidasAlias,
                Certificate = certificate,
                Algorithm = signingAlgorithm
            };

            VerifyEidasResponse response = recoveredMember
                    .VerifyEidas(verifyPayload, payloadSigner.Sign(verifyPayload))
                    .Result;

            return recoveredMember;
        }

        public static TppMember RegisterWithEidas(Tokenio.Tpp.TokenClient tokenClient,
            IKeyStore keyStore,
            string bankId,
            KeyPair keyPair,
            string certificate)
        {
            //create a signer using the certificate private key
            var signingAlgorithm = Algorithm.Rs256;
            ISigner payloadSigner = new Rs256Signer(Util.Nonce(),
                keyPair.PrivateKey);

            var payload = new RegisterWithEidasPayload
            {
                Certificate = certificate,
                BankId = bankId
            };

            var resp = tokenClient.RegisterWithEidas(payload,
                payloadSigner.Sign(payload))
                .Result;
            var memberId = resp.MemberId;
            // now we can load a member and also check a status of the certificate verification
            var member = tokenClient.GetMemberBlocking(memberId);
            var statusResp = member.GetEidasVerificationStatus(resp.VerificationId)
                .Result;
            return member;
        }

        /// <summary>
        /// Creates a TPP member under realm of a bank and registers it with the provided eIDAS
        /// certificate.The created member has a registered PRIVILEGED-level RSA key from the provided
        /// certificate and an EIDAS alias with value equal to authNumber from the certificate.
        ///
        /// </summary>
        /// <param name="tokenClient">token client</param>
        /// <param name="keyStore">a key store that is used by token client and contains eIDAS certificate and
        ///      a private key</param>
        /// <param name="bankId">id of the bank the TPP trying to get access to</param>
        /// <returns>a newly created and oboarded member</returns>
        public static Member CreateMemberWithEidas(Tokenio.Tpp.TokenClient tokenClient,
            IEidasKeyStore keyStore, string bankId)
        {
            Member member = null;
            try
            {
                member = tokenClient.CreateMemberWithEidas(bankId,
                    keyStore, 30000);
            }
            catch (EidasTimeoutException ex)
            {
                System.Console.WriteLine($"Unable to complete eIDAS verification: memberId={ex.GetMemberId()} | verivicationId={ex.GetVerificationId()}");
            }
            return member;
        }
    }
}
