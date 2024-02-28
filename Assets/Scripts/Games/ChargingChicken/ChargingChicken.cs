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
            return new Minigame("chargingChicken", "Charging Chicken", "FFFFFF", false, false, new List<GameAction>()
            {
                new GameAction("input", "Charge")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out ChargingChicken instance)) {
                            instance.ChargeUp(e.beat, e.length, e["drumbeat"], e["bubble"]);
                        }
                        ChargingChicken.CountIn(e.beat);
                    },
                    parameters = new List<Param>()
                    {
                        new Param("drumbeat", ChargingChicken.DrumLoopList.Straight, "Drum Beat", "Choose which drum beat to play while filling."),
                        new Param("bubble", false, "Countdown Bubble", "Choose whether the counting bubble will spawn for this input."),
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
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new[] { "colorFrom", "colorFrom2"  }),
                        }),
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
                            new Param.CollapseParam((x, _) => (int)x != (int)Util.EasingFunction.Ease.Instant, new[] { "lightFrom", "headLightFrom"  }),
                        }),
                    }
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
        [SerializeField] Transform sea;
        [SerializeField] TMP_Text yardsText;
        [SerializeField] TMP_Text bubbleText;
        [SerializeField] GameObject countBubble;
        [SerializeField] Island IslandBase;
        [SerializeField] Material chickenColors;
        [SerializeField] SpriteRenderer headlightColor;

        bool isInputting = false;
        bool canBlastOff = false;

        bool flowForward = true;

        double bgColorStartBeat = -1;
        float bgColorLength = 0;
        double fgLightStartBeat = -1;
        float fgLightLength = 0;
        Util.EasingFunction.Ease lastEase;
        Color colorFrom;
        Color colorTo;
        Color colorFrom2;
        Color colorTo2;
        float lightFrom = 1;
        float lightTo = 1;
        float lightFrom2 = 0;
        float lightTo2 = 0;

        double bubbleEndCount = 0;
        double bubbleSizeChangeStart = 0;
        double bubbleSizeChangeEnd = 0;
        bool bubbleSizeChangeGrows = false;

        string yardsTextString = "# yards to the goal.";
        bool yardsTextIsEditable = false;
        double yardsTextLength = 0;
        private static Color _defaultHighlightColor;
        public static Color defaultHighlightColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFFF00", out _defaultHighlightColor);
                return _defaultHighlightColor;
            }
        }


        Island nextIsland;
        Island currentIsland;
        Island staleIsland;
        Island stone;
        double journeyIntendedLength;
        StonePlatform[] stonePlatformJourney;

        private struct StonePlatform
        {
            public int stoneNumber;
            public Island thisPlatform;
        }

        double platformDistanceConstant = 5.35 / 2;
        int platformsPerBeat = 4;

        float forgivenessConstant = 1.3f;

        public enum DrumLoopList
        {
            None,
            Straight,
            SwungSixteenth,
            SwungEighth,
            Triplet,
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

        private DrumLoop[][] drumLoops = new DrumLoop[][] { 

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

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            //player whiffs (press)
            {
                isInputting = false; //stops the drums (just in case)
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
            float newAnimScale = Util.EasingFunction.EaseOutQuad(1, 0, nextIsland.value1);
            if (nextIsland.isMoving) ChickenAnim.SetScaledAnimationSpeed((newAnimScale * 0.8f) + 0.4f);
            float waterFlowSpeed = (nextIsland.speed1 / 5.83f) + ((1f / Conductor.instance.pitchedSecPerBeat) * 0.2f);
            if ((-waterFlowSpeed) - ((1f / Conductor.instance.pitchedSecPerBeat) * 0.4f) < 0) 
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
                if ((-waterFlowSpeed) - ((1f / Conductor.instance.pitchedSecPerBeat) * 0.4f) > 0) WaterAnim.speed = (-waterFlowSpeed) - ((1f / Conductor.instance.pitchedSecPerBeat) * 0.4f);
                if (flowForward)
                {
                    WaterAnim.DoScaledAnimationAsync("AntiScroll", (-waterFlowSpeed) - ((1f / Conductor.instance.pitchedSecPerBeat) * 0.4f));
                    flowForward = false; 
                }
            }

            //bubble shrinkage
            if (bubbleSizeChangeStart < Conductor.instance.songPositionInBeatsAsDouble && Conductor.instance.songPositionInBeatsAsDouble <= bubbleSizeChangeEnd)
            {
                float value = (Conductor.instance.GetPositionFromBeat(bubbleSizeChangeStart, bubbleSizeChangeEnd - bubbleSizeChangeStart));
                float newScale = Util.EasingFunction.Linear(1, 0, value);
                countBubble.transform.localScale = bubbleSizeChangeGrows ? new Vector3(1 - newScale, 1 - newScale, 1) : new Vector3(newScale, newScale, 1);
                if (bubbleSizeChangeGrows) //refresh the text to remove mipmapping
                {
                    bubbleText.text = "";
                    bubbleText.text = ($"{Math.Clamp(Math.Ceiling(bubbleEndCount - Conductor.instance.songPositionInBeatsAsDouble - 1), 0, bubbleEndCount)}");
                }
            }
        }

        public override void OnPlay(double beat)
        {
            PersistThings(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            PersistThings(beat);
        }

        private void Awake()
        {
            colorFrom = defaultBGColor;
            colorTo = defaultBGColor;
            colorFrom2 = defaultBGColorBottom;
            colorTo2 = defaultBGColorBottom;

            nextIsland = Instantiate(IslandBase, transform).GetComponent<Island>();
            nextIsland.SmallLandmass.SetActive(true);
            WaterAnim.DoScaledAnimationAsync("Scroll", 0.2f);

            string textColor = ColorUtility.ToHtmlStringRGBA(defaultHighlightColor);
            yardsTextString = yardsTextString.Replace("#", $"<color=#{textColor}>%</color>");

            PersistThings(Conductor.instance.songPositionInBeatsAsDouble);
        }

        #endregion

        //chicken methods
        #region Chicken Methods

        public static void CountIn(double beat)
        {
            //cowbell count in
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("chargingChicken/cowbell", beat - 4),
                new MultiSound.Sound("chargingChicken/cowbell", beat - 3),
                new MultiSound.Sound("chargingChicken/cowbell", beat - 2),
                new MultiSound.Sound("chargingChicken/cowbell", beat - 1)
            }, forcePlay: true);
        }

        public void ChargeUp(double beat, double actualLength, int whichDrum, bool bubble = false) //TO DO: make this inactive
        {
            //convert length to an integer, which is at least 2
            double length = Math.Ceiling(actualLength);
            if (length < 4) length = 4;

            //hose count animation
            nextIsland.ChargerArmCountIn(beat);

            //collapse island (successful) TO DO: MAKE THIS PER-ISLAND SO IT'S NOT A FUCKING RACE CONDITION
            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + (length * 2) - 1, delegate { 
                    SoundByte.PlayOneShotGame("chargingChicken/complete");
                    nextIsland.BigLandmass.SetActive(false);
                    nextIsland.SmallLandmass.SetActive(true);
                }),
            });

            //input
            ScheduleInput(beat - 1, 1, InputAction_BasicPress, whichDrum == 0 ? StartChargingJust : StartChargingJustMusic, StartChargingMiss, Nothing);
            ScheduleInput(beat, length, InputAction_BasicRelease, EndChargingJust, EndChargingMiss, Nothing);

            //set up the big beataction
            var actions = new List<BeatAction.Action>();

            //"X yards to goal" text, spawn the journey
            yardsTextLength = length;
            double journeyBeat = beat + yardsTextLength;
            actions.Add(new(beat - 2, delegate {
                string yardsTextStringTemp = yardsTextString.Replace("%", $"{yardsTextLength}");
                yardsText.text = yardsTextStringTemp;
                yardsTextIsEditable = true;
                SpawnStones(journeyBeat, yardsTextLength - 1);
            }));

            //chicken ducks into the car window, and the bubble text is set up, and the platform noise plays, and next island spawns
            actions.Add(new(beat - 1, delegate {
                ChickenAnim.DoScaledAnimationAsync("Prepare", 0.5f);
                bubbleEndCount = beat + length;
                SoundByte.PlayOneShotGame("chargingChicken/platformSet", volume: 0.5f);
                SpawnJourney(journeyBeat, yardsTextLength - 1);
            }));

            //spawns the countdown bubble, allows stones to fall
            actions.Add(new(beat, delegate {
                countBubble.SetActive(bubble);
                foreach (var a in stonePlatformJourney)
                {
                    stone = a.thisPlatform;
                    stone.isBeingSet = false;
                }
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
            while ( length >= 0 )
		    {
                //add drums to the beataction
                var drumActions = PlayDrumLoop(beat, whichDrum, length);
                actions.AddRange(drumActions);

                //start the next drum loop
                beat += 4;
                length -= 4;
            }

            //activate the big beat action
            BeatAction.New(GameManager.instance, actions);
        }

        public void StartChargingJust(PlayerActionEvent caller, float state)
        {
            //sound
            isInputting = true; //starts the drums
            //SoundByte.PlayOneShotGame("chargingChicken/inputJust", pitch: 0.9f, volume: 0.8f); //TO DO: maybe change this

            //chicken animation
            ChickenAnim.DoScaledAnimationAsync("Charge", 0.5f);

            //hose animation
            currentIsland.ChargingAnimation();
            canBlastOff = false;
        }

        public void StartChargingJustMusic(PlayerActionEvent caller, float state)
        {
            //sound
            isInputting = true; //starts the drums
            SoundByte.PlayOneShotGame("chargingChicken/kick");
            SoundByte.PlayOneShotGame("chargingChicken/hihat");
            //SoundByte.PlayOneShotGame("chargingChicken/inputJust", pitch: 0.9f, volume: 0.8f); //TO DO: maybe change this

            //chicken animation
            ChickenAnim.DoScaledAnimationAsync("Charge", 0.5f);

            //hose animation
            currentIsland.ChargingAnimation();
            canBlastOff = false;
        }

        public void StartChargingMiss(PlayerActionEvent caller)
        {
            //sound
            isInputting = false; //ends the drums (just in case)

            //erase text TO DO: make this happen later (but for now it's fine here)
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble TO DO: make this happen later (but for now it's fine here)
            countBubble.SetActive(false);
        }

        public void EndChargingJust(PlayerActionEvent caller, float state)
        {
            BlastOff(state, false);
        }

        public void EndChargingMiss(PlayerActionEvent caller)
        {

        }

        public void Nothing(PlayerActionEvent caller) { }

        public List<BeatAction.Action> PlayDrumLoop(double beat, int whichDrum, double length)
        {
            //create the beat action
            var actions = new List<BeatAction.Action>();

            //sort drums by timing
            Array.Sort(drumLoops[whichDrum]);

            //fill the beat action
            foreach (var drumLoop in drumLoops[whichDrum]) {
                string drumTypeInterpreted = drumLoop.drumType switch {
                    0 => "chargingChicken/kick",
                    1 => "chargingChicken/snare",
                    2 => "chargingChicken/hihat"
                };
                if (length > drumLoop.timing)
                {
                    actions.Add(new(beat + drumLoop.timing, delegate {
                        PlayDrum(drumTypeInterpreted, drumLoop.volume);
                    }));
                }
            }

            //return the list of actions
            return actions;
        }

        public void PlayDrum(string whichDrum, float drumVolume)
        {
            if (isInputting) SoundByte.PlayOneShotGame(whichDrum, volume: drumVolume);
        }

        public void PumpBeat()
        {
            if (isInputting) currentIsland.ChargingAnimation();
        }

        public void SpawnJourney(double beat, double length)
        {
            staleIsland = currentIsland;
            currentIsland = nextIsland;
            nextIsland = Instantiate(IslandBase, transform).GetComponent<Island>();

            nextIsland.transform.localPosition = new Vector3((float)(length * platformDistanceConstant * platformsPerBeat), 0, 0);
            nextIsland.BigLandmass.SetActive(true);

            nextIsland.journeySave = length * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
            nextIsland.journeyStart = length * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
            nextIsland.journeyEnd = 0;
            nextIsland.journeyLength = length;

            currentIsland.journeySave = length * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
            currentIsland.journeyStart = 0;
            currentIsland.journeyEnd = -length * platformDistanceConstant * platformsPerBeat - (platformDistanceConstant / 2);
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
                        Explode();
                    }),
                });
        }

        public void SpawnStones(double beat, double length)
        {
            stonePlatformJourney = new StonePlatform[(int)(length * 4 - 1)];
            for ( int i = 0; i < length * 4 - 1; i++ )
            {
                stonePlatformJourney[i].thisPlatform = Instantiate(IslandBase, transform).GetComponent<Island>();
                stonePlatformJourney[i].stoneNumber = i;
            }

            foreach (var a in stonePlatformJourney)
            {
                stone = a.thisPlatform;
                stone.transform.localPosition = new Vector3((float)(((a.stoneNumber + 1) * platformDistanceConstant) + (platformDistanceConstant / 2)), 0, 0);
                stone.BecomeStonePlatform(a.stoneNumber);
                stone.StoneFall(a.stoneNumber);
                stone.isBeingSet = true;

                stone.journeySave = length * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
                stone.journeyStart = ((a.stoneNumber + 1) * platformDistanceConstant) + (platformDistanceConstant / 2);
                stone.journeyEnd = ((a.stoneNumber) * platformDistanceConstant) + (platformDistanceConstant / 2) - (length * platformDistanceConstant * platformsPerBeat - (platformDistanceConstant / 2));
                stone.journeyLength = length;

                stone.respawnEnd = beat + length;
            }
        }

        public void BlastOff(float state = 0, bool missed = true)
        {
            //sound
            isInputting = false; //ends the drums
            SoundByte.PlayOneShotGame("chargingChicken/blastoff");

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
            nextIsland.isMoving = true;
            currentIsland.isMoving = true;

            nextIsland.journeyBlastOffTime = Conductor.instance.songPositionInBeatsAsDouble;
            currentIsland.journeyBlastOffTime = Conductor.instance.songPositionInBeatsAsDouble;

            foreach (var a in stonePlatformJourney)
            {
                stone = a.thisPlatform;
                stone.isMoving = true;
                stone.journeyBlastOffTime = Conductor.instance.songPositionInBeatsAsDouble;
            }

            nextIsland.PositionIsland(state * forgivenessConstant);

            if(missed)
            {
                nextIsland.journeyEnd += (nextIsland.journeyLength - ((nextIsland.journeyBlastOffTime - journeyIntendedLength)) * (nextIsland.journeyLength / (nextIsland.journeyLength + 1))) * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
                nextIsland.journeyLength = Math.Clamp(((nextIsland.journeyBlastOffTime - journeyIntendedLength) / 1.5) + (nextIsland.journeyLength / 3) - 1, 0, nextIsland.journeyLength - 2);

                currentIsland.journeyEnd += ((currentIsland.journeyLength - (currentIsland.journeyBlastOffTime - journeyIntendedLength) + 1) * (currentIsland.journeyLength / (currentIsland.journeyLength + 1))) * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
                currentIsland.journeyLength = Math.Clamp(((currentIsland.journeyBlastOffTime - journeyIntendedLength) / 1.5) + (currentIsland.journeyLength / 3) - 1, 0, currentIsland.journeyLength - 2);

                foreach (var a in stonePlatformJourney)
                {
                    stone = a.thisPlatform;
                    stone.journeyEnd += (stone.journeyLength - ((stone.journeyBlastOffTime - journeyIntendedLength)) * (stone.journeyLength / (stone.journeyLength + 1))) * platformDistanceConstant * platformsPerBeat + (platformDistanceConstant / 2);
                    stone.journeyLength = Math.Clamp(((stone.journeyBlastOffTime - journeyIntendedLength) / 1.5) + (stone.journeyLength / 3) - 1, 0, stone.journeyLength - 2);
                }

                BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + currentIsland.journeyLength, delegate { 
                        nextIsland.isMoving = false;
                        currentIsland.isMoving = false;
                        foreach (var a in stonePlatformJourney)
                        {
                            stone = a.thisPlatform;
                            stone.isMoving = false;
                        }
                        ChickenFall();
                    }),
                });
            }
            else
            {
                nextIsland.journeyEnd -= state * 1.03f * forgivenessConstant;

                currentIsland.journeyEnd -= state * 1.03f * forgivenessConstant;

                foreach (var a in stonePlatformJourney)
                {
                    stone = a.thisPlatform;
                    stone.journeyEnd -= state * 1.03f * forgivenessConstant;
                }

                BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(journeyIntendedLength + (currentIsland.journeyLength * 2) + 1, delegate { 
                        nextIsland.isMoving = false;
                        currentIsland.isMoving = false;
                        foreach (var a in stonePlatformJourney)
                        {
                            stone = a.thisPlatform;
                            stone.isMoving = false;
                        }
                    }),
                });
            }
        }

        public void Uncharge()
        {
            ChickenAnim.DoScaledAnimationAsync("Idle", 0.5f);
            currentIsland.ChargerAnim.DoScaledAnimationAsync("Idle", 0.5f);

            isInputting = false;

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);
        }

        public void CollapseUnderPlayer() //TO DO: change this
        {
            if (isInputting) return;
            
            isInputting = false;
            nextIsland.journeyEnd = nextIsland.journeyLength * platformDistanceConstant * platformsPerBeat;
            currentIsland.journeyEnd = 0;
            foreach (var a in stonePlatformJourney)
            {
                stone = a.thisPlatform;
                stone.journeyEnd = ((a.stoneNumber + 1) * platformDistanceConstant) + (platformDistanceConstant / 2);
            }

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);

            //collapse animation
            currentIsland.CollapseUnderPlayer();
            ChickenFall();
        }

        public void Explode() //TO DO: fix the hell out of this
        {
            if (!isInputting) return;

            isInputting = false;
            nextIsland.journeyEnd = nextIsland.journeyLength * platformDistanceConstant * platformsPerBeat;
            currentIsland.journeyEnd = 0;
            foreach (var a in stonePlatformJourney)
            {
                stone = a.thisPlatform;
                stone.journeyEnd = ((a.stoneNumber + 1) * platformDistanceConstant) + (platformDistanceConstant / 2);
            }

            //erase text
            yardsTextIsEditable = false;
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);

            //burn animation
            ChickenAnim.DoScaledAnimationAsync("Gone", 0.5f);
            currentIsland.FakeChickenAnim.DoScaledAnimationAsync("Burn", 0.5f);
            ChickenRespawn();
        }

        public void ChickenFall()
        {
            ChickenAnim.DoScaledAnimationAsync("Fall", 0.5f);

            ChickenRespawn();


            //SoundByte.PlayOneShotGame("chargingChicken/blastoff");
        }

        public void ChickenRespawn()
        {
            isInputting = false;

            currentIsland.respawnStart = Conductor.instance.songPositionInBeatsAsDouble + 0.5;
            currentIsland.isRespawning = true;

            nextIsland.respawnStart = Conductor.instance.songPositionInBeatsAsDouble + 0.5;
            nextIsland.isRespawning = true;
            nextIsland.FakeChickenAnim.DoUnscaledAnimation("Respawn");

            foreach (var a in stonePlatformJourney)
            {
                stone = a.thisPlatform;
                stone.respawnStart = Conductor.instance.songPositionInBeatsAsDouble + 0.5;
                stone.isRespawning = true;
            }

            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(nextIsland.respawnEnd, delegate { 
                    if (staleIsland != null) staleIsland.isRespawning = false;
                    currentIsland.isRespawning = false;
                    nextIsland.isRespawning = false;
                    if (staleIsland != null) staleIsland.FakeChickenAnim.DoScaledAnimationAsync("Idle", 0.5f);
                    currentIsland.FakeChickenAnim.DoScaledAnimationAsync("Idle", 0.5f);
                    nextIsland.FakeChickenAnim.DoScaledAnimationAsync("Idle", 0.5f);
                    foreach (var a in stonePlatformJourney)
                    {
                        stone = a.thisPlatform;
                        stone.isRespawning = false;
                    }
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

        #region ColorShit

        public void ChangeColor(double beat, float length, Color color1, Color color2, Color color3, Color color4, int ease)
        {
            bgColorStartBeat = beat;
            bgColorLength = length;
            colorFrom = color1;
            colorTo = color2;
            colorFrom2 = color3;
            colorTo2 = color4;
            lastEase = (Util.EasingFunction.Ease)ease;
        }

        public void ChangeLight(double beat, float length, float light1, float light2, float light3, float light4, int ease)
        {
            fgLightStartBeat = beat;
            fgLightLength = length;
            lightFrom = light1;
            lightTo = light2;
            lightFrom2 = light3;
            lightTo2 = light4;
            lastEase = (Util.EasingFunction.Ease)ease;
        }

        private void PersistThings(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("chargingChicken", new string[] { "changeBgColor" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                ChangeColor(lastEvent.beat, lastEvent.length, lastEvent["colorFrom"], lastEvent["colorTo"], lastEvent["colorFrom2"], lastEvent["colorTo2"], lastEvent["ease"]);
            }

            allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("chargingChicken", new string[] { "changeFgLight" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                ChangeLight(lastEvent.beat, lastEvent.length, lastEvent["lightFrom"], lastEvent["lightTo"], lastEvent["headLightFrom"], lastEvent["headLightTo"], lastEvent["ease"]);
            }

            allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("chargingChicken", new string[] { "textEdit" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                TextEdit(lastEvent.beat, lastEvent["text"], lastEvent["color"]);
            }
        }

        private void AllColorsUpdate(Conductor cond)
        {
            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);

            //bg color
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
            float normalizedBeatFG = Mathf.Clamp01(cond.GetPositionFromBeat(fgLightStartBeat, fgLightLength));
            float newLight = func(lightFrom, lightTo, normalizedBeatFG);
            chickenColors.color = new Color(newLight, newLight, newLight);

            newLight = func(lightFrom2, lightTo2, normalizedBeatFG);
            headlightColor.color = new Color(1, 1, 1, newLight);
        }

        #endregion

        #endregion
    }
}
