using System;
using Xunit;
using TokenClient = Tokenio.Tpp.TokenClient;
using TppMember = Tokenio.Tpp.Member;
using UserMember = Tokenio.User.Member;


namespace TokenioSample
{
    public class DeleteMemberSampleTest
    {
        [Fact]
        public void DeleteMemberTest()
        {
            using (TokenClient tokenClient = TestUtil.CreateClient())
            {
                TppMember member = tokenClient.CreateMemberBlocking(TestUtil.RandomAlias());

                Assert.Equal(tokenClient.GetMemberBlocking(member.MemberId()).MemberId(), member.MemberId());

                member.DeleteMemberBlocking();


                Assert.Throws<AggregateException>(() =>
                    tokenClient.GetMemberBlocking(member.MemberId())

                    );
            }

        }

    }
}

