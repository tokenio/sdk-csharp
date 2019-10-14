using System.Text.RegularExpressions;

namespace Tokenio.User.Utils {
    /// <summary>
    /// Utility methods.
    /// </summary>
    public class Util : Tokenio.Utils.Util {
        /// <summary>
        /// Returns the first capturing group in this regex pattern's match.
        /// </summary>
        /// <param name = "text">text to match</param>
        /// <param name="regex">regex pattern with a capturing group</param>
        /// <returns>the first capturing group or null if not found</returns>
        public static string FindFirstCapturingGroup(string text, string regex) {
            Regex pattern = new Regex(regex);
            Match m = pattern.Match(text);
            if (m.Groups.Count > 0 && m.Length == regex.Length) {
                GroupCollection groupCollection = m.Groups;
                return groupCollection[1].ToString();
            }
            return null;
        }
    }
}
