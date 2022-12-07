using System.Collections.Generic;

namespace Bread2Unity
{
    public class PrefabData
    {
        public List<BccadAnimation> Animations;
        public string Name;
        public int SpriteIndex;

        public PrefabData(string name, int spriteIndex)
        {
            Name = name;
            SpriteIndex = spriteIndex;
        }
    }
}