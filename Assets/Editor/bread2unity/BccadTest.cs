using System;
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
            // var problematicFiles = new HashSet<string>();
            foreach (var file in allFiles)
            {
                var bccad = BCCAD.Read(File.ReadAllBytes(file));
                var name = Path.GetFileName(file);
                /*for (var spriteIndex = 0; spriteIndex < bccad.sprites.Count; spriteIndex++)
                {
                    var sprite = bccad.sprites[spriteIndex];
                    for (var partIndex = 0; partIndex < sprite.parts.Count; partIndex++)
                    {
                        var part = sprite.parts[partIndex];
                        if (part.Multicolor != Color.white)
                            Debug.Log($"multycolor not white at {name} sprite: {spriteIndex} part: {partIndex}");

                        if (part.ScreenColor != Color.black)
                            Debug.Log($"screen color not black at {name} sprite: {spriteIndex} part: {partIndex}");
                    }
                }*/

                for (var animIndex = 0; animIndex < bccad.animations.Count; animIndex++)
                {
                    var anim = bccad.animations[animIndex];
                    // if (anim.Interpolated) Debug.Log($"interpolated {name} anim: {animIndex}");
                    if(anim.Steps.Select(step => step.TranslateX).Distinct().Count() > 1)
                        Debug.Log($"translation x changes at {name} anim: {animIndex}");
                    if(anim.Steps.Select(step => step.TranslateY).Distinct().Count() > 1)
                        Debug.Log($"translation y changes at {name} anim: {animIndex}");
                    /*for (var stepIndex = 0; stepIndex < anim.Steps.Count; stepIndex++) 
                    {
                        var step = anim.Steps[stepIndex];
                        /*if(step.Color != Color.white)
                            Debug.Log($"step color not white at {name} anim: {animIndex} step: {stepIndex}");#1#
                        /*if (step.Opacity != 255)
                            Debug.Log($"step opacity not 255 at {name} anim: {animIndex} step: {stepIndex}");#1#
                        /*if (Math.Abs(step.StretchX - 1f) > 0.0000001)
                        {
                            Debug.Log($"stretch x at {name} anim: {animIndex} step: {stepIndex}");
                        }
                        if (Math.Abs(step.StretchY - 1f) > 0.0000001)
                        {
                            Debug.Log($"stretch y at {name} anim: {animIndex} step: {stepIndex}");
                        }#1#
                        /*if (step.Rotation > 0)
                        {
                            Debug.Log($"rotation at {name} anim: {animIndex} step: {stepIndex}");
                        }#1#
                    }*/
                    
                }
            }

            /*foreach (var filename in problematicFiles)
            {
                Debug.Log(filename);
            }*/
        }
    }
}