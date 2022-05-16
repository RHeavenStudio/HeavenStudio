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
                new GameAction("jump (side to middle)",     delegate { }, 4f, true),
                new GameAction("jump (back to front)",      delegate { }, 4f, true),
                //idem
                new GameAction("slumber",                   delegate {PajamaParty.instance.DoSleepSequence(eventCaller.currentEntity.beat);}, 8f, true),
                new GameAction("throw",                     delegate { }, 8f, true),
                //cosmetic
                new GameAction("open / close background",   delegate { }, 2f, true),
                // do shit with mako's face?
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

        public void DoSleepSequence(float beat)
        {
            var cond = Conductor.instance;
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
                // test
                new BeatAction.Action(
                    beat + 4f,
                    delegate { Mako.anim.Play("MakoSleepJust", -1, 0); 
                            Mako.anim.speed = 1f; 
                        }
                ),
            });
        }

        public void DoBedImpact()
        {
            Bed.GetComponent<Animator>().Play("BedImpact", -1, 0);
        }
    }
}
