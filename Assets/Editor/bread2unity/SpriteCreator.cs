using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using Bread2Unity;

namespace Bread2Unity
{
    public class SpriteCreator : MonoBehaviour
    {
        public const int PixelsPerUnit = 100;
        public static Texture2D ComputeSprites(BCCAD bccad, string texturePath, string prefabName, bool shouldRotate = false)
        {
            var textureName = Path.GetFileName(texturePath);
            var spritesFolder =
                $"Assets\\Resources\\Sprites\\Games\\{char.ToUpperInvariant(prefabName[0]) + prefabName.Substring(1)}\\";
            if (!Directory.Exists(spritesFolder))
            {
                Directory.CreateDirectory(spritesFolder);
            }

            var destTexturePath =
                spritesFolder +
                $"{textureName}";
            var newTexture = new Texture2D(bccad.sheetW, bccad.sheetH);
            newTexture.LoadImage(File.ReadAllBytes(texturePath));
            var finalTexture = shouldRotate ? RotateTexture(newTexture) : newTexture;
            finalTexture.name = textureName.Substring(0, textureName.Length - ".png".Length);
            File.WriteAllBytes(destTexturePath, finalTexture.EncodeToPNG());
            AssetDatabase.ImportAsset(destTexturePath);
            var ti = AssetImporter.GetAtPath(destTexturePath) as TextureImporter;
            
            if (ti != null)
            {
                ti.isReadable = true;
                //  constants
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Multiple;
                ti.spritePixelsPerUnit = PixelsPerUnit;
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                var newData = new List<SpriteMetaData>();
                var rectCtr = 0;
                var heightRatio = (float)finalTexture.height / bccad.sheetH;
                var widthRatio = (float)finalTexture.width / bccad.sheetW;
                foreach (var r in bccad.regions)
                {
                    var smd = new SpriteMetaData
                    {
                        pivot = new Vector2(0.5f, 0.5f),
                        alignment = 0,
                        name = finalTexture.name + "_" + rectCtr,
                        rect = new Rect(r.regionX * widthRatio,
                            finalTexture.height - (r.regionH + r.regionY) * heightRatio, r.regionW * widthRatio,
                            r.regionH * heightRatio)
                    };

                    newData.Add(smd);
                    rectCtr++;
                }

                ti.spritesheet = newData.ToArray();
            }

            AssetDatabase.ImportAsset(destTexturePath, ImportAssetOptions.ForceUpdate);
            return finalTexture;
        }

        public static Texture2D RotateTexture(Texture2D image)
        {
            Texture2D
                target = new Texture2D(image.height, image.width, image.format,
                    false); //flip image width<>height, as we rotated the image, it might be a rect. not a square image

            Color32[] pixels = image.GetPixels32(0);
            pixels = RotateTextureGrid(pixels, image.width, image.height);
            target.SetPixels32(pixels);
            target.Apply();

            //flip image width<>height, as we rotated the image, it might be a rect. not a square image

            return target;
        }


        private static Color32[] RotateTextureGrid(Color32[] tex, int wid, int hi)
        {
            Color32[] ret = new Color32[wid * hi]; //reminder we are flipping these in the target

            for (int y = 0; y < hi; y++)
            {
                for (int x = 0; x < wid; x++)
                {
                    ret[(hi - 1) - y + x * hi] = tex[x + y * wid]; //juggle the pixels around
                }
            }

            return ret;
        }
    }
}