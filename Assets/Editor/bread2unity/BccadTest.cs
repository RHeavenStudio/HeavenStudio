using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Bread2Unity
{
    public static class BccadTest
    {
        public static void TestBccad()
        {
            const string folderPath =
                "C:\\Users\\Eliya\\games\\3DS\\PackEnglishV12\\PackEnglishV12\\PackHack\\ExtractedRomFS\\cellanim";
            var allFiles = Directory.GetFiles(folderPath, "*.bccad", SearchOption.AllDirectories);
            var problematicFiles = new HashSet<string>();
            foreach (var file in allFiles)
            {
                var bccad = BCCAD.Read(File.ReadAllBytes(file));
                var name = Path.GetFileName(file);
                for (var spriteIndex = 0; spriteIndex < bccad.sprites.Count; spriteIndex++)
                {
                    var sprite = bccad.sprites[spriteIndex];
                    /*for (var partIndex = 0; partIndex < sprite.parts.Count; partIndex++)
                    {
                        var part = sprite.parts[partIndex];
                        if (part.Multicolor != Color.white)
                            Debug.Log($"multycolor not white at {name} sprite: {spriteIndex} part: {partIndex}");

                        if (part.ScreenColor != Color.black)
                            Debug.Log($"screen color not black at {name} sprite: {spriteIndex} part: {partIndex}");
                    }*/
                    var v = sprite.parts.GroupBy(p => p.FlipX)
                        .Any(a => a.Select(part => part.RegionIndex.Index).Distinct().Count() < a.Count());
                    // if (sprite.parts.Select(part => part.Region).Distinct().Count() < sprite.parts.Count)
                    if(v)
                        // Debug.Log($"duplicate regions {name} sprite: {spriteIndex}");
                        problematicFiles.Add(name);
                }

                /*for (var animIndex = 0; animIndex < bccad.animations.Count; animIndex++)
                {
                    var anim = bccad.animations[animIndex];
                    if (anim.Interpolated) Debug.Log($"interpolated {name} anim: {animIndex}");
                    for (var stepIndex = 0; stepIndex < anim.Steps.Count; stepIndex++)
                    {
                        var step = anim.Steps[stepIndex];
                        if(step.Color != Color.white)
                            Debug.Log($"step color not white at {name} anim: {animIndex} step: {stepIndex}");
                        if (step.Opacity != 255)
                            Debug.Log($"step opacity not 255 at {name} anim: {animIndex} step: {stepIndex}");
                    }
                }*/
            }

            foreach (var filename in problematicFiles)
            {
                Debug.Log(filename);
            }
        }
    }
}