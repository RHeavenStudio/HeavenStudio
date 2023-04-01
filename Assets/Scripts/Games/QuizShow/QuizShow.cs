using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbQuizShowLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("quizShow", "Quiz Show", "0058CE", false, false, new List<GameAction>()
            {
                new GameAction("intervalStart", "Start Interval")
                {
                    function = delegate {var e = eventCaller.currentEntity; QuizShow.instance.StartInterval(e.beat, e.length); },
                    defaultLength = 8f,
                    resizable = true
                },
                new GameAction("dPad", "DPad Press")
                {
                    function = delegate {var e = eventCaller.currentEntity; QuizShow.instance.HostPressButton(e.beat, true); },
                    defaultLength = 0.5f
                },
                new GameAction("aButton", "A Button Press")
                {
                    function = delegate {var e = eventCaller.currentEntity; QuizShow.instance.HostPressButton(e.beat, false); },
                    defaultLength = 0.5f
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate {var e = eventCaller.currentEntity; QuizShow.instance.PassTurn(e.beat, e.length); },
                    defaultLength = 1f,
                    resizable = true
                },
                new GameAction("revealAnswer", "Reveal Answer")
                {
                    function = delegate { var e = eventCaller.currentEntity; QuizShow.instance.RevealAnswer(e.beat, e.length); },
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("answerReaction", "Answer Reaction")
                {
                    function = delegate { var e = eventCaller.currentEntity; QuizShow.instance.AnswerReaction(e["audience"], e["jingle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("audience", true, "Audience", "Should the audience make a sound?"),
                        new Param("jingle", false, "Jingle", "Should the quiz show jingle play?")
                    }
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class QuizShow : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator contesteeLeftArmAnim;
        [SerializeField] Animator contesteeRightArmAnim;
        [Header("Properties")]
        bool intervalStarted;
        float intervalStartBeat;
        float playerIntervalStartBeat;
        float playerBeatInterval;
        float beatInterval = 8f;
        static List<float> dpadInputs = new List<float>();
        static List<float> aButtonInputs = new List<float>();
        float pressCount;
        float countToMatch;
        public static QuizShow instance;

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (dpadInputs.Count > 0) dpadInputs.Clear();
                if (aButtonInputs.Count > 0) aButtonInputs.Clear();
            }
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(playerIntervalStartBeat, playerBeatInterval);
                if (normalizedBeat >= 0 && normalizedBeat <= 1)
                {
                    if (PlayerInput.Pressed())
                    {
                        ContesteePressButton(false);
                    }
                    if (PlayerInput.GetAnyDirectionDown())
                    {
                        ContesteePressButton(true);
                    }
                }
            }
        }

        public void HostPressButton(float beat, bool dpad)
        {
            if (!intervalStarted)
            {
                StartInterval(beat, beatInterval);
            }
            if (dpad)
            {
                dpadInputs.Add(beat - intervalStartBeat);
                
                Jukebox.PlayOneShotGame("quizShow/hostDPad");
            }
            else
            {
                aButtonInputs.Add(beat - intervalStartBeat);
                Jukebox.PlayOneShotGame("quizShow/hostA");
            }
        }

        public void StartInterval(float beat, float interval)
        {
            pressCount = 0;
            intervalStartBeat = beat;
            beatInterval = interval;
            intervalStarted = true;
        }

        public void PassTurn(float beat, float length)
        {
            if (dpadInputs.Count == 0 && aButtonInputs.Count == 0) return;
            
            intervalStarted = false;
            countToMatch = dpadInputs.Count + aButtonInputs.Count;
            playerBeatInterval = beatInterval;
            playerIntervalStartBeat = beat + length;
            Jukebox.PlayOneShotGame("quizShow/timerStart");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length + beatInterval, delegate 
                { 
                    Jukebox.PlayOneShotGame("quizShow/timerStop"); 
                }   
            ),
                new BeatAction.Action(beat + length + beatInterval + 0.5f, delegate { Jukebox.PlayOneShotGame("quizShow/timeUp"); })
            });
            foreach (var dpad in dpadInputs)
            {
                ScheduleAutoplayInput(beat, length + dpad, InputType.DIRECTION_DOWN, AutoplayDPad, Nothing, Nothing);
            }
            foreach (var aButton in aButtonInputs)
            {
                ScheduleAutoplayInput(beat, length + aButton, InputType.STANDARD_DOWN, AutoplayAButton, Nothing, Nothing);
            }
            dpadInputs.Clear();
            aButtonInputs.Clear();
        }

        void ContesteePressButton(bool dpad)
        {
            if (dpad)
            {
                Jukebox.PlayOneShotGame("quizShow/contestantDPad");
                contesteeLeftArmAnim.DoScaledAnimationAsync("LeftArmPress", 0.5f);
            }
            else
            {
                Jukebox.PlayOneShotGame("quizShow/contestantA");
                contesteeRightArmAnim.DoScaledAnimationAsync("RightArmHit", 0.5f);
            }
            pressCount++;
        }

        public void RevealAnswer(float beat, float length)
        {
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate { Jukebox.PlayOneShotGame("quizShow/answerReveal");})
            });
        }

        public void AnswerReaction(bool audience, bool jingle)
        {
            if (pressCount == countToMatch)
            {
                GameProfiler.instance.IncreaseScore();
                Jukebox.PlayOneShotGame("quizShow/correct");
                if (audience) Jukebox.PlayOneShotGame("quizShow/audienceCheer");
                if (jingle) Jukebox.PlayOneShotGame("quizShow/correctJingle");
            }
            else
            {
                ScoreMiss();
                Jukebox.PlayOneShotGame("quizShow/incorrect");
                if (audience) Jukebox.PlayOneShotGame("quizShow/audienceSad");
                if (jingle) Jukebox.PlayOneShotGame("quizShow/incorrectJingle");
            }
        }

        void AutoplayAButton(PlayerActionEvent caller, float state)
        {
            ContesteePressButton(false);
        }

        void AutoplayDPad(PlayerActionEvent caller, float state)
        {
            ContesteePressButton(true);
        }

        void Nothing(PlayerActionEvent caller) { }
    }
}


