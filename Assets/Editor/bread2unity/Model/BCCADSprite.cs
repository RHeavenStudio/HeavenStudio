using System;
using System.Collections;
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
        public RegionIndex RegionIndex;

        public short PosX;
        public short PosY;

        public float StretchX;
        public float StretchY;

        public float Rotation;

        public bool FlipX;
        public bool FlipY;
        

        public byte designation;
        public short unknown;
        public float tlDepth;
        public float blDepth;
        public float trDepth;
        public float brDepth;
        public Color Multicolor;
        public Color ScreenColor;
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
            int hash = 23;
            hash = hash * 31 + Index;
            hash = hash * 31 + RepeatedNumber;
            return hash;
        }
    }
}