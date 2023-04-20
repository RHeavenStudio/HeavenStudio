using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

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
                    function = delegate { var e = eventCaller.currentEntity; TossBoys.instance.Dispense(e.beat, e["who"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Aokun, "Receiver", "Who will receive the ball?")
                    }
                },
                new GameAction("pass", "Normal Toss")
                {
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Akachan, "Receiver", "Who will receive the ball?")
                    }
                },
                new GameAction("dual", "Dual Toss")
                {
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Akachan, "Receiver", "Who will receive the ball?")
                    }
                },
                new GameAction("high", "High Toss")
                {
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("who", TossBoys.KidChoice.Kiiyan, "Receiver", "Who will receive the ball?")
                    }
                },
                new GameAction("pop", "Pop Ball")
                {
                    preFunction = delegate { TossBoys.PrePop(eventCaller.currentEntity.beat); },
                    defaultLength = 2f,
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
                }
            });
        }
    }
}
namespace HeavenStudio.Games
{
    using Scripts_TossBoys;
    using System.Windows.Forms;

    public class TossBoys : Minigame
    {
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

        [Header("Properties")]
        WhichTossKid lastReceiver = WhichTossKid.None;
        WhichTossKid currentReceiver = WhichTossKid.None;
        public TossBoysBall currentBall = null;
        Dictionary<float, DynamicBeatmap.DynamicEntity> passBallDict = new Dictionary<float, DynamicBeatmap.DynamicEntity>();
        string currentPassType;
        public static TossBoys instance;
        bool shouldBop = true;
        public GameEvent bop = new GameEvent();
        static List<float> queuedPops = new List<float>();

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (queuedPops.Count > 0) queuedPops.Clear();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedPops.Count > 0)
                {
                    foreach (var pop in queuedPops)
                    {
                        Pop(pop);
                    }
                    queuedPops.Clear();
                }
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

