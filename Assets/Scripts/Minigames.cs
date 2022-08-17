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

            private AssetBundle bundleCommon = null;
            private bool commonLoaded = false;
            private bool commonPreloaded = false;
            private string currentLoadedLocale = "";
            private AssetBundle bundleLocalized = null;
            private bool localeLoaded = false;
            private bool localePreloaded = false;

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
                    new GameAction("switchGame",            delegate { GameManager.instance.SwitchGame(eventCaller.currentSwitchGame, eventCaller.currentEntity.beat); }, 0.5f, inactiveFunction: delegate { GameManager.instance.SwitchGame(eventCaller.currentSwitchGame, eventCaller.currentEntity.beat); }),
                    new GameAction("end",                   delegate { Debug.Log("end"); GameManager.instance.Stop(0); Timeline.instance?.SetTimeButtonColors(true, false, false);}),
                    new GameAction("skill star",            delegate {  }, 1f, true),
                   
                    new GameAction("toggle inputs",            delegate
                    {
                        GameManager.instance.ToggleInputs(eventCaller.currentEntity.toggle);
                    }, 0.5f, true, new List<Param>()
                    {
                        new Param("toggle", true, "Enable Inputs")
                    }),

                    // DEPRECATED! Now in VFX
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
                    }, hidden: true ),

                    new GameAction("move camera",              delegate 
                    {
                    }, 1f, true, new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Float(-50, 50, 0), "Right / Left"),
                        new Param("valB", new EntityTypes.Float(-50, 50, 0), "Up / Down"),
                        new Param("valC", new EntityTypes.Float(-0, 250, 10), "In / Out"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease Type")
                    },
                    hidden: true ),

                    new GameAction("rotate camera",            delegate 
                    {
                    }, 1f, true, new List<Param>() 
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

                new Minigame("vfx", "Visual Effects", "", false, true, new List<GameAction>()
                {
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
                    }, hidden: false ),

                    new GameAction("screen shake",              delegate
                    {
                        //TODO: move cam
                    }, 1f, true, new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Float(0, 10, 2), "Intensity")
                    } ),

                    new GameAction("move camera",              delegate 
                    {
                        //TODO: move cam
                    }, 1f, true, new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Float(-50, 50, 0), "Right / Left"),
                        new Param("valB", new EntityTypes.Float(-50, 50, 0), "Up / Down"),
                        new Param("valC", new EntityTypes.Float(-0, 250, 10), "In / Out"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease Type")
                    } ),

                    new GameAction("rotate camera",            delegate 
                    {
                        //TODO: rot cam
                    }, 1f, true, new List<Param>() 
                    {
                        new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Pitch"),
                        new Param("valB", new EntityTypes.Integer(-360, 360, 0), "Yaw"),
                        new Param("valC", new EntityTypes.Integer(-360, 360, 0), "Roll"),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease Type")
                    } ),

                    new GameAction("display textbox",           delegate 
                    {
                    }, 1f, true, new List<Param>() 
                    {
                        new Param("text1", "", "Text", "The text to display in the textbox (Rich Text is supported!)"),
                        new Param("type", Games.Global.Textbox.TextboxAnchor.TopMiddle, "Anchor", "Where to anchor the textbox"),
                        new Param("valA", new EntityTypes.Float(0.25f, 4, 1), "Textbox Width", "Textbox width multiplier"),
                        new Param("valB", new EntityTypes.Float(0.5f, 8, 1), "Textbox Height", "Textbox height multiplier")
                    } ),
                    new GameAction("display open captions",           delegate 
                    {
                    }, 1f, true, new List<Param>() 
                    {
                        new Param("text1", "", "Text", "The text to display in the captions (Rich Text is supported!)"),
                        new Param("type", Games.Global.Textbox.TextboxAnchor.BottomMiddle, "Anchor", "Where to anchor the captions"),
                        new Param("valA", new EntityTypes.Float(0.25f, 4, 1), "Captions Width", "Captions width multiplier"),
                        new Param("valB", new EntityTypes.Float(0.5f, 8, 1), "Captions Height", "Captions height multiplier")
                    } ),
                    new GameAction("display closed captions",           delegate 
                    {
                    }, 1f, true, new List<Param>() 
                    {
                        new Param("text1", "", "Text", "The text to display in the captions (Rich Text is supported!)"),
                        new Param("type", Games.Global.Textbox.ClosedCaptionsAnchor.Top, "Anchor", "Where to anchor the captions"),
                        new Param("valA", new EntityTypes.Float(0.5f, 4, 1), "Captions Height", "Captions height multiplier")
                    } ),
                    new GameAction("display song artist",           delegate 
                    {
                    }, 1f, true, new List<Param>() 
                    {
                        new Param("text1", "", "Title", "Text to display in the upper label (Rich Text is supported!)"),
                        new Param("text2", "", "Artist", "Text to display in the lower label (Rich Text is supported!)"),
                    } ),
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