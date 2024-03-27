using NaughtyBezierCurves;
using HeavenStudio.Common;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    using Jukebox;
    public static class CtrChargingChickenLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("chargingChicken", "Charging Chicken", "6ED6FF", false, false, new List<GameAction>()
            {
                new GameAction("input", "Charge")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ChargeUp(e.beat, e.length, 4 /*e["forceHold"]*/, e["drumbeat"], e["bubble"], e["endText"], e["textLength"], e["success"], e["fail"], e["destination"], e["customDestination"], e["spaceHelmet"]);
                        }
                        ChargingChicken.CountIn(e.beat, e["cowbell"]);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("cowbell", true, "Cue Sound", "Choose whether to play the cue sound for this charge."),
                        new Param("spaceHelmet", false, "Space Helmet", "Choose whether the chicken wears its trusty space helmet while driving."),
                        new Param("drumbeat", ChargingChicken.DrumLoopList.Straight, "Drum Beat", "Choose which drum beat to play while filling."),
                        new Param("bubble", false, "Countdown Bubble", "Choose whether the counting bubble will spawn for this input."),
                        //ending text
                        new Param("endText", ChargingChicken.TextOptions.None, "Ending Text", "What text will appear once the ending platform is reached.", new() {
                            new Param.CollapseParam((x, _) => (int)x != (int)0, new[] { "textLength" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)1, new[] { "success", "fail" }),
                            new Param.CollapseParam((x, _) => (int)x == (int)2, new[] { "destination" }),
                        }),
                        new Param("textLength", new EntityTypes.Integer(1, 16, 4), "Text Stay Length", "How long the text will stay after the ending platform is reached."),
                        //praise
                        new Param("success", "Well Done!", "Success Text", "Text to display if the input is hit."),
                        new Param("fail", "Too bad...", "Fail Text", "Text to display if the input is missed."),
                        //destination
                        new Param("destination", ChargingChicken.Destinations.Seattle, "Destination", "Which destination will be reached once the ending platform is reached.", new() {
                            new Param.CollapseParam((x, _) => (int)x == (int)0, new[] { "customDestination" }),
                        }),
                        new Param("customDestination", "You arrived in The Backrooms!", "Custom Destination", "Custom text to display once the ending platform is reached."),
                    },
                    defaultLength = 8,
                    resizable = true,
                    preFunctionLength = 4,
                },
                new GameAction("bubbleShrink", "Shrink Countdown Bubble")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.BubbleShrink(e.beat, e.length, e["grow"], e["instant"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("grow", false, "Grow Bubble", "Make the bubble grow instead."),
                        new Param("instant", false, "Instant", "Make the bubble appear or disappear instantly."),
                    },
                    defaultLength = 4,
                    resizable = true,
                },
                new GameAction("textEdit", "Edit Cue Text")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.TextEdit(e.beat, e["text"], e["color"]);
                        }
                    },
                    parameters = new List<Param>()
                    {
                        new Param("text", "# yards to the goal.", "Cue Text", "The text to display for a cue ('#' is the length of the cue in beats)."),
                        new Param("color", ChargingChicken.defaultHighlightColor, "Highlight Color", "Set the color of the cue number."),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("musicFade", "Fade Music")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.MusicFade(e.beat, e.length, e["fadeIn"], e["instant"], e["drums"], e["reset"]);
                        }
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("instant", false, "Instant", "Whether the fade is instant."),
                        new Param("fadeIn", false, "Fade In", "Fade the music back in."),
                        new Param("drums", true, "Affect Drums", "Whether to affect the volume of the charging drums."),
                        new Param("reset", true, "Reset After Blastoff", "Whether to reset the volume of the music after a charge input is over."),
                    }
                },
                new GameAction("changeCarColor", "Car Appearance")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ChangeCarColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["ease"]);
                        }
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorFrom", ChargingChicken.defaultCarColor, "Idle Color Start", "Set the car's resting color at the start of the event."),
                        new Param("colorTo", ChargingChicken.defaultCarColor, "Idle Color End", "Set the car's resting color at the end of the event."),
                        new Param("colorFrom2", ChargingChicken.defaultCarColorCharged, "Charged Color Start", "Set the car's charged color at the start of the event."),
                        new Param("colorTo2", ChargingChicken.defaultCarColorCharged, "Charged Color End", "Set the car's charged color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new() {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new[] { "colorFrom", "colorFrom2" }),
                        }),
                    }
                },
                new GameAction("changeBgColor", "Background Appearance")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ChangeColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["ease"]);
                        }
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorFrom", ChargingChicken.defaultBGColor, "Color A Start", "Set the top-most color of the background gradient at the start of the event."),
                        new Param("colorTo", ChargingChicken.defaultBGColor, "Color A End", "Set the top-most color of the background gradient at the end of the event."),
                        new Param("colorFrom2", ChargingChicken.defaultBGColorBottom, "Color B Start", "Set the bottom-most color of the background gradient at the start of the event."),
                        new Param("colorTo2", ChargingChicken.defaultBGColorBottom, "Color B End", "Set the bottom-most color of the background gradient at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new() {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new[] { "colorFrom", "colorFrom2" }),
                        }),
                    }
                },
                new GameAction("unParallaxObjects", "Background Objects")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.UnParallaxObjects(e.beat, e.length, e["appearance"], e["instant"]);
                        }
                    },
                    resizable = true,
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("instant", false, "Instant", "Whether the appearance immediately changes."),
                        new Param("appearance", ChargingChicken.Backgrounds.Sky , "Appearance", "Set the appearance of the background."),
                    }
                },
                new GameAction("changeCloudColor", "Midground Appearance")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ChangeCloudColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["ease"]);
                        }
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorFrom", ChargingChicken.defaultCloudColor, "Primary Color Start", "Set the midground's primary color at the start of the event. (Used in: Clouds, Doodles)"),
                        new Param("colorTo", ChargingChicken.defaultCloudColor, "Primary Color End", "Set the midground's primary color at the start of the event. (Used in: Clouds, Doodles)"),
                        new Param("colorFrom2", ChargingChicken.defaultCloudColorBottom, "Secondary Color Start", "Set the midground's secondary color at the start of the event. (Used in: Clouds, Doodles)"),
                        new Param("colorTo2", ChargingChicken.defaultCloudColorBottom, "Secondary Color End", "Set the midground's secondary color at the start of the event. (Used in: Clouds, Doodles)"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new() {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new[] { "colorFrom", "colorFrom2" }),
                        }),
                    }
                },
                new GameAction("parallaxObjects", "Midground Objects")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ParallaxObjects(e.beat, e.length, e["instant"], e["stars"], e["clouds"], e["earth"], e["mars"], e["doodles"], e["birds"]);
                        }
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("instant", false, "Instant", "Whether the objects immediately appear."),
                        new Param("stars", false, "Stars", "Whether the stars will be visible by the end of the block."),
                        new Param("clouds", true, "Clouds", "Whether the clouds will be visible by the end of the block."),
                        new Param("earth", false, "Earth", "Whether the Earth will be visible by the end of the block."),
                        new Param("mars", false, "Mars", "Whether Mars will be visible by the end of the block."),
                        new Param("doodles", false, "Doodles", "Whether the doodles will be visible by the end of the block."),
                        new Param("birds", false, "Birds", "Whether the birds will be visible by the end of the block."),
                    }
                },
                new GameAction("changeFgLight", "Foreground Appearance")
                {
                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ChangeLight(e.beat, e.length, e["lightFrom"], e["lightTo"], e["headLightFrom"], e["headLightTo"], e["ease"]);
                        }
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("lightFrom", new EntityTypes.Float(0, 1, 1), "Scene Brightness Start", "Set the brightness of the foreground at the start of the event."),
                        new Param("lightTo", new EntityTypes.Float(0, 1, 1), "Scene Brightness End", "Set the brightness of the foreground at the end of the event."),
                        new Param("headLightFrom", new EntityTypes.Float(0, 1, 0), "Headlight Brightness Start", "Set the brightness of the car's headlights at the start of the event."),
                        new Param("headLightTo", new EntityTypes.Float(0, 1, 0), "Headlight Brightness End", "Set the brightness of the car's headlights at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.", new() {
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new[] { "lightFrom", "headLightFrom" }),
                        }),
                    }
                },
                new GameAction("parallaxProgress", "Parallax Progress")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ParallaxProgress(e["starProgress"], e["cloudProgress"], e["planetProgress"], e["doodleProgress"], e["birdProgress"]);
                        }
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("starProgress", new EntityTypes.Integer(-1, 100, -1), "Star Progress", "Instantly sets what percent through the loop the stars are. (-1 = no change)"),
                        new Param("cloudProgress", new EntityTypes.Integer(-1, 100, -1), "Cloud Progress", "Instantly sets what percent through the loop the clouds are. (-1 = no change)"),
                        new Param("planetProgress", new EntityTypes.Integer(-1, 100, -1), "Planet Progress", "Instantly sets what percent through the loop the Earth and Mars are. (-1 = no change)"),
                        new Param("doodleProgress", new EntityTypes.Integer(-1, 100, -1), "Doodle Progress", "Instantly sets what percent through the loop the doodles are. (-1 = no change)"),
                        new Param("birdProgress", new EntityTypes.Integer(-1, 100, -1), "Bird Progress", "Instantly sets what percent through the loop the birds are. (-1 = no change)"),
                    }
                },
                new GameAction("lookhaha", "Look At Camera")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.LookButFunee(e.beat, e.length);
                        }
                    },
                    resizable = true,
                    defaultLength = 2f,
                },
                new GameAction("explodehaha", "Force Explosion")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ExplodeButFunee();
                        }
                    },
                    defaultLength = 0.5f,
                },
                },
                new List<string>() { "ctr", "aim" },
                "ctrChargingChicken", "en",
                new List<string>() { "en" }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_ChargingChicken;
    public class ChargingChicken : Minigame
    {
        //definitions
        #region Definitions

        [SerializeField] SpriteRenderer gradient;
        [SerializeField] SpriteRenderer bgLow;
        [SerializeField] SpriteRenderer bgHigh;
        [SerializeField] Animator ChickenAnim;
        [SerializeField] Animator WaterAnim;
        [SerializeField] Animator ParallaxFade;
        [SerializeField] Animator UnParallaxFade;
        [SerializeField] Animator BirdsAnim;
        [SerializeField] Transform Stars;
        [SerializeField] Transform Clouds;
        [SerializeField] Transform Planets;
        [SerializeField] Transform Doodles;
        [SerializeField] Transform Birds;
        [SerializeField] TMP_Text yardsText;
        [SerializeField] TMP_Text endingText;
        [SerializeField] TMP_Text bubbleText;
        [SerializeField] GameObject countBubble;
        [SerializeField] GameObject Helmet;
        [SerializeField] GameObject FallingHelmet;
        [SerializeField] Material chickenColors;
        [SerializeField] Material chickenColorsCar;
        [SerializeField] Material chickenColorsCloud;
        [SerializeField] Material chickenColorsDoodles;
        [SerializeField] Material chickenColorsWater;
        [SerializeField] SpriteRenderer headlightColor;

        public enum TextOptions
        {
            None,
            Praise,
            Destination,
        }
        public enum Destinations
        {
            Custom,
            //early locations 1 - 14
            Seattle,
            Mexico,
            Brazil,
            France,
            England,
            Italy,
            Egypt,
            Turkey,
            Dubai,
            India,
            Thailand,
            China,
            Japan,
            Australia,
            //later locations 15 - 20
            TheMoon, //15
            Mars,
            Jupiter,
            Uranus,
            TheEdgeOfTheGalaxy, //19
            TheFuture, //20
        }

        public enum Backgrounds
        {
            Sky,
            Galaxy,
            Future,
        }

        bool isInputting = false;
        bool canPressWhiff = false;
        bool canBlastOff = false;
        bool playerSucceeded = false;
        bool fellTooFar = false;
        bool checkFallingDistance = false;
        double successAnimationKillOnBeat = double.MaxValue;

        bool flowForward = true;
        int lastBg = 0;
        bool starsVisible = false;
        bool cloudsVisible = true;
        bool earthVisible = false;
        bool marsVisible = false;
        bool doodlesVisible = false;
        bool birdsVisible = false;

        double bgColorStartBeat = -1;
        float bgColorLength = 0;
        double fgLightStartBeat = -1;
        float fgLightLength = 0;
        double carColorStartBeat = -1;
        float carColorLength = 0;
        double cloudColorStartBeat = -1;
        float cloudColorLength = 0;
        Util.EasingFunction.Ease lastEase1;
        Util.EasingFunction.Ease lastEase2;
        Util.EasingFunction.Ease lastEase3;
        Util.EasingFunction.Ease lastEase4;
        Color colorFrom;
        Color colorTo;
        Color colorFrom2;
        Color colorTo2;
        float lightFrom = 1;
        float lightTo = 1;
        float lightFrom2 = 0;
        float lightTo2 = 0;
        Color carColorFrom;
        Color carColorTo;
        Color carColorFrom2;
        Color carColorTo2;
        Color cloudColorFrom;
        Color cloudColorTo;
        Color cloudColorFrom2;
        Color cloudColorTo2;
        bool colorsCanUpdate = false;

        double bubbleEndCount = 0;
        double bubbleSizeChangeStart = 0;
        double bubbleSizeChangeEnd = 0;
        bool bubbleSizeChangeGrows = false;

        string yardsTextString = "# yards to the goal.";
        bool yardsTextIsEditable = false;
        double yardsTextLength = 0;
        double carColorChangeReady = 0;
        double carColorChangeLength = 0;
        private static Color _defaultHighlightColor;
        public static Color defaultHighlightColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFF00", out _defaultHighlightColor);
                return _defaultHighlightColor;
            }
        }

        float drumVolume = 1;
        float drumTempVolume = 1;
        double drumFadeStart = 0;
        double drumFadeLength = 0;
        bool drumFadeIn = true;
        bool drumReset = true;
        bool drumLoud = false;
        double drumSwitch = double.MinValue;

        Sound whirring;
        bool isWhirringPlaying = false;

        [SerializeField] Island IslandBase;
        Island nextIsland;
        Island currentIsland;
        Island staleIsland;
        double journeyIntendedLength;

        public static double platformDistanceConstant = 5.35 / 2;
        public static int platformsPerBeat = 4;

        float forgivenessConstant = 1.3f;

        double nextInputReady = 0;

        public enum DrumLoopList
        {
            None,
            Straight,
            SwungSixteenth,
            SwungEighth,
            Triplet,
            FeverDrumKit,
            DSDrumKit,
            GBADrumKit,
            AmenBreak,
            Remix2GBA,
            PracticeDrumKit,
        }

        private static Color _defaultBGColor;
        public static Color defaultBGColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#6ED6FF", out _defaultBGColor);
                return _defaultBGColor;
            }
        }
        private static Color _defaultBGColorBottom;
        public static Color defaultBGColorBottom
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFFFF", out _defaultBGColorBottom);
                return _defaultBGColorBottom;
            }
        }

        private static Color _defaultCarColor;
        public static Color defaultCarColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#F4DB2E", out _defaultCarColor);
                return _defaultCarColor;
            }
        }
        private static Color _defaultCarColorCharged;
        public static Color defaultCarColorCharged
        {
            get
            {
                ColorUtility.TryParseHtmlString("#F42E25", out _defaultCarColorCharged);
                return _defaultCarColorCharged;
            }
        }

        private static Color _defaultCloudColor;
        public static Color defaultCloudColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFFFF", out _defaultCloudColor);
                return _defaultCloudColor;
            }
        }
        private static Color _defaultCloudColorBottom;
        public static Color defaultCloudColorBottom
        {
            get
            {
                ColorUtility.TryParseHtmlString("#C8F0F0", out _defaultCloudColorBottom);
                return _defaultCloudColorBottom;
            }
        }

        //drum loops
        #region DrumLoops

        private struct DrumLoop : IComparable<DrumLoop>
        {
            // override object.Equals
            public override bool Equals(object obj)
            {
                //
                // See the full list of guidelines at
                //   http://go.microsoft.com/fwlink/?LinkID=85237
                // and also the guidance for operator== at
                //   http://go.microsoft.com/fwlink/?LinkId=85238
                //
                
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                
                // TODO: write your implementation of Equals() here
                throw new System.NotImplementedException();
            }
            
            // override object.GetHashCode
            public override int GetHashCode()
            {
                // TODO: write your implementation of GetHashCode() here
                throw new System.NotImplementedException();
            }
            public int CompareTo(DrumLoop other)
            {
                if (other == null) return 1;

                return timing.CompareTo(other.timing);
            }

            public static bool operator > (DrumLoop operand1, DrumLoop operand2)
            {
                return operand1.CompareTo(operand2) > 0;
            }

            public static bool operator < (DrumLoop operand1, DrumLoop operand2)
            {
                return operand1.CompareTo(operand2) < 0;
            }

            public static bool operator >=(DrumLoop operand1, DrumLoop operand2)
            {
                return operand1.CompareTo(operand2) >= 0;
            }

            public static bool operator <=(DrumLoop operand1, DrumLoop operand2)
            {
                return operand1.CompareTo(operand2) <= 0;
            }

            public static bool operator ==(DrumLoop operand1, DrumLoop operand2)
            {
                return operand1.CompareTo(operand2) == 0;
            }

            public static bool operator !=(DrumLoop operand1, DrumLoop operand2)
            {
                return operand1.CompareTo(operand2) != 0;
            }
            public int drumType;
            public double timing;
            public float volume;

            public DrumLoop(double timing, int drumType, float volume = 1)
            {
                this.drumType = drumType;
                this.timing = timing;
                this.volume = volume;
            }
        }

        private readonly DrumLoop[][] drumLoops = new DrumLoop[][] { 

            new DrumLoop[] {}, //silent

            new DrumLoop[] { //straight
                //kick
                new(4.00, 0),
                new(0.50, 0),
                new(1.75, 0),
                new(2.50, 0),
                //snare
                new(1.00, 1),
                new(3.00, 1),
                //loud hihat
                new(4.00, 2),
                new(1.00, 2),
                new(2.00, 2),
                new(3.00, 2),
                //quiet hihat
                new(0.50, 2, 0.7f),
                new(1.50, 2, 0.7f),
                new(2.50, 2, 0.7f),
                new(3.50, 2, 0.7f),
            },

            new DrumLoop[] { //swungsixteenth
                //kick
                new(4.00, 0),
                new(0.50, 0),
                new((double)20/6, 0),
                new(2.50, 0),
                //snare
                new(1.00, 1),
                new(3.00, 1),
                //loud hihat
                new(4.00, 2),
                new(1.00, 2),
                new(2.00, 2),
                new(3.00, 2),
                //quiet hihat
                new(0.50, 2, 0.7f),
                new(1.50, 2, 0.7f),
                new(2.50, 2, 0.7f),
                new(3.50, 2, 0.7f),
                //silent hihat
                new((double) 2/6, 2, 0.5f),
                new((double) 5/6, 2, 0.5f),
                new((double) 8/6, 2, 0.5f),
                new((double)11/6, 2, 0.5f),
                new((double)14/6, 2, 0.5f),
                new((double)17/6, 2, 0.5f),
                new((double)20/6, 2, 0.5f),
                new((double)23/6, 2, 0.5f),
            },

            new DrumLoop[] { //swungeighth
                //kick
                new(4.00, 0),
                new((double)2/3, 0),
                new((double)5/3, 0),
                new((double)8/3, 0),
                //snare 
                new(1.00, 1),
                new(3.00, 1),
                //loud hihat
                new(4.00, 2),
                new(1.00, 2),
                new(2.00, 2),
                new(3.00, 2),
                //quiet hihat
                new((double) 2/3, 2, 0.7f),
                new((double) 5/3, 2, 0.7f),
                new((double) 8/3, 2, 0.7f),
                new((double)11/3, 2, 0.7f),
            },

            new DrumLoop[] { //triplet
                //kick
                new(4.00, 0),
                new((double) 2/3, 0),
                new((double) 5/3, 0),
                new(2.00, 0),
                new((double) 8/3, 0),
                //snare 
                new((double) 4/3, 1),
                new(3.00, 1),
                //loud hihat
                new(4.00, 2),
                new((double) 4/3, 2),
                new(2.00, 2),
                new(3.00, 2),
                //quiet hihat
                new((double) 1/3, 2, 0.7f),
                new(1.00, 2, 0.7f),
                new((double) 5/3, 2, 0.7f),
                new((double) 7/3, 2, 0.7f),
                new((double) 8/3, 2, 0.7f),
                new((double)11/3, 2, 0.7f),
            },

            new DrumLoop[] { //fever kit
                new(2.00, 3, 0.8f), //kick
                new(0.50, 5, 0.7f), //hat
                new(1.00, 4, 1.2f), //snare
                new(1.50, 5, 0.7f), //hat
            },

            new DrumLoop[] { //ds kit
                //kick
                new(4.00, 6),
                new(2.00, 6),
                //snare 
                new(1.00, 7),
                new(3.00, 7),
                //hihat
                new(0.50, 8),
                new(1.50, 8),
                new(2.50, 8),
                new(3.50, 8),
                //quiet drums
                new((double)11/6, 8, 0.4f),
                new((double)23/6, 7, 0.4f),
            },

            new DrumLoop[] { //gba kit
                //kick
                new(2.00, 9),
                new(0.75, 9),
                //snare 
                new(1.00, 10),
                //hihat
                new(0.50, 11),
                new(1.50, 11),
            },

            new DrumLoop[] { //amen
                new(4.00, 21),
                new(0.50, 22),
                new(1.00, 23),
                new(1.50, 24),
                new(1.75, 25),
                new(2.00, 26),
                new(2.25, 27),
                new(2.50, 28),
                new(2.75, 29),
                new(3.00, 30),
                new(3.50, 31),
                new(3.75, 32),
            },

            new DrumLoop[] { //remix 2 gba
                new(4.00, 41, 1.5f),
                new(0.25, 42, 1.5f),
                new(0.50, 43, 1.5f),
                new(0.75, 44, 1.5f),
                new(1.00, 45, 1.5f),
                new(1.25, 46, 1.5f),
                new(1.50, 47, 1.5f),
                new(1.75, 48, 1.5f),
                new(2.00, 49, 1.5f),
                new(2.25, 50, 1.5f),
                new(2.50, 51, 1.5f),
                new(2.75, 52, 1.5f),
                new(3.00, 53, 1.5f),
                new(3.25, 54, 1.5f),
                new(3.50, 55, 1.5f),
                new(3.75, 56, 1.5f),
            },

            new DrumLoop[] { //practice drums
                //kick
                new(4.00, 12, 2.5f),
                new(1.75, 12, 2.5f),
                new(2.50, 12, 2.5f),
                //snare
                new(1.00, 13, 2.5f),
                new(3.00, 13, 2.5f),
                //hihat
                new(0.50, 14, 2.5f),
                new(1.50, 14, 2.5f),
                new(2.50, 14, 2.5f),
                new(3.50, 14, 2.5f),
            },
        };

        #endregion

        #endregion

        //global methods
        #region Global Methods

        public void Update()
        {
            //update background color
            AllColorsUpdate(Conductor.instance);

            //update counting bubble
            bubbleText.text = ($"{Math.Clamp(Math.Ceiling(bubbleEndCount - Conductor.instance.songPositionInBeatsAsDouble - 1), 0, bubbleEndCount)}");

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && canPressWhiff)
            //player whiffs (press)
            {
                isInputting = false; //stops the drums (just in case)
                currentIsland.ChargerAnim.DoScaledAnimationAsync("Bounce", 0.5f);
                SoundByte.PlayOneShotGame("chargingChicken/somen_catch", volume: 0.5f);
                SoundByte.PlayOneShotGame("chargingChicken/somen_catch_old", volume: 0.3f);
                SoundByte.PlayOneShot("miss");
                ScoreMiss(0.5f);
            }

            if (PlayerInput.GetIsAction(InputAction_BasicRelease) && !IsExpectingInputNow(InputAction_BasicRelease))
            //player whiffs (press)
            {
                if (isInputting)
                //if the player was doing well
                {
                    if (canBlastOff)
                    {
                        BlastOff();
                    }
                    else
                    {
                        Uncharge();
                    }
                }

                isInputting = false; //stops the drums
            }

            //chicken/water movement speed
            if (nextIsland.isMoving) ChickenAnim.SetScaledAnimationSpeed((nextIsland.speed1 / 60) + 0.2f);
            float waterFlowSpeed = (nextIsland.speed1 / 5.83f) + ((1f / Conductor.instance.pitchedSecPerBeat) * 0.1f);
            if ((-waterFlowSpeed) - ((1f / Conductor.instance.pitchedSecPerBeat) * 0.2f) < 0) 
            {
                if (waterFlowSpeed > 0) WaterAnim.speed = waterFlowSpeed;
                if (!flowForward)
                {
                    WaterAnim.DoScaledAnimationAsync("Scroll", waterFlowSpeed);
                    flowForward = true; 
                }
            }
            else 
            { 
                if ((-waterFlowSpeed) - ((1f / Conductor.instance.pitchedSecPerBeat) * 0.2f) > 0) WaterAnim.speed = (-waterFlowSpeed) - ((1f / Conductor.instance.pitchedSecPerBeat) * 0.2f);
                if (flowForward)
                {
                    WaterAnim.DoScaledAnimationAsync("AntiScroll", (-waterFlowSpeed) - ((1f / Conductor.instance.pitchedSecPerBeat) * 0.2f));
                    flowForward = false; 
                }
            }

            //bubble shrinkage
            if (bubbleSizeChangeStart < Conductor.instance.songPositionInBeatsAsDouble && Conductor.instance.songPositionInBeatsAsDouble <= bubbleSizeChangeEnd)
            {
                float value = (Conductor.instance.GetPositionFromBeat(bubbleSizeChangeStart, bubbleSizeChangeEnd - bubbleSizeChangeStart));
                float newScale = Util.EasingFunction.Linear(1.038702f, 0, value);
                countBubble.transform.localScale = bubbleSizeChangeGrows ? new Vector3(1.038702f - newScale, 1.038702f - newScale, 1) : new Vector3(newScale, newScale, 1);
                if (bubbleSizeChangeGrows) //refresh the text to remove mipmapping
                {
                    bubbleText.text = "";
                    bubbleText.text = ($"{Math.Clamp(Math.Ceiling(bubbleEndCount - Conductor.instance.songPositionInBeatsAsDouble - 1), 0, bubbleEndCount)}");
                }
            }

            //drum volume
            if (Conductor.instance.songPositionInBeatsAsDouble <= drumFadeStart + drumFadeLength + 0.5)
            {
                double valueFade = Conductor.instance.GetPositionFromBeat(drumFadeStart, drumFadeLength);
                drumVolume = Mathf.Lerp(drumFadeIn ? 0 : 1, drumFadeIn ? 1 : 0, (float)valueFade);
            }

            //various sound loops and shizz
            if (isInputting)
            {
                chickenColorsCar.SetFloat("_Progress", Conductor.instance.GetPositionFromBeat(carColorChangeReady - (carColorChangeLength * 2), carColorChangeLength));
                drumTempVolume = 0;

                if (!isWhirringPlaying) { whirring = SoundByte.PlayOneShotGame("chargingChicken/chargeLoop", volume: 0.5f, looping: true); isWhirringPlaying = true; }
            }
            if (!isInputting)
            {
                chickenColorsCar.SetFloat("_Progress", 0);

                Conductor.instance.FadeMinigameVolume(0, 0, 1);
                drumTempVolume = 1;

                if (isWhirringPlaying) { whirring.Stop(); isWhirringPlaying = false; }
            }

            //make sure music volume resetting can be remembered between blastoffs
            float drumActualVolume = (drumVolume > drumTempVolume) ? drumVolume : drumTempVolume;
            Conductor.instance.SetMinigameVolume(drumActualVolume);

            //chicken fall off the right of the platform
            if (checkFallingDistance && nextIsland.transform.localPosition.x < -2f)
            {  
                fellTooFar = true;

                ChickenAnim.DoScaledAnimationAsync("TooFar", 0.3f);
                BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 0.60, delegate { 
                        SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_CAR_FALL", volume: 0.5f);
                        SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_CAR_FALL_WATER", volume: 0.5f); 
                        nextIsland.ChickenFall();
                    }),
                });
                checkFallingDistance = false;
            }
        }

        public void LateUpdate()
        {
            //parallax movement
            float parallaxSpeed = nextIsland.speed1 / 20000;
            Stars.localPosition -= new Vector3((parallaxSpeed * 0.3f), 0, 0);
            if (Stars.localPosition.x < -48) Stars.localPosition += new Vector3(32, 0, 0);

            Clouds.localPosition -= new Vector3((parallaxSpeed * 0.6f), 0, 0);
            if (Clouds.localPosition.x < -24) Clouds.localPosition += new Vector3(24, 0, 0);
            Planets.localPosition -= new Vector3((parallaxSpeed * 0.6f), 0, 0);
            if (Planets.localPosition.x < -30) Planets.localPosition += new Vector3(30, 0, 0);
            Doodles.localPosition -= new Vector3((parallaxSpeed * 0.6f), 0, 0);
            if (Doodles.localPosition.x < -31.5f) Doodles.localPosition += new Vector3(31.5f, 0, 0);

            Birds.localPosition -= new Vector3((parallaxSpeed * 0.67f), 0, 0);
            if (Birds.localPosition.x < -15) Birds.localPosition += new Vector3(25, 0, 0);
            Birds.localPosition = new Vector3(Birds.localPosition.x, (Birds.localPosition.x / (1.65f * -3)), 0);
            Birds.localScale = new Vector3(1 + (Birds.localPosition.x / 16.5f), 1 + (Birds.localPosition.x / 16.5f), 1);
        }

        public override void OnGameSwitch(double beat)
        {
            drumSwitch = beat;

            foreach(var entity in GameManager.instance.Beatmap.Entities)
            {
                if(entity.beat > beat + 4)
                {
                    break;
                }
                if((entity.datamodel != "chargingChicken/input") || entity.beat + entity.length < beat) //check for charges that happen right before the switch
                {
                    continue;
                }

                if(entity.datamodel == "chargingChicken/input")
                {
                    var e = entity;
                    double lateness = entity.beat - beat;
                    ChargeUp(e.beat, e.length, lateness, e["drumbeat"], e["bubble"], e["endText"], e["textLength"], e["success"], e["fail"], e["destination"], e["customDestination"], e["spaceHelmet"]);
                }
            }
        }

        private void Awake()
        {
            PersistThings(Conductor.instance.songPositionInBeatsAsDouble);

            nextIsland = Instantiate(IslandBase, transform).GetComponent<Island>();
            nextIsland.SmallLandmass.SetActive(true);
            WaterAnim.DoScaledAnimationAsync("Scroll", 0.2f);
        }

        #endregion

        //chicken methods
        #region Chicken Methods

        public static void CountIn(double beat, bool enabled = true)
        {
            if (!enabled) return; //i trust that you can figure out what this does lol

            //cowbell count in
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("chargingChicken/cowbell", beat - 4),
                new MultiSound.Sound("chargingChicken/cowbell", beat - 3),
                new MultiSound.Sound("chargingChicken/cowbell", beat - 2),
                new MultiSound.Sound("chargingChicken/cowbell", beat - 1)
            }, forcePlay: true);
        }

        public void ChargeUp(double beat, double actualLength, double lateness, int whichDrum, bool bubble = false, int endText = 0, int textLength = 4, string successText = "", string failText = "", int destination = 1, string customDestination = "You arrived in The Backrooms!", bool helmet = false)
        {
            //convert length to an integer, which is at least 4
            double length = Math.Ceiling(actualLength);
            if (length < 4) length = 4;

            //don't queue more than one input at a time
            if (beat < nextInputReady) return;
            nextInputReady = beat + (length * 2);

            //set up some variables
            yardsTextLength = length;
            double journeyBeat = beat + yardsTextLength;

            //hose count animation
            nextIsland.ChargerArmCountIn(beat, lateness);

            //cancel previous success animation if needed
            successAnimationKillOnBeat = beat - 1;

            //emergency spawnjourney so game switch inputs don't break
            if (lateness < 1) SpawnJourney(journeyBeat, yardsTextLength - 1);

            //input
            if (lateness > 0)
            {
                switch(whichDrum)
                {
                    case 0: ScheduleInput(beat - 1, 1, InputAction_BasicPress, StartChargingJust, StartChargingMiss, Nothing); break;
                    case 5: ScheduleInput(beat - 1, 1, InputAction_BasicPress, StartChargingJustFever, StartChargingMiss, Nothing); break;
                    case 6: ScheduleInput(beat - 1, 1, InputAction_BasicPress, StartChargingJustDS, StartChargingMiss, Nothing); break;
                    case 7: ScheduleInput(beat - 1, 1, InputAction_BasicPress, StartChargingJustGBA, StartChargingMiss, Nothing); break;
                    case 8: ScheduleInput(beat - 1, 1, InputAction_BasicPress, StartChargingJustBreak, StartChargingMiss, Nothing); break;
                    case 9: ScheduleInput(beat - 1, 1, InputAction_BasicPress, StartChargingJustRemix, StartChargingMiss, Nothing); break;
                    case 10: ScheduleInput(beat - 1, 1, InputAction_BasicPress, StartChargingJustPractice, StartChargingMiss, Nothing); break;
                    default: ScheduleInput(beat - 1, 1, InputAction_BasicPress, StartChargingJustMusic, StartChargingMiss, Nothing); break;
                }
            }
            else
            {
                if (PlayerInput.GetIsAction(InputAction_BasicPressing) || GameManager.instance.autoplay)
                {
                    //sound
                    if (lateness == 0)
                    {
                        switch(whichDrum)
                        {
                            case 8: 
                            {
                                SoundByte.PlayOneShotGame("chargingChicken/MISC1");
                                break;
                            }
                            default: 
                            {
                                SoundByte.PlayOneShotGame("chargingChicken/kick");
                                SoundByte.PlayOneShotGame("chargingChicken/hihat");
                                break;
                            }
                        }
                    }
                    isInputting = true; //starts the drums

                    //chicken animation
                    ChickenAnim.DoScaledAnimationAsync("Charge", 0.5f);

                    //hose animation
                    currentIsland.ChargingAnimation();
                    if (lateness > -1) canBlastOff = false;
                    else canBlastOff = true;
                }
                else
                {
                    //if the player didn't hold, just dump 'em in the ocean
                    currentIsland.ChargerAnim.DoScaledAnimationAsync("Idle", 0.5f);
                }
            }

            var releaseInput = ScheduleInput(beat, length, InputAction_BasicRelease, EndChargingJust, EndChargingMiss, Nothing);

            releaseInput.IsHittable = () => {
                return isInputting;
            };

            //set up the big beataction
            var actions = new List<BeatAction.Action>();

            //"X yards to goal" text, spawn the journey
            actions.Add(new(beat - 2, delegate {
                string yardsTextStringTemp = yardsTextString.Replace("%", $"{yardsTextLength}");
                yardsText.text = yardsTextStringTemp;
                yardsTextIsEditable = true;
                if (lateness >= 1)
                {
                    nextIsland.SpawnStones(journeyBeat, yardsTextLength - 1, lateness < 2);
                }
                else
                {
                    currentIsland.SpawnStones(journeyBeat, yardsTextLength - 1, lateness < 2);
                }
            }));

            //chicken ducks into the car window, and the bubble text is set up, and the platform noise plays, music volume is reset if needed, next island spawns, car color is set up
            actions.Add(new(beat - 1, delegate {
                if (lateness >= 1) ChickenAnim.DoScaledAnimationAsync("Prepare", 0.5f);
                if (lateness > 0 && lateness < 1) ChickenAnim.DoScaledAnimationAsync("Idle", 0.5f);
                bubbleEndCount = beat + length;
                if (lateness >= 2) SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_BLOCK_SET");
                if (lateness >= 1) SpawnJourney(journeyBeat, yardsTextLength - 1);
                canPressWhiff = true;
                if (drumReset) drumVolume = 1;
                carColorChangeReady = nextInputReady;
                carColorChangeLength = yardsTextLength;
            }));

            //spawns the countdown bubble, resets the success anim killer
            actions.Add(new(beat, delegate {
                countBubble.SetActive(bubble);
                successAnimationKillOnBeat = double.MaxValue;
                Helmet.SetActive(helmet);
                FallingHelmet.SetActive(helmet);
                currentIsland.Helmet.SetActive(helmet);
                if (lateness >= 1) nextIsland.Helmet.SetActive(helmet);
            }));

            length += 1;

            //hose beat animations
            var hoseActions = new List<BeatAction.Action>();
            for (int i = 1; i < length; i++ )
            hoseActions.Add(new(beat + i, delegate {
                PumpBeat();
            }));
            BeatAction.New(GameManager.instance, hoseActions);

            //drum loop
            double loopLength;
            if (drumLoops[whichDrum][0] != null) { loopLength = drumLoops[whichDrum][0].timing; }
            else { loopLength = 4; }

            while ( length >= 0 )
		    {
                //add drums to the beataction
                var drumActions = PlayDrumLoop(beat, whichDrum, length);
                actions.AddRange(drumActions);

                //start the next drum loop
                beat += loopLength;
                length -= loopLength;
            }

            //set ending text
            actions.Add(new(journeyBeat + yardsTextLength - 1, delegate {
                SetEndText(endText, successText, failText, textLength, destination, customDestination);
            }));

            //activate the big beat action
            BeatAction.New(GameManager.instance, actions);
        }

        public void StartChargingJust(PlayerActionEvent caller, float state)
        {
            //sound
            isInputting = true; //starts the drums
            PumpSound(state);

            //chicken animation
            ChickenAnim.DoScaledAnimationAsync("Charge", 0.5f);

            //hose animation
            currentIsland.ChargingAnimation();
            canBlastOff = false;
        }

        public void StartChargingJustMusic(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("chargingChicken/kick");
            SoundByte.PlayOneShotGame("chargingChicken/hihat");
            StartChargingJust(caller, state);
        }

        public void StartChargingJustFever(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("chargingChicken/feverkick", volume: 0.8f);
            StartChargingJust(caller, state);
        }

        public void StartChargingJustDS(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("chargingChicken/dskick");
            StartChargingJust(caller, state);
        }

        public void StartChargingJustGBA(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("chargingChicken/gbakick");
            StartChargingJust(caller, state);
        }

        public void StartChargingJustBreak(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("chargingChicken/MISC1");
            StartChargingJust(caller, state);
        }

        public void StartChargingJustRemix(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("chargingChicken/MISC21", volume: 1.5f);
            StartChargingJust(caller, state);
        }

        public void StartChargingJustPractice(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("chargingChicken/practicekick", volume: 2.5f);
            StartChargingJust(caller, state);
        }

        public void StartChargingMiss(PlayerActionEvent caller)
        {
            //sound
            isInputting = false; //ends the drums (just in case)

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);
        }

        public void PumpSound(float state)
        {
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
            }
            else
            {
                SoundByte.PlayOneShotGame("chargingChicken/PumpStart");
            }
        }

        public void EndChargingJust(PlayerActionEvent caller, float state)
        {
            BlastOff(state, false);

            nextIsland.grassState = state;

            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("miss");
            }
        }

        public void EndChargingMiss(PlayerActionEvent caller)
        {
            if (isInputting) ChickenAnim.DoScaledAnimationAsync("Bomb", 0.5f);
        }

        public void Nothing(PlayerActionEvent caller) { }

        public List<BeatAction.Action> PlayDrumLoop(double beat, int whichDrum, double length)
        {
            //create the beat action
            var actions = new List<BeatAction.Action>();

            //sort drums by timing
            DrumLoop[] drumLoopsCopy = new DrumLoop[drumLoops[whichDrum].Length];
            drumLoops[whichDrum].CopyTo(drumLoopsCopy, 0);
            Array.Sort(drumLoopsCopy);

            //fill the beat action
            foreach (var drumLoop in drumLoopsCopy) {
                string drumTypeInterpreted = drumLoop.drumType switch {
                    0 => "chargingChicken/kick",
                    1 => "chargingChicken/snare",
                    2 => "chargingChicken/hihat",
                    3 => "chargingChicken/feverkick",
                    4 => "chargingChicken/feversnare",
                    5 => "chargingChicken/feverhat",
                    6 => "chargingChicken/dskick",
                    7 => "chargingChicken/dssnare",
                    8 => "chargingChicken/dshat",
                    9 => "chargingChicken/gbakick", //NYI
                    10 => "chargingChicken/gbasnare", //NYI
                    11 => "chargingChicken/gbahat", //NYI
                    12 => "chargingChicken/practicekick",
                    13 => "chargingChicken/practicesnare",
                    14 => "chargingChicken/practicehat",
                    _ => $"chargingChicken/MISC{drumLoop.drumType - 20}" //1 - 12 = AMEN, 21 - 36 = r2gba
                };
                if (length > drumLoop.timing)
                {
                    actions.Add(new(beat + drumLoop.timing, delegate {
                        PlayDrum(drumTypeInterpreted, drumLoop.volume, beat + drumLoop.timing);
                    }));
                }
            }

            //return the list of actions
            return actions;
        }

        public void PlayDrum(string whichDrum, float drumVolumeThis, double lateness)
        {
            float drumActualVolume = (drumVolume > drumTempVolume) ? drumVolumeThis * drumVolume : drumVolumeThis * drumTempVolume;
            if (isInputting && lateness >= drumSwitch) SoundByte.PlayOneShotGame(whichDrum, volume: drumLoud ? drumVolumeThis : drumActualVolume);
        }

        public void PumpBeat()
        {
            if (isInputting) currentIsland.ChargingAnimation();
        }

        public void SpawnJourney(double beat, double length)
        {
            //pass along the next island data
            if (staleIsland != null) Destroy(staleIsland.gameObject);
            staleIsland = currentIsland;
            currentIsland = nextIsland;
            nextIsland = Instantiate(IslandBase, transform).GetComponent<Island>();

            nextIsland.SetUpCollapse(beat + length);

            nextIsland.transform.localPosition = new Vector3((float)(length * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant * 1.5)), 0, 0);
            nextIsland.BigLandmass.SetActive(true);

            nextIsland.journeySave = length * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant * 1.5);
            nextIsland.journeyStart = length * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant * 1.5);
            nextIsland.journeyEnd = 0;
            nextIsland.journeyLength = length;

            currentIsland.journeySave = length * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant * 1.5);
            currentIsland.journeyStart = 0;
            currentIsland.journeyEnd = -length * platformDistanceConstant * platformsPerBeat - (platformDistanceConstant * 1.5);
            currentIsland.journeyLength = length;

            journeyIntendedLength = beat - length - 1;

            currentIsland.respawnEnd = beat + length;
            nextIsland.respawnEnd = beat + length;

            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat - length, delegate { 
                        canBlastOff = true;
                        CollapseUnderPlayer();
                    }),
                    new BeatAction.Action(beat + 1, delegate { 
                        Explode(length);
                    }),
                });
        }

        public void BlastOff(float state = 0, bool missed = true)
        {
            canPressWhiff = false;

            //sound
            isInputting = false; //ends the drums
            SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_CAR_START", volume: 0.7f);

            //make him go :)
            ChickenAnim.DoScaledAnimationAsync("Ride", 0.5f);

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);

            //hose animation
            currentIsland.BlastoffAnimation();

            //buncha math here
            currentIsland.PositionIsland(0);
            currentIsland.transform.localPosition = new Vector3(0, 0, 0);

            nextIsland.isMoving = true;
            currentIsland.isMoving = true;

            nextIsland.journeyBlastOffTime = Conductor.instance.songPositionInBeatsAsDouble;
            currentIsland.journeyBlastOffTime = Conductor.instance.songPositionInBeatsAsDouble;

            nextIsland.PositionIsland(state * forgivenessConstant);

            if(missed)
            {
                playerSucceeded = false;
                ScoreMiss();

                fellTooFar = false;
                checkFallingDistance = true;

                nextIsland.journeyEnd += (nextIsland.journeyLength - ((nextIsland.journeyBlastOffTime - journeyIntendedLength)) * (nextIsland.journeyLength / (nextIsland.journeyLength + 1))) * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
                nextIsland.journeyLength = Math.Clamp(((nextIsland.journeyBlastOffTime - journeyIntendedLength) / 1.5) + (nextIsland.journeyLength / 3) - 1, 0, nextIsland.journeyLength - 2);

                currentIsland.journeyEnd += ((currentIsland.journeyLength - (currentIsland.journeyBlastOffTime - journeyIntendedLength) + 1) * (currentIsland.journeyLength / (currentIsland.journeyLength + 1))) * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
                currentIsland.journeyLength = Math.Clamp(((currentIsland.journeyBlastOffTime - journeyIntendedLength) / 1.5) + (currentIsland.journeyLength / 3) - 1, 0, currentIsland.journeyLength - 2);

                //make sure the chicken can't land on the island
                if (nextIsland.journeyEnd <= 4 && nextIsland.journeyEnd > 2)  { nextIsland.journeyEnd += 2; currentIsland.journeyEnd += 2; }
                if (nextIsland.journeyEnd <= 2 && nextIsland.journeyEnd > 0)  { nextIsland.journeyEnd += 4; currentIsland.journeyEnd += 4; }
                if (nextIsland.journeyEnd <= 0 && nextIsland.journeyEnd > -2) { nextIsland.journeyEnd -= 4; currentIsland.journeyEnd -= 4; }
                if (nextIsland.journeyEnd <= -2 && nextIsland.journeyEnd > 4) { nextIsland.journeyEnd -= 2; currentIsland.journeyEnd -= 2; }

                BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + currentIsland.journeyLength, delegate { 
                        ChickenFall(fellTooFar);
                        checkFallingDistance = false;
                    }),
                });
            }
            else
            {
                playerSucceeded = true;

                nextIsland.journeyEnd -= state * 1.03f * forgivenessConstant;

                currentIsland.journeyEnd -= state * 1.03f * forgivenessConstant;

                BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(journeyIntendedLength + (currentIsland.journeyLength * 2) + 1, delegate {
                        currentIsland.isMoving = false;
                        nextIsland.isMoving = false;
                        SuccessAnim();
                    }),
                });
            }
        }

        public void SuccessAnim()
        {
            if (Conductor.instance.songPositionInBeatsAsDouble < successAnimationKillOnBeat)
            {
                LookButFunee(Conductor.instance.songPositionInBeatsAsDouble, 2);
            }
        }

        public void RespawnedAnim()
        {
            if (Conductor.instance.songPositionInBeatsAsDouble < successAnimationKillOnBeat)
            {
                ChickenAnim.DoScaledAnimationAsync("Back", 0.5f);
            }
        }

        public void Uncharge()
        {
            canPressWhiff = false;

            ChickenAnim.DoScaledAnimationAsync("Idle", 0.5f);
            currentIsland.ChargerAnim.DoScaledAnimationAsync("Idle", 0.5f);

            SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_CHARGE_CANCEL");
            SoundByte.PlayOneShotGame("chargingChicken/chargeRelease", volume: 0.5f);

            isInputting = false;

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);
        }

        public void CollapseUnderPlayer()
        {
            if (isInputting) return;

            canPressWhiff = false;

            currentIsland.PositionIsland(0);
            currentIsland.transform.localPosition = new Vector3(0, 0, 0);
            
            isInputting = false;
            nextIsland.journeyEnd = nextIsland.journeyLength * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant * 1.5);
            currentIsland.journeyEnd = 0;

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);

            //collapse animation
            currentIsland.CollapseUnderPlayer();
            ChickenFall(false);
        }

        public void Explode(double length)
        {
            if (!isInputting) return;

            canPressWhiff = false;

            isInputting = false;
            nextIsland.journeyEnd = nextIsland.journeyLength * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant * 1.5);
            currentIsland.journeyEnd = 0;

            //boom
            SoundByte.PlayOneShotGame("chargingChicken/SE_NTR_ROBOT_EN_BAKUHATU_PITCH100", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-150, 151), false));

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);

            //burn animation
            ChickenAnim.DoScaledAnimationAsync("Gone", 0.5f);
            currentIsland.FakeChickenAnim.DoUnscaledAnimation("Burn");
            ChickenRespawn(Math.Min(length / 2, 3));
        }

        public void ExplodeButFunee()
        {
            successAnimationKillOnBeat = Conductor.instance.songPositionInBeatsAsDouble;

            if (currentIsland == null) currentIsland = nextIsland;

            canPressWhiff = false;

            //just in case
            isInputting = false;

            //boom
            SoundByte.PlayOneShotGame("chargingChicken/SE_NTR_ROBOT_EN_BAKUHATU_PITCH100", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-150, 151), false));

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);

            //burn animation
            ChickenAnim.DoScaledAnimationAsync("Gone", 0.5f);
            currentIsland.FakeChickenAnim.DoUnscaledAnimation("Burn");
            nextIsland.FakeChickenAnim.DoUnscaledAnimation("Burn");
        }

        public void LookButFunee(double beat, double length)
        {
            successAnimationKillOnBeat = beat;

            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { 
                    ChickenAnim.DoScaledAnimationAsync("ChickenLookTo", 0.499f);
                }),
                new BeatAction.Action(beat + length, delegate { 
                    LookBack();
                }),
            });
        }

        public void LookBack()
        {
            if (ChickenAnim.IsPlayingAnimationNames("ChickenLooking", "ChickenLookTo")) ChickenAnim.DoScaledAnimationAsync("ChickenLookFrom", 0.5f);
        }

        public void ChickenFall(bool fellTooFar)
        {
            if (!fellTooFar)
            {
                ChickenAnim.DoScaledAnimationAsync("Fall", 0.3f);
                SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_CAR_FALL", volume: 0.5f);
                currentIsland.StoneSplashCheck(4);
                BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 0.60, delegate { 
                        SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_CAR_FALL_WATER", volume: 0.5f); 
                        nextIsland.ChickenFall();
                    }),
                });
            }

            ChickenRespawn();
        }

        public void ChickenRespawn(double length = 0.6)
        {
            playerSucceeded = false;
            isInputting = false;

            currentIsland.respawnStart = Conductor.instance.songPositionInBeatsAsDouble + length;
            currentIsland.isRespawning = true;

            nextIsland.respawnStart = Conductor.instance.songPositionInBeatsAsDouble + length;
            nextIsland.isRespawning = true;
            nextIsland.FakeChickenAnim.DoUnscaledAnimation("Respawn");

            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(nextIsland.respawnEnd, delegate { 
                    currentIsland.isRespawning = false;
                    nextIsland.isRespawning = false;
                    currentIsland.FakeChickenAnim.DoScaledAnimationAsync("Idle", 0.5f);
                    nextIsland.FakeChickenAnim.DoScaledAnimationAsync("Idle", 0.5f);
                    RespawnedAnim();
                }),
            });
        }

        public void SetEndText(int endText, string successText, string failText, int stayLength, int destination, string customDestination)
        {
            //none
            if (endText == 0) return;

            //praise
            if (endText == 1)
            {
                if (playerSucceeded)
                {
                    endingText.text = successText;
                }
                else
                {
                    endingText.text = failText;
                }
            }

            //destination
            if (endText == 2)
            {
                if (destination >= 1 && destination <= 14)
                {
                    endingText.text = $"You arrived in {Enum.GetName(typeof(Destinations), destination)}!";
                }
                if (destination >= 15 && destination <= 20)
                {
                    string adjustedDestinationString;
                    switch (destination)
                    {
                        case 15: adjustedDestinationString = "The Moon"; break;
                        case 19: adjustedDestinationString = "The Edge of the Galaxy"; break;
                        case 20: adjustedDestinationString = "The Future"; break;
                        default: adjustedDestinationString = Enum.GetName(typeof(Destinations), destination); break;
                    }
                    endingText.text = $"You arrived at {adjustedDestinationString}!";
                }
                if (destination < 1 || destination > 20)
                {
                    endingText.text = customDestination;
                }
            }

            //remove text
            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + stayLength, delegate { 
                    endingText.text = "";
                }),
            });
        }

        public void BubbleShrink(double beat, double length, bool grows, bool instant)
        {
            if (nextIsland.isRespawning || !isInputting) return;

            if (instant)
            {
                countBubble.SetActive(grows);
                countBubble.transform.localScale = new Vector3(1, 1, 1);
                return;
            }

            if (grows) countBubble.SetActive(true);

            bubbleSizeChangeStart = beat;
            bubbleSizeChangeEnd = beat + length;
            bubbleSizeChangeGrows = grows;

            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate { 
                    if (!grows) { countBubble.SetActive(false); countBubble.transform.localScale = new Vector3(1, 1, 1); }
                }),
            });
        }

        public void TextEdit(double beat, string text, Color highlightColor)
        {
            yardsTextString = text;

            string textColor = ColorUtility.ToHtmlStringRGBA(highlightColor);
            yardsTextString = yardsTextString.Replace("#", $"<color=#{textColor}>%</color>");

            if(yardsTextIsEditable) 
            {
                string yardsTextStringTemp = yardsTextString.Replace("%", $"{yardsTextLength}");
                yardsText.text = yardsTextStringTemp;
            }
        }

        public void MusicFade(double beat, double length, bool fadeIn, bool instant, bool drums, bool reset)
        {
            drumFadeStart = beat;
            drumFadeLength = instant ? 0 : length;
            drumFadeIn = fadeIn;
            drumReset = reset;
            drumLoud = !drums;
        }

        public void ParallaxObjects(double beat, double length, bool instant, bool stars, bool clouds, bool earth, bool mars, bool doodles, bool birds)
        {
            float animSpeed = 0.5f / (float)length;
            //stars
            if (!starsVisible)
            {
                if (stars)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "StarsEnable" : "StarsIn", animSpeed, animLayer: 0);
                    starsVisible = true;
                }
            }
            else
            {
                if (!stars)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "StarsDisable" : "StarsOut", animSpeed, animLayer: 0);
                    starsVisible = false;
                }
            }
            //clouds
            if (!cloudsVisible)
            {
                if (clouds)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "CloudEnable" : "CloudIn", animSpeed, animLayer: 1);
                    cloudsVisible = true;
                }
            }
            else
            {
                if (!clouds)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "CloudDisable" : "CloudOut", animSpeed, animLayer: 1);
                    cloudsVisible = false;
                }
            }
            //earth
            if (!earthVisible)
            {
                if (earth)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "EarthEnable" : "EarthIn", animSpeed, animLayer: 2);
                    earthVisible = true;
                }
            }
            else
            {
                if (!earth)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "EarthDisable" : "EarthOut", animSpeed, animLayer: 2);
                    earthVisible = false;
                }
            }
            //mars
            if (!marsVisible)
            {
                if (mars)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "MarsEnable" : "MarsIn", animSpeed, animLayer: 3);
                    marsVisible = true;
                }
            }
            else
            {
                if (!mars)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "MarsDisable" : "MarsOut", animSpeed, animLayer: 3);
                    marsVisible = false;
                }
            }
            //doodles
            if (!doodlesVisible)
            {
                if (doodles)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "DoodlesEnable" : "DoodlesIn", animSpeed, animLayer: 4);
                    doodlesVisible = true;
                }
            }
            else
            {
                if (!doodles)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "DoodlesDisable" : "DoodlesOut", animSpeed, animLayer: 4);
                    doodlesVisible = false;
                }
            }
            //birds
            if (!birdsVisible)
            {
                if (birds)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "BirdsEnable" : "BirdsIn", animSpeed, animLayer: 5);
                    BirdsAnim.DoScaledAnimationAsync("BirdsFly", 0.5f, animLayer: 0);
                    birdsVisible = true;
                }
            }
            else
            {
                if (!birds)
                {
                    ParallaxFade.DoScaledAnimationAsync(instant ? "BirdsDisable" : "BirdsOut", animSpeed, animLayer: 5);
                    birdsVisible = false;
                }
            }
        }

        public void UnParallaxObjects(double beat, double length, int bg, bool instant)
        {
            if (bg == lastBg) return;

            float animSpeed = 0.5f / (float)length;

            switch (bg)
            {
                case 0: //sky
                {
                    if (lastBg == 1) UnParallaxFade.DoScaledAnimationAsync(instant ? "GalaxyDisable" : "GalaxyOut", animSpeed, animLayer: 0);
                    else UnParallaxFade.DoScaledAnimationAsync(instant ? "FutureDisable" : "FutureOut", animSpeed, animLayer: 1);
                    break;
                }
                case 1: //galaxy
                {
                    UnParallaxFade.DoScaledAnimationAsync(instant ? "GalaxyEnable" : "GalaxyIn", animSpeed, animLayer: 0);
                    if (lastBg == 2) UnParallaxFade.DoScaledAnimationAsync(instant ? "FutureDisable" : "FutureOut", animSpeed, animLayer: 1);
                    break;
                }
                case 2: //future
                {
                    UnParallaxFade.DoScaledAnimationAsync(instant ? "FutureEnable" : "FutureIn", animSpeed, animLayer: 1);
                    if (lastBg == 1) UnParallaxFade.DoScaledAnimationAsync(instant ? "GalaxyDisable" : "GalaxyOut", animSpeed, animLayer: 0);
                    break;
                }
            }

            lastBg = bg;
        }

        public void ParallaxProgress(int starProgress, int cloudProgress, int planetProgress, int doodleProgress, int birdProgress)
        {
            if (starProgress > -1) Stars.localPosition = new Vector3(((-starProgress - 50) * .32f), Stars.localPosition.y, Stars.localPosition.z);
            if (cloudProgress > -1) Clouds.localPosition = new Vector3((-cloudProgress) * .24f, Clouds.localPosition.y, Clouds.localPosition.z);
            if (planetProgress > -1) Planets.localPosition = new Vector3((-planetProgress) * .30f, Planets.localPosition.y, Planets.localPosition.z);
            if (doodleProgress > -1) Doodles.localPosition = new Vector3(((-doodleProgress) * .315f), Doodles.localPosition.y, Doodles.localPosition.z);
            if (birdProgress > -1) Birds.localPosition = new Vector3(((-birdProgress) * .25f), Birds.localPosition.y, Birds.localPosition.z);
        }

        #region ColorShit

        public void ChangeColor(double beat, float length, Color color1, Color color2, Color color3, Color color4, int ease)
        {
            bgColorStartBeat = beat;
            bgColorLength = length;
            colorFrom = color1;
            colorTo = color2;
            colorFrom2 = color3;
            colorTo2 = color4;
            lastEase1 = (Util.EasingFunction.Ease)ease;
        }

        public void ChangeLight(double beat, float length, float light1, float light2, float light3, float light4, int ease)
        {
            fgLightStartBeat = beat;
            fgLightLength = length;
            lightFrom = light1;
            lightTo = light2;
            lightFrom2 = light3;
            lightTo2 = light4;
            lastEase2 = (Util.EasingFunction.Ease)ease;
        }

        public void ChangeCarColor(double beat, float length, Color color1, Color color2, Color color3, Color color4, int ease)
        {
            carColorStartBeat = beat;
            carColorLength = length;
            carColorFrom = color1;
            carColorTo = color2;
            carColorFrom2 = color3;
            carColorTo2 = color4;
            lastEase3 = (Util.EasingFunction.Ease)ease;
        }

        public void ChangeCloudColor(double beat, float length, Color color1, Color color2, Color color3, Color color4, int ease)
        {
            cloudColorStartBeat = beat;
            cloudColorLength = length;
            cloudColorFrom = color1;
            cloudColorTo = color2;
            cloudColorFrom2 = color3;
            cloudColorTo2 = color4;
            lastEase4 = (Util.EasingFunction.Ease)ease;
        }

        private void PersistThings(double beat)
        {
            var allEvents = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] is "chargingChicken");
            var eventsBefore = allEvents.FindAll(e => e.beat < beat);

            var lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/changeBgColor");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                ChangeColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["ease"]);
            }
            else
            {
                colorFrom = defaultBGColor;
                colorTo = defaultBGColor;
                colorFrom2 = defaultBGColorBottom;
                colorTo2 = defaultBGColorBottom;
            }

            lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/changeFgLight");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                ChangeLight(e.beat, e.length, e["lightFrom"], e["lightTo"], e["headLightFrom"], e["headLightTo"], e["ease"]);
            }

            lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/changeCarColor");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                ChangeCarColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["ease"]);
            }
            else
            {
                carColorFrom = defaultCarColor;
                carColorTo = defaultCarColor;
                carColorFrom2 = defaultCarColorCharged;
                carColorTo2 = defaultCarColorCharged;
            }

            lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/changeCloudColor");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                ChangeCloudColor(e.beat, e.length, e["colorFrom"], e["colorTo"], e["colorFrom2"], e["colorTo2"], e["ease"]);
            }
            else
            {
                cloudColorFrom = defaultCloudColor;
                cloudColorTo = defaultCloudColor;
                cloudColorFrom2 = defaultCloudColorBottom;
                cloudColorTo2 = defaultCloudColorBottom;
            }

            AllColorsUpdate(Conductor.instance);

            UnParallaxFade.DoScaledAnimationAsync("GalaxyDisable", 0.5f, animLayer: 0);
            UnParallaxFade.DoScaledAnimationAsync("FutureDisable", 0.5f, animLayer: 1);

            lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/unParallaxObjects");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                UnParallaxObjects(e.beat, e.length, e["appearance"], true);
            }

            ParallaxFade.DoScaledAnimationAsync("StarsDisable", 0.5f, animLayer: 0);
            ParallaxFade.DoScaledAnimationAsync("EarthDisable", 0.5f, animLayer: 2);
            ParallaxFade.DoScaledAnimationAsync("MarsDisable", 0.5f, animLayer: 3);
            ParallaxFade.DoScaledAnimationAsync("DoodlesDisable", 0.5f, animLayer: 4);
            ParallaxFade.DoScaledAnimationAsync("BirdsDisable", 0.5f, animLayer: 5);

            lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/parallaxObjects");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                ParallaxObjects(e.beat, e.length, true, e["stars"], e["clouds"], e["earth"], e["mars"], e["doodles"], e["birds"]);
            }

            lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/parallaxProgress");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                ParallaxProgress(e["starProgress"], e["cloudProgress"], e["planetProgress"], e["doodleProgress"], e["birdProgress"]);
            }

            lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/textEdit");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                TextEdit(e.beat, e["text"], e["color"]);
            }
            else
            {
                string textColor = ColorUtility.ToHtmlStringRGBA(defaultHighlightColor);
                yardsTextString = yardsTextString.Replace("#", $"<color=#{textColor}>%</color>");
            }

            lastColorEvent = eventsBefore.FindLast(e => e.datamodel == "chargingChicken/musicFade");
            if (lastColorEvent != null)
            {
                var e = lastColorEvent;
                if(!e["reset"] && !e["fadeIn"])
                {
                    drumVolume = 0;
                    drumLoud = !e["drums"];
                    drumReset = false;
                }
            }

            colorsCanUpdate = true;
        }

        private void AllColorsUpdate(Conductor cond)
        {
            //bg color
            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase1);

            float normalizedBeatBG = Mathf.Clamp01(cond.GetPositionFromBeat(bgColorStartBeat, bgColorLength));
            float newColorR = func(colorFrom.r, colorTo.r, normalizedBeatBG);
            float newColorG = func(colorFrom.g, colorTo.g, normalizedBeatBG);
            float newColorB = func(colorFrom.b, colorTo.b, normalizedBeatBG);
            bgHigh.color = new Color(newColorR, newColorG, newColorB);
            gradient.color = new Color(newColorR, newColorG, newColorB);

            newColorR = func(colorFrom2.r, colorTo2.r, normalizedBeatBG);
            newColorG = func(colorFrom2.g, colorTo2.g, normalizedBeatBG);
            newColorB = func(colorFrom2.b, colorTo2.b, normalizedBeatBG);
            bgLow.color = new Color(newColorR, newColorG, newColorB);

            //fg light
            func = Util.EasingFunction.GetEasingFunction(lastEase2);

            normalizedBeatBG = Mathf.Clamp01(cond.GetPositionFromBeat(fgLightStartBeat, fgLightLength));
            newColorR = func(lightFrom, lightTo, normalizedBeatBG);
            chickenColors.color = new Color(newColorR, newColorR, newColorR);
            chickenColorsCar.color = new Color(newColorR, newColorR, newColorR);
            chickenColorsWater.color = new Color(newColorR, newColorR, newColorR);

            newColorR = func(lightFrom2, lightTo2, normalizedBeatBG);
            headlightColor.color = new Color(1, 1, 1, newColorR);

            //car color
            func = Util.EasingFunction.GetEasingFunction(lastEase3);
            
            normalizedBeatBG = Mathf.Clamp01(cond.GetPositionFromBeat(carColorStartBeat, carColorLength));
            newColorR = func(carColorFrom.r, carColorTo.r, normalizedBeatBG);
            newColorG = func(carColorFrom.g, carColorTo.g, normalizedBeatBG);
            newColorB = func(carColorFrom.b, carColorTo.b, normalizedBeatBG);
            chickenColorsCar.SetColor("_Color1", new Color(newColorR, newColorG, newColorB));

            newColorR = func(carColorFrom2.r, carColorTo2.r, normalizedBeatBG);
            newColorG = func(carColorFrom2.g, carColorTo2.g, normalizedBeatBG);
            newColorB = func(carColorFrom2.b, carColorTo2.b, normalizedBeatBG);
            chickenColorsCar.SetColor("_Color2", new Color(newColorR, newColorG, newColorB));

            //cloud color
            func = Util.EasingFunction.GetEasingFunction(lastEase4);
            
            normalizedBeatBG = Mathf.Clamp01(cond.GetPositionFromBeat(cloudColorStartBeat, cloudColorLength));
            newColorR = func(cloudColorFrom.r, cloudColorTo.r, normalizedBeatBG);
            newColorG = func(cloudColorFrom.g, cloudColorTo.g, normalizedBeatBG);
            newColorB = func(cloudColorFrom.b, cloudColorTo.b, normalizedBeatBG);
            chickenColorsCloud.SetColor("_Color", new Color(newColorR, newColorG, newColorB));
            chickenColorsDoodles.SetColor("_Color1", new Color(newColorR, newColorG, newColorB));

            newColorR = func(cloudColorFrom2.r, cloudColorTo2.r, normalizedBeatBG);
            newColorG = func(cloudColorFrom2.g, cloudColorTo2.g, normalizedBeatBG);
            newColorB = func(cloudColorFrom2.b, cloudColorTo2.b, normalizedBeatBG);
            chickenColorsCloud.SetColor("_OutlineColor", new Color(newColorR, newColorG, newColorB));
            chickenColorsDoodles.SetColor("_Color", new Color(newColorR, newColorG, newColorB));
        }

        #endregion

        #endregion
    }
}
