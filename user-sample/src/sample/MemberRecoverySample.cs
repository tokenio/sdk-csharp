using System;
using System.Linq;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using UserMember = Tokenio.User.Member;
namespace Tokenio.Sample.User {
    /// <summary>
    /// Illustrate steps of Member recovery.
    /// </summary>
    public class MemberRecoverySample {

        public UserMember agentMember; /* used by complex recovery rule sample */

        public void SetUpDefaultRecoveryRule (UserMember member) {
            member.UseDefaultRecoveryRuleBlocking ();
        }

        /// <summary>
        /// Recover previously-created member, assuming they were
        /// configured with a "normal consumer" recovery rule.
        /// </summary>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="alias">alias of member to recoverWithDefaultRule</param>
        /// <returns>recovered member</returns>
        public UserMember RecoverWithDefaultRule (Tokenio.User.TokenClient tokenClient, Alias alias) {
            string verificationId = tokenClient.BeginRecoveryBlocking (alias);
            // recoverWithDefault begin snippet to include in docs
            string memberId = tokenClient.GetMemberIdBlocking (alias);
            ICryptoEngine cryptoEngine = new TokenCryptoEngine (memberId, new InMemoryKeyStore ());

            // In the real world, we'd prompt the user to enter the code emailed to them.
            // Since our test member uses an auto-verify email address, any string will work,
            // so we use "1thru6".
            UserMember recoveredMember = tokenClient.CompleteRecoveryWithDefaultRuleBlocking (
                memberId,
                verificationId,
                "1thru6",
                cryptoEngine);
            // We can use the same verification code to re-claim this alias.
            recoveredMember.VerifyAliasBlocking (verificationId, "1thru6");
            // recoverWithDefault done snippet to include in docs

            return recoveredMember;
        }

        private void TellRecoveryAgentMemberId (string memberId) { } /* this simple sample uses a no op */

        /// <summary>
        /// Illustrate setting up a recovery rule more complex than "normal consumer"
        /// mode, without the "normal consumer" shortcuts.
        /// </summary>
        /// <param name="newMember">newly-created member we are setting up</param>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="agentAlias">Alias of recovery agent.</param>
        public void SetUpComplexRecoveryRule (
            UserMember newMember,
            Tokenio.User.TokenClient tokenClient,
            Alias agentAlias) {
            // setUpComplex begin snippet to include in docs
            // Someday in the future, this user might ask the recovery agent
            // "Please tell Token that I am the member with ID m:12345678 ."
            // While we're setting up this new member, we need to tell the
            // recovery agent the new member ID so the agent can "remember" later.
            TellRecoveryAgentMemberId (newMember.MemberId ());

            string agentId = tokenClient.GetMemberIdBlocking (agentAlias);

            RecoveryRule recoveryRule = new RecoveryRule { PrimaryAgent = agentId };

            // This example doesn't call .setSecondaryAgents ,
            // but could have. If it had, then recovery would have
            // required one secondary agent authorization along with
            // the primary agent authorization.
            newMember.AddRecoveryRuleBlocking (recoveryRule);
            // setUpComplex done snippet to include in docs
        }

        /* this simple sample approves everybody */
        private bool CheckMemberId (string memberId) {
            return true;
        }

        /// <summary>
        /// Illustrate how a recovery agent signs an authorization.
        /// </summary>
        /// <param name="authorization">client's claim to be some member</param>
        /// <returns>if authorization seems legitimate, return signature; else error</returns>
        public Signature GetRecoveryAgentSignature (MemberRecoveryOperation.Types.Authorization authorization) {
            // authorizeRecovery begin snippet to include in doc
            // "Remember" whether this person who claims to be member with
            // the ID m:12345678 really is:
            bool isCorrect = CheckMemberId (authorization.MemberId);
            if (isCorrect) {
                return agentMember.AuthorizeRecoveryBlocking (authorization);
            }
            throw new ArgumentException ("I don't authorize this");
            // authorizeRecovery done snippet to include in doc
        }

        /// <summary>
        /// Illustrate recovery using a not-normal-"consumer mode" recovery agent.
        /// </summary>
        /// <param name="tokenClient">SDK client</param>
        /// <param name="alias">Alias of member to recover</param>
        /// <returns>recovered member</returns>
        public UserMember RecoverWithComplexRule (
            Tokenio.User.TokenClient tokenClient,
            Alias alias) {
            // complexRecovery begin snippet to include in docs
            string memberId = tokenClient.GetMemberIdBlocking (alias);

            ICryptoEngine cryptoEngine = new TokenCryptoEngine (memberId, new InMemoryKeyStore ());
            Key newKey = cryptoEngine.GenerateKey (Key.Types.Level.Privileged);

            string verificationId = tokenClient.BeginRecoveryBlocking (alias);
            MemberRecoveryOperation.Types.Authorization authorization = tokenClient.CreateRecoveryAuthorizationBlocking (
                memberId,
                newKey);

            // ask recovery agent to verify that I really am this member
            Signature agentSignature = GetRecoveryAgentSignature (authorization);

            // We have all the signed authorizations we need.
            // (In this example, "all" is just one.)
            MemberRecoveryOperation mro = new MemberRecoveryOperation {
                Authorization = authorization,
                AgentSignature = agentSignature
            };
            UserMember recoveredMember = tokenClient.CompleteRecoveryBlocking (
                memberId,
                (new [] { mro }).ToList (),
                newKey,
                cryptoEngine);
            // after recovery, aliases aren't verified

            // In the real world, we'd prompt the user to enter the code emailed to them.
            // Since our test member uses an auto-verify email address, any string will work,
            // so we use "1thru6".
            recoveredMember.VerifyAliasBlocking (verificationId, "1thru6");
            // complexRecovery done snippet to include in docs

            return recoveredMember;
        }
    }
}