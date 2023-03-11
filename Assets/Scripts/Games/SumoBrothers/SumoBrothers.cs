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
            return new Minigame("sumoBrothers", "Sumo Brothers \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "EDED15", false, false, new List<GameAction>()
            {
                 new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Bop(e["inu"], e["sumo"]); },
                    parameters = new List<Param>()
                    {
                        new Param("inu", true, "Inu Bop", "Whether Inu Sensei bops or not."),
                        new Param("sumo", true, "Brothers Bop", "Whether the Sumo Brothers bop or not."),
                    },
                    defaultLength = 0.5f
                },

                 new GameAction("crouch", "Crouch")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Crouch(e.beat, e.length, e["inu"], e["sumo"]); },
                    parameters = new List<Param>()
                    {
                        new Param("inu", true, "Inu Crouch", "Whether Inu Sensei crouches or not."),
                        new Param("sumo", true, "Brothers Crouch", "Whether the Sumo Brothers crouch or not.")
                    },
                    defaultLength = 1f,
                    resizable = true
                },

                 new GameAction("stompSignal", "Stomp Signal")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.StompSignal(e.beat); },
                    defaultLength = 4f
                },

                 new GameAction("slapSignal", "Slap Signal")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.SlapSignal(e.beat); },
                    defaultLength = 4f
                },

                new GameAction("endPose", "End Pose")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.EndPose(e.beat); },
                    parameters = new List<Param>()
                    {
                        new Param("type", SumoBrothers.PoseType.Crouching, "Pose", "The pose that the Sumo Brothers will make."),
                        new Param("bg", SumoBrothers.BGType.TheGreatWave, "Background", "The background that appears on a successful input.")
                    },
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
        [SerializeField] Animator sumoBrotherP;
        [SerializeField] Animator sumoBrotherG;

        [Header("Properties")]
        static List<queuedSumoInputs> queuedInputs = new List<queuedSumoInputs>();
        public struct queuedSumoInputs
        {
            public float beat;
            public int cue;
            public int poseType;
            public int poseBG;
        }

        private bool goBopSumo;
        private bool goBopInu;

        private bool allowBopSumo;
        private bool allowBopInu;

        private float lastReportedBeat = 0f;

        public static SumoBrothers instance;

        public enum PoseType
        {
            Crouching = 0,
            Crossed = 1,
            Pointing = 2,
            Finale = 3
        }

        public enum BGType
        {
            TheGreatWave = 0,
            OtaniOniji = 1
        }

        // Start is called before the first frame update
        void Awake()
        {
            goBopInu = true;
            goBopSumo = true;
            allowBopInu = true;
            allowBopSumo = true;
            instance = this;

        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
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
                if (goBopSumo && allowBopSumo)
                {
                    sumoBrotherP.DoScaledAnimationAsync("SumoBop", 0.5f);
                    sumoBrotherG.DoScaledAnimationAsync("SumoBop", 0.5f);
                }

            }

            if (PlayerInput.Pressed() && !IsExpectingInputNow())
            {

            }
        }

        public void Bop(bool inu, bool sumo)
        {
            goBopInu = inu;
            goBopSumo = sumo;
        }

        public void StompSignal(float beat)
        {
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("sumoBrothers/stompsignal"); }),
                new BeatAction.Action(beat + 2, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate { Jukebox.PlayOneShotGame("sumoBrothers/stompsignal"); })
            });
        }

        public void SlapSignal(float beat)
        {
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat, delegate { Jukebox.PlayOneShotGame("sumoBrothers/slapsignal"); }),
                new BeatAction.Action(beat + 1, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { Jukebox.PlayOneShotGame("sumoBrothers/slapsignal"); }),
                new BeatAction.Action(beat + 2, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate { Jukebox.PlayOneShotGame("sumoBrothers/slapsignal"); }),
                new BeatAction.Action(beat + 3, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { Jukebox.PlayOneShotGame("sumoBrothers/slapsignal"); })
            });
        }

        public void Crouch(float beat, float length, bool inu, bool sumo)
        {
            if (inu)
            {
                allowBopInu = false;
            }
            if (sumo)
            {
                allowBopSumo = false;
            }
            
            
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
            new BeatAction.Action(beat, delegate { if (inu == true) inuSensei.DoScaledAnimationAsync("InuCrouch", 0.5f); }),
            new BeatAction.Action(beat, delegate { if (sumo == true) sumoBrotherP.DoScaledAnimationAsync("SumoCrouch", 0.5f); }),
            new BeatAction.Action(beat, delegate { if (sumo == true) sumoBrotherG.DoScaledAnimationAsync("SumoCrouch", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { allowBopInu = true; }),
            new BeatAction.Action(beat + length, delegate { allowBopSumo = true; }),
            new BeatAction.Action(beat + length, delegate { if (goBopInu == false) inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (goBopSumo == false) sumoBrotherP.DoScaledAnimationAsync("SumoIdle", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (goBopSumo == false) sumoBrotherG.DoScaledAnimationAsync("SumoIdle", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (goBopSumo == true) sumoBrotherP.DoScaledAnimationAsync("SumoBop", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (goBopSumo == true) sumoBrotherG.DoScaledAnimationAsync("SumoBop", 0.5f); })
            });

        }

        public void EndPose(float beat)
        {
            var cond = Conductor.instance;
            #region Tweet
                var loopAmount = Math.Round((3 * cond.secPerBeat - 0.208) / 0.395);
                var beatsPerSecond = cond.songBpm / 60;
                var soundBeatLength = beatsPerSecond * 0.395;

                List<MultiSound.Sound> sound = new List<MultiSound.Sound>();
                sound.Add(new MultiSound.Sound("sumoBrothers/posesignalBegin", beat));

                for (int i = 1; i <= loopAmount; i++)
                {
                    sound.Add(new MultiSound.Sound("sumoBrothers/posesignalLoop", i * (float)soundBeatLength + beat - (float)soundBeatLength + ((float)beatsPerSecond * 0.208f))); 
                }

                MultiSound.Play(sound.ToArray());

                var tweetLength = (float)loopAmount * (float)soundBeatLength - (float)soundBeatLength + ((float)beatsPerSecond * 0.208f);
                print(tweetLength);
                
            #endregion

            //SumoBrothers.instance.EndPoseCheck(beat);

            

            allowBopInu = false;
            inuSensei.DoScaledAnimationAsync("InuTweet", 0.5f);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 3, delegate { allowBopInu = true; }),
                new BeatAction.Action(beat + 3, delegate { if (goBopInu == false) inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); })
            });

        }

        void EndPoseCheck(float beat)
        {

            //ScheduleInput(beat, 5f, InputType.STANDARD_ALT_DOWN, SuccessEndPose, MissEndPose, Nothing);

        }

        void SuccessEndPose(PlayerActionEvent caller)
        {



        }

        void MissEndPose(PlayerActionEvent caller)
        {



        }

        void Nothing(PlayerActionEvent caller) { }

    }
}
