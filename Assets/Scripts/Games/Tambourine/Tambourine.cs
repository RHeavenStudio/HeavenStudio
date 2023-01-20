using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlTambourineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tambourine", "Tambourine \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "812021", false, false, new List<GameAction>()
            {
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_Tambourine;
    public class Tambourine : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator handsAnimator;

        public static Tambourine instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                handsAnimator.Play("Shake", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/shake/{UnityEngine.Random.Range(1, 6)}");
            }
            else if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
            {
                handsAnimator.Play("Smack", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/hit/{UnityEngine.Random.Range(1, 6)}");
            }
        }
    }
}