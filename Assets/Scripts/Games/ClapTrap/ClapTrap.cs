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
            return new Minigame("clapTrap", "Clap Trap", "FFf362B", false, false, new List<GameAction>()
            {
                new GameAction("clap", "Clap")
                {
                    function = delegate {var e = eventCaller.currentEntity; ClapTrap.instance.Clap(e.beat, e.length, e["sword"], e["spotlight"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("sword", ClapTrap.ClapType.Hand, "Object", "The evil, giant object attempting to karate chop the doll."),
                        //new Param("sighBeat", new EntityTypes.Float(2, 100), "Sigh Beat", "The slapper attempting to hit the doll"),
                        new Param("spotlight", true, "Spotlight", "Toggle if the spotlight should appear."),
                    }
                },
                new GameAction("doll animations", "Doll Animations")
                {
                    function = delegate {var e = eventCaller.currentEntity; ClapTrap.instance.DollAnimations(e.beat, e["animate"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("animate", ClapTrap.DollAnim.Inhale, "Animation", "The animation that the doll will play."),
                    }
                },
                new GameAction("spotlight", "Force Spotlight")
                {
                    function = delegate {var e = eventCaller.currentEntity; ClapTrap.instance.Spotlight(e["force"]);},
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("force", true, "Force Spotlight", "Toggle if the spotlight should appear."),
                    }
                },
                new GameAction("background color", "Background Colors")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClapTrap.instance.BackgroundColor(e["bgColor"], e["bgColorEnd"], e["ease"], e.length, e.beat); },
                    defaultLength = 0.5f,
                    resizable = true, 
                    parameters = new List<Param>()
                    {
                        new Param("bgColor", ClapTrap.defaultBgColor, "Start Color", "Set the color at the start of the event."),
                        new Param("bgColorEnd", ClapTrap.defaultBgColor, "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")

                    }, 
                },
                new GameAction("hand color", "Object Colors")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClapTrap.instance.ChangeHandColor(e["left"], e["right"], e["spotlightTop"], e["spotlightBottom"], e["spotlightGlow"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("left", ClapTrap.defaultLeftColor, "Left Hand", "The color for the doll's right hand."),
                        new Param("right", ClapTrap.defaultRightColor, "Right Hand", "The color for the doll's left hand."),
                        new Param("spotlightBottom", ClapTrap.defaultBgColor, "Spotlight Bottom", "The color for the bottom of the spotlight."),
                        new Param("spotlightTop", ClapTrap.glowSpotlight, "Spotlight Top", "The color for the top of the spotlight."),
                        new Param("spotlightGlow", ClapTrap.glowSpotlight, "Spotlight Glow", "The color for the glow around the spotlight.")
                    }, 
                },
                
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_ClapTrap;

    public class ClapTrap : Minigame
    {

        public enum ClapType
        {
            Hand,
            //Paw,
            //GreenOnion,
            //Branch,
            //Random
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

        // i stole these from rhythm tweezers lol
        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStart; //obviously put to the default color of the game
        private Color colorEnd;
        private Util.EasingFunction.Ease colorEase; //putting Util in case this game is using jukebox
        Tween bgColorTween;

        public SpriteRenderer stageLeft;

        public SpriteRenderer stageRight;

        public SpriteRenderer stageLeftRim;

        public SpriteRenderer stageRightRim;

        [Header("Spotlight")]
        public GameObject spotlight;
        public Material spotlightMaterial;

        [Header("Animators")]
        public Animator doll;
        public Animator dollHead;
        public Animator dollArms;
        public Animator dollBody;
        public Animator clapEffect;
        public Animator sword;

        [Header("Sword")]
        public GameObject swordObj;

        [Header("Shadows")]
        public GameObject shadowHead;
        public GameObject shadowLeftArm;
        public GameObject shadowLeftGlove;
        public GameObject shadowLeftGloveRim;
        public GameObject shadowRightArm;
        public GameObject shadowRightGlove;
        public GameObject shadowRightGloveRim;

        [Header("Properties")]
        private bool canClap = true;
        public int currentSpotlightClaps = 0;
        private Color backgroundColor;
        private bool forceSpotlight = false;
        
        void Awake()
        {
            instance = this;


            spotlightMaterial.SetColor("_ColorAlpha", glowSpotlight);
            spotlightMaterial.SetColor("_ColorBravo", glowSpotlight);
            spotlightMaterial.SetColor("_ColorDelta", defaultBgColor);

            backgroundColor = defaultBgColor;
            colorStart = defaultBgColor;
            colorEnd = defaultBgColor;
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && canClap == true && !IsExpectingInputNow(InputAction_BasicPress))
            {
                SoundByte.PlayOneShotGame($"clapTrap/clap");

                dollArms.DoScaledAnimationAsync("ArmsWhiff", 0.5f);
                clapEffect.DoScaledAnimationAsync("ClapEffect", 0.5f);

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Conductor.instance.songPositionInBeats, delegate { canClap = false; }),
                    new BeatAction.Action(Conductor.instance.songPositionInBeats + 0.4, delegate { canClap = true; })
                });
            }

            if (spotlight.activeSelf && currentSpotlightClaps == 0 && forceSpotlight == false)
            {
                spotlight.SetActive(false);
            }

            shadowHead.SetActive(spotlight.activeSelf);
            shadowLeftArm.SetActive(spotlight.activeSelf);
            shadowLeftGlove.SetActive(spotlight.activeSelf);
            shadowLeftGloveRim.SetActive(spotlight.activeSelf);
            shadowRightArm.SetActive(spotlight.activeSelf);
            shadowRightGlove.SetActive(spotlight.activeSelf);
            shadowRightGloveRim.SetActive(spotlight.activeSelf);
            
            if (spotlight.activeSelf) { dollBody.DoScaledAnimationAsync("BodyIdleLit", 0.5f); }
            else { dollBody.DoScaledAnimationAsync("BodyIdle", 0.5f);
            BackgroundColorUpdate();
            }
        }

        private void LateUpdate()
        {
            
        }


        public void Clap(double beat, float length, int type, bool spotlightBool)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("clapTrap/donk",  beat),
                new MultiSound.Sound("clapTrap/donk",  beat + length), 
                new MultiSound.Sound("clapTrap/donk",  beat + length * 2f),
                new MultiSound.Sound("clapTrap/whiff", beat + length * 3.5f),
            }, forcePlay: true);

            if (spotlightBool)
            {
                currentSpotlightClaps += 1;
                spotlight.SetActive(true);

                if (bg.color != Color.black)
                {
                    backgroundColor = bg.color;
                }
                bg.color = Color.black;
            }

            Sword swordClone = Instantiate(swordObj, gameObject.transform).GetComponent<Sword>();
            swordClone.cueLength = length * 4;
            swordClone.cueStart = beat;

            swordClone.cueType = Enum.GetName(typeof(ClapType), type);;
            swordClone.spotlightToggle = spotlightBool;
        }
        
        public void DollAnimations(double beat, int animate)
        {
            if (animate == 0)
            {
                dollHead.DoScaledAnimationAsync("HeadIdle", 0.5f);
            }
            else if (animate == 1)
            {
                dollHead.DoScaledAnimationAsync("HeadBreatheIn", 0.5f);
                SoundByte.PlayOneShotGame($"clapTrap/deepInhale");
            }
            else if (animate == 2)
            {
                dollHead.DoScaledAnimationAsync("HeadBreatheOut", 0.5f);
                SoundByte.PlayOneShotGame($"clapTrap/deepExhale{UnityEngine.Random.Range(1, 3)}");
            }
            else if (animate == 3)
            {
                dollHead.DoScaledAnimationAsync("HeadTalk", 0.5f);
            }
        }

        public void Spotlight(bool toggle)
        {
            forceSpotlight = toggle;

            if (forceSpotlight)
            {
                spotlight.SetActive(true);

                if (bg.color != Color.black)
                {
                    backgroundColor = bg.color;
                }
                bg.color = Color.black;
            }
        }

        public void ChangeBackgroundColor(Color color, float length)
        {
            var seconds = Conductor.instance.secPerBeat * length;

            if (bgColorTween != null)
                bgColorTween.Kill(true);
            
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

        public void BackgroundColor(Color start, Color end, int ease, float length, double beat)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStart = start;
            colorEnd = end;
            colorEase = (Util.EasingFunction.Ease)ease;

            /*if (fade)
            {
                ChangeBackgroundColor(bg.color, 0f);
                ChangeBackgroundColor(end, length - 0.0001f);
            }
            else
            {
                ChangeBackgroundColor(end, 0f);
            }*/
        }

        private void BackgroundColorUpdate() // stolen from tweezers too lol
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newR = func(colorStart.r, colorEnd.r, normalizedBeat);
            float newG = func(colorStart.g, colorEnd.g, normalizedBeat);
            float newB = func(colorStart.b, colorEnd.b, normalizedBeat);

            bg.color = new Color(newR, newG, newB);
        }

        public void ChangeHandColor(Color leftHand, Color rightHand, Color topSpot, Color bottomSpot, Color glowSpot)
        {
            stageLeft.color = leftHand;
            stageLeftRim.color = leftHand;
            stageRight.color = rightHand;
            stageRightRim.color = rightHand;

            spotlightMaterial.SetColor("_ColorAlpha", topSpot);
            spotlightMaterial.SetColor("_ColorBravo", glowSpot);
            spotlightMaterial.SetColor("_ColorDelta", bottomSpot);
        }

    }
}