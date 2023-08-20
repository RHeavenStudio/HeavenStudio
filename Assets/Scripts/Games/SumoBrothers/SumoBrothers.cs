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
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Bop(e["inuT"], e["sumoT"]); },
                    parameters = new List<Param>()
                    {
                        new Param("inuT", true, "Inu Sensei", "Whether Inu Sensei bops or not."),
                        new Param("sumoT", true, "Sumo Brothers", "Whether the Sumo Brothers bop or not.")
                    },
                    defaultLength = 0.5f
                },

                 new GameAction("crouch", "Crouch")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Crouch(e.beat, e.length, e["inuT"], e["sumoT"]); },
                    parameters = new List<Param>()
                    {
                        new Param("inuT", true, "Inu Sensei", "Whether Inu Sensei crouches or not."),
                        new Param("sumoT", true, "Sumo Brothers", "Whether the Sumo Brothers crouch or not.")
                    },
                    defaultLength = 1f,
                    resizable = true
                },

                 new GameAction("stompSignal", "Stomp")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.StompSignal(e.beat, e["mute"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute", "Disables the sound and animations that cue for the transition to stomping.")
                    },
                    defaultLength = 4f
                },

                 new GameAction("slapSignal", "Slap")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.SlapSignal(e.beat, e["mute"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute", "Disables the sound and animations that cue for the transition to slapping.")
                    },
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

        private double lastReportedBeat = 0f;

        public static SumoBrothers instance;

        public enum PoseType
        {
            Crouching,
            Crossed,
            Pointing,
            Finale
        }

        public enum BGType
        {
            TheGreatWave,
            OtaniOniji
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
                // Bop Code - Inu Sensei
                if (allowBopInu)
                {
                    if (goBopInu)
                    {
                        // Bop
                        inuSensei.DoScaledAnimationAsync("InuBop", 0.5f);
                    }
                    else
                    {
                        // Reset to Idle if Bop is off
                        inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f);
                    }

                }
                // Bop Code - Sumo Brothers
                if (allowBopSumo)
                {
                    if(goBopSumo)
                    {
                        // Bop
                        sumoBrotherP.DoScaledAnimationAsync("SumoBop", 0.5f);
                        sumoBrotherG.DoScaledAnimationAsync("SumoBop", 0.5f);
                    }
                    else
                    {
                        // Reset to Idle if Bop is off
                        sumoBrotherP.DoScaledAnimationAsync("SumoIdle", 0.5f);
                        sumoBrotherG.DoScaledAnimationAsync("SumoIdle", 0.5f);
                    }
                    
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

        public void StompSignal(double beat, bool mute)
        {
            if (mute == false)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                    new BeatAction.Action(beat + 2, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); })
                });

                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("sumoBrothers/stompsignal", beat),
                    new MultiSound.Sound("sumoBrothers/stompsignal", beat + 2f)
                }, forcePlay: true);
            
            }

        }

        public void SlapSignal(double beat, bool mute)
        {
            if (mute == false)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { inuSensei.DoScaledAnimationAsync("InuBounce", 0.5f); })
                });

                MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("sumoBrothers/slapsignal", beat),
                new MultiSound.Sound("sumoBrothers/slapsignal", beat + 1f),
                new MultiSound.Sound("sumoBrothers/slapsignal", beat + 2f),
                new MultiSound.Sound("sumoBrothers/slapsignal", beat + 3f)
                }, forcePlay: true);
            }

        }

        public void Crouch(double beat, float length, bool inu, bool sumo)
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
            new BeatAction.Action(beat + length - 0.001, delegate { allowBopInu = true; }),
            new BeatAction.Action(beat + length - 0.001, delegate { allowBopSumo = true; }),
            new BeatAction.Action(beat + length, delegate { if (goBopSumo == true) sumoBrotherP.DoScaledAnimationAsync("SumoBop", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (goBopSumo == true) sumoBrotherG.DoScaledAnimationAsync("SumoBop", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); })
            });

        }

        public void EndPose(double beat)
        {
            var cond = Conductor.instance;
            #region Old Goofy as Hell Tweet Code
            /*    var loopAmount = Math.Round((3 * cond.secPerBeat - 0.208) / 0.395);
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
                print(tweetLength);*/

            #endregion

            ScheduleInput(beat, 4f, InputType.STANDARD_ALT_DOWN, PoseHit, PoseMiss, Nothing);

            var tweet = SoundByte.PlayOneShotGame("sumoBrothers/posesignal", -1, 1f, 1f, true);
            tweet.SetLoopParams(beat + 3, 0.05f);

            allowBopInu = false;
            inuSensei.DoScaledAnimationAsync("InuTweet", 0.5f);

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 3, delegate { allowBopInu = true; }),
                new BeatAction.Action(beat + 3, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); })
            });

        }

        void PoseHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("nearmiss");
            }
            else
            {

            }
            SoundByte.PlayOneShotGame("sumoBrothers/pose");


        }

        void PoseMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("sumoBrothers/miss");


        }

        void Nothing(PlayerActionEvent caller) { }

    }
}
