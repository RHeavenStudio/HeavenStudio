using System;
using System.Collections;
using System.Collections.Generic;

namespace Bread2Unity
{
    public class DataModel
    {
        public List<Region> regions = new List<Region>();
        public List<BccadSprite> sprites = new List<BccadSprite>();
        public List<Animation> animations = new List<Animation>();
        public int sheetW;
        public int sheetH;
    }

    public class Region
    {
        public ushort regionX;
        public ushort regionY;
        public ushort regionW;
        public ushort regionH;

        public override string ToString()
        {
            return $"regionX: {regionX} regionY: {regionY} regionW: {regionW} regionH: {regionH}";
        }

        protected bool Equals(Region other)
        {
            return regionX == other.regionX && regionY == other.regionY && regionW == other.regionW &&
                   regionH == other.regionH;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Region)obj);
        }

        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 31 + regionX;
            hash = hash * 31 + regionY;
            hash = hash * 31 + regionW;
            hash = hash * 31 + regionH;
            return hash;
        }
    }
}