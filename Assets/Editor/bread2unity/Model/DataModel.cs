using System.Collections.Generic;

namespace Bread2Unity
{
    public class DataModel
    {
        public List<Animation> Animations = new List<Animation>();
        public List<Region> Regions = new List<Region>();
        public int SheetH;
        public int SheetW;
        public List<BccadSprite> Sprites = new List<BccadSprite>();
    }

    public class Region
    {
        public ushort RegionH;
        public ushort RegionW;
        public ushort RegionX;
        public ushort RegionY;

        public override string ToString()
        {
            return $"regionX: {RegionX} regionY: {RegionY} regionW: {RegionW} regionH: {RegionH}";
        }

        protected bool Equals(Region other)
        {
            return RegionH == other.RegionH && RegionW == other.RegionW && RegionX == other.RegionX &&
                   RegionY == other.RegionY;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Region)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RegionH.GetHashCode();
                hashCode = (hashCode * 397) ^ RegionW.GetHashCode();
                hashCode = (hashCode * 397) ^ RegionX.GetHashCode();
                hashCode = (hashCode * 397) ^ RegionY.GetHashCode();
                return hashCode;
            }
        }
    }
}