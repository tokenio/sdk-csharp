using System.Linq;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Sample.User;
using Xunit;
using static Tokenio.Proto.Common.SecurityProtos.Key.Types;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp
{
    public class GetTokensSampleTest
    {
        [Fact]
        public void GetTokenTest()
        {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateUserMember();
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Token token = CreateTransferTokenSample.CreateTransferToken(payer, payeeAlias, Level.Low);

                Assert.Equal(GetTokensSample.GetToken(payee, token.Id).Id, token.Id);
                var sigList = GetTokensSample.GetToken(payee, token.Id).PayloadSignatures
                    .Where(sig => sig.Action == TokenSignature.Types.Action.Cancelled).ToList();
                Assert.Empty(sigList);
                // cancel token
                payee.CancelTokenBlocking(token);
                sigList = GetTokensSample.GetToken(payee, token.Id).PayloadSignatures
                    .Where(sig => sig.Action == TokenSignature.Types.Action.Cancelled).ToList();
                Assert.NotEmpty(sigList);
            }
        }
    }
}