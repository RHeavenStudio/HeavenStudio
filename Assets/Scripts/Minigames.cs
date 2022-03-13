using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using HeavenStudio.Util;

using HeavenStudio.Games;

namespace HeavenStudio
{
    public class Minigames
    {
        public class Minigame
        {
            public string name;
            public string displayName;
            public string color;
            public GameObject holder;
            public bool threeD;
            public bool fxOnly;
            public List<GameAction> actions = new List<GameAction>();

            public Minigame(string name, string displayName, string color, bool threeD, bool fxOnly, List<GameAction> actions)
            {
                this.name = name;
                this.displayName = displayName;
                this.color = color;
                this.actions = actions;
                this.threeD = threeD;
                this.fxOnly = fxOnly;
            }
        }

        public class GameAction
        {
            public string actionName;
            public EventCallback function;
            public float defaultLength;
            public bool resizable;
            public List<Param> parameters;
            public bool hidden;
            public EventCallback inactiveFunction;

            /// <summary>
            /// <para>Creates a block that can be used in the editor. The block's function and attributes are defined in the parentheses.</para>
            /// <para>Note: Every parameter after the second one is an optional parameter. You can change optional parameters by adding (name): (value) after the second parameter.</para>
            /// </summary>
            /// <param name="actionName">Name of the block</param>
            /// <param name="function"><para>What the block does when read during playback</para>
            /// <para>Only does this if the game that it is associated with is loaded.</para></param>
            /// <param name="defaultLength">How long the block appears in the editor</param>
            /// <param name="resizable">Allows the user to resize the block</param>
            /// <param name="parameters">Extra parameters for this block that change how it functions.</param>
            /// <param name="hidden">Prevents the block from being shown in the game list. Block will still function normally if it is in the timeline.</param>
            /// <param name="inactiveFunction">What the block does when read while the game it's associated with isn't loaded.</param>
            public GameAction(string actionName, EventCallback function, float defaultLength = 1, bool resizable = false, List<Param> parameters = null, bool hidden = false, EventCallback inactiveFunction = null)
            {
                this.actionName = actionName;
                this.function = function;
                this.defaultLength = defaultLength;
                this.resizable = resizable;
                this.parameters = parameters;
                this.hidden = hidden;
                if(inactiveFunction == null) inactiveFunction = delegate { };
                this.inactiveFunction = inactiveFunction;
            }
        }

        [System.Serializable]
        public class Param
        {
            public string propertyName;
            public object parameter;
            public string propertyCaption;
            public string tooltip;

            /// <summary>
            /// A parameter that changes the function of a GameAction.
            /// </summary>
            /// <param name="propertyName">The name of the variable that's being changed. Must be one of the variables in <see cref="Beatmap.Entity"/></param>
            /// <param name="parameter">The value of the parameter</param>
            /// <param name="propertyCaption">The name shown in the editor. Can be anything you want.</param>
            public Param(string propertyName, object parameter, string propertyCaption, string tooltip = "")
            {
                this.propertyName = propertyName;
                this.parameter = parameter;
                this.propertyCaption = propertyCaption;
                this.tooltip = tooltip;
            }
        }

        public delegate void EventCallback();

