using System.Linq;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.TokenProtos;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;
namespace TokenioSample
{
    public class GetTokensSampleTest
    {
        [Fact]
        public void GetTokenTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember payer = TestUtil.CreateMemberAndLinkAccounts(tokenClient);
                Alias granteeAlias = TestUtil.RandomAlias();
                UserMember payee = tokenClient.CreateMemberBlocking(granteeAlias);

                Token token = CreateAndEndorseTransferTokenSample.CreateTransferToken(payer, granteeAlias);

                Assert.Equal(GetTokensSample.GetToken(payer, token.Id).Id, token.Id);
                var sigList = GetTokensSample.GetToken(payer, token.Id).PayloadSignatures.Where(sig => sig.Action == TokenSignature.Types.Action.Cancelled).ToList();
                Assert.Empty(sigList);
                // cancel token
                payer.CancelTokenBlocking(token);
                sigList = GetTokensSample.GetToken(payer, token.Id).PayloadSignatures.Where(sig => sig.Action == TokenSignature.Types.Action.Cancelled).ToList();
                Assert.NotEmpty(sigList);


            }
        }
    }
}
