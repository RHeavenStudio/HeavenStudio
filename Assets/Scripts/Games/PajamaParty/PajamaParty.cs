using HeavenStudio.Util;
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
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("pajamaParty", "Somnophiliac Girl", "fc9ac3", false, false, new List<GameAction>()
                {
                    // both same timing
                    new GameAction("jump (side to middle)", "Side to Middle Jumps")
                    {
                        function = delegate {PajamaParty.instance.DoThreeJump(eventCaller.currentEntity.beat);},
                        defaultLength = 4f,
                        inactiveFunction = delegate {PajamaParty.WarnThreeJump(eventCaller.currentEntity.beat);}
                    },
                    new GameAction("jump (back to front)", "Back to Front Jumps")
                    {
                        function =delegate {PajamaParty.instance.DoFiveJump(eventCaller.currentEntity.beat);}, 
                        defaultLength = 4f, 
                        inactiveFunction = delegate {PajamaParty.WarnFiveJump(eventCaller.currentEntity.beat);}
                    },
                    //idem
                    new GameAction("slumber", "Slumber")
                    {
                        function = delegate {var e = eventCaller.currentEntity; PajamaParty.instance.DoSleepSequence(e.beat, e["toggle"], e["type"]);}, 
                        defaultLength = 8f,
                        parameters = new List<Param>()
                        {
                            new Param("type", PajamaParty.SleepType.Normal, "Sleep Type", "Type of sleep action to use"),
                            new Param("toggle", false, "Alt. Animation", "Use an alternate animation for Mako")
                        }, 
                        inactiveFunction = delegate {var e = eventCaller.currentEntity; PajamaParty.WarnSleepSequence(e.beat, e["toggle"], e["type"]);}
                    },
                    new GameAction("throw", "Throw Pillows")
                    {
                        function = delegate {PajamaParty.instance.DoThrowSequence(eventCaller.currentEntity.beat);}, 
                        defaultLength = 8f,
                        inactiveFunction = delegate {PajamaParty.WarnThrowSequence(eventCaller.currentEntity.beat);}
                    },
                    // todo cosmetic crap
                    // background stuff
                    // do shit with mako's face? (talking?)
                },
                new List<string>() {"ctr", "normal"},
                "ctrpillow", "jp",
                new List<string>() {"en", "jp", "ko"}
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
        public CtrPillowPlayer Mako;
        public GameObject Bed;
        public GameObject MonkeyPrefab;

        [Header("Positions")]
        public Transform SpawnRoot;

        //game scene
        public static PajamaParty instance;
        CtrPillowMonkey[,] monkeys;

        //cues while unoaded
        static float WantThreeJump = Single.MinValue;
        static float WantFiveJump = Single.MinValue;
        static float WantThrowSequence = Single.MinValue;
        static float WantSleepSequence = Single.MinValue;
        static bool WantSleepType = false;
        static int WantSleepAction = (int) PajamaParty.SleepType.Normal;

        public enum SleepType {
            Normal,
            NoAwake,
        }

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
            monkeys = new CtrPillowMonkey[5,5];
            float RADIUS = 2.75f;
            float scale = 1.0f;
            int sorting = 10;
            //set our start position (at Mako + 2*radius to the right)
            Vector3 spawnPos = SpawnRoot.position + new Vector3(-RADIUS*3, 0);
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    //on x-axis we go left to right
                    spawnPos += new Vector3(RADIUS*scale, 0);
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
                spawnPos = SpawnRoot.position - new Vector3(RADIUS*3*scale, -RADIUS/3.75f*(y+1), -RADIUS/5f*(y+1));
                sorting--;
            }
        }

        public override void OnGameSwitch(float beat)
        {
            if (WantThreeJump != Single.MinValue)
            {
                DoThreeJump(WantThreeJump, false);
                WantThreeJump = Single.MinValue;
            }
            if (WantFiveJump != Single.MinValue)
            {
                DoFiveJump(WantFiveJump, false);
                WantFiveJump = Single.MinValue;
            }
            if (WantThrowSequence != Single.MinValue)
            {
                DoThrowSequence(WantThrowSequence, false);
                WantThrowSequence = Single.MinValue;
            }
            if (WantSleepSequence != Single.MinValue)
            {
                DoSleepSequence(WantSleepSequence, WantSleepType, WantSleepAction, false);
                WantSleepSequence = Single.MinValue;
            }
        }

        public void DoThreeJump(float beat, bool doSound = true)
        {
            Mako.ScheduleJump(beat);
            if (doSound)
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("pajamaParty/three1", beat), 
                    new MultiSound.Sound("pajamaParty/three2", beat + 1f),
                    new MultiSound.Sound("pajamaParty/three3", beat + 2f),
                });

            BeatAction.New(Bed, new List<BeatAction.Action>()
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

        public static void WarnThreeJump(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/three1", beat), 
                new MultiSound.Sound("pajamaParty/three2", beat + 1f),
                new MultiSound.Sound("pajamaParty/three3", beat + 2f),
            }, forcePlay:true);
            WantThreeJump = beat;
        }

        public void DoFiveJump(float beat, bool doSound = true)
        {
            Mako.ScheduleJump(beat);
            if (doSound)
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("pajamaParty/five1", beat), 
                    new MultiSound.Sound("pajamaParty/five2", beat + 0.5f),
                    new MultiSound.Sound("pajamaParty/five3", beat + 1f),
                    new MultiSound.Sound("pajamaParty/five4", beat + 1.5f),
                    new MultiSound.Sound("pajamaParty/five5", beat + 2f)
                });

            BeatAction.New(Bed, new List<BeatAction.Action>()
            {
                new BeatAction.Action( beat,        delegate { JumpRow(4, beat); }),
                new BeatAction.Action( beat + 0.5f, delegate { JumpRow(3, beat + 0.5f, 2); }),
                new BeatAction.Action( beat + 1f,   delegate { JumpRow(2, beat + 1f); }),
                new BeatAction.Action( beat + 1.5f, delegate { JumpRow(1, beat + 1.5f, 2); }),
                new BeatAction.Action( beat + 2f,   delegate { JumpRow(0, beat + 2f); }),
            });
        }

        public static void WarnFiveJump(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/five1", beat), 
                new MultiSound.Sound("pajamaParty/five2", beat + 0.5f),
                new MultiSound.Sound("pajamaParty/five3", beat + 1f),
                new MultiSound.Sound("pajamaParty/five4", beat + 1.5f),
                new MultiSound.Sound("pajamaParty/five5", beat + 2f)
            }, forcePlay:true);
            WantFiveJump = beat;
        }

        public void DoThrowSequence(float beat, bool doSound = true)
        {
            Mako.ScheduleThrow(beat);
            if (doSound)
                PlayThrowSequenceSound(beat);

            BeatAction.New(Mako.Player, new List<BeatAction.Action>()
            {
                new BeatAction.Action( beat + 2f, delegate { MonkeyCharge(beat + 2f); } ),
                new BeatAction.Action( beat + 3f, delegate { MonkeyThrow(beat + 3f); } ),
            });
        }

        public static void WarnThrowSequence(float beat)
        {
            PlayThrowSequenceSound(beat, true);
            WantThrowSequence = beat;
        }

        public static void PlayThrowSequenceSound(float beat, bool force = false)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/throw1", beat), 
                new MultiSound.Sound("pajamaParty/throw2", beat + 0.5f),
                new MultiSound.Sound("pajamaParty/throw3", beat + 1f),

                //TODO: change when locales are a thing
                new MultiSound.Sound("pajamaParty/throw4a", beat + 1.5f),

                new MultiSound.Sound("pajamaParty/charge", beat + 2f),
            }, forcePlay: force);
        }

        public void DoSleepSequence(float beat, bool alt = false, int action = (int) PajamaParty.SleepType.Normal, bool doSound = true)
        {
            var cond = Conductor.instance;
            Mako.StartSleepSequence(beat, alt, action);
            MonkeySleep(beat, action);
            if (doSound)
                MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("pajamaParty/siesta1", beat), 
                    new MultiSound.Sound("pajamaParty/siesta2", beat + 0.5f),
                    new MultiSound.Sound("pajamaParty/siesta3", beat + 1f),
                    new MultiSound.Sound("pajamaParty/siesta3", beat + 2.5f),
                    new MultiSound.Sound("pajamaParty/siesta3", beat + 4f)
                });
        }

        public static void WarnSleepSequence(float beat, bool alt = false, int action = (int) PajamaParty.SleepType.Normal)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/siesta1", beat), 
                new MultiSound.Sound("pajamaParty/siesta2", beat + 0.5f),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 1f),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 2.5f),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 4f)
            }, forcePlay: true);
            WantSleepSequence = beat;
            WantSleepType = alt;
            WantSleepAction = action;
        }

        public void DoBedImpact()
        {
            Bed.GetComponent<Animator>().Play("BedImpact", -1, 0);
        }

        public void JumpRow(int row, float beat, int alt = 1)
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

        public void JumpCol(int col, float beat, int alt = 1)
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

        public void MonkeyCharge(float beat)
        {
            foreach (CtrPillowMonkey monkey in monkeys)
            {
                if (monkey != null)
                {
                    monkey.Charge(beat);
                }
            }
        }

        public void MonkeyThrow(float beat)
        {
            foreach (CtrPillowMonkey monkey in monkeys)
            {
                if (monkey != null)
                {
                    monkey.Throw(beat);
                }
            }
        }

        public void MonkeySleep(float beat, int action)
        {
            foreach (CtrPillowMonkey monkey in monkeys)
            {
                if (monkey != null)
                {
                    monkey.ReadySleep(beat, action);
                }
            }
        }
    }
}
