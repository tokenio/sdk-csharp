using Tokenio;

namespace samples
{
    /// <summary>
    /// Deletes the member.
    /// </summary>
    public class DeleteMemberSample
    {
        /// <summary>
        /// Deletes a member
        /// </summary>
        /// <param name="member">member</param>
        public static void deleteMember(Member member)
        {
            member.DeleteMember();
        }
    }
}