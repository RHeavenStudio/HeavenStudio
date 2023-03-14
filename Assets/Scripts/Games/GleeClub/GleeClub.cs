using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

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
                    function = delegate {var e = eventCaller.currentEntity; GleeClub.instance.StartInterval(e.beat, e.length); },
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
                        new Param("semiTones", new EntityTypes.Integer(-24, 24, 0), "Semitones", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTones1", new EntityTypes.Integer(-24, 24, 0), "Semitones (Next)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesPlayer", new EntityTypes.Integer(-24, 24, 0), "Semitones (Player)", "The number of semitones up or down this note should be pitched"),
                        new Param("close", GleeClub.MouthOpenClose.Both, "Close/Open Mouth", "Should the chorus kids close/open their mouth?"),
                        new Param("repeat", false, "Repeating", "Should the left and middle chorus kid repeat this singing cue?"),
                        new Param("semiTonesLeft2", new EntityTypes.Integer(-24, 24, 0), "Semitones (Repeat Left First)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesLeft3", new EntityTypes.Integer(-24, 24, 0), "Semitones (Repeat Left Last)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesMiddle2", new EntityTypes.Integer(-24, 24, 0), "Semitones (Repeat Middle)", "The number of semitones up or down this note should be pitched"),
                    }
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.PassTurn(e.beat, e.length); },
                    resizable = true,
                    defaultLength = 0.5f
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
                        new Param("semiTones", new EntityTypes.Integer(-24, 24, 0), "Semitones", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTones1", new EntityTypes.Integer(-24, 24, 0), "Semitones (Next)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesPlayer", new EntityTypes.Integer(-24, 24, 0), "Semitones (Player)", "The number of semitones up or down this note should be pitched"),
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
                }
            });
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
            public float startBeat;
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
        [SerializeField] ChorusKid leftChorusKid;
        [SerializeField] ChorusKid middleChorusKid;
        public ChorusKid playerChorusKid;
        [Header("Variables")]
        static List<QueuedSinging> queuedSingings = new List<QueuedSinging>();
        static List<float> queuedBatons = new List<float>();
        bool intervalStarted;
        float intervalStartBeat;
        float beatInterval = 4f;
        public bool missed;
        public static GleeClub instance;
        float currentYellPitch = 1f;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (!PlayerInput.Pressing() && Conductor.instance.isPlaying && !GameManager.instance.autoplay)
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
        }

        void Update()
        {
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                playerChorusKid.StopSinging();
                leftChorusKid.MissPose();
                middleChorusKid.MissPose();
                ScoreMiss();
            }
            if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
            {
                playerChorusKid.StartSinging();
                leftChorusKid.MissPose();
                middleChorusKid.MissPose();
                ScoreMiss();
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
            }

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
            leftChorusKid.currentPitch = Mathf.Pow(2f, (1f / 12f) * semiTones) * Conductor.instance.musicSource.pitch;
            middleChorusKid.currentPitch = Mathf.Pow(2f, (1f / 12f) * semiTones1) * Conductor.instance.musicSource.pitch;
            playerChorusKid.currentPitch = Mathf.Pow(2f, (1f / 12f) * semiTonesPlayer) * Conductor.instance.musicSource.pitch;
            leftChorusKid.StartSinging(true);
            middleChorusKid.StartSinging(true);
            if (!PlayerInput.Pressing() || GameManager.instance.autoplay) playerChorusKid.StartSinging(true);
            else missed = true;
        }

        public void TogetherNow(float beat, int semiTones, int semiTones1, int semiTonesPlayer, float conductorPitch)
        {
            ScheduleInput(beat, 2.5f, InputType.STANDARD_UP, JustTogetherNow, Out, Out);
            ScheduleInput(beat, 3.5f, InputType.STANDARD_DOWN, JustTogetherNowClose, MissBaton, Out);
            float pitch = Mathf.Pow(2f, (1f / 12f) * semiTones) * Conductor.instance.musicSource.pitch;
            float pitch1 = Mathf.Pow(2f, (1f / 12f) * semiTones1) * Conductor.instance.musicSource.pitch;
            currentYellPitch = Mathf.Pow(2f, (1f / 12f) * semiTonesPlayer) * Conductor.instance.musicSource.pitch;
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
                    ShowHeart(beat + 6f);
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
            float pitch = Mathf.Pow(2f, (1f / 12f) * semiTones) * Conductor.instance.musicSource.pitch;
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
            ShowHeart(beat + length + beatInterval * 2 + 1);
            foreach (var sing in queuedSingings)
            {
                float playerPitch = Mathf.Pow(2f, (1f / 12f) * sing.semiTonesPlayer) * Conductor.instance.musicSource.pitch;
                GleeClubSingInput spawnedInput = Instantiate(singInputPrefab, transform);
                spawnedInput.pitch = playerPitch;
                spawnedInput.Init(beat + length + sing.startBeat + beatInterval, sing.length, sing.closeMouth);
                float pitch = Mathf.Pow(2f, (1f / 12f) * sing.semiTones) * Conductor.instance.musicSource.pitch;
                float pitchLeft2 = Mathf.Pow(2f, (1f / 12f) * sing.semiTonesLeft2) * Conductor.instance.musicSource.pitch;
                float pitchLeft3 = Mathf.Pow(2f, (1f / 12f) * sing.semiTonesLeft3) * Conductor.instance.musicSource.pitch;
                float pitchMiddle2 = Mathf.Pow(2f, (1f / 12f) * sing.semiTonesMiddle2) * Conductor.instance.musicSource.pitch;
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

        public static void PreBaton(float beat)
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

        public void Baton(float beat)
        {
            missed = false;
            ScheduleInput(beat, 1, InputType.STANDARD_DOWN, JustBaton, MissBaton, Out);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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

        public void ShowHeart(float beat)
        {

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
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

