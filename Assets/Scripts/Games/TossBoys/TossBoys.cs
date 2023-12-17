using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTossBoysLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tossBoys", "Toss Boys", "9cfff7", false, false, new List<GameAction>()
            {
                new GameAction("dispense", "Dispense")
                {
                    function = delegate { var e = eventCaller.currentEntity; TossBoys.instance.Dispense(e.beat, e.length, e["who"], e["auto"], e["interval"], e["ignore"], e["callAuto"], true, e["call"]); },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; TossBoys.DispenseSound(e.beat, e["who"], e["call"]); },
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Akachan, "Receiver", "Who will receive the ball?"),
                        new Param("call", false, "Name Call", "Should the other kids than the receiver call their name?"),

                        //auto dispense stuff
                        new Param("auto", true, "Auto Redispense", "", new()
                        {
                            new((x, _) => (bool)x, new string[] { "interval", "ignore", "callAuto" })
                        }),
                        new Param("interval", new EntityTypes.Integer(1, 20, 2), "Redispense Interval", "Based on passes and not beats"),
                        new Param("ignore", true, "Ignore Special Passes"),
                        new Param("callAuto", false, "Name Call On Redispense")
                    }
                },
                new GameAction("pass", "Normal Toss")
                {
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Aokun, "Receiver", "Who will receive the ball?")
                    }
                },
                new GameAction("dual", "Dual Toss")
                {
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Akachan, "Receiver", "Who will receive the ball?")
                    }
                },
                new GameAction("high", "High Toss")
                {
                    defaultLength = 3f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Kiiyan, "Receiver", "Who will receive the ball?")
                    }
                },
                new GameAction("lightning", "Lightning Toss")
                {
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Aokun, "Receiver", "Who will receive the ball?")
                    }
                },
                new GameAction("blur", "Blur Toss")
                {
                    defaultLength = 2f
                },
                new GameAction("pop", "Pop Ball")
                {
                    defaultLength = 1f,
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; TossBoys.instance.Bop(e.beat, e.length, e["auto"], e["bop"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Should the toss boys bop to the beat?"),
                        new Param("auto", false, "Bop (Auto)", "Should the toss boys auto bop to the beat?")
                    }
                },
                new GameAction("changeBG", "Change Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; TossBoys.instance.BackgroundColor(e.beat, e.length, e["start"], e["end"], e["ease"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", TossBoys.defaultBGColor, "Start Color", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("end", TossBoys.defaultBGColor, "End Color", "The end color for the fade."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
                    }
                },
            },
            new List<string>() {"agb", "normal"},
            "agbtoss", "en",
            new List<string>() {}
            );
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_TossBoys;

    public class TossBoys : Minigame
    {
        private static Color _defaultBGColor;
        public static Color defaultBGColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#62FDBB", out _defaultBGColor);
                return _defaultBGColor;
            }
        }
        public enum KidChoice
        {
            Akachan = 0,
            Aokun = 1,
            Kiiyan = 2
        }
        public enum WhichTossKid
        {
            None = -1,
            Akachan = 0,
            Aokun = 1,
            Kiiyan = 2
        }
        [Header("Components")]
        [SerializeField] TossKid akachan;
        [SerializeField] TossKid aokun;
        [SerializeField] TossKid kiiyan;
        [SerializeField] Animator hatchAnim;
        [SerializeField] TossBoysBall ballPrefab;
        [SerializeField] GameObject specialAka;
        [SerializeField] GameObject specialAo;
        [SerializeField] GameObject specialKii;
        [SerializeField] TossKid currentSpecialKid;
        [SerializeField] SpriteRenderer bg;

        [Header("Properties")]
        [SerializeField] SuperCurveObject.Path[] ballPaths;
        WhichTossKid lastReceiver = WhichTossKid.None;
        WhichTossKid currentReceiver = WhichTossKid.None;
        public TossBoysBall currentBall = null;
        Dictionary<double, RiqEntity> passBallDict = new();
        string currentPassType;
        public static TossBoys instance;
        float currentEventLength;

        const int IAAka = IAMAXCAT;
        const int IAAo = IAMAXCAT + 1;
        const int IAKii = IAMAXCAT + 2;

        protected static bool IA_PadDir(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_PadAlt(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }

        protected static bool IA_TouchNrm(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && (instance.currentReceiver is WhichTossKid.Akachan
                    || (instance.lastReceiver is WhichTossKid.Akachan or WhichTossKid.None
                        && instance.currentReceiver is WhichTossKid.None)
                    || (instance.IsExpectingInputNow(InputAction_Aka)
                        && !(instance.IsExpectingInputNow(InputAction_Ao) || instance.IsExpectingInputNow(InputAction_Kii))));
        }
        protected static bool IA_TouchDir(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && (instance.currentReceiver is WhichTossKid.Kiiyan
                    || (instance.lastReceiver is WhichTossKid.Kiiyan
                        && instance.currentReceiver is WhichTossKid.None)
                    || (instance.IsExpectingInputNow(InputAction_Kii)
                        && !(instance.IsExpectingInputNow(InputAction_Ao) || instance.IsExpectingInputNow(InputAction_Aka))));
        }
        protected static bool IA_TouchAlt(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && (instance.currentReceiver is WhichTossKid.Aokun
                    || (instance.lastReceiver is WhichTossKid.Aokun
                        && instance.currentReceiver is WhichTossKid.None)
                    || (instance.IsExpectingInputNow(InputAction_Ao)
                        && !(instance.IsExpectingInputNow(InputAction_Aka) || instance.IsExpectingInputNow(InputAction_Kii))));
        }

        protected static bool IA_BatonNrm(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.Face, out dt)
                && (instance.currentReceiver is WhichTossKid.Akachan
                    || (instance.lastReceiver is WhichTossKid.Akachan or WhichTossKid.None
                        && instance.currentReceiver is WhichTossKid.None)
                    || (instance.IsExpectingInputNow(InputAction_Aka)
                        && !(instance.IsExpectingInputNow(InputAction_Ao) || instance.IsExpectingInputNow(InputAction_Kii))));
        }
        protected static bool IA_BatonDir(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.Face, out dt)
                && (instance.currentReceiver is WhichTossKid.Kiiyan
                    || (instance.lastReceiver is WhichTossKid.Kiiyan
                        && instance.currentReceiver is WhichTossKid.None)
                    || (instance.IsExpectingInputNow(InputAction_Ao)
                        && !(instance.IsExpectingInputNow(InputAction_Aka) || instance.IsExpectingInputNow(InputAction_Kii))));
        }
        protected static bool IA_BatonAlt(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.Face, out dt)
                && (instance.currentReceiver is WhichTossKid.Aokun
                    || (instance.lastReceiver is WhichTossKid.Aokun
                        && instance.currentReceiver is WhichTossKid.None));
        }

        public static PlayerInput.InputAction InputAction_Aka =
            new("BasicPress", new int[] { IAAka, IAAka, IAAka },
            IA_PadBasicPress, IA_TouchNrm, IA_BatonNrm);
        public static PlayerInput.InputAction InputAction_Ao =
            new("BasicPress", new int[] { IAAo, IAAo, IAAo },
            IA_PadAlt, IA_TouchAlt, IA_BatonAlt);
        public static PlayerInput.InputAction InputAction_Kii =
            new("BasicPress", new int[] { IAKii, IAKii, IAKii },
            IA_PadDir, IA_TouchDir, IA_BatonDir);

        private void Awake()
        {
            instance = this;
            colorStart = defaultBGColor;
            colorEnd = defaultBGColor;
            SetupBopRegion("tossBoys", "bop", "auto");
            SetPassBallEvents();
        }

        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in ballPaths)
            {
                if (path.preview)
                {
                    ballPrefab.DrawEditorGizmo(path);
                }
            }
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in ballPaths)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat))
            {
                SingleBop();
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            BackgroundColorUpdate();
            if (cond.isPlaying && !cond.isPaused)
            {
                if (PlayerInput.GetIsAction(InputAction_Aka) && !IsExpectingInputNow(InputAction_Aka))
                {
                    akachan.HitBall(false);
                }
                if (PlayerInput.GetIsAction(InputAction_Ao) && !IsExpectingInputNow(InputAction_Ao))
                {
                    aokun.HitBall(false);
                }
                if (PlayerInput.GetIsAction(InputAction_Kii) && !IsExpectingInputNow(InputAction_Kii))
                {
                    kiiyan.HitBall(false);
                }
            }
        }

        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStart = Color.white; //obviously put to the default color of the game
        private Color colorEnd = Color.white;
        private Util.EasingFunction.Ease colorEase; //putting Util in case this game is using jukebox

        //call this in update
        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newR = func(colorStart.r, colorEnd.r, normalizedBeat);
            float newG = func(colorStart.g, colorEnd.g, normalizedBeat);
            float newB = func(colorStart.b, colorEnd.b, normalizedBeat);

            bg.color = new Color(newR, newG, newB);
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStart = colorStartSet;
            colorEnd = colorEndSet;
            colorEase = (Util.EasingFunction.Ease)ease;
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("tossBoys", new string[] { "changeBG" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["start"], lastEvent["end"], lastEvent["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
            HandleDispenses(beat);
        }

        private void HandleDispenses(double beat)
        {
            var allRelevantDispenses = EventCaller.GetAllInGameManagerList("tossBoys", new string[] { "dispense" }).FindAll(x => x.beat < beat && x.beat + x.length >= beat);
            if (allRelevantDispenses.Count == 0) return;

            var e = allRelevantDispenses[^1];

            Dispense(e.beat, e.length, e["who"], e["auto"], e["interval"], e["ignore"], e["callAuto"], false, e["call"]);
        }

        #region Bop 
        void SingleBop()
        {
            akachan.Bop();
            aokun.Bop();
            kiiyan.Bop();
        }

        public void Bop(double beat, float length, bool auto, bool goBop)
        {
            if (goBop)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate { SingleBop(); }));
                }
                BeatAction.New(instance, bops);
            }
        }
        #endregion

        public static void DispenseSound(double beat, int who, bool call)
        {
            SoundByte.PlayOneShotGame("tossBoys/ballStart" + GetColorBasedOnTossKid((WhichTossKid)who, true), beat, forcePlay: true);
            if (!call) return;
            double callBeat = beat;
            switch (who)
            {
                case (int)WhichTossKid.Akachan:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("tossBoys/blueRedHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/yellowRedHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/blueRedHigh2", callBeat + 0.25f),
                        new MultiSound.Sound("tossBoys/yellowRedHigh2", callBeat + 0.25f),
                        new MultiSound.Sound("tossBoys/blueRedHigh3", callBeat + 0.5f),
                        new MultiSound.Sound("tossBoys/yellowRedHigh3", callBeat + 0.5f),
                    }, forcePlay: true);
                    break;
                case (int)WhichTossKid.Aokun:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("tossBoys/redBlueHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/yellowBlueHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/redBlueHigh2", callBeat + 0.5f),
                        new MultiSound.Sound("tossBoys/yellowBlueHigh2", callBeat + 0.5f),
                    }, forcePlay: true);
                    break;
                case (int)WhichTossKid.Kiiyan:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("tossBoys/redYellowHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/blueYellowHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/redYellowHigh2", callBeat + 0.5f),
                        new MultiSound.Sound("tossBoys/blueYellowHigh2", callBeat + 0.5f),
                    }, forcePlay: true);
                    break;
                default:
                    break;
            }
        }

        public void Dispense(double beat, float length, int who, bool auto, int autoInterval, bool ignoreSpecial, bool callAuto, bool playSound, bool call)
        {
            if (playSound) DispenseSound(beat, who, call);
            DispenseExec(beat, length, who, false, "");
            if (auto && passBallDict.TryGetValue(beat + length, out var e))
            {
                if (e.datamodel == "tossBoys/blur")
                {
                    DispenseRecursion(beat + length, -1, autoInterval, ignoreSpecial, callAuto, (int)WhichTossKid.None, who, false, e.length, true, true, e.datamodel);
                }
                else DispenseRecursion(beat + length, -1, autoInterval, ignoreSpecial, callAuto, e["who"], who, false, e.length, IsSpecialEvent(e.datamodel), false, e.datamodel);
            }
        }

        public void DispenseRecursion(double beat, int index, int interval, bool ignore, bool call, int curReceiver, int previousReceiver, bool isBlur, float currentLength, bool isSpecial, bool shouldForce, string eventDatamodel)
        {
            if (index % interval == 0 && !isBlur && !(ignore && isSpecial))
            {
                double dispenseBeat = beat - 2;
                BeatAction.New(this, new()
                {
                    new(dispenseBeat, delegate 
                    {
                        if (currentBall != null) return;
                        DispenseSound(dispenseBeat, curReceiver, call);
                        DispenseExec(dispenseBeat, 2, curReceiver, shouldForce, eventDatamodel);
                    })
                });
            }
            if (!isBlur && !(ignore && isSpecial)) index++;

            var tempLastReceiver = previousReceiver;
            var lastLength = isBlur ? 1 : currentLength;
            previousReceiver = curReceiver;
            var nextIsSpecial = isSpecial;

            var blurSet = isBlur;
            var nextForce = false;
            if (passBallDict.TryGetValue(beat + lastLength, out var e))
            {
                if (e.datamodel == "tossBoys/pop") return;
                curReceiver = e["who"];
                blurSet = e.datamodel == "tossBoys/blur";
                currentLength = e.length;
                nextIsSpecial = IsSpecialEvent(e.datamodel);
                eventDatamodel = e.datamodel;
            }
            else
            {
                curReceiver = tempLastReceiver;
                nextForce = true;
            }
            // let's not do a stack overflow, alright?
            BeatAction.New(this, new()
            {
                new(beat + lastLength - 2, delegate { DispenseRecursion(beat + lastLength, index, interval, ignore, call, curReceiver, previousReceiver, blurSet, currentLength, nextIsSpecial, nextForce, eventDatamodel); })
            });
        }

        public void DispenseExec(double beat, float length, int who, bool forcePass, string eventDatamodel)
        {
            if (currentBall != null) return;
            SetReceiver(who);
            GetCurrentReceiver().ShowArrow(beat, length - 1);

            hatchAnim.Play("HatchOpen", 0, 0);
            currentBall = Instantiate(ballPrefab, transform);
            currentBall.gameObject.SetActive(true);
            switch (who)
            {
                case (int)WhichTossKid.Akachan:
                    currentBall.SetState(TossBoysBall.State.RedDispense, beat, length);
                    break;
                case (int)WhichTossKid.Aokun:
                    currentBall.SetState(TossBoysBall.State.BlueDispense, beat, length);
                    break;
                case (int)WhichTossKid.Kiiyan:
                    currentBall.SetState(TossBoysBall.State.YellowDispense, beat, length);
                    break;
                default:
                    break;
            }

            if (passBallDict.ContainsKey(beat + length))
            {
                ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
                if (IsSpecialEvent(passBallDict[beat + length].datamodel))
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                    });
                }
                else if (passBallDict[beat + length].datamodel == "tossBoys/pop")
                {
                    currentBall.willBePopped = true;
                    if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + length - 1, delegate { GetCurrentReceiver().PopBallPrepare(); })
                        });
                }
            }
            else if (forcePass)
            {
                ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
                if (IsSpecialEvent(eventDatamodel))
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                    });
                }
                else if (eventDatamodel == "tossBoys/pop")
                {
                    currentBall.willBePopped = true;
                    if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + length - 1, delegate { GetCurrentReceiver().PopBallPrepare(); })
                        });
                }
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length, delegate { Miss(null); })
                });
            }
        }

        void SetPassBallEvents()
        {
            passBallDict.Clear();
            var passBallEvents = EventCaller.GetAllInGameManagerList("tossBoys", new string[] { "pass", "dual", "pop", "high", "lightning", "blur" });
            for (int i = 0; i < passBallEvents.Count; i++)
            {
                if (passBallDict.ContainsKey(passBallEvents[i].beat)) continue;
                passBallDict.Add(passBallEvents[i].beat, passBallEvents[i]);
            }
        }

        private void DeterminePassValues(double beat)
        {
            var tempLastReceiver = lastReceiver;
            lastReceiver = currentReceiver;
            if (passBallDict.TryGetValue(beat, out var receiver))
            {
                if (receiver.datamodel != "tossBoys/blur") currentReceiver = (WhichTossKid)receiver["who"];
                currentPassType = receiver.datamodel;
                currentEventLength = receiver.length;
            }
            else
            {
                currentReceiver = tempLastReceiver;
            }
        }

        void DeterminePass(double beat, bool barely)
        {
            DeterminePassValues(beat);
            switch (currentPassType)
            {
                case "tossBoys/pass":
                    PassBall(beat, currentEventLength);
                    break;
                case "tossBoys/dual":
                    DualToss(beat, currentEventLength);
                    break;
                case "tossBoys/high":
                    HighToss(beat, currentEventLength);
                    break;
                case "tossBoys/lightning":
                    LightningToss(beat, currentEventLength);
                    break;
                default:
                    break;
            }
            currentBall.anim.DoScaledAnimationAsync(barely ? "WiggleBall" : "Hit", 0.5f);
            if (passBallDict.ContainsKey(beat + currentEventLength) && passBallDict[beat + currentEventLength].datamodel == "tossBoys/pop")
            {
                currentBall.willBePopped = true;
                if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + currentEventLength - 1, delegate { GetCurrentReceiver().PopBallPrepare(); })
                    });
            }
        }

        void PassBall(double beat, float length)
        {
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float secondBeat = 1f;
            float secondOffset = 0;
            float thirdOffset = 0;
            if (currentBall != null)
            {
                switch (last + current)
                {
                    case "redBlue":
                        currentBall.SetState(TossBoysBall.State.RedBlue, beat, length);
                        break;
                    case "blueRed":
                        secondBeat = 0.5f;
                        currentBall.SetState(TossBoysBall.State.BlueRed, beat, length);
                        break;
                    case "blueYellow":
                        currentBall.SetState(TossBoysBall.State.BlueYellow, beat, length);
                        break;
                    case "yellowRed":
                        secondBeat = 0.5f;
                        currentBall.SetState(TossBoysBall.State.YellowRed, beat, length);
                        break;
                    case "redYellow":
                        secondBeat = 0.5f;
                        thirdOffset = 0.060f;
                        currentBall.SetState(TossBoysBall.State.RedYellow, beat, length);
                        break;
                    case "yellowBlue":
                        currentBall.SetState(TossBoysBall.State.YellowBlue, beat, length);
                        break;
                    default:
                        break;
                }
            }

            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("tossBoys/" + last + current + 1, beat),
                new MultiSound.Sound("tossBoys/" + last + current + 2, beat + secondBeat, 1, 1, false, secondOffset),
            };
            if (passBallDict.ContainsKey(beat + length) && (passBallDict[beat + length].datamodel is "tossBoys/dual" or "tossBoys/lightning" or "tossBoys/blur"))
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                });
            }
            if (secondBeat == 0.5f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + 3, beat + 1, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        void DualToss(double beat, float length)
        {
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float secondBeat = 0.5f;
            float secondOffset = 0;
            float thirdOffset = 0;
            if (currentBall != null)
            {
                switch (last + current)
                {
                    case "redBlue":
                        currentBall.SetState(TossBoysBall.State.RedBlueDual, beat, length);
                        break;
                    case "blueYellow":
                        currentBall.SetState(TossBoysBall.State.BlueYellowDual, beat, length);
                        break;
                    case "yellowBlue":
                        currentBall.SetState(TossBoysBall.State.YellowBlueDual, beat, length);
                        break;
                    case "blueRed":
                        secondBeat = 0.25f;
                        thirdOffset = 0.020f;
                        currentBall.SetState(TossBoysBall.State.BlueRedDual, beat, length);
                        break;
                    case "yellowRed":
                        secondBeat = 0.25f;
                        currentBall.SetState(TossBoysBall.State.YellowRedDual, beat, length);
                        break;
                    case "redYellow":
                        secondOffset = 0.060f;
                        currentBall.SetState(TossBoysBall.State.RedYellowDual, beat, length);
                        break;
                    default:
                        break;
                }
            }

            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("tossBoys/" + last + current + "Low" + 1, beat),
                new MultiSound.Sound("tossBoys/" + last + current + "Low" + 2, beat + secondBeat, 1, 1, false, secondOffset),
            };
            if (passBallDict.ContainsKey(beat + length) && (passBallDict[beat + length].datamodel is "tossBoys/lightning" or "tossBoys/blur"))
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                });
            }
            if (secondBeat == 0.25f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + "Low" + 3, beat + 0.5f, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            bool stopSpecial = passBallDict.ContainsKey(beat + length) && passBallDict[beat + length].datamodel is "tossBoys/pass" or "tossBoys/high" or "tossBoys/pop";
            ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), stopSpecial ? JustHitBallUnSpecial : JustHitBall, Miss, Empty);
        }

        void HighToss(double beat, float length)
        {
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float secondBeat = 0.5f;
            float secondOffset = 0;
            float thirdOffset = 0;
            if (currentBall != null)
            {
                switch (last + current)
                {
                    case "redBlue":
                        currentBall.SetState(TossBoysBall.State.RedBlueHigh, beat, length);
                        break;
                    case "redYellow":
                        currentBall.SetState(TossBoysBall.State.RedYellowHigh, beat, length);
                        break;
                    case "blueYellow":
                        currentBall.SetState(TossBoysBall.State.BlueYellowHigh, beat, length);
                        break;
                    case "yellowBlue":
                        currentBall.SetState(TossBoysBall.State.YellowBlueHigh, beat, length);
                        break;
                    case "yellowRed":
                        secondBeat = 0.25f;
                        currentBall.SetState(TossBoysBall.State.YellowRedHigh, beat, length);
                        break;
                    case "blueRed":
                        secondBeat = 0.25f;
                        currentBall.SetState(TossBoysBall.State.BlueRedHigh, beat, length);
                        break;
                    default:
                        break;
                }
            }

            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("tossBoys/" + last + current + "High" + 1, beat),
                new MultiSound.Sound("tossBoys/" + last + current + "High" + 2, beat + secondBeat, 1, 1, false, secondOffset),
            };
            if (passBallDict.ContainsKey(beat + length) && (passBallDict[beat + length].datamodel is "tossBoys/dual" or "tossBoys/lightning" or "tossBoys/blur"))
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                });
            }
            if (secondBeat == 0.25f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + "High" + 3, beat + 0.5f, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        void LightningToss(double beat, float length)
        {
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float secondBeat = 0.5f;
            float secondOffset = 0;
            float thirdOffset = 0;
            switch (last + current)
            {
                case "blueRed":
                    secondBeat = 0.25f;
                    thirdOffset = 0.020f;
                    break;
                case "yellowRed":
                    secondBeat = 0.25f;
                    break;
                case "redYellow":
                    secondOffset = 0.060f;
                    break;
                default:
                    secondBeat = 0.5f;
                    break;
            }
            if (currentBall != null)
            {
                switch (last)
                {
                    case "blue":
                        currentBall.SetState(TossBoysBall.State.BlueKeep, beat, length / 2);
                        break;
                    case "red":
                        currentBall.SetState(TossBoysBall.State.RedKeep, beat, length / 2);
                        break;
                    case "yellow":
                        currentBall.SetState(TossBoysBall.State.YellowKeep, beat, length / 2);
                        break;
                }
            }
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("tossBoys/" + last + current + "Low" + 1, beat),
                new MultiSound.Sound("tossBoys/" + last + current + "Low" + 2, beat + secondBeat, 1, 1, false, secondOffset),
            };
            if (secondBeat == 0.25f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + "Low" + 3, beat + 0.5f, 1, 1, false, thirdOffset));
            if (passBallDict.ContainsKey(beat + length) && (passBallDict[beat + length].datamodel is "tossBoys/dual" or "tossBoys/blur"))
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                });
            }
            MultiSound.Play(soundsToPlay.ToArray());
            bool stopSpecial = passBallDict.ContainsKey(beat + length) && passBallDict[beat + length].datamodel is "tossBoys/pass" or "tossBoys/high" or "tossBoys/pop";
            ScheduleInput(beat, length / 2, GetInputBasedOnTossKid(lastReceiver), stopSpecial ? JustKeepUnSpecial : JustKeep, Miss, Empty);
            ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        void BlurToss(double beat)
        {
            string current = GetColorBasedOnTossKid(currentReceiver, false);
            if (currentBall != null)
            {
                switch (current)
                {
                    case "blue":
                        currentBall.SetState(TossBoysBall.State.BlueBlur, beat);
                        break;
                    case "red":
                        currentBall.SetState(TossBoysBall.State.RedBlur, beat);
                        break;
                    case "yellow":
                        currentBall.SetState(TossBoysBall.State.YellowBlur, beat);
                        break;
                }
            }

            ScheduleInput(beat, 2f, GetInputTypeBasedOnCurrentReceiver(), JustKeepContinue, Miss, Empty);
        }

        #region Inputs
        void JustHitBall(PlayerActionEvent caller, float state)
        {
            if (currentBall == null) return;
            if (passBallDict.ContainsKey(caller.startBeat + caller.timer))
            {
                if (passBallDict[caller.startBeat + caller.timer].datamodel == "tossBoys/pop")
                {
                    GetCurrentReceiver().PopBall();
                    Destroy(currentBall.gameObject);
                    currentBall = null;
                    switch (currentReceiver)
                    {
                        case WhichTossKid.Akachan:
                            SoundByte.PlayOneShotGame("tossBoys/redPop");
                            break;
                        case WhichTossKid.Aokun:
                            SoundByte.PlayOneShotGame("tossBoys/bluePop");
                            break;
                        case WhichTossKid.Kiiyan:
                            SoundByte.PlayOneShotGame("tossBoys/yellowPop");
                            break;
                        default:
                            break;
                    }
                    return;
                }
                if (passBallDict[caller.startBeat + caller.timer].datamodel == "tossBoys/blur")
                {
                    JustKeepCurrent(caller, state);
                    BlurToss(caller.startBeat + caller.timer);
                    return;
                }
                if ((WhichTossKid)passBallDict[caller.startBeat + caller.timer]["who"] == currentReceiver)
                {
                    Miss(null);
                    return;
                }
            }
            if (state >= 1f || state <= -1f)
            {
                GetCurrentReceiver().Barely();
                DeterminePass(caller.timer + caller.startBeat, true);
                return;
            }
            GetCurrentReceiver().HitBall();
            DeterminePass(caller.timer + caller.startBeat, false);
        }

        void JustHitBallUnSpecial(PlayerActionEvent caller, float state)
        {
            if (currentBall == null) return;
            specialAo.SetActive(false);
            specialAka.SetActive(false);
            specialKii.SetActive(false);
            currentSpecialKid.crouch = false;
            if (passBallDict.ContainsKey(caller.startBeat + caller.timer))
            {
                if (passBallDict[caller.startBeat + caller.timer].datamodel == "tossBoys/pop")
                {
                    GetCurrentReceiver().PopBall();
                    Destroy(currentBall.gameObject);
                    currentBall = null;
                    switch (currentReceiver)
                    {
                        case WhichTossKid.Akachan:
                            SoundByte.PlayOneShotGame("tossBoys/redPop");
                            break;
                        case WhichTossKid.Aokun:
                            SoundByte.PlayOneShotGame("tossBoys/bluePop");
                            break;
                        case WhichTossKid.Kiiyan:
                            SoundByte.PlayOneShotGame("tossBoys/yellowPop");
                            break;
                        default:
                            break;
                    }
                    return;
                }
                if (passBallDict[caller.startBeat + caller.timer].datamodel == "tossBoys/blur")
                {
                    JustKeepCurrentUnSpecial(caller, state);
                    BlurToss(caller.startBeat + caller.timer);
                    return;
                }
                if ((WhichTossKid)passBallDict[caller.startBeat + caller.timer]["who"] == currentReceiver)
                {
                    Miss(null);
                    return;
                }
            }
            if (state >= 1f || state <= -1f)
            {
                GetCurrentReceiver().Barely();
                DeterminePass(caller.timer + caller.startBeat, true);
                return;
            }
            GetCurrentReceiver().HitBall();
            DeterminePass(caller.timer + caller.startBeat, false);
        }

        void JustKeepContinue(PlayerActionEvent caller, float state)
        {
            if (currentBall == null) return;
            if (passBallDict.ContainsKey(caller.timer + caller.startBeat))
            {
                if (passBallDict[caller.timer + caller.startBeat].datamodel is "tossBoys/pass" or "tossBoys/high" or "tossBoys/pop")
                {
                    JustHitBallUnSpecial(caller, state);
                }
                else
                {
                    JustHitBall(caller, state);
                }

            }
            else
            {
                JustKeepCurrent(caller, state);
                ScheduleInput(caller.timer + caller.startBeat, 1f, GetInputTypeBasedOnCurrentReceiver(), JustKeepContinue, Miss, Empty);
            }

        }

        void JustKeepCurrent(PlayerActionEvent caller, float state)
        {
            if (currentBall == null) return;
            SoundByte.PlayOneShotGame("tossBoys/" + GetColorBasedOnTossKid(currentReceiver, false) + "Keep");
            string current = GetColorBasedOnTossKid(currentReceiver, false);
            double beat = caller.timer + caller.startBeat;
            if (currentBall != null)
            {
                switch (current)
                {
                    case "blue":
                        currentBall.SetState(TossBoysBall.State.BlueKeep, beat);
                        break;
                    case "red":
                        currentBall.SetState(TossBoysBall.State.RedKeep, beat);
                        break;
                    case "yellow":
                        currentBall.SetState(TossBoysBall.State.YellowKeep, beat);
                        break;
                }
            }

            if (state >= 1f || state <= -1f)
            {
                currentBall.anim.DoScaledAnimationAsync("WiggleBall", 0.5f);
                GetCurrentReceiver().Barely();
                return;
            }
            GetCurrentReceiver().HitBall();
            currentBall.anim.DoScaledAnimationAsync("Hit", 0.5f);
        }

        void JustKeepCurrentUnSpecial(PlayerActionEvent caller, float state)
        {
            if (currentBall == null) return;
            specialAo.SetActive(false);
            specialAka.SetActive(false);
            specialKii.SetActive(false);
            currentSpecialKid.crouch = false;
            SoundByte.PlayOneShotGame("tossBoys/" + GetColorBasedOnTossKid(currentReceiver, false) + "Keep");
            if (state >= 1f || state <= -1f)
            {
                currentBall.anim.DoScaledAnimationAsync("WiggleBall", 0.5f);
                GetCurrentReceiver().Barely();
                return;
            }
            GetCurrentReceiver().HitBall();
            currentBall.anim.DoScaledAnimationAsync("Hit", 0.5f);
        }

        void JustKeep(PlayerActionEvent caller, float state)
        {
            if (currentBall == null) return;
            SoundByte.PlayOneShotGame("tossBoys/" + GetColorBasedOnTossKid(lastReceiver, false) + "Keep");
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            double beat = caller.timer + caller.startBeat;
            if (currentBall != null)
            {
                switch (last + current)
                {
                    case "redBlue":
                        currentBall.SetState(TossBoysBall.State.RedBlueDual, beat, currentEventLength / 2);
                        break;
                    case "blueYellow":
                        currentBall.SetState(TossBoysBall.State.BlueYellowDual, beat, currentEventLength / 2);
                        break;
                    case "yellowBlue":
                        currentBall.SetState(TossBoysBall.State.YellowBlueDual, beat, currentEventLength / 2);
                        break;
                    case "blueRed":
                        currentBall.SetState(TossBoysBall.State.BlueRedDual, beat, currentEventLength / 2);
                        break;
                    case "yellowRed":
                        currentBall.SetState(TossBoysBall.State.YellowRedDual, beat, currentEventLength / 2);
                        break;
                    case "redYellow":
                        currentBall.SetState(TossBoysBall.State.RedYellowDual, beat, currentEventLength / 2);
                        break;
                    default:
                        break;
                }
            }
            if (state >= 1f || state <= -1f)
            {
                currentBall.anim.DoScaledAnimationAsync("WiggleBall", 0.5f);
                GetReceiver(lastReceiver).Barely();
                return;
            }
            GetReceiver(lastReceiver).HitBall();
            currentBall.anim.DoScaledAnimationAsync("Hit", 0.5f);
        }

        void JustKeepUnSpecial(PlayerActionEvent caller, float state)
        {
            if (currentBall == null) return;
            specialAo.SetActive(false);
            specialAka.SetActive(false);
            specialKii.SetActive(false);
            currentSpecialKid.crouch = false;
            SoundByte.PlayOneShotGame("tossBoys/" + GetColorBasedOnTossKid(lastReceiver, false) + "Keep");
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            double beat = caller.timer + caller.startBeat;
            if (currentBall != null)
            {
                switch (last + current)
                {
                    case "redBlue":
                        currentBall.SetState(TossBoysBall.State.RedBlueDual, beat);
                        break;
                    case "blueYellow":
                        currentBall.SetState(TossBoysBall.State.BlueYellowDual, beat);
                        break;
                    case "yellowBlue":
                        currentBall.SetState(TossBoysBall.State.YellowBlueDual, beat);
                        break;
                    case "blueRed":
                        currentBall.SetState(TossBoysBall.State.BlueRedDual, beat);
                        break;
                    case "yellowRed":
                        currentBall.SetState(TossBoysBall.State.YellowRedDual, beat);
                        break;
                    case "redYellow":
                        currentBall.SetState(TossBoysBall.State.RedYellowDual, beat);
                        break;
                    default:
                        break;
                }
            }

            if (state >= 1f || state <= -1f)
            {
                currentBall.anim.DoScaledAnimationAsync("WiggleBall", 0.5f);
                GetReceiver(lastReceiver).Barely();
                return;
            }
            GetReceiver(lastReceiver).HitBall();
            currentBall.anim.DoScaledAnimationAsync("Hit", 0.5f);
        }

        void Miss(PlayerActionEvent caller)
        {
            if (currentBall == null) return;
            GetCurrentReceiver().Miss();
            aokun.crouch = false;
            akachan.crouch = false;
            kiiyan.crouch = false;
            specialAo.SetActive(false);
            specialAka.SetActive(false);
            specialKii.SetActive(false);
            Destroy(currentBall.gameObject);
            currentBall = null;
            SoundByte.PlayOneShotGame("tossBoys/misshit");
            if (caller != null) DeterminePassValues(caller.startBeat + caller.timer);
        }

        void Empty(PlayerActionEvent caller) { }
        #endregion

        #region HelperFunctions

        void DoSpecialBasedOnReceiver(double beat)
        {
            specialAo.SetActive(false);
            specialAka.SetActive(false);
            specialKii.SetActive(false);
            if (currentSpecialKid != null) currentSpecialKid.crouch = false;
            currentSpecialKid = GetCurrentReceiver();

            GetCurrentReceiver().Crouch();

            GetSpecialBasedOnReceiver().SetActive(true);
            switch (currentReceiver)
            {
                case WhichTossKid.Akachan:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("tossBoys/redSpecial1", beat),
                        new MultiSound.Sound("tossBoys/redSpecial2", beat + 0.25f),
                        new MultiSound.Sound("tossBoys/redSpecialCharge", beat + 0.25f, 1, 1, false, 0.085f),
                    });
                    break;
                case WhichTossKid.Aokun:
                    MultiSound.Play(new MultiSound.Sound[]
                    {
                        new MultiSound.Sound("tossBoys/blueSpecial1", beat),
                        new MultiSound.Sound("tossBoys/blueSpecial2", beat + 0.25f),
                    });
                    break;
                case WhichTossKid.Kiiyan:
                    SoundByte.PlayOneShotGame("tossBoys/yellowSpecial", beat);
                    break;
                default:
                    break;
            }
        }
        public TossKid GetCurrentReceiver()
        {
            return GetReceiver(currentReceiver);
        }

        public TossKid GetReceiver(WhichTossKid receiver)
        {
            switch (receiver)
            {
                case WhichTossKid.Akachan:
                    return akachan;
                case WhichTossKid.Aokun:
                    return aokun;
                case WhichTossKid.Kiiyan:
                    return kiiyan;
                default:
                    return null;
            }
        }

        public static string GetColorBasedOnTossKid(WhichTossKid tossKid, bool capital)
        {
            switch (tossKid)
            {
                case WhichTossKid.Akachan:
                    return capital ? "Red" : "red";
                case WhichTossKid.Aokun:
                    return capital ? "Blue" : "blue";
                case WhichTossKid.Kiiyan:
                    return capital ? "Yellow" : "yellow";
                default:
                    return "";
            }
        }

        public void SetReceiver(int who)
        {
            currentReceiver = (WhichTossKid)who;
        }

        PlayerInput.InputAction GetInputTypeBasedOnCurrentReceiver()
        {
            return GetInputBasedOnTossKid(currentReceiver);
        }

        PlayerInput.InputAction GetInputBasedOnTossKid(WhichTossKid tossKid)
        {
            switch (tossKid)
            {
                case WhichTossKid.Aokun:
                    return InputAction_Ao;
                case WhichTossKid.Kiiyan:
                    return InputAction_Kii;
                default:
                    return InputAction_Aka;
            }
        }

        GameObject GetSpecialBasedOnReceiver()
        {
            switch (currentReceiver)
            {
                case WhichTossKid.Akachan:
                    return specialAka;
                case WhichTossKid.Aokun:
                    return specialAo;
                case WhichTossKid.Kiiyan:
                    return specialKii;
                default:
                    return null;
            }
        }

        private bool IsSpecialEvent(string e)
        {
            bool b = false;

            switch (e)
            {
                case "tossBoys/dual":
                case "tossBoys/lightning":
                case "tossBoys/blur":
                    b = true; break;
                default:
                    return b;
            }

            return b;
        }

        #endregion
    }
}

