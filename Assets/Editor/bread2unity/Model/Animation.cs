using System.Collections.Generic;
using UnityEngine;

namespace Bread2Unity
{
    public class Animation
    {
        public int InterpolationInt = 0;
        public string Name;
        public List<AnimationStep> Steps = new List<AnimationStep>();

        public bool Interpolated => (InterpolationInt & 0b1) > 0;
    }

    public class AnimationStep
    {
        public BccadSprite BccadSprite;
        public Color Color; //needs to add
        public ushort Delay;

        public float Depth;

        public byte Opacity; //needs to add

        public float Rotation; //needs to add

        public float StretchX; //needs to add
        public float StretchY; //needs to add

        public short TranslateX;
        public short TranslateY;
    }
}