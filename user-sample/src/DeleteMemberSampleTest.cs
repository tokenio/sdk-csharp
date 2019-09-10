using System;
using Xunit;
using UserMember = Tokenio.User.Member;


namespace Tokenio.Sample.User
{
    public class DeleteMemberSampleTest
    {
        [Fact]
        public void CreatePaymentTokenTest()
        {
            using (Tokenio.User.TokenClient tokenClient = TestUtil.CreateClient())
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

