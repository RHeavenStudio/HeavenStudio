using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio
{
    public static class Datamodel
    {
        public static string Split(this string s, int index)
        {
            return s.Split('/')[index];
        }

        public static string GetExtension(this string s)
        {
            string[] split = s.Split('.');
            return split[split.Length - 1];
        }
    }
}