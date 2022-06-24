using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTramLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tram&Pauline", "Tram&Pauline \n<color=#eb5454>[WIP don't use]</color>", "000000", false, false, new List<GameAction>()
            {
                new GameAction("curtains",          delegate { TramAndPauline.instance.Curtains(eventCaller.currentEntity.beat); }, 0.5f),
                new GameAction("SFX",          delegate { var e = eventCaller.currentEntity; TramAndPauline.instance.SFX(e.beat,  e.toggle);  }, 2.5f, false, new List<Param>()
                {
                   new Param("type", TramAndPauline.SoundEffects.Henge, "calls", "the sound effects to choose from"),
                }),

            });
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_TramAndPauline;

    public class TramAndPauline : Minigame
    {
        public enum CurtainState
        {
            Raised,
            Lower
        }

        public enum SoundEffects
        {
            Henge, //Shapeshift
            Henshin, //Transform
            Jump,
            Seino //One Two Three Go

        }
        public static TramAndPauline instance;

        [Header("Animators")]
        public Animator RaiseCurtains;
        public Animator LowerCurtains;


        private void Awake()
        {
            instance = this;
        }

        public void Curtains(float beat)
        {

        }

        public void SFX(float beat, bool playSound)
        {
            playSound = false;
            var sound = new[]
            {
                new MultiSound.Sound("tram&Pauline/trampoline_unused_henge", beat),
                new MultiSound.Sound("tram&Pauline/trampoline_unused_henshin", beat + 1f),
                new MultiSound.Sound("tram&Pauline/trampoline_unused_jump", beat + 2f),
                new MultiSound.Sound("tram&Pauline/trampoline_unused_senio", beat + 3f)
            };
        }
    }
}
