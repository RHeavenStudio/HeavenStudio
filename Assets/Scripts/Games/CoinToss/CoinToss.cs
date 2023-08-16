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
                new GameAction("fade background color", "Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity;
                        CoinToss.instance.BackgroundColor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["colorStartF"], e["colorEndF"], e["ease"]); },
                    resizable = true, 
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", CoinToss.defaultBgColor, "BG Start Color", "The starting color in the fade"),
                        new Param("colorEnd", CoinToss.defaultBgColor, "BG End Color", "The ending color in the fade"),
                        new Param("colorStartF", CoinToss.defaultBgColor, "FG Start Color", "The starting color in the fade"),
                        new Param("colorEndF", CoinToss.defaultBgColor, "FG End Color", "The ending color in the fade"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
                    } 
                },

                //left in for backwards-compatibility, but cannot be placed
                new GameAction("set background color", "Set Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; CoinToss.instance.BackgroundColor(e.beat, e.length, e["colorA"], e["colorA"], e["colorB"], e["colorB"], (int)Util.EasingFunction.Ease.Instant); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", CoinToss.defaultBgColor, "Background Color", "The background color to change to"),
                        new Param("colorB", CoinToss.defaultFgColor, "Foreground Color", "The foreground color to change to")
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

            colorStart = defaultBgColor;
            colorEnd = defaultBgColor;
            colorStartF = defaultBgColor;
            colorEndF = defaultBgColor;
            BackgroundColorUpdate();
        }

        private void Update()
        {
            BackgroundColorUpdate();
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

        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStart; //obviously put to the default color of the game
        private Color colorEnd;
        private Color colorStartF; //obviously put to the default color of the game
        private Color colorEndF;
        private Util.EasingFunction.Ease colorEase; //putting Util in case this game is using jukebox

        //call this in update
        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newR = func(colorStart.r, colorEnd.r, normalizedBeat);
            float newG = func(colorStart.g, colorEnd.g, normalizedBeat);
            float newB = func(colorStart.b, colorEnd.b, normalizedBeat);

            bg.color = new Color(newR, newG, newB);

            float newRF = func(colorStartF.r, colorEndF.r, normalizedBeat);
            float newGF = func(colorStartF.g, colorEndF.g, normalizedBeat);
            float newBF = func(colorStartF.b, colorEndF.b, normalizedBeat);

            fg.color = new Color(newRF, newGF, newBF);
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, Color colorStartSetF, Color colorEndSetF, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStart = colorStartSet;
            colorEnd = colorEndSet;
            colorStartF = colorStartSetF;
            colorEndF = colorEndSetF;
            colorEase = (Util.EasingFunction.Ease)ease;
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("coinToss", new string[] { "fade background color" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["colorStartF"], lastEvent["colorEndF"], lastEvent["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistColor(beat);
        }
    }
}
