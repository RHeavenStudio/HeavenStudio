using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using DG.Tweening;

using HeavenStudio.Util;
using HeavenStudio.Editor.Track;
using HeavenStudio.Games;

using System;
using System.Linq;
using System.Reflection;
using System.IO;

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

            public List<string> tags;
            public string defaultLocale = "en";
            public string wantAssetBundle = "";
            public List<string> supportedLocales;

            public bool usesAssetBundle => (wantAssetBundle != "");
            public bool hasLocales => (supportedLocales.Count > 0);
            public bool AssetsLoaded => (((hasLocales && localeLoaded && currentLoadedLocale == defaultLocale) || (!hasLocales)) && commonLoaded);
            public bool SequencesPreloaded => soundSequences != null;

            private AssetBundle bundleCommon = null;
            private bool commonLoaded = false;
            private bool commonPreloaded = false;
            private string currentLoadedLocale = "";
            private AssetBundle bundleLocalized = null;
            private bool localeLoaded = false;
            private bool localePreloaded = false;

            private SoundSequence.SequenceKeyValue[] soundSequences = null;

            public SoundSequence.SequenceKeyValue[] LoadedSoundSequences
            {
                get => soundSequences;
                set => soundSequences = value;
            }

            public Minigame(string name, string displayName, string color, bool threeD, bool fxOnly, List<GameAction> actions, List<string> tags = null, string assetBundle = "", string defaultLocale = "en", List<string> supportedLocales = null)
            {
                this.name = name;
                this.displayName = displayName;
                this.color = color;
                this.actions = actions;
                this.threeD = threeD;
                this.fxOnly = fxOnly;

                this.tags = tags ?? new List<string>();
                this.wantAssetBundle = assetBundle;
                this.defaultLocale = defaultLocale;
                this.supportedLocales = supportedLocales ?? new List<string>();
            }

            public AssetBundle GetLocalizedAssetBundle()
            {
                if (!hasLocales) return null;
                if (!usesAssetBundle) return null;
                if (bundleLocalized == null || currentLoadedLocale != defaultLocale) //TEMPORARY: use the game's default locale until we add localization support
                {
                    if (localeLoaded) return bundleLocalized;
                    // TODO: try/catch for missing assetbundles
                    currentLoadedLocale = defaultLocale;
                    bundleLocalized = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, wantAssetBundle + "/locale." + defaultLocale));
                    localeLoaded = true;
                }
                return bundleLocalized;
            }

            public AssetBundle GetCommonAssetBundle()
            {
                if (commonLoaded) return bundleCommon;
                if (!usesAssetBundle) return null;
                if (bundleCommon == null)
                {
                    // TODO: try/catch for missing assetbundles
                    bundleCommon = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, wantAssetBundle + "/common"));
                    commonLoaded = true;
                }
                return bundleCommon;
            }

            public IEnumerator LoadCommonAssetBundleAsync()
            {
                if (commonPreloaded || commonLoaded) yield break;
                commonPreloaded = true;
                if (!usesAssetBundle) yield break;
                if (bundleCommon != null) yield break;

                AssetBundleCreateRequest asyncBundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, wantAssetBundle + "/common"));
                if (bundleCommon != null) yield break;
                yield return asyncBundleRequest;

                AssetBundle localAssetBundle = asyncBundleRequest.assetBundle;
                if (bundleCommon != null) yield break;
                yield return localAssetBundle;

                if (localAssetBundle == null) yield break;

                bundleCommon = localAssetBundle;
                commonLoaded = true;
            }

            public IEnumerator LoadLocalizedAssetBundleAsync()
            {
                if (localePreloaded) yield break;
                localePreloaded = true;
                if (!hasLocales) yield break;
                if (!usesAssetBundle) yield break;
                if (localeLoaded && bundleLocalized != null && currentLoadedLocale == defaultLocale) yield break;

                AssetBundleCreateRequest asyncBundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, wantAssetBundle + "/locale." + defaultLocale));
                if (localeLoaded && bundleLocalized != null && currentLoadedLocale == defaultLocale) yield break;
                yield return asyncBundleRequest;

                AssetBundle localAssetBundle = asyncBundleRequest.assetBundle;
                if (localeLoaded && bundleLocalized != null && currentLoadedLocale == defaultLocale) yield break;
                yield return localAssetBundle;

                if (localAssetBundle == null) yield break;

                bundleLocalized = localAssetBundle;
                currentLoadedLocale = defaultLocale;
                localeLoaded = true;
            }
        }

        public class GameAction
        {
            public string actionName;
            public string displayName;
            public EventCallback function = delegate { };
            public float defaultLength = 1;
            public bool resizable = false;
            public List<Param> parameters = null;
            public bool hidden = false;
            public int priority = 0;
            public EventCallback inactiveFunction = delegate { };
            public EventCallback preFunction = delegate { };

            /// <summary>
            /// <para>Creates a block that can be used in the editor. The block's function and attributes are defined in the parentheses.</para>
            /// <para>Note: Every parameter after the second one is an optional parameter. You can change optional parameters by adding (name): (value) after the second parameter.</para>
            /// </summary>
            /// <param name="actionName">Entity model name</param>
            /// <param name="displayName">Name of the block used in the UI</param>
            /// <param name="defaultLength">How long the block appears in the editor</param>
            /// <param name="resizable">Allows the user to resize the block</param>
            /// <param name="parameters">Extra parameters for this block that change how it functions.</param>
            /// <param name="function"><para>What the block does when read during playback</para>
            /// <para>Only does this if the game that it is associated with is loaded.</para></param>
            /// <param name="inactiveFunction">What the block does when read while the game it's associated with isn't loaded.</param>
            /// <param name="prescheduleFunction">What the block does when the GameManager seeks to this cue for pre-scheduling.</param>
            /// <param name="hidden">Prevents the block from being shown in the game list. Block will still function normally if it is in the timeline.</param>
            /// <param name="preFunction">Runs two beats before this event is reached.</param>
            /// <param name="priority">Priority of this event. Higher priority events will be run first.</param>
            public GameAction(string actionName, string displayName, float defaultLength = 1, bool resizable = false, List<Param> parameters = null, EventCallback function = null, EventCallback inactiveFunction = null, EventCallback prescheduleFunction = null, bool hidden = false, EventCallback preFunction = null, int priority = 0)
            {
                this.actionName = actionName;
                if (displayName == String.Empty) this.displayName = actionName;
                else this.displayName = displayName;
                this.defaultLength = defaultLength;
                this.resizable = resizable;
                this.parameters = parameters;
                this.hidden = hidden;

                this.function = function ?? delegate { };
                this.inactiveFunction = inactiveFunction ?? delegate { };
                this.preFunction = prescheduleFunction ?? delegate { };
                this.priority = priority;


                //todo: converting to new versions of GameActions
            }

            /// <summary>
            /// <para>Shorthand constructor for a GameAction with only required data</para>
            /// </summary>
            /// <param name="actionName">Entity model name</param>
            /// <param name="displayName">Name of the block used in the UI</param>
            public GameAction(string actionName, string displayName)
            {
                this.actionName = actionName;
                if (displayName == String.Empty) this.displayName = actionName;
                else this.displayName = displayName;
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
            /// <param name="propertyName">The name of the variable that's being changed.</param>
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
        public delegate void ParamChangeCallback(string paramName, object paramValue, DynamicBeatmap.DynamicEntity entity);

        // overengineered af but it's a modified version of
        // https://stackoverflow.com/a/19877141
        static List<Func<EventCaller, Minigame>> loadRunners;
        static void BuildLoadRunnerList() {
            loadRunners = System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.Namespace == "HeavenStudio.Games.Loaders" && x.GetMethod("AddGame", BindingFlags.Public | BindingFlags.Static) != null)
            .Select(t => (Func<EventCaller, Minigame>) Delegate.CreateDelegate(
                typeof(Func<EventCaller, Minigame>), 
                null, 
                t.GetMethod("AddGame", BindingFlags.Public | BindingFlags.Static),
                false
                ))
            .ToList();
                
        }

        public static void Init(EventCaller eventCaller)
        {
            eventCaller.minigames = new List<Minigame>()
            {
                new Minigame("gameManager", "Game Manager", "", false, true, new List<GameAction>()
                {
                    new GameAction("switchGame", "Switch Game", 0.5f, false, 
                        function: delegate { GameManager.instance.SwitchGame(eventCaller.currentSwitchGame, eventCaller.currentEntity.beat); }, 
                        inactiveFunction: delegate { GameManager.instance.SwitchGame(eventCaller.currentSwitchGame, eventCaller.currentEntity.beat); }
                    ),
                    new GameAction("end", "End Remix",
                        function: delegate { 
                            Debug.Log("end"); 
                            if (Timeline.instance != null)
                                Timeline.instance?.Stop(0);
                            else
                                GameManager.instance.Stop(0);
                        }
                    ),
                    new GameAction("skill star", "Skill Star", 1f, true),
                    new GameAction("toggle inputs", "Toggle Inputs", 0.5f, true,
                        new List<Param>()
                        {
                            new Param("toggle", true, "Enable Inputs")
                        },
                        delegate
                        {
                            GameManager.instance.ToggleInputs(eventCaller.currentEntity["toggle"]);
                        }
                    ),

                    // These are still here for backwards-compatibility but are hidden in the editor
                    new GameAction("flash", "", 1f, true, 
                        new List<Param>()
                        {
                            new Param("colorA", Color.white, "Start Color"),
                            new Param("colorB", Color.white, "End Color"),
                            new Param("valA", new EntityTypes.Float(0, 1, 1), "Start Opacity"),
                            new Param("valB", new EntityTypes.Float(0, 1, 0), "End Opacity"),
                            new Param("ease", EasingFunction.Ease.Linear, "Ease")
                        },
                        hidden: true
                    ),
                    new GameAction("move camera", "", 1f, true, new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Float(-50, 50, 0), "Right / Left"),
                        new Param("valB", new EntityTypes.Float(-50, 50, 0), "Up / Down"),
                        new Param("valC", new EntityTypes.Float(-0, 250, 10), "In / Out"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease Type")
                    },
                    hidden: true ),
                    new GameAction("rotate camera", "", 1f, true, new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Pitch"),
                        new Param("valB", new EntityTypes.Integer(-360, 360, 0), "Yaw"),
                        new Param("valC", new EntityTypes.Integer(-360, 360, 0), "Roll"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease Type")
                    },
                    hidden: true ),
                }),

                new Minigame("countIn", "Count-Ins", "", false, true, new List<GameAction>()
                {
                    new GameAction("4 beat count-in", "4 Beat Count-In", 4f, true,
                        new List<Param>()
                        {
                            new Param("type", SoundEffects.CountInType.Normal, "Type", "The sounds to play for the count-in")
                        },
                        delegate { var e = eventCaller.currentEntity; SoundEffects.FourBeatCountIn(e.beat, e.length / 4f, e["type"]); }
                    ),
                    new GameAction("8 beat count-in", "8 Beat Count-In", 8f, true,
                        new List<Param>()
                        {
                            new Param("type", SoundEffects.CountInType.Normal, "Type", "The sounds to play for the count-in")
                        },
                        delegate { var e = eventCaller.currentEntity; SoundEffects.EightBeatCountIn(e.beat, e.length / 8f, e["type"]); }
                    ),
                    new GameAction("count", "Count", 1f, false,
                        new List<Param>()
                        {
                            new Param("type", SoundEffects.CountNumbers.One, "Number", "The sound to play"),
                            new Param("toggle", false, "Alt", "Whether or not the alternate version should be played")
                        },
                        delegate { var e = eventCaller.currentEntity; SoundEffects.Count(e["type"], e["toggle"]); }
                    ),
                    new GameAction("cowbell", "Cowbell",
                        function: delegate { SoundEffects.Cowbell(); }
                    ),
                    new GameAction("ready!", "Ready!", 2f, true,
                        function: delegate { var e = eventCaller.currentEntity; SoundEffects.Ready(e.beat, e.length / 2f); }
                    ),
                    new GameAction("and", "And", 0.5f,
                        function: delegate { SoundEffects.And(); }
                    ),
                    new GameAction("go!", "Go!", 1f, false, 
                        new List<Param>()
                        {
                            new Param("toggle", false, "Alt", "Whether or not the alternate version should be played")
                        },
                        function: delegate { SoundEffects.Go(eventCaller.currentEntity["toggle"]); }
                    ),

                    // These are still here for backwards-compatibility but are hidden in the editor
                    new GameAction("4 beat count-in (alt)", "", 4f, function: delegate { var e = eventCaller.currentEntity; SoundEffects.FourBeatCountIn(e.beat, e.length, 1); }, hidden: true),
                    new GameAction("4 beat count-in (cowbell)", "", 4f, function: delegate { var e = eventCaller.currentEntity; SoundEffects.FourBeatCountIn(e.beat, e.length, 2); }, hidden: true),
                    new GameAction("8 beat count-in (alt)", "", 8f, function: delegate { var e = eventCaller.currentEntity; SoundEffects.EightBeatCountIn(e.beat, e.length, 1); }, hidden: true),
                    new GameAction("8 beat count-in (cowbell)", "", 8f, function: delegate { var e = eventCaller.currentEntity; SoundEffects.EightBeatCountIn(e.beat, e.length, 2); }, hidden: true),

                    new GameAction("one", "", function: delegate { SoundEffects.Count(0, false); }, hidden: true),
                    new GameAction("two", "", function: delegate { SoundEffects.Count(1, false); }, hidden: true),
                    new GameAction("three", "", function: delegate { SoundEffects.Count(2, false); }, hidden: true),
                    new GameAction("four", "", function: delegate { SoundEffects.Count(3, false); }, hidden: true),
                    new GameAction("one (alt)", "", function: delegate { SoundEffects.Count(0, true); }, hidden: true),
                    new GameAction("two (alt)", "", function: delegate { SoundEffects.Count(1, true); }, hidden: true),
                    new GameAction("three (alt)", "", function: delegate { SoundEffects.Count(2, true); }, hidden: true),
                    new GameAction("four (alt)", "", function: delegate { SoundEffects.Count(3, true); }, hidden: true),
                    new GameAction("go! (alt)", "", function: delegate { SoundEffects.Go(true); }, hidden: true),
                }),

                new Minigame("vfx", "Visual Effects", "", false, true, new List<GameAction>()
                {
                    new GameAction("flash", "Flash", 1f, true, 
                        new List<Param>()
                        {
                            new Param("colorA", Color.white, "Start Color"),
                            new Param("colorB", Color.white, "End Color"),
                            new Param("valA", new EntityTypes.Float(0, 1, 1), "Start Opacity"),
                            new Param("valB", new EntityTypes.Float(0, 1, 0), "End Opacity"),
                            new Param("ease", EasingFunction.Ease.Linear, "Ease")
                        }
                    ),
                    new GameAction("filter", "Filter", 1f, true,
                        new List<Param>()
                        {
                            new Param("filter", Games.Global.Filter.FilterType.grayscale, "Filter"),
                            new Param("inten", new EntityTypes.Float(0, 100, 100), "Intensity"),
                            new Param("fadein", new EntityTypes.Float(0, 100, 0), "Fade In"),
                            new Param("fadeout", new EntityTypes.Float(0, 100, 0), "Fade Out")
                        }
                    ),
                    new GameAction("move camera", "Move Camera", 1f, true, new List<Param>() 
                        {
                            new Param("valA", new EntityTypes.Float(-50, 50, 0), "Right / Left"),
                            new Param("valB", new EntityTypes.Float(-50, 50, 0), "Up / Down"),
                            new Param("valC", new EntityTypes.Float(-0, 250, 10), "In / Out"),
                            new Param("ease", EasingFunction.Ease.Linear, "Ease Type")
                        }
                    ),
                    new GameAction("rotate camera", "Rotate Camera", 1f, true, new List<Param>() 
                        {
                            new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Pitch"),
                            new Param("valB", new EntityTypes.Integer(-360, 360, 0), "Yaw"),
                            new Param("valC", new EntityTypes.Integer(-360, 360, 0), "Roll"),
                            new Param("ease", EasingFunction.Ease.Linear, "Ease Type")
                        } 
                    ),

                    new GameAction("screen shake", "Screen Shake", 1f, true,
                        new List<Param>()
                        {
                            new Param("valA", new EntityTypes.Float(0, 10, 0), "Horizontal Intensity"),
                            new Param("valB", new EntityTypes.Float(0, 10, 1), "Vertical Intensity")
                        }
                    ),

                    new GameAction("display textbox", "Display Textbox", 1f, true, new List<Param>() 
                        {
                            new Param("text1", "", "Text", "The text to display in the textbox (Rich Text is supported!)"),
                            new Param("type", Games.Global.Textbox.TextboxAnchor.TopMiddle, "Anchor", "Where to anchor the textbox"),
                            new Param("valA", new EntityTypes.Float(0.25f, 4, 1), "Textbox Width", "Textbox width multiplier"),
                            new Param("valB", new EntityTypes.Float(0.5f, 8, 1), "Textbox Height", "Textbox height multiplier")
                        }
                    ),
                    new GameAction("display open captions", "Display Open Captions", 1f, true, 
                        new List<Param>() 
                        {
                            new Param("text1", "", "Text", "The text to display in the captions (Rich Text is supported!)"),
                            new Param("type", Games.Global.Textbox.TextboxAnchor.BottomMiddle, "Anchor", "Where to anchor the captions"),
                            new Param("valA", new EntityTypes.Float(0.25f, 4, 1), "Captions Width", "Captions width multiplier"),
                            new Param("valB", new EntityTypes.Float(0.5f, 8, 1), "Captions Height", "Captions height multiplier")
                        } 
                    ),
                    new GameAction("display closed captions", "Display Closed Captions", 1f, true, 
                        new List<Param>() 
                        {
                            new Param("text1", "", "Text", "The text to display in the captions (Rich Text is supported!)"),
                            new Param("type", Games.Global.Textbox.ClosedCaptionsAnchor.Top, "Anchor", "Where to anchor the captions"),
                            new Param("valA", new EntityTypes.Float(0.5f, 4, 1), "Captions Height", "Captions height multiplier")
                        }
                    ),
                    new GameAction("display song artist", "Display Song Info", 1f, true, 
                        new List<Param>()
                        {
                            new Param("text1", "", "Title", "Text to display in the upper label (Rich Text is supported!)"),
                            new Param("text2", "", "Artist", "Text to display in the lower label (Rich Text is supported!)"),
                        }
                    ),
                }),
            };

            BuildLoadRunnerList();
            foreach(var load in loadRunners)
            {
                Debug.Log("Running game loader " + RuntimeReflectionExtensions.GetMethodInfo(load).DeclaringType.Name);
                eventCaller.minigames.Add(load(eventCaller));
            }
        }
    }
}