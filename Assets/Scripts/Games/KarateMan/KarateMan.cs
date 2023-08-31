using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using Jukebox;
using System.Linq;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlNewKarateLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            /* what the fuck
            RiqEntity NameUpdater(string datamodel, RiqEntity entity)
            {
                if (entity.datamodel.Split('/')[0] == "karateMan")
                {
                    Debug.Log(datamodel);
                    // this was the only way i could figure out how to convert from that freaky format into an actual color :(
                    // if (entity.dynamicData.ContainsKey("colorA")) {
                    //     for (int i = 0; i < (entity.dynamicData.ContainsKey("colorB") ? 4 : 1); i++)
                    //     {
                    //         string whichColor = i switch {
                    //             0 => "colorA",
                    //             1 => "colorB",
                    //             2 => "colorC",
                    //             _ => "colorD",
                    //         };
                    //         string[] tempValues = entity[whichColor].ToString().Split(": ");
                    //         List<float> values = new();
                    //         foreach (string value in tempValues)
                    //         {
                    //             string temp = string.Join("", value.Where(x => x == '.' || char.IsDigit(x)));
                    //             if (temp != string.Empty) values.Add(float.Parse(temp));
                    //         }
                            
                    //         entity[whichColor] = new Color(values[0], values[1], values[2]);
                    //     }
                    // }
                    
                    entity.datamodel = entity.datamodel.Remove(0, 9);
                    entity.datamodel = "karateMan" + entity.datamodel;
                    return entity;
                } else if (entity.datamodel == "gameManager/switchGame/karateMan") {
                    entity.datamodel = "gameManager/switchGame/karateMan";
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += NameUpdater;
            */

            RiqEntity WarningUpdater(string datamodel, RiqEntity entity)
            {
                if (datamodel == "karateman/hitX")
                {
                    int newWarning = (int)entity["type"];
                    if (entity["type"] < 7) {
                        newWarning++;
                    } else {
                        newWarning = 0;
                    }

                    entity.CreateProperty("whichWarning", newWarning);
                    entity.CreateProperty("customLength", false);
                    entity.CreateProperty("cutOut", true);
                    
                    entity.dynamicData.Remove("type");

                    entity.datamodel = "karateMan/warnings";
                    return entity;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += WarningUpdater;
            
            RiqEntity BackgroundUpdater(string datamodel, RiqEntity entity)
            {
                if (datamodel == "karateman/set background effects")
                {
                    
                    return entity;
                }
                return null;
            }
            //RiqBeatmap.OnUpdateEntity += BackgroundUpdater;

            RiqEntity GameCapitalizer(string datamodel, RiqEntity entity)
            {
                if (datamodel.Split('/')[0] == "karateman")
                {
                    string name = datamodel.Split('/')[1];
                    entity.datamodel = "karateMan/" + name;
                    var tempData = entity.dynamicData.ToDictionary(x => x.Key);
                    foreach ((string key, dynamic item) in tempData)
                    {
                        if (item.GetType() == typeof(Newtonsoft.Json.Linq.JObject)) {
                            entity.dynamicData[key] = new Color((float)item["r"], (float)item["g"], (float)item["b"]);
                        }
                        Debug.Log(key + ", " + item);
                    }
                    entity.version = 1;

                    return entity;
                } else if (datamodel == "gameManager/switchGame/karateman") {
                    entity.datamodel = "gameManager/switchGame/karateMan";
                    return entity;
                }
                return null;
            }
            RiqBeatmap.OnUpdateEntity += GameCapitalizer;

            return new Minigame("karateMan", "Karate Man", "fbca3e", false, false, new List<GameAction>()
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
                        KarateMan.QueueItem(e.beat, e["type"], e["type2"]);
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
                        KarateMan.instance.CreateBulbSpecial(e.beat, e["type"], e["colorA"], e["type2"]);
                        KarateMan.CreateBulbSFX(e.beat, e["type"], false);
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.QueueBulb(e.beat, e["type"], e["colorA"], e["type2"]);
                        KarateMan.CreateBulbSFX(e.beat, e["type"], false);
                    },
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.LightBulbType.Normal, "Type", "The preset bulb type. Yellow is used for kicks while Blue is used for combos", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam(x => (int)x == (int)KarateMan.LightBulbType.Custom, new string[] { "colorA" })
                        }),
                        new Param("colorA", new Color(1f,1f,1f), "Custom Color", "The color to use when the bulb type is set to Custom"),
                        new Param("type2", KarateMan.KarateManFaces.Normal, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("mute", false, "Mute", "Should the throwing sound be muted?"),
                    },
                },
                new GameAction("kick", "Special: Kick")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.Kick(e.beat, e["toggle"], e["type"], e["pitchVoice"], e["forcePitch"], e["cutOut"], e["disableVoice"]);
                        KarateMan.KickSFX();
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.QueueKick(e.beat, e["toggle"], e["type"], e["pitchVoice"], e["forcePitch"], e["cutOut"], e["disableVoice"]);
                        KarateMan.KickSFX();
                    },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Contains Ball", "Barrel contains a ball instead of a bomb?"),
                        new Param("type", KarateMan.KarateManFaces.Smirk, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("pitchVoice", false, "Pitch Voice", "Pitch the voice of this cue?", new List<Param.CollapseParam>() 
                        {
                            new Param.CollapseParam(x => (bool)x, new string[] { "forcePitch" }),
                        }),
                        new Param("forcePitch", new EntityTypes.Float(0.5f, 2f, 1f), "Force Pitch", "Override the automatic pitching if not set to 1"),
                        new Param("cutOut", true, "Cut Out Voice", "Will this cue be cut out by another voice?"),
                        new Param("disableVoice", false, "Disable Voice", "When enabled, there will be no voice during this cue"),
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
                        KarateMan.QueueCombo(e.beat, e["type"], e["pitchVoice"], e["forcePitch"], e["cutOut"], e["disableVoice"]);
                        KarateMan.ComboSFX();
                    },
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.KarateManFaces.Happy, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("pitchVoice", false, "Pitch Voice", "Pitch the voice of this cue?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam(x => (bool)x, new string[] { "forcePitch" }),
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
                        new Param("pitchVoice", false, "Pitch Voice", "Pitch the voice of this cue?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam(x => (bool)x, new string[] { "forcePitch" }),
                        }),
                        new Param("forcePitch", new EntityTypes.Float(0.5f, 2f, 1f), "Force Pitch", "Override the automatic pitching if not set to 1"),
                        new Param("customLength", false, "Custom Length", "Have the warning text appear for the length of the block"),
                        new Param("cutOut", true, "Cut Out Voice", "Will this cue be cut out by another voice?"),
                    },
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.QueueWord(e.beat, e.length, e["whichWarning"], e["pitchVoice"], e["forcePitch"], e["customLength"]);
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
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetGameplayMods(e.beat, e["type"], e["toggle"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.NoriMode.None, "Flow Bar type", "The type of Flow bar to use"),
                        new Param("toggle", true, "Enable Combos", "Allow the player to combo? (Contextual combos will still be allowed even when off)"),
                        new Param("toggle2", true, "Enable Kicks", "Allow the player to kick? (Contextual kicks will still be allowed even when off)"),
                    },
                },
                new GameAction("background appearance", "Background Appearance")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        KarateMan.instance.BackgroundColor(e.beat, e.length, e["presetBg"], e["startColor"], e["endColor"], e["bgEase"], e["shadowType"], e["shadowStart"], e["shadowEnd"]);
                        KarateMan.instance.SetBgFx(e.beat, e.length, e["fxType"], e["textureType"], e["autoColor"], e["startTexture"], e["endTexture"], e["textureEase"]);
                    },
                    defaultLength = 0.5f,
                    resizable = true,
                    parameters = new List<Param>() // uncomment these collapses when overlapping collapses are implemented
                    {
                        new Param("presetBg", KarateMan.BackgroundType.Yellow, "Preset BG Color", "The preset background type (will by default fade from the existing background color)", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam(x => (int)x == (int)KarateMan.BackgroundType.Custom, new string[] { "startColor", "endColor" })
                        }),
                        new Param("startColor", new Color(0.985f, 0.79f, 0.243f), "Start BG Color", "The background color to use when background type is set to Custom"),
                        new Param("endColor", new Color(0.985f, 0.79f, 0.243f), "End BG Color", "When using the Fade background effect, make filter colour fade to this colour"),
                        new Param("bgEase", Util.EasingFunction.Ease.Instant, "BG Color Ease", "Ease to use when fading color", new List<Param.CollapseParam>()
                        {
                            //new Param.CollapseParam(x => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "startColor" })
                        }),
                        new Param("shadowType", KarateMan.ShadowType.Tinted, "Shadow Type", "The shadow type. If Tinted doesn't work with your background color try Custom"),
                        new Param("shadowStart", new Color(), "Start Shadow Color", "The shadow color to use when shadow type is set to Custom. When fading the background colour shadows fade to this color"),
                        new Param("shadowEnd", new Color(), "End Shadow Color", "The shadow color to use when shadow type is set to Custom. When fading the background colour shadows fade to this color"),
                        
                        new Param("fxType", KarateMan.BackgroundFXType.None, "FX Type", "The background effect to be displayed"),
                        new Param("textureType", KarateMan.BackgroundTextureType.Plain, "Texture", "The type of background texture to use", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam(x => (int)x != (int)KarateMan.BackgroundTextureType.Plain, new string[] { "startTexture", "endTexture" })
                        }),
                        new Param("autoColor", true, "Use BG Color For Texture", "Use a tint of the background color for the ", new List<Param.CollapseParam>()
                        {
                            //new Param.CollapseParam(x => (int)x != (int)KarateMan.ShadowType.Tinted, new string[] { "startTexture", "endTexture" })
                        }),
                        new Param("startTexture", new Color(), "Start Filter Color", "The filter color to use when color filter type is set to Custom"),
                        new Param("endTexture", new Color(), "End Filter Color", "When using the Fade background effect, make filter colour fade to this colour"),
                        new Param("textureEase", Util.EasingFunction.Ease.Instant, "Texture Color Ease", "Ease to use when fading color", new List<Param.CollapseParam>()
                        {
                            //new Param.CollapseParam(x => (int)x != (int)Util.EasingFunction.Ease.Instant, new string[] { "startTexture" })
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
                        KarateMan.UpdateMaterialColour(e["colorA"], e["colorB"], e["colorC"]);
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
                        KarateMan.instance.SetParticleEffect(e.beat, e["type"], e["valA"], e["valB"]);
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.ParticleType.None, "Particle Type", "The type of particle effect to spawn. Using \"None\" will stop all effects"),
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
        static List<QueuedObject> queuedObjects = new();
        static List<QueuedObject> queuedBulbs = new();
        struct QueuedObject
        {
            public double beat;
            public int type;
            public int expression;
            public Color color;
        }

        static List<QueuedSpecial> queuedKicks = new();
        static List<QueuedSpecial> queuedCombos = new();
        struct QueuedSpecial
        {
            public double beat;
            public int expression;
            public bool pitchVoice;
            public float forcePitch;
            public bool cutOut;
            public bool noVoice;
            public bool ball;
        }

        public static KarateMan instance;

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
            Rings,
            Fade
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

        public static bool IsComboEnable = true; //only stops Out combo inputs, this basically makes combo contextual
        public bool IsNoriActive { get { return Nori.MaxNori > 0; } }
        public float NoriPerformance { get { if (IsNoriActive) return Nori.Nori / Nori.MaxNori; else return 1f; } }

        public Color[] LightBulbColors;
        public Color[] BackgroundColors;
        public Color[] ShadowColors;

        //camera positions (normal, special)
        [Header("Camera Positions")]
        public Transform[] CameraPosition;
        Vector3 cameraPosition;
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
        public static Color BodyColor = Color.white;
        public static Color HighlightColor = new Color(0.81f,0.81f,0.81f,1);
        public static Color ItemColor = Color.white;

        [Header("Word")]
        public Animator Word;
        static double wordClearTime = double.MinValue;
        const float hitVoiceOffset = 0.042f;

        [Header("Backgrounds")]
        // new bg fading
        private double[] colorStartBeats = new double[3] {
            -1,
            -1,
            -1
        };
        private float[] colorLengths = new float[3];
        private Color[] colorStarts = new Color[3];
        private Color[] colorEnds = new Color[3]; // 0 = bg color, 1 = shadow color, 2 = filter color
        private Util.EasingFunction.Ease[] colorEases = new Util.EasingFunction.Ease[3];

        public int currentBgEffect = (int) BackgroundFXType.None;

        public SpriteRenderer BGPlane;
        public GameObject BGEffect;
        public GameObject BGGradient;
        SpriteRenderer bgGradientRenderer;
        public GameObject BGBlood;
        SpriteRenderer bgBloodRenderer;
        public GameObject BGRadial;
        SpriteRenderer bgRadialRenderer;
        SpriteRenderer[] bgFxRenderers;
        Animator bgEffectAnimator;
        SpriteRenderer bgEffectSpriteRenderer;

        [Header("Particles")]
            //wind
        public WindZone Wind;

        public ParticleSystem[] Effects;
        public GameObject[] EffectsGO;
            //snow
        public ParticleSystem SnowEffect;
        public GameObject SnowEffectGO;
            //fire
        public ParticleSystem FireEffect;
        public GameObject FireEffectGO;
            //rain
        public ParticleSystem RainEffect;
        public GameObject RainEffectGO;

        [Header("Unloaded Game Calls")]
        public List<RiqEntity> voiceEntities = new();
        public List<RiqEntity> hitVoiceEntities = new();

        private void Awake()
        {
            instance = this;
            KarateManPot.ResetLastCombo();
            cameraPosition = CameraPosition[0].position;
            
            bgGradientRenderer = BGGradient.GetComponent<SpriteRenderer>();
            bgBloodRenderer = BGBlood.GetComponent<SpriteRenderer>();
            bgRadialRenderer = BGRadial.GetComponent<SpriteRenderer>();
        }

        public override void OnGameSwitch(double beat)
        {
            // queued objects
            if (queuedObjects.Count > 0) {
                foreach (var qObj in queuedObjects) { CreateItem(qObj.beat, qObj.type, qObj.expression); }
                queuedObjects.Clear();
            }

            if (queuedBulbs.Count > 0) {
                foreach (var qObj in queuedBulbs) { CreateBulbSpecial(qObj.beat, qObj.type, qObj.color, qObj.expression); }
                queuedBulbs.Clear();
            }
            
            if (queuedKicks.Count > 0) {
                foreach (var qObj in queuedKicks) { Kick(qObj.beat, qObj.ball, qObj.expression, qObj.pitchVoice, qObj.forcePitch, qObj.cutOut, qObj.noVoice); }
                queuedKicks.Clear();
            }

            if (queuedCombos.Count > 0) {
                foreach (var qObj in queuedCombos) { Combo(qObj.beat, qObj.expression, qObj.pitchVoice, qObj.forcePitch, qObj.cutOut, qObj.noVoice); }
                queuedCombos.Clear();
            }

            List<RiqEntity> prevEntities = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel.Split(0) == "karateMan");

            RiqEntity voice = prevEntities.FindLast(c => c.beat < beat && c.datamodel.Split(1) == "warnings");
            if (wordClearTime > beat && voice != null) {
                var type = voice["whichWarning"];
                Word.Play($"Word0{(type < (int)HitThree.HitThreeAlt ? type : type - 1)}");
            }

            // init colors
            RiqEntity bg = prevEntities.FindLast(c => c.beat < beat && c.datamodel.Split(1) == "set background effects");
            RiqEntity obj = prevEntities.FindLast(c => c.beat < beat && c.datamodel.Split(1) == "set object colors");
            
            if (bg != null) {
                BackgroundColor(bg.beat, bg.length, bg["presetBg"], bg["startColor"], bg["endColor"], bg["bgEase"], bg["shadowType"], bg["shadowStart"], bg["shadowEnd"]);
                SetBgFx(bg.beat, bg.length, bg["fxType"], bg["textureType"], bg["autoColor"], bg["startTexture"], bg["endTexture"], bg["textureEase"]);
            }
            
            if (obj != null) {
                UpdateMaterialColour(obj["colorA"], obj["colorB"], obj["colorC"]);
            } else {
                UpdateMaterialColour(Color.white, new Color(0.81f, 0.81f, 0.81f), Color.white);
            }

            // init modifier(s)
            RiqEntity bop = prevEntities.FindLast(c => c.beat < beat && c.datamodel.Split(1) == "bop");
            if (bop != null) ToggleBop(0, 0, false, bop["toggle"]);
            else ToggleBop(0, 0, false, true);

            // get all entities to later check against eachother to cut out voices
            voiceEntities = prevEntities.FindAll(c => c.beat > beat && (c.datamodel.Split(1) is "kick" or "combo"));
            hitVoiceEntities = prevEntities.FindAll(c => c.beat > beat && (c.datamodel.Split(1) is "warnings" && c["whichWarning"] <= (int)HitThree.HitFour));
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        private void Start()
        {
            GameCamera.additionalPosition = cameraPosition - GameCamera.defaultPosition;
            bgEffectAnimator = BGEffect.GetComponent<Animator>();
            bgEffectSpriteRenderer = BGEffect.GetComponent<SpriteRenderer>();

            bgGradientRenderer = BGGradient.GetComponent<SpriteRenderer>();
            bgBloodRenderer = BGBlood.GetComponent<SpriteRenderer>();
            bgRadialRenderer = BGRadial.GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            
            switch (currentBgEffect)
            {
                case (int) BackgroundFXType.Sunburst:
                    bgEffectAnimator.DoNormalizedAnimation("Sunburst", (cond.songPositionInBeats * 0.5f) % 1f);
                    break;
                case (int) BackgroundFXType.Rings:
                    bgEffectAnimator.DoNormalizedAnimation("Rings", (cond.songPositionInBeats * 0.5f) % 1f);
                    break;
                default:
                    bgEffectAnimator.Play("NoPose", -1, 0);
                    break;
            }

            if (cond.songPositionInBeatsAsDouble >= wordClearTime)
            {
                Word.Play("NoPose");
            }

            if (cond.songPositionInBeatsAsDouble >= startCamSpecial && cond.songPositionInBeatsAsDouble <= wantsReturn)
            {
                float camX = 0f;
                float camY = 0f;
                float camZ = 0f;
                if (cond.songPositionInBeatsAsDouble <= startCamSpecial + cameraReturnLength)
                {
                    float prog = cond.GetPositionFromBeat(startCamSpecial, cameraReturnLength);
                    camX = Util.EasingFunction.EaseOutCubic(CameraPosition[0].position.x, CameraPosition[1].position.x, prog);
                    camY = Util.EasingFunction.EaseOutCubic(CameraPosition[0].position.y, CameraPosition[1].position.y, prog);
                    camZ = Util.EasingFunction.EaseOutCubic(CameraPosition[0].position.z, CameraPosition[1].position.z, prog);
                    cameraPosition = new Vector3(camX, camY, camZ);
                }
                else if (cond.songPositionInBeatsAsDouble >= wantsReturn - cameraReturnLength)
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

            //BackgroundColorUpdate();
            
            GameCamera.additionalPosition = cameraPosition - GameCamera.defaultPosition;
            BGEffect.transform.position = new Vector3(GameCamera.instance.transform.position.x, GameCamera.instance.transform.position.y, 0);
        }

        private void OnDestroy()
        {
            if (!Conductor.instance.NotStopped()) {
                if (queuedObjects.Count > 0) queuedObjects.Clear();
                if (queuedBulbs.Count > 0) queuedBulbs.Clear();
                if (queuedKicks.Count > 0) queuedKicks.Clear();
                if (queuedCombos.Count > 0) queuedCombos.Clear();
            }
        }

        static List<RiqEntity> allHits = new List<RiqEntity>();
        static List<RiqEntity> allEnds = new List<RiqEntity>();
        public static int CountHitsToEnd(double fromBeat)
        {
            allHits = EventCaller.GetAllInGameManagerList("karateMan", new string[] { "hit", "bulb", "kick", "combo" });
            allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" });

            allHits.Sort((x, y) => x.beat.CompareTo(y.beat));
            allEnds.Sort((x, y) => x.beat.CompareTo(y.beat));
            double endBeat = double.MaxValue;

            //get the beat of the closest end event
            foreach (var end in allEnds)
            {
                if (end.beat > fromBeat)
                {
                    endBeat = end.beat;
                    break;
                }
            }

            //count each hit event beginning from our current beat to the beat of the closest game switch or end
            int count = 0;
            string type;
            for (int i = 0; i < allHits.Count; i++)
            {
                RiqEntity h = allHits[i];
                if (h.beat >= fromBeat)
                {
                    if (h.beat < endBeat) {
                        //kicks and combos count for 2 hits
                        type = h.datamodel.Split('/')[1];
                        count += (type is "kick" or "combo") ? 2 : 1;
                    } else {
                        break;
                    }
                }
            }
            return count;
        }

        public static void DoSpecialCamera(double beat, float length, bool returns)
        {
            if (cameraAngle == CameraAngle.Normal)
            {
                startCamSpecial = beat;
                cameraAngle = CameraAngle.Special;
            }
            wantsReturn = returns ? beat + length - 0.001f : double.MaxValue;
            cameraReturnLength = Mathf.Min(2f, length*0.5f);
        }

        public void DoWord(double beat, double length, int type, bool pitchVoice, float forcePitch, bool customLength, bool doSound = true)
        {
            Word.Play(DoWordSound(beat, length, type, pitchVoice, forcePitch, customLength, doSound));
        }

        public static void QueueWord(double beat, double length, int type, bool pitchVoice, float forcePitch, bool customLength, bool doSound = true)
        {
            DoWordSound(beat, length, type, pitchVoice, forcePitch, customLength, doSound);
        }

        public static string DoWordSound(double beat, double length, int type, bool pitchVoice, float forcePitch, bool customLength, bool doSound = true)
        {
            string word = $"Word0{(type < (int)HitThree.HitThreeAlt ? type : type - 1)}";
            double clear = type switch {
                <= (int)HitThree.HitFour => beat + 4f,
                <= (int)HitThree.Warning => beat + 1f,
                _ => beat + 3f,
            };

            if (type <= (int)HitThree.HitFour)
            {
                string number = ((HitThree)type).ToString()[3..];
                number = char.ToLower(number[0]).ToString() + number[1..];
                var sounds = new MultiSound.Sound[] {
                    new MultiSound.Sound($"karateMan/{(type == (int)HitThree.HitThreeAlt ? "hitAlt" : "hit")}", beat + 0.5f, offset: hitVoiceOffset),
                    new MultiSound.Sound($"karateMan/{number}", beat + 1f),
                };
                if (pitchVoice) {
                    foreach (var sound in sounds) {
                        sound.pitch = (forcePitch == 1) ? Conductor.instance.GetBpmAtBeat(sound.beat) / 125 : forcePitch;
                    }
                }
                MultiSound.Play(sounds, forcePlay: true);
            }

            var songPos = Conductor.instance.songPositionInBeatsAsDouble;
            if (songPos <= clear && songPos >= beat) {
                wordClearTime = customLength ? (beat + length) : clear;
            }
            return word;
        }

        public static void CreateItemSFX(double beat, int type, bool muteSound = false)
        {
            if (!muteSound) SoundByte.PlayOneShotGame($"karateMan/{(beat % 1.0 == 0.5 ? $"offbeatObject" : "object")}Out", forcePlay: true);
        }

        public static void CreateBulbSFX(double beat, int type, bool muteSound = false)
        {
            string obj = type switch {
                (int)LightBulbType.Normal => "LightbulbAgb",
                (int)LightBulbType.Yellow => "LightbulbNtr",
                _ => "Lightbulb",
            };
            Debug.Log(obj);
            if (!muteSound) SoundByte.PlayOneShotGame($"karateMan/{(beat % 1.0 == 0.5 ? $"offbeat{obj}" : obj.ToLower())}Out", forcePlay: true);
        }

        public static void QueueItem(double beat, int type, int expression)
        {
            queuedObjects.Add(new QueuedObject() {
                beat = beat,
                type = type,
                expression = expression,
            });
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
                    mobj.GetComponent<KarateManPot>().SetBulbColor(LightBulbColors[0]);
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

        public static void QueueBulb(double beat, int type, Color color, int expression)
        {
            queuedBulbs.Add(new QueuedObject() {
                beat = beat,
                type = type,
                color = color,
                expression = expression,
            });
        }

        public void CreateBulbSpecial(double beat, int type, Color c, int expression)
        {
            string obj = type switch {
                (int)LightBulbType.Normal => "LightbulbAgb",
                (int)LightBulbType.Yellow => "LightbulbNtr",
                _ => "Lightbulb",
            };
            var mobj = CreateItemInstance(beat, "Item01", expression, KarateManPot.ItemType.Bulb, hitSfxOverride: $"karateMan/{obj}Hit");

            if (type == (int) LightBulbType.Custom) {
                mobj.GetComponent<KarateManPot>().SetBulbColor(c);
            } else {
                mobj.GetComponent<KarateManPot>().SetBulbColor(LightBulbColors[type]);
            }
        }

        public static void ComboSFX()
        {
            SoundByte.PlayOneShotGame("karateMan/barrelOutCombos", forcePlay: true);
        }

        public static void QueueCombo(double beat, int expression, bool pitchVoice, float forcePitch, bool cutOut, bool noVoice)
        {
            queuedCombos.Add(new QueuedSpecial() {
                beat = beat,
                expression = expression,
                pitchVoice = pitchVoice,
                forcePitch = forcePitch,
                cutOut = cutOut,
                noVoice = noVoice
            });
        }

        public void Combo(double beat, int expression, bool pitchVoice, float forcePitch, bool cutOut, bool noVoice)
        {
            int comboId = KarateManPot.GetNewCombo();

            BeatAction.New(gameObject, new List<BeatAction.Action>() 
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
                new MultiSound.Sound("karateMan/punchy1", beat + 1f),
                new MultiSound.Sound("karateMan/punchy2", beat + 1.25f),
                new MultiSound.Sound("karateMan/punchy3", beat + 1.5f),
                new MultiSound.Sound("karateMan/punchy4", beat + 1.75f),
                new MultiSound.Sound("karateMan/ko", beat + 2f),
                new MultiSound.Sound("karateMan/pow", beat + 2.5f)
            };

            if (pitchVoice) {
                foreach (var sound in sounds) {
                    sound.pitch = (forcePitch == 1) ? Conductor.instance.GetBpmAtBeat(sound.beat) / 125 : forcePitch;
                }
            }

            if (voiceEntities.Count > 0 && cutOut)
            {
                RiqEntity firstVoice = voiceEntities.Find(x => x.beat >= beat + 1);
                RiqEntity firstHitVoice = hitVoiceEntities.Find(x => x.beat >= beat + 1);
                if (firstVoice != null) sounds.RemoveAll(x => x.beat > firstVoice.beat);
                if (firstHitVoice != null) sounds.RemoveAll(x => x.beat > firstHitVoice.beat - 0.5);
            }
            
            MultiSound.Play(sounds.ToArray(), forcePlay: true);
        }

        public static void KickSFX()
        {
            SoundByte.PlayOneShotGame("karateMan/barrelOutKicks", forcePlay: true);
        }

        public static void QueueKick(double beat, bool ball, int expression, bool pitchVoice, float forcePitch, bool cutOut, bool noVoice)
        {
            queuedKicks.Add(new QueuedSpecial() {
                beat = beat,
                expression = expression,
                pitchVoice = pitchVoice,
                forcePitch = forcePitch,
                cutOut = cutOut,
                noVoice = noVoice,
                ball = ball,
            });
        }

        public void Kick(double beat, bool ball, int expression, bool pitchVoice, float forcePitch, bool cutOut, bool noVoice)
        {
            CreateItemInstance(beat, "Item05", expression, KarateManPot.ItemType.KickBarrel, content: ball);

            if (noVoice) return;

            List<MultiSound.Sound> sounds = new() {
                new MultiSound.Sound("karateMan/punchKick1", beat + 1f),
                new MultiSound.Sound("karateMan/punchKick2", beat + 1.5f),
                new MultiSound.Sound("karateMan/punchKick3", beat + 1.75f),
                new MultiSound.Sound("karateMan/punchKick4", beat + 2.5f),
            };

            if (pitchVoice) {
                foreach (var sound in sounds) {
                    sound.pitch = (forcePitch == 1) ? Conductor.instance.GetBpmAtBeat(sound.beat) / 125 : forcePitch;
                }
            }

            if (voiceEntities.Count > 0 && cutOut)
            {
                RiqEntity firstVoice = voiceEntities.Find(x => x.beat >= beat + 1);
                RiqEntity firstHitVoice = hitVoiceEntities.Find(x => x.beat >= beat + 1);
                if (firstVoice != null) sounds.RemoveAll(x => x.beat > firstVoice.beat);
                if (firstHitVoice != null) sounds.RemoveAll(x => x.beat > firstHitVoice.beat);
            }
            
            MultiSound.Play(sounds.ToArray(), forcePlay: true);
        }

        public GameObject CreateItemInstance(double beat, string awakeAnim, int successExpression, KarateManPot.ItemType type = KarateManPot.ItemType.Pot, int comboId = -1, bool content = false, string hitSfxOverride = null)
        {
            GameObject mobj = GameObject.Instantiate(Item, ItemHolder);
            KarateManPot mobjDat = mobj.GetComponent<KarateManPot>();
            mobjDat.type = type;
            mobjDat.startBeat = beat;
            mobjDat.awakeAnim = awakeAnim;
            mobjDat.comboId = comboId;
            mobjDat.OnHitExpression = successExpression;
            mobjDat.KickBarrelContent = content;
            mobjDat.hitSfxOverride = hitSfxOverride;

            mobj.SetActive(true);
            
            return mobj;
        }

        public void BackgroundColor(double beat, float length, int presetBG, Color colorStart, Color colorEnd, int colorEaseSet, int shadowType, Color shadowStart, Color shadowEnd)
        {
            if (presetBG != (int)BackgroundType.Custom) {
                colorEnd = BackgroundColors[presetBG];
            }

            colorStartBeats[0] = beat;
            colorLengths[0] = length;
            colorStarts[0] = colorStart;
            colorEnds[0] = colorEnd;
            colorEases[0] = (Util.EasingFunction.Ease)colorEaseSet;

            BackgroundColorUpdate();
        }

        public void SetBgFx(double beat, float length, int fxType, int textureType, bool autoColor, Color filterStartSet, Color filterEndSet, int ease)
        {
            colorStarts[2] = filterStartSet;
            colorEnds[2] = filterEndSet;

            BGBlood.SetActive(fxType == (int)BackgroundTextureType.Blood);
            BGGradient.SetActive(fxType == (int)BackgroundTextureType.Gradient);
            BGRadial.SetActive(fxType == (int)BackgroundTextureType.Radial);
        }

        private void BackgroundColorUpdate()
        {
            float[,] newRgb = new float[3, 3];

            for (int i = 0; i < newRgb.GetLength(0); i++) 
            {
                float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeats[i], colorLengths[i]));
                var func = Util.EasingFunction.GetEasingFunction(colorEases[i]);
                
                for (int j = 0; j < newRgb.GetLength(1); j++) 
                {
                    float Hue(Color[] whichColors) => j switch {
                        0 => whichColors[i].r,
                        1 => whichColors[i].g,
                        _ => whichColors[i].b,
                    };
                    newRgb[j, i] = func(Hue(colorStarts), Hue(colorEnds), normalizedBeat);
                }
            }

            BGPlane.color = new Color(newRgb[0, 0], newRgb[0, 1], newRgb[0, 2]);

            bgGradientRenderer.color =
            bgBloodRenderer.color =
            bgRadialRenderer.color = new Color(newRgb[1, 0], newRgb[1, 1], newRgb[1, 2]);
        }

        public void SetGameplayMods(double beat, int mode, bool combo)
        {
            NoriGO.SetActive(true);
            Nori.SetNoriMode(beat, mode);
            IsComboEnable = combo;
        }


        public static void UpdateMaterialColour(Color mainCol, Color highlightCol, Color objectCol)
        {
            BodyColor = mainCol;
            HighlightColor = highlightCol;
            ItemColor = objectCol;
        }

        public void SetParticleEffect(double beat, int type, float windStrength, float particleStrength)
        {
            ParticleSystem.EmissionModule emm;
            switch (type)
            {
                case (int) ParticleType.Snow:
                    SnowEffectGO.SetActive(true);
                    SnowEffect.Play();
                    emm = SnowEffect.emission;
                    emm.rateOverTime = particleStrength * 6f;
                    break;
                case (int) ParticleType.Fire:
                    FireEffectGO.SetActive(true);
                    FireEffect.Play();
                    emm = FireEffect.emission;
                    emm.rateOverTime = particleStrength * 6f;
                    break;
                case (int) ParticleType.Rain:
                    RainEffectGO.SetActive(true);
                    RainEffect.Play();
                    emm = RainEffect.emission;
                    emm.rateOverTime = particleStrength * 32f;
                    break;
                default:
                    SnowEffect.Stop();
                    FireEffect.Stop();
                    RainEffect.Stop();
                    break;
            }
            Wind.windMain = windStrength;
        }

        public void ToggleBop(double beat, float length, bool toggle, bool autoBop)
        {
            if (autoBop)
                Joe.bop.length = float.MaxValue;
            else
                Joe.bop.length = 0;
            if (toggle)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            Joe.Bop();
                        })
                    });
                }
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