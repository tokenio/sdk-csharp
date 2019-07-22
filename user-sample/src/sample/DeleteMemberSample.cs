using System;
using UserMember = Tokenio.User.Member;

namespace TokenioSample
{
    /// <summary>
    /// Delete member sample.
    /// </summary>
    public class DeleteMemberSample
    {
        public static void DeleteMember(UserMember member)
        {
            member.DeleteMember();
        }
    }
}
