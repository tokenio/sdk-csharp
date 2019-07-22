using System;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.MemberProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.MemberProtos.MemberRecoveryOperation.Types;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;
using System.Linq;
namespace TokenioSample
{
    public class MemberRecoverySample
    {


        public TppMember agentMember; /* used by complex recovery rule sample */

        /// <summary>
        /// Sets up default recovery rule.
        /// </summary>
        /// <param name="member">Member.</param>
        public void SetUpDefaultRecoveryRule(TppMember member)
        {
            member.UseDefaultRecoveryRuleBlocking();
        }

       /// <summary>
       /// Recovers the with default rule.
       /// </summary>
       /// <returns>The with default rule.</returns>
       /// <param name="tokenClient">Token client.</param>
       /// <param name="alias">Alias.</param>
        public TppMember RecoverWithDefaultRule(TokenClient tokenClient, Alias alias)
        {
            string verificationId = tokenClient.BeginRecoveryBlocking(alias);
            // recoverWithDefault begin snippet to include in docs
            string memberId = tokenClient.GetMemberIdBlocking(alias);
            ICryptoEngine cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());

            // In the real world, we'd prompt the user to enter the code emailed to them.
            // Since our test member uses an auto-verify email address, any string will work,
            // so we use "1thru6".
            TppMember recoveredMember = tokenClient. CompleteRecoveryWithDefaultRuleBlocking(
                    memberId,
                    verificationId,
                    "1thru6",
                    cryptoEngine);
            // We can use the same verification code to re-claim this alias.
            recoveredMember.VerifyAliasBlocking(verificationId, "1thru6");
            // recoverWithDefault done snippet to include in docs

            return recoveredMember;
        }

        private void TellRecoveryAgentMemberId(string memberId) { } /* this simple sample uses a no op */

       /// <summary>
       /// Sets up complex recovery rule.
       /// </summary>
       /// <param name="newMember">New member.</param>
       /// <param name="tokenClient">Token client.</param>
       /// <param name="agentAlias">Agent alias.</param>
        public void SetUpComplexRecoveryRule(
                TppMember newMember,
                TokenClient tokenClient,
                Alias agentAlias)
        {
            // setUpComplex begin snippet to include in docs
            // Someday in the future, this user might ask the recovery agent
            // "Please tell Token that I am the member with ID m:12345678 ."
            // While we're setting up this new member, we need to tell the
            // recovery agent the new member ID so the agent can "remember" later.
            TellRecoveryAgentMemberId(newMember.MemberId());

            string agentId = tokenClient.GetMemberIdBlocking(agentAlias);

            RecoveryRule recoveryRule = new RecoveryRule() { PrimaryAgent = agentId };

            // This example doesn't call .setSecondaryAgents ,
            // but could have. If it had, then recovery would have
            // required one secondary agent authorization along with
            // the primary agent authorization.
            newMember.AddRecoveryRuleBlocking(recoveryRule);
            // setUpComplex done snippet to include in docs
        }

        /* this simple sample approves everybody */
        private bool CheckMemberId(string memberId)
        {
            return true;
        }

       /// <summary>
       /// Gets the recovery agent signature.
       /// </summary>
       /// <returns>The recovery agent signature.</returns>
       /// <param name="authorization">Authorization.</param>
        public Signature getRecoveryAgentSignature(Authorization authorization)
        {
            // authorizeRecovery begin snippet to include in doc
            // "Remember" whether this person who claims to be member with
            // the ID m:12345678 really is:
            bool isCorrect = CheckMemberId(authorization.MemberId);
            if (isCorrect)
            {
                return agentMember.AuthorizeRecoveryBlocking(authorization);
            }
            throw new ArgumentException("I don't authorize this");
            // authorizeRecovery done snippet to include in doc
        }

        /// <summary>
        /// Recovers the with complex rule.
        /// </summary>
        /// <returns>The with complex rule.</returns>
        /// <param name="tokenClient">Token client.</param>
        /// <param name="alias">Alias.</param>
        public TppMember RecoverWithComplexRule(
                TokenClient tokenClient,
                Alias alias)
        {
            // complexRecovery begin snippet to include in docs
            string memberId = tokenClient.GetMemberIdBlocking(alias);

            ICryptoEngine cryptoEngine = new TokenCryptoEngine(memberId, new InMemoryKeyStore());
            Key newKey = cryptoEngine.GenerateKey(Key.Types.Level.Privileged);

            string verificationId = tokenClient.BeginRecoveryBlocking(alias);
            Authorization authorization = tokenClient.CreateRecoveryAuthorizationBlocking(
                    memberId,
                    newKey);

            // ask recovery agent to verify that I really am this member
            Signature agentSignature = getRecoveryAgentSignature(authorization);

            // We have all the signed authorizations we need.
            // (In this example, "all" is just one.)
            MemberRecoveryOperation mro = new MemberRecoveryOperation()
            {
                Authorization = authorization,
                AgentSignature = agentSignature
            };
            TppMember recoveredMember = tokenClient.CompleteRecoveryBlocking(
                    memberId,
                    (new[] { mro }).ToList(),
                    newKey,
                    cryptoEngine);
            // after recovery, aliases aren't verified

            // In the real world, we'd prompt the user to enter the code emailed to them.
            // Since our test member uses an auto-verify email address, any string will work,
            // so we use "1thru6".
            recoveredMember.VerifyAliasBlocking(verificationId, "1thru6");
            // complexRecovery done snippet to include in docs

            return recoveredMember;
        }
    }
}
