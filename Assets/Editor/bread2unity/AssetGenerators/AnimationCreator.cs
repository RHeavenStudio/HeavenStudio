using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bread2Unity;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Animation = Bread2Unity.Animation;

namespace Bread2Unity
{
    public static class AnimationCreator
    {
        private class BccadCurve : AnimationCurve
        {
            private float _prev;

            public new void AddKey(float time, float value)
            {
                if (keys.Length != 0 && !(Math.Abs(value - _prev) > 0.000001)) return;
                AddKey(new Keyframe(time, value, float.PositiveInfinity, float.PositiveInfinity));
                _prev = value;
            }

            public void CopyLastKey(float time)
            {
                Keyframe lastKey = keys.LastOrDefault();

                base.AddKey(time, keys.Length > 0 ? lastKey.value : 1);
            }
        }

        public static void CreateAnimation(BccadPrefab bccadPrefab, BCCAD bccad, PrefabData prefabData,
            List<Sprite> sprites)
        {
            var gameObject = bccadPrefab.ParentObject;
            var rootPrefabName = gameObject.transform.parent.gameObject.name;
            var spritesFolderPath =
                $"Assets\\Resources\\Sprites\\Games\\{char.ToUpperInvariant(rootPrefabName[0]) + rootPrefabName.Substring(1)}";

            var animationsFolderPath = spritesFolderPath + $"/Animations/{prefabData.Name}";
            if (!Directory.Exists(animationsFolderPath))
            {
                Directory.CreateDirectory(animationsFolderPath);
            }

            var controller =
                AnimatorController.CreateAnimatorControllerAtPath(
                    AssetDatabase.GenerateUniqueAssetPath(
                        $"{animationsFolderPath}/{prefabData.Name}Controller.controller"));
            var bccadSprites = bccad.sprites;

            //get all of the parts associated with the game object
            var steps = prefabData.Animations.SelectMany(animation => animation.Steps);
            var bccadSpritesOfPrefab = steps.Select(step => step.BccadSprite).ToList();


            foreach (var animation in prefabData.Animations)
            {
                var clip = CreateAnimationClip(bccadPrefab, animation, sprites, bccadSpritesOfPrefab);

                AssetDatabase.CreateAsset(clip,
                    AssetDatabase.GenerateUniqueAssetPath(
                        $"{animationsFolderPath}/{animation.Name}.anim"));
                controller.AddMotion(clip);
            }

            var animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
        }

