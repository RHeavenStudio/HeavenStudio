using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
// using GhostlyGuy's Balls;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrOctopusMachineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("OctopusMachine", "Octopus Machine \n<color=#eb5454>[INITIALIZATION ONLY]</color>", "FFf362B", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.Bop(e.beat, e["disableBop"], e[""], e["whichBop"]); 
                    },
                    parameters = new List<Param>()                     
                    {
                        new Param("bop", false, "Which Bop?", "Plays a sepcific bop type"),
                        new Param("whichBop", OctopusMachine.Bops.Bop, "Which Bop?", "Plays a sepcific bop type"),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("StartInterval", "Start Interval")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.StartInterval(e.beat); 
                    },
                    defaultLength = 1f,
                },
                new GameAction("Expand", "Expand")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.Expand(e.beat); 
                    },
                    defaultLength = 1f,
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Prepare(e.beat);
                    },
                },
                new GameAction("Prepare", "Prepare")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        OctopusMachine.instance.Prepare(e.beat);
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("OctopusAnimation", "Octopus Animation")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.PlayAnimation(e.beat, e["keepWhich"], e["whichBop"]);
                    },
                    parameters = new List<Param>()                     
                    {
                        new Param("keepWhich", true, "Bop Like This?", "Keep bopping using the selected bop"),
                        new Param("whichBop", OctopusMachine.Bops.Bop, "Which Bop?", "Plays a specific bop type"),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("GameplayModifiers", "Gameplay Modifiers")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.GameplayModifiers(e.beat, e["color"], e["octoColor"], e["oct1"], e["oct2"], e["oct3"]);
                    },
                    parameters = new List<Param>()                     
                    {
                        new Param("color", new Color(1f, 0.84f, 0), "Background Color", "Set the background color"),
                        new Param("octoColor", new Color(1f, 0.145f, 0.5f), "Octopodes Color", "Set the octopode's colors"),
                        new Param("oct1", true, "Show Octopus 1?", "Keep bopping using the selected bop"),
                        new Param("oct2", true, "Show Octopus 2?", "Keep bopping using the selected bop"),
                        new Param("oct3", true, "Show Octopus 3?", "Keep bopping using the selected bop"),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("MoveOctopodes", "Move Octopodes")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity; 
                        OctopusMachine.instance.MoveOctopodes(e.beat, e["oct1x"], e["oct2x"], e["oct3x"]);
                    },
                    parameters = new List<Param>()                     
                    {
                        new Param("oct1x", new EntityTypes.Float(-10, 10), "X Octopus 1", "Change Octopus 1's X"),
                        new Param("oct2x", new EntityTypes.Float(-10, 10), "X Octopus 2", "Change Octopus 2's X"),
                        new Param("oct3x", new EntityTypes.Float(-10, 10), "X Octopus 3", "Change Octopus 3's X"),
                    },
                    defaultLength = 0.5f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_OctopusMachine;
    public partial class OctopusMachine : Minigame
    {
        [Header("Sprite Renderers")]
        [SerializeField] SpriteRenderer Background;
        
        [Header("Octopodes")]
        public Octopus Octopus1;
        public Octopus Octopus2;
        public Octopus Octopus3;

        [Header("Variables")]
        bool intervalStarted;
        float intervalStartBeat;
        float beatInterval = 4f;
        public bool isHappy;
        public bool isAngry;
        public bool isShocked;
        public bool isPreparing;
        public bool bopOn = true;
        public float lastReportedBeat = 0f;

        static List<QueuedHolding> QueuedHoldings = new List<QueuedHolding>();
        public struct QueuedHolding
        {
            public float startBeat;
            public float length;
        }

        private List<DynamicBeatmap.DynamicEntity> allIntervalEvents = new List<DynamicBeatmap.DynamicEntity>();

        public static OctopusMachine instance;

        public enum Bops
        {
            Bop,
            Joyful,
            Upset,
            Shocked,
        }

        void Awake()
        {
            instance = this;
            var intervalEvents = EventCaller.GetAllInGameManagerList("octopusMachine", new string[] { "StartInterval" });
            List<DynamicBeatmap.DynamicEntity> tempEvents = new List<DynamicBeatmap.DynamicEntity>();
            for (int i = 0; i < intervalEvents.Count; i++)
            {
                if (intervalEvents[i].beat + intervalEvents[i].beat >= Conductor.instance.songPositionInBeats)
                {
                    tempEvents.Add(intervalEvents[i]);
                }
            }

            allIntervalEvents = tempEvents;
        }

        private void Update()
        {
            if (!Octopus1.IsActive())
            {
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    
                }
                if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
                {
                    
                }
            }

            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (queuedBatons.Count > 0)
                {
                    foreach (var baton in queuedBatons)
                    {
                        Baton(baton);
                    }
                    queuedBatons.Clear();
                }
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(intervalStartBeat, beatInterval);
                if (normalizedBeat >= 1f && intervalStarted)
                {
                    PassTurn(intervalStartBeat + beatInterval, 0f);
                }
                if (allIntervalEvents.Count > 0)
                {
                    if (startIntervalIndex < allIntervalEvents.Count && startIntervalIndex >= 0)
                    {
                        if (Conductor.instance.songPositionInBeats >= allIntervalEvents[startIntervalIndex].beat)
                        {
                            StartInterval(allIntervalEvents[startIntervalIndex].beat, allIntervalEvents[startIntervalIndex].length);
                            startIntervalIndex++;
                        }
                    }
                }
            }
        }

        private void LateUpdate() 
        {
            
        }

        public void Prepare(float beat)
        {
            Octopus1.GameplayModifiers(oct1, octoColor);
            Octopus2.GameplayModifiers(oct2, octoColor);
            Octopus3.GameplayModifiers(oct3, octoColor);
            isPreparing = true;
        }

        public void Expand(float beat)
        {
            Debug.Log("expand event rn");
        }

        public void Bop(float beat, float length, bool doesBop, bool autoBop)
        {
            bopOn = autoBop;
            if (doesBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            
                        })
                    });
                }
            }
        }

        public void PlayAnimation(float beat, bool keepBopping, int whichBop)
        {
            
        }

        public void GameplayModifiers(float beat, Color bgColor, Color octoColor, bool oct1, bool oct2, bool oct3)
        {
            Background.color = bgColor;
            
            Octopus1.GameplayModifiers(oct1, octoColor);
            Octopus2.GameplayModifiers(oct2, octoColor);
            Octopus3.GameplayModifiers(oct3, octoColor);
        }

        public void MoveOctopodes(float beat, float oct1x, float oct2x, float oct3x)
        {
            Octopus1.MoveOctopodes(oct1x);
            Octopus2.MoveOctopodes(oct2x);
            Octopus3.MoveOctopodes(oct3x);
        }


        

        int startIntervalIndex;

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedSingings.Count > 0) queuedSingings.Clear();
                if (queuedBatons.Count > 0) queuedBatons.Clear();
            }
        }

        public void SetGameSwitchFadeOutTime(float fadeOut, float fadeOut1, float fadeOutPlayer)
        {
            leftChorusKid.gameSwitchFadeOutTime = fadeOut;
            middleChorusKid.gameSwitchFadeOutTime = fadeOut1;
            playerChorusKid.gameSwitchFadeOutTime = fadeOutPlayer;
        }

        public void ForceSing(int semiTones, int semiTones1, int semiTonesPlayer)
        {
            leftChorusKid.currentPitch = Jukebox.GetPitchFromSemiTones(semiTones, true);
            middleChorusKid.currentPitch = Jukebox.GetPitchFromSemiTones(semiTones1, true);
            playerChorusKid.currentPitch = Jukebox.GetPitchFromSemiTones(semiTonesPlayer, true);
            leftChorusKid.StartSinging(true);
            middleChorusKid.StartSinging(true);
            if (!PlayerInput.Pressing() || GameManager.instance.autoplay) playerChorusKid.StartSinging(true);
            else missed = true;
        }

        public void TogetherNow(float beat, int semiTones, int semiTones1, int semiTonesPlayer, float conductorPitch)
        {
            if (!playerChorusKid.disappeared) ScheduleInput(beat, 2.5f, InputType.STANDARD_UP, JustTogetherNow, Out, Out);
            if (!playerChorusKid.disappeared) ScheduleInput(beat, 3.5f, InputType.STANDARD_DOWN, JustTogetherNowClose, MissBaton, Out);
            float pitch = Jukebox.GetPitchFromSemiTones(semiTones, true);
            float pitch1 = Jukebox.GetPitchFromSemiTones(semiTones1, true);
            currentYellPitch = Jukebox.GetPitchFromSemiTones(semiTonesPlayer, true);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("gleeClub/togetherEN-01", beat + 0.5f, conductorPitch),
                new MultiSound.Sound("gleeClub/togetherEN-02", beat + 1f, conductorPitch),
                new MultiSound.Sound("gleeClub/togetherEN-03", beat + 1.5f, conductorPitch),
                new MultiSound.Sound("gleeClub/togetherEN-04", beat + 2f, conductorPitch, 1, false, 0.02f),
            });

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.5f, delegate
                {
                    leftChorusKid.StartCrouch();
                    middleChorusKid.StartCrouch();
                    playerChorusKid.StartCrouch();
                }),
                new BeatAction.Action(beat + 2.5f, delegate
                {
                    leftChorusKid.currentPitch = pitch;
                    middleChorusKid.currentPitch = pitch1;
                    leftChorusKid.StartYell();
                    middleChorusKid.StartYell();
                }),
                new BeatAction.Action(beat + 3.5f, delegate
                {
                    leftChorusKid.StopSinging(true);
                    middleChorusKid.StopSinging(true);
                }),
                new BeatAction.Action(beat + 6f, delegate
                {
                    if (!playerChorusKid.disappeared) ShowHeart(beat + 6f);
                })
            });
        }

        void JustTogetherNow(PlayerActionEvent caller, float state)
        {
            playerChorusKid.currentPitch = currentYellPitch;
            playerChorusKid.StartYell();
        }

        void JustTogetherNowClose(PlayerActionEvent caller, float state)
        {
            playerChorusKid.StopSinging(true);
        }

        public void StartInterval(float beat, float length)
        {
            intervalStartBeat = beat;
            beatInterval = length;
            intervalStarted = true;
        }

        public void Sing(float beat, float length, int semiTones, int semiTones1, int semiTonesPlayer, int closeMouth, bool repeating, int semiTonesLeft2, int semiTonesLeft3, int semiTonesMiddle2)
        {
            float pitch = Jukebox.GetPitchFromSemiTones(semiTones, true);
            if (!intervalStarted)
            {
                StartInterval(beat, length);
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { if (closeMouth != (int)MouthOpenClose.OnlyClose) leftChorusKid.currentPitch = pitch; leftChorusKid.StartSinging(); }),
                new BeatAction.Action(beat + length, delegate { if (closeMouth != (int)MouthOpenClose.OnlyOpen) leftChorusKid.StopSinging(); }),
            });
            queuedSingings.Add(new QueuedSinging
            {
                startBeat = beat - intervalStartBeat,
                length = length,
                semiTones = semiTones1,
                closeMouth = closeMouth,
                semiTonesPlayer = semiTonesPlayer,
                repeating = repeating,
                semiTonesLeft2 = semiTonesLeft2,
                semiTonesLeft3 = semiTonesLeft3,
                semiTonesMiddle2 = semiTonesMiddle2
            });
        }

        public void PassTurn(float beat, float length)
        {
            if (queuedSingings.Count == 0) return;
            intervalStarted = false;
            missed = false;
            if (!playerChorusKid.disappeared) ShowHeart(beat + length + beatInterval * 2 + 1);
            foreach (var sing in queuedSingings)
            {
                float playerPitch = Jukebox.GetPitchFromSemiTones(sing.semiTonesPlayer, true);
                if (!playerChorusKid.disappeared)
                {
                    GleeClubSingInput spawnedInput = Instantiate(singInputPrefab, transform);
                    spawnedInput.pitch = playerPitch;
                    spawnedInput.Init(beat + length + sing.startBeat + beatInterval, sing.length, sing.closeMouth);
                }
                float pitch = Jukebox.GetPitchFromSemiTones(sing.semiTones, true);
                float pitchLeft2 = Jukebox.GetPitchFromSemiTones(sing.semiTonesLeft2, true);
                float pitchLeft3 = Jukebox.GetPitchFromSemiTones(sing.semiTonesLeft3, true);
                float pitchMiddle2 = Jukebox.GetPitchFromSemiTones(sing.semiTonesMiddle2, true);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length + sing.startBeat, delegate
                    {
                        if (sing.closeMouth != (int)MouthOpenClose.OnlyClose) 
                        {
                            middleChorusKid.currentPitch = pitch;
                            middleChorusKid.StartSinging();
                            if (sing.repeating)
                            {
                                leftChorusKid.currentPitch = pitchLeft2;
                                leftChorusKid.StartSinging();
                            }
                        } 
                    }),
                    new BeatAction.Action(beat + length + sing.startBeat + sing.length, delegate
                    {
                        if (sing.closeMouth != (int)MouthOpenClose.OnlyOpen) 
                        {
                            middleChorusKid.StopSinging();
                            if (sing.repeating) leftChorusKid.StopSinging();
                        } 
                    }),
                    new BeatAction.Action(beat + length + sing.startBeat + beatInterval, delegate
                    {
                        if (sing.closeMouth != (int)MouthOpenClose.OnlyClose && sing.repeating)
                        {
                            middleChorusKid.currentPitch = pitchMiddle2;
                            leftChorusKid.currentPitch = pitchLeft3;
                            middleChorusKid.StartSinging();
                            leftChorusKid.StartSinging();
                        }
                    }),
                    new BeatAction.Action(beat + length + sing.startBeat + sing.length + beatInterval, delegate
                    {
                        if (sing.closeMouth != (int)MouthOpenClose.OnlyOpen && sing.repeating)
                        {
                            middleChorusKid.StopSinging();
                            leftChorusKid.StopSinging();
                        }
                    }),
                });
            }
            queuedSingings.Clear();
        }
    }
}