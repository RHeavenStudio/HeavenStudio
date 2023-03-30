using System;

namespace HeavenStudio
{
    public enum NodeType
    {
        Chromatic_Aberration,
        Vignette,
        Bloom,
        Grain,
        Lens_Distortion,
        Pixelize,
        Glitch,
        Jitter,
        CRT
    }

    [Serializable]
    public class Node
    {
        public NodeType Type; // I spent a few hours trying to avoid this, honestly if you can't beat them, join them.
        public float Beat;
        public float Intensity;

        public Node(NodeType type, float beat, float intensity)
        {
            Type = type;
            Beat = beat;
            Intensity = intensity;
        }
    }
}
