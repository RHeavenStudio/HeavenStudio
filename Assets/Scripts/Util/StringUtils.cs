using System.Globalization;
using System.Text.RegularExpressions;

using UnityEngine;

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

        public static string Color2Hex(this Color color)
        {
            Color32 col = (Color32)color;
            string hex = col.r.ToString("X2") + col.g.ToString("X2") + col.b.ToString("X2");
            return hex;
        }

        public static Color Hex2RGB(this string hex)
        {
            if (hex is null or "") return Color.black;
            try
            {
                hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
                hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
                byte a = 255;//assume fully visible unless specified in hex
                byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                //Only use alpha if the string has enough characters
                if (hex.Length >= 8)
                {
                    a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                }
                return new Color32(r, g, b, a);
            }
            catch
            {
                return Color.black;
            }
        }
    }
}