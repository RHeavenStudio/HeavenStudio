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
                        new Param("toggle", true, "Inu Bop", "Whether Inu Sensei bops or not."),
                        new Param("toggle1", true, "Brothers Bop", "Whether the Sumo Brothers bop or not."),
                    },
                    defaultLength = 0.5f
                },

                 new GameAction("crouch", "Crouch")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Crouch(e.beat, e.length, e["toggle"], e["toggle1"]); },
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Inu Crouch", "Whether Inu Sensei crouches or not."),
                        new Param("toggle1", true, "Brothers Crouch", "Whether the Sumo Brothers crouch or not.")
                    },
                    defaultLength = 1f,
                    resizable = true
                },

                new GameAction("endPose", "End Pose")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Pose(e.beat); },
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

        [Header("Properties")]
        public bool goBopSumo;
        public bool goBopInu;
        public bool allowBopSumo;
        public bool allowBopInu;
        private float lastReportedBeat = 0f;

        public static SumoBrothers instance;

        // Start is called before the first frame update
        void Awake()
        {
            goBopInu = true;
            goBopSumo = true;
            allowBopInu = true;
            allowBopSumo = true;
            instance = this;

        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

            if (cond.ReportBeat(ref lastReportedBeat))
            {
                if (goBopInu && allowBopInu)
                {
                    inuSensei.DoScaledAnimationAsync("InuBop", 0.5f);

                }

            }
        }

        public void Bop(bool inu, bool sumo)
        {
            goBopInu = inu;
            goBopSumo = sumo;
        }

        public void Crouch(float beat, float length, bool inu, bool sumo)
        {
            allowBopInu = false;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
            new BeatAction.Action(beat, delegate { if (inu == true) inuSensei.DoScaledAnimationAsync("InuCrouch", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { allowBopInu = true; }),
            new BeatAction.Action(beat + length, delegate { if (goBopInu == false) inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); })
            });

        }

        public void Pose(float beat)
        {
            var cond = Conductor.instance;
            var cueLength = 3 * cond.secPerBeat;
            print(cond.pitchedSecPerBeat);
            var loopAmount = Math.Round((cueLength - 0.208) / 0.395);
            var beatsPerSecond = cond.songBpm / 60;
            var soundBeatLength = beatsPerSecond * 0.395;
            

            List<MultiSound.Sound> sound = new List<MultiSound.Sound>();
            sound.Add(new MultiSound.Sound("sumoBrothers/posesignalBegin", beat));

            for (int i = 1; i <= loopAmount; i++)
            {
                sound.Add(new MultiSound.Sound("sumoBrothers/posesignalLoop", i * (float)soundBeatLength + beat - (float)soundBeatLength + ((float)beatsPerSecond * 0.208f))); 
            }

            MultiSound.Play(sound.ToArray());

            allowBopInu = false;
            inuSensei.DoScaledAnimationAsync("InuTweet", 0.5f);
            var tweetLength = (float)loopAmount * (float)soundBeatLength - (float)soundBeatLength + ((float)beatsPerSecond * 0.208f);
            print(tweetLength);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() { 
                new BeatAction.Action(beat + tweetLength + 1, delegate { allowBopInu = true; }),
                new BeatAction.Action(beat + tweetLength + 1, delegate { if (goBopInu == false) inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f); }),
                new BeatAction.Action(beat + tweetLength + 1, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); })
            });

            


        }
    }
}
