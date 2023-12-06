using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrGleeClubLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("gleeClub", "Glee Club", "cfcecf", false, false, new List<GameAction>()
            {
                new GameAction("intervalStart", "Start Interval")
                {
                    defaultLength = 1f,
                    resizable = true
                },
                new GameAction("sing", "Sing")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.Sing(e.beat, e.length, e["semiTones"], e["semiTones1"], e["semiTonesPlayer"],
                        e["close"], e["repeat"], e["semiTonesLeft2"], e["semiTonesLeft3"], e["semiTonesMiddle2"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("semiTones", new EntityTypes.Integer(-24, 24, -5), "Semitones", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTones1", new EntityTypes.Integer(-24, 24, -1), "Semitones (Next)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesPlayer", new EntityTypes.Integer(-24, 24, 2), "Semitones (Player)", "The number of semitones up or down this note should be pitched"),
                        new Param("close", GleeClub.MouthOpenClose.Both, "Close/Open Mouth", "Should the chorus kids close/open their mouth?"),
                        new Param("repeat", false, "Repeating", "Should the left and middle chorus kid repeat this singing cue?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "semiTonesLeft2", "semiTonesLeft3", "semiTonesMiddle2" })
                        }),
                        new Param("semiTonesLeft2", new EntityTypes.Integer(-24, 24, 0), "Semitones (Repeat Left First)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesLeft3", new EntityTypes.Integer(-24, 24, 0), "Semitones (Repeat Left Last)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesMiddle2", new EntityTypes.Integer(-24, 24, 0), "Semitones (Repeat Middle)", "The number of semitones up or down this note should be pitched"),
                    }
                },
                new GameAction("baton", "Baton")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; GleeClub.PreBaton(e.beat); },
                    defaultLength = 2f
                },
                new GameAction("togetherNow", "Together Now!")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.TogetherNow(e.beat, e["semiTones"], e["semiTones1"], e["semiTonesPlayer"], e["pitch"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("semiTones", new EntityTypes.Integer(-24, 24, -1), "Semitones", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTones1", new EntityTypes.Integer(-24, 24, 4), "Semitones (Next)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesPlayer", new EntityTypes.Integer(-24, 24, 10), "Semitones (Player)", "The number of semitones up or down this note should be pitched"),
                        new Param("pitch", new EntityTypes.Float(0f, 5f, 1f), "Conductor Voice Pitch", "Which pitch should the conductor's voice be at? (1 is normal pitch)")
                    }
                },
                new GameAction("forceSing", "Force Sing")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.ForceSing(e["semiTones"], e["semiTones1"], e["semiTonesPlayer"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("semiTones", new EntityTypes.Integer(-24, 24, 0), "Semitones", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTones1", new EntityTypes.Integer(-24, 24, 0), "Semitones (Next)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesPlayer", new EntityTypes.Integer(-24, 24, 0), "Semitones (Player)", "The number of semitones up or down this note should be pitched"),
                    }
                },
                new GameAction("presence", "Toggle Chorus Kids Presence")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.ToggleKidsPresence(!e["left"], !e["middle"], !e["player"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("left", false, "Left Kid Present", "Should this chorus kid be present?"),
                        new Param("middle", false, "Middle Kid Present", "Should this chorus kid be present?"),
                        new Param("player", false, "Player Kid Present", "Should this chorus kid be present?"),
                    }
                },
                new GameAction("fadeOutTime", "Set Sing Game-Switch Fade Out Time")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.SetGameSwitchFadeOutTime(e["fade"], e["fade1"], e["fadeP"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("fade", new EntityTypes.Float(0, 20f, 0f), "Left Chorus Kid", "For how much time in seconds should this chorus kid's singing fade out?"),
                        new Param("fade1", new EntityTypes.Float(0, 20f, 0f), "Middle Chorus Kid", "For how much time in seconds should this chorus kid's singing fade out?"),
                        new Param("fadeP", new EntityTypes.Float(0, 20f, 0f), "Player Chorus Kid", "For how much time in seconds should this chorus kid's singing fade out?"),
                    }
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.PassTurn(e.beat, e.length); },
                    resizable = true,
                    defaultLength = 0.5f,
                    hidden = true
                },
            },
            new List<string>() { "ntr", "repeat" },
            "ntrchorus", "en",
            new List<string>() { "en" }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_GleeClub;
    public class GleeClub : Minigame
    {
        public enum MouthOpenClose
        {
            Both,
            OnlyOpen,
            OnlyClose
        }
        public struct QueuedSinging
        {
            public double startBeat;
            public float length;
            public int semiTones;
            public int semiTonesPlayer;
            public int closeMouth;
            public bool repeating;
            public int semiTonesLeft2;
            public int semiTonesLeft3;
            public int semiTonesMiddle2;
        }
        [Header("Prefabs")]
        [SerializeField] GleeClubSingInput singInputPrefab;
        [Header("Components")]
        [SerializeField] Animator heartAnim;
        [SerializeField] Animator condAnim;
        public ChorusKid leftChorusKid;
        public ChorusKid middleChorusKid;
        public ChorusKid playerChorusKid;
        [Header("Variables")]
        static List<QueuedSinging> queuedSingings = new List<QueuedSinging>();
        static List<double> queuedBatons = new();
        bool intervalStarted;
        double intervalStartBeat;
        float beatInterval = 4f;
        public bool missed;
        public static GleeClub instance;
        float currentYellPitch = 1f;

        int startIntervalIndex;

        private List<RiqEntity> allIntervalEvents = new List<RiqEntity>();

        void Awake()
        {
            instance = this;
            var camEvents = EventCaller.GetAllInGameManagerList("gleeClub", new string[] { "intervalStart" });
            List<RiqEntity> tempEvents = new List<RiqEntity>();
            for (int i = 0; i < camEvents.Count; i++)
            {
                if (camEvents[i].beat + camEvents[i].beat >= Conductor.instance.songPositionInBeatsAsDouble)
                {
                    tempEvents.Add(camEvents[i]);
                }
            }

            allIntervalEvents = tempEvents;
        }

        void Start()
        {
            if (!PlayerInput.GetIsAction(InputAction_BasicPressing) && Conductor.instance.isPlaying && !GameManager.instance.autoplay)
            {
                playerChorusKid.StartSinging();
                leftChorusKid.MissPose();
                middleChorusKid.MissPose();
            }
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedSingings.Count > 0) queuedSingings.Clear();
                if (queuedBatons.Count > 0) queuedBatons.Clear();
            }
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        void Update()
        {
            if (!playerChorusKid.disappeared)
            {
                if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
                {
                    playerChorusKid.StopSinging();
                    leftChorusKid.MissPose();
                    middleChorusKid.MissPose();
                    ScoreMiss();
                }
                if (PlayerInput.GetIsAction(InputAction_BasicRelease) && !IsExpectingInputNow(InputAction_BasicRelease))
                {
                    playerChorusKid.StartSinging();
                    leftChorusKid.MissPose();
                    middleChorusKid.MissPose();
                    ScoreMiss();
                }
                if (PlayerInput.GetIsAction(InputAction_FlickRelease) && !IsExpectingInputNow(InputAction_FlickRelease))
                {
                    if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                    {
                        playerChorusKid.StartYell();
                        leftChorusKid.MissPose();
                        middleChorusKid.MissPose();
                        ScoreMiss();
                    }
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
                        if (Conductor.instance.songPositionInBeatsAsDouble >= allIntervalEvents[startIntervalIndex].beat)
                        {
                            StartInterval((float)allIntervalEvents[startIntervalIndex].beat, allIntervalEvents[startIntervalIndex].length);
                            startIntervalIndex++;
                        }
                    }
                }

            }

            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedSingings.Count > 0) queuedSingings.Clear();
                if (queuedBatons.Count > 0) queuedBatons.Clear();
            }
        }

        public void ToggleKidsPresence(bool left, bool middle, bool player)
        {
            leftChorusKid.TogglePresence(left);
            middleChorusKid.TogglePresence(middle);
            playerChorusKid.TogglePresence(player);
        }

        public void SetGameSwitchFadeOutTime(float fadeOut, float fadeOut1, float fadeOutPlayer)
        {
            leftChorusKid.gameSwitchFadeOutTime = fadeOut;
            middleChorusKid.gameSwitchFadeOutTime = fadeOut1;
            playerChorusKid.gameSwitchFadeOutTime = fadeOutPlayer;
        }

        public void ForceSing(int semiTones, int semiTones1, int semiTonesPlayer)
        {
            leftChorusKid.currentPitch = SoundByte.GetPitchFromSemiTones(semiTones, true);
            middleChorusKid.currentPitch = SoundByte.GetPitchFromSemiTones(semiTones1, true);
            playerChorusKid.currentPitch = SoundByte.GetPitchFromSemiTones(semiTonesPlayer, true);
            leftChorusKid.StartSinging(true);
            middleChorusKid.StartSinging(true);
            if (!PlayerInput.GetIsAction(InputAction_BasicPressing) || GameManager.instance.autoplay) playerChorusKid.StartSinging(true);
            else missed = true;
        }

        public void TogetherNow(double beat, int semiTones, int semiTones1, int semiTonesPlayer, float conductorPitch)
        {
            if (!playerChorusKid.disappeared) ScheduleInput(beat, 2.5f, InputAction_FlickRelease, JustTogetherNow, Out, Out);
            if (!playerChorusKid.disappeared) ScheduleInput(beat, 3.5f, InputAction_BasicPress, JustTogetherNowClose, MissBaton, Out);
            float pitch = SoundByte.GetPitchFromSemiTones(semiTones, true);
            float pitch1 = SoundByte.GetPitchFromSemiTones(semiTones1, true);
            currentYellPitch = SoundByte.GetPitchFromSemiTones(semiTonesPlayer, true);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("gleeClub/togetherEN-01", beat + 0.5f, conductorPitch),
                new MultiSound.Sound("gleeClub/togetherEN-02", beat + 1f, conductorPitch),
                new MultiSound.Sound("gleeClub/togetherEN-03", beat + 1.5f, conductorPitch),
                new MultiSound.Sound("gleeClub/togetherEN-04", beat + 2f, conductorPitch, 1, false, 0.02f),
            });

            BeatAction.New(instance, new List<BeatAction.Action>()
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

        public void StartInterval(double beat, float length)
        {
            intervalStartBeat = beat;
            beatInterval = length;
            intervalStarted = true;
        }

        public void Sing(double beat, float length, int semiTones, int semiTones1, int semiTonesPlayer, int closeMouth, bool repeating, int semiTonesLeft2, int semiTonesLeft3, int semiTonesMiddle2)
        {
            float pitch = SoundByte.GetPitchFromSemiTones(semiTones, true);
            if (!intervalStarted)
            {
                StartInterval(beat, length);
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
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

        public void PassTurn(double beat, float length)
        {
            if (queuedSingings.Count == 0) return;
            intervalStarted = false;
            missed = false;
            if (!playerChorusKid.disappeared) ShowHeart(beat + length + beatInterval * 2 + 1);
            foreach (var sing in queuedSingings)
            {
                float playerPitch = SoundByte.GetPitchFromSemiTones(sing.semiTonesPlayer, true);
                if (!playerChorusKid.disappeared)
                {
                    GleeClubSingInput spawnedInput = Instantiate(singInputPrefab, transform);
                    spawnedInput.pitch = playerPitch;
                    spawnedInput.Init(beat + length + sing.startBeat + beatInterval, sing.length, sing.closeMouth);
                }
                float pitch = SoundByte.GetPitchFromSemiTones(sing.semiTones, true);
                float pitchLeft2 = SoundByte.GetPitchFromSemiTones(sing.semiTonesLeft2, true);
                float pitchLeft3 = SoundByte.GetPitchFromSemiTones(sing.semiTonesLeft3, true);
                float pitchMiddle2 = SoundByte.GetPitchFromSemiTones(sing.semiTonesMiddle2, true);
                BeatAction.New(instance, new List<BeatAction.Action>()
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

        public static void PreBaton(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("gleeClub/BatonUp", beat),
                new MultiSound.Sound("gleeClub/BatonDown", beat + 1),
            }, forcePlay: true);
            if (GameManager.instance.currentGame == "gleeClub")
            {
                instance.Baton(beat);
            }
            else
            {
                queuedBatons.Add(beat);
            }
        }

        public void Baton(double beat)
        {
            missed = false;
            if (!playerChorusKid.disappeared) ScheduleInput(beat, 1, InputAction_BasicPress, JustBaton, MissBaton, Out);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    condAnim.DoScaledAnimationAsync("ConductorBatonUp", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    condAnim.DoScaledAnimationAsync("ConductorBatonDown", 0.5f);
                    leftChorusKid.StopSinging();
                    middleChorusKid.StopSinging();
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    condAnim.DoUnscaledAnimation("ConductorIdle", 0, -1);
                }),
            });
        }

        void JustBaton(PlayerActionEvent caller, float state)
        {
            playerChorusKid.StopSinging();
            ShowHeart(caller.timer + caller.startBeat + 1f);
        }

        void MissBaton(PlayerActionEvent caller)
        {
            missed = true;
        }

        void Out(PlayerActionEvent caller) { }

        public void ShowHeart(double beat)
        {

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    if (missed)
                    {
                        leftChorusKid.MissPose();
                        middleChorusKid.MissPose();
                        return;
                    }
                    heartAnim.Play("HeartIdle", 0, 0);
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    heartAnim.Play("HeartNothing", 0, 0);
                })
            });
        }
    }
}

