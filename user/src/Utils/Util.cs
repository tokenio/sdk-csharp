using System;

namespace Tokenio.User.Utils
{
    public class Util : Tokenio.Utils.Util
    {
        public static string ParseOauthAccessToken(string fullUrl)
        {
            string[] stringSeparators = new string[] { "#|&" };
            string[] urlParts = fullUrl.Split(stringSeparators, StringSplitOptions.None);
            for (int i = (urlParts.Length - 1); (i >= 0); i--)
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
