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
                },
                new GameAction("changeStage", "Change Expression Stage") 
                {
                    function = delegate {QuizShow.instance.ChangeStage(eventCaller.currentEntity["value"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>() 
                    {
                        new Param("value", QuizShow.HeadStage.Stage1, "Stage", "What's the current stage of the expressions?")
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
        public enum HeadStage
        {
            Stage1 = 1,
            Stage2 = 2,
            Stage3 = 3
        }
        [Header("Components")]
        [SerializeField] Animator contesteeLeftArmAnim;
        [SerializeField] Animator contesteeRightArmAnim;
        [SerializeField] Animator contesteeHead;
        [SerializeField] Transform timerTransform;
        [SerializeField] GameObject stopWatch;
        [Header("Properties")]
        bool intervalStarted;
        float intervalStartBeat;
        float playerIntervalStartBeat;
        float playerBeatInterval;
        float beatInterval = 8f;
        int currentStage;
        struct QueuedInput 
        {
            public float beat;
            public bool dpad;
        }
        static List<QueuedInput> queuedInputs = new List<QueuedInput>();
        float pressCount;
        float countToMatch;
        public static QuizShow instance;

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
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
                    timerTransform.rotation = Quaternion.Euler(0, 0, normalizedBeat * -360);
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
        
        public void ChangeStage(int stage) 
        {
            currentStage = stage;
        }

        public void HostPressButton(float beat, bool dpad)
        {
            if (!intervalStarted)
            {
                StartInterval(beat, beatInterval);
            }
            if (currentStage == 0) 
            {
                contesteeHead.Play("ContesteeHeadIdle", -1, 0);
            }
            else 
            {
                contesteeHead.DoScaledAnimationAsync("ContesteeHeadStage" + currentStage.ToString(), 0.5f);
            }
            Jukebox.PlayOneShotGame( dpad ? "quizShow/hostDPad" : "quizShow/hostA");
            queuedInputs.Add(new QueuedInput 
            {
                beat = beat - intervalStartBeat,
                dpad = dpad,
            });
        }

        public void StartInterval(float beat, float interval)
        {
            contesteeHead.Play("ContesteeHeadIdle", 0, 0);
            pressCount = 0;
            intervalStartBeat = beat;
            beatInterval = interval;
            intervalStarted = true;
        }

        public void PassTurn(float beat, float length)
        {
            if (queuedInputs.Count == 0) return;
            contesteeLeftArmAnim.DoScaledAnimationAsync("LeftPrepare", 0.5f);
            contesteeRightArmAnim.DoScaledAnimationAsync("RIghtPrepare", 0.5f);
            stopWatch.SetActive(true);
            intervalStarted = false;
            countToMatch = queuedInputs.Count;
            playerBeatInterval = beatInterval;
            playerIntervalStartBeat = beat + length;
            Jukebox.PlayOneShotGame("quizShow/timerStart");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length + beatInterval, delegate 
                { 
                    Jukebox.PlayOneShotGame("quizShow/timerStop"); 
                    contesteeLeftArmAnim.DoScaledAnimationAsync("LeftRest", 0.5f);
                    contesteeRightArmAnim.DoScaledAnimationAsync("RightRest", 0.5f);
                    stopWatch.SetActive(false);
                }   
            ),
                new BeatAction.Action(beat + length + beatInterval + 0.5f, delegate { Jukebox.PlayOneShotGame("quizShow/timeUp"); })
            });
            foreach (var input in queuedInputs) 
            {
                if (input.dpad) 
                {
                    ScheduleAutoplayInput(beat, length + input.beat, InputType.DIRECTION_DOWN, AutoplayDPad, Nothing, Nothing);
                }
                else 
                {
                    ScheduleAutoplayInput(beat, length + input.beat, InputType.STANDARD_DOWN, AutoplayAButton, Nothing, Nothing);
                }
            }
            queuedInputs.Clear();
        }

        void ContesteePressButton(bool dpad)
        {
            if (currentStage == 0) 
            {
                contesteeHead.Play("ContesteeHeadIdle", -1, 0);
            }
            else 
            {
                contesteeHead.DoScaledAnimationAsync("ContesteeHeadStage" + currentStage.ToString(), 0.5f);
            }
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
            switch (pressCount) 
            {
                case 100:
                    Jukebox.PlayOneShotGame("quizShow/contestantExplode");
                    break;
                case 120:
                    Jukebox.PlayOneShotGame("quizShow/hostExplode");
                    break;
                case 150:
                    Jukebox.PlayOneShotGame("quizShow/signExplode");
                    break;
            }
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
                contesteeHead.Play("ContesteeSmile", -1, 0);
                if (audience) Jukebox.PlayOneShotGame("quizShow/audienceCheer");
                if (jingle) Jukebox.PlayOneShotGame("quizShow/correctJingle");
            }
            else
            {
                ScoreMiss();
                Jukebox.PlayOneShotGame("quizShow/incorrect");
                contesteeHead.Play("ContesteeSad", -1, 0);
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


