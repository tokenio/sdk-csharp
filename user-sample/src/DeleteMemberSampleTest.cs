using System;
using Xunit;
using TokenClient = Tokenio.User.TokenClient;
using UserMember = Tokenio.User.Member;


namespace TokenioSample
{
    public class DeleteMemberSampleTest
    {
        [Fact]
        public void CreatePaymentTokenTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                UserMember member = TestUtil.CreateMemberAndLinkAccounts(tokenClient);

                Assert.Equal(tokenClient.GetMemberBlocking(member.MemberId()).MemberId(), member.MemberId());

                member.DeleteMemberBlocking();


                Assert.Throws<AggregateException>(() =>
                    tokenClient.GetMemberBlocking(member.MemberId())

                    );
            }
        }
    }
}

