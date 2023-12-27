using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrPillowLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("pajamaParty", "Pajama Party", "fc9ac3", false, false, new List<GameAction>()
                {
                    new GameAction("bop", "Bop")
                    {
                        function = delegate { var e = eventCaller.currentEntity; PajamaParty.instance.Bop(e.beat, e.length, e["bop"], e["autoBop"]); },
                        parameters = new List<Param>()
                        {
                            new Param("bop", true, "Keep Bopping", "Should Mako and the monkeys bop for the duration of the block?"),
                            new Param("autoBop", false, "Keep Bopping (Auto)", "Should Mako and the monkeys bop indefinitely?"),
                        },
                        resizable = true,
                    },
                    // both same timing
                    new GameAction("jump (side to middle)", "Side to Middle Jumps")
                    {
                        function = delegate { PajamaParty.instance.DoThreeJump(eventCaller.currentEntity.beat); },
                        defaultLength = 4f,
                        inactiveFunction = delegate { PajamaParty.WarnThreeJump(eventCaller.currentEntity.beat); }
                    },
                    new GameAction("jump (back to front)", "Back to Front Jumps")
                    {
                        function = delegate { PajamaParty.instance.DoFiveJump(eventCaller.currentEntity.beat); },
                        defaultLength = 4f,
                        inactiveFunction = delegate { PajamaParty.WarnFiveJump(eventCaller.currentEntity.beat); }
                    },
                    //idem
                    new GameAction("slumber", "Slumber")
                    {
                        function = delegate { var e = eventCaller.currentEntity; PajamaParty.instance.DoSleepSequence(e.beat, e["toggle"], e["type"]); },
                        defaultLength = 8f,
                        parameters = new List<Param>()
                        {
                            new Param("type", PajamaParty.SleepType.Normal, "Sleep Type", "Type of sleep action to use"),
                            new Param("toggle", false, "Alt. Animation", "Use an alternate animation for Mako")
                        },
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; PajamaParty.WarnSleepSequence(e.beat, e["toggle"], e["type"]); }
                    },
                    new GameAction("throw", "Throw Pillows")
                    {
                        function = delegate { var e = eventCaller.currentEntity; PajamaParty.instance.DoThrowSequence(e.beat, high: e["high"]); },
                        defaultLength = 8f,
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; PajamaParty.WarnThrowSequence(e.beat, e["high"]); },
                        parameters = new List<Param>()
                        {
                            new Param("high", false, "Change Costumes", "Change character costumes"),
                        },
                    },
                    new GameAction("open background", "Open / Close Background")
                    {
                        function = delegate { var e = eventCaller.currentEntity; PajamaParty.instance.OpenBackground(e.beat, e.length, e["instant"]); },
                        defaultLength = 4f,
                        parameters = new List<Param>()
                        {
                            new Param("instant", false, "Instant", "Instantly open / close the background"),
                        },
                    },
                    new GameAction("instant slumber", "Instant Slumber")
                    {
                        function = delegate { var e = eventCaller.currentEntity; PajamaParty.instance.DoInstantSleep(e.beat + e.length - 1, e["type"]); },
                        defaultLength = 0.5f,
                        inactiveFunction = delegate { var e = eventCaller.currentEntity; PajamaParty.WarnInstantSleep(e.beat, e.length, e["type"]); },
                        resizable = true,
                        parameters = new List<Param>()
                        {
                            new Param("type", PajamaParty.SleepType.Normal, "Sleep Type", "Type of sleep action to use"),
                        },
                        priority = 5,
                    },
                    // todo cosmetic crap
                    // background stuff
                    // do shit with mako's face? (talking?)
                },
                new List<string>() { "ctr", "normal" }
                // "ctrpillow", "jp",
                // new List<string>() {"en", "jp", "ko"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_PajamaParty;
    public class PajamaParty : Minigame
    {
        [Header("Objects")]
        [SerializeField] CtrPillowPlayer Mako;
        [SerializeField] GameObject Bed;
        [SerializeField] GameObject MonkeyPrefab;
        [SerializeField] GameObject Castle;
        [SerializeField] Animator BgAnimator;

        [Header("Positions")]
        [SerializeField] Transform SpawnRoot;
        [SerializeField] float HighCameraHeight;

        //game scene
        public static PajamaParty instance;
        CtrPillowMonkey[,] monkeys;
        double cameraHighStart = double.MaxValue;
        bool bgState, highState;

        //cues while unoaded
        static double WantThreeJump = double.MinValue;
        static double WantFiveJump = double.MinValue;
        static double WantThrowSequence = double.MinValue;
        static bool WantThrowHigh = false;
        static double WantSleepSequence = double.MinValue;
        static double WantInstantSleep = double.MinValue;
        static bool WantSleepType = false;
        static int WantSleepAction = (int)PajamaParty.SleepType.Normal;
        static int WantInstantSleepAction = (int)PajamaParty.SleepType.Normal;

        public enum SleepType
        {
            Normal,
            NoAwake,
        }

        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltStart);
        }

        protected static bool IA_PadAltRelease(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltRelease(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }

        public static PlayerInput.InputAction InputAction_AltStart =
            new("CtrPillowAltStart", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_AltFinish =
            new("CtrPillowAltFinish", new int[] { IAAltUpCat, IAFlickCat, IAAltUpCat },
            IA_PadAltRelease, IA_TouchFlick, IA_BatonAltRelease);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("CtrPillowTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        void Awake()
        {
            instance = this;

            //spawn monkeys
            // is 5x5 grid with row 0, col 2 being empty (the player)
            // m  m  m  m  m
            // m  m  m  m  m
            // m  m  m  m  m
            // m  m  m  m  m
            // m  m  P  m  m
            monkeys = new CtrPillowMonkey[5, 5];
            float RADIUS = 2.75f;
            float scale = 1.0f;
            int sorting = 10;
            //set our start position (at Mako + 2*radius to the right)
            Vector3 spawnPos = SpawnRoot.position + new Vector3(-RADIUS * 3, 0);
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    //on x-axis we go left to right
                    spawnPos += new Vector3(RADIUS * scale, 0);
                    if (!(y == 0 && x == 2)) //don't spawn at the player's position
                    {
                        GameObject mobj = Instantiate(MonkeyPrefab, SpawnRoot.parent);
                        CtrPillowMonkey monkey = mobj.GetComponent<CtrPillowMonkey>();
                        mobj.GetComponent<SortingGroup>().sortingOrder = sorting;
                        mobj.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, spawnPos.z);
                        mobj.transform.localScale = new Vector3(scale, scale);
                        monkey.row = y;
                        monkey.col = x;
                        monkeys[x, y] = monkey;
                    }
                }
                // on the y-axis we go front to back (player to the rear)
                scale -= 0.1f;
                spawnPos = SpawnRoot.position - new Vector3(RADIUS * 3 * scale, -RADIUS / 3.75f * (y + 1), -RADIUS / 5f * (y + 1));
                sorting--;
            }
        }

        void Start()
        {
            Castle.SetActive(false);
        }

        void Update()
        {
            var cond = Conductor.instance;
            Vector3 additional = Vector3.zero;
            if (cond.songPositionInBeatsAsDouble >= cameraHighStart && cond.songPositionInBeatsAsDouble < cameraHighStart + 4)
            {
                float prog = cond.GetPositionFromBeat(cameraHighStart, 4, true);
                float yMul = prog * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                additional.y = yWeight * HighCameraHeight;
            }
            GameCamera.AdditionalPosition = additional;
        }

        public override void OnGameSwitch(double beat)
        {
            if (WantThreeJump != double.MinValue)
            {
                DoThreeJump(WantThreeJump, false);
                WantThreeJump = double.MinValue;
            }
            if (WantFiveJump != double.MinValue)
            {
                DoFiveJump(WantFiveJump, false);
                WantFiveJump = double.MinValue;
            }
            if (WantThrowSequence != double.MinValue)
            {
                DoThrowSequence(WantThrowSequence, false);
                WantThrowSequence = double.MinValue;
            }
            if (WantSleepSequence != double.MinValue)
            {
                DoSleepSequence(WantSleepSequence, WantSleepType, WantSleepAction, false);
                WantSleepSequence = double.MinValue;
            }
            if (WantInstantSleep != double.MinValue)
            {
                DoInstantSleep(WantInstantSleep, WantInstantSleepAction);
                WantInstantSleep = double.MinValue;
            }
        }

        public void Bop(double beat, double length, bool doesBop, bool autoBop)
        {
            Mako.shouldBop = autoBop;
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    if (!(y == 0 && x == 2)) monkeys[x, y].shouldBop = autoBop;
                }
            }

            if (doesBop)
            {
                var actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate
                    {
                        Mako.anim.DoScaledAnimationAsync("MakoBeat", 0.5f);
                        for (int y = 0; y < 5; y++)
                        {
                            for (int x = 0; x < 5; x++)
                            {
                                if (!(y == 0 && x == 2)) monkeys[x, y].anim.DoScaledAnimationAsync("MonkeyBeat", 0.5f);
                            }
                        }
                    }));
                }
                BeatAction.New(this, actions);
            }
        }

        public void DoThreeJump(double beat, bool doSound = true)
        {
            Mako.ScheduleJump(beat);
            if (doSound)
                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("pajamaParty/three1", beat),
                    new MultiSound.Sound("pajamaParty/three2", beat + 1),
                    new MultiSound.Sound("pajamaParty/three3", beat + 2),
                });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(
                    beat,
                    delegate {
                        JumpCol(0, beat);
                        JumpCol(4, beat);
                    }
                ),
                new BeatAction.Action(
                    beat + 1,
                    delegate {
                        JumpCol(1, beat + 1, 3);
                        JumpCol(3, beat + 1, 3);
                    }
                ),
                new BeatAction.Action(
                    beat + 2,
                    delegate {
                        JumpCol(2, beat + 2);
                    }
                ),
            });
        }

        public static void WarnThreeJump(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("pajamaParty/three1", beat),
                new MultiSound.Sound("pajamaParty/three2", beat + 1),
                new MultiSound.Sound("pajamaParty/three3", beat + 2),
            }, forcePlay: true);
            WantThreeJump = beat;
        }

        public void DoFiveJump(double beat, bool doSound = true)
        {
            Mako.ScheduleJump(beat);
            if (doSound)
                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("pajamaParty/five1", beat),
                    new MultiSound.Sound("pajamaParty/five2", beat + 0.5),
                    new MultiSound.Sound("pajamaParty/five3", beat + 1),
                    new MultiSound.Sound("pajamaParty/five4", beat + 1.5),
                    new MultiSound.Sound("pajamaParty/five5", beat + 2)
                });

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action( beat,        delegate { JumpRow(4, beat); }),
                new BeatAction.Action( beat + 0.5, delegate { JumpRow(3, beat + 0.5, 2); }),
                new BeatAction.Action( beat + 1,   delegate { JumpRow(2, beat + 1); }),
                new BeatAction.Action( beat + 1.5, delegate { JumpRow(1, beat + 1.5, 2); }),
                new BeatAction.Action( beat + 2,   delegate { JumpRow(0, beat + 2); }),
            });
        }

        public static void WarnFiveJump(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("pajamaParty/five1", beat),
                new MultiSound.Sound("pajamaParty/five2", beat + 0.5),
                new MultiSound.Sound("pajamaParty/five3", beat + 1),
                new MultiSound.Sound("pajamaParty/five4", beat + 1.5),
                new MultiSound.Sound("pajamaParty/five5", beat + 2)
            }, forcePlay: true);
            WantFiveJump = beat;
        }

        public void DoThrowSequence(double beat, bool doSound = true, bool high = false)
        {
            Mako.ScheduleThrow(beat);
            if (doSound)
                PlayThrowSequenceSound(beat);

            if (high)
            {
                cameraHighStart = beat + 3;
            }

            BeatAction.New(Mako, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 2, delegate { MonkeyCharge(beat + 2); } ),
                new BeatAction.Action(beat + 3, delegate { MonkeyThrow(beat + 3); } ),
            });
        }

        public static void WarnThrowSequence(double beat, bool high = false)
        {
            PlayThrowSequenceSound(beat, true);
            WantThrowSequence = beat;
            WantThrowHigh = high;
        }

        public static void PlayThrowSequenceSound(double beat, bool force = false)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("pajamaParty/throw1", beat),
                new MultiSound.Sound("pajamaParty/throw2", beat + 0.5),
                new MultiSound.Sound("pajamaParty/throw3", beat + 1),

                //TODO: change when locales are a thing
                new MultiSound.Sound("pajamaParty/throw4a", beat + 1.5),

                new MultiSound.Sound("pajamaParty/charge", beat + 2),
            }, forcePlay: force);
        }

        public void DoSleepSequence(double beat, bool alt = false, int action = (int)PajamaParty.SleepType.Normal, bool doSound = true)
        {
            Mako.StartSleepSequence(beat, alt, action);
            MonkeySleep(beat, action);
            if (doSound)
                MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound("pajamaParty/siesta1", beat),
                    new MultiSound.Sound("pajamaParty/siesta2", beat + 0.5),
                    new MultiSound.Sound("pajamaParty/siesta3", beat + 1),
                    new MultiSound.Sound("pajamaParty/siesta3", beat + 2.5),
                    new MultiSound.Sound("pajamaParty/siesta3", beat + 4)
                });
        }

        public static void WarnSleepSequence(double beat, bool alt = false, int action = (int)PajamaParty.SleepType.Normal)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("pajamaParty/siesta1", beat),
                new MultiSound.Sound("pajamaParty/siesta2", beat + 0.5),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 1),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 2.5),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 4)
            }, forcePlay: true);
            WantSleepSequence = beat;
            WantSleepType = alt;
            WantSleepAction = action;
        }

        public void DoInstantSleep(double deslumber, int action)
        {
            Mako.anim.Play("MakoSleepJust", -1, 1);
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    if (!(y == 0 && x == 2)) monkeys[x, y].anim.Play("MonkeySleep02", -1, 1);
                }
            }

            if (action == 1) return;
            BeatAction.New(this, new List<BeatAction.Action>() {
                new BeatAction.Action(deslumber, delegate {
                    Mako.anim.DoScaledAnimationAsync("MakoAwake", 0.5f);
                    SoundByte.PlayOneShotGame("pajamaParty/siestaDone");
                    for (int y = 0; y < 5; y++) {
                        for (int x = 0; x < 5; x++) {
                            if (!(y == 0 && x == 2)) monkeys[x, y].anim.DoScaledAnimationAsync("MonkeyAwake", 0.5f);
                        }
                    }
                }),
            });
        }

        public static void WarnInstantSleep(double beat, double length, int action)
        {
            WantInstantSleep = beat + length - 1;
            WantInstantSleepAction = action;
        }

        public void DoBedImpact()
        {
            Bed.GetComponent<Animator>().Play("BedImpact", -1, 0);
        }

        public void JumpRow(int row, double beat, int alt = 1)
        {
            if (row > 4 || row < 0)
            {
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                if (!(i == 2 && row == 0))
                {
                    monkeys[i, row].Jump(beat, alt);
                }
            }
        }

        public void JumpCol(int col, double beat, int alt = 1)
        {
            if (col > 4 || col < 0)
            {
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                if (!(col == 2 && i == 0))
                {
                    monkeys[col, i].Jump(beat, alt);
                }
            }
        }

        public void MonkeyCharge(double beat)
        {
            foreach (CtrPillowMonkey monkey in monkeys)
            {
                if (monkey != null)
                {
                    monkey.Charge(beat);
                }
            }
        }

        public void MonkeyThrow(double beat)
        {
            foreach (CtrPillowMonkey monkey in monkeys)
            {
                if (monkey != null)
                {
                    monkey.Throw(beat);
                }
            }
        }

        public void MonkeySleep(double beat, int action)
        {
            foreach (CtrPillowMonkey monkey in monkeys)
            {
                if (monkey != null)
                {
                    monkey.ReadySleep(beat, action);
                }
            }
        }

        public void OpenBackground(double beat, double length, bool instant = false)
        {
            bgState = !bgState;
            if (instant)
            {
                BgAnimator.Play(bgState ? "SlideOpen" : "SlideClose", -1, 1);
                BgAnimator.speed = 0;
            }
            else
            {
                BgAnimator.DoScaledAnimationAsync(bgState ? "SlideOpen" : "SlideClose", (float)(1.0 / length));
            }
        }
    }
}
