using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;

namespace samples
{
    public class CreateMemberSample
    {
        /// <summary>
        /// Creates and returns a new token member.
        /// </summary>
        /// <returns>a new Member instance</returns>
        public static MemberSync CreateMember()
        {
            // Create the client, which communicates with
            // the Token cloud.
            var tokenIO = TokenIO.NewBuilder()
                .WithKeyStore(new InMemoryKeyStore())
                .ConnectTo(TokenCluster.SANDBOX)
                .Build();

            // An alias is a "human-readable" reference to a member.
            // Here, we use a random email. This works in test environments.
            // but in production, TokenOS would try to verify we own the address,
            // so a random address wouldn't be useful for much.
            // We use a random address because otherwise, if we ran a second
            // time, Token would say the alias was already taken.
            var alias = new Alias
            {
                Type = Email,
                Value = Util.Nonce() + "+noverify@example.com"
            };

            var newMember = tokenIO.CreateMember(alias);

            // let user recover member by verifying email if they lose keys
            newMember.UseDefaultRecoveryRule();

            return newMember;
        }
    }
}