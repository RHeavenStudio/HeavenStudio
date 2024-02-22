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
    public class ChargingChicken : Minigame
    {
        //definitions
        #region Definitions

        [SerializeField] SpriteRenderer gradient;
        [SerializeField] SpriteRenderer bgLow;
        [SerializeField] SpriteRenderer bgHigh;
        [SerializeField] Animator ChickenAnim;
        [SerializeField] Transform sea;
        [SerializeField] TMP_Text yardsText;
        [SerializeField] TMP_Text bubbleText;
        [SerializeField] GameObject countBubble;
        [SerializeField] Material chickenColors;
        [SerializeField] SpriteRenderer headlightColor;

        bool isInputting = false;

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

        float seaPos = 0;

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
            bubbleText.text = ($"{Math.Ceiling(bubbleEndCount - Conductor.instance.songPositionInBeatsAsDouble)}");

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
                    SoundByte.PlayOneShotGame("chargingChicken/blastoff"); ChickenAnim.DoScaledAnimationAsync("Ride", 0.5f); //TO DO: replace with proper takeoff function

                    //erase text
                    yardsText.text = "";

                    //despawn the counting bubble
                    countBubble.SetActive(false);
                }

                isInputting = false; //stops the drums
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

        private void Awake()
        {
            colorFrom = defaultBGColor;
            colorTo = defaultBGColor;
            colorFrom2 = defaultBGColorBottom;
            colorTo2 = defaultBGColorBottom;
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
            if (length < 2) length = 2;

            //TO DO: GET RID OF THIS THIS IS JUST FOR DEMO PURPOSES, IT PLAYS THE LITTLE "COLLAPSE" NOISE BUT IT NEEDS TO BE REPLACED WITH A PROPER DISTANCE CHECK
            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + (length * 2) - 1, delegate { 
                    SoundByte.PlayOneShotGame("chargingChicken/complete");
                }),
            });

            //input
            ScheduleInput(beat - 1, 1, InputAction_BasicPress, whichDrum == 0 ? StartChargingJust : StartChargingJustMusic, StartChargingMiss, Nothing);
            ScheduleInput(beat, length, InputAction_BasicRelease, EndChargingJust, EndChargingMiss, Nothing);

            //set up bubble
            bubbleEndCount = beat + length;

            //set up the big beataction
            var actions = new List<BeatAction.Action>();

            //"X yards to goal" text
            double yardsTextLength = length;
            actions.Add(new(beat - 2, delegate {
                yardsText.text = $"<color=yellow>{yardsTextLength}</color> yards to the goal.";
            }));

            //chicken ducks into the car window
            actions.Add(new(beat - 1, delegate {
                ChickenAnim.DoScaledAnimationAsync("Prepare", 0.5f);
            }));

            //spawns the countdown bubble
            actions.Add(new(beat, delegate {
                    countBubble.SetActive(bubble);
            }));

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

            //animation
            ChickenAnim.DoScaledAnimationAsync("Charge", 0.5f);
        }

        public void StartChargingJustMusic(PlayerActionEvent caller, float state)
        {
            //sound
            isInputting = true; //starts the drums
            SoundByte.PlayOneShotGame("chargingChicken/kick");
            SoundByte.PlayOneShotGame("chargingChicken/hihat");

            //animation
            ChickenAnim.DoScaledAnimationAsync("Charge", 0.5f);
        }

        public void StartChargingMiss(PlayerActionEvent caller)
        {
            //sound
            isInputting = false; //ends the drums (just in case)

            //erase text TO DO: make this happen later (but for now it's fine here)
            yardsText.text = "";

            //despawn the counting bubble TO DO: make this happen later (but for now it's fine here)
            countBubble.SetActive(false);
        }

        public void EndChargingJust(PlayerActionEvent caller, float state)
        {
            //sound
            isInputting = false; //ends the drums
            SoundByte.PlayOneShotGame("chargingChicken/blastoff");

            //TO DO: remove this and make it better
            ChickenAnim.DoScaledAnimationAsync("Ride", 0.5f);

            //erase text
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);
        }

        public void EndChargingMiss(PlayerActionEvent caller)
        {
            //sound
            isInputting = false; //ends the drums

            //erase text
            yardsText.text = "";

            //despawn the counting bubble
            countBubble.SetActive(false);
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

        private void PersistColor(double beat)
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