        private static AnimationClip CreateAnimationClip(BccadPrefab bccadPrefab, Animation animation,
            List<Sprite> sprites,
            IReadOnlyCollection<BccadSprite> spritesAssociatedWithPrefab)
        {
            var animationClip = new AnimationClip();
            var prefab = bccadPrefab.ParentObject;
            for (int childIndex = 0; childIndex < prefab.transform.childCount; childIndex++)
            {
                var child = prefab.transform.GetChild(childIndex).gameObject;

                var partsOfGameObject = spritesAssociatedWithPrefab.SelectMany(sprite => sprite.parts)
                    .Where(part => bccadPrefab.RegionToChild[part.RegionIndex] == child).ToList();

                var enabledCurve = new BccadCurve();

                var xTransformCurve = new BccadCurve();
                var yTransformCurve = new BccadCurve();
                var zTransformCurve = new BccadCurve();

                var rotationCurve = new BccadCurve();
                
                var flipXCurve = new BccadCurve();
                var flipYCurve = new BccadCurve();

                var scaleXCurve = new BccadCurve();
                var scaleYCurve = new BccadCurve();

                var spriteFrames = new List<ObjectReferenceKeyframe>();

                var currentTime = 0f;

                for (int stepIndex = 0; stepIndex < animation.Steps.Count; stepIndex++)
                {
                    var currentStep = animation.Steps[stepIndex];
                    var bccadSprite = currentStep.BccadSprite;
                    // Find the index of part of the game object
                    var partIndex = bccadSprite.parts.Select((value, index) => new { value, index })
                        .Where(pair => bccadPrefab.RegionToChild[pair.value.RegionIndex] == child)
                        .Select(pair => pair.index).DefaultIfEmpty(-1)
                        .FirstOrDefault();
                    
                    enabledCurve.AddKey(currentTime, partIndex == -1 ? 0 : 1);
                    
                    if (partIndex != -1)
                    {
                        var bccadSpritePart = bccadSprite.parts[partIndex];

                        var sprite = sprites[bccadSpritePart.RegionIndex.Index];
                        var width = bccadSpritePart.StretchX / bccadPrefab.WidthRatio;
                        var height = bccadSpritePart.StretchY / bccadPrefab.HeightRatio;
                        var x = (bccadSpritePart.PosX - 512f) /
                            SpriteCreator.PixelsPerUnit + sprite.bounds.size.x * 0.5f * width;
                        var y = -(bccadSpritePart.PosY - 512f) / SpriteCreator.PixelsPerUnit -
                                sprite.bounds.size.y * 0.5f * height;
                        var z = -0.00001f * partIndex;

                        xTransformCurve.AddKey(currentTime, x);
                        yTransformCurve.AddKey(currentTime, y);
                        zTransformCurve.AddKey(currentTime, z);

                        scaleXCurve.AddKey(currentTime, width);
                        scaleYCurve.AddKey(currentTime, height);

                        if (spriteFrames.Count == 0 || spriteFrames.Last().value != sprite)
                        {
                            var spriteKeyframe = new ObjectReferenceKeyframe
                            {
                                time = currentTime,
                                value = sprite
                            };
                            spriteFrames.Add(spriteKeyframe);
                        }

                        flipXCurve.AddKey(currentTime, bccadSpritePart.FlipX ? 1 : 0);
                        flipYCurve.AddKey(currentTime, bccadSpritePart.FlipY ? 1 : 0);
                        
                        rotationCurve.AddKey(currentTime, -bccadSpritePart.Rotation);
                    }

                    // Increase the time for the next frame
                    currentTime += currentStep.Delay / 30f;
                }

                if (childIndex == 0)
                {
                    enabledCurve.CopyLastKey(currentTime);
                }
                
                var spriteBinding = new EditorCurveBinding
                {
                    type = typeof(SpriteRenderer),
                    propertyName = "m_Sprite",
                    path = $"{prefab.name} {childIndex}"
                };

                var animateActive = childIndex == 0 || spritesAssociatedWithPrefab.Any(sprite =>
                    sprite.parts.All(part => bccadPrefab.RegionToChild[part.RegionIndex] != child));
                if (animateActive)
                    animationClip.SetCurve(child.name, typeof(GameObject), "m_IsActive", enabledCurve);
                if ((from part in partsOfGameObject select part.FlipX).Distinct().Count() > 1)
                    animationClip.SetCurve(child.name, typeof(SpriteRenderer), "m_FlipX", flipXCurve);
                if ((from part in partsOfGameObject select part.FlipY).Distinct().Count() > 1)
                    animationClip.SetCurve(child.name, typeof(SpriteRenderer), "m_FlipY", flipYCurve);
                if ((from part in partsOfGameObject select part.RegionIndex.Index).Distinct().Count() > 1)
                    AnimationUtility.SetObjectReferenceCurve(animationClip, spriteBinding, spriteFrames.ToArray());
                if ((from part in partsOfGameObject select part.PosX).Distinct().Count() > 1 ||
                    (from part in partsOfGameObject select part.PosY).Distinct().Count() > 1)
                {
                    animationClip.SetCurve(child.name, typeof(Transform), "localPosition.x", xTransformCurve);
                    animationClip.SetCurve(child.name, typeof(Transform), "localPosition.y", yTransformCurve);
                    animationClip.SetCurve(child.name, typeof(Transform), "localPosition.z", zTransformCurve);
                }

                if ((from part in partsOfGameObject select part.Rotation).Distinct().Count() > 1)
                {
                    animationClip.SetCurve(child.name, typeof(Transform), "localEulerAngles.z", rotationCurve);
                }

                if ((from part in partsOfGameObject select part.StretchX).Distinct().Count() > 1 ||
                    (from part in partsOfGameObject select part.StretchY).Distinct().Count() > 1)
                {
                    animationClip.SetCurve(child.name, typeof(Transform), "localScale.x", scaleXCurve);
                    animationClip.SetCurve(child.name, typeof(Transform), "localScale.y", scaleYCurve);
                }
            }

            animationClip.frameRate = 30; //fps
            animationClip.name = animation.Name;

            return animationClip;
        }
    }
}