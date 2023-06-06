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
            return new Minigame("quizShow", "Quiz Show", "c96efa", false, false, new List<GameAction>()
            {
                new GameAction("intervalStart", "Start Interval")
                {
                    function = delegate {var e = eventCaller.currentEntity; QuizShow.instance.StartInterval(e.beat, e.length); },
                    defaultLength = 8f,
                    resizable = true
                },
                new GameAction("prepare", "Prepare Host Hands")
                {
                    function = delegate { QuizShow.instance.HostPrepareHands(); }
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
                new GameAction("randomPresses", "Random Presses")
                {
                    function = delegate { var e = eventCaller.currentEntity; QuizShow.instance.RandomPress(e.beat, e.length, e["min"], e["max"], e["random"], e["con"]); },
                    parameters = new List<Param>()
                    {
                        new Param("min", new EntityTypes.Integer(0, 666, 0), "Minimum", "The minimum number of presses this block will do."),
                        new Param("max", new EntityTypes.Integer(0, 666, 1), "Maximum", "The maximum number of presses this block will do."),
                        new Param("random", QuizShow.WhichButtonRandom.Random, "Which Buttons", "Which buttons will be pressed randomly?"),
                        new Param("con", true, "Consecutive Presses", "Will the presses be consecutive? As in if the first press doesn't trigger, the ones proceeding will not either.")
                    },
                    resizable = true
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate {var e = eventCaller.currentEntity; QuizShow.instance.PassTurn(e.beat, e.length, e["sound"], e["con"], e["visual"], e["audio"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("sound", true, "Play Time-Up Sound?", "Should the Time-Up sound play at the end of the interval?"),
                        new Param("con", false, "Consecutive", "Disables everything that happens at the end of the interval if ticked on."),
                        new Param("visual", true, "Stopwatch (Visual)", "Should the stopwatch visually appear?"),
                        new Param("audio", QuizShow.ClockAudio.Both, "Stopwatch (Audio)", "Should the sounds of the stopwatch play?")
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
                    function = delegate { var e = eventCaller.currentEntity; QuizShow.instance.AnswerReaction(e["audience"], e["jingle"], e["reveal"]); },
                    parameters = new List<Param>()
                    {
                        new Param("audience", true, "Audience", "Should the audience make a sound?"),
                        new Param("jingle", false, "Jingle", "Should the quiz show jingle play?"),
                        new Param("reveal", false, "Reveal Answer (Instant)", "Should the answer be revealed when this block starts?")
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
            },
            new List<string>() {"agb", "repeat"},
            "agbquiz", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class QuizShow : Minigame
    {
        public enum ClockAudio
        {
            Both,
            Start,
            End,
            Neither
        }
        public enum HeadStage
        {
            Stage0 = 0,
            Stage1 = 1,
            Stage2 = 2,
            Stage3 = 3,
            Stage4 = 4,
        }
        public enum WhichButtonRandom
        {
            Random = 0,
            DpadOnly = 1,
            AOnly = 2,
            AlternatingDpad = 3,
            AlternatingA = 4
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
        [SerializeField] Animator signAnim;
        [SerializeField] Transform timerTransform;
        [SerializeField] GameObject stopWatch;
        [SerializeField] GameObject blackOut;
        [SerializeField] SpriteRenderer firstDigitSr;
        [SerializeField] SpriteRenderer secondDigitSr;
        [SerializeField] SpriteRenderer hostFirstDigitSr;
        [SerializeField] SpriteRenderer hostSecondDigitSr;
        [SerializeField] SpriteRenderer contCounter;
        [SerializeField] SpriteRenderer hostCounter;
        [SerializeField] ParticleSystem contExplosion;
        [SerializeField] ParticleSystem hostExplosion;
        [SerializeField] ParticleSystem signExplosion;
        [Header("Properties")]
        [SerializeField] List<Sprite> contestantNumberSprites = new List<Sprite>();
        [SerializeField] List<Sprite> hostNumberSprites = new List<Sprite>();
        [SerializeField] Sprite explodedCounter;
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
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
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

        public void RandomPress(float beat, float length, int min, int max, int whichButtons, bool consecutive)
        {
            if (min > max) return;
            int pressAmount = UnityEngine.Random.Range(min, max + 1);
            if (pressAmount < 1) return;
            List<BeatAction.Action> buttonEvents = new List<BeatAction.Action>();
            if (consecutive)
            {
                for (int i = 0; i < pressAmount; i++)
                {
                    bool dpad = UnityEngine.Random.Range(0, 2) == 1;
                    switch (whichButtons)
                    {
                        case (int)WhichButtonRandom.Random:
                            break;
                        case (int)WhichButtonRandom.DpadOnly:
                            dpad = true;
                            break;
                        case (int)WhichButtonRandom.AOnly:
                            dpad = false;
                            break;
                        case (int)WhichButtonRandom.AlternatingDpad:
                            dpad = i % 2 == 0;
                            break;
                        case (int)WhichButtonRandom.AlternatingA:
                            dpad = i % 2 != 0;
                            break;
                    }
                    float spawnBeat = beat + i * length;
                    buttonEvents.Add(new BeatAction.Action(spawnBeat, delegate { HostPressButton(spawnBeat, dpad); }));
                }
            }
            else
            {
                for (int i = 0; i < max; i++)
                {
                    if (pressAmount == 0) break;
                    if (UnityEngine.Random.Range(0, 2) == 1 && Mathf.Abs(i - max) != pressAmount) continue;
                    bool dpad = UnityEngine.Random.Range(0, 2) == 1;
                    switch (whichButtons)
                    {
                        case (int)WhichButtonRandom.Random:
                            break;
                        case (int)WhichButtonRandom.DpadOnly:
                            dpad = true;
                            break;
                        case (int)WhichButtonRandom.AOnly:
                            dpad = false;
                            break;
                        case (int)WhichButtonRandom.AlternatingDpad:
                            dpad = i % 2 == 0;
                            break;
                        case (int)WhichButtonRandom.AlternatingA:
                            dpad = i % 2 != 0;
                            break;
                    }
                    float spawnBeat = beat + i * length;
                    buttonEvents.Add(new BeatAction.Action(spawnBeat, delegate { HostPressButton(spawnBeat, dpad); }));
                    pressAmount--;
                }
            }

            BeatAction.New(instance.gameObject, buttonEvents);
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

        public void HostPrepareHands()
        {
            instance.hostLeftArmAnim.DoScaledAnimationAsync("HostLeftPrepare", 0.5f);
            instance.hostRightArmAnim.DoScaledAnimationAsync("HostPrepare", 0.5f);
        }

        public void StartInterval(float beat, float interval)
        {
            if (!intervalStarted)
            {
                if (shouldPrepareArms)
                {
                    hostLeftArmAnim.DoNormalizedAnimation("HostLeftPrepare", 1);
                    hostRightArmAnim.DoNormalizedAnimation("HostPrepare", 1);
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

        public void PassTurn(float beat, float length, bool timeUpSound, bool consecutive, bool visualClock, int audioClock)
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
            if (visualClock) stopWatch.SetActive(true);
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
            float timeUpBeat = 0f;
            if (audioClock == (int)ClockAudio.Both || audioClock == (int)ClockAudio.Start) 
            {
                Jukebox.PlayOneShotGame("quizShow/timerStart");
                timeUpBeat = 0.5f;
            }
            if (audioClock == (int)ClockAudio.End) timeUpBeat = 0.5f;
            
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length + beatInterval, delegate 
                { 
                    if (!consecutive) 
                    {
                        if (audioClock == (int)ClockAudio.Both || audioClock == (int)ClockAudio.End) Jukebox.PlayOneShotGame("quizShow/timerStop"); 
                        contesteeLeftArmAnim.DoScaledAnimationAsync("LeftRest", 0.5f);
                        contesteeRightArmAnim.DoScaledAnimationAsync("RightRest", 0.5f);
                        shouldPrepareArms = true;
                        stopWatch.SetActive(false);
                    }
                }   
            ),
                new BeatAction.Action(beat + length + beatInterval + timeUpBeat, delegate { if (timeUpSound && !consecutive) Jukebox.PlayOneShotGame("quizShow/timeUp"); })
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
                    ForceExplode((int)ShouldExplode.Contestant);
                    break;
                case 120:
                    ForceExplode((int)ShouldExplode.Host);
                    break;
                case 150:
                    ForceExplode((int)ShouldExplode.Sign);
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
                    contCounter.sprite = explodedCounter;
                    contExploded = true;
                    contExplosion.Play();
                    break;
                case (int)ShouldExplode.Host:
                    if (hostExploded) return;
                    Jukebox.PlayOneShotGame("quizShow/hostExplode");
                    hostFirstDigitSr.color = new Color(1, 1, 1, 0);
                    hostSecondDigitSr.color = new Color(1, 1, 1, 0);
                    hostCounter.sprite = explodedCounter;
                    hostExploded = true;
                    hostExplosion.Play();
                    break;
                case (int)ShouldExplode.Sign:
                    if (signExploded) return;
                    Jukebox.PlayOneShotGame("quizShow/signExplode");
                    signExploded = true;
                    signExplosion.Play();
                    signAnim.Play("Exploded", 0, 0);
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

        public void AnswerReaction(bool audience, bool jingle, bool revealAnswer)
        {
            //In the future make this a prefunction that makes skill stars compatible
            blackOut.SetActive(false);
            if (revealAnswer)
            {
                Jukebox.PlayOneShotGame("quizShow/answerReveal");
                hostFirstDigitSr.sprite = hostNumberSprites[GetSpecificDigit(countToMatch, 1)];
                hostSecondDigitSr.sprite = hostNumberSprites[GetSpecificDigit(countToMatch, 2)];
            }
            if (pressCount == countToMatch)
            {
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


