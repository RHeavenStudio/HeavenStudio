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
            return new Minigame("tramAndPauline", "Tram & Pauline", "adb5e7", false, false, new List<GameAction>()
            {
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { TramAndPauline.instance.Prepare(eventCaller.currentEntity["who"]); },
                    parameters = new List<Param>()
                    {
                        new Param("who", TramAndPauline.TramOrPauline.Pauline, "Who Prepares?")
                    }
                },
                new GameAction("pauline", "Pauline")
                {
                    function = delegate { TramAndPauline.instance.Jump(eventCaller.currentEntity.beat, TramAndPauline.TramOrPauline.Pauline); },
                    defaultLength = 2f
                },
                new GameAction("tram", "Tram")
                {
                    function = delegate { TramAndPauline.instance.Jump(eventCaller.currentEntity.beat, TramAndPauline.TramOrPauline.Tram); },
                    defaultLength = 2f
                },
                new GameAction("shape", "Change Transformation")
                {
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("pauline", false, "Pauline is Human?"),
                        new Param("tram", false, "Tram is Human?")
                    }
                },
                new GameAction("curtains", "Curtains")
                {
                    defaultLength = 4f,
                    resizable = true
                }
            }
            );
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_TramAndPauline;
    public class TramAndPauline : Minigame
    {
        public enum TramOrPauline
        {
            Pauline = 0,
            Tram = 1,
            Both = 2
        }

        public static TramAndPauline instance;

        [Header("Components")]
        [SerializeField] private AgbAnimalKid tram;
        [SerializeField] private AgbAnimalKid pauline;

        private void Awake()
        {
            instance = this;
        }

        public void Prepare(TramOrPauline who)
        {

        }

        public void Jump(double beat, TramOrPauline who)
        {
            switch (who)
            {
                case TramOrPauline.Pauline:
                    PaulineJump(beat);
                    break;
                case TramOrPauline.Tram:
                    TramJump(beat);
                    break;
                case TramOrPauline.Both:
                    PaulineJump(beat);
                    TramJump(beat);
                    break;
            }
        }

        private void TramJump(double beat)
        {
            SoundByte.PlayOneShotGame("tramAndPauline/jump" + UnityEngine.Random.Range(1, 3));
            ScheduleInput(beat, 1, InputType.DIRECTION_DOWN, TramJust, Empty, Empty);
        }

        private void PaulineJump(double beat)
        {
            SoundByte.PlayOneShotGame("tramAndPauline/jump" + UnityEngine.Random.Range(1, 3));
            ScheduleInput(beat, 1, InputType.STANDARD_DOWN, PaulineJust, Empty, Empty);
        }

        private void TramJust(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("tramAndPauline/transformTram");
        }

        private void PaulineJust(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
                return;
            }
            SoundByte.PlayOneShotGame("tramAndPauline/transformPauline");
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}
