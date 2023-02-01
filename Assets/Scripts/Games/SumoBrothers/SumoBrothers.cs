using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrSumouLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("sumoBrothers", "Sumo Brothers \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "0058CE", false, false, new List<GameAction>()
            {
                 new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Bop(e["toggle"], e["toggle1"]); },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Inu Bop", "Inu Sensei bops or not."),
                        new Param("toggle1", true, "Sumo Bop", "Sumo Brothers bop or not."),
                    },
                    defaultLength = 1f
                },

                 new GameAction("inuCrouch", "Inu Crouch")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Crouch(e["toggle"]); },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Crouch", "Inu Sensei crouches or not.")
                    },
                    defaultLength = 1f
                },

                new GameAction("endPose", "End Pose")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.Pose(e.beat); },
                    defaultLength = 5f
                }

            });
        }
    }
}

namespace HeavenStudio.Games
{
    //    using Scripts_SumoBrothers;
    public class SumoBrothers : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator inuSensei;

        public static SumoBrothers instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Bop(bool inu, bool sumo)
        {
            if (inu)
            {
                inuSensei.DoScaledAnimationAsync("InuBop", 0.5f);

            }
            if (sumo) 
            {

            }
        }

        public void Crouch(bool crouch)
        {
            if (crouch)
            {
                inuSensei.DoScaledAnimationAsync("InuCrouch", 0.5f);

            }
            else
            {
                inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f);

            }
        }

        public static void Pose(float beat)
        {
            var cond = Conductor.instance;
            var cueLength = 60 / cond.songBpm * 3;
            var loopAmount = Math.Ceiling((cueLength - 0.208) / 0.395);
            var beatsPerSecond = cond.songBpm / 60;
            var soundBeatLength = beatsPerSecond * 0.395;
            
            print(cueLength + " and " + loopAmount);

            List<MultiSound.Sound> sound = new List<MultiSound.Sound>();
            sound.Add(new MultiSound.Sound("sumoBrothers/posesignalBegin", beat));

            for (int i = 1; i <= loopAmount; i++)
            {
                sound.Add(new MultiSound.Sound("sumoBrothers/posesignalLoop", i * (float)soundBeatLength + beat - (float)soundBeatLength + ((float)beatsPerSecond * 0.208f))); 
            }
            print(sound.ToArray());
            MultiSound.Play(sound.ToArray());



        }
    }
}
