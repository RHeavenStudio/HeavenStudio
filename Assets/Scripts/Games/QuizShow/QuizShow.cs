using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
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
                    preFunction = delegate { var e = eventCaller.currentEntity; QuizShow.PreInterval(e.beat, e.length, 
                        e["auto"], e["sound"], e["con"], e["visual"], e["audio"]); },
                    defaultLength = 7f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "sound", "con", "visual", "audio" })
                        }),
                        new Param("sound", true, "Play Time-Up Sound?", "Should the Time-Up sound play at the end of the interval?"),
                        new Param("con", false, "Consecutive", "Disables everything that happens at the end of the interval if ticked on."),
                        new Param("visual", true, "Stopwatch (Visual)", "Should the stopwatch visually appear?"),
                        new Param("audio", QuizShow.ClockAudio.Both, "Stopwatch (Audio)", "Should the sounds of the stopwatch play?")
                    }
                },
                new GameAction("prepare", "Prepare Host Hands")
                {
                    function = delegate { QuizShow.instance.HostPrepareHands(); }
                },
                new GameAction("dPad", "DPad Press")
                {
                    defaultLength = 0.5f
                },
                new GameAction("aButton", "A Button Press")
                {
                    defaultLength = 0.5f
                },
                new GameAction("randomPresses", "Random Presses")
                {
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
                    preFunction = delegate {var e = eventCaller.currentEntity; QuizShow.PrePassTurn(e.beat, e["sound"], e["con"], e["visual"], e["audio"]); },
                    parameters = new List<Param>()
                    {
                        new Param("sound", true, "Play Time-Up Sound?", "Should the Time-Up sound play at the end of the interval?"),
                        new Param("con", false, "Consecutive", "Disables everything that happens at the end of the interval if ticked on."),
                        new Param("visual", true, "Stopwatch (Visual)", "Should the stopwatch visually appear?"),
                        new Param("audio", QuizShow.ClockAudio.Both, "Stopwatch (Audio)", "Should the sounds of the stopwatch play?")
                    },
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
    using Jukebox;
    using Scripts_QuizShow;

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
        [SerializeField] QSTimer stopWatchRef;
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
        bool shouldResetCount;
        bool doingConsectiveIntervals;
        int currentStage;
        bool shouldPrepareArms = true;
        bool contExploded;
        bool hostExploded;
        bool signExploded;
        int pressCount;
        int countToMatch;
        private double playerStartBeat = -1;
        private float playerLength;
        public static QuizShow instance;
        private struct RandomPress
        {
            public List<RiqEntity> randomPresses;
            public double beat;
        }

        private List<RandomPress> randomPresses = new();

        const int IALeft = 0;
        const int IARight = 1;

        protected static bool IA_PadLeft(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_TouchLeft(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Left, out dt);
        }

        protected static bool IA_PadRight(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt);
        }
        protected static bool IA_TouchRight(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Right, out dt);
        }

        public static PlayerInput.InputAction InputAction_Left =
            new("AgbQuizLeft", new int[] { IALeft, IALeft, IALeft },
            IA_PadLeft, IA_TouchLeft, IA_BatonBasicPress);

        // Baton Style only has one button
        public static PlayerInput.InputAction InputAction_Right =
            new("AgbQuizRight", new int[] { IARight, IARight, IAEmptyCat },
            IA_PadRight, IA_TouchRight, IA_Empty);

        void Awake()
        {
            instance = this;
        }

        public override void OnPlay(double beat)
        {
            var allRandomEvents = EventCaller.GetAllInGameManagerList("quizShow", new string[] { "randomPresses" });
            foreach (var randomEvent in allRandomEvents)
            {
                randomPresses.Add(new RandomPress()
                {
                    beat = randomEvent.beat,
                    randomPresses = GetRandomPress(randomEvent.beat, randomEvent.length,
                    randomEvent["min"], randomEvent["max"], randomEvent["random"], randomEvent["con"])
                });
            }
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(playerStartBeat, playerLength);
                if (normalizedBeat >= 0f && normalizedBeat <= 1f)
                {
                    if (PlayerInput.GetIsAction(InputAction_Right))
                    {
                        ContesteePressButton(false);
                    }
                    if (PlayerInput.GetIsAction(InputAction_Left))
                    {
                        ContesteePressButton(true);
                    }
                }
            }

            if (passedTurns.Count > 0)
            {
                foreach (var pass in passedTurns)
                {
                    PassTurnStandalone(pass.beat, pass.timeUpSound, pass.consecutive, pass.visualClock, pass.audioClock);
                }
                passedTurns.Clear();
            }
        }

        private List<RiqEntity> GetInputsBetweenBeat(double beat, double endBeat)
        {
            List<RiqEntity> allEvents = new();
            List<RiqEntity> nonRandoms = EventCaller.GetAllInGameManagerList("quizShow", new string[] { "dPad", "aButton" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
            List<RiqEntity> randoms = new();
            foreach (var rand in randomPresses)
            {
                if (rand.beat >= beat && rand.beat < endBeat)
                {
                    randoms.AddRange(rand.randomPresses);
                }
            }
            allEvents.AddRange(nonRandoms);
            allEvents.AddRange(randoms);
            return allEvents;
        }

        private RiqEntity GetLastIntervalBeforeBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("quizShow", new string[] { "intervalStart" }).FindLast(x => x.beat <= beat);
        }

        public void CountModifier(bool shouldReset)
        {
            shouldResetCount = shouldReset;
        }
        
        public void ChangeStage(int stage) 
        {
            currentStage = stage;
        }

        private List<RiqEntity> GetRandomPress(double beat, float length, int min, int max, int whichButtons, bool consecutive)
        {
            if (min > max) return new();
            int pressAmount = UnityEngine.Random.Range(min, max + 1);
            if (pressAmount < 1) return new();
            List<RiqEntity> buttonEntities = new();
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
                    double spawnBeat = beat + i * length;
                    buttonEntities.Add(new RiqEntity(new RiqEntityData()
                    {
                        beat = spawnBeat,
                        datamodel = dpad ? "dPad" : "aButton"
                    }));
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
                    double spawnBeat = beat + i * length;
                    buttonEntities.Add(new RiqEntity(new RiqEntityData()
                    {
                        beat = spawnBeat,
                        datamodel = dpad ? "dPad" : "aButton"
                    }));
                    pressAmount--;
                }
            }
            return buttonEntities;
        }

        public void HostPressButton(double beat, bool dpad)
        {
            if (currentStage == 0) 
            {
                contesteeHead.Play("ContesteeHeadIdle", -1, 0);
                hostHead.Play("HostIdleHead", -1, 0);
            }
            else 
            {
                hostHead.DoScaledAnimationAsync("HostStage" + currentStage.ToString(), 0.5f);
            }
            SoundByte.PlayOneShotGame(dpad ? "quizShow/hostDPad" : "quizShow/hostA");
            if (dpad)
            {
                hostRightArmAnim.DoScaledAnimationAsync("HostRightHit", 0.5f);
            }
            else
            {
                hostLeftArmAnim.DoScaledAnimationAsync("HostLeftHit", 0.5f);
            }
        }

        public void HostPrepareHands()
        {
            instance.hostLeftArmAnim.DoScaledAnimationAsync("HostLeftPrepare", 0.5f);
            instance.hostRightArmAnim.DoScaledAnimationAsync("HostPrepare", 0.5f);
        }

        public static void PreInterval(double beat, float interval,
            bool autoPassTurn, bool timeUpSound, bool consecutive, bool visualClock, int audioClock)
        {
            if (GameManager.instance.currentGame == "quizShow")
            {
                instance.StartInterval(beat, interval, beat, autoPassTurn, timeUpSound, consecutive, visualClock, audioClock);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    interval = interval,
                    autoPassTurn = autoPassTurn,
                    timeUpSound = timeUpSound,
                    consecutive = consecutive,
                    visualClock = visualClock,
                    audioClock = audioClock
                });
            }
        }

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool autoPassTurn;
            public bool timeUpSound;
            public bool consecutive;
            public bool visualClock;
            public int audioClock;
        }

        private static List<QueuedInterval> queuedIntervals = new();

        public override void OnGameSwitch(double beat)
        {
            if (queuedIntervals.Count > 0)
            {
                foreach (var interval in queuedIntervals)
                {
                    StartInterval(interval.beat, interval.interval, beat, interval.autoPassTurn, 
                        interval.timeUpSound, interval.consecutive, interval.visualClock, interval.audioClock);
                }
                queuedIntervals.Clear();
            }
            var allRandomEvents = EventCaller.GetAllInGameManagerList("quizShow", new string[] { "randomPresses" });
            foreach (var randomEvent in allRandomEvents)
            {
                randomPresses.Add(new RandomPress()
                {
                    beat = randomEvent.beat,
                    randomPresses = GetRandomPress(randomEvent.beat, randomEvent.length,
                    randomEvent["min"], randomEvent["max"], randomEvent["random"], randomEvent["con"])
                });
            }
        }

        private void StartInterval(double beat, float interval,
            double gameSwitchBeat, bool autoPassTurn, bool timeUpSound, bool consecutive, bool visualClock, int audioClock)
        {
            List<BeatAction.Action> actions = new()
            {
                new BeatAction.Action(beat, delegate
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
                })
            };

            var relevantInputs = GetInputsBetweenBeat(beat, beat + interval);
            relevantInputs.Sort((x, y) => x.beat.CompareTo(y.beat));
            for (int i = 0; i < relevantInputs.Count; i++)
            {
                var input = relevantInputs[i];
                double inputBeat = input.beat;
                if (inputBeat < gameSwitchBeat) continue;
                bool isDpad = input.datamodel == "quizShow/dPad";
                bool isRandom = input.datamodel == "quizShow/randomPresses";
                actions.Add(new BeatAction.Action(inputBeat, delegate
                {
                    HostPressButton(inputBeat, isDpad);
                }));
            }
            BeatAction.New(this, actions);

            if (autoPassTurn)
            {
                PassTurn(beat + interval, beat, interval, timeUpSound, consecutive, visualClock, audioClock, 1);
            }
        }

        public static void PrePassTurn(double beat, bool timeUpSound, bool consecutive, bool visualClock, int audioClock)
        {
            if (GameManager.instance.currentGame == "quizShow")
            {
                instance.PassTurnStandalone(beat, timeUpSound, consecutive, visualClock, audioClock);
            }
            else
            {
                passedTurns.Add(new PassedTurn()
                {
                    beat = beat,
                    timeUpSound = timeUpSound,
                    consecutive = consecutive,
                    visualClock = visualClock,
                    audioClock = audioClock
                });
            }
        }

        private struct PassedTurn
        {
            public double beat;
            public bool timeUpSound;
            public bool consecutive;
            public bool visualClock;
            public int audioClock;
        }

        private static List<PassedTurn> passedTurns = new();

        private void PassTurnStandalone(double beat, bool timeUpSound, bool consecutive, bool visualClock, int audioClock)
        {
            var lastInterval = GetLastIntervalBeforeBeat(beat);
            float length = EventCaller.GetAllInGameManagerList("quizShow", new string[] { "passTurn" }).Find(x => x.beat == beat).length;
            if (lastInterval != null)
            {
                PassTurn(beat, lastInterval.beat, lastInterval.length, timeUpSound, consecutive, visualClock, audioClock, length);
            }
        }

        private void PassTurn(double beat, double intervalBeat, float intervalLength, bool timeUpSound, bool consecutive, bool visualClock, int audioClock, float length)
        {
            playerStartBeat = beat + length - Conductor.instance.SecsToBeats(ngEarlyTime, Conductor.instance.GetBpmAtBeat(beat + length));
            playerLength = intervalLength + (float)Conductor.instance.SecsToBeats(ngEarlyTime, Conductor.instance.GetBpmAtBeat(beat + length));
            var relevantInputs = GetInputsBetweenBeat(intervalBeat, intervalBeat + intervalLength);
            relevantInputs.Sort((x, y) => x.beat.CompareTo(y.beat));

            for (int i = 0; i < relevantInputs.Count; i++)
            {
                double inputBeat = relevantInputs[i].beat - intervalBeat;
                bool isDpad = relevantInputs[i].datamodel == "quizShow/dPad";
                if (isDpad)
                {
                    ScheduleAutoplayInput(beat, length + inputBeat, InputAction_Left, AutoplayDPad, Nothing, Nothing);
                }
                else
                {
                    ScheduleAutoplayInput(beat, length + inputBeat, InputAction_Right, AutoplayAButton, Nothing, Nothing);
                }
            }
            int hundredLoops = Mathf.FloorToInt((float)countToMatch / 100f);
            countToMatch -= hundredLoops * 100;
            doingConsectiveIntervals = consecutive;
            float timeUpBeat = 0f;
            if (audioClock == (int)ClockAudio.Both || audioClock == (int)ClockAudio.Start) 
            {
                SoundByte.PlayOneShotGame("quizShow/timerStart", beat);
                timeUpBeat = 0.5f;
            }
            if (audioClock == (int)ClockAudio.End) timeUpBeat = 0.5f;
            QSTimer spawnedTimer = Instantiate(stopWatchRef, transform);
            if (!visualClock) Destroy(spawnedTimer.gameObject);
            List<BeatAction.Action> actions = new()
            {
                new BeatAction.Action(beat, delegate
                {
                    if (doingConsectiveIntervals)
                    {
                        countToMatch += relevantInputs.Count;
                    }
                    else
                    {
                        countToMatch = relevantInputs.Count;
                    }
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
                    if (visualClock)
                    {
                        spawnedTimer.gameObject.SetActive(true);
                        spawnedTimer.Init(beat + length, intervalLength);
                    }
                }),
                new BeatAction.Action(beat + length + intervalLength, delegate
                {
                    if (!consecutive)
                    {
                        if (audioClock == (int)ClockAudio.Both || audioClock == (int)ClockAudio.End) SoundByte.PlayOneShotGame("quizShow/timerStop");
                        contesteeLeftArmAnim.DoScaledAnimationAsync("LeftRest", 0.5f);
                        contesteeRightArmAnim.DoScaledAnimationAsync("RightRest", 0.5f);
                        shouldPrepareArms = true;
                    }
                    if (visualClock) Destroy(spawnedTimer.gameObject);
                }
            ),
                new BeatAction.Action(beat + length + intervalLength + timeUpBeat, delegate { if (timeUpSound && !consecutive) SoundByte.PlayOneShotGame("quizShow/timeUp"); }),
            };
            BeatAction.New(instance, actions);
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
                SoundByte.PlayOneShotGame("quizShow/contestantDPad");
                contesteeLeftArmAnim.DoScaledAnimationAsync("LeftArmPress", 0.5f);
            }
            else
            {
                SoundByte.PlayOneShotGame("quizShow/contestantA");
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
                    SoundByte.PlayOneShotGame("quizShow/contestantExplode");
                    firstDigitSr.color = new Color(1, 1, 1, 0);
                    secondDigitSr.color = new Color(1, 1, 1, 0);
                    contCounter.sprite = explodedCounter;
                    contExploded = true;
                    contExplosion.Play();
                    break;
                case (int)ShouldExplode.Host:
                    if (hostExploded) return;
                    SoundByte.PlayOneShotGame("quizShow/hostExplode");
                    hostFirstDigitSr.color = new Color(1, 1, 1, 0);
                    hostSecondDigitSr.color = new Color(1, 1, 1, 0);
                    hostCounter.sprite = explodedCounter;
                    hostExploded = true;
                    hostExplosion.Play();
                    break;
                case (int)ShouldExplode.Sign:
                    if (signExploded) return;
                    SoundByte.PlayOneShotGame("quizShow/signExplode");
                    signExploded = true;
                    signExplosion.Play();
                    signAnim.Play("Exploded", 0, 0);
                    break;
            }
        }

        public void RevealAnswer(double beat, float length)
        {
            blackOut.SetActive(true);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate 
                { 
                    SoundByte.PlayOneShotGame("quizShow/answerReveal");
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
                SoundByte.PlayOneShotGame("quizShow/answerReveal");
                hostFirstDigitSr.sprite = hostNumberSprites[GetSpecificDigit(countToMatch, 1)];
                hostSecondDigitSr.sprite = hostNumberSprites[GetSpecificDigit(countToMatch, 2)];
            }
            if (pressCount == countToMatch)
            {
                SoundByte.PlayOneShotGame("quizShow/correct");
                contesteeHead.Play("ContesteeSmile", -1, 0);
                hostHead.Play("HostSmile", -1, 0);
                if (audience) SoundByte.PlayOneShotGame("quizShow/audienceCheer");
                if (jingle) SoundByte.PlayOneShotGame("quizShow/correctJingle");
            }
            else
            {
                ScoreMiss();
                SoundByte.PlayOneShotGame("quizShow/incorrect");
                contesteeHead.Play("ContesteeSad", -1, 0);
                hostHead.Play("HostSad", -1, 0);
                if (audience) SoundByte.PlayOneShotGame("quizShow/audienceSad");
                if (jingle) SoundByte.PlayOneShotGame("quizShow/incorrectJingle");
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


