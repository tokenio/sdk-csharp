using System;
using Xunit;
using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp {
    public class DeleteMemberSampleTest {
        [Fact]
        public void DeleteMemberTest () {
            using (Tokenio.Tpp.TokenClient tokenClient = TestUtil.CreateClient ()) {
                TppMember member = tokenClient.CreateMemberBlocking (TestUtil.RandomAlias ());

                Assert.Equal (tokenClient.GetMemberBlocking (member.MemberId ()).MemberId (), member.MemberId ());

                member.DeleteMemberBlocking ();

                Assert.Throws<AggregateException> (() =>
                    tokenClient.GetMemberBlocking (member.MemberId ())

                );
            }

        }

    }
}