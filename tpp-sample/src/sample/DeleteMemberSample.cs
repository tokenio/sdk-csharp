using TppMember = Tokenio.Tpp.Member;

namespace Tokenio.Sample.Tpp {
    /// <summary>
    /// Deletes a member.
    /// </summary>
    public static class DeleteMemberSample {
        /// <summary>
        /// Deletes a member.
        /// </summary>
        /// <param name="member">member</param>
        public static void DeleteMember (TppMember member) {
            member.DeleteMember ();
        }
    }
}