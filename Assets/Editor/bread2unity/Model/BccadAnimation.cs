using System.Collections.Generic;
using UnityEngine;

namespace Bread2Unity
{
    public class BccadAnimation
    {
        public int InterpolationInt = 0;
        public string Name;
        public List<AnimationStep> Steps = new List<AnimationStep>();

        public bool Interpolated => (InterpolationInt & 0b1) > 0;
    }

    public class AnimationStep
    {
        public BccadSprite BccadSprite;
        public Color Color;
        
        public ushort Delay;

        public float Depth;

        public float Rotation;

        public float StretchX;
        public float StretchY;

        public short TranslateX;
        public short TranslateY;
    }
}