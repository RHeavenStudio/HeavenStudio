using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System.Diagnostics.CodeAnalysis;

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
                    function = delegate {var e = eventCaller.currentEntity; QuizShow.instance.PassTurn(e.beat, e.length, e["sound"], e["con"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("sound", true, "Play Time-Up Sound?", "Should the Time-Up sound play at the end of the interval?"),
                        new Param("con", false, "Consecutive", "Disables everything that happens at the end of the interval if ticked on."),
                    }
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
                },
                new GameAction("countMod", "Count Modifier")
                {
                    function = delegate { QuizShow.instance.CountModifier(eventCaller.currentEntity["value"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("value", true, "Should Reset Count?", "Will the contestant's counter reset to 0 each time it hits 100 instead of exploding?")
                    }
                },
                new GameAction("forceExplode", "Force Explode")
                {
                    function = delegate { QuizShow.instance.ForceExplode(eventCaller.currentEntity["value"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("value", QuizShow.ShouldExplode.Contestant, "What To Explode", "What will explode?")
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
            Stage3 = 3,
            Stage4 = 4,
        }
        public enum ShouldExplode
        {
            Contestant = 0,
            Host = 1,
            Sign = 2
        }
        [Header("Components")]
        [SerializeField] Animator contesteeLeftArmAnim;
        [SerializeField] Animator contesteeRightArmAnim;
        [SerializeField] Animator contesteeHead;
        [SerializeField] Animator hostLeftArmAnim;
        [SerializeField] Animator hostRightArmAnim;
        [SerializeField] Animator hostHead;
        [SerializeField] Transform timerTransform;
        [SerializeField] GameObject stopWatch;
        [SerializeField] GameObject blackOut;
        [SerializeField] SpriteRenderer firstDigitSr;
        [SerializeField] SpriteRenderer secondDigitSr;
        [SerializeField] SpriteRenderer hostFirstDigitSr;
        [SerializeField] SpriteRenderer hostSecondDigitSr;
        [Header("Properties")]
        [SerializeField] List<Sprite> contestantNumberSprites = new List<Sprite>();
        [SerializeField] List<Sprite> hostNumberSprites = new List<Sprite>();
        bool intervalStarted;
        bool shouldResetCount;
        bool doingConsectiveIntervals;
        float intervalStartBeat;
        float playerIntervalStartBeat;
        float playerBeatInterval;
        float beatInterval = 8f;
        int currentStage;
        bool shouldPrepareArms = true;
        bool contExploded;
        bool hostExploded;
        bool signExploded;
        struct QueuedInput 
        {
            public float beat;
            public bool dpad;
        }
        static List<QueuedInput> queuedInputs = new List<QueuedInput>();
        int pressCount;
        int countToMatch;
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

        public void CountModifier(bool shouldReset)
        {
            shouldResetCount = shouldReset;
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
                hostHead.Play("HostIdleHead", -1, 0);
            }
            else 
            {
                hostHead.DoScaledAnimationAsync("HostStage" + currentStage.ToString(), 0.5f);
            }
            Jukebox.PlayOneShotGame( dpad ? "quizShow/hostDPad" : "quizShow/hostA");
            if (dpad)
            {
                hostRightArmAnim.DoScaledAnimationAsync("HostRightHit", 0.5f);
            }
            else
            {
                hostLeftArmAnim.DoScaledAnimationAsync("HostLeftHit", 0.5f);
            }
            queuedInputs.Add(new QueuedInput 
            {
                beat = beat - intervalStartBeat,
                dpad = dpad,
            });
        }

        public void StartInterval(float beat, float interval)
        {
            if (!intervalStarted)
            {
                if (shouldPrepareArms)
                {
                    hostLeftArmAnim.DoScaledAnimationAsync("HostLeftPrepare", 0.5f);
                    hostRightArmAnim.DoScaledAnimationAsync("HostPrepare", 0.5f);
                    contesteeHead.Play("ContesteeHeadIdle", 0, 0);
                }
                if (!doingConsectiveIntervals) pressCount = 0;
                firstDigitSr.sprite = contestantNumberSprites[0];
                secondDigitSr.sprite = contestantNumberSprites[0];
                hostFirstDigitSr.sprite = hostNumberSprites[10];
                hostSecondDigitSr.sprite = hostNumberSprites[10];
            }
            intervalStartBeat = beat;
            beatInterval = interval;
            intervalStarted = true;
        }

        public void PassTurn(float beat, float length, bool timeUpSound, bool consecutive)
        {
            if (queuedInputs.Count == 0) return;
            if (shouldPrepareArms) 
            {
                contesteeLeftArmAnim.DoScaledAnimationAsync("LeftPrepare", 0.5f);
                contesteeRightArmAnim.DoScaledAnimationAsync("RIghtPrepare", 0.5f);
            }
            if (!consecutive)
            {
                hostLeftArmAnim.DoScaledAnimationAsync("HostLeftRest", 0.5f);
                hostRightArmAnim.DoScaledAnimationAsync("HostRightRest", 0.5f);
            }
            shouldPrepareArms = false;
            stopWatch.SetActive(true);
            intervalStarted = false;
            if (doingConsectiveIntervals)
            {
                countToMatch += queuedInputs.Count;
            }
            else
            {
                countToMatch = queuedInputs.Count;
            }
            int hundredLoops = Mathf.FloorToInt(countToMatch / 100);
            countToMatch -= hundredLoops * 100;
            doingConsectiveIntervals = consecutive;
            playerBeatInterval = beatInterval;
            playerIntervalStartBeat = beat + length;
            Jukebox.PlayOneShotGame("quizShow/timerStart");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length + beatInterval, delegate 
                { 
                    if (!consecutive) 
                    {
                        Jukebox.PlayOneShotGame("quizShow/timerStop"); 
                        contesteeLeftArmAnim.DoScaledAnimationAsync("LeftRest", 0.5f);
                        contesteeRightArmAnim.DoScaledAnimationAsync("RightRest", 0.5f);
                        shouldPrepareArms = true;
                        stopWatch.SetActive(false);
                    }
                }   
            ),
                new BeatAction.Action(beat + length + beatInterval + 0.5f, delegate { if (timeUpSound && !consecutive) Jukebox.PlayOneShotGame("quizShow/timeUp"); })
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
                if (currentStage != 4) contesteeHead.DoScaledAnimationAsync("ContesteeHeadStage" + currentStage.ToString(), 0.5f);
                else
                {
                    contesteeHead.DoScaledAnimationAsync("ContesteeHeadStage3", 0.5f);
                }
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
            if (shouldResetCount && pressCount > 99) pressCount = 0;
            switch (pressCount)
            {
                case int x when x < 100:
                    firstDigitSr.sprite = contestantNumberSprites[GetSpecificDigit(pressCount, 1)];
                    secondDigitSr.sprite = contestantNumberSprites[GetSpecificDigit(pressCount, 2)];
                    break;
                case 100:
                    if (contExploded) return;
                    Jukebox.PlayOneShotGame("quizShow/contestantExplode");
                    firstDigitSr.color = new Color(1, 1, 1, 0);
                    secondDigitSr.color = new Color(1, 1, 1, 0);
                    contExploded = true;
                    break;
                case 120:
                    if (hostExploded) return;
                    Jukebox.PlayOneShotGame("quizShow/hostExplode");
                    hostFirstDigitSr.color = new Color(1, 1, 1, 0);
                    hostSecondDigitSr.color = new Color(1, 1, 1, 0);
                    hostExploded = true;
                    break;
                case 150:
                    if (signExploded) return;
                    Jukebox.PlayOneShotGame("quizShow/signExplode");
                    signExploded = true;
                    break;
            }
        }

        public void ForceExplode(int whoToExplode)
        {
            switch (whoToExplode)
            {
                case (int)ShouldExplode.Contestant:
                    if (contExploded) return;
                    Jukebox.PlayOneShotGame("quizShow/contestantExplode");
                    firstDigitSr.color = new Color(1, 1, 1, 0);
                    secondDigitSr.color = new Color(1, 1, 1, 0);
                    contExploded = true;
                    break;
                case (int)ShouldExplode.Host:
                    if (hostExploded) return;
                    Jukebox.PlayOneShotGame("quizShow/hostExplode");
                    hostFirstDigitSr.color = new Color(1, 1, 1, 0);
                    hostSecondDigitSr.color = new Color(1, 1, 1, 0);
                    hostExploded = true;
                    break;
                case (int)ShouldExplode.Sign:
                    if (signExploded) return;
                    Jukebox.PlayOneShotGame("quizShow/signExplode");
                    signExploded = true;
                    break;
            }
        }

        public void RevealAnswer(float beat, float length)
        {
            blackOut.SetActive(true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate 
                { 
                    Jukebox.PlayOneShotGame("quizShow/answerReveal");
                    hostFirstDigitSr.sprite = hostNumberSprites[GetSpecificDigit(countToMatch, 1)];
                    hostSecondDigitSr.sprite = hostNumberSprites[GetSpecificDigit(countToMatch, 2)];
                })
            });
        }

        public void AnswerReaction(bool audience, bool jingle)
        {
            blackOut.SetActive(false);
            if (pressCount == countToMatch)
            {
                GameProfiler.instance.IncreaseScore();
                Jukebox.PlayOneShotGame("quizShow/correct");
                contesteeHead.Play("ContesteeSmile", -1, 0);
                hostHead.Play("HostSmile", -1, 0);
                if (audience) Jukebox.PlayOneShotGame("quizShow/audienceCheer");
                if (jingle) Jukebox.PlayOneShotGame("quizShow/correctJingle");
            }
            else
            {
                ScoreMiss();
                Jukebox.PlayOneShotGame("quizShow/incorrect");
                contesteeHead.Play("ContesteeSad", -1, 0);
                hostHead.Play("HostSad", -1, 0);
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

        int GetSpecificDigit(int num, int nth)
        {
            return (num / (int)Mathf.Pow(10, nth - 1)) % 10;
        }
    }
}


