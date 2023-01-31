using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlFlipperFlopLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("flipperFlop", "Flipper-Flop \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("attentionCompany", "Attention Company!")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; FlipperFlop.AttentionCompany(e.beat); },
                    defaultLength = 4f,
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class FlipperFlop : Minigame
    {

        public static FlipperFlop instance;

        void Awake()
        {
            instance = this;
        }

        public static void AttentionCompany(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("flipperFlop/attention/attention1", beat - 0.25f),
                new MultiSound.Sound("flipperFlop/attention/attention2", beat),
                new MultiSound.Sound("flipperFlop/attention/attention3", beat + 0.5f),
                new MultiSound.Sound("flipperFlop/attention/attention4", beat + 2f),
                new MultiSound.Sound("flipperFlop/attention/attention5", beat + 2.25f),
                new MultiSound.Sound("flipperFlop/attention/attention6", beat + 2.5f),
                new MultiSound.Sound("flipperFlop/attention/attention7", beat + 3),
            }, forcePlay: true);
        }
    }
}
