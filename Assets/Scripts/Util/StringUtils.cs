using System.Globalization;
using System.Text.RegularExpressions;

namespace HeavenStudio.Util
{
    public static class StringUtils
    {
        public static string DisplayName(this string name)
        {
            // "gameName" -> "Game Name"
            // "action name" -> "Action Name"
            if (!name.Contains(" "))
                name = SplitCamelCase(name);
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(name);
        }

        // https://stackoverflow.com/a/5796793
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }
    }
}