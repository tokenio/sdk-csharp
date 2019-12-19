using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.User
{
    /// <summary>
    /// Deletes a member.
    /// </summary>
    public static class DeleteMemberSample
    {
        /// <summary>
        /// Deletes a member.
        /// </summary>
        /// <param name="member">member</param>
        public static void DeleteMember(UserMember member)
        {
            member.DeleteMember();
        }
    }
}