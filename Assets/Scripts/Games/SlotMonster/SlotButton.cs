using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

using HeavenStudio.Util;
using UnityEngine.Playables;

namespace HeavenStudio.Games.Scripts_SlotMonster
{
    public class SlotButton : MonoBehaviour
    {
        public bool pressed;
        public Color color; // used to ease between button colors and button flash colors! wow
        public PlayerActionEvent input;
        public bool missed;

        [Header("Components")]
        public Animator anim;
        [SerializeField] SpriteRenderer[] srs;

        private bool flashing;
        private const float FLASH_FRAMES = 4f;
        private int currentFrame;

        private SlotMonster game;

        public void Init(SlotMonster instance)
        {
            game = instance;
            pressed = true;
            color = srs[0].color;
        }

        private void LateUpdate()
        {
            Color newColor = color;
            if (pressed) {
                newColor = Color.LerpUnclamped(color, Color.black, 0.5f);
            } else if (flashing)  {
                float normalized = currentFrame / FLASH_FRAMES;

                Debug.Log("normalized : " + normalized);
                float newR = EasingFunction.Linear(game.buttonFlashColor.r, color.r, normalized);
                float newG = EasingFunction.Linear(game.buttonFlashColor.g, color.g, normalized);
                float newB = EasingFunction.Linear(game.buttonFlashColor.b, color.b, normalized);

                newColor = new Color(newR, newG, newB);
                // Debug.Log("currentFrame / FLASH_FRAMES : " + currentFrame + "/" + FLASH_FRAMES);
                // newColor = Color.LerpUnclamped(color, game.buttonFlashColor, normalized);
                // Debug.Log("color : " + color);
                // Debug.Log("newColor : " + newColor);
            }

            foreach (var sr in srs) {
                sr.color = newColor;
            }
        }

        public void Ready()
        {
            anim.Play("PopUp", 0, 0);
            pressed = false;
            flashing = false;
            missed = false;
        }

        public void Press(bool isMiss)
        {
            anim.DoScaledAnimationAsync("Press", 0.5f);
            pressed = true;
            flashing = false;
            missed = isMiss;
            if (isMiss && input != null) {
                input.Disable();
                input.CleanUp();
            }
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
