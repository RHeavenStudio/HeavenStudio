using System.Collections.Generic;
using Bread2Unity;

namespace Bread2Unity
{
    public class PrefabData
    {
        public string Name;
        public List<Animation> Animations;
        public int SpriteIndex;

        public PrefabData(string name, int spriteIndex)
        {
            Name = name;
            SpriteIndex = spriteIndex;
        }
    }
}