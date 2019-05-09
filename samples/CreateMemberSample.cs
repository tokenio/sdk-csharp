using Tokenio;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Security;
using static Tokenio.Proto.Common.AliasProtos.Alias.Types.Type;

namespace Sample
{
    public class CreateMemberSample
    {
        /// <summary>
        /// Creates and returns a new token member.
        /// </summary>
        /// <returns>a new Member instance</returns>
        public static Member CreateMember()
        {
            var tokenClient = TokenClient.NewBuilder()
                .WithKeyStore(new InMemoryKeyStore())
                .ConnectTo(TokenCluster.SANDBOX)
                .Build();

            var alias = new Alias
            {
                Type = Email,
                Value = Util.Nonce() + "+noverify@example.com"
            };

            var newMember = tokenClient.CreateMember(alias).Result;

            newMember.UseDefaultRecoveryRule().Wait();

            return newMember;
        }
    }
}