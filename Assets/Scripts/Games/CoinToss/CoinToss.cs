using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrCoinLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("coinToss", "Coin Toss", "f9ec3b", false, false, new List<GameAction>()
            {
                new GameAction("toss", "Toss Coin")
                {
                    function = delegate { CoinToss.instance.TossCoin(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], eventCaller.currentEntity["toggle"]); }, 
                    defaultLength = 7, 
                    parameters = new List<Param>()
                    {
                        new Param("type", CoinToss.CoinVariation.Default, "Variation", "Special Coin Variations"),
                        new Param("toggle", false, "Audience Reaction", "Enable Audience Reaction"),
                    }
                },
                new GameAction("set background color", "Set Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; CoinToss.instance.ChangeBackgroundColor(e["colorA"], 0f); CoinToss.instance.ChangeBackgroundColor(e["colorB"], 0f, true); }, 
                    defaultLength = 0.5f,  
                    parameters = new List<Param>()
                    {
                        new Param("colorA", CoinToss.defaultBgColor, "Background Color", "The background color to change to"),
                        new Param("colorB", CoinToss.defaultFgColor, "Foreground Color", "The foreground color to change to")
                    } 
                },
                new GameAction("fade background color", "Fade Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; CoinToss.instance.FadeBackgroundColor(e["colorA"], e["colorB"], e.length); CoinToss.instance.FadeBackgroundColor(e["colorC"], e["colorD"], e.length, true); },
                    resizable = true, 
                    parameters = new List<Param>()
                    {
                        new Param("colorA", Color.white, "BG Start Color", "The starting color in the fade"),
                        new Param("colorB", CoinToss.defaultBgColor, "BG End Color", "The ending color in the fade"),
                        new Param("colorC", Color.white, "FG Start Color", "The starting color in the fade"),
                        new Param("colorD", CoinToss.defaultFgColor, "FG End Color", "The ending color in the fade")
                    } 
                },

                //left in for backwards-compatibility, but cannot be placed
                new GameAction("set foreground color", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; CoinToss.instance.ChangeBackgroundColor(e["colorA"], 0f, true); }, 
                    defaultLength = 0.5f, 
                    parameters = new List<Param>
            
                    {
                        new Param("colorA", CoinToss.defaultFgColor, "Foreground Color", "The foreground color to change to")

                    }, 
                    hidden = true 
                },

                new GameAction("fade foreground color", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; CoinToss.instance.FadeBackgroundColor(e["colorA"], e["colorB"], e.length, true); },
                    resizable = true, 
                    parameters = new List<Param>()
                    {
                        new Param("colorA", Color.white, "Start Color", "The starting color in the fade"),
                        new Param("colorB", CoinToss.defaultFgColor, "End Color", "The ending color in the fade")
                    }, 
                    hidden = true 
                },
            },
            new List<string>() {"ntr", "aim"},
            "ntrcoin", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_CoinToss;
    public class CoinToss : Minigame
    {

        //Right now, you can only throw one coin at a time.
        //..Which makes sense, you only have one coin in the original game

        //Though it would need a bit of code rewrite to make it work with multiple coins

        public static CoinToss instance { get; set; }

        private static Color _defaultBgColor;
        public static Color defaultBgColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#F7F742", out _defaultBgColor);
                return _defaultBgColor;
            }
        }

        
        private static Color _defaultFgColor;
        public static Color defaultFgColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFF83", out _defaultFgColor);
                return _defaultFgColor;
            }
        }

        [Header("Backgrounds")]
        public SpriteRenderer fg;
        public SpriteRenderer bg;

        Tween bgColorTween;
        Tween fgColorTween;

        [Header("Animators")]
        public Animator handAnimator;

        public Boolean isThrowing;

        public bool audienceReacting;

        public PlayerActionEvent coin;

        public enum CoinVariation
        {
            Default,
            Cowbell,
        }

        private void Awake()
        {
            instance = this;
            isThrowing = false;

            coin = null;
        }

        private void Update()
        {
           //nothing
        }

        private void LateUpdate()
        {
            //nothing
        }

        public void TossCoin(double beat, int type, bool audienceReacting)
        {
            if (coin != null) return;

            //Play sound and animations
            SoundByte.PlayOneShotGame("coinToss/throw");
            handAnimator.Play("Throw", 0, 0);
            //Game state says the hand is throwing the coin
            isThrowing = true;

            switch (type)
                {
                    case (int) CoinToss.CoinVariation.Cowbell:
                        //this was intentional. it was to avoid the throw and cowbells to go offbeat.
                        SoundByte.PlayOneShotGame("coinToss/cowbell1");
                        MultiSound.Play(new MultiSound.Sound[] {
                        new MultiSound.Sound("coinToss/cowbell2", beat + 1f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell1", beat + 2f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell2", beat + 3f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell1", beat + 4f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell2", beat + 5f, offset: 0.01f),
                        new MultiSound.Sound("coinToss/cowbell1", beat + 6f, offset: 0.01f),
                        });
                        break;
                    default:
                        break;
                }
            
            this.audienceReacting = audienceReacting;

            coin = ScheduleInput(beat, 6f, InputType.STANDARD_DOWN, CatchSuccess, CatchMiss, CatchEmpty);
            //coin.perfectOnly = true;
        }

        public void TossCoin(double beat)
        {
            if (coin != null) return;

            //Play sound and animations
            SoundByte.PlayOneShotGame("coinToss/throw");
            handAnimator.Play("Throw", 0, 0);
            //Game state says the hand is throwing the coin
            isThrowing = true;
            this.audienceReacting = false;

            coin = ScheduleInput(beat, 6f, InputType.STANDARD_DOWN, CatchSuccess, CatchMiss, CatchEmpty);
            //coin.perfectOnly = true;
        }

        public void CatchSuccess(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("coinToss/catch");
            if(this.audienceReacting) SoundByte.PlayOneShot("applause");
            handAnimator.Play("Catch_success", 0, 0);

            isThrowing = false; 
        }

        public void CatchMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShot("miss");
            if(this.audienceReacting) SoundByte.PlayOneShot("audience/disappointed");
            handAnimator.Play("Pickup", 0, 0);

            isThrowing = false;
        }

        public void CatchEmpty(PlayerActionEvent caller)
        {
            handAnimator.Play("Catch_empty", 0, 0);
            isThrowing = false;

            coin.CanHit(false);
        }

        public void ChangeBackgroundColor(Color color, float beats, bool isFg = false)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if(!isFg)
            {
                if (bgColorTween != null)
                    bgColorTween.Kill(true);
            } else
            {
                if (fgColorTween != null)
                    fgColorTween.Kill(true);
            }
            

            if (seconds == 0)
            {
                if(!isFg) bg.color = color;
                if (isFg) fg.color = color;
            }
            else
            {
                if(!isFg) bgColorTween = bg.DOColor(color, seconds);
                if(isFg)  fgColorTween = fg.DOColor(color, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats, bool isFg = false)
        {
            ChangeBackgroundColor(start, 0f, isFg);
            ChangeBackgroundColor(end, beats, isFg);
        }
    }
}
