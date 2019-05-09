using System;
using System.Collections.Generic;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace Sample
{
    public class MemberRecoverySample
    {
        public Tokenio.Member agentMember; // used by complex recovery rule sample

        public void SetUpDefaultRecoveryRule(Tokenio.Member member)
        {
            member.UseDefaultRecoveryRule().Wait();
        }

        /// <summary>
        /// Recover previously-created member, assuming they were
        /// configured with a "normal consumer" recovery rule.
        /// </summary>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="alias">alias of member to recoverWithDefaultRule</param>
        /// <returns>the recovered member</returns>
        public Tokenio.Member RecoverWithDefaultRule(TokenClient tokenClient, Alias alias)
        {
            var verificationId = tokenClient.BeginRecovery(alias).Result;
            var memberId = tokenClient.GetMemberId(alias).Result;

            var recoveredMember = tokenClient.CompleteRecoveryWithDefaultRule(
                memberId,
                verificationId,
                "1thru6").Result;
            recoveredMember.VerifyAlias(verificationId, "1thru6").Wait();

            return recoveredMember;
        }

        /// <summary>
        /// Illustrate setting up a recovery rule more complex than "normal consumer"
        /// mode, without the "normal consumer" shortcuts.
        /// </summary>
        /// <param name="newMember">newly-created member we are setting up</param>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="agentAlias"> Alias of recovery agent.</param>
        public void SetUpComplexRecoveryRule(
            Tokenio.Member newMember,
            TokenClient tokenClient,
            Alias agentAlias)
        {
            TellRecoveryAgentMemberId(newMember.MemberId());

            var agentId = tokenClient.GetMemberId(agentAlias).Result;
            var recoveryRule = new RecoveryRule {PrimaryAgent = agentId};
            newMember.AddRecoveryRule(recoveryRule).Wait();
        }

        /// <summary>
        /// Illustrate how a recovery agent signs an authorization.
        /// </summary>
        /// <param name="authorization">client's claim to be some member</param>
        /// <returns>if authorization seems legitimate, return signature; else error</returns>
        public Signature GetRecoveryAgentSignature(Authorization authorization)
        {
            var isCorrect = CheckMemberId(authorization.MemberId);
            if (isCorrect)
            {
                return agentMember.AuthorizeRecovery(authorization).Result;
            }

            throw new Exception("I don't authorize this");
        }

        /// <summary>
        /// Illustrate recovery using a not-normal-"consumer mode" recovery agent.
        /// </summary>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="alias">Alias of member to recover</param>
        /// <returns>the recovered member</returns>
        public Tokenio.Member RecoverWithComplexRule(
            TokenClient tokenClient,
            Alias alias)
        {
            var memberId = tokenClient.GetMemberId(alias).Result;

            var cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
            var newKey = cryptoEngine.GenerateKey(Privileged);

            var verificationId = tokenClient.BeginRecovery(alias).Result;
            var authorization = tokenClient.CreateRecoveryAuthorization(memberId, newKey).Result;

            var agentSignature = GetRecoveryAgentSignature(authorization);

            var mro = new MemberRecoveryOperation
            {
                AgentSignature = agentSignature,
                Authorization = authorization
            };
            var recoveredMember = tokenClient.CompleteRecovery(
                memberId,
                new List<MemberRecoveryOperation> {mro}, 
                newKey,
                cryptoEngine).Result;
            
            recoveredMember.VerifyAlias(verificationId, "1thru6").Wait();

            return recoveredMember;
        }

        private static void TellRecoveryAgentMemberId(string memberId)
        {
        } /* this simple sample uses a no op */

        /* this simple sample approves everybody */
        private static bool CheckMemberId(string memberId)
        {
            return true;
        }
    }
}
