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
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.Bop(e.beat, e.length, e["bopInu"], e["bopSumo"], e["bopInuAuto"], e["bopSumoAuto"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bopInu", true, "Bop Inu", "Whether Inu Sensei should bop or not."),
                        new Param("bopSumo", true, "Bop Sumo", "Whether the Sumo Brothers should bop or not."),
                        new Param("bopInuAuto", false, "Bop Inu (Auto)", "Whether Inu Sensei should bop automatically or not."),
                        new Param("bopSumoAuto", false, "Bop Sumo (Auto)", "Whether the Sumo Brothers should bop automatically or not."),
                    },
                    defaultLength = 1f,
                    resizable = true
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

                 new GameAction("stompSignal", "Stomp Signal")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.StompSignal(e.beat, e["mute"], e["look"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute", "Disables the sound and animations that cue for the transition to stomping."),
                        new Param("look", true, "Look Forward", "The Sumo Brothers will look at the camera if transitioning from slapping.")
                    },
                    defaultLength = 4f,
                    priority = 4
                },

                 new GameAction("slapSignal", "Slap Signal")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.SlapSignal(e.beat, e["mute"]); },
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute", "Disables the sound and animations that cue for the transition to slapping.")
                    },
                    defaultLength = 4f,
                    priority = 3
                },

                new GameAction("endPose", "End Pose")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.EndPose(e.beat); },
                    parameters = new List<Param>()
                    {
                        new Param("type", SumoBrothers.PoseType.Crouching, "Pose", "The pose that the Sumo Brothers will make."),
                        new Param("bg", SumoBrothers.BGType.TheGreatWave, "Background", "The background that appears on a successful input.")
                    },
                    defaultLength = 5f,
                    priority = 2
                },

                new GameAction("look", "Look Forward")
                {
                    function = delegate { var e = eventCaller.currentEntity; SumoBrothers.instance.LookAtCamera(e.beat, e.length, e["look"]); },
                    /*parameters = new List<Param>()
                    {
                        new Param("look", true, "Look at Camera", "Whether the Sumo Brothers will look at the camera while slapping."),
                    },*/
                    defaultLength = 1f,
                    resizable = true
                },

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
        [SerializeField] Animator sumoBrotherGHead;
        [SerializeField] Animator sumoBrotherPHead;

        [Header("Properties")]
        /*static List<queuedSumoInputs> queuedInputs = new List<queuedSumoInputs>();
        public struct queuedSumoInputs
        {
            public float beat;
            public int cue;
            public int poseType;
            public int poseBG;
        }*/

        private bool goBopSumo;
        private bool goBopInu;

        private bool allowBopSumo;
        private bool allowBopInu;

        private bool sumoStompDir = false;
        private double lastReportedBeat = 0f;

        private bool cueCurrentlyActive = false; 

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

        private enum SumoState
        {
            Idle,
            Slap,
            Stomp,
            Pose
        }
        private SumoState sumoState = SumoState.Idle;
        private SumoState sumoStatePrevious = SumoState.Idle;

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
            /*if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }*/
        }

        // Update is called once per frame
        void Update()
        {
            var cond = Conductor.instance;

        }
        
        public override void OnBeatPulse(double beat)
        {
                if (allowBopInu)
                {
                    if (goBopInu)
                    {
                        inuSensei.DoScaledAnimationAsync("InuBop", 0.5f);
                    }
                    else
                    {
                    //    inuSensei.DoScaledAnimationAsync("InuIdle", 0.5f);
                    }

                }

                if (allowBopSumo)
                {
                    if(goBopSumo)
                    {
                        sumoBrotherP.DoScaledAnimationAsync("SumoBop", 0.5f);
                        sumoBrotherG.DoScaledAnimationAsync("SumoBop", 0.5f);
                        sumoBrotherGHead.DoScaledAnimationAsync("SumoGIdle", 0.5f);
                        sumoBrotherPHead.DoScaledAnimationAsync("SumoGIdle", 0.5f);
                    }
                    else
                    {
                    //    sumoBrotherP.DoScaledAnimationAsync("SumoIdle", 0.5f);
                    //    sumoBrotherG.DoScaledAnimationAsync("SumoIdle", 0.5f);
                    }
                    
                }

                print("current sumo state:");
                print(sumoState);
                print("previous sumo state:");
                print(sumoStatePrevious);
        }

        public void Bop(double beat, float length, bool inu, bool sumo, bool inuAuto, bool sumoAuto)
        {
            goBopInu = inuAuto;
            goBopSumo = sumoAuto;

            if (inu || sumo)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    bops.Add(new BeatAction.Action(beat + i, delegate
                    {
                        if (inu)
                        {
                            inuSensei.DoScaledAnimationAsync("InuBop", 0.5f);
                        }
                        if (sumo)
                        {
                            sumoBrotherP.DoScaledAnimationAsync("SumoBop", 0.5f);
                            sumoBrotherG.DoScaledAnimationAsync("SumoBop", 0.5f);
                            sumoBrotherGHead.DoScaledAnimationAsync("SumoGIdle", 0.5f);
                        }
                    }));
                }
                BeatAction.New(instance, bops);
            }

        }

        public void StompSignal(double beat, bool mute, bool lookatcam)
        {     
            if (sumoState == SumoState.Stomp || cueCurrentlyActive)
            {
                return;
            }

            CueRunning(beat + 0);

            if (lookatcam) {
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapLook", 0.5f);
                sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlapLook", 0.5f);

            }

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat + 3, delegate { allowBopSumo = false; }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherPHead.DoScaledAnimationAsync("SumoPStomp", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGStomp", 0.5f); })
                });

            if (mute == false)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
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

            sumoStatePrevious = sumoState;
            sumoState = SumoState.Stomp;
            StompRecursive(beat + 3, 1);
        }

        private void StompRecursive(double beat, double remaining)
        {

            if (sumoState != SumoState.Stomp) {

                remaining -= 1;

            }

            if (remaining <= 0) {
                
                return;
            }

            if (sumoStompDir) {
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { sumoBrotherP.DoScaledAnimationAsync("SumoStompPrepareL", 0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompPrepareR", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompR", 0.5f); })                    
                });
            } else {
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { sumoBrotherP.DoScaledAnimationAsync("SumoStompPrepareR", 0.5f); }),
                    new BeatAction.Action(beat, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompPrepareL", 0.5f); }),
                    new BeatAction.Action(beat + 1, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoStompL", 0.5f); })                    
                });
            }
        

            ScheduleInput(beat , 1, InputAction_BasicPress, StompHit, StompMiss, Nothing);
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { StompRecursive(beat + 2, remaining); })
                });

            if (sumoStompDir) { sumoStompDir = false;}
            else
            { sumoStompDir = true;}
            print(sumoStompDir);
            
        }

        public void SlapSignal(double beat, bool mute)
        {
            if (sumoState == SumoState.Slap || cueCurrentlyActive)
            {
                return;
            }

            CueRunning(beat + 0);

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat + 3, delegate { allowBopSumo = false; }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherP.DoScaledAnimationAsync("SumoSlapPrepare",0.5f); }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherG.DoScaledAnimationAsync("SumoSlapPrepare", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapPrepare", 0.5f); }),
                new BeatAction.Action(beat + 3, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlapPrepare", 0.5f); })
                });

            if (mute == false)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
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

            sumoStatePrevious = sumoState;
            sumoState = SumoState.Slap;
            SlapRecursive(beat + 4, 4);

        }

        private void SlapRecursive(double beat, double remaining)
        {

            if (sumoState != SumoState.Slap) {

                remaining -= 1;

            }

            if (remaining <= 0) {

                return;
            }

            ScheduleInput(beat - 1, 1, InputAction_BasicPress, SlapHit, SlapMiss, Nothing);
            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { SlapRecursive(beat + 1, remaining); })
                });
            
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
            
            BeatAction.New(instance, new List<BeatAction.Action>() {
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

        public void LookAtCamera(double beat, float length, bool lookatcam)
        {
            if (sumoState == SumoState.Slap) {
            BeatAction.New(instance, new List<BeatAction.Action>() {
            new BeatAction.Action(beat, delegate { sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapLook", 0.5f); }),
            new BeatAction.Action(beat, delegate { sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlapLook", 0.5f); }),
            new BeatAction.Action(beat, delegate { print("look"); }),
            new BeatAction.Action(beat + length, delegate { if (sumoState == SumoState.Slap) sumoBrotherPHead.DoScaledAnimationAsync("SumoPSlapPrepare", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { if (sumoState == SumoState.Slap) sumoBrotherGHead.DoScaledAnimationAsync("SumoGSlapPrepare", 0.5f); }),
            new BeatAction.Action(beat + length, delegate { print("lookun"); })
            });
            }

        }

        public void EndPose(double beat)
        {
            if (cueCurrentlyActive)
            { return; }

            CueRunning(beat + 0);
            sumoStatePrevious = sumoState;
            sumoState = SumoState.Pose;

            var cond = Conductor.instance;

            ScheduleInput(beat, 4f, InputAction_FlickPress, PoseHit, PoseMiss, Nothing);

            var tweet = SoundByte.PlayOneShotGame("sumoBrothers/posesignal", -1, 1f, 1f, true);
            tweet.SetLoopParams(beat + 3, 0.05f);

            allowBopInu = false;
            inuSensei.DoScaledAnimationAsync("InuTweet", 0.5f);

            BeatAction.New(instance, new List<BeatAction.Action>() {
                new BeatAction.Action(beat + 3, delegate { allowBopInu = true; }),
                new BeatAction.Action(beat + 3, delegate { if (goBopInu == true) inuSensei.DoScaledAnimationAsync("InuBop", 0.5f); }),
                new BeatAction.Action(beat + 5, delegate { allowBopSumo = true; })
            });

        }

        public void CueRunning(double beat)
        {
            cueCurrentlyActive = true;

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                new BeatAction.Action(beat, delegate { cueCurrentlyActive = false; })
                });
        }

        void PoseHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("sumoBrothers/tink");
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

        void SlapHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("sumoBrothers/tink");

            }
            else
            {

            }
            SoundByte.PlayOneShotGame("sumoBrothers/slap");
            sumoBrotherP.DoScaledAnimationAsync("SumoSlapFront", 0.5f);
            sumoBrotherG.DoScaledAnimationAsync("SumoSlapFront", 0.5f);
            


        }

        void SlapMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("sumoBrothers/miss");
            sumoBrotherG.DoScaledAnimationAsync("SumoSlapFront", 0.5f);
            sumoBrotherP.DoScaledAnimationAsync("SumoBop", 0.5f); // temp miss animation until i get real one done


        }

        void StompHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("sumoBrothers/tink");
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPStompBarely", 0.5f);
            }
            else
            {
                sumoBrotherPHead.DoScaledAnimationAsync("SumoPStomp", 0.5f);
            }
            SoundByte.PlayOneShotGame("sumoBrothers/stomp");

            if (sumoStompDir) 
            {
                sumoBrotherP.DoScaledAnimationAsync("SumoStompL", 0.5f);
            } else {
                sumoBrotherP.DoScaledAnimationAsync("SumoStompR", 0.5f);
            }
            


        }

        void StompMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("sumoBrothers/miss");


        }

        void Nothing(PlayerActionEvent caller) { }

    }
}
