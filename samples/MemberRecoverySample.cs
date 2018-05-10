using System;
using System.Collections.Generic;
using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types.Level;

namespace samples
{
    public class MemberRecoverySample
    {
        public MemberSync agentMember; // used by complex recovery rule sample

        public void SetUpDefaultRecoveryRule(MemberSync member)
        {
            member.UseDefaultRecoveryRule();
        }

        /// <summary>
        /// Recover previously-created member, assuming they were
        /// configured with a "normal consumer" recovery rule.
        /// </summary>
        /// <param name="tokenIO">SDK client</param>
        /// <param name="alias">alias of member to recoverWithDefaultRule</param>
        /// <returns>the recovered member</returns>
        public MemberSync RecoverWithDefaultRule(TokenIO tokenIO, Alias alias)
        {
            var verificationId = tokenIO.BeginRecovery(alias);
            // recoverWithDefault begin snippet to include in docs
            var memberId = tokenIO.GetMemberId(alias);

            // In the real world, we'd prompt the user to enter the code emailed to them.
            // Since our test member uses an auto-verify email address, any string will work,
            // so we use "1thru6".
            var recoveredMember = tokenIO.CompleteRecoveryWithDefaultRule(
                memberId,
                verificationId,
                "1thru6");
            // We can use the same verification code to re-claim this alias.
            recoveredMember.VerifyAlias(verificationId, "1thru6");
            // recoverWithDefault done snippet to include in docs

            return recoveredMember;
        }

        /// <summary>
        /// Illustrate setting up a recovery rule more complex than "normal consumer"
        /// mode, without the "normal consumer" shortcuts.
        /// </summary>
        /// <param name="newMember">newly-created member we are setting up</param>
        /// <param name="tokenIO">SDK client</param>
        /// <param name="agentAlias"> Alias of recovery agent.</param>
        public void SetUpComplexRecoveryRule(
            MemberSync newMember,
            TokenIO tokenIO,
            Alias agentAlias)
        {
            // setUpComplex begin snippet to include in docs
            // Someday in the future, this user might ask the recovery agent
            // "Please tell Token that I am the member with ID m:12345678 ."
            // While we're setting up this new member, we need to tell the
            // recovery agent the new member ID so the agent can "remember" later.
            TellRecoveryAgentMemberId(newMember.MemberId());

            var agentId = tokenIO.GetMemberId(agentAlias);
            var recoveryRule = new RecoveryRule {PrimaryAgent = agentId};
            // This example doesn't call .setSecondaryAgents ,
            // but could have. If it had, then recovery would have
            // required one secondary agent authorization along with
            // the primary agent authorization.

            newMember.AddRecoveryRule(recoveryRule);
            // setUpComplex done snippet to include in docs
        }

        /// <summary>
        /// Illustrate how a recovery agent signs an authorization.
        /// </summary>
        /// <param name="authorization">client's claim to be some member</param>
        /// <returns>if authorization seems legitimate, return signature; else error</returns>
        public Signature GetRecoveryAgentSignature(Authorization authorization)
        {
            // authorizeRecovery begin snippet to include in doc
            // "Remember" whether this person who claims to be member with
            // the ID m:12345678 really is:
            var isCorrect = checkMemberId(authorization.MemberId);
            if (isCorrect)
            {
                return agentMember.AuthorizeRecovery(authorization);
            }

            throw new Exception("I don't authorize this");
            // authorizeRecovery done snippet to include in doc
        }

        /// <summary>
        /// Illustrate recovery using a not-normal-"consumer mode" recovery agent.
        /// </summary>
        /// <param name="tokenIO">SDK client</param>
        /// <param name="alias">Alias of member to recover</param>
        /// <returns>the recovered member</returns>
        public MemberSync RecoverWithComplexRule(
            TokenIO tokenIO,
            Alias alias)
        {
            // complexRecovery begin snippet to include in docs
            var memberId = tokenIO.GetMemberId(alias);

            var cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
            var newKey = cryptoEngine.GenerateKey(Privileged);

            var verificationId = tokenIO.BeginRecovery(alias);
            var authorization = tokenIO.CreateRecoveryAuthorization(memberId, newKey);

            // ask recovery agent to verify that I really am this member
            var agentSignature = GetRecoveryAgentSignature(authorization);

            // We have all the signed authorizations we need.
            // (In this example, "all" is just one.)
            var mro = new MemberRecoveryOperation
            {
                AgentSignature = agentSignature,
                Authorization = authorization
            };
            var recoveredMember = tokenIO.CompleteRecovery(
                memberId,
                new List<MemberRecoveryOperation> {mro}, 
                newKey,
                cryptoEngine);
            // after recovery, aliases aren't verified

            // In the real world, we'd prompt the user to enter the code emailed to them.
            // Since our test member uses an auto-verify email address, any string will work,
            // so we use "1thru6".
            recoveredMember.VerifyAlias(verificationId, "1thru6");
            // complexRecovery done snippet to include in docs

            return recoveredMember;
        }

        private void TellRecoveryAgentMemberId(string memberId)
        {
        } /* this simple sample uses a no op */

        /* this simple sample approves everybody */
        private bool checkMemberId(string memberId)
        {
            return true;
        }
    }
}