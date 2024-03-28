using System.Collections;
using System.Collections.Generic;
using HeavenStudio;
using HeavenStudio.Games;
using HeavenStudio.Util;
using Jukebox;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Rendering;

namespace HeavenStudio.Games.Scripts_CatchOfTheDay
{
    public class BGFish : MonoBehaviour
    {
        [SerializeField] Animator _Animator;
        [SerializeField] SpriteRenderer _Sprite;
        [SerializeField] FleeAnimation FleeAnim;
        [SerializeField] bool FlipSprite;

        private bool Out = false;

        public void SetColor(Color color)
        {
            _Sprite.color = color;
        }
        public void Flee()
        {
            bool doFlip = transform.localScale.x < 0;// i hate this. it works

            switch (FleeAnim)
            {
                case FleeAnimation.WestSouthWest:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_ESE" : "BGFishOut_WSW", 0.5f);
                    break;
                case FleeAnimation.SouthWest:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_SE" : "BGFishOut_SW", 0.5f);
                    break;
                case FleeAnimation.WestNorthWest:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_ENE" : "BGFishOut_WNW", 0.5f);
                    break;
                case FleeAnimation.NorthWest:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_NE" : "BGFishOut_NW", 0.5f);
                    break;
                case FleeAnimation.West:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_E" : "BGFishOut_W", 0.5f);
                    break;
                case FleeAnimation.EastSouthEast:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_WSW" : "BGFishOut_ESE", 0.5f);
                    break;
                case FleeAnimation.SouthEast:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_SW" : "BGFishOut_SE", 0.5f);
                    break;
                case FleeAnimation.EastNorthEast:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_WNW" : "BGFishOut_ENE", 0.5f);
                    break;
                case FleeAnimation.NorthEast:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_NW" : "BGFishOut_NE", 0.5f);
                    break;
                case FleeAnimation.East:
                default:
                    _Animator.DoScaledAnimationAsync(doFlip ? "BGFishOut_W" : "BGFishOut_E", 0.5f);
                    break;
            }

            Out = true;
        }

        public enum FleeAnimation : int
        {
            East = 0,
            EastSouthEast = 1,
            SouthEast = 2,
            EastNorthEast = 3,
            NorthEast = 4,
            West = 8,
            WestSouthWest = 9,
            SouthWest = 10,
            WestNorthWest = 11,
            NorthWest = 12,
        }
    }
}