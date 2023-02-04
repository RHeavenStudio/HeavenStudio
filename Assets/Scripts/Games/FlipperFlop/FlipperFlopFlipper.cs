using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_FlipperFlop
{
    public class FlipperFlopFlipper : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] Animator faceAnim;
        public bool player;
        bool left;
        bool up;

        public void Flip(bool roll, bool hit, bool barely = false)
        {
            if (roll)
            {
                if (player && hit && !barely) Jukebox.PlayOneShotGame("flipperFlop/roll" + (left ? "L" : "R"));
                else if (barely) Jukebox.PlayOneShotGame("flipperFlop/tink");
                up = !up;
            }
            else
            {
                if (player && hit)
                {
                    if (up && !barely)
                    {
                        Jukebox.PlayOneShotGame($"flipperFlop/flipB{UnityEngine.Random.Range(1, 3)}");
                    }
                    else if (!barely)
                    {
                        Jukebox.PlayOneShotGame($"flipperFlop/flip{UnityEngine.Random.Range(1, 3)}");

                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("flipperFlop/tink");
                    }
                }
                if (left)
                {
                    anim.DoScaledAnimationAsync("FlopLeft", 0.5f);
                }
                else
                {
                    anim.DoScaledAnimationAsync("FlopRight", 0.5f);
                }
            }
            left = !left;
        } 

        public void Bop()
        {
            anim.DoScaledAnimationAsync("FlipperBop", 0.5f);
        }
    }
}