        public static void Init(EventCaller eventCaller)
        {
            eventCaller.minigames = new List<Minigame>()
            {
                new Minigame("gameManager", "Game Manager", "", false, true, new List<GameAction>()
                {
                    new GameAction("switchGame",            delegate { GameManager.instance.SwitchGame(eventCaller.currentSwitchGame, eventCaller.currentEntity.beat); }, 0.5f, inactiveFunction: delegate { GameManager.instance.SwitchGame(eventCaller.currentSwitchGame, eventCaller.currentEntity.beat); }),
                    new GameAction("end",                   delegate { Debug.Log("end"); }),
                    new GameAction("skill star",            delegate {  }, 1f, true),
                    new GameAction("flash",                 delegate 
                    {

                        /*Color colA = eventCaller.currentEntity.colorA;
                        Color colB = eventCaller.currentEntity.colorB;

                        Color startCol = new Color(colA.r, colA.g, colA.b, eventCaller.currentEntity.valA);
                        Color endCol = new Color(colB.r, colB.g, colB.b, eventCaller.currentEntity.valB);

                        GameManager.instance.fade.SetFade(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, startCol, endCol, eventCaller.currentEntity.ease);*/

                    }, 1f, true, new List<Param>() 
                    {
                        new Param("colorA", Color.white, "Start Color"),
                        new Param("colorB", Color.white, "End Color"),
                        new Param("valA", new EntityTypes.Float(0, 1, 1), "Start Opacity"),
                        new Param("valB", new EntityTypes.Float(0, 1, 0), "End Opacity"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease")
                    } ),
                    new GameAction("toggle inputs",            delegate
                    {
                        GameManager.instance.ToggleInputs(eventCaller.currentEntity.toggle);
                    }, 0.5f, true, new List<Param>()
                    {
                        new Param("toggle", true, "Enable Inputs")
                    }),
                }),
                new Minigame("countIn", "Count-Ins", "", false, true, new List<GameAction>()
                {
                    new GameAction("4 beat count-in",       delegate { var e = eventCaller.currentEntity; SoundEffects.FourBeatCountIn(e.beat, e.length / 4f, e.type); }, 4f, true, new List<Param>()
                    {
                        new Param("type", SoundEffects.CountInType.Normal, "Type", "The sounds to play for the count-in")
                    }),
                    new GameAction("8 beat count-in",       delegate { var e = eventCaller.currentEntity; SoundEffects.EightBeatCountIn(e.beat, e.length / 8f, e.type); }, 8f, true, new List<Param>()
                    {
                        new Param("type", SoundEffects.CountInType.Normal, "Type", "The sounds to play for the count-in")
                    }),
                    new GameAction("count",                 delegate { var e = eventCaller.currentEntity; SoundEffects.Count(e.type, e.toggle); }, 1f, false, new List<Param>()
                    {
                        new Param("type", SoundEffects.CountNumbers.One, "Number", "The sound to play"),
                        new Param("toggle", false, "Alt", "Whether or not the alternate version should be played")
                    }),
                    new GameAction("cowbell",               delegate { SoundEffects.Cowbell(); }, 1f),        
                    new GameAction("ready!",                delegate { var e = eventCaller.currentEntity; SoundEffects.Ready(e.beat, e.length / 2f); }, 2f, true),
                    new GameAction("and",                   delegate {SoundEffects.And(); }, 0.5f),
                    new GameAction("go!",                   delegate { SoundEffects.Go(eventCaller.currentEntity.toggle); }, 1f, false, new List<Param>()
                    {
                        new Param("toggle", false, "Alt", "Whether or not the alternate version should be played")
                    }),
                    // These are still here for backwards-compatibility but are hidden in the editor
                    new GameAction("4 beat count-in (alt)",     delegate { var e = eventCaller.currentEntity;  SoundEffects.FourBeatCountIn(e.beat, e.length, 1); }, 4f, hidden: true),
                    new GameAction("4 beat count-in (cowbell)", delegate { var e = eventCaller.currentEntity;  SoundEffects.FourBeatCountIn(e.beat, e.length, 2); }, 4f, hidden: true),
                    new GameAction("8 beat count-in (alt)",     delegate { var e = eventCaller.currentEntity;  SoundEffects.EightBeatCountIn(e.beat, e.length, 1); }, 4f, hidden: true),
                    new GameAction("8 beat count-in (cowbell)", delegate { var e = eventCaller.currentEntity;  SoundEffects.EightBeatCountIn(e.beat, e.length, 2); }, 4f, hidden: true),
                    new GameAction("one",                       delegate { SoundEffects.Count(0, false); }, 1f, hidden: true),
                    new GameAction("one (alt)",                 delegate { SoundEffects.Count(0, true); }, 1f, hidden: true),
                    new GameAction("two",                       delegate { SoundEffects.Count(1, false); }, 1f, hidden: true),
                    new GameAction("two (alt)",                 delegate { SoundEffects.Count(1, true); }, 1f, hidden: true),
                    new GameAction("three",                     delegate { SoundEffects.Count(2, false); }, 1f, hidden: true),
                    new GameAction("three (alt)",               delegate { SoundEffects.Count(2, true); }, 1f, hidden: true),
                    new GameAction("four",                      delegate { SoundEffects.Count(3, false); }, 1f, hidden: true),
                    new GameAction("four (alt)",                delegate { SoundEffects.Count(3, true); }, 1f, hidden: true),
                    new GameAction("go! (alt)",                 delegate { SoundEffects.Go(true); }, 1f, hidden: true),
                }),
                new Minigame("forkLifter", "Fork Lifter", "FFFFFF", false, false, new List<GameAction>()
                {
                    new GameAction("flick",                 delegate { var e = eventCaller.currentEntity; ForkLifter.instance.Flick(e.beat, e.type); }, 3, false, new List<Param>()
                    {
                        new Param("type", ForkLifter.FlickType.Pea, "Object", "The object to be flicked")
                    }),
                    new GameAction("prepare",               delegate { ForkLifter.instance.ForkLifterHand.Prepare(); }, 0.5f),
                    new GameAction("gulp",                  delegate { ForkLifter.playerInstance.Eat(); }),
                    new GameAction("sigh",                  delegate { Jukebox.PlayOneShot("games/forkLifter/sigh"); }),
                    // These are still here for backwards-compatibility but are hidden in the editor
                    new GameAction("pea",                   delegate { ForkLifter.instance.Flick(eventCaller.currentEntity.beat, 0); }, 3, hidden: true),
                    new GameAction("topbun",                delegate { ForkLifter.instance.Flick(eventCaller.currentEntity.beat, 1); }, 3, hidden: true),
                    new GameAction("burger",                delegate { ForkLifter.instance.Flick(eventCaller.currentEntity.beat, 2); }, 3, hidden: true),
                    new GameAction("bottombun",             delegate { ForkLifter.instance.Flick(eventCaller.currentEntity.beat, 3); }, 3, hidden: true),

                }),
                new Minigame("clappyTrio", "The Clappy Trio", "29E7FF", false, false, new List<GameAction>()
                {
                    new GameAction("clap",                  delegate { ClappyTrio.instance.Clap(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 3, true),
                    new GameAction("bop",                   delegate { ClappyTrio.instance.Bop(eventCaller.currentEntity.beat); } ),
                    new GameAction("prepare",               delegate { ClappyTrio.instance.Prepare(eventCaller.currentEntity.toggle ? 3 : 0); }, parameters: new List<Param>()
                    {
                        new Param("toggle", false, "Alt", "Whether or not the alternate version should be played")
                    }),
                    new GameAction("change lion count",     delegate { ClappyTrio.instance.ChangeLionCount((int)eventCaller.currentEntity.valA); }, 0.5f, false, new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Integer(1, 8, 3), "Lion Count", "The amount of lions")
                    }),
                    // This is still here for backwards-compatibility but is hidden in the editor
                    new GameAction("prepare_alt",           delegate { ClappyTrio.instance.Prepare(3); }, hidden: true),
                }),
                new Minigame("spaceball", "Spaceball", "00A518", false, false, new List<GameAction>()
                {
                    new GameAction("shoot",                 delegate { Spaceball.instance.Shoot(eventCaller.currentEntity.beat, false, eventCaller.currentEntity.type); }, 2, false),
                    new GameAction("shootHigh",             delegate { Spaceball.instance.Shoot(eventCaller.currentEntity.beat, true, eventCaller.currentEntity.type); }, 3),
                    new GameAction("costume",               delegate { Spaceball.instance.Costume(eventCaller.currentEntity.type); }, 1f, false, new List<Param>() 
                    {
                        new Param("type", Spaceball.CostumeType.Standard, "Type", "The costume to change to") 
                    } ),
                    new GameAction("alien",                 delegate { Spaceball.instance.alien.Show(eventCaller.currentEntity.beat); } ),
                    new GameAction("camera",                delegate { Spaceball.instance.OverrideCurrentZoom(); }, 4, true, new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Integer(1, 320, 10), "Zoom", "The camera's zoom level (Lower value = Zoomed in)"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "The easing function to use while zooming") 
                    } ),
                    new GameAction("prepare dispenser",     delegate { Spaceball.instance.PrepareDispenser(); }, 1 ),
                }),
                new Minigame("karateman", "Karate Man", "70A8D8", false, false, new List<GameAction>()
                {
                    new GameAction("bop",                   delegate { KarateMan.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 0.5f, true),
                    new GameAction("hit",                   delegate
                    {
                        KarateMan.instance.Shoot(eventCaller.currentEntity.beat, eventCaller.currentEntity.type);
                    }, 2, false, new List<Param>()
                    {
                        new Param("type", KarateMan.HitType.Pot, "Object", "The object to fire")
                    }),
                    new GameAction("bulb",                  delegate {
                        var e = eventCaller.currentEntity;
                        var c = KarateMan.instance.LightBulbColors[e.type];
                        if(e.type == (int)KarateMan.LightBulbType.Custom) c = e.colorA;
                        KarateMan.instance.Shoot(e.beat, 1, tint: c);
                    }, 2, false, new List<Param>()
                    {
                        new Param("type", KarateMan.LightBulbType.Normal, "Type", "The preset bulb type. Yellow is used for kicks while Blue is used for combos"),
                        new Param("colorA", new Color(), "Custom Color", "The color to use when the bulb type is set to Custom")
                    }),
                    new GameAction("kick",                  delegate { KarateMan.instance.Shoot(eventCaller.currentEntity.beat, 4); }, 4.5f),
                    new GameAction("combo",                 delegate { KarateMan.instance.Combo(eventCaller.currentEntity.beat); }, 4f),
                    new GameAction("hit3",                  delegate
                    {
                        var e = eventCaller.currentEntity;
                        switch ((KarateMan.HitThree)e.type)
                        {
                            case KarateMan.HitThree.HitTwo: KarateMan.instance.Hit2(e.beat); break;
                            case KarateMan.HitThree.HitThreeAlt: KarateMan.instance.Hit3(e.beat, true); break;
                            case KarateMan.HitThree.HitFour: KarateMan.instance.Hit4(e.beat); break;
                            default: KarateMan.instance.Hit3(e.beat); break;
                        }
                    }, 1f, false, new List<Param>()
                    {
                        new Param("type", KarateMan.HitThree.HitThree, "Type", "What should be called out")
                    }),
                    new GameAction("prepare",               delegate { KarateMan.instance.Prepare(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 1f, true),
                    new GameAction("set background color",  delegate {
                        var e = eventCaller.currentEntity;
                        var c = KarateMan.instance.BackgroundColors[e.type];
                        if(e.type == (int)KarateMan.BackgroundType.Custom) c = e.colorA;
                        KarateMan.instance.SetBackgroundColor(e.type, e.type2, c, e.colorB);
                    }, 0.5f, false, new List<Param>()
                    {
                        new Param("type", KarateMan.BackgroundType.Yellow, "Background Type", "The preset background type"),
                        new Param("type2", KarateMan.ShadowType.Tinted, "Shadow Type", "The shadow type. If Tinted doesn't work with your background color try Custom"),
                        new Param("colorA", new Color(), "Custom Background Color", "The background color to use when background type is set to Custom"),
                        new Param("colorB", new Color(), "Custom Shadow Color", "The shadow color to use when shadow type is set to Custom"),

                    }),
                    new GameAction("set background fx",  delegate {
                        KarateMan.instance.SetBackgroundFX((KarateMan.BackgroundFXType)eventCaller.currentEntity.type);
                    }, 0.5f, false, new List<Param>()
                    {
                        new Param("type", KarateMan.BackgroundFXType.None, "FX Type", "The background effect to be displayed")

                    }),
                    // These are still here for backwards-compatibility but are hidden in the editor
                    new GameAction("pot",                   delegate { KarateMan.instance.Shoot(eventCaller.currentEntity.beat, 0); }, 2, hidden: true),
                    new GameAction("rock",                  delegate { KarateMan.instance.Shoot(eventCaller.currentEntity.beat, 2); }, 2, hidden: true),
                    new GameAction("ball",                  delegate { KarateMan.instance.Shoot(eventCaller.currentEntity.beat, 3); }, 2, hidden: true),
                    new GameAction("tacobell",              delegate { KarateMan.instance.Shoot(eventCaller.currentEntity.beat, 999); }, 2, hidden: true),
                    new GameAction("hit4",                  delegate { KarateMan.instance.Hit4(eventCaller.currentEntity.beat); }, hidden: true),
                    new GameAction("bgfxon",                delegate { KarateMan.instance.SetBackgroundFX(KarateMan.BackgroundFXType.Sunburst); }, hidden: true),
                    new GameAction("bgfxoff",               delegate { KarateMan.instance.SetBackgroundFX(KarateMan.BackgroundFXType.None); }, hidden: true),

                }),
                new Minigame("spaceSoccer", "Space Soccer", "B888F8", false, false, new List<GameAction>()
                {
                    new GameAction("ball dispense",         delegate { SpaceSoccer.instance.Dispense(eventCaller.currentEntity.beat); }, 2f,
                    inactiveFunction: delegate { SpaceSoccer.DispenseSound(eventCaller.currentEntity.beat); }),
                    new GameAction("keep-up",               delegate { }, 4f, true),
                    new GameAction("high kick-toe!",        delegate { }, 3f, false, new List<Param>() 
                    {
                        new Param("swing", new EntityTypes.Float(0, 1, 0.5f), "Swing", "The amount of swing") 
                    }),
                }),
                new Minigame("djSchool", "DJ School", "008c97", false, false, new List<GameAction>()
                {
                    new GameAction("bop",                   delegate { DJSchool.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);  }, 0.5f, true),
                    new GameAction("and stop ooh",          delegate { var e = eventCaller.currentEntity; DJSchool.instance.AndStop(e.beat, e.toggle);  }, 2.5f, false, new List<Param>()
                    {
                        new Param("toggle", true, "Ooh", "Whether or not the \"ooh\" sound should be played")
                    }),
                    new GameAction("break c'mon ooh",       delegate { var e = eventCaller.currentEntity; DJSchool.instance.BreakCmon(e.beat, e.type, e.toggle);  }, 3f, false, new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoice.Standard, "Voice", "The voice line to play"),
                        new Param("toggle", true, "Ooh", "Whether or not the \"ooh\" sound should be played")
                    }),
                    new GameAction("scratch-o hey",         delegate { DJSchool.instance.ScratchoHey(eventCaller.currentEntity.beat, eventCaller.currentEntity.type);  }, 3f, false, new List<Param>()
                    {
                        new Param("type", DJSchool.DJVoice.Standard, "Voice", "The voice line to play"),
                    }),
                }),
                new Minigame("rhythmTweezers", "Rhythm Tweezers", "98b389", false, false, new List<GameAction>()
                {
                    new GameAction("start interval",        delegate { RhythmTweezers.instance.SetIntervalStart(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 4f, true),
                    new GameAction("short hair",            delegate { RhythmTweezers.instance.SpawnHair(eventCaller.currentEntity.beat); }, 0.5f),
                    new GameAction("long hair",             delegate { RhythmTweezers.instance.SpawnLongHair(eventCaller.currentEntity.beat); }, 0.5f),
                    new GameAction("next vegetable",        delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.NextVegetable(e.beat, e.type, e.colorA, e.colorB); }, 0.5f, false, new List<Param>() 
                    {
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "The vegetable to switch to"),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Onion Color", "The color of the onion"),
                        new Param("colorB", RhythmTweezers.defaultPotatoColor, "Potato Color", "The color of the potato")
                    } ),
                    new GameAction("change vegetable",      delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.ChangeVegetableImmediate(e.type, e.colorA, e.colorB); }, 0.5f, false, new List<Param>() 
                    {
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "The vegetable to switch to"),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Onion Color", "The color of the onion"),
                        new Param("colorB", RhythmTweezers.defaultPotatoColor, "Potato Color", "The color of the potato")
                    } ),
                    new GameAction("set tweezer delay",     delegate { RhythmTweezers.instance.tweezerBeatOffset = eventCaller.currentEntity.length; }, 1f, true),
                    new GameAction("reset tweezer delay",   delegate { RhythmTweezers.instance.tweezerBeatOffset = 0f; }, 0.5f),
                    new GameAction("set background color",  delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.ChangeBackgroundColor(e.colorA, 0f); }, 0.5f, false, new List<Param>() 
                    {
                        new Param("colorA", RhythmTweezers.defaultBgColor, "Background Color", "The background color to change to")
                    } ),
                    new GameAction("fade background color", delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.FadeBackgroundColor(e.colorA, e.colorB, e.length); }, 1f, true, new List<Param>() 
                    {
                        new Param("colorA", Color.white, "Start Color", "The starting color in the fade"),
                        new Param("colorB", RhythmTweezers.defaultBgColor, "End Color", "The ending color in the fade")
                    } ),
                }),
                
                new Minigame("rhythmRally", "Rhythm Rally", "FFFFFF", true, false, new List<GameAction>()
                {
                    new GameAction("bop",                   delegate { RhythmRally.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 0.5f, true),
                    new GameAction("whistle",               delegate { RhythmRally.instance.PlayWhistle(); }, 0.5f),
                    new GameAction("toss ball",             delegate { RhythmRally.instance.Toss(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, 6f, true); }, 2f),
                    new GameAction("rally",                 delegate { RhythmRally.instance.Serve(eventCaller.currentEntity.beat, RhythmRally.RallySpeed.Normal); }, 4f, true),
                    new GameAction("slow rally",            delegate { RhythmRally.instance.Serve(eventCaller.currentEntity.beat, RhythmRally.RallySpeed.Slow); }, 8f, true),
                    new GameAction("fast rally",            delegate { RhythmRally.instance.PrepareFastRally(eventCaller.currentEntity.beat, RhythmRally.RallySpeed.Fast); }, 6f),
                    new GameAction("superfast rally",       delegate { RhythmRally.instance.PrepareFastRally(eventCaller.currentEntity.beat, RhythmRally.RallySpeed.SuperFast); }, 12f),
                    new GameAction("pose",                  delegate { RhythmRally.instance.Pose(); }, 0.5f),
                    new GameAction("camera",                delegate {
                        var e = eventCaller.currentEntity;
                        var rotation = new Vector3(0, e.valA, 0);
                        RhythmRally.instance.ChangeCameraAngle(rotation, e.valB, e.length, (Ease)e.type, (RotateMode)e.type2);
                    }, 4, true, new List<Param>() {
                        new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Angle", "The rotation of the camera around the center of the table"),
                        new Param("valB", new EntityTypes.Float(0.5f, 4f, 1), "Zoom", "The camera's level of zoom (Lower value = Zoomed in)"),
                        new Param("type", Ease.Linear, "Ease", "The easing function to use"),
                        new Param("type2", RotateMode.Fast, "Rotation Mode", "The rotation mode to use")
                    } ),
                }),
                new Minigame("builtToScaleDS", "Built To Scale (DS)", "00BB00", true, false, new List<GameAction>()
                {
                    new GameAction("spawn blocks",          delegate { }, 1f, true),
                    new GameAction("play piano",            delegate { BuiltToScaleDS.instance.PlayPiano(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity.type); }, 1f, true, new List<Param>() 
                    {
                        new Param("type", new EntityTypes.Integer(-24, 24, 0), "Semitones", "The number of semitones up or down this note should be pitched")
                    } ),
                }),
                new Minigame("tapTrial", "Tap Trial \n<color=#eb5454>[WIP don't use]</color>", "93ffb3", false, false, new List<GameAction>()
                {
                    new GameAction("tap",                   delegate { TapTrial.instance.Tap(eventCaller.currentEntity.beat); }, 2.0f, false),
                    new GameAction("double tap",            delegate { TapTrial.instance.DoubleTap(eventCaller.currentEntity.beat); }, 2.0f, false),
                    new GameAction("triple tap",            delegate { TapTrial.instance.TripleTap(eventCaller.currentEntity.beat); }, 4.0f, false),
                    new GameAction("jump tap",              delegate { TapTrial.instance.JumpTap(eventCaller.currentEntity.beat); }, 2.0f, false),
                    new GameAction("final jump tap",        delegate { TapTrial.instance.FinalJumpTap(eventCaller.currentEntity.beat); }, 2.0f, false),
                }),
                new Minigame("cropStomp", "Crop Stomp", "BFDEA6", false, false, new List<GameAction>()
                {
                    new GameAction("start marching",        delegate { CropStomp.instance.StartMarching(eventCaller.currentEntity.beat); }, 2f, false, inactiveFunction: delegate { CropStomp.MarchInactive(eventCaller.currentEntity.beat); }),
                    new GameAction("veggies",               delegate { }, 4f, true),
                    new GameAction("mole",               delegate { }, 2f, false),
                }),
                new Minigame("wizardsWaltz", "Wizard's Waltz \n<color=#adadad>(Mahou Tsukai)</color>", "FFEF9C", false, false, new List<GameAction>()
                {
                    new GameAction("start interval",        delegate { WizardsWaltz.instance.SetIntervalStart(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 4f, true),
                    new GameAction("plant",        delegate { WizardsWaltz.instance.SpawnFlower(eventCaller.currentEntity.beat); }, 0.5f, false),
                }),
                new Minigame("mrUpbeat", "Mr. Upbeat", "FFFFFF", false, false, new List<GameAction>()
                {
                    new GameAction("prepare",               delegate { MrUpbeat.instance.SetInterval(eventCaller.currentEntity.beat); }, 0.5f, true, inactiveFunction: delegate { MrUpbeat.Beep(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }),
                    new GameAction("go",                    delegate { MrUpbeat.instance.Go(eventCaller.currentEntity.beat);  }, 4f, true),
                    new GameAction("ding!",                 delegate { MrUpbeat.instance.Ding(eventCaller.currentEntity.toggle); }, 0.5f, parameters: new List<Param>()
                    {
                        new Param("toggle", false, "Applause")
                    }),
                }),
                new Minigame("drummingPractice", "Drumming Practice", "2BCF33", false, false, new List<GameAction>()
                {
                    new GameAction("bop",                   delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.SetBop(e.beat, e.length); }, 0.5f, true),
                    new GameAction("drum",                  delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.Prepare(e.beat, e.toggle); }, 2f, parameters: new List<Param>()
                    {
                        new Param("toggle", true, "Applause", "Whether or not an applause should be played on a successful hit")
                    }),
                    new GameAction("set mii",               delegate { var e = eventCaller.currentEntity; DrummingPractice.instance.SetMiis(e.type, e.type2, e.type3, e.toggle); }, 0.5f, parameters: new List<Param>()
                    {
                        new Param("type", DrummingPractice.MiiType.Random, "Player Mii", "The Mii that the player will control"),
                        new Param("type2", DrummingPractice.MiiType.Random, "Left Mii", "The Mii on the left"),
                        new Param("type3", DrummingPractice.MiiType.Random, "Right Mii", "The Mii on the right"),
                        new Param("toggle", false, "Set All to Player", "Sets all Miis to the Player's Mii")
                    }),

                }),
                new Minigame("blueBear", "Blue Bear \n<color=#eb5454>[WIP don't use]</color>", "B4E6F6", false, false, new List<GameAction>()
                {
                    new GameAction("donut",                 delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, false); }, 3, false),
                    new GameAction("cake",                  delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, true); }, 4, false),
                }),
                /*new Minigame("spaceDance", "Space Dance", "B888F8", new List<GameAction>()
                {
                }),
                new Minigame("sneakySpirits", "Sneaky Spirits", "B888F8", new List<GameAction>()
                {
                }),
                new Minigame("munchyMonk", "Munchy Monk", "B888F8", new List<GameAction>()
                {
                }),
                new Minigame("airRally", "Air Rally", "B888F8", new List<GameAction>()
                {
                }),
                new Minigame("ringside", "Ringside", "B888F8", new List<GameAction>()
                {
                }),
                new Minigame("workingDough", "Working Dough", "B888F8", new List<GameAction>()
                {
                })*/
            };
        }
    }
}