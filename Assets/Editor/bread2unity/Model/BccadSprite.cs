using System.Collections.Generic;
using UnityEngine;

namespace Bread2Unity
{
    public class BccadSprite
    {
        public List<SpritePart> parts = new List<SpritePart>();
    }

    public class SpritePart
    {
        public float blDepth;
        public float brDepth;


        public byte designation;

        public bool FlipX;
        public bool FlipY;
        public Color Multicolor;

        public short PosX;
        public short PosY;
        public RegionIndex RegionIndex;

        public float Rotation;
        public Color ScreenColor;

        public float StretchX;
        public float StretchY;
        public float tlDepth;
        public float trDepth;

        public short unknown;
        // public Color GetColor() => new Color(Multicolor.r, Multicolor.g, Multicolor.b, Multicolor.a);
    }

    public struct RegionIndex
    {
        public int Index { get; }
        public int RepeatedNumber { get; }

        public RegionIndex(int index, int repeatedNumber)
        {
            RepeatedNumber = repeatedNumber;
            Index = index;
        }

        public bool Equals(RegionIndex other)
        {
            return Index == other.Index && RepeatedNumber == other.RepeatedNumber;
        }

        public override bool Equals(object obj)
        {
            return obj is RegionIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 31 + Index;
            hash = hash * 31 + RepeatedNumber;
            return hash;
        }
    }
}