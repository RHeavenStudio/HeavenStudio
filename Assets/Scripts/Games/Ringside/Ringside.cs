using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRingsideLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("ringside", "Ringside \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "WUTRU3", false, false, new List<GameAction>()
            {
                new GameAction("wubbaDubba", "Wubba Dubba Dubba")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.Question(e.beat); },
                    defaultLength = 1.25f
                },
                new GameAction("thatTrue", "That True?")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.ThatTrue(e.beat); },
                    defaultLength = 0.75f
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class Ringside : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator wrestlerAnim;

        [Header("Variables")]
        public int currentQuestion = 1;


        public static Ringside instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    wrestlerAnim.DoScaledAnimationAsync("Ye", 0.5f);
                    Jukebox.PlayOneShotGame($"ringside/ye{UnityEngine.Random.Range(1, 4)}");
                }
            }
        }

        public void Question(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/wubba{currentQuestion}-1", beat),
                new MultiSound.Sound($"ringside/wubba{currentQuestion}-2", beat + 0.25f),
                new MultiSound.Sound($"ringside/dubba{currentQuestion}-1", beat + 0.5f),
                new MultiSound.Sound($"ringside/dubba{currentQuestion}-2", beat + 0.75f),
                new MultiSound.Sound($"ringside/dubba{currentQuestion}-3", beat + 1f),
                new MultiSound.Sound($"ringside/dubba{currentQuestion}-4", beat + 1.25f),
            }, forcePlay: true);
        }

        public void ThatTrue(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/that{currentQuestion}", beat + 0.25f),
                new MultiSound.Sound($"ringside/true{currentQuestion}", beat + 0.75f),
            }, forcePlay: true);
            ScheduleInput(beat, 1.75f, InputType.STANDARD_DOWN, JustQuestion, MissQuestion, Nothing);
            if (currentQuestion < 3)
            {
                currentQuestion++;
            } 
            else
            {
                currentQuestion = 1;
            }
        }

        public void JustQuestion(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessQuestion();
        }

        public void SuccessQuestion()
        {
            wrestlerAnim.DoScaledAnimationAsync("Ye", 0.5f);
            Jukebox.PlayOneShotGame($"ringside/ye{UnityEngine.Random.Range(1, 4)}");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 0.5f, delegate { Jukebox.PlayOneShotGame("ringside/yeCamera"); }),
            });
        }

        public void MissQuestion(PlayerActionEvent caller)
        {

        }

        public void Nothing(PlayerActionEvent caller){}
    }
}
