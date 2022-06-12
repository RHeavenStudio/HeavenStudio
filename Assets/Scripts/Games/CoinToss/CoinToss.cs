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
            return new Minigame("coinToss", "Coin Toss \n [One coin at a time!]", "B4E6F6", false, false, new List<GameAction>()
            {
                new GameAction("toss",                 delegate { CoinToss.instance.TossCoin(eventCaller.currentEntity.beat, eventCaller.currentEntity.toggle); }, 7, false, parameters: new List<Param>()
                {
                    new Param("toggle", false, "Audience Reaction", "Enable Audience Reaction"),
                }),

                new GameAction("set background color",  delegate { var e = eventCaller.currentEntity; CoinToss.instance.ChangeBackgroundColor(e.colorA, 0f); }, 0.5f, false, new List<Param>()
                {
                    new Param("colorA", CoinToss.defaultBgColor, "Background Color", "The background color to change to")
                } ),
                new GameAction("fade background color", delegate { var e = eventCaller.currentEntity; CoinToss.instance.FadeBackgroundColor(e.colorA, e.colorB, e.length); }, 1f, true, new List<Param>()
                {
                    new Param("colorA", Color.white, "Start Color", "The starting color in the fade"),
                    new Param("colorB", CoinToss.defaultBgColor, "End Color", "The ending color in the fade")
                } ),

                new GameAction("set foreground color",  delegate { var e = eventCaller.currentEntity; CoinToss.instance.ChangeBackgroundColor(e.colorA, 0f, true); }, 0.5f, false, new List<Param>()
                {
                    new Param("colorA", CoinToss.defaultFgColor, "Background Color", "The background color to change to")
                } ),
                new GameAction("fade foreground color", delegate { var e = eventCaller.currentEntity; CoinToss.instance.FadeBackgroundColor(e.colorA, e.colorB, e.length, true); }, 1f, true, new List<Param>()
                {
                    new Param("colorA", Color.white, "Start Color", "The starting color in the fade"),
                    new Param("colorB", CoinToss.defaultFgColor, "End Color", "The ending color in the fade")
                } ),
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

        public void TossCoin(float beat, bool audienceReacting)
        {
            if (coin != null) return;

            //Play sound and animations
            Jukebox.PlayOneShotGame("coinToss/throw");
            handAnimator.Play("Throw", 0, 0);
            //Game state says the hand is throwing the coin
            isThrowing = true;

            this.audienceReacting = audienceReacting;

            coin = ScheduleInput(beat, 6f, InputType.STANDARD_DOWN, CatchSuccess, CatchMiss, CatchEmpty);
            //coin.perfectOnly = true;
        }

        public void CatchSuccess(PlayerActionEvent caller, float state)
        {
            Jukebox.PlayOneShotGame("coinToss/catch");
            if(this.audienceReacting) Jukebox.PlayOneShotGame("coinToss/applause");
            handAnimator.Play("Catch_success", 0, 0);

            isThrowing = false; 
        }

        public void CatchMiss(PlayerActionEvent caller)
        {
            Jukebox.PlayOneShotGame("coinToss/miss");
            if(this.audienceReacting) Jukebox.PlayOneShotGame("coinToss/disappointed");
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
