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
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("sing", "Sing")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.Sing(e.beat, e.length, e["semiTones"], e["semiTones1"], e["semiTonesPlayer"], e["close"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("semiTones", new EntityTypes.Integer(-24, 24, 0), "Semitones", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTones1", new EntityTypes.Integer(-24, 24, 0), "Semitones (Next)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesPlayer", new EntityTypes.Integer(-24, 24, 0), "Semitones (Player)", "The number of semitones up or down this note should be pitched"),
                        new Param("close", true, "Close Mouth", "Should the chorus kids close their mouth at the end of this block?")
                    }
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.PassTurn(e.beat, e.length); },
                    resizable = true
                },
                new GameAction("baton", "Baton")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.Baton(e.beat); },
                    defaultLength = 2f
                },
                new GameAction("togetherNow", "Together Now!")
                {
                    function = delegate { var e = eventCaller.currentEntity; GleeClub.instance.TogetherNow(e.beat, e["semiTones"], e["semiTones1"], e["semiTonesPlayer"]); },
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("semiTones", new EntityTypes.Integer(-24, 24, 0), "Semitones", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTones1", new EntityTypes.Integer(-24, 24, 0), "Semitones (Next)", "The number of semitones up or down this note should be pitched"),
                        new Param("semiTonesPlayer", new EntityTypes.Integer(-24, 24, 0), "Semitones (Player)", "The number of semitones up or down this note should be pitched"),
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
        public struct QueuedSinging
        {
            public float startBeat;
            public float length;
            public int semiTones;
            public int semiTonesPlayer;
            public bool closeMouth;
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
            }
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedSingings.Count > 0) queuedSingings.Clear();
            }
        }

        void Update()
        {
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                playerChorusKid.StopSinging();
                leftChorusKid.MissPose();
                middleChorusKid.MissPose();
            }
            if (PlayerInput.PressedUp() && !IsExpectingInputNow(InputType.STANDARD_UP))
            {
                playerChorusKid.StartSinging();
                leftChorusKid.MissPose();
                middleChorusKid.MissPose();
            }
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedSingings.Count > 0) queuedSingings.Clear();
            }
        }

        public void TogetherNow(float beat, int semiTones, int semiTones1, int semiTonesPlayer)
        {
            ScheduleInput(beat, 1.25f, InputType.STANDARD_UP, JustTogetherNow, Out, Out);
            ScheduleInput(beat, 1.75f, InputType.STANDARD_DOWN, JustTogetherNowClose, MissBaton, Out);
            float pitch = Mathf.Pow(2f, (1f / 12f) * semiTones) * Conductor.instance.musicSource.pitch;
            float pitch1 = Mathf.Pow(2f, (1f / 12f) * semiTones1) * Conductor.instance.musicSource.pitch;
            currentYellPitch = Mathf.Pow(2f, (1f / 12f) * semiTonesPlayer) * Conductor.instance.musicSource.pitch;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("gleeClub/togetherEN-01", beat + 0.25f),
                new MultiSound.Sound("gleeClub/togetherEN-02", beat + 0.5f),
                new MultiSound.Sound("gleeClub/togetherEN-03", beat + 0.75f),
                new MultiSound.Sound("gleeClub/togetherEN-04", beat + 1f),
            });

            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.75f, delegate
                {
                    leftChorusKid.StartCrouch();
                    middleChorusKid.StartCrouch();
                    playerChorusKid.StartCrouch();
                }),
                new BeatAction.Action(beat + 1.25f, delegate
                {
                    leftChorusKid.currentPitch = pitch;
                    middleChorusKid.currentPitch = pitch1;
                    leftChorusKid.StartYell();
                    middleChorusKid.StartYell();
                }),
                new BeatAction.Action(beat + 1.75f, delegate
                {
                    leftChorusKid.StopSinging(true);
                    middleChorusKid.StopSinging(true);
                }),
                new BeatAction.Action(beat + 3f, delegate
                {
                    ShowHeart(beat + 3f);
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

        public void Sing(float beat, float length, int semiTones, int semiTones1, int semiTonesPlayer, bool closeMouth)
        {
            float pitch = Mathf.Pow(2f, (1f / 12f) * semiTones) * Conductor.instance.musicSource.pitch;
            if (!intervalStarted)
            {
                StartInterval(beat, beatInterval);
            }
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { leftChorusKid.currentPitch = pitch; leftChorusKid.StartSinging(); }),
                new BeatAction.Action(beat + length, delegate { if (closeMouth) leftChorusKid.StopSinging(); }),
            });
            queuedSingings.Add(new QueuedSinging
            {
                startBeat = beat - intervalStartBeat,
                length = length,
                semiTones = semiTones1,
                closeMouth = closeMouth,
                semiTonesPlayer = semiTonesPlayer,
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
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length + sing.startBeat, delegate
                    {
                        middleChorusKid.currentPitch = pitch; 
                        middleChorusKid.StartSinging();
                    }),
                    new BeatAction.Action(beat + length + sing.startBeat + sing.length, delegate
                    {
                        if (sing.closeMouth) middleChorusKid.StopSinging();
                    }),
                });
            }
            queuedSingings.Clear();
        }

        public void Baton(float beat)
        {
            missed = false;
            ScheduleInput(beat, 1, InputType.STANDARD_DOWN, JustBaton, MissBaton, Out);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("gleeClub/BatonUp", beat),
                new MultiSound.Sound("gleeClub/BatonDown", beat + 1),
            });
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

