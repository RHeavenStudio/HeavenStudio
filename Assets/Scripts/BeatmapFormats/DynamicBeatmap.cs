using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using HeavenStudio.Util;

namespace HeavenStudio
{
    [Serializable]
    public class DynamicBeatmap
    {
        public static int CurrentRiqVersion = 0;
        public float bpm;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(100)] public int musicVolume; // In percent (1-100)
        
        public Dictionary<string, object> properties = 
            new Dictionary<string, object>() {
                // software version (MajorMinorPatch, revision)
                {"productversion", 000},
                {"productsubversion", 0},
                // file format version
                {"riqversion", CurrentRiqVersion},
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

        public List<DynamicEntity> entities = new List<DynamicEntity>();
        public List<TempoChange> tempoChanges = new List<TempoChange>();
        public List<VolumeChange> volumeChanges = new List<VolumeChange>();
        public List<ChartSection> beatmapSections = new List<ChartSection>();
        public float firstBeatOffset;

        [Serializable]
        public class DynamicEntity : ICloneable
        {
            public float beat;
            public int track;
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public float length;
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public float swing;
            public Dictionary<string, dynamic> DynamicData = new Dictionary<string, dynamic>();

            public string datamodel;
            [JsonIgnore] public Editor.Track.TimelineEventObj eventObj;

            public object Clone()
            {
                return this.MemberwiseClone();
            }

            public DynamicEntity DeepCopy()
            {
                DynamicEntity copy = (DynamicEntity)this.MemberwiseClone();
                copy.DynamicData = new Dictionary<string, dynamic>(this.DynamicData);
                return copy;
            }

            public dynamic this[string propertyName]
            {
                get
                {
                    switch (propertyName)
                    {
                        case "beat":
                            return beat;
                        case "track":
                            return track;
                        case "length":
                            return length;
                        case "swing":
                            return swing;
                        case "datamodel":
                            return datamodel;
                        default:
                            if (DynamicData.ContainsKey(propertyName))
                                return DynamicData[propertyName];
                            else
                            {
                                Minigames.Minigame game = EventCaller.instance.GetMinigame(datamodel.Split(0));
                                Minigames.Param param = EventCaller.instance.GetGameParam(game, datamodel.Split(1), propertyName);
                                return param.parameter;
                            }
                    }
                }
                set
                {
                    switch (propertyName)
                    {
                        case "beat":
                        case "track":
                        case "length":
                        case "swing":
                        case "datamodel":
                            UnityEngine.Debug.LogWarning($"Property name {propertyName} is reserved and cannot be set.");
                            break;
                        default:
                            if (DynamicData.ContainsKey(propertyName))
                                DynamicData[propertyName] = value;
                            else
                                UnityEngine.Debug.LogError($"This entity does not have a property named {propertyName}! Attempted to insert value of type {value.GetType()}");
                            break;
                    }
                    
                }
            }

            public void CreateProperty(string name, dynamic defaultValue)
            {
                if (!DynamicData.ContainsKey(name))
                    DynamicData.Add(name, defaultValue);
            }
        }

        [Serializable]
        public class TempoChange : ICloneable
        {
            public float beat;
            public float length;
            public float tempo;

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        [Serializable]
        public class VolumeChange : ICloneable
        {
            public float beat;
            public float length;
            public float volume;

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        [Serializable]
        public class ChartSection : ICloneable
        {
            public float beat;
            public bool startPerfect;
            public string sectionName;
            public bool isCheckpoint;   // really don't think we need this but who knows

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        public dynamic this[string propertyName]
        {
            get
            {
                return properties[propertyName] ?? null;
            }
            set
            {
                if (properties.ContainsKey(propertyName))
                {
                    properties[propertyName] = value;
                }
                else
                {
                    UnityEngine.Debug.LogError($"This beatmap does not have a property named {propertyName}! Attempted to insert value of type {value.GetType()}");
                }
            }
        }

        /// <summary>
        /// converts from the old "rhmania" / "tengoku" format to the new "riq" format
        /// </summary>
        /// <param name="beatmap">a deserialized .rhmania or .tengoku beatmap</param>
        /// <returns>a .riq beatmap</returns>
        public static DynamicBeatmap BeatmapConverter(Beatmap beatmap)
        {
            DynamicBeatmap dynamicBeatmap = new DynamicBeatmap();
            dynamicBeatmap.bpm = beatmap.bpm;
            dynamicBeatmap.musicVolume = beatmap.musicVolume;
            dynamicBeatmap.firstBeatOffset = beatmap.firstBeatOffset;

            Minigames.Minigame game;
            Minigames.GameAction action;
            System.Type type, pType;
            foreach (var e in beatmap.entities)
            {
                game = EventCaller.instance.GetMinigame(e.datamodel.Split(0));
                action = EventCaller.instance.GetGameAction(game, e.datamodel.Split(1));

                if (game == null || action == null)
                {
                    //FUTURE: attempt to convert to a new entity if a converter exists for this datamodel
                    UnityEngine.Debug.LogError($"Could not find game or gameaction from datamodel {e.datamodel} @ beat {e.beat}, skipping entity");
                    continue;
                }
                // Debug.Log($"{game.name} {action.displayName} @ beat {e.beat}");

                Dictionary<string, dynamic> dynamicData = new Dictionary<string, dynamic>();
                //check each param of the action
                if (action.parameters != null)
                {
                    foreach (var param in action.parameters)
                    {
                        type = param.parameter.GetType();
                        pType = e[param.propertyName].GetType();
                        // Debug.Log($"adding parameter {param.propertyName} of type {type}");
                        if (!dynamicData.ContainsKey(param.propertyName))
                        {
                            if (pType == type)
                            {
                                dynamicData.Add(param.propertyName, e[param.propertyName]);
                            }
                            else
                            {
                                if (type == typeof(EntityTypes.Integer))
                                    dynamicData.Add(param.propertyName, (int) e[param.propertyName]);
                                else if (type == typeof(EntityTypes.Float))
                                    dynamicData.Add(param.propertyName, (float) e[param.propertyName]);
                                else if (type.IsEnum && param.propertyName != "ease")
                                    dynamicData.Add(param.propertyName, (int) e[param.propertyName]);
                                else if (pType == typeof(Newtonsoft.Json.Linq.JObject))
                                    dynamicData.Add(param.propertyName, e[param.propertyName].ToObject(type));
                                else
                                    dynamicData.Add(param.propertyName, Convert.ChangeType(e[param.propertyName], type));
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Property {param.propertyName} already exists in the entity's dynamic data! Skipping...");
                        }
                    }
                }

                dynamicBeatmap.entities.Add(new DynamicEntity()
                {
                    beat = e.beat,
                    track = e.track,
                    length = e.length,
                    swing = e.swing,
                    datamodel = e.datamodel,
                    DynamicData = dynamicData
                });
            }
            foreach (var tempoChange in beatmap.tempoChanges)
            {
                dynamicBeatmap.tempoChanges.Add(new TempoChange()
                {
                    beat = tempoChange.beat,
                    length = tempoChange.length,
                    tempo = tempoChange.tempo
                });
            }
            foreach (var volumeChange in beatmap.volumeChanges)
            {
                dynamicBeatmap.volumeChanges.Add(new VolumeChange()
                {
                    beat = volumeChange.beat,
                    length = volumeChange.length,
                    volume = volumeChange.volume
                });
            }
            return dynamicBeatmap;
        }

        /// <summary>
        /// FUTURE: converts from a karateka mania chart ("bor") to the "riq" format
        /// </summary>
        /// <param name="bor">a rawtext .bor chart</param>
        /// <returns>a .riq beatmap</returns>
        /// <remarks>not implemented yet</remarks>
        public static DynamicBeatmap KManiaBorConverter(String bor)
        {
            return null;
        }

        /// <summary>
        /// updates an "riq" beatmap
        /// </summary>
        /// <param name="beatmap">old beatmap</param>
        /// <param name="version">version of old beatmap</param>
        /// <returns>updated beatmap</returns>
        /// <remarks>not implemented yet</remarks>
        public static DynamicBeatmap BeatmapUpdater(DynamicBeatmap beatmap, int version)
        {
            return beatmap;
        }

        /// <summary>
        /// processes an riq beatmap after it is loaded
        /// </summary>
        public void PostProcess()
        {
            DynamicBeatmap beatmapModel = new DynamicBeatmap();
            Minigames.Minigame game;
            Minigames.GameAction action;
            System.Type type, pType;
            foreach (var e in entities)
            {
                var gameName = e.datamodel.Split(0);
                var actionName = e.datamodel.Split(1);
                game = EventCaller.instance.GetMinigame(gameName);
                if (game == null)
                {
                    Debug.LogWarning($"Unknown game {gameName} found in remix.json! Adding game...");
                    game = new Minigames.Minigame(gameName, DisplayName(gameName) + " \n<color=#eb5454>[inferred from remix.json]</color>", "", false, true, new List<Minigames.GameAction>());
                    EventCaller.instance.minigames.Add(game);
                    Editor.Editor.instance.AddIcon(game);
                }
                action = EventCaller.instance.GetGameAction(game, actionName);
                if (action == null)
                {
                    Debug.LogWarning($"Unknown action {gameName}/{actionName} found in remix.json! Adding action...");
                    var parameters = new List<Minigames.Param>();
                    foreach (var item in e.DynamicData)
                    {
                        var value = item.Value;
                        if (value.GetType() == typeof(long))
                            value = new EntityTypes.Integer(int.MinValue, int.MaxValue, (int)value);
                        else if (value.GetType() == typeof(double))
                            value = new EntityTypes.Float(float.NegativeInfinity, float.PositiveInfinity, (float)value);
                        parameters.Add(new Minigames.Param(item.Key, value, item.Key, "[inferred from remix.json]"));
                    }
                    action = new Minigames.GameAction(actionName, DisplayName(actionName), e.length, true, parameters);
                    game.actions.Add(action);
                }
                Dictionary<string, dynamic> dynamicData = new Dictionary<string, dynamic>();
                //check each param of the action
                if (action.parameters != null)
                {
                    foreach (var param in action.parameters)
                    {
                        if (!dynamicData.ContainsKey(param.propertyName))
                        {
                            type = param.parameter.GetType();
                            //FUTURE: attempt to convert to a new entity if a converter exists for this datamodel
                            //add property if it doesn't exist
                            if (!e.DynamicData.ContainsKey(param.propertyName))
                            {
                                Debug.LogWarning($"Property {param.propertyName} does not exist in the entity's dynamic data! Adding...");
                                if (type == typeof(EntityTypes.Integer))
                                    dynamicData.Add(param.propertyName, ((EntityTypes.Integer)param.parameter).val);
                                else if (type == typeof(EntityTypes.Float))
                                    dynamicData.Add(param.propertyName, ((EntityTypes.Float)param.parameter).val);
                                else if (type.IsEnum && param.propertyName != "ease")
                                    dynamicData.Add(param.propertyName, (int)param.parameter);
                                else
                                    dynamicData.Add(param.propertyName, Convert.ChangeType(param.parameter, type));
                                continue;
                            }
                            pType = e[param.propertyName].GetType();
                            if (pType == type)
                            {
                                dynamicData.Add(param.propertyName, e[param.propertyName]);
                            }
                            else
                            {
                                if (type == typeof(EntityTypes.Integer))
                                    dynamicData.Add(param.propertyName, (int)e[param.propertyName]);
                                else if (type == typeof(EntityTypes.Float))
                                    dynamicData.Add(param.propertyName, (float)e[param.propertyName]);
                                else if (type == typeof(EasingFunction.Ease) && pType == typeof(string))
                                    dynamicData.Add(param.propertyName, Enum.Parse(typeof(EasingFunction.Ease), (string)e[param.propertyName]));
                                else if (type.IsEnum)
                                    dynamicData.Add(param.propertyName, (int)e[param.propertyName]);
                                else if (pType == typeof(Newtonsoft.Json.Linq.JObject))
                                    dynamicData.Add(param.propertyName, e[param.propertyName].ToObject(type));
                                else
                                    dynamicData.Add(param.propertyName, Convert.ChangeType(e[param.propertyName], type));
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Property {param.propertyName} already exists in the entity's dynamic data! Skipping...");
                        }
                    }
                }
                e.DynamicData = dynamicData;
            }
            //go thru each property of the model beatmap and add any missing keyvalue pair
            foreach (var prop in beatmapModel.properties)
            {
                if (!properties.ContainsKey(prop.Key))
                {
                    properties.Add(prop.Key, prop.Value);
                }
            }
        }

        private string DisplayName(string name)
        {
            // "gameName" -> "Game Name"
            // "action name" -> "Action Name"
            if (!name.Contains(" "))
                name = SplitCamelCase(name);
            System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(name);
        }

        // https://stackoverflow.com/a/5796793
        public static string SplitCamelCase(string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }
    }
}