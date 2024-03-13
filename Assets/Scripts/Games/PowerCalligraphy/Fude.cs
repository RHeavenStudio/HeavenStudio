using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_PowerCalligraphy
{
    public class Fude : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SpriteRenderer handRenderer;
        [SerializeField] SpriteRenderer thumbRenderer;
        [SerializeField] SpriteRenderer stickRenderer;
        [SerializeField] SpriteRenderer tipRenderer;
        [SerializeField] SpriteRenderer ballRenderer;

        [Header("Variables")]
        [SerializeField] float REDRATE_1;
        [SerializeField] float REDRATE_2;

        public float redRate = 0;
        private int red
        {
            get
            {
                if (redRate >= REDRATE_2)
                {
                    return 2;
                }
                else if (redRate >= REDRATE_1)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static Sprite GetSprite(string spriteName) {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Games/PowerCalligraphy/fude");
            return System.Array.Find<Sprite>(sprites, (sprite) => sprite.name.Equals(spriteName));
        }

        public void HaltTurnRed(int frame)
        {
            int stick = 0, tip = 0;
            if (frame==0)
            {
                stick = 1;
            }
            else
            {
                stick = frame + 2;
            }
            tip = frame + 7;
            TurnRed(stick, tip, red);
        }
        public void SweepTurnRed(int frame)
        {
            int stick = 0, tip = 0;
            if (frame<=5)
            {
                tip = frame + 1;
            }
            else
            {
                stick = 2;
                tip = frame%2 + 5;
            }
            TurnRed(stick, tip, red);
        }
        public void TurnRed(int stick, int tip, int red)
        {
            handRenderer.sprite = GetSprite($"hand_{red}");
            thumbRenderer.sprite = GetSprite($"thumb_{red}");
            stickRenderer.sprite = GetSprite($"fude_stick_{stick}_{red}");
            tipRenderer.sprite = GetSprite($"fude_tip_{tip}_{red}");
            ballRenderer.sprite = GetSprite($"fude_ball_{red}");
        }
        public void Tap()
        {
            TurnRed(0, 12, red);
        }
        public void Idle()
        {
            TurnRed(0, 0, red);
        }
    }
}