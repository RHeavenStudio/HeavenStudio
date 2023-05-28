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
                    function = delegate {var e = eventCaller.currentEntity; ClapTrap.instance.Clap(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity["slapper"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("slapper", ClapTrap.ClapType.Hand, "Which slapper?", "The thing attempting to hit the dummy"),
                    }
                },
                new GameAction("change background color", "Change Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClapTrap.instance.FadeBackgroundColor(e["colorA"], e["colorB"], e.length); },
                    resizable = true, 
                    parameters = new List<Param>()
                    {
                        new Param("colorA", ClapTrap.defaultBgColor, "Start Color", "The starting color in the fade"),
                        new Param("colorB", ClapTrap.defaultBgColor, "End Color", "The ending color in the fade")
                    }, 
                },
                new GameAction("change hand color", "Change Hand Colors")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClapTrap.instance.ChangeHandColor(e["stageLeftHandColor"], e["stageRightHandColor"]); },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("stageLeftHandColor", ClapTrap.defaultLeftColor, "Stage Left Hand Color", "The color used on the dummy's hand (stage left)"),
                        new Param("stageRightHandColor", ClapTrap.defaultRightColor, "Stage Right Hand Color", "The color used on the dummy's hand (stage right)")
                    }, 
                },
                new GameAction("delete", "animate girn")
                {
                    function = delegate {var e = eventCaller.currentEntity; ClapTrap.instance.Grin(eventCaller.currentEntity.beat);},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public partial class ClapTrap : Minigame
    {
        [Header("Sprite Renderers")]
        [SerializeField] SpriteRenderer Background;

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

        [Header("Colors")]
        public SpriteRenderer bg;

        Tween bgColorTween;

        public SpriteRenderer stageLeft;

        public SpriteRenderer stageRight;

        public SpriteRenderer stageLeftRim;

        public SpriteRenderer stageRightRim;

        public enum ClapType
        {
            Hand,
            Cat,
            Stick,
            Random,
        }

        [Header("Animators")]
        public Animator dollHead;


        public void Clap(float beat, float length, int type)
        {

            MultiSound.Play(new MultiSound.Sound[] { 
                new MultiSound.Sound("clapTrap/donk", beat),
                new MultiSound.Sound("clapTrap/donk", beat + length), 
                new MultiSound.Sound("clapTrap/donk", beat + length * 2),
                new MultiSound.Sound("clapTrap/whiff", beat + length * 4, offset : (float)(Jukebox.GetClipLengthGame("clapTrap/whiff"))),
            }, forcePlay: true);
            ScheduleInput(beat, length * 4, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Hit, Miss, Out);

        }

        public void Grin(float beat)
        {
            dollHead.DoScaledAnimationAsync("HeadHit", 0.5f);

        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f) {
                Jukebox.PlayOneShotGame($"clapTrap/barely{UnityEngine.Random.Range(1, 2)}");
            } 
            else if (state == 0f) {
                Jukebox.PlayOneShotGame($"clapTrap/aceClap{UnityEngine.Random.Range(1, 4)}");
                dollHead.DoScaledAnimationAsync("HeadHit", 0.5f);
            }
            else {
                Jukebox.PlayOneShotGame($"clapTrap/goodClap{UnityEngine.Random.Range(1, 4)}");
                dollHead.DoScaledAnimationAsync("HeadHit", 0.5f);
            }
        }

        private void Miss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame($"clapTrap/miss");
        }

        private void Out(PlayerActionEvent caller) 
        {
            
        }

        void Awake()
        {
            instance = this;
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

        
    }
}