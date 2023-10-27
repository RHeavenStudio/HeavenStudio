using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

using UnityEngine;
using DG.Tweening;

using HeavenStudio.Util;
using HeavenStudio.Editor.Track;
using HeavenStudio.Games;
using Jukebox;

using System;
using System.Linq;
using System.Reflection;
using System.IO;

namespace HeavenStudio
{

    public class Minigames
    {
        public enum RecommendedControlStyle
        {
            Any,
            Pad,
            Touch,
            Baton,
        }

        public static void InitPreprocessor()
        {
            RiqBeatmap.OnUpdateBeatmap += PreProcessBeatmap;
        }

        public static Dictionary<string, object> propertiesModel = new()
            {
                // mapper set properties? (future: use this to flash the button)
                {"propertiesmodified", false},

                ////// CATEGORY 1: SONG INFO
                // general chart info
                {"remixtitle", "New Remix"},        // chart name
                {"remixauthor", "Your Name"},       // charter's name
                {"remixdesc", "Remix Description"}, // chart description
                {"remixlevel", 1},                  // chart difficulty (maybe offer a suggestion but still have the mapper determine it)
                {"remixtempo", 120f},               // avg. chart tempo
                {"remixtags", ""},                  // chart tags
                {"icontype", 0},                    // chart icon (presets, custom - future)
                {"iconurl", ""},                    // custom icon location (future)
                {"challengetype", 0},               // perfect challenge type
                {"playstyle", RecommendedControlStyle.Any},                   // recommended control style

                // chart song info
                {"idolgenre", "Song Genre"},        // song genre
                {"idolsong", "Song Name"},          // song name
                {"idolcredit", "Artist"},           // song artist

                ////// CATEGORY 2: PROLOGUE AND EPILOGUE
                // chart prologue
                {"prologuetype", 0},                // prologue card animation (future)
                {"prologuecaption", "Remix"},       // prologue card sub-title (future)

                // chart results screen messages
                {"resultcaption", "Rhythm League Notes"},                       // result screen header
                {"resultcommon_hi", "Good rhythm."},                            // generic "Superb" message (one-liner, or second line for single-type)
                {"resultcommon_ok", "Eh. Passable."},                           // generic "OK" message (one-liner, or second line for single-type)
                {"resultcommon_ng", "Try harder next time."},                   // generic "Try Again" message (one-liner, or second line for single-type)

                    // the following are shown / hidden in-editor depending on the tags of the games used
                {"resultnormal_hi", "You show strong fundamentals."},           // "Superb" message for normal games (two-liner)
                {"resultnormal_ng", "Work on your fundamentals."},              // "Try Again" message for normal games (two-liner)

                {"resultkeep_hi", "You kept the beat well."},                   // "Superb" message for keep-the-beat games (two-liner)
                {"resultkeep_ng", "You had trouble keeping the beat."},         // "Try Again" message for keep-the-beat games (two-liner)

                {"resultaim_hi", "You had great aim."},                         // "Superb" message for aim games (two-liner)
                {"resultaim_ng", "Your aim was a little shaky."},               // "Try Again" message for aim games (two-liner)

                {"resultrepeat_hi", "You followed the example well."},          // "Superb" message for call-and-response games (two-liner)
                {"resultrepeat_ng", "Next time, follow the example better."},   // "Try Again" message for call-and-response games (two-liner)
            };

        static Dictionary<string, object> tempoChangeModel = new()
        {
            {"tempo", 120f},
            {"swing", 0f},
            {"timeSignature", new Vector2(4, 4)},
        };

        static Dictionary<string, object> volumeChangeModel = new()
        {
            {"volume", 1f},
            {"fade", Util.EasingFunction.Ease.Instant},
        };

        static Dictionary<string, object> sectionMarkModel = new()
        {
            {"sectionName", ""},
            {"isCheckpoint", false},
            {"startPerfect", false},
            {"breakSection", false},
            {"extendsPrevious", false},
            {"sectionWeight", 1f},
        };

