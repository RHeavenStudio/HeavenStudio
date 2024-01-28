using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbTapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tapTrial", "Tap Trial", "94ffb5", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.Bop(e.beat, e.length, e["toggle"], e["toggle2"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the characters should bop for the duration of this event."),
                        new Param("toggle2", false, "Bop (Auto)", "Toggle if the characters should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("tap", "Tap")
                {
                    function = delegate { TapTrial.instance.Tap(eventCaller.currentEntity.beat); },
                    defaultLength = 2.0f
                },
                new GameAction("double tap", "Double Tap")
                {
                    function = delegate { TapTrial.instance.DoubleTap(eventCaller.currentEntity.beat); },
                    defaultLength = 2.0f
                },
                new GameAction("triple tap", "Triple Tap")
                {
                    function = delegate { TapTrial.instance.TripleTap(eventCaller.currentEntity.beat); },
                    defaultLength = 4.0f
                },
                new GameAction("jump tap prep", "Jump Prepare")
                {
                    function = delegate { TapTrial.instance.JumpPrepare(); },
                },
                new GameAction("jump tap", "Jump Tap")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.JumpTap(e.beat, e["final"]); },
                    defaultLength = 2.0f,
                    parameters = new List<Param>()
                    {
                        new Param("final", false, "Final", "Toggle if this jump should be the final one of the set.")
                    }
                },
                new GameAction("scroll event", "Scroll Background")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.Scroll(e["toggle"], e["flash"], e["m"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Scroll", "Toggle if the background should scroll.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "flash", "m"})
                        }),
                        new Param("flash", true, "White Fade", "Toggle if the background will have a white overlay."),
                        new Param("m", new EntityTypes.Float(0, 10, 1), "Speed", "Set how fast the background should scroll.")
                    }
                },
                new GameAction("giraffe events", "Giraffe Animations")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.GiraffeAnims(e.beat, e.length, e["toggle"], e["instant"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", TapTrial.GiraffeAnimation.Enter, "Animation", "Set the animation for the giraffe to perform."),
                        new Param("instant", EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
                // backwards-compatibility
                new GameAction("final jump tap", "Final Jump Tap")
                {
                    function = delegate { var e = eventCaller.currentEntity; TapTrial.instance.JumpTap(e.beat, true); },
                    defaultLength = 2.0f,
                    hidden = true
                },
            },
            new List<string>() { "agb", "normal" },
            "agbtap", "en",
            new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using HeavenStudio.Common;
    using Scripts_TapTrial;

    public class TapTrial : Minigame
    {
        [Header("Components")]
        [SerializeField] private TapTrialPlayer player;
        [SerializeField] private Animator monkeyL, monkeyR, giraffe;
        [SerializeField] private ParticleSystem monkeyTapLL, monkeyTapLR, monkeyTapRL, monkeyTapRR;
        [SerializeField] private Transform rootPlayer, rootMonkeyL, rootMonkeyR;
        [SerializeField] private CanvasScroll bgScroll;
        [SerializeField] private SpriteRenderer flash;
        [Header("Values")]
        [SerializeField] private float jumpHeight = 4f;
        [SerializeField] private float monkeyJumpHeight = 3f;
        [SerializeField] private float maxFlashOpacity = 0.8f;

        private bool canBop = true;

        private double jumpStartBeat = double.MinValue;

        public static TapTrial instance;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("tapTrial", "bop", "toggle2");
        }

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) SingleBop();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                GiraffeUpdate(cond);
                JumpUpdate(cond);
                ScrollUpdate(cond);
            }
        }

        public void Scroll(bool scroll, bool flash, float multiplier)
        {
            scrolling = scroll;
            flashing = flash;
            scrollMultiplier = multiplier;
            ResetScroll();
        }

        public void ResetScroll()
        {
            currentScrollSpeed = 0;
            currentNormalizedY = 0;
            flash.color = new Color(1, 1, 1, 0);
        }

        private bool scrolling;
        private bool flashing;
        [SerializeField] private float maxScrollSpeed = 0.25f;
        [SerializeField] private float accelerationSpeed = 0.01f;
        private float currentScrollSpeed = 0;
        private float currentNormalizedY = 0;
        private float scrollMultiplier = 1;
        private void ScrollUpdate(Conductor cond)
        {
            if (!scrolling)
            {
                bgScroll.Normalized = Vector2.zero;
                ResetScroll();
                return;
            }
            currentNormalizedY += currentScrollSpeed * Time.deltaTime;
            bgScroll.NormalizedY = currentNormalizedY * scrollMultiplier;
            if (flashing) flash.color = new Color(1, 1, 1, Mathf.Lerp(0, maxFlashOpacity, currentNormalizedY));
            currentScrollSpeed += accelerationSpeed * Time.deltaTime;
            currentScrollSpeed = Mathf.Min(maxScrollSpeed, currentScrollSpeed);
        }

        private void GiraffeUpdate(Conductor cond)
        {
            float normalizedGiraffeBeat = cond.GetPositionFromBeat(animStartBeat, animLength);
            EasingFunction.Function func = EasingFunction.GetEasingFunction(currentEase);

            if (normalizedGiraffeBeat <= 1f && normalizedGiraffeBeat >= 0f)
            {
                switch (currentAnim)
                {
                    case GiraffeAnimation.Enter:
                        giraffe.DoNormalizedAnimation("Enter", func(0, 1, normalizedGiraffeBeat));
                        break;
                    case GiraffeAnimation.Exit:
                        giraffe.DoNormalizedAnimation("Exit", func(0, 1, normalizedGiraffeBeat));
                        break;
                    default: break;
                }
            }
        }

        private void JumpUpdate(Conductor cond)
        {
            float normalizedJumpBeat = cond.GetPositionFromBeat(jumpStartBeat, 1);

            if (normalizedJumpBeat >= 0 && normalizedJumpBeat <= 1)
            {
                if (normalizedJumpBeat >= 0.5f)
                {
                    float normalizedUp = cond.GetPositionFromBeat(jumpStartBeat, 0.5);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuad);
                    float newPlayerY = func(0, jumpHeight, normalizedUp);
                    float newMonkeyY = func(0, monkeyJumpHeight, normalizedUp);
                    rootPlayer.localPosition = new Vector3(0, newPlayerY);
                    rootMonkeyL.localPosition = new Vector3(0, newMonkeyY);
                    rootMonkeyR.localPosition = new Vector3(0, newMonkeyY);
                }
                else
                {
                    float normalizedDown = cond.GetPositionFromBeat(jumpStartBeat + 0.5, 0.5);
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
                    float newPlayerY = func(jumpHeight, 0, normalizedDown);
                    float newMonkeyY = func(monkeyJumpHeight, 0, normalizedDown);
                    rootPlayer.localPosition = new Vector3(0, newPlayerY);
                    rootMonkeyL.localPosition = new Vector3(0, newMonkeyY);
                    rootMonkeyR.localPosition = new Vector3(0, newMonkeyY);
                }
            }
            else
            {
                rootPlayer.localPosition = Vector3.zero;
                rootMonkeyL.localPosition = Vector3.zero;
                rootMonkeyR.localPosition = Vector3.zero;
            }
        }

        public enum GiraffeAnimation
        {
            Enter,
            Exit,
            Blink
        }
        private GiraffeAnimation currentAnim = GiraffeAnimation.Enter;
        private double animStartBeat = -1;
        private float animLength = 0;
        private EasingFunction.Ease currentEase = EasingFunction.Ease.Linear;

        public void GiraffeAnims(double beat, float length, int type, int ease)
        {
            animStartBeat = beat;
            animLength = length;
            currentAnim = (GiraffeAnimation)type;
            currentEase = (EasingFunction.Ease)ease;
            if (currentAnim == GiraffeAnimation.Blink) giraffe.DoScaledAnimationAsync("Blink", 0.5f);
        }

        public void Bop(double beat, float length, bool bop, bool autoBop)
        {
            if (bop)
            {
                List<BeatAction.Action> actions = new();
                for (int i = 0; i < length; i++)
                {
                    actions.Add(new BeatAction.Action(beat + i, delegate { SingleBop(); }));
                }
                BeatAction.New(this, actions);
            }
        }

        private void SingleBop()
        {
            if (!canBop) return;
            PlayMonkeyAnimationScaledAsync("Bop", 0.5f);
            player.Bop();
        }

        private bool IsEventAtBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("tapTrial", new string[] { "tap", "double tap", "triple tap", "jump tap" }).Find(x => x.beat >= beat && x.beat < endBeat) != null;
        }

        public void Tap(double beat)
        {
            canBop = false;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    PlayMonkeyAnimationScaledAsync("TapPrepare", 0.5f);
                    player.PrepareTap();
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync("Tap", 0.5f);
                    MonkeyParticles(true);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    if (!IsEventAtBeat(beat + 1, beat + 2)) canBop = true;
                })
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ook", beat),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1, 1.4f, 0.5f),
            });

            ScheduleInput(beat, 1, InputAction_BasicPress, JustTap, Miss, Empty);
        }

        public void DoubleTap(double beat)
        {
            canBop = false;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTapPrepare", 0.5f);
                    player.PrepareTap(true);
                }),
                new BeatAction.Action(beat + 0.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTapPrepare_2", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTap", 0.5f);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("DoubleTap", 0.5f);
                    MonkeyParticles(false);
                    if (!IsEventAtBeat(beat + 1, beat + 2)) canBop = true;
                }),
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ookook", beat),
                new MultiSound.Sound("tapTrial/ookook", beat + 0.5),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1, 1.4f, 0.5f),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1.5, 1.4f, 0.5f),
            });

            ScheduleInput(beat, 1, InputAction_BasicPress, JustDoubleTap, Miss, Empty);
            ScheduleInput(beat, 1.5, InputAction_BasicPress, JustDoubleTap, Miss, Empty);
        }

        public void TripleTap(double beat)
        {
            canBop = false;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    player.PrepareTripleTap(beat);
                    PlayMonkeyAnimationScaledAsync("PostPrepare_1", 0.5f);
                }),
                new BeatAction.Action(beat + 0.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostPrepare_2", 0.5f);
                }),
                new BeatAction.Action(beat + 2, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap", 0.5f);
                    MonkeyParticles(true);
                }),
                new BeatAction.Action(beat + 2.5, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap_2", 0.5f);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 3, delegate
                {
                    PlayMonkeyAnimationScaledAsync("PostTap", 0.5f);
                    MonkeyParticles(true);
                }),
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tapTrial/ooki1", beat),
                new MultiSound.Sound("tapTrial/ooki2", beat + 0.5),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 2, 1.4f, 0.5f),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 2.5, 1.4f, 0.5f),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 3, 1.4f, 0.5f),
            });

            ScheduleInput(beat, 2, InputAction_BasicPress, JustTripleTap, Miss, Empty);
            ScheduleInput(beat, 2.5, InputAction_BasicPress, JustTripleTap, Miss, Empty);
            ScheduleInput(beat, 3, InputAction_BasicPress, JustTripleTap, Miss, Empty);
        }

        public void JumpPrepare()
        {
            canBop = false;
            player.PrepareJump();
            PlayMonkeyAnimationScaledAsync("JumpPrepare", 0.5f);
        }

        public void JumpTap(double beat, bool final)
        {
            canBop = false;
            jumpStartBeat = beat;
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate
                {
                    player.Jump(final);
                    PlayMonkeyAnimationScaledAsync(final ? "Jump" : "JumpTap", 0.5f);
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    PlayMonkeyAnimationScaledAsync(final ? "FinalJumpTap" : "Jumpactualtap", 0.5f);
                    MonkeyParticles(true);
                    MonkeyParticles(false);
                }),
                new BeatAction.Action(beat + 1.5, delegate
                {
                    if (!IsEventAtBeat(beat + 1, beat + 2)) canBop = final;
                })
            });

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound(final ? "tapTrial/jumptap2" : "tapTrial/jumptap1", beat),
                new MultiSound.Sound("tapTrial/tapMonkey", beat + 1, 1.4f, 0.5f),
            });

            ScheduleInput(beat, 1, InputAction_BasicPress, final ? JustJumpTapFinal : JustJumpTap, final ? MissJumpFinal : MissJump, Empty);
        }

        private void JustJumpTap(PlayerActionEvent caller, float state)
        {
            player.JumpTap(state < 1f && state > -1f, false);
        }

        private void JustJumpTapFinal(PlayerActionEvent caller, float state)
        {
            player.JumpTap(state < 1f && state > -1f, true);
        }

        private void MissJump(PlayerActionEvent caller)
        {
            player.JumpTapMiss(false);
            if (giraffe.IsAnimationNotPlaying() && currentAnim != GiraffeAnimation.Exit) giraffe.DoScaledAnimationAsync("Miss", 0.5f);
            ResetScroll();
        }

        private void MissJumpFinal(PlayerActionEvent caller)
        {
            player.JumpTapMiss(true);
            if (giraffe.IsAnimationNotPlaying() && currentAnim != GiraffeAnimation.Exit) giraffe.DoScaledAnimationAsync("Miss", 0.5f);
            ResetScroll();
        }

        private void JustTap(PlayerActionEvent caller, float state)
        {
            player.Tap(state < 1f && state > -1f);
        }

        private void JustDoubleTap(PlayerActionEvent caller, float state)
        {
            player.Tap(state < 1f && state > -1f, true);
        }

        private void JustTripleTap(PlayerActionEvent caller, float state)
        {
            player.TripleTap(state < 1f && state > -1f);
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (giraffe.IsAnimationNotPlaying() && currentAnim != GiraffeAnimation.Exit) giraffe.DoScaledAnimationAsync("Miss", 0.5f);
            ResetScroll();
        }

        private void Empty(PlayerActionEvent caller) { }

        private void PlayMonkeyAnimationScaledAsync(string name, float timeScale)
        {
            monkeyL.DoScaledAnimationAsync(name, timeScale);
            monkeyR.DoScaledAnimationAsync(name, timeScale);
        }

        private void MonkeyParticles(bool left)
        {
            ParticleSystem spawnedEffectL = Instantiate(left ? monkeyTapLL : monkeyTapLR, transform);
            spawnedEffectL.Play();

            ParticleSystem spawnedEffectR = Instantiate(left ? monkeyTapRL : monkeyTapRR, transform);
            spawnedEffectR.Play();
        }
    }
}