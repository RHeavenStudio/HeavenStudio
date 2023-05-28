using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using DG.Tweening;

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
                    function = delegate { var e = eventCaller.currentEntity; TossBoys.instance.Dispense(e.beat, e.length, e["who"], e["call"]); },
                    defaultLength = 2f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Akachan, "Receiver", "Who will receive the ball?"),
                        new Param("call", false, "Name Call", "Should the other kids than the receiver call their name?")
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
                    function = delegate {var e = eventCaller.currentEntity; TossBoys.instance.FadeBackgroundColor(e["start"], e["end"], e.length, e["toggle"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", TossBoys.defaultBGColor, "Start Color", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("end", TossBoys.defaultBGColor, "End Color", "The end color for the fade."),
                        new Param("toggle", false, "Instant", "Should the background instantly change color?")
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
        Tween bgColorTween;
        [SerializeField] SuperCurveObject.Path[] ballPaths;
        WhichTossKid lastReceiver = WhichTossKid.None;
        WhichTossKid currentReceiver = WhichTossKid.None;
        public TossBoysBall currentBall = null;
        Dictionary<float, DynamicBeatmap.DynamicEntity> passBallDict = new Dictionary<float, DynamicBeatmap.DynamicEntity>();
        string currentPassType;
        public static TossBoys instance;
        bool shouldBop = true;
        public GameEvent bop = new GameEvent();
        float currentEventLength;

        private void Awake()
        {
            instance = this;
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

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
                {
                    if (shouldBop)
                    {
                        SingleBop();
                    }
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    akachan.HitBall(false);
                }
                if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
                {
                    aokun.HitBall(false);
                }
                if (PlayerInput.GetAnyDirectionDown() && !IsExpectingInputNow(InputType.DIRECTION_DOWN))
                {
                    kiiyan.HitBall(false);
                }
            }
        }

        public void ChangeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);

            if (seconds == 0)
            {
                bg.color = color;
            }
            else
            {
                bgColorTween = bg.DOColor(color, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats, bool instant)
        {
            ChangeBackgroundColor(start, 0f);
            if (!instant) ChangeBackgroundColor(end, beats);
        }

        #region Bop 
        void SingleBop()
        {
            akachan.Bop();
            aokun.Bop();
            kiiyan.Bop();
        }

        public void Bop(float beat, float length, bool auto, bool goBop)
        {
            shouldBop = auto;
            if (goBop)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate { SingleBop(); }));
                }
                BeatAction.New(instance.gameObject, bops);
            }
        }
        #endregion

        public void Dispense(float beat, float length, int who, bool call)
        {
            if (currentBall != null) return;
            SetPassBallEvents();
            SetReceiver(who);
            GetCurrentReceiver().ShowArrow(beat, length - 1);
            Jukebox.PlayOneShotGame("tossBoys/ballStart" + GetColorBasedOnTossKid(currentReceiver, true));
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

            if (call)
            {
                float callBeat = beat;
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
                        });
                        break;
                    case (int)WhichTossKid.Aokun:
                        MultiSound.Play(new MultiSound.Sound[]
                        {
                        new MultiSound.Sound("tossBoys/redBlueHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/yellowBlueHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/redBlueHigh2", callBeat + 0.5f),
                        new MultiSound.Sound("tossBoys/yellowBlueHigh2", callBeat + 0.5f),
                        });
                        break;
                    case (int)WhichTossKid.Kiiyan:
                        MultiSound.Play(new MultiSound.Sound[]
                        {
                        new MultiSound.Sound("tossBoys/redYellowHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/blueYellowHigh1", callBeat),
                        new MultiSound.Sound("tossBoys/redYellowHigh2", callBeat + 0.5f),
                        new MultiSound.Sound("tossBoys/blueYellowHigh2", callBeat + 0.5f),
                        });
                        break;
                    default:
                        break;
                }
            }


            if (passBallDict.ContainsKey(beat + length))
            {
                ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
                if (passBallDict[beat + length].datamodel == "tossBoys/dual" || passBallDict[beat + length].datamodel == "tossBoys/lightning" || passBallDict[beat + length].datamodel == "tossBoys/blur")
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                    });
                }
                else if (passBallDict[beat + length].datamodel == "tossBoys/pop")
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + length - 1, delegate { GetCurrentReceiver().PopBallPrepare(); })
                    });
                }
            }
            else
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length, delegate { Miss(null); })
                });
            }
        }

        void SetPassBallEvents()
        {
            passBallDict.Clear();
            var passBallEvents = EventCaller.GetAllInGameManagerList("tossBoys", new string[] { "pass", "dual", "pop", "high", "lightning", "blur" });
            for (int i = 0;  i < passBallEvents.Count; i++)
            {
                if (passBallEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    if (passBallDict.ContainsKey(passBallEvents[i].beat)) continue;
                    passBallDict.Add(passBallEvents[i].beat, passBallEvents[i]);
                }
            }
        }

        void DeterminePass(float beat, bool barely)
        {
            var tempLastReceiver = lastReceiver;
            lastReceiver = currentReceiver;
            if (passBallDict.TryGetValue(beat, out var receiver))
            {
                currentReceiver = (WhichTossKid)receiver["who"];
                currentPassType = receiver.datamodel;
                currentEventLength = receiver.length;
            }
            else
            {
                /*
                DynamicBeatmap.DynamicEntity spawnedEntity = new DynamicBeatmap.DynamicEntity();
                spawnedEntity.DynamicData.Add("who", (int)tempLastReceiver);
                spawnedEntity.datamodel = currentPassType;
                passBallDict.Add(beat, spawnedEntity);
                */
                currentReceiver = tempLastReceiver;
            }
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
            if (barely)
            {
                currentBall.anim.DoScaledAnimationAsync("WiggleBall", 0.5f);
            }
            else
            {
                currentBall.anim.DoScaledAnimationAsync("Hit", 0.5f);
            }
            if (passBallDict.ContainsKey(beat + currentEventLength) && passBallDict[beat + currentEventLength].datamodel == "tossBoys/pop")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + currentEventLength - 1, delegate { GetCurrentReceiver().PopBallPrepare(); })
                });
            }
        }

        void PassBall(float beat, float length)
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
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                });
            }
            if (secondBeat == 0.5f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + 3, beat + 1, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        void DualToss(float beat, float length)
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
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                });
            }
            if (secondBeat == 0.25f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + "Low" + 3, beat + 0.5f, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            bool stopSpecial = passBallDict.ContainsKey(beat + length) && passBallDict[beat + length].datamodel is "tossBoys/pass" or "tossBoys/high" or "tossBoys/pop";
            ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), stopSpecial ? JustHitBallUnSpecial : JustHitBall, Miss, Empty);
        }

        void HighToss(float beat, float length)
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
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                });
            }
            if (secondBeat == 0.25f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + "High" + 3, beat + 0.5f, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        void LightningToss(float beat, float length)
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
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length - 1, delegate { DoSpecialBasedOnReceiver(beat + length - 1); })
                });
            }
            MultiSound.Play(soundsToPlay.ToArray());
            bool stopSpecial = passBallDict.ContainsKey(beat + length) && passBallDict[beat + length].datamodel is "tossBoys/pass" or "tossBoys/high" or "tossBoys/pop";
            ScheduleInput(beat, length / 2, GetInputBasedOnTossKid(lastReceiver), stopSpecial ? JustKeepUnSpecial : JustKeep, Miss, Empty);
            ScheduleInput(beat, length, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        void BlurToss(float beat)
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
                            Jukebox.PlayOneShotGame("tossBoys/redPop");
                            break;
                        case WhichTossKid.Aokun:
                            Jukebox.PlayOneShotGame("tossBoys/bluePop");
                            break;
                        case WhichTossKid.Kiiyan:
                            Jukebox.PlayOneShotGame("tossBoys/yellowPop");
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
                            Jukebox.PlayOneShotGame("tossBoys/redPop");
                            break;
                        case WhichTossKid.Aokun:
                            Jukebox.PlayOneShotGame("tossBoys/bluePop");
                            break;
                        case WhichTossKid.Kiiyan:
                            Jukebox.PlayOneShotGame("tossBoys/yellowPop");
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
            Jukebox.PlayOneShotGame("tossBoys/" + GetColorBasedOnTossKid(currentReceiver, false) + "Keep");
            string current = GetColorBasedOnTossKid(currentReceiver, false);
            float beat = caller.timer + caller.startBeat;
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
            Jukebox.PlayOneShotGame("tossBoys/" + GetColorBasedOnTossKid(currentReceiver, false) + "Keep");
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
            Jukebox.PlayOneShotGame("tossBoys/" + GetColorBasedOnTossKid(lastReceiver, false) + "Keep");
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float beat = caller.timer + caller.startBeat;
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

        void JustKeepUnSpecial(PlayerActionEvent caller, float state)
        {
            if (currentBall == null) return;
            specialAo.SetActive(false);
            specialAka.SetActive(false);
            specialKii.SetActive(false);
            currentSpecialKid.crouch = false;
            Jukebox.PlayOneShotGame("tossBoys/" + GetColorBasedOnTossKid(lastReceiver, false) + "Keep");
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float beat = caller.timer + caller.startBeat;
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
            Jukebox.PlayOneShotGame("tossBoys/misshit");
        }

        void Empty(PlayerActionEvent caller) { }
        #endregion

        #region HelperFunctions

        void DoSpecialBasedOnReceiver(float beat)
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
                    Jukebox.PlayOneShotGame("tossBoys/yellowSpecial", beat);
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

        string GetColorBasedOnTossKid(WhichTossKid tossKid, bool capital)
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

        InputType GetInputTypeBasedOnCurrentReceiver()
        {
            return GetInputBasedOnTossKid(currentReceiver);
        }

        InputType GetInputBasedOnTossKid(WhichTossKid tossKid)
        {
            switch (tossKid)
            {
                case WhichTossKid.Akachan:
                    return InputType.STANDARD_DOWN;
                case WhichTossKid.Aokun:
                    return InputType.STANDARD_ALT_DOWN;
                case WhichTossKid.Kiiyan:
                    return InputType.DIRECTION_DOWN;
                default:
                    return InputType.ANY;
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
        #endregion
    }
}

