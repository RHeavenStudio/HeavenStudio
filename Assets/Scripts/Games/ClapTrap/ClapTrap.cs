using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlClapTrapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("clapTrap", "Clap Trap \n<color=#eb5454>[WIP]</color>", "FFf362B", false, false, new List<GameAction>()
            {
                new GameAction("clap", "Clap")
                {
                    function = delegate {var e = eventCaller.currentEntity; ClapTrap.instance.Clap(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["object"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("object", ClapTrap.ClapType.Hand, "Object", "The object attempting to hit the doll"),
                        //new Param("sighBeat", new EntityTypes.Float(2, 100), "Sigh Beat", "The slapper attempting to hit the doll"),
                       // new Param("spotlight", true, "Spotlight", "Whether or not there's a spotlight for the cue"),
                    }
                },
                new GameAction("change background color", "Change Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClapTrap.instance.FadeBackgroundColor(e["bgColorA"], e["bgColorB"], e.length); },
                    resizable = true, 
                    parameters = new List<Param>()
                    {
                        new Param("bgColorA", ClapTrap.defaultBgColor, "Start Color", "The starting color in the fade"),
                        new Param("bgColorB", ClapTrap.defaultBgColor, "End Color", "The ending color in the fade")
                    }, 
                },
                new GameAction("change hand color", "Change Hand Colors")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClapTrap.instance.ChangeHandColor(e["stageLeftHandColor"], e["stageRightHandColor"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("stageLeftHandColor", ClapTrap.defaultLeftColor, "Left Hand Color", "The color used on the dummy's hand (stage left)"),
                        new Param("stageRightHandColor", ClapTrap.defaultRightColor, "Right Hand Color", "The color used on the dummy's hand (stage right)")
                    }, 
                },
                new GameAction("spotlight", "Change Spotlight Colors")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClapTrap.instance.ChangeSpotlightColor(e["spotlightTop"], e["spotlightBottom"], e["spotlightGlow"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("spotlightBottom", ClapTrap.bottomSpotlight, "Bottom Color", "The color at the bottom of the spotlight"),
                        new Param("spotlightTop", ClapTrap.topSpotlight, "Top Color", "The color at the top of the spotlight"),
                        new Param("spotlightGlow", ClapTrap.glowSpotlight, "Glow Color", "The color that glows around the spotlight")
                    },
                },
                new GameAction("doll animations", "Doll Animations")
                {
                    function = delegate {var e = eventCaller.currentEntity; ClapTrap.instance.DollAnimations(eventCaller.currentEntity.beat, eventCaller.currentEntity["animate"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("animate", ClapTrap.DollAnim.Inhale, "Animation", "The animation that the doll will play"),
                    }
                },
                
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public partial class ClapTrap : Minigame
    {
        public enum ClapType
        {
            Hand,
            Paw,
            Leek,
            Branch,
            Random
        }

        public enum DollAnim
        {
            Idle,
            Inhale,
            Exhale,
            Talk,

        }

        public static ClapTrap instance;

        private static Color _defaultBgColor;
        public static Color defaultBgColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFEF11", out _defaultBgColor);
                return _defaultBgColor;
            }
        }

        private static Color _defaultLeftColor;
        public static Color defaultLeftColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#10B5E7", out _defaultLeftColor);
                return _defaultLeftColor;
            }
        }

        private static Color _defaultRightColor;
        public static Color defaultRightColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#EC740F", out _defaultRightColor);
                return _defaultRightColor;
            }
        }

        private static Color _topSpotlight;
        public static Color topSpotlight
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFFFF", out _topSpotlight);
                return _topSpotlight;
            }
        }

        private static Color _bottomSpotlight;
        public static Color bottomSpotlight
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFE118", out _bottomSpotlight);
                return _bottomSpotlight;
            }
        }

        private static Color _glowSpotlight;
        public static Color glowSpotlight
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFFFF", out _glowSpotlight);
                return _glowSpotlight;
            }
        }

        [Header("Sprite Renderers")]
        [SerializeField] SpriteRenderer Background;

        [Header("Colors")]
        public SpriteRenderer bg;

        Tween bgColorTween;

        public SpriteRenderer stageLeft;

        public SpriteRenderer stageRight;

        public SpriteRenderer stageLeftRim;

        public SpriteRenderer stageRightRim;

        [Header("Spotlight")]
        public GameObject spotlight;
        public Material spotlightMaterial;

        [Header("Animators")]
        public Animator dollHead;
        public Animator dollArms;
        public Animator clapEffect;

        [Header("Values")]
        private bool canClap = true;
        private int currentClaps = 0;
        private Color backgroundColor;

        void Awake()
        {
            instance = this;

            spotlightMaterial.SetColor("_ColorAlpha", topSpotlight);
            spotlightMaterial.SetColor("_ColorBravo", glowSpotlight);
            spotlightMaterial.SetColor("_ColorDelta", bottomSpotlight);

            backgroundColor = defaultBgColor;
        }

        private void Update()
        {
            if (PlayerInput.Pressed() && canClap == true && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                Jukebox.PlayOneShotGame($"clapTrap/clap");

                dollArms.DoScaledAnimationAsync("ArmsWhiff", 0.5f);
                clapEffect.DoScaledAnimationAsync("ClapEffect", 0.5f);

                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Conductor.instance.songPositionInBeats, delegate { canClap = false; }),
                    new BeatAction.Action(Conductor.instance.songPositionInBeats + 1.2, delegate { canClap = true; })
                });
            }


        }

        private void LateUpdate()
        {
            if (spotlight.activeSelf == true && currentClaps == 0)
            {
                spotlight.SetActive(false);
                bg.color = backgroundColor;
            }
            print(currentClaps);
        }


        public void Clap(float beat, float length, int type)
        {

            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("clapTrap/donk", beat),
                new MultiSound.Sound("clapTrap/donk", beat + length), 
                new MultiSound.Sound("clapTrap/donk", beat + length * 2),
                new MultiSound.Sound("clapTrap/whiff", beat + length * 4, offset : (float)(Jukebox.GetClipLengthGame("clapTrap/whiff"))),
            }, forcePlay: true);
            ScheduleInput(beat, length * 4, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Hit, Miss, Out);

            currentClaps += 1;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length * 4, delegate { currentClaps -= 1; })
            });

            spotlight.SetActive(true);

            if (bg.color != Color.black)
            {
                backgroundColor = bg.color;
            }
            bg.color = Color.black;

        }


        private void Hit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) {
                Jukebox.PlayOneShotGame($"clapTrap/barely{UnityEngine.Random.Range(1, 2)}");
                dollHead.DoScaledAnimationAsync("HeadBarely", 0.5f);
            } 
            else if (state == 0f) {
                Jukebox.PlayOneShotGame($"clapTrap/aceClap{UnityEngine.Random.Range(1, 4)}");
                dollHead.DoScaledAnimationAsync("HeadHit", 0.5f);
            }
            else {
                Jukebox.PlayOneShotGame($"clapTrap/goodClap{UnityEngine.Random.Range(1, 4)}");
                dollHead.DoScaledAnimationAsync("HeadHit", 0.5f);
            }

            dollArms.DoScaledAnimationAsync("ArmsHit", 0.5f);
            clapEffect.DoScaledAnimationAsync("ClapEffect", 0.5f);
        }

        private void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame($"clapTrap/miss");
            dollHead.DoScaledAnimationAsync("HeadMiss", 0.5f);
            dollArms.DoScaledAnimationAsync("ArmsMiss", 0.5f);
        }

        private void Out(PlayerActionEvent caller) 
        {
            
        }

        public void DollAnimations(float beat, int animate)
        {
            if (animate == 0)
            {
                dollHead.DoScaledAnimationAsync("HeadIdle", 0.5f);
            }
            else if (animate == 1)
            {
                dollHead.DoScaledAnimationAsync("HeadBreatheIn", 0.5f);
                Jukebox.PlayOneShotGame($"clapTrap/deepInhale");
            }
            else if (animate == 2)
            {
                dollHead.DoScaledAnimationAsync("HeadBreatheOut", 0.5f);
                Jukebox.PlayOneShotGame($"clapTrap/deepExhale{UnityEngine.Random.Range(1, 2)}");
            }
            else if (animate == 3)
            {
                dollHead.DoScaledAnimationAsync("HeadTalk", 0.5f);
            }

        }


        public void ChangeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            {
                if (bgColorTween != null)
                    bgColorTween.Kill(true);
            }
            

            if (seconds == 0)
            {
                bg.color = color;
            }
            else
            {
                bgColorTween = bg.DOColor(color, seconds);
            }

                backgroundColor = color;
            
            
        }

        public void FadeBackgroundColor(Color start, Color end, float beats)
        {
            ChangeBackgroundColor(start, 0f);
            ChangeBackgroundColor(end, beats);

            
        }

        public void ChangeHandColor(Color stageLeftHandColor, Color stageRightHandColor)
        {
            stageLeft.color = stageLeftHandColor;
            stageLeftRim.color = stageLeftHandColor;
            stageRight.color = stageRightHandColor;
            stageRightRim.color = stageRightHandColor;
        }

        public void ChangeSpotlightColor(Color topSpotlightColor, Color bottomSpotlightColor, Color glowSpotlightColor)
        {
            spotlightMaterial.SetColor("_ColorAlpha", topSpotlightColor);
            spotlightMaterial.SetColor("_ColorBravo", glowSpotlightColor);
            spotlightMaterial.SetColor("_ColorDelta", bottomSpotlightColor);
        }


    }
}