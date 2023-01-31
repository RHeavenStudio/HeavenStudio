using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_FlipperFlop
{
    public class FlipperFlopFlipper : MonoBehaviour
    {
        [SerializeField] Animator anim;
        public bool player;
        bool left;
        bool up;

        public void Flip(bool roll, bool hit)
        {
            if (roll)
            {
                if (player && hit) Jukebox.PlayOneShotGame("flipperFlop/roll" + (left ? "L" : "R"));
                up = !up;
            }
            else
            {
                if (player && hit)
                {
                    if (up)
                    {
                        Jukebox.PlayOneShotGame($"flipperFlop/flipB{UnityEngine.Random.Range(1, 3)}");
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame($"flipperFlop/flip{UnityEngine.Random.Range(1, 3)}");
                    }
                }
            }
            left = !left;
        } 
    }
}


