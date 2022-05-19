using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;

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
                new GameAction("throw",                     delegate { }, 8f, false),
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

        //game scene
        public static PajamaParty instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            
        }

        public void DoThreeJump(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/three1", beat), 
                new MultiSound.Sound("pajamaParty/three2", beat + 1f),
                new MultiSound.Sound("pajamaParty/three3", beat + 2f),
            });
        }

        public void DoFiveJump(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/five1", beat), 
                new MultiSound.Sound("pajamaParty/five2", beat + 0.5f),
                new MultiSound.Sound("pajamaParty/five3", beat + 1f),
                new MultiSound.Sound("pajamaParty/five4", beat + 1.5f),
                new MultiSound.Sound("pajamaParty/five5", beat + 2f)
            });
        }

        public void DoSleepSequence(float beat)
        {
            var cond = Conductor.instance;
            Mako.StartSleepSequence(beat);

            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("pajamaParty/siesta1", beat), 
                new MultiSound.Sound("pajamaParty/siesta2", beat + 0.5f),
                new MultiSound.Sound("pajamaParty/siesta3", beat + 1f),
                new MultiSound.Sound("pajamaParty/siesta4", beat + 2.5f),
                new MultiSound.Sound("pajamaParty/siesta5", beat + 4f)
            });

            BeatAction.New(Mako.Player, new List<BeatAction.Action>()
            {
                new BeatAction.Action(
                    beat,
                    delegate { Mako.anim.Play("MakoSleep00", -1, 0); 
                            Mako.anim.speed = 1f / cond.pitchedSecPerBeat;
                        }
                ),
                new BeatAction.Action(
                    beat + 0.5f,
                    delegate { Mako.anim.Play("MakoSleep01", -1, 0); 
                            Mako.anim.speed = 1f;
                        }
                ),
                new BeatAction.Action(
                    beat + 3,
                    delegate { Mako.anim.Play("MakoReadySleep", -1, 0); 
                            Mako.anim.speed = 1f / cond.pitchedSecPerBeat;
                        }
                ),
                //test
                    new BeatAction.Action(
                        beat + 4,
                        delegate { Mako.anim.Play("MakoSleepJust", -1, 0); 
                                Mako.anim.speed = 1f;
                            }
                    ),
                //
            });
        }

        public void DoBedImpact()
        {
            Bed.GetComponent<Animator>().Play("BedImpact", -1, 0);
        }
    }
}
