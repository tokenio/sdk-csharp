using System.Linq;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;
namespace TokenioSample
{
    public class GetTokensSampleTest
    {

        [Fact]
        public void GetTokenTest()
        {
            using  (TokenClient tokenClient = TestUtil.CreateClient()) {
                UserMember payer = TestUtil.CreateUserMember();
                Alias payeeAlias = TestUtil.RandomAlias();
                TppMember payee = tokenClient.CreateMemberBlocking(payeeAlias);

                Token token = TestUtil.CreateTransferToken(payer, payeeAlias);

                Assert.Equal(GetTokensSample.GetToken(payee, token.Id).Id,token.Id);
                var sigList = GetTokensSample.GetToken(payee, token.Id).PayloadSignatures.Where(sig=>sig.Action==TokenSignature.Types.Action.Cancelled).ToList();
                Assert.Empty(sigList);
                // cancel token
                payee.CancelTokenBlocking(token);
                sigList = GetTokensSample.GetToken(payee, token.Id).PayloadSignatures.Where(sig => sig.Action == TokenSignature.Types.Action.Cancelled).ToList();
                Assert.NotEmpty(sigList);


            }
        }


    }
}
