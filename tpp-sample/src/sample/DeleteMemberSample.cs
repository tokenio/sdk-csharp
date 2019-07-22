using System;
using TppMember = Tokenio.Tpp.Member;

namespace TokenioSample
{
    /// <summary>
    /// Delete member sample.
    /// </summary>
    public class DeleteMemberSample
    {
        public static void DeleteMember(TppMember member)
        {
            member.DeleteMember();
        }
    }
}
