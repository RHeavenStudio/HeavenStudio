using System;
using System.Collections.Generic;
using System.Linq;
using Bread2Unity;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Animation = Bread2Unity.Animation;

namespace Bread2Unity
{
    public static class PrefabCreator
    {
        public static void CreatePrefab(GameObject prefab, BCCAD bccad,
            List<PrefabData> prefabDataList, Texture2D texture)
        {
            var prefabName = prefab.name;
            var spritesFolderPath =
                $"Sprites\\Games\\{char.ToUpperInvariant(prefabName[0]) + prefabName.Substring(1)}";
            var prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
            var root = PrefabUtility.LoadPrefabContents(prefabAssetPath);
            foreach (var prefabData in prefabDataList)
            {
                var defaultSprite = bccad.sprites[prefabData.SpriteIndex];
                var bccadPrefab = new BccadPrefab(prefabData, bccad, texture);
                var newPrefab = bccadPrefab.ParentObject;
                newPrefab.transform.SetParent(root.transform);
                var sprites = Resources.LoadAll<Sprite>(spritesFolderPath).ToList()
                    .FindAll(s => s.name.Contains(texture.name));
                for (var index = 0; index < defaultSprite.parts.Count; index++)
                {
                    var spritePart = defaultSprite.parts[index];
                    var gameObjectPart = bccadPrefab.RegionToChild[spritePart.RegionIndex];
                    ApplySpriteSettingsFromBccad(gameObjectPart, spritePart, bccadPrefab,
                        sprites[spritePart.RegionIndex.Index],
                        index);
                }

                foreach (var (partIndex, spritePart, hiddenGameObject) in bccadPrefab.GetHiddenParts())
                {
                    hiddenGameObject.SetActive(false);
                    ApplySpriteSettingsFromBccad(hiddenGameObject, spritePart, bccadPrefab,
                        sprites[spritePart.RegionIndex.Index], partIndex);
                }

                if (prefabData.Animations.Count > 0)
                    AnimationCreator.CreateAnimation(bccadPrefab, bccad, prefabData, sprites);

                PrefabUtility.SaveAsPrefabAsset(root, AssetDatabase.GetAssetPath(prefab));
            }
        }

        private static void ApplySpriteSettingsFromBccad(GameObject gameObjectPart, SpritePart spritePart,
            BccadPrefab prefab, Sprite sprite, int index)
        {
            var spriteRenderer = (SpriteRenderer)gameObjectPart.AddComponent(typeof(SpriteRenderer));

            spriteRenderer.sprite = sprite;
            spriteRenderer.flipX = spritePart.FlipX;
            spriteRenderer.color = spritePart.Multicolor;
            spriteRenderer.flipY = spritePart.FlipY;
            spriteRenderer.enabled = true;
            gameObjectPart.transform.SetParent(prefab.ParentObject.transform);

            // Bread draws sprites from the edge, and unity from the middle.
            var width = spritePart.StretchX / prefab.WidthRatio;
            var height = spritePart.StretchY / prefab.HeightRatio;
            // var pixelsPerUnit = 73;

            var position =
                new Vector3(
                    (spritePart.PosX - 512f) /
                    SpriteCreator.PixelsPerUnit + sprite.bounds.size.x * 0.5f * width,
                    -(spritePart.PosY - 512f) / SpriteCreator.PixelsPerUnit -
                    sprite.bounds.size.y * 0.5f * height,
                    -0.00001f * index);
            var rotation = Quaternion.AngleAxis(spritePart.Rotation, new Vector3(0, 0, -1));
            gameObjectPart.transform.localPosition = position;
            gameObjectPart.transform.localRotation = rotation;
            gameObjectPart.transform.localScale = new Vector3(width, height, 1f);
        }
    }
}