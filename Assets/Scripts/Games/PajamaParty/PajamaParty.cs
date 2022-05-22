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
            return new Minigame("pajamaParty", "Pajama Party \n<color=#eb5454>[WIP don't use]</color>", "000000", false, false, new List<GameAction>()
            {
                // both same timing
                new GameAction("jump (side to middle)",     delegate {PajamaParty.instance.DoThreeJump(eventCaller.currentEntity.beat);}, 4f, false),
                new GameAction("jump (back to front)",      delegate {PajamaParty.instance.DoFiveJump(eventCaller.currentEntity.beat);}, 4f, false),
                //idem
                new GameAction("slumber",                   delegate {PajamaParty.instance.DoSleepSequence(eventCaller.currentEntity.beat);}, 8f, false),
                new GameAction("throw",                     delegate {PajamaParty.instance.DoThrowSequence(eventCaller.currentEntity.beat);}, 8f, false),
                //cosmetic
                new GameAction("open / close background",   delegate { }, 2f, true),
                // do shit with mako's face? (talking?)
            });
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
                        monkeys[x, y] = monkey;
                    }
                }
                // on the y-axis we go front to back (player to the rear)
                scale -= 0.1f;
                spawnPos = SpawnRoot.position - new Vector3(RADIUS*3*scale, -RADIUS/3.75f*(y+1), -RADIUS/5f*(y+1));
                sorting--;
            }
        }

        void Update()
        {
            
        }

        public void DoThreeJump(float beat)
        {
            Mako.ScheduleJump(beat);
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
                        JumpCol(1, beat + 1);
                        JumpCol(3, beat + 1);
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

        public void DoFiveJump(float beat)
        {
            Mako.ScheduleJump(beat);
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
                new BeatAction.Action( beat + 0.5f, delegate { JumpRow(3, beat + 0.5f); }),
                new BeatAction.Action( beat + 1f,   delegate { JumpRow(2, beat + 1f); }),
                new BeatAction.Action( beat + 1.5f, delegate { JumpRow(1, beat + 1.5f); }),
                new BeatAction.Action( beat + 2f,   delegate { JumpRow(0, beat + 2f); }),
            });
        }

        public void DoThrowSequence(float beat)
        {
            Mako.ScheduleThrow(beat);
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/throw1", beat), 
                new MultiSound.Sound("pajamaParty/throw2", beat + 0.5f),
                new MultiSound.Sound("pajamaParty/throw3", beat + 1f),
                //TODO: change when locales are a thing
                //new MultiSound.Sound("pajamaParty/en/throw4a", beat + 1.5f),    //will only play if this clip exists (aka just en)
                new MultiSound.Sound("pajamaParty/charge", beat + 2f),
            });

            BeatAction.New(Mako.Player, new List<BeatAction.Action>()
            {
                new BeatAction.Action( beat + 2f, delegate { MonkeyCharge(beat + 2f); } ),
                new BeatAction.Action( beat + 3f, delegate { MonkeyThrow(beat + 3f); } ),
            });
        }

        public void DoSleepSequence(float beat)
        {
            var cond = Conductor.instance;
            Mako.StartSleepSequence(beat);
            MonkeySleep(beat);

            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/siesta1", beat), 
                new MultiSound.Sound("pajamaParty/siesta2", beat + 0.5f),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 1f),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 2.5f),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 4f)
            });
        }

        public void DoBedImpact()
        {
            Bed.GetComponent<Animator>().Play("BedImpact", -1, 0);
        }

        public void JumpRow(int row, float beat)
        {
            if (row > 4 || row < 0)
            {
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                if (!(i == 2 && row == 0))
                {
                    monkeys[i, row].Jump(beat);
                }
            }
        }

        public void JumpCol(int col, float beat)
        {
            if (col > 4 || col < 0)
            {
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                if (!(col == 2 && i == 0))
                {
                    monkeys[col, i].Jump(beat);
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

        public void MonkeySleep(float beat)
        {
            foreach (CtrPillowMonkey monkey in monkeys)
            {
                if (monkey != null)
                {
                    monkey.ReadySleep(beat);
                }
            }
        }
    }
}
