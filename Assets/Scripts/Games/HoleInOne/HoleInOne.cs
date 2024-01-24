using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlGolfLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("holeInOne", "Hole in One", "6ab99e", false, false, new List<GameAction>()
            {
                new GameAction("testanims", "Test Animation")
                {
                    function = delegate { HoleInOne.instance.DoTestAnim(eventCaller.currentEntity.beat); },
                }
            }//,
            // new List<string>() { "rvl", "normal" },
            // "rvlgolf", "en",
            // new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    /// This class handles the minigame logic.
    /// Minigame inherits directly from MonoBehaviour, and adds Heaven Studio specific methods to override.
    public class HoleInOne : Minigame
    {
        public Animator Monkey;

        public static HoleInOne instance;

        // Awake is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        void Update()
        {

        }

        public void DoTestAnim(double beat)
        {
            SoundByte.PlayOneShotGame("holeInOne/whale");

            // BeatAction.New(instance, new List<BeatAction.Action>()
            // {
            // new BeatAction.Action(beat,     delegate { Monkey.DoScaledAnimationAsync("MonkeySpin", 0.5f);}),
            // });

        }
    }
}