        public void Dispense(float beat, int who)
        {
            if (currentBall != null) return;
            SetPassBallEvents();
            SetReceiver(who);
            GetCurrentReceiver().ShowArrow(beat, 1f);
            Jukebox.PlayOneShotGame("tossBoys/ballStart");
            hatchAnim.Play("HatchOpen");
            currentBall = Instantiate(ballPrefab, transform);

            if (passBallDict.ContainsKey(beat + 2))
            {
                ScheduleInput(beat, 2f, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
                if (passBallDict[beat + 2].datamodel == "tossBoys/dual")
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 1, delegate { DoSpecialBasedOnReceiver(beat + 1); })
                    });
                }
            }
            else
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2f, delegate { Miss(null); })
                });
            }
        }

        void SetPassBallEvents()
        {
            passBallDict.Clear();
            var passBallEvents = EventCaller.GetAllInGameManagerList("tossBoys", new string[] { "pass", "dual", "pop", "high" });
            for (int i = 0;  i < passBallEvents.Count; i++)
            {
                if (passBallEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    if (passBallDict.ContainsKey(passBallEvents[i].beat)) continue;
                    passBallDict.Add(passBallEvents[i].beat, passBallEvents[i]);
                }
            }
        }

        void DeterminePass(float beat)
        {
            var tempLastReceiver = lastReceiver;
            lastReceiver = currentReceiver;
            if (passBallDict.TryGetValue(beat, out var receiver))
            {
                currentReceiver = (WhichTossKid)receiver["who"];
                currentPassType = receiver.datamodel;
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
                    PassBall(beat);
                    break;
                case "tossBoys/dual":
                    DualToss(beat);
                    break;
                case "tossBoys/high":
                    HighToss(beat);
                    break;
                default:
                    break;
            }
        }

        void PassBall(float beat)
        {
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float secondBeat = 1f;
            float secondOffset = 0;
            float thirdOffset = 0;
            switch (last + current)
            {
                case "blueRed":
                case "yellowRed":
                    secondBeat = 0.5f;
                    break;
                case "redYellow":
                    secondBeat = 0.5f;
                    thirdOffset = 0.060f;
                    break;
                default:
                    secondBeat = 1f;
                    break;
            }
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("tossBoys/" + last + current + 1, beat),
                new MultiSound.Sound("tossBoys/" + last + current + 2, beat + secondBeat, 1, 1, false, secondOffset),
            };
            if (passBallDict.ContainsKey(beat + 2) && passBallDict[beat + 2].datamodel == "tossBoys/dual")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 1, delegate { DoSpecialBasedOnReceiver(beat + 1); })
                });
            }
            if (secondBeat == 0.5f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + 3, beat + 1, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            ScheduleInput(beat, 2f, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        void DualToss(float beat)
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
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("tossBoys/" + last + current + "Low" + 1, beat),
                new MultiSound.Sound("tossBoys/" + last + current + "Low" + 2, beat + secondBeat, 1, 1, false, secondOffset),
            };
            if (secondBeat == 0.25f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + "Low" + 3, beat + 0.5f, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            bool stopSpecial = passBallDict.ContainsKey(beat + 1) && passBallDict[beat + 1].datamodel != "tossBoys/dual";
            ScheduleInput(beat, 1f, GetInputTypeBasedOnCurrentReceiver(), stopSpecial ? JustHitBallUnSpecial : JustHitBall, stopSpecial ? MissUnSpecial : Miss, Empty);
        }

        void HighToss(float beat)
        {
            string last = GetColorBasedOnTossKid(lastReceiver, false);
            string current = GetColorBasedOnTossKid(currentReceiver, true);
            float secondBeat = 0.5f;
            float secondOffset = 0;
            float thirdOffset = 0;
            switch (last + current)
            {
                case "yellowRed":
                case "blueRed":
                    secondBeat = 0.25f;
                    break;
                default:
                    secondBeat = 0.5f;
                    break;
            }
            List<MultiSound.Sound> soundsToPlay = new List<MultiSound.Sound>()
            {
                new MultiSound.Sound("tossBoys/" + last + current + "High" + 1, beat),
                new MultiSound.Sound("tossBoys/" + last + current + "High" + 2, beat + secondBeat, 1, 1, false, secondOffset),
            };
            if (passBallDict.ContainsKey(beat + 3) && passBallDict[beat + 3].datamodel == "tossBoys/dual")
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 2, delegate { DoSpecialBasedOnReceiver(beat + 2); })
                });
            }
            if (secondBeat == 0.25f) soundsToPlay.Add(new MultiSound.Sound("tossBoys/" + last + current + "High" + 3, beat + 0.5f, 1, 1, false, thirdOffset));
            MultiSound.Play(soundsToPlay.ToArray());
            ScheduleInput(beat, 3f, GetInputTypeBasedOnCurrentReceiver(), JustHitBall, Miss, Empty);
        }

        public static void PrePop(float beat)
        {
            if (GameManager.instance.currentGame == "tossBoys")
            {
                instance.Pop(beat);
            }
            else
            {
                queuedPops.Add(beat);
            }
        }

        void Pop(float beat)
        {
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.5f, delegate
                {
                    if (currentBall != null) GetCurrentReceiver().PopBallPrepare();
                })
            });
        }

        #region Inputs
        void JustHitBall(PlayerActionEvent caller, float state)
        {
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
                if ((WhichTossKid)passBallDict[caller.startBeat + caller.timer]["who"] == currentReceiver)
                {
                    Miss(null);
                    return;
                }
            }
            if (state >= 1f || state <= -1f)
            {
                GetCurrentReceiver().Barely();
                DeterminePass(caller.timer + caller.startBeat);
                return;
            }
            GetCurrentReceiver().HitBall();
            DeterminePass(caller.timer + caller.startBeat);
        }

        void JustHitBallUnSpecial(PlayerActionEvent caller, float state)
        {
            specialAo.SetActive(false);
            specialAka.SetActive(false);
            specialKii.SetActive(false);
            currentSpecialKid.crouch = false;
            currentSpecialKid = null;
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
                if ((WhichTossKid)passBallDict[caller.startBeat + caller.timer]["who"] == currentReceiver)
                {
                    MissUnSpecial(null);
                    return;
                }
            }
            if (state >= 1f || state <= -1f)
            {
                GetCurrentReceiver().Barely();
                DeterminePass(caller.timer + caller.startBeat);
                return;
            }
            GetCurrentReceiver().HitBall();
            DeterminePass(caller.timer + caller.startBeat);
        }

        void Miss(PlayerActionEvent caller)
        {
            GetCurrentReceiver().Miss();
            Destroy(currentBall.gameObject);
            currentBall = null;
        }

        void MissUnSpecial(PlayerActionEvent caller)
        {
            GetCurrentReceiver().Miss();
            GetCurrentReceiver().crouch = false;
            specialAo.SetActive(false);
            specialAka.SetActive(false);
            specialKii.SetActive(false);
            Destroy(currentBall.gameObject);
            currentBall = null;
        }

        void Empty(PlayerActionEvent caller) { }
        #endregion

        #region HelperFunctions

        void DoSpecialBasedOnReceiver(float beat)
        {
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
                        new MultiSound.Sound("tossBoys/redSpecialCharge", beat + 0.25f),
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
            switch (currentReceiver)
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

