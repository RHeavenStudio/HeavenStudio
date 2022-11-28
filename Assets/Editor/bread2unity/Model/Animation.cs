using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bread2Unity
{
    public class Animation
    {
        public List<AnimationStep> Steps = new List<AnimationStep>();
        public string Name;
        public int InterpolationInt = 0;

        public bool Interpolated => (InterpolationInt & 0b1) > 0;
    }

    public class AnimationStep
    {
        public ushort Delay;

        public BccadSprite BccadSprite;

        public short TranslateX;
        public short TranslateY;
        
        public float Depth;
        
        public float StretchX;
        public float StretchY;

        public float Rotation;

        public byte Opacity;
        public Color Color;
    }
}