        static void PreProcessSpecialEntity(RiqEntity e, Dictionary<string, object> model)
        {
            foreach (var t in model)
            {
                string propertyName = t.Key;
                Type type = t.Value.GetType();
                if (!e.dynamicData.ContainsKey(propertyName))
                {
                    e.CreateProperty(propertyName, t.Value);
                }
                Type pType = e[propertyName].GetType();
                if (pType != type)
                {
                    try
                    {
                        if (type.IsEnum)
                        {
                            if (pType == typeof(string))
                                e.dynamicData[propertyName] = (int)Enum.Parse(type, (string)e[propertyName]);
                            else
                                e.dynamicData[propertyName] = (int)e[propertyName];
                        }
                        else if (pType == typeof(Newtonsoft.Json.Linq.JObject))
                            e[propertyName] = e[propertyName].ToObject(type);
                        else
                            e[propertyName] = Convert.ChangeType(e[propertyName], type);
                    }
                    catch
                    {
                        Debug.LogWarning($"Could not convert {propertyName} to {type}! Using default value...");
                        // use default value
                        e.CreateProperty(propertyName, t.Value);
                    }
                }
            }
        }

        /// <summary>
        /// processes an riq beatmap after it is loaded
        /// </summary>
        public static RiqBeatmapData? PreProcessBeatmap(string version, RiqBeatmapData data)
        {
            Debug.Log("Preprocessing beatmap...");
            Minigames.Minigame game;
            Minigames.GameAction action;
            System.Type type, pType;
            foreach (var e in data.entities)
            {
                var gameName = e.datamodel.Split(0);
                var actionName = e.datamodel.Split(1);
                game = EventCaller.instance.GetMinigame(gameName);
                if (game == null)
                {
                    Debug.LogWarning($"Unknown game {gameName} found in remix.json! Adding game...");
                    game = new Minigames.Minigame(gameName, gameName.DisplayName() + " \n<color=#eb5454>[inferred from remix.json]</color>", "", false, false, new List<Minigames.GameAction>(), inferred: true);
                    EventCaller.instance.minigames.Add(game);
                    if (Editor.Editor.instance != null)
                        Editor.Editor.instance.AddIcon(game);
                }
                action = EventCaller.instance.GetGameAction(game, actionName);
                if (action == null)
                {
                    Debug.LogWarning($"Unknown action {gameName}/{actionName} found in remix.json! Adding action...");
                    var parameters = new List<Minigames.Param>();
                    foreach (var item in e.dynamicData)
                    {
                        Debug.Log($"k: {item.Key}, v: {item.Value}");
                        if (item.Key == "track")
                            continue;
                        if (item.Value == null)
                            continue;
                        var value = item.Value;
                        if (value.GetType() == typeof(long))
                            value = new EntityTypes.Integer(int.MinValue, int.MaxValue, (int)value);
                        else if (value.GetType() == typeof(double))
                            value = new EntityTypes.Float(float.NegativeInfinity, float.PositiveInfinity, (float)value);
                        parameters.Add(new Minigames.Param(item.Key, value, item.Key.DisplayName(), "[inferred from remix.json]"));
                    }
                    action = new Minigames.GameAction(actionName, actionName.DisplayName(), e.length, true, parameters);
                    game.actions.Add(action);
                }

                //check each param of the action
                if (action.parameters != null)
                {
                    foreach (var param in action.parameters)
                    {
                        type = param.parameter.GetType();
                        //add property if it doesn't exist
                        if (!e.dynamicData.ContainsKey(param.propertyName))
                        {
                            Debug.LogWarning($"Property {param.propertyName} does not exist in the entity's dynamic data! Adding...");
                            if (type == typeof(EntityTypes.Integer))
                                e.dynamicData.Add(param.propertyName, ((EntityTypes.Integer)param.parameter).val);
                            else if (type == typeof(EntityTypes.Float))
                                e.dynamicData.Add(param.propertyName, ((EntityTypes.Float)param.parameter).val);
                            else if (type.IsEnum)
                                e.dynamicData.Add(param.propertyName, (int)param.parameter);
                            else
                                e.dynamicData.Add(param.propertyName, Convert.ChangeType(param.parameter, type));
                            continue;
                        }
                        pType = e[param.propertyName].GetType();
                        if (pType != type)
                        {
                            try
                            {
                                if (type == typeof(EntityTypes.Integer))
                                    e.dynamicData[param.propertyName] = (int)e[param.propertyName];
                                else if (type == typeof(EntityTypes.Float))
                                    e.dynamicData[param.propertyName] = (float)e[param.propertyName];
                                else if (type.IsEnum)
                                {
                                    if (pType == typeof(string))
                                        e.dynamicData[param.propertyName] = (int)Enum.Parse(type, (string)e[param.propertyName]);
                                    else
                                        e.dynamicData[param.propertyName] = (int)e[param.propertyName];
                                }
                                else if (pType == typeof(Newtonsoft.Json.Linq.JObject))
                                    e.dynamicData[param.propertyName] = e[param.propertyName].ToObject(type);
                                else
                                    e.dynamicData[param.propertyName] = Convert.ChangeType(e[param.propertyName], type);
                            }
                            catch
                            {
                                Debug.LogWarning($"Could not convert {param.propertyName} to {type}! Using default value...");
                                // GlobalGameManager.ShowErrorMessage("Warning", $"Could not convert {e.datamodel}/{param.propertyName} to {type}! This will be loaded using the default value, so chart may be unstable.");
                                // use default value
                                if (type == typeof(EntityTypes.Integer))
                                    e.dynamicData[param.propertyName] = ((EntityTypes.Integer)param.parameter).val;
                                else if (type == typeof(EntityTypes.Float))
                                    e.dynamicData[param.propertyName] = ((EntityTypes.Float)param.parameter).val;
                                else if (type.IsEnum && param.propertyName != "ease")
                                    e.dynamicData[param.propertyName] = (int)param.parameter;
                                else
                                    e.dynamicData[param.propertyName] = Convert.ChangeType(param.parameter, type);
                            }
                        }
                    }
                }
            }

            foreach (var tempo in data.tempoChanges)
            {
                PreProcessSpecialEntity(tempo, tempoChangeModel);
            }
            if (data.tempoChanges[0]["tempo"] <= 0)
            {
                data.tempoChanges[0]["tempo"] = 120;
            }

            foreach (var vol in data.volumeChanges)
            {
                PreProcessSpecialEntity(vol, volumeChangeModel);
            }

            foreach (var section in data.beatmapSections)
            {
                PreProcessSpecialEntity(section, sectionMarkModel);
            }

            //go thru each property of the model beatmap and add any missing keyvalue pair
            foreach (var prop in propertiesModel)
            {
                if (!data.properties.ContainsKey(prop.Key))
                {
                    data.properties.Add(prop.Key, prop.Value);
                }
            }

            return data;
        }

        public class Minigame
        {

            public string name;
            public string displayName;
            public string color;
            public string splitColorL;
            public string splitColorR;
            public bool hidden;
            public bool fxOnly;
            public List<GameAction> actions = new List<GameAction>();

            public List<string> tags;
            public string defaultLocale = "en";
            public string wantAssetBundle = "";
            public List<string> supportedLocales;
            public bool inferred;

            public bool usesAssetBundle => wantAssetBundle != "";
            public bool hasLocales => supportedLocales.Count > 0;
            public bool AssetsLoaded => ((hasLocales && localeLoaded && currentLoadedLocale == defaultLocale) || (!hasLocales)) && commonLoaded;
            public bool SequencesPreloaded => soundSequences != null;
            public string LoadableName => inferred ? "noGame" : name;
            public GameObject LoadedPrefab => loadedPrefab;

            private AssetBundle bundleCommon = null;
            private bool commonLoaded = false;
            private bool commonPreloaded = false;
            private string currentLoadedLocale = "";
            private AssetBundle bundleLocalized = null;
            private bool localeLoaded = false;
            private bool localePreloaded = false;
            private GameObject loadedPrefab = null;
            private Dictionary<string, AudioClip> commonAudioClips;
            private Dictionary<string, AudioClip> localeAudioClips;

            private SoundSequence.SequenceKeyValue[] soundSequences = null;

            public SoundSequence.SequenceKeyValue[] LoadedSoundSequences
            {
                get => soundSequences;
                set => soundSequences = value;
            }
            public Dictionary<string, AudioClip> CommonAudioClips => commonAudioClips;
            public Dictionary<string, AudioClip> LocaleAudioClips => localeAudioClips;

            public Minigame(string name, string displayName, string color, bool hidden, bool fxOnly, List<GameAction> actions, List<string> tags = null, string assetBundle = "", string defaultLocale = "en", List<string> supportedLocales = null, bool inferred = false)
            {
                this.name = name;
                this.displayName = displayName;
                this.color = color;
                this.actions = actions;
                this.hidden = hidden;
                this.fxOnly = fxOnly;

                this.tags = tags ?? new List<string>();
                this.wantAssetBundle = assetBundle;
                this.defaultLocale = defaultLocale;
                this.supportedLocales = supportedLocales ?? new List<string>();
                this.inferred = inferred;

                this.splitColorL = null;
                this.splitColorR = null;
            }

            public Minigame(string name, string displayName, string color, string splitColorL, string splitColorR, bool hidden, bool fxOnly, List<GameAction> actions, List<string> tags = null, string assetBundle = "", string defaultLocale = "en", List<string> supportedLocales = null, bool inferred = false)
            {
                this.name = name;
                this.displayName = displayName;
                this.color = color;
                this.actions = actions;
                this.hidden = hidden;
                this.fxOnly = fxOnly;

                this.tags = tags ?? new List<string>();
                this.wantAssetBundle = assetBundle;
                this.defaultLocale = defaultLocale;
                this.supportedLocales = supportedLocales ?? new List<string>();
                this.inferred = inferred;

                this.splitColorL = splitColorL;
                this.splitColorR = splitColorR;
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

            public async UniTask LoadAssetsAsync()
            {
                if (AssetsLoaded || !usesAssetBundle) return;
                await UniTask.WhenAll(LoadCommonAssetBundleAsync(), LoadLocalizedAssetBundleAsync());
                await UniTask.WhenAll(LoadGamePrefabAsync());
                await UniTask.WhenAll(LoadCommonAudioClips());
                await UniTask.WhenAll(LoadLocalizedAudioClips());
            }

            public async UniTask LoadCommonAssetBundleAsync()
            {
                if (commonPreloaded || commonLoaded) return;
                commonPreloaded = true;
                if (!usesAssetBundle) return;
                if (bundleCommon != null) return;

                AssetBundle bundle = await AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, wantAssetBundle + "/common")).ToUniTask();

                bundleCommon = bundle;
                commonLoaded = true;
            }

            public async UniTask LoadLocalizedAssetBundleAsync()
            {
                if (!hasLocales) return;
                if (localePreloaded) return;
                localePreloaded = true;
                if (!usesAssetBundle) return;
                if (localeLoaded && bundleLocalized != null && currentLoadedLocale == defaultLocale) return;

                AssetBundle bundle = await AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, wantAssetBundle + "/locale." + defaultLocale)).ToUniTask();
                if (localeLoaded && bundleLocalized != null && currentLoadedLocale == defaultLocale) return;

                bundleLocalized = bundle;
                currentLoadedLocale = defaultLocale;
                localeLoaded = true;
            }

            public async UniTask LoadGamePrefabAsync()
            {
                if (!usesAssetBundle) return;
                if (!commonLoaded) return;
                if (bundleCommon == null) return;

                UnityEngine.Object asset = await bundleCommon.LoadAssetAsync<GameObject>(name).ToUniTask();
                loadedPrefab = asset as GameObject;

                // load sound sequences here for now
                // this is taxing and is still done synchronously
                // move sequences to their own assets so that we don't have to look up a component
                if (loadedPrefab.TryGetComponent<Games.Minigame>(out Games.Minigame minigame))
                {
                    soundSequences = minigame.SoundSequences;
                }
            }

            public async UniTask LoadCommonAudioClips()
            {
                if (!commonLoaded) return;
                if (bundleCommon == null) return;

                commonAudioClips = new();

                var assets = bundleCommon.LoadAllAssetsAsync();
                await assets;

                // await UniTask.SwitchToThreadPool();
                // foreach (var asset in assets.allAssets)
                // {
                //     AudioClip clip = asset as AudioClip;
                //     commonAudioClips.Add(clip.name, clip);
                // }
                // await UniTask.SwitchToMainThread();
            }

            public async UniTask LoadLocalizedAudioClips()
            {
                if (!localeLoaded) return;
                if (bundleLocalized == null) return;

                localeAudioClips = new();

                var assets = bundleLocalized.LoadAllAssetsAsync();
                await assets;

                // await UniTask.SwitchToThreadPool();
                // foreach (var asset in assets.allAssets)
                // {
                //     AudioClip clip = asset as AudioClip;
                //     localeAudioClips.Add(clip.name, clip);
                // }
                // await UniTask.SwitchToMainThread();
            }

            public async UniTask UnloadAllAssets()
            {
                if (!usesAssetBundle) return;
                commonAudioClips.Clear();
                localeAudioClips.Clear();
                if (loadedPrefab != null)
                {
                    loadedPrefab = null;
                }
                if (bundleCommon != null)
                {
                    await bundleCommon.UnloadAsync(true);
                    bundleCommon = null;
                    commonLoaded = false;
                    commonPreloaded = false;
                }
                if (bundleLocalized != null)
                {
                    await bundleLocalized.UnloadAsync(true);
                    bundleLocalized = null;
                    localeLoaded = false;
                    localePreloaded = false;
                }
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
            public float preFunctionLength = 2.0f;

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
            public GameAction(string actionName, string displayName, float defaultLength = 1, bool resizable = false, List<Param> parameters = null, EventCallback function = null, EventCallback inactiveFunction = null, EventCallback prescheduleFunction = null, bool hidden = false, EventCallback preFunction = null, int priority = 0, float preFunctionLength = 2.0f)
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
                this.preFunctionLength = preFunctionLength;


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
            public List<CollapseParam> collapseParams;

            /// <summary>
            /// A parameter that changes the function of a GameAction.
            /// </summary>
            /// <param name="propertyName">The name of the variable that's being changed.</param>
            /// <param name="parameter">The value of the parameter</param>
            /// <param name="propertyCaption">The name shown in the editor. Can be anything you want.</param>
            public Param(string propertyName, object parameter, string propertyCaption, string tooltip = "", List<CollapseParam> collapseParams = null)
            {
                this.propertyName = propertyName;
                this.parameter = parameter;
                this.propertyCaption = propertyCaption;
                this.tooltip = tooltip;
                this.collapseParams = collapseParams;
            }

            public class CollapseParam
            {
                public Func<object, RiqEntity, bool> CollapseOn;
                public string[] collapseables;
                /// <summary>
                /// Class that decides how other parameters will be collapsed
                /// </summary>
                /// <param name="collapseOn">What values should make it collapse/uncollapse?</param>
                /// <param name="collapseables">IDs of the parameters to collapse</param>
                public CollapseParam(Func<object, RiqEntity, bool> collapseOn, string[] collapseables)
                {
                    CollapseOn = collapseOn;
                    this.collapseables = collapseables;
                }
            }
        }

        public delegate void EventCallback();
        public delegate void ParamChangeCallback(string paramName, object paramValue, RiqEntity entity);

        // overengineered af but it's a modified version of
        // https://stackoverflow.com/a/19877141
        static List<Func<EventCaller, Minigame>> loadRunners;
        static void BuildLoadRunnerList()
        {
            loadRunners = System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.Namespace == "HeavenStudio.Games.Loaders" && x.GetMethod("AddGame", BindingFlags.Public | BindingFlags.Static) != null)
            .Select(t => (Func<EventCaller, Minigame>)Delegate.CreateDelegate(
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
                        function: delegate { var e = eventCaller.currentEntity; GameManager.instance.SwitchGame(eventCaller.currentSwitchGame, eventCaller.currentEntity.beat, e["toggle"]); },
                        parameters: new List<Param>()
                            {
                                new Param("toggle", true, "Black Flash", "Enable or disable the black screen for this Game Switch")
                            },
                        inactiveFunction: delegate { var e = eventCaller.currentEntity; GameManager.instance.SwitchGame(eventCaller.currentSwitchGame, eventCaller.currentEntity.beat, e["toggle"]); }
                    ),
                    new GameAction("end", "End Remix",
                        function: delegate {
                            Debug.Log("end");
                            if (Timeline.instance != null)
                                Timeline.instance?.Stop(Timeline.instance.PlaybackBeat);
                            else
                                GameManager.instance.Stop(0);
                        }
                    ),
                    new GameAction("skill star", "Skill Star", 1f, true)
                    {
                        //temp for testing
                        function = delegate {
                            var e = eventCaller.currentEntity;
                            Common.SkillStarManager.instance.DoStarIn(e.beat, e.length); 
                        }
                    },
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
                            new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
                        },
                        hidden: true
                    ),
                    new GameAction("move camera", "", 1f, true, new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Float(-50, 50, 0), "Right / Left"),
                        new Param("valB", new EntityTypes.Float(-50, 50, 0), "Up / Down"),
                        new Param("valC", new EntityTypes.Float(-0, 250, 10), "In / Out"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease Type")
                    },
                    hidden: true ),
                    new GameAction("rotate camera", "", 1f, true, new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Pitch"),
                        new Param("valB", new EntityTypes.Integer(-360, 360, 0), "Yaw"),
                        new Param("valC", new EntityTypes.Integer(-360, 360, 0), "Roll"),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease Type")
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
                            new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
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
                            new Param("valA", new EntityTypes.Float(-50, 50, 0), "Right / Left", "Next position on the X axis"),
                            new Param("valB", new EntityTypes.Float(-50, 50, 0), "Up / Down", "Next position on the Y axis"),
                            new Param("valC", new EntityTypes.Float(-0, 250, 10), "In / Out", "Next position on the Z axis"),
                            new Param("ease", Util.EasingFunction.Ease.Linear, "Ease Type"),
                            new Param("axis", GameCamera.CameraAxis.All, "Axis", "The axis to move the camera on" )
                        }
                    ),
                    new GameAction("rotate camera", "Rotate Camera", 1f, true, new List<Param>()
                        {
                            new Param("valA", new EntityTypes.Integer(-360, 360, 0), "Pitch", "Next rotation on the X axis"),
                            new Param("valB", new EntityTypes.Integer(-360, 360, 0), "Yaw", "Next rotation on the Y axis"),
                            new Param("valC", new EntityTypes.Integer(-360, 360, 0), "Roll", "Next rotation on the Z axis"),
                            new Param("ease", Util.EasingFunction.Ease.Linear, "Ease Type"),
                            new Param("axis", GameCamera.CameraAxis.All, "Axis", "The axis to move the camera on" )
                        }
                    ),
                    new GameAction("camera background color", "Camera Background Color", 1, true, new List<Param>()
                        {
                            new Param("color", Color.black, "Start Color"),
                            new Param("color2", Color.black, "End Color"),
                            new Param("ease", Util.EasingFunction.Ease.Linear, "Ease Type")
                        }
                    ),
                    new GameAction("pan view", "Pan Viewport", 1f, true, new List<Param>()
                        {
                            new Param("valA", new EntityTypes.Float(-50, 50, 0), "Right / Left", "Next position on the X axis"),
                            new Param("valB", new EntityTypes.Float(-50, 50, 0), "Up / Down", "Next position on the Y axis"),
                            new Param("ease", Util.EasingFunction.Ease.Linear, "Ease Type"),
                            new Param("axis", StaticCamera.ViewAxis.All, "Axis", "The axis to pan the viewport in" )
                        }
                    ),
                    new GameAction("rotate view", "Rotate Viewport", 1f, true, new List<Param>()
                        {
                            new Param("valA", new EntityTypes.Float(-360, 360, 0), "Rotation", "Next viewport rotation"),
                            new Param("ease", Util.EasingFunction.Ease.Linear, "Ease Type"),
                        }
                    ),
                    new GameAction("scale view", "Scale Viewport", 1f, true, new List<Param>()
                        {
                            new Param("valA", new EntityTypes.Float(0, 50, 1), "Width", "Next viewport width"),
                            new Param("valB", new EntityTypes.Float(0, 50, 1), "Height", "Next viewport height"),
                            new Param("ease", Util.EasingFunction.Ease.Linear, "Ease Type"),
                            new Param("axis", StaticCamera.ViewAxis.All, "Axis", "The axis to scale the viewport in" )
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
                            new Param("instantOn", false, "Instant Show", "Skip the show animation?"),
                            new Param("instantOff", false, "Instant Hide", "Skip the hide animation?"),
                        }
                    ),
                }),
            };

            BuildLoadRunnerList();
            foreach (var load in loadRunners)
            {
                Debug.Log("Running game loader " + RuntimeReflectionExtensions.GetMethodInfo(load).DeclaringType.Name);
                eventCaller.minigames.Add(load(eventCaller));
            }
        }
    }
}