using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using Jukebox;
using System.Linq;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlNewKarateLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            RiqEntity WarningUpdater(string datamodel, RiqEntity e)
            {
                if (datamodel == "karateman/hitX")
                {
                    if (e["type"] == null) return null;
                    
                    int newWarning = (int)e["type"];
                    newWarning = (e["type"] < 7) ? newWarning + 1 : 0;

                    e.CreateProperty("whichWarning", newWarning);
                    e.CreateProperty("pitchVoice", false);
                    e.CreateProperty("forcePitch", 1);
                    e.CreateProperty("customLength", false);
                    e.CreateProperty("cutOut", true);

                    e.dynamicData.Remove("type");

                    e.datamodel = "karateman/warnings";
                    return e;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += WarningUpdater;

            RiqEntity BackgroundUpdater(string datamodel, RiqEntity e)
            {
                if (e.datamodel == "karateman/set background effects")
                {
                    var toRemove = e.dynamicData.Keys.Where(x => x != "track").ToList();

                    bool fade = e["type3"] == 3;
                    e.CreateProperty("presetBg",     (int)e["type"]);
                    e.CreateProperty("startColor",   e["colorA"]);
                    e.CreateProperty("shadowType",   (int)e["type2"]);
                    e.CreateProperty("shadowStart",  e["colorB"]);
                    e.CreateProperty("shadowEnd",    e["colorB"]);
                    e.CreateProperty("textureType",  (int)e["type4"]);
                    e.CreateProperty("autoColor",    e["type5"] == (int)KarateMan.ShadowType.Tinted);
                    e.CreateProperty("startTexture", e["colorC"]);
                    e.CreateProperty("endTexture",   e["colorC"]);
                    e.CreateProperty("endColor",     fade ? e["colorD"] : e["colorA"]);
                    e.CreateProperty("ease",         fade ? (int)Util.EasingFunction.Ease.Linear : (int)Util.EasingFunction.Ease.Instant);
                    e.CreateProperty("fxType",       !fade ? (int)e["type3"] : 3);

                    foreach (var remove in toRemove) {
                        e.dynamicData.Remove(remove);
                    }

                    e.datamodel = "karateman/background appearance";

                    return e;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += BackgroundUpdater;

            // RiqEntity GameCapitalizer(string datamodel, RiqEntity entity)
            // {
            //     if (datamodel.Split('/')[0] == "karateman")
            //     {
            //         string name = datamodel.Split('/')[1];
            //         entity.datamodel = "karateman/" + name;
            //         var tempData = entity.dynamicData.ToDictionary(x => x.Key);
            //         foreach ((string key, dynamic item) in tempData)
            //         {
            //             if (item.GetType() == typeof(Newtonsoft.Json.Linq.JObject)) {
            //                 entity.dynamicData[key] = new Color((float)item["r"], (float)item["g"], (float)item["b"]);
            //             }
            //             Debug.Log(key + ", " + item);
            //         }
            //         entity.version = 1;

            //         return entity;
            //     } else if (datamodel == "gameManager/switchGame/karateman") {
            //         entity.datamodel = "gameManager/switchGame/karateman";
            //         return entity;
            //     }
            //     return null;
            // }
            // RiqBeatmap.OnUpdateEntity += GameCapitalizer;

            return new Minigame("karateman", "Karate Man", "fbca3e", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.ToggleBop(e.beat, e.length, e["toggle2"], e["toggle"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle2", true, "Bop", "Whether to bop to the beat or not"),
                        new Param("toggle", false, "Bop (Auto)", "Whether to auto bop to the beat or not")
                    },
                },
                new GameAction("hit", "Toss Object") {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.CreateItem(e.beat, e["type"], e["type2"]);
                        KarateMan.CreateItemSFX(e.beat, e["type"], e["mute"]);
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.QueueCue(e);
                        KarateMan.CreateItemSFX(e.beat, e["type"], e["mute"]);
                    },
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.HitType.Pot, "Object", "The object to fire"),
                        new Param("type2", KarateMan.KarateManFaces.Normal, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("mute", false, "Mute", "Should the throwing sound be muted?"),
                    }
                },
                new GameAction("bulb", "Toss Lightbulb")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.CreateBulbSpecial(e.beat, e["type"], e["colorA"], e["type2"], e["sfx"], e["hitSfx"]);
                        if (!e["mute"]) KarateMan.CreateBulbSFX(e.beat, e["type"], e["sfx"], e["throwSfx"]);
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.QueueCue(e);
                        if (!e["mute"]) KarateMan.CreateBulbSFX(e.beat, e["type"], e["sfx"], e["throwSfx"]);
                    },
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.LightBulbType.Normal, "Type", "The preset bulb type. Yellow is used for kicks while Blue is used for combos", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)KarateMan.LightBulbType.Custom, new string[] { "colorA" })
                        }),
                        new Param("colorA", new Color(1f,1f,1f), "Custom Color", "The color to use when the bulb type is set to Custom"),
                        new Param("type2", KarateMan.KarateManFaces.Normal, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("mute", false, "Mute", "Should the throwing sound be muted?"),
                        new Param("sfx", KarateMan.LightBulbSfx.Automatic, "SFX", "What type of SFX to use for the bulb?", new List<Param.CollapseParam>() 
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)KarateMan.LightBulbSfx.Custom, new string[] { "throwSfx", "hitSfx" }),
                        }),
                        new Param("throwSfx", "lightbulbOut", "Throw SFX", "Custom throw SFX to use for the bulb"),
                        new Param("hitSfx", "lightbulbHit", "Hit SFX", "Custom hit SFX to use for the bulb"),
                    },
                },
                new GameAction("kick", "Special: Kick")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.Kick(e.beat, e["toggle"], e["shouldGlow"], e["type"], e["pitchVoice"], e["forcePitch"], e["cutOut"], e["disableVoice"], e["woodColor"], e["hoopColor"]);
                        KarateMan.KickSFX();
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.QueueCue(e);
                        KarateMan.KickSFX();
                    },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Contains Ball", "Barrel contains a ball instead of a bomb?"),
                        new Param("shouldGlow", true, "Bomb Glow", "Should Joe be lit up by the bomb in the barrel?"),
                        new Param("type", KarateMan.KarateManFaces.Smirk, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("pitchVoice", false, "Pitch Voice", "Pitch the voice of this cue?", new List<Param.CollapseParam>() 
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "forcePitch" }),
                        }),
                        new Param("forcePitch", new EntityTypes.Float(0.5f, 2f, 1f), "Force Pitch", "Override the automatic pitching if not set to 1"),
                        new Param("cutOut", true, "Cut Out Voice", "Will this cue be cut out by another voice?"),
                        new Param("disableVoice", false, "Disable Voice", "When enabled, there will be no voice during this cue"),
                        new Param("woodColor", new Color(0.451f, 0.302f, 0.271f), "Barrel Wood Color", "Color of the barrel's wood"),
                        new Param("hoopColor", new Color(0.714f, 0.309f, 0.424f), "Barrel Hoop Color", "Color of the barrel's hoops"),
                    }
                },
                new GameAction("combo", "Special: Combo")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.Combo(e.beat, e["type"], e["pitchVoice"], e["forcePitch"], e["cutOut"], e["disableVoice"]);
                        KarateMan.ComboSFX();
                    }, 
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.QueueCue(e);
                        KarateMan.ComboSFX();
                    },
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.KarateManFaces.Happy, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("pitchVoice", false, "Pitch Voice", "Pitch the voice of this cue?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "forcePitch" }),
                        }),
                        new Param("forcePitch", new EntityTypes.Float(0.5f, 2f, 1f), "Force Pitch", "Override the automatic pitching if not set to 1"),
                        new Param("cutOut", true, "Cut Out Voice", "Will this cue be cut out by another voice?"),
                        new Param("disableVoice", false, "Disable Voice", "When enabled, there will be no voice during this cue"),
                    }
                },
                new GameAction("warnings", "Warnings")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.DoWord(e.beat, e.length, e["whichWarning"], e["pitchVoice"], e["forcePitch"], e["customLength"]);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("whichWarning", KarateMan.HitThree.HitThree, "Which Warning", "The warning text to show and the sfx to play"),
                        new Param("pitchVoice", false, "Auto Pitch Voice", "Pitch the voice of this cue depending on the BPM", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "forcePitch" }),
                        }),
                        new Param("forcePitch", new EntityTypes.Float(0.5f, 2f, 1f), "Force Pitch", "Pitch the voice of this cue depending on the value"),
                        new Param("customLength", false, "Custom Length", "Have the warning text appear for the length of the block"),
                        new Param("cutOut", true, "Cut Out Voice", "Will this cue be cut out by another voice?"),
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.DoWordSound(e.beat, e.length, e["whichWarning"], e["pitchVoice"], e["forcePitch"], e["customLength"]);
                    }
                },
                new GameAction("special camera", "Special Camera")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.DoSpecialCamera(e.beat, e.length, e["toggle"]); },
                    defaultLength = 8f, 
                    resizable = true, 
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Return Camera", "Camera zooms back in?"),
                    },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; KarateMan.DoSpecialCamera(e.beat, e.length, e["toggle"]); }
                },
                new GameAction("prepare", "Preparation Stance")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.Prepare(e.beat, e.length);}, 
                    resizable = true,
                },
                new GameAction("set gameplay modifiers", "Flow/Gameplay Modifiers")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetGameplayMods(e.beat, e["fxType"], e["type"], e["toggle"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("fxType", KarateMan.BackgroundFXType.None, "FX Type", "The background effect to be displayed"),
                        new Param("type", KarateMan.NoriMode.None, "Flow Bar type", "The type of Flow bar to use", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)KarateMan.NoriMode.None, new string[] { "startColor" })
                        }),
                        new Param("hitsPerHeart", new EntityTypes.Float(0f, 20f, 0f), "Hits Per Heart", "How many hits will it take for each heart to light up? (0 will do it automatically.)"),
                        new Param("toggle", true, "Enable Combos", "Allow the player to combo? (Contextual combos will still be allowed even when off)"),
                        //new Param("toggle2", true, "Enable Kicks", "Allow the player to kick? (Contextual kicks will still be allowed even when off)"),
                    },
                },
                new GameAction("background appearance", "Background Appearance")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.BackgroundColor(
                            e.beat, e.length,
                            e["presetBg"], e["startColor"], e["endColor"], e["ease"],
                            e["shadowType"], e["shadowStart"], e["shadowEnd"],
                            e["textureType"], e["autoColor"], e["startTexture"], e["endTexture"]
                        );
                        // backwards compatibility
                        if (e["fxType"] != 3) KarateMan.instance.currentBgEffect = e["fxType"];
                    },
                    defaultLength = 0.5f,
                    resizable = true,
                    parameters = new List<Param>() // uncomment these collapses when overlapping collapses are implemented
                    {
                        new Param("presetBg", KarateMan.BackgroundType.Yellow, "Preset BG Color", "The preset background type (will by default fade from the existing background color)", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)KarateMan.BackgroundType.Custom, new string[] { "startColor", "endColor" }),
                        }),
                        new Param("startColor", new Color(0.985f, 0.79f, 0.243f), "Start BG Color", "The background color to start with"),
                        new Param("endColor", new Color(0.985f, 0.79f, 0.243f), "End BG Color", "The background color to end with"),
                        new Param("ease", Util.EasingFunction.Ease.Instant, "BG Color Ease", "Ease to use when fading color"),
                        new Param("shadowType", KarateMan.ShadowType.Tinted, "Shadow Type", "The shadow type. If Tinted doesn't work with your background color try Custom", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x == (int)KarateMan.ShadowType.Custom, new string[] { "shadowStart", "shadowEnd" }),
                        }),
                        new Param("shadowStart", new Color(), "Start Shadow Color", "The shadow color to start with"),
                        new Param("shadowEnd", new Color(), "End Shadow Color", "The shadow color to end with"),
                        
                        new Param("textureType", KarateMan.BackgroundTextureType.Plain, "Texture", "The type of background texture to use", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => (int)x != (int)KarateMan.BackgroundTextureType.Plain, new string[] { "startTexture", "endTexture" })
                        }),
                        new Param("autoColor", true, "Use BG Color For Texture", "Use a tint of the background color for the texture?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "startTexture", "endTexture" })
                        }),
                        new Param("startTexture", new Color(), "Start Texture Color", "The texture color to start with"),
                        new Param("endTexture", new Color(), "End Texture Color", "The texture color to end with"),
                        new Param("fxType", new EntityTypes.Integer(0, 3, 3), "Check Tooltip", "Ping @AstrlJelly on discord if you see this; it should be hidden.", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => false, new string[] { "fxType" })
                        }),
                    },
                },
                // new GameAction("set background effects", "Background Appearance (OLD)")
                // {
                //     function = delegate {
                //         var e = eventCaller.currentEntity;
                //         KarateMan.instance.SetBgAndShadowCol(e.beat, e.length, e["type"], e["type2"], e["colorA"], e["colorB"], e["type3"]);
                //         KarateMan.instance.SetBgFx(e["type4"], e["type5"], e["colorC"], e["colorD"]);
                //     }, 
                //     defaultLength = 0.5f, 
                //     resizable = true, 
                //     parameters = new List<Param>()
                //     {
                //         new Param("type", KarateMan.BackgroundType.Yellow, "Background Type", "The preset background type"),
                //         new Param("type2", KarateMan.ShadowType.Tinted, "Shadow Type", "The shadow type. If Tinted doesn't work with your background color try Custom"),
                //         new Param("colorA", new Color(), "Custom Background Color", "The background color to use when background type is set to Custom"),
                //         new Param("colorB", new Color(), "Custom Shadow Color", "The shadow color to use when shadow type is set to Custom. When fading the background colour shadows fade to this color"),
                //         new Param("type3", KarateMan.BackgroundFXType.None, "FX Type", "The background effect to be displayed. Fade uses the entity length to determine colour fading speed"),
                //         new Param("type4", KarateMan.BackgroundTextureType.Plain, "Texture", "The type of background texture to use"),
                //         new Param("type5", KarateMan.ShadowType.Tinted, "Color Filter Type", "The method used to apply colour to the texture"),
                //         new Param("colorC", new Color(), "Custom Filter Color", "The filter color to use when color filter type is set to Custom"),
                //         new Param("colorD", new Color(), "Fading Filter Color", "When using the Fade background effect, make filter colour fade to this colour"),
                //     },
                // },
                new GameAction("set object colors", "Object Colors")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.UpdateMaterialColour(e["colorA"], e["colorB"], e["colorC"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", new Color(1,1,1,1), "Joe Body Color", "The color to use for Karate Joe's body"),
                        new Param("colorB", new Color(0.81f,0.81f,0.81f,1), "Joe Highlight Color", "The color to use for Karate Joe's highlights"),
                        new Param("colorC", new Color(1,1,1,1), "Item Color", "The color to use for the thrown items"),
                    },
                },
                new GameAction("particle effects", "Particle Effects")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.SetParticleEffect(e.beat, e["type"], e["instant"], e["valA"], e["valB"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.ParticleType.None, "Particle Type", "The type of particle effect to spawn. Using \"None\" will stop all effects"),
                        new Param("instant", false, "Instant", "Start/Stop particles instantly"),
                        new Param("valA", new EntityTypes.Float(0f, 64f, 1f), "Wind Strength", "The strength of the particle wind"),
                        new Param("valB", new EntityTypes.Float(1f, 16f, 1f), "Particle Intensity", "The intensity of the particle effect")
                    },
                },
                new GameAction("force facial expression", "Set Facial Expression")
                {
                    function = delegate { KarateMan.instance.SetFaceExpression(eventCaller.currentEntity["type"]); }, 
                    defaultLength = 0.5f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.KarateManFaces.Normal, "Facial Expression", "The facial expression to force Joe to. Special moves may override this")
                    }
                },

                // blah blah blah compatibility
                new GameAction("hitX", "Old Warning (you shouldn't see this.)")
                {
                    hidden = true,
                    parameters = new List<Param>(){
                        new Param("type", KarateMan.HitThree.HitThree, "Which Warning", "The warning text to show and the sfx to play"),
                    },
                },
                new GameAction("set background effects", "Background Appearance (OLD)")
                {
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.BackgroundType.Yellow, "Background Type", "The preset background type"),
                        new Param("type2", KarateMan.ShadowType.Tinted, "Shadow Type", "The shadow type. If Tinted doesn't work with your background color try Custom"),
                        new Param("colorA", new Color(), "Custom Background Color", "The background color to use when background type is set to Custom"),
                        new Param("colorB", new Color(), "Custom Shadow Color", "The shadow color to use when shadow type is set to Custom. When fading the background colour shadows fade to this color"),
                        new Param("type3", KarateMan.BackgroundFXType.None, "FX Type", "The background effect to be displayed. Fade uses the entity length to determine colour fading speed"),
                        new Param("type4", KarateMan.BackgroundTextureType.Plain, "Texture", "The type of background texture to use"),
                        new Param("type5", KarateMan.ShadowType.Tinted, "Color Filter Type", "The method used to apply colour to the texture"),
                        new Param("colorC", new Color(), "Custom Filter Color", "The filter color to use when color filter type is set to Custom"),
                        new Param("colorD", new Color(), "Fading Filter Color", "When using the Fade background effect, make filter colour fade to this colour"),
                    },
                },
            },
            new List<string>() {"agb", "ntr", "rvl", "ctr", "pco", "normal"},
            "karate", "en",
            new List<string>() {"en"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_KarateMan;
    public class KarateMan : Minigame
    {

        #region Enums
        public enum HitType
        {
            Pot = 0,
            Lightbulb = 1,
            Rock = 2,
            Ball = 3,
            CookingPot = 6,
            Alien = 7,
            Bomb = 8,
            TacoBell = 999
        }

        public enum HitThree
        {
            HitOne, // 0
            HitTwo, // 1
            HitThree, // 2
            HitThreeAlt, // 3
            HitFour, // 4
            Grr, // 5
            Warning, // 6
            Combo, // 7
        }

        public enum LightBulbType
        {
            Normal,
            Blue,
            Yellow,
            Custom
        }

        public enum LightBulbSfx
        {
            Automatic,
            Megamix,
            DS,
            Custom
        }

        public enum BackgroundType
        {
            Yellow,
            Fuchsia,
            Blue,
            Red,
            Orange,
            Pink,
            Custom
        }

        public enum BackgroundFXType
        {
            None,
            Sunburst,
            Rings
        }

        public enum BackgroundTextureType
        {
            Plain,
            Gradient,
            Radial,
            Blood,
            //ManMan?
        }

        public enum ShadowType
        {
            Tinted,
            Custom
        }

        public enum CameraAngle
        {
            Normal,
            Special
        }

        public enum ParticleType
        {
            None,
            Snow,
            Fire,
            Rain
        }
        
        public enum KarateManFaces
        {
            Normal,
            Smirk,
            Surprise,
            Sad,
            Lenny,
            Happy,
            VerySad,
            Blush
        }

        public enum NoriMode
        {
            None,
            Tengoku,
            Mania,
            ManiaHorizontal,
        }
        #endregion

        static List<RiqEntity> queuedCues = new();
        public static bool IsComboEnable = true; //only stops Out combo inputs, this basically makes combo contextual
        public bool IsNoriActive { get { return Nori.MaxNori > 0; } }
        public float NoriPerformance { get { if (IsNoriActive) return Nori.Nori / Nori.MaxNori; else return 1f; } }

        public Color[] LightBulbColors;
        public Color[] BackgroundColors;

        //camera positions (normal, special)
        [Header("Camera Positions")]
        public Transform[] CameraPosition;
        Vector3 cameraPosition;
        // for future astrl : maybe make these not static? (use a RiqEntity check on game switch)
        static double startCamSpecial = double.MinValue;
        static double wantsReturn = double.MinValue;
        static float cameraReturnLength = 0f;
        static CameraAngle cameraAngle = CameraAngle.Normal;

        //pot trajectory stuff
        [Header("References")]
        public Transform ItemHolder;
        public GameObject Item;
        public KarateManJoe Joe;
        public GameObject NoriGO;
        public KarateManNoriController Nori;

        [Header("Colour Map")]
        public Material MappingMaterial;
        public Color BodyColor = Color.white;
        public Color HighlightColor = new Color(0.81f, 0.81f, 0.81f);
        public Color ItemColor = Color.white;

        [Header("Word")]
        public Animator Word;
        static double wordStartTime = double.MinValue;
        static double wordClearTime = double.MinValue;

        [Header("Backgrounds")]
        // 0 = bg color, 1 = shadow color, 2 = filter color
        private double[] colorStartBeats = new double[3] {
            -1,
            -1,
            -1
        };
        private float[] colorLengths = new float[3];
        private Color[] colorStarts, colorEnds = new Color[3];
        private Util.EasingFunction.Ease[] colorEases = new Util.EasingFunction.Ease[3];

        public int currentBgEffect = (int)BackgroundFXType.None;

        public SpriteRenderer BGPlane;
        public GameObject BGEffect;

        public SpriteRenderer[] BGTextures;

        Animator bgEffectAnimator;
        SpriteRenderer bgEffectSpriteRenderer;

        [Header("Particles")]
        // wind
        public WindZone Wind;
        public ParticleSystem[] Effects;

        [Header("Unloaded Game Calls")]
        //public static Queue<Beatmap.Entity> ItemQueue = new Queue<Beatmap.Entity>();
        public static bool WantBop = true;
        public static bool WantNori = true;
        public static int WantNoriType = (int) NoriMode.None;
        public static double WantBgChangeStart = double.MinValue;
        public static float WantBgChangeLength = 0f;
        
        const int IAAltDownCat = IAMAXCAT;
        const int IAAltUpCat = IAMAXCAT + 1;

        protected static bool IA_PadAnyDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_PadAnyUp(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadUp(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_PadAny(out double dt)
        {
            dt = 0;
            return PlayerInput.GetPad(InputController.ActionsPad.East)
                    || PlayerInput.GetPad(InputController.ActionsPad.Up)
                    || PlayerInput.GetPad(InputController.ActionsPad.Down)
                    || PlayerInput.GetPad(InputController.ActionsPad.Left)
                    || PlayerInput.GetPad(InputController.ActionsPad.Right);
        }
        protected static bool IA_TouchDown(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && !instance.IsExpectingInputNow(InputAction_AltDown);
        }
        protected static bool IA_TouchUp(out double dt)
        {
            return PlayerInput.GetTouchUp(InputController.ActionsTouch.Tap, out dt)
                && !instance.IsExpectingInputNow(InputAction_AltDown);
        }

        protected static bool IA_EmptyTouchUp(out double dt)
        {
            return PlayerInput.GetTouchUp(InputController.ActionsTouch.Tap, out dt) && !PlayerInput.GetFlick(out _);
        }

        protected static bool IA_PadAltDown(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltDown(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltDown(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltDown);
        }

        protected static bool IA_PadAltUp(out double dt)
        {
            return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltUp(out double dt)
        {
            return PlayerInput.GetSqueezeUp(out dt);
        }
        protected static bool IA_TouchAltUp(out double dt)
        {
            return PlayerInput.GetFlick(out dt)
                && instance.IsExpectingInputNow(InputAction_AltUp);
        }

        public static PlayerInput.InputAction InputAction_Press =
            new("KaratePress", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadAnyDown, IA_TouchDown, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_Flick =
            new("KaratePress", new int[] { IAFlickCat, IAFlickCat, IAFlickCat },
            IA_PadAnyUp, IA_TouchFlick, IA_BatonBasicRelease);
        public static PlayerInput.InputAction InputAction_Pressing =
            new("KaratePress", new int[] { IAPressingCat, IAPressingCat, IAPressingCat },
            IA_PadAny, IA_TouchBasicPressing, IA_BatonBasicPressing);
        public static PlayerInput.InputAction InputAction_AltDown =
            new("KarateAltDown", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltDown, IA_TouchAltDown, IA_BatonAltDown);
        public static PlayerInput.InputAction InputAction_AltUp =
            new("KarateAltUp", new int[] { IAAltUpCat, IAAltUpCat, IAAltUpCat },
            IA_PadAltUp, IA_TouchAltUp, IA_BatonAltUp);
        public static PlayerInput.InputAction InputAction_TouchUp =
            new("KarateAltUp", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_PadAltUp, IA_EmptyTouchUp, IA_BatonAltUp);
        public List<RiqEntity> voiceEntities, hitVoiceEntities = new();

        public static KarateMan instance;

        private void Awake()
        {
            instance = this;
            
            KarateManPot.ResetLastCombo();

            bgEffectAnimator = BGEffect.GetComponent<Animator>();
            bgEffectSpriteRenderer = BGEffect.GetComponent<SpriteRenderer>();

            colorEnds =
            colorStarts = new Color[] {
                BackgroundColors[0],
                TintColor(BackgroundColors[0]),
                new Color(),
            };

            Update();
        }

        private void Start()
        {
            Update();
        }

        public override void OnGameSwitch(double beat)
        {
            // queued objects
            if (queuedCues.Count > 0) {
                foreach (var e in queuedCues) {
                    switch (e.datamodel) {
                        case "karateman/hit"  : CreateItem(e.beat, e["type"], e["type2"]); break;
                        case "karateman/bulb" : CreateBulbSpecial(e.beat, e["type"], e["colorA"], e["type2"], e["sfx"], e["hitSfx"]); break;
                        case "karateman/kick" : Kick(e.beat, e["toggle"], e["shouldGlow"], e["type"], e["pitchVoice"], e["forcePitch"], e["cutOut"], e["disableVoice"], e["woodColor"], e["hoopColor"]); break;
                        case "karateman/combo": Combo(e.beat, e["type"], e["pitchVoice"], e["forcePitch"], e["cutOut"], e["disableVoice"]); break;
                        default : Debug.LogError($"Karate Man has failed to cue an object with datamodel {e.datamodel} at beat {e.beat}"); break;
                    }
                }
                queuedCues.Clear();
            }

            EntityPreCheck(beat);
        }

        public override void OnPlay(double beat)
        {
            EntityPreCheck(beat);
        }

        void EntityPreCheck(double beat)
        {
            List<RiqEntity> prevEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split(0) == "karateman");

            RiqEntity voice = prevEntities.FindLast(c => c.beat < beat && c.datamodel == "karateman/warnings");
            if (wordClearTime > beat && wordStartTime < beat && voice != null) {
                DoWord(voice.beat, voice.length, voice["whichWarning"], false, 1, voice["customLength"], false);
            }

            // init colors
            RiqEntity bg = prevEntities.FindLast(c => c.beat <= beat && c.datamodel == "karateman/background appearance");
            RiqEntity obj = prevEntities.FindLast(c => c.beat <= beat && c.datamodel == "karateman/set object colors");
            
            if (bg != null) {
                BackgroundColor(
                    bg.beat, bg.length,
                    bg["presetBg"], bg["startColor"], bg["endColor"], bg["ease"],
                    bg["shadowType"], bg["shadowStart"], bg["shadowEnd"],
                    bg["textureType"], bg["autoColor"], bg["startTexture"], bg["endTexture"]
                );
                if (bg["fxType"] != 3) currentBgEffect = bg["fxType"];
            } else {
                var c = new Color();
                BackgroundColor(0, 0, 0, c, c, (int)Util.EasingFunction.Ease.Instant, 0, c, c, 0, true, c, c);
            }
            
            if (obj != null) {
                UpdateMaterialColour(obj["colorA"], obj["colorB"], obj["colorC"]);
            } else {
                UpdateMaterialColour(Color.white, new Color(0.81f, 0.81f, 0.81f), Color.white);
            }

            // init modifier(s)
            RiqEntity bop = prevEntities.FindLast(c => c.beat < beat && c.datamodel == "karateman/bop");
            ToggleBop(0, 0, false, bop?["toggle"] ?? true);

            // get all entities to later check against eachother to cut out voices
            voiceEntities = prevEntities.FindAll(c => c.beat > beat && (c.datamodel is "karateman/kick" or "karateman/combo"));
            hitVoiceEntities = prevEntities.FindAll(c => c.beat > beat && (c.datamodel is "karateman/warnings" && c["whichWarning"] <= (int)HitThree.HitFour));
        }


        private void Update()
        {
            var cond = Conductor.instance;
            var songPos = cond.songPositionInBeatsAsDouble;

            if (!cond.isPlaying) {
                EntityPreCheck(songPos);
            }
            
            switch (currentBgEffect)
            {
                case (int) BackgroundFXType.Sunburst:
                    bgEffectAnimator.DoNormalizedAnimation("Sunburst", (float)(songPos * 0.5) % 1f);
                    break;
                case (int) BackgroundFXType.Rings:
                    bgEffectAnimator.DoNormalizedAnimation("Rings", (float)(songPos * 0.5) % 1f);
                    break;
                default:
                    bgEffectAnimator.Play("NoPose", -1, 0);
                    break;
            }

            if (songPos >= wordClearTime || songPos < wordStartTime) {
                Word.Play("NoPose");
            }

            if (songPos >= startCamSpecial && songPos <= wantsReturn)
            {
                float camX = 0f;
                float camY = 0f;
                float camZ = 0f;
                if (songPos <= startCamSpecial + cameraReturnLength)
                {
                    float prog = cond.GetPositionFromBeat(startCamSpecial, cameraReturnLength);
                    camX = Util.EasingFunction.EaseOutCubic(CameraPosition[0].position.x, CameraPosition[1].position.x, prog);
                    camY = Util.EasingFunction.EaseOutCubic(CameraPosition[0].position.y, CameraPosition[1].position.y, prog);
                    camZ = Util.EasingFunction.EaseOutCubic(CameraPosition[0].position.z, CameraPosition[1].position.z, prog);
                    cameraPosition = new Vector3(camX, camY, camZ);
                }
                else if (songPos >= wantsReturn - cameraReturnLength)
                {
                    float prog = cond.GetPositionFromBeat(wantsReturn - cameraReturnLength, cameraReturnLength);
                    camX = Util.EasingFunction.EaseOutQuad(CameraPosition[1].position.x, CameraPosition[0].position.x, prog);
                    camY = Util.EasingFunction.EaseOutQuad(CameraPosition[1].position.y, CameraPosition[0].position.y, prog);
                    camZ = Util.EasingFunction.EaseOutQuad(CameraPosition[1].position.z, CameraPosition[0].position.z, prog);
                    cameraPosition = new Vector3(camX, camY, camZ);
                }
                else
                {
                    cameraPosition = CameraPosition[1].position;
                }
            }
            else
            {
                if (cameraAngle == CameraAngle.Special)
                    cameraAngle = CameraAngle.Normal;
                cameraPosition = CameraPosition[0].position;
            }

            BackgroundColorUpdate();
            GameCamera.AdditionalPosition = cameraPosition - GameCamera.defaultPosition;
            BGEffect.transform.position = new Vector3(GameCamera.instance.transform.position.x, GameCamera.instance.transform.position.y, 0);
        }

        private void OnDestroy()
        {
            foreach (var evt in scheduledInputs) {
                evt.Disable();
            }
            if (!Conductor.instance.NotStopped()) {
                if (queuedCues.Count > 0) queuedCues.Clear();
                startCamSpecial = double.MinValue;
                wantsReturn = double.MinValue;
                cameraReturnLength = 0f;
                cameraAngle = CameraAngle.Normal;
            }
        }

        private Color TintColor(Color color) => Color.LerpUnclamped(color, new Color(195 / 255f, 48 / 255f, 2 / 255f), 0.45f);

        public static void DoSpecialCamera(double beat, float length, bool returns)
        {
            if (cameraAngle == CameraAngle.Normal)
            {
                startCamSpecial = beat;
                cameraAngle = CameraAngle.Special;
            }
            wantsReturn = returns ? beat + length - 0.001f : double.MaxValue;
            cameraReturnLength = Mathf.Min(2f, length * 0.5f);
        }

        public void DoWord(double beat, double length, int type, bool pitchVoice, float forcePitch, bool customLength, bool doSound = true)
        {
            Word.Play(DoWordSound(beat, length, type, pitchVoice, forcePitch, customLength, doSound));
        }

        public static string DoWordSound(double beat, double length, int type, bool bpmPitch = false, float forcePitch = 1, bool customLength = false, bool doSound = true)
        {
            double clear = type switch {
                <= (int)HitThree.HitFour => beat + 4f,
                <= (int)HitThree.Warning => beat + 1f,
                _ => beat + 3f,
            };

            if (type <= (int)HitThree.HitFour && doSound)
            {
                string number = ((HitThree)type).ToString()[3..];
                number = char.ToLower(number[0]).ToString() + number[1..];
                var sounds = new MultiSound.Sound[] {
                    new MultiSound.Sound($"karateman/{(type == (int)HitThree.HitThreeAlt ? "hitAlt" : "hit")}", beat + 0.5f, offset: 0.042f),
                    new MultiSound.Sound($"karateman/{number}", beat + 1f),
                };
                Array.ForEach(sounds, x => x.pitch = bpmPitch ? Conductor.instance.GetBpmAtBeat(x.beat) / 125 : forcePitch);
                MultiSound.Play(sounds, forcePlay: true);
            }

            var songPos = Conductor.instance.songPositionInBeatsAsDouble;
            if (songPos <= clear && songPos >= beat) {
                wordClearTime = customLength ? (beat + length) : clear;
                wordStartTime = beat;
            }
            return $"Word0{(type < (int)HitThree.HitThreeAlt ? type : type - 1)}";
        }

        public static void QueueCue(RiqEntity entity)
        {
            queuedCues.Add(entity);
        }

        public static void CreateItemSFX(double beat, int type, bool muteSound = false)
        {
            if (!muteSound) SoundByte.PlayOneShotGame($"karateman/{(beat % 1.0 == 0.5 ? $"offbeatObject" : "object")}Out", forcePlay: true);
        }

        public static void CreateBulbSFX(double beat, int type, int sfx, string throwSfx)
        {
            string obj = sfx switch {
                (int)LightBulbSfx.Automatic => type == (int)LightBulbType.Yellow ? "LightbulbNtr" : "Lightbulb",
                (int)LightBulbSfx.DS => "LightbulbNtr",
                (int)LightBulbSfx.Custom => throwSfx,
                _ => "Lightbulb",
            };
            if (sfx != (int)LightBulbSfx.Custom) {
                obj = (beat % 1.0 == 0.5) ? $"offbeat{obj}Out" : obj.ToLower() + "Out";
            }
            SoundByte.PlayOneShotGame($"karateman/{obj}", forcePlay: true);
        }

        public void CreateItem(double beat, int type, int expression)
        {
            switch (type)
            {
                case (int) HitType.Pot:
                    CreateItemInstance(beat, "Item00", expression);
                    break;
                case (int) HitType.Lightbulb:
                    var mobj = CreateItemInstance(beat, "Item01", expression, KarateManPot.ItemType.Bulb);
                    mobj.SetBulbColor(LightBulbColors[0]);
                    break;
                case (int) HitType.Rock:
                    CreateItemInstance(beat, "Item02", expression, KarateManPot.ItemType.Rock);
                    break;
                case (int) HitType.Ball:
                    CreateItemInstance(beat, "Item03", expression, KarateManPot.ItemType.Ball);
                    break;
                case (int) HitType.CookingPot:
                    CreateItemInstance(beat, "Item06", expression, KarateManPot.ItemType.Cooking);
                    break;
                case (int) HitType.Alien:
                    CreateItemInstance(beat, "Item07", expression, KarateManPot.ItemType.Alien);
                    break;
                case (int) HitType.Bomb:
                    CreateItemInstance(beat, "Item04", expression, KarateManPot.ItemType.Bomb);
                    break;
                case (int) HitType.TacoBell:
                    CreateItemInstance(beat, "Item99", expression, KarateManPot.ItemType.TacoBell);
                    break;
                default:
                    CreateItemInstance(beat, "Item00", expression);
                    break;
            }
        }

        public void CreateBulbSpecial(double beat, int type, Color color, int expression, int sfx, string hitSfx = "")
        {
            string obj = sfx switch {
                (int)LightBulbSfx.Automatic => type == (int)LightBulbType.Yellow ? "LightbulbNtr" : "Lightbulb",
                (int)LightBulbSfx.DS => "LightbulbNtr",
                (int)LightBulbSfx.Custom => hitSfx,
                _ => "Lightbulb",
            };
            if (sfx != (int)LightBulbSfx.Custom) obj += "Hit";
            var mobj = CreateItemInstance(beat, "Item01", expression, KarateManPot.ItemType.Bulb, hitSfxOverride: $"karateman/{obj}");

            mobj.SetBulbColor((type == (int)LightBulbType.Custom) ? color : LightBulbColors[type]);
        }

        public static void ComboSFX()
        {
            SoundByte.PlayOneShotGame("karateman/barrelOutCombos", forcePlay: true);
        }

        public void Combo(double beat, int expression, bool bpmPitch, float forcePitch, bool cutOut, bool noVoice)
        {
            int comboId = KarateManPot.GetNewCombo();

            BeatAction.New(this, new List<BeatAction.Action>()
            { 
                new BeatAction.Action(beat, delegate { CreateItemInstance(beat, "Item00", 0, KarateManPot.ItemType.ComboPot1, comboId); }),
                new BeatAction.Action(beat + 0.25f, delegate { CreateItemInstance(beat + 0.25f, "Item00", 0, KarateManPot.ItemType.ComboPot2, comboId); }),
                new BeatAction.Action(beat + 0.5f, delegate { CreateItemInstance(beat + 0.5f, "Item00", 0, KarateManPot.ItemType.ComboPot3, comboId); }),
                new BeatAction.Action(beat + 0.75f, delegate { CreateItemInstance(beat + 0.75f, "Item00", 0, KarateManPot.ItemType.ComboPot4, comboId); }),
                new BeatAction.Action(beat + 1f, delegate { CreateItemInstance(beat + 1f, "Item00", 0, KarateManPot.ItemType.ComboPot5, comboId); }),
                new BeatAction.Action(beat + 1.5f, delegate { CreateItemInstance(beat + 1.5f, "Item05", expression, KarateManPot.ItemType.ComboBarrel, comboId); }),
            });

            if (noVoice) return;

            List<MultiSound.Sound> sounds = new() {
                new MultiSound.Sound("karateman/punchy1", beat + 1f),
                new MultiSound.Sound("karateman/punchy2", beat + 1.25f),
                new MultiSound.Sound("karateman/punchy3", beat + 1.5f),
                new MultiSound.Sound("karateman/punchy4", beat + 1.75f),
                new MultiSound.Sound("karateman/ko", beat + 2f),
                new MultiSound.Sound("karateman/pow", beat + 2.5f)
            };

            if (voiceEntities.Count > 0 && cutOut)
            {
                RiqEntity firstVoice = voiceEntities.Find(x => x.beat >= beat + 1);
                RiqEntity firstHitVoice = hitVoiceEntities.Find(x => x.beat >= beat + 1);
                if (firstVoice != null) sounds.RemoveAll(x => x.beat > firstVoice.beat);
                if (firstHitVoice != null) sounds.RemoveAll(x => x.beat > firstHitVoice.beat - 0.5);
            }

            sounds.ForEach(x => x.pitch = bpmPitch ? Conductor.instance.GetBpmAtBeat(x.beat) / 125 : forcePitch);
            
            MultiSound.Play(sounds.ToArray(), forcePlay: true);
        }

        public static void KickSFX()
        {
            SoundByte.PlayOneShotGame("karateman/barrelOutKicks", forcePlay: true);
        }

        public void Kick(double beat, bool ball, bool glow, int expression, bool bpmPitch, float forcePitch, bool cutOut, bool noVoice, Color woodColor, Color hoopColor)
        {
            var barrel = CreateItemInstance(beat, "Item05", expression, KarateManPot.ItemType.KickBarrel, content: ball, shouldGlow: glow);
            // red : new Color(0.451f, 0.302f, 0.271f)
            // green : new Color(0.302f, 0.169f, 0.035f) - unused?
            // blue : new Color(0.714f, 0.31f, 0.424f)
            barrel.ItemBarrelMap[0] = woodColor;
            barrel.ItemBarrelMap[2] = hoopColor;

            if (noVoice) return;

            List<MultiSound.Sound> sounds = new() {
                new MultiSound.Sound("karateman/punchKick1", beat + 1f),
                new MultiSound.Sound("karateman/punchKick2", beat + 1.5f),
                new MultiSound.Sound("karateman/punchKick3", beat + 1.75f),
                new MultiSound.Sound("karateman/punchKick4", beat + 2.5f),
            };

            if (voiceEntities.Count > 0 && cutOut)
            {
                RiqEntity firstVoice = voiceEntities.Find(x => x.beat >= beat + 1);
                RiqEntity firstHitVoice = hitVoiceEntities.Find(x => x.beat >= beat + 1);
                if (firstVoice != null) sounds.RemoveAll(x => x.beat > firstVoice.beat);
                if (firstHitVoice != null) sounds.RemoveAll(x => x.beat > firstHitVoice.beat);
            }

            sounds.ForEach(x => x.pitch = bpmPitch ? Conductor.instance.GetBpmAtBeat(x.beat) / 125 : forcePitch);
            
            MultiSound.Play(sounds.ToArray(), forcePlay: true);
        }

        public KarateManPot CreateItemInstance(double beat, string awakeAnim, int successExpression, KarateManPot.ItemType type = KarateManPot.ItemType.Pot, int comboId = -1, bool content = false, bool shouldGlow = false, string hitSfxOverride = null)
        {
            GameObject mobj = Instantiate(Item, ItemHolder);
            KarateManPot mobjDat = mobj.GetComponent<KarateManPot>();
            mobjDat.type = type;
            mobjDat.startBeat = beat;
            mobjDat.awakeAnim = awakeAnim;
            mobjDat.comboId = comboId;
            mobjDat.OnHitExpression = successExpression;
            mobjDat.KickBarrelContent = content;
            mobjDat.ShouldGlow = shouldGlow;
            mobjDat.hitSfxOverride = hitSfxOverride;

            mobj.SetActive(true);
            
            return mobjDat;
        }

        public void BackgroundColor(double beat, float length, int presetBG, Color colorStart, Color colorEnd, int colorEaseSet, int shadowType, Color shadowStart, Color shadowEnd, int textureType, bool autoColor, Color filterStart, Color filterEnd)
        {
            for (int i = 0; i < colorStarts.Length; i++) {
                colorStartBeats[i] = beat;
                colorLengths[i] = length;
                colorEases[i] = (Util.EasingFunction.Ease)colorEaseSet;
            }

            bool preset = presetBG != (int)BackgroundType.Custom;
            bool tinted = shadowType == (int)ShadowType.Tinted;

            Color bgColorStart = preset ? BGPlane.color : colorStart;
            colorStarts = new Color[] {
                bgColorStart,
                tinted ? TintColor(bgColorStart) : shadowStart,
                autoColor ? TintColor(bgColorStart): filterStart,
            };

            Color bgColorEnd = preset ? BackgroundColors[presetBG] : colorEnd;
            colorEnds = new Color[] {
                bgColorEnd,
                tinted ? TintColor(bgColorEnd) : shadowEnd,
                autoColor ? TintColor(bgColorEnd) : filterEnd,
            };

            for (int i = 0; i < BGTextures.Length; i++) {
                BGTextures[i].gameObject.SetActive(textureType == (i + 1));
            }

            BackgroundColorUpdate();
        }

        private void BackgroundColorUpdate()
        {
            SpriteRenderer[][] spriteRenderers = new[] {
                new[] { BGPlane },
                Joe.Shadows,
                BGTextures,
            };

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeats[i], colorLengths[i]));
                if (double.IsNaN(normalizedBeat)) normalizedBeat = 0; // happens if the game is stopped onto the first beat
                var func = Util.EasingFunction.GetEasingFunction(colorEases[i]);
                float[] color = new float[3] {
                    func(colorStarts[i].r, colorEnds[i].r, normalizedBeat),
                    func(colorStarts[i].g, colorEnds[i].g, normalizedBeat),
                    func(colorStarts[i].b, colorEnds[i].b, normalizedBeat),
                };

                foreach (var renderer in spriteRenderers[i]) {   
                    renderer.color = new Color(color[0], color[1], color[2]);
                }
            }
        }

        public void SetGameplayMods(double beat, int fxType, int mode, bool combo)
        {
            NoriGO.SetActive(true);
            Nori.SetNoriMode(beat, mode);
            currentBgEffect = fxType;
            IsComboEnable = combo;
        }

        public void UpdateMaterialColour(Color mainCol, Color highlightCol, Color objectCol)
        {
            BodyColor = mainCol;
            HighlightColor = highlightCol;
            ItemColor = objectCol;
        }

        public void SetParticleEffect(double beat, int type, bool instant, float windStrength, float particleStrength)
        {
            if (type == (int) ParticleType.None) {
                foreach (var eff in Effects) eff.Stop();
                return;
            }

            ParticleSystem particleSystem = Effects[type - 1];

            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();

            var emm = particleSystem.emission;
            var main = particleSystem.main;

            emm.rateOverTime = particleStrength * (type == (int)ParticleType.Rain ? 32f : 6f);
            main.prewarm = instant;

            Wind.windMain = windStrength;
        }

        public override void OnBeatPulse(double beat)
        {
            Joe.RequestBop();
        }

        public void ToggleBop(double beat, float length, bool toggle, bool autoBop)
        {
            Joe.bop.length = autoBop ? float.MaxValue : 0;

            if (toggle)
            {
                var actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++) actions.Add(new(beat + i, delegate { Joe.Bop(); }));
                BeatAction.New(instance, actions);
            }
        }

        public void Prepare(double beat, float length)
        {
            Joe.Prepare(beat, length);
        }

        public void SetFaceExpression(int face)
        {
            Joe.SetFaceExpression(face);
        }
    }
}