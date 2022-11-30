using System;
using System.Collections.Generic;
using System.Linq;
using Bread2Unity;
using UnityEngine;

namespace Bread2Unity
{
    public class BccadPrefab
    {
        public GameObject ParentObject { get; }
        public readonly Dictionary<RegionIndex, GameObject> RegionToChild = new Dictionary<RegionIndex, GameObject>();
        private readonly List<GameObject> _children = new List<GameObject>();
        private readonly PrefabData _data;
        private readonly BCCAD _bccad;
        public float HeightRatio { get; }
        public float WidthRatio { get; }

        public BccadPrefab(PrefabData data, BCCAD bccad, Texture2D texture)
        {
            ParentObject = new GameObject(data.Name);
            _data = data;
            HeightRatio = (float)texture.height / bccad.sheetH;
            WidthRatio = (float)texture.width / bccad.sheetW;
            _bccad = bccad;
            CalculateParts();
        }

        private void CalculateParts()
        {
            var defaultSprite = _bccad.sprites[_data.SpriteIndex];
            if (_data.Animations.Count == 0)
            {
                for (var index = 0; index < defaultSprite.parts.Count; index++)
                {
                    var part = defaultSprite.parts[index];
                    var child = new GameObject($"{_data.Name} {index}");
                    child.transform.SetParent(ParentObject.transform);
                    _children.Add(child);
                    RegionToChild.Add(part.RegionIndex, child);
                }
                return;
            }
            // Get all regions
            var anim = _data.Animations;
            var bccadSprites = anim.SelectMany(a => a.Steps).Select(step => step.BccadSprite).Distinct().ToList();
            var availableRegions = bccadSprites.SelectMany(sprite => sprite.parts).Select(part => part.RegionIndex)
                .Distinct().ToList();

            var childIndex = 0;
            while (availableRegions.Count > 0)
            {
                var child = new GameObject($"{_data.Name} {childIndex}");
                child.transform.SetParent(ParentObject.transform);
                _children.Add(child);
                var nextRegion = availableRegions[0];
                availableRegions.RemoveAt(0);
                var regionsList = new List<RegionIndex> { nextRegion };
                while (true)
                {
                    var notAdjacentRegions = FindNotAdjacentRegions(regionsList, bccadSprites, availableRegions);
                    var maybeNotAdjacentRegion = availableRegions.Select(reg => (RegionIndex?)reg)
                        .FirstOrDefault(region => notAdjacentRegions.Contains((RegionIndex)region));
                    if (maybeNotAdjacentRegion is { } notAdjacentRegion)
                    {
                        availableRegions.Remove(notAdjacentRegion);
                        regionsList.Add(notAdjacentRegion);
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var r in regionsList)
                {
                    RegionToChild.Add(r, child);
                }

                childIndex++;
            }
        }

        private static List<RegionIndex> FindNotAdjacentRegions(List<RegionIndex> regions, List<BccadSprite> sprites, List<RegionIndex> availableRegions)
        {
            var notAdjacentRegions = new List<RegionIndex>(availableRegions);
            foreach (var sprite in sprites)
            {
                var regionsOfSprite = sprite.parts.Select(part => part.RegionIndex).ToArray();
                if (regionsOfSprite.Intersect(regions).Any())
                {
                    foreach (var r in regionsOfSprite)
                    {
                        notAdjacentRegions.Remove(r);
                    }
                }

            }

            return notAdjacentRegions;
        }

        public List<Tuple<int, SpritePart, GameObject>> GetHiddenParts()
        {
            var sprite = _bccad.sprites[_data.SpriteIndex];
            var pairs = new List<Tuple<int, SpritePart, GameObject>>();
            var gameObjects = new List<GameObject>(_children);
            foreach (var part in sprite.parts)
            {
                var child = RegionToChild[part.RegionIndex];
                gameObjects.Remove(child);
            }

            foreach (var gameObject in gameObjects)
            {
                //find a random part associated with the game object
                var region = RegionToChild.FirstOrDefault(keyValuePair => keyValuePair.Value == gameObject)
                    .Key;
                var parts = _data.Animations.SelectMany(anim => anim.Steps).Select(s => s.BccadSprite)
                    .SelectMany(bccadSprite => bccadSprite.parts);

                var partIndexPair = parts
                    .Select((value, index) => new { value, index })
                    .First(pair => pair.value.RegionIndex.Equals(region));
                var index = partIndexPair.index;
                var part = partIndexPair.value;
                pairs.Add(new Tuple<int, SpritePart, GameObject>(index, part, gameObject));
                
            }

            return pairs;
        }
    }
}