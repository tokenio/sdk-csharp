using System;

namespace Tokenio.User.Utils
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public class Util : Tokenio.Utils.Util
    {
        /// <summary>
        /// Retrieve the access token from the URL fragment, given the full URL.
        /// </summary>
        /// <param name = "fullUrl">full url</param>
        /// <returns>oauth access token, or null if not found</returns>
        public static string ParseOauthAccessToken(string fullUrl)
        {
            string[] stringSeparators = {
                "#|&"
            };
            string[] urlParts = fullUrl.Split(stringSeparators, StringSplitOptions.None);
            for (int i = (urlParts.Length - 1); i >= 0; i--)
            {
                if (urlParts[i].Contains("access_token="))
                {
                    return urlParts[i].Substring(13);
                }
            }
            return null;
        }
    }
}