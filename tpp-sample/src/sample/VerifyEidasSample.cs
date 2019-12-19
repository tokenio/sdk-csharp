using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.EidasProtos;
using Tokenio.Proto.Gateway;
using Tokenio.Security;
using Tokenio.Security.Crypto;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;

namespace Tokenio.Sample.Tpp
{
    public class VerifyEidasSample
    {
        /// <summary>
        /// Creates a TPP member and verifies it using eIDAS certificate.
        /// </summary>
        /// <param name="client">token client</param>
        /// <param name="tppAuthNumber">authNumber of the TPP</param>
        /// <param name="certificate">base64 encoded eIDAS certificate</param>
        /// <param name="bankId">id of the bank the TPP trying to get access to</param>
        /// <param name="privateKey">private key corresponding to the public key in the certificate</param>
        /// <returns>verified business member</returns>
        public static Member VerifyEidas(
            Tokenio.Tpp.TokenClient client,
            string tppAuthNumber,
            string certificate,
            string bankId,
            byte[] privateKey)
        {
            Algorithm signingAlgorithm = Algorithm.Rs256;
            ISigner signer = new Rs256Signer("eidas", privateKey);

            // resolve memberId of the bank TPP is trying to get access to
            string bankMemberId = client
                .ResolveAliasBlocking(new Alias {Value = bankId, Type = Alias.Types.Type.Bank})
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
    }
}