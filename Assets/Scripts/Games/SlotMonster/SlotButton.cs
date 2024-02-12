using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SlotMonster
{
    public class SlotButton : MonoBehaviour
    {
        public bool pressed;
        public Color color; // used to ease between button colors and button flash colors! wow

        public Animator anim;
        public SpriteRenderer[] srs;

        public bool flashing;
        const int FLASH_FRAMES = 4;
        public int currentFrame;

        private SlotMonster game;

        public void Init(SlotMonster instance)
        {
            color = srs[0].color;
            game = instance;
        }

        private void LateUpdate()
        {
            if (flashing) {
                foreach (var sr in srs) {
                    // sr.color = Color.Lerp(color, game.buttonFlashColor, currentFrame / FRAMES);

                    var normalized = currentFrame / FLASH_FRAMES;

                    float newR = EasingFunction.Linear(game.buttonFlashColor.r, color.r, normalized);
                    float newG = EasingFunction.Linear(game.buttonFlashColor.g, color.g, normalized);
                    float newB = EasingFunction.Linear(game.buttonFlashColor.b, color.b, normalized);

                    sr.color = new Color(newR, newG, newB);
                    Debug.Log("sr.color : " + sr.color);
                    Debug.Log("currentFrame : " + currentFrame);
                }
            } else {
                foreach (var sr in srs) {
                    sr.color = color;
                }
            }
        }

        public void Press()
        {
            anim.DoScaledAnimationAsync("Press", 0.5f);
            pressed = true;
        }

        public void TryFlash()
        {
            if (!pressed) {
                anim.DoScaledAnimationAsync("Flash", 0.5f);
            }
        }

        // animation events
        public void AnimateColor(int frame)
        {
            currentFrame = frame;
            flashing = frame < FLASH_FRAMES;
        }
    }
}
