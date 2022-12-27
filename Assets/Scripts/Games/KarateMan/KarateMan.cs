using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlNewKarateLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("karateman", "Karate Man", "70A8D8", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.ToggleBop(e["toggle"], e.length); },
                    defaultLength = 0.5f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Whether to bop to the beat or not")
                    },
                    inactiveFunction = delegate { KarateMan.ToggleBopUnloaded(eventCaller.currentEntity["toggle"]); }
                },
                new GameAction("hit", "Toss Object") {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.CreateItem(e.beat, e["type"], e["type2"], e["type3"], e["type4"]); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.HitType.Pot, "Object", "The object to fire"),
                        new Param("type2", KarateMan.KarateManFaces.Normal, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("type3", KarateMan.JoeFaceLength.Normal, "Face Keep Length", "How long Joe will keep his face from success"),
                        new Param("type4", KarateMan.ForceObjectSound.None, "Force Sound", "Make the object play the specific sound no matter what")
                    }
                },
                new GameAction("bulb", "Toss Lightbulb")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.CreateBulbSpecial(e.beat, e["type"], e["colorA"], e["type2"], e["type3"], e["type4"]); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.LightBulbType.Normal, "Type", "The preset bulb type. Yellow is used for kicks while Blue is used for combos"),
                        new Param("colorA", new Color(1f,1f,1f), "Custom Color", "The color to use when the bulb type is set to Custom"),
                        new Param("type2", KarateMan.KarateManFaces.Normal, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("type3", KarateMan.JoeFaceLength.Normal, "Face Keep Length", "How long Joe will keep his face from success"),
                        new Param("type4", KarateMan.ForceObjectSound.None, "Force Sound", "Make the object play the specific sound no matter what")
                    },
                },
                new GameAction("kick", "Special: Kick")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.Kick(e.beat, e["toggle"], e["type"], e["type2"], e["toggle2"], e["type3"]); }, 
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Contains Ball", "Barrel contains a ball instead of a bomb?"),
                        new Param("type", KarateMan.KarateManFaces.Smirk, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("type2", KarateMan.JoeFaceLength.Normal, "Face Keep Length", "How long Joe will keep his face from success"),
                        new Param("toggle2", false, "Pitch Shift", "Make the voice pitch shift like in DS"),
                        new Param("type3", KarateMan.SpecialSoundTypes.SpecialAndVoice, "Voice Type", "Change how the voice will play with the Punch KEEK")
                    }
                },
                new GameAction("combo", "Special: Combo")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.Combo(e.beat, e["type"], e["type2"], e["toggle"], e["type3"]); }, 
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.KarateManFaces.Happy, "Success Expression", "The facial expression to set Joe to on hit"),
                        new Param("type2", KarateMan.JoeFaceLength.Normal, "Face Keep Length", "How long Joe will keep his face from success"),
                        new Param("toggle", false, "Pitch Shift", "Change the voice pitch based on BPM like on DS"),
                        new Param("type3", KarateMan.SpecialSoundTypes.SpecialAndVoice, "Voice Type", "Change how the voice will play with the Combo!")
                    }
                },
                new GameAction("honki", "Serious Chance")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.Honki(e.beat, e.length, e["type"], e["toggle"], e["toggle2"], e["type2"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.HonkiChanceType.OneObject, "Serious Chance Type", "Whether all objects, one object or no object is required for Serious Mode"),
                        new Param("toggle", false, "Deactivate Serious", "This deactivates Serious Mode"),
                        new Param("toggle2", false, "Miss can Lose", "This makes it so if you miss too many times, Serious Mode deactivates"),
                        new Param("type2", KarateMan.HonkiSoundType.SeriousHit, "Sound", "Sound that plays on Switch"),
                    }
                },
                new GameAction("hitX", "Warnings")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.DoWord(e.beat, e["type"], e["type2"], e["type3"], e["toggle"], e["type4"], e["type5"], e["toggle2"]); }, 
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.HitThree.HitThree, "Type", "The warning text to show"),
                        new Param("type2", KarateMan.HitThreeVoice.Megamix3DS, "Voice Clip", "If the voice clip plays or not"),
                        new Param("type3", KarateMan.HitThreeGraphicLang.Japanese, "Language", "The language the graphic will show in"),
                        new Param("toggle", false, "Pitch Shift", "Makes the voice shift pitch and speed like in DS"),
                        new Param("type4", KarateMan.HitThreeLength.None, "Length Override", "Overrides the standered length of the graphic appearing"),
                        new Param("type5", KarateMan.HitThreeVoiceDelay.OneBeat, "Voice Delay", "If the voice is delayed by 1 or half a beat"),
                        new Param("toggle2", false, "Fast Voice", "Makes the voice faster")
                    },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; KarateMan.DoWordSound(e.beat, e["type"], true, false, (int) KarateMan.HitThreeGraphicLang.Japanese, e["toggle"], (int) KarateMan.HitThreeLength.None, e["type5"], e["toggle2"]); }
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
                    resizable = true
                },
                new GameAction("set gameplay modifiers", "Gameplay Modifiers")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetGameplayMods(e.beat, e["type"], e["type2"], e["toggle"], e["toggle2"], e["toggle3"], e["type3"], 1f); },//e["valA"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.NoriMode.None, "Flow Bar type", "The type of Flow bar to use"),
                        new Param("type2", KarateMan.HighNoriBg.None, "Flow Background", "The background type when you reach high flow"),
                        new Param("toggle", true, "Enable Combos", "Allow the player to combo? (Contextual combos will still be allowed even when off)"),
                        new Param("toggle2", false, "Pot Break Sound", "Should there be a breaking sound when punching an object with high flow?"),
                        new Param("toggle3", false, "High Flow Punch", "Determines if Joe punches with the right hand when having high flow"),
                        new Param("type3", KarateMan.SoundEffectTypes.Megamix3DS, "Sound Effect Ver", "The version of sounds that will play"),
                        //new Param("valA", new EntityTypes.Float(0f, 10f, 1f), "Pot Break Delay", "Sets the pot break delay, used in Tengoku Arcade"),
                    }
                },
                new GameAction("set background effects", "Background Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetBgAndShadowCol(e.beat, e.length, e["type"], e["type2"], e["colorA"], e["colorB"], e["type3"]); KarateMan.instance.SetBgTexture(e["type4"], e["type5"], e["colorC"], e["colorD"]); }, 
                    defaultLength = 0.5f, 
                    resizable = true, 
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
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; KarateMan.SetBgEffectsUnloaded(e.beat, e.length, e["type"], e["type2"], e["colorA"], e["colorB"], e["type3"], e["type4"], e["type5"], e["colorC"], e["colorD"]); }
                },
                new GameAction("set object colors", "Object Colors")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.UpdateMaterialColour(e["colorA"], e["colorB"], e["colorC"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", new Color(1,1,1,1), "Joe Body Color", "The color to use for Karate Joe's body"),
                        new Param("colorB", new Color(0.81f,0.81f,0.81f,1), "Joe Highlight Color", "The color to use for Karate Joe's highlights"),
                        new Param("colorC", new Color(1,1,1,1), "Item Color", "The color to use for the thrown items"),
                    },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; KarateMan.UpdateMaterialColour(e["colorA"], e["colorB"], e["colorC"]); }
                },
                new GameAction("particle effects", "Particle Effects")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetParticleEffect(e.beat, e["type"], e["valA"], e["valB"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.ParticleType.None, "Particle Type", "The type of particle effect to spawn. Using \"None\" will stop all effects"),
                        new Param("valA", new EntityTypes.Float(0f, 64f, 1f), "Wind Strength", "The strength of the particle wind"),
                        new Param("valB", new EntityTypes.Float(1f, 16f, 1f), "Particle Intensity", "The intensity of the particle effect")
                    }
                },
                new GameAction("force facial expression", "Set Facial Expression")
                {
                    function = delegate { KarateMan.instance.SetFaceExpression(eventCaller.currentEntity["type"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.KarateManFaces.Normal, "Facial Expression", "The facial expression to force Joe to. Special moves may override this")
                    }
                },

                // These are still here for backwards-compatibility but are hidden in the editor
                new GameAction("pot", "")
                {
                    function = delegate { KarateMan.instance.CreateItem(eventCaller.currentEntity.beat, 0, (int) KarateMan.HitType.Pot, 0, 0); }, 
                    defaultLength = 2, 
                    hidden = true
                },
                new GameAction("rock", "")
                {
                    function = delegate { KarateMan.instance.CreateItem(eventCaller.currentEntity.beat, 0, (int) KarateMan.HitType.Rock, 0, 0); }, 
                    defaultLength = 2, 
                    hidden = true
                },
                new GameAction("ball", "")
                {
                    function = delegate { KarateMan.instance.CreateItem(eventCaller.currentEntity.beat, 0, (int) KarateMan.HitType.Ball, 0, 0); }, 
                    defaultLength = 2, 
                    hidden = true
                },
                new GameAction("tacobell", "")
                {
                    function = delegate { KarateMan.instance.CreateItem(eventCaller.currentEntity.beat, 0, (int) KarateMan.HitType.TacoBell, 0, 0); }, 
                    defaultLength = 2, 
                    hidden = true
                },
                new GameAction("bgfxon", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetBgFx((int) KarateMan.BackgroundFXType.Sunburst, e.beat, e.length); }, 
                    hidden = true
                },
                new GameAction("bgfxoff", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetBgFx((int) KarateMan.BackgroundFXType.None, e.beat, e.length); }, 
                    hidden = true
                },
                new GameAction("hit3", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.DoWord(e.beat, e["type"], (int) KarateMan.HitThreeVoice.Megamix3DS, (int) KarateMan.HitThreeGraphicLang.Japanese, false, (int) KarateMan.HitThreeLength.None, (int) KarateMan.HitThreeVoiceDelay.OneBeat, false); },
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.HitThree.HitThree, "Type", "The warning text to show")
                    }, 
                    hidden = true
                },
                new GameAction("hit4", "")
                {
                    function = delegate { KarateMan.instance.DoWord(eventCaller.currentEntity.beat, (int) KarateMan.HitThree.HitFour, (int) KarateMan.HitThreeVoice.Megamix3DS, (int) KarateMan.HitThreeGraphicLang.Japanese, false, (int) KarateMan.HitThreeLength.None, (int) KarateMan.HitThreeVoiceDelay.OneBeat, false); },
                    hidden = true
                },
                new GameAction("set background color", "")
                {

                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetBgAndShadowCol(e.beat, e.length, e["type"], e["type2"], e["colorA"], e["colorB"], (int) KarateMan.currentBgEffect); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.BackgroundType.Yellow, "Background Type", "The preset background type"),
                        new Param("type2", KarateMan.ShadowType.Tinted, "Shadow Type", "The shadow type. If Tinted doesn't work with your background color try Custom"),
                        new Param("colorA", new Color(), "Custom Background Color", "The background color to use when background type is set to Custom"),
                        new Param("colorB", new Color(), "Custom Shadow Color", "The shadow color to use when shadow type is set to Custom"),

                    },
                    hidden = true
                },
                new GameAction("set background fx", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetBgFx(e["type"], e.beat, e.length); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.BackgroundFXType.None, "FX Type", "The background effect to be displayed")
                    },
                    hidden = true
                },

                new GameAction("set background texture", "")
                {
                    function = delegate { var e = eventCaller.currentEntity; KarateMan.instance.SetBgTexture(e["type"], e["type2"], e["colorA"], e["colorB"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", KarateMan.BackgroundTextureType.Plain, "Texture", "The type of background texture to use"),
                        new Param("type2", KarateMan.ShadowType.Tinted, "Color Filter Type", "The method used to apply colour to the texture"),
                        new Param("colorA", new Color(), "Custom Filter Color", "The filter color to use when color filter type is set to Custom"),
                        new Param("colorB", new Color(), "Fading Filter Color", "When using the Fade background effect, make filter colour fade to this colour"),
                    },
                    hidden = true
                },
            },
            new List<string>() {"agb", "ntr", "rvl", "ctr", "pco", "normal"},
            "karate", "jp",
            new List<string>() {"en", "jp", "ko"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using HeavenStudio.Editor;
    using Scripts_KarateMan;
    using System.Windows.Forms.VisualStyles;
    using UnityEditor;
    using static HeavenStudio.Util.MultiSound;

    public class KarateMan : Minigame
    {
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
            HitTwo,
            HitThree,
            HitThreeAlt,
            HitFour,
            Grr,
            Warning,
            Combo,
            HitOne,
            Clear,
        }

        public enum HitThreeVoice
        {
            Megamix3DS,
            GoldDS,
            TengokuGBA
        }

        public enum HitThreeGraphicLang
        {
            English,
            Japanese,
            Korean
        }

        public enum HitThreeLength
        {
            None,
            HalfBeat,
            OneBeat,
            TwoBeats,
            ThreeBeats,
            FourBeats,
            Indefinite
        }

        public enum HitThreeVoiceDelay
        {
            OneBeat,
            HalfBeat
        }

        public enum HonkiChanceType
        {
            OneObject,
            AllObjects,
            IgnoreObject
        }
        
        public enum HonkiSoundType
        {
            SeriousHit,
            SeriousTransition,
            None
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
        
        public enum JoeFaceLength
        {
            Normal,
            Kick,
            Combo,
            Infinite,
            OneBeat
        }

        public enum ForceObjectSound
        {
            None,
            Onbeat,
            Offbeat
        }

        public enum SoundEffectTypes
        {
            Megamix3DS,
            GoldDS,
            TengokuGBA
        }

        public enum SpecialSoundTypes
        {
            SpecialAndVoice,
            VoiceOnly,
            NoVoice
        }

        public enum NoriMode
        {
            None,
            Tengoku,
            Mania,
        }

        public enum HighNoriBg
        {
            None = 0,
            Sunburst = 1,
            Rings = 2,
        }

        public static bool IsComboEnable = true;   //only stops Out combo inputs, this basically makes combo contextual
        public static bool NoriBreakSound = false; //the sound when punching an object with high nori, off by default
        public static bool HighFlowPunch = false; //this determines if Joe always punches heavy with high flow, off by default cause it looks odd
        public static bool HonkiMode = false;
        public bool IsNoriActive { get { return Nori.MaxNori > 0; } }
        public float NoriPerformance { get { if (IsNoriActive) return Nori.Nori / Nori.MaxNori; else return 1f; } }
        public float HonkiChanceLength;
        public float HonkiChanceStart;

        public Color[] LightBulbColors;
        public Color[] BackgroundColors;
        public Color[] ShadowColors;

        //camera positions (normal, special)
        [Header("Camera Positions")]
        public Transform[] CameraPosition;
        Vector3 cameraPosition;
        static float startCamSpecial = Single.MinValue;
        static float wantsReturn = Single.MinValue;
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
        public static Color HighlightColor = Color.white;
        public static Color ItemColor = Color.white;

        [Header("Word")]
        public Animator Word;
        static float wordClearTime = Single.MinValue;
        const float hitVoiceOffset = 0.042f;

        [Header("Backgrounds")]
        static int bgType = (int) BackgroundType.Yellow;
        public static int currentBgEffect = (int) BackgroundFXType.None;
        static float bgFadeTime = Single.MinValue;
        static float bgFadeDuration = 0f;
        static Color bgColour = Color.white;
        public SpriteRenderer BGPlane;
        public GameObject BGEffect;
        Color bgColourLast;

        Animator bgEffectAnimator;
        SpriteRenderer bgEffectSpriteRenderer;

        static int textureType = (int) BackgroundTextureType.Plain;
        static int textureFilterType = (int) ShadowType.Tinted;
        static int highNoriBackground = 0;
        static Color filterColour = Color.white;
        static Color filterColourNext = Color.white;
        public GameObject BGGradient;
        SpriteRenderer bgGradientRenderer;
        public GameObject BGBlood;
        SpriteRenderer bgBloodRenderer;
        public GameObject BGRadial;
        SpriteRenderer bgRadialRenderer;
        public GameObject BGHonki;
        SpriteRenderer bgHonkiRenderer;

        [Header("Shadows")]
        static int currentShadowType = (int) ShadowType.Tinted;
        static Color customShadowColour = Color.white;
        static Color oldShadowColour;

        [Header("Particles")]
            //wind
        public WindZone Wind;
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
        //public static Queue<Beatmap.Entity> ItemQueue = new Queue<Beatmap.Entity>();
        public static bool WantBop = true;
        public static bool WantNori = true;
        public static int WantNoriType = (int) NoriMode.None;
        public static int SoundEffectsVersion = (int) SoundEffectTypes.Megamix3DS;
        public static float WantBgChangeStart = Single.MinValue;
        public static float WantBgChangeLength = 0f;
        public static float BopLength = 1f;
        public static float PotBreakDelay = 1f;

        private void Awake()
        {
            instance = this;
            KarateManPot.ResetLastCombo();
            cameraPosition = CameraPosition[0].position;
        }

        public override void OnPlay(float beat)
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying)
            {
                SetBgEffectsToLast(beat);
                // remove all children of the ItemHolder
                foreach (Transform child in ItemHolder)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void Start()
        {
            var cond = Conductor.instance;
            
            GameCamera.additionalPosition = cameraPosition - GameCamera.defaultPosition;
            bgEffectAnimator = BGEffect.GetComponent<Animator>();
            bgEffectSpriteRenderer = BGEffect.GetComponent<SpriteRenderer>();

            bgGradientRenderer = BGGradient.GetComponent<SpriteRenderer>();
            bgBloodRenderer = BGBlood.GetComponent<SpriteRenderer>();
            bgRadialRenderer = BGRadial.GetComponent<SpriteRenderer>();
            bgHonkiRenderer = BGHonki.GetComponent<SpriteRenderer>();

            SetBgEffectsToLast(cond.songPositionInBeats);
            SetBgAndShadowCol(WantBgChangeStart, WantBgChangeLength, bgType, (int) currentShadowType, bgColour, customShadowColour, (int)currentBgEffect);
            SetBgTexture(textureType, textureFilterType, filterColour, filterColour);
            UpdateMaterialColour(BodyColor, HighlightColor, ItemColor);
            ToggleBop(WantBop, 1f);
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying)
                SetBgEffectsToLast(cond.songPositionInBeats);

            switch (currentBgEffect)
            {
                case (int)BackgroundFXType.Sunburst:
                    bgEffectAnimator.DoNormalizedAnimation("Sunburst", (cond.songPositionInBeats * 0.5f) % 1f);
                    break;
                case (int)BackgroundFXType.Rings:
                    bgEffectAnimator.DoNormalizedAnimation("Rings", (cond.songPositionInBeats * 0.5f) % 1f);
                    break;
                default:
                    bgEffectAnimator.Play("NoPose", -1, 0);
                break;
            }


            if (cond.songPositionInBeats >= wordClearTime)
            {
                Word.Play("NoPose");
            }

            if (cond.songPositionInBeats >= startCamSpecial && cond.songPositionInBeats <= wantsReturn)
            {
                float camX = 0f;
                float camY = 0f;
                float camZ = 0f;
                if (cond.songPositionInBeats <= startCamSpecial + cameraReturnLength)
                {
                    float prog = cond.GetPositionFromBeat(startCamSpecial, cameraReturnLength);
                    camX = EasingFunction.EaseOutCubic(CameraPosition[0].position.x, CameraPosition[1].position.x, prog);
                    camY = EasingFunction.EaseOutCubic(CameraPosition[0].position.y, CameraPosition[1].position.y, prog);
                    camZ = EasingFunction.EaseOutCubic(CameraPosition[0].position.z, CameraPosition[1].position.z, prog);
                    cameraPosition = new Vector3(camX, camY, camZ);
                }
                else if (cond.songPositionInBeats >= wantsReturn - cameraReturnLength)
                {
                    float prog = cond.GetPositionFromBeat(wantsReturn - cameraReturnLength, cameraReturnLength);
                    camX = EasingFunction.EaseOutQuad(CameraPosition[1].position.x, CameraPosition[0].position.x, prog);
                    camY = EasingFunction.EaseOutQuad(CameraPosition[1].position.y, CameraPosition[0].position.y, prog);
                    camZ = EasingFunction.EaseOutQuad(CameraPosition[1].position.z, CameraPosition[0].position.z, prog);
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
            float fadeProg = cond.GetPositionFromBeat(bgFadeTime, bgFadeDuration);
            if (bgFadeTime != Single.MinValue && fadeProg >= 0)
            {
                if (fadeProg >= 1f)
                {
                    bgFadeTime = Single.MinValue;
                    bgFadeDuration = 0f;
                    BGPlane.color = bgColour;
                    filterColour = filterColourNext;
                    UpdateFilterColour(bgColour, filterColour);
                    oldShadowColour = GetShadowColor(true);
                }
                else
                {
                    Color col = Color.LerpUnclamped(bgColourLast, bgColour, fadeProg);
                    BGPlane.color = col;
                    UpdateFilterColour(col, Color.LerpUnclamped(filterColour, filterColourNext, fadeProg));
                }
            }
            if (IsNoriActive && NoriPerformance >= 0.6f && highNoriBackground != (int) HighNoriBg.None)
                currentBgEffect = highNoriBackground;
            else if (IsNoriActive && NoriPerformance < 0.6f && highNoriBackground != (int) HighNoriBg.None)
                currentBgEffect = (int) BackgroundFXType.None;

            GameCamera.additionalPosition = cameraPosition - GameCamera.defaultPosition;
            BGEffect.transform.position = new Vector3(GameCamera.instance.transform.position.x, GameCamera.instance.transform.position.y, 0);
        }

        static List<DynamicBeatmap.DynamicEntity> allHits = new List<DynamicBeatmap.DynamicEntity>();
        static List<DynamicBeatmap.DynamicEntity> allEnds = new List<DynamicBeatmap.DynamicEntity>();
        public static int CountHitsToEnd(float fromBeat)
        {
            allHits = EventCaller.GetAllInGameManagerList("karateman", new string[] { "hit", "bulb", "kick", "combo" });
            allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "switchGame", "end" });

            allHits.Sort((x, y) => x.beat.CompareTo(y.beat));
            allEnds.Sort((x, y) => x.beat.CompareTo(y.beat));
            float endBeat = Single.MaxValue;

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
            // this still counts hits even if they happen after a switch / end!!!
            int count = 0;
            string type;
            for (int i = 0; i < allHits.Count; i++)
            {
                var h = allHits[i];
                if (h.beat >= fromBeat)
                {
                    if (h.beat < endBeat)
                    {
                        //kicks and combos count for 2 hits
                        type = (h.datamodel.Split('/'))[1];
                        count += (type == "kick" || type == "combo" ? 2 : 1);
                    }
                    else
                        break;
                }
            }
            return count;
        }

        public static void DoSpecialCamera(float beat, float length, bool returns)
        {
            if (cameraAngle == CameraAngle.Normal)
            {
                startCamSpecial = beat;
                cameraAngle = CameraAngle.Special;
            }
            wantsReturn = returns ? beat + length - 0.001f : Single.MaxValue;
            cameraReturnLength = Mathf.Min(2f, length*0.5f);
        }

        public void Honki(float beat, float length, int type, bool deactivate, bool miss, int sound)
        {
            GameObject mobj = GameObject.Instantiate(Item, ItemHolder);
            KarateManPot mobjDat = mobj.GetComponent<KarateManPot>();
            HonkiChanceStart = beat;
            HonkiChanceLength = length;
            mobjDat.deactivateHonki = deactivate;
            mobjDat.honkiSound = sound;
            
            if (type == (int) HonkiChanceType.IgnoreObject)
                ActivateHonki(beat, deactivate, sound);
        }

        public void ActivateHonki(float beat, bool deactivate, int sound)
        {
            switch (sound)
            {
                case (int)HonkiSoundType.SeriousHit:
                    Jukebox.PlayOneShotGame("karateman/gogo");
                    break;
                case (int)HonkiSoundType.SeriousTransition:
                    Jukebox.PlayOneShotGame("karateman/gogoSwitch");
                    break;
                default:
                    break;
            }

            if (!deactivate)
            {
                HonkiMode = true;
                BGHonki.SetActive(true);
                BGGradient.SetActive(false);
                BGBlood.SetActive(false);
                BGRadial.SetActive(false);
                SetBgAndShadowCol(beat, 0f, (int)BackgroundType.Custom, (int)ShadowType.Custom, Color.white, Color.black, (int)BackgroundFXType.None);
                Nori.SetNoriMode(beat, (int)KarateMan.NoriMode.None);
                BodyColor = Color.black;
                HighlightColor = Color.white;
            }
            else
            {
                HonkiMode = false;
                BGHonki.SetActive(false);
            }
        }

        public void DoWord(float beat, int type, int voiceType, int language, bool pitch, int graphLength, int voiceDelay, bool fast)
        {
            bool sound;
            bool graphic;

            switch (voiceType)
            {
                case (int) KarateMan.HitThreeVoice.TengokuGBA:
                    sound = false;
                    graphic = true;
                    break;
                case (int) KarateMan.HitThreeVoice.GoldDS:
                    sound = true;
                    graphic = false;
                    break;
                case (int) KarateMan.HitThreeVoice.Megamix3DS:
                    sound = true;
                    graphic = true;
                    break;
                default:
                    sound = true;
                    graphic = true;
                    break;
            }

            Word.Play(DoWordSound(beat, type, sound, graphic, language, pitch, graphLength, voiceDelay, fast));
        }

        static float HitThreeGraphicLengthFloat(int type)
        {
            switch (type)
            {
                case (int) HitThreeLength.HalfBeat:
                    return 0.5f;
                case (int) HitThreeLength.OneBeat:
                    return 1f;
                case (int) HitThreeLength.TwoBeats:
                    return 2f;
                case (int) HitThreeLength.ThreeBeats:
                    return 3f;
                case (int) HitThreeLength.FourBeats:
                    return 4f;
                case (int) HitThreeLength.Indefinite:
                    return float.MaxValue;
                default:
                    return 0f;
            }
        }

        static float HitThreeVoiceOfset(int delay)
        {
            switch (delay)
            {
                case (int) HitThreeVoiceDelay.HalfBeat:
                    return 1f;
                default:
                    return hitVoiceOffset;
            }
        }

        public static string DoWordSound(float beat, int type, bool doSound, bool graphic, int language, bool pitchShift, int graphLength, int voiceDelay, bool fast)
        {
            String word = "NoPose";
            float clear = 0f;
            float pitch = 1f;
            float numOffset = hitVoiceOffset;
            float hitOffset = 1f;
            float beatOffset = 0.5f;
            float startBeat = Conductor.instance.songPositionInBeats;
            if (pitchShift)
            { 
                pitch = Conductor.instance.songBpm / 128;
                hitOffset = 0.96f;
            }
            if (fast)
            {
                numOffset = 1f;
                hitOffset /= 2;
            }
            if (HitThreeVoiceOfset(voiceDelay) != hitVoiceOffset)
            {
                numOffset = 1f;
                hitOffset -= 0.42f;
            }
            switch (type)
            {
                case (int) HitThree.HitTwo:
                    if (graphic) word = "Word02";
                    clear = beat + 4f;
                    if (doSound)
                        MultiSound.Play(new MultiSound.Sound[] 
                        {
                            new MultiSound.Sound("karateman/hit", beat + hitOffset, pitch: pitch),
                        }, forcePlay: true);
                    break;
                case (int) HitThree.HitThree:
                    if (graphic) word = "Word03";
                    clear = beat + 4f;
                    if (doSound)
                        MultiSound.Play(new MultiSound.Sound[] 
                        {
                            new MultiSound.Sound("karateman/three", beat + beatOffset, pitch: pitch, offset: numOffset), 
                            new MultiSound.Sound("karateman/hit", beat + hitOffset, pitch: pitch),
                        }, forcePlay: true);
                    break;
                case (int) HitThree.HitThreeAlt:
                    if (graphic) word = "Word03";
                    clear = beat + 4f;
                    if (doSound)
                        MultiSound.Play(new MultiSound.Sound[] 
                        {
                            new MultiSound.Sound("karateman/threeAlt", beat + beatOffset, pitch: pitch, offset: numOffset), 
                            new MultiSound.Sound("karateman/hitAlt", beat + hitOffset, pitch: pitch),
                        }, forcePlay: true);
                    break;
                case (int) HitThree.HitFour:
                    if (graphic) word = "Word04";
                    clear = beat + 4f;
                    if (doSound)
                        MultiSound.Play(new MultiSound.Sound[] 
                        {
                            new MultiSound.Sound("karateman/four", beat + beatOffset, pitch: pitch, offset: numOffset), 
                            new MultiSound.Sound("karateman/hit", beat + hitOffset, pitch: pitch),
                        }, forcePlay: true);
                    break;
                case (int) HitThree.Grr:
                    word = "Word01";
                    clear = beat + 1f;
                    break;
                case (int) HitThree.Warning:
                    word = "Word05";
                    clear = beat + 1f;
                    break;
                case (int) HitThree.Combo:
                    word = "Word00";
                    clear = beat + 3f;
                    break;
                case (int) HitThree.HitOne: //really?
                    if (graphic) word = "Word06";
                    clear = beat + 4f;
                    if (doSound)
                        MultiSound.Play(new MultiSound.Sound[] 
                        {
                            new MultiSound.Sound("karateman/hit", beat + hitOffset, pitch: pitch, offset: numOffset)
                        }, forcePlay: true);
                    break;
                case (int) HitThree.Clear:
                    word = "NoPose";
                    clear = 0f;
                    break;
            }
            if (HitThreeGraphicLengthFloat(graphLength) != 0)
                clear = beat + HitThreeGraphicLengthFloat(graphLength);
            if (Conductor.instance.songPositionInBeats <= clear && Conductor.instance.songPositionInBeats >= beat)
            {
                wordClearTime = clear;
            }
            switch (language)
            {
                case (int) HitThreeGraphicLang.Japanese:
                    return word + "jp";
                case (int) HitThreeGraphicLang.Korean:
                    return word + "ko";
                default:
                    return word;
            }
        }

        float HitExpressionLength(int type)
        {
            switch (type)
            {
                case (int) JoeFaceLength.Normal:
                    return 2f;
                case (int) JoeFaceLength.Kick:
                    return 0.7f;
                case (int) JoeFaceLength.Combo:
                    return 4f;
                case (int) JoeFaceLength.Infinite:
                    return float.MaxValue;
                case (int) JoeFaceLength.OneBeat:
                    return 1f;
                default:
                    return 2f;
            }
        }

        public string SoundEffectsType(int type)
        {
            switch (type)
            {
                case (int) SoundEffectTypes.GoldDS:
                    return "_ds";
                case (int) SoundEffectTypes.TengokuGBA:
                    return "_gba";
                default:
                    return null;
            }
        }

        public void CreateItem(float beat, int type, int expression, int length, int sound)
        {

            string outSound;
            if (Starpelly.Mathp.GetDecimalFromFloat(beat + 0.5f) == 0f && sound != (int)KarateMan.ForceObjectSound.Onbeat || sound == (int)KarateMan.ForceObjectSound.Offbeat)
                outSound = "karateman/offbeatObjectOut" + SoundEffectsType(SoundEffectsVersion);
            else
                outSound = "karateman/objectOut" + SoundEffectsType(SoundEffectsVersion);

            switch (type)
            {
                case (int) HitType.Pot:
                    CreateItemInstance(beat, HitExpressionLength(length), "Item00", expression);
                    break;
                case (int) HitType.Lightbulb:
                    if (Starpelly.Mathp.GetDecimalFromFloat(beat + 0.5f) == 0f && sound != (int) KarateMan.ForceObjectSound.Onbeat || sound == (int) KarateMan.ForceObjectSound.Offbeat)
                        outSound = "karateman/offbeatLightbulbOut" + SoundEffectsType(SoundEffectsVersion);
                    else
                        outSound = "karateman/lightbulbOut" + SoundEffectsType(SoundEffectsVersion);
                    var mobj = CreateItemInstance(beat, HitExpressionLength(length), "Item01", expression, KarateManPot.ItemType.Bulb);
                    mobj.GetComponent<KarateManPot>().SetBulbColor(LightBulbColors[0]);
                    break;
                case (int) HitType.Rock:
                    CreateItemInstance(beat, HitExpressionLength(length), "Item02", expression, KarateManPot.ItemType.Rock);
                    break;
                case (int) HitType.Ball:
                    CreateItemInstance(beat, HitExpressionLength(length), "Item03", expression, KarateManPot.ItemType.Ball);
                    break;
                case (int) HitType.CookingPot:
                    CreateItemInstance(beat, HitExpressionLength(length), "Item06", expression, KarateManPot.ItemType.Cooking);
                    break;
                case (int) HitType.Alien:
                    CreateItemInstance(beat, HitExpressionLength(length), "Item07", expression, KarateManPot.ItemType.Alien);
                    break;
                case (int) HitType.Bomb:
                    CreateItemInstance(beat, HitExpressionLength(length), "Item04", expression, KarateManPot.ItemType.Bomb);
                    break;
                case (int) HitType.TacoBell:
                    CreateItemInstance(beat, HitExpressionLength(length), "Item99", expression, KarateManPot.ItemType.TacoBell);
                    break;
                default:
                    CreateItemInstance(beat, HitExpressionLength(length), "Item00", expression);
                    break;
            }
            Jukebox.PlayOneShotGame(outSound, forcePlay: true);
        }

        public void CreateBulbSpecial(float beat, int type, Color c, int expression, int length, int sound)
        {
            string outSound;
            if (Starpelly.Mathp.GetDecimalFromFloat(beat + 0.5f) == 0f && sound != (int) KarateMan.ForceObjectSound.Onbeat || sound == (int)KarateMan.ForceObjectSound.Offbeat)
                if (SoundEffectsType(SoundEffectsVersion) == "_gba")
                    outSound = "karateman/offbeatLightbulbOut_ds";
                else
                    outSound = "karateman/offbeatLightbulbOut" + SoundEffectsType(SoundEffectsVersion);
            else
                if (SoundEffectsType(SoundEffectsVersion) == "_gba")
                    outSound = "karateman/lightbulbOut_ds";
                else
                    outSound = "karateman/lightbulbOut" + SoundEffectsType(SoundEffectsVersion);
            var mobj = CreateItemInstance(beat, HitExpressionLength(length), "Item01", expression, KarateManPot.ItemType.Bulb);

            if (type == (int) LightBulbType.Custom)
                mobj.GetComponent<KarateManPot>().SetBulbColor(c);
            else
                mobj.GetComponent<KarateManPot>().SetBulbColor(LightBulbColors[type]);
            Jukebox.PlayOneShotGame(outSound, forcePlay: true);
        }

        public void Combo(float beat, int expression, int length, bool pitchShift, int noCombo)
        {
            float pitch = 1f;

            if (pitchShift)
                pitch = Conductor.instance.songBpm / 128;

            if (noCombo != (int) SpecialSoundTypes.VoiceOnly)
            {
                Jukebox.PlayOneShotGame("karateman/barrelOutCombos", forcePlay: true);

                int comboId = KarateManPot.GetNewCombo();

                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { CreateItemInstance(beat, 0f, "Item00", 0, KarateManPot.ItemType.ComboPot1, comboId); }),
                    new BeatAction.Action(beat + 0.25f, delegate { CreateItemInstance(beat + 0.25f, 0f, "Item00", 0, KarateManPot.ItemType.ComboPot2, comboId); }),
                    new BeatAction.Action(beat + 0.5f, delegate { CreateItemInstance(beat + 0.5f, 0f, "Item00", 0, KarateManPot.ItemType.ComboPot3, comboId); }),
                    new BeatAction.Action(beat + 0.75f, delegate { CreateItemInstance(beat + 0.75f, 0f, "Item00", 0, KarateManPot.ItemType.ComboPot4, comboId); }),
                    new BeatAction.Action(beat + 1f, delegate { CreateItemInstance(beat + 1f, 0f, "Item00", 0, KarateManPot.ItemType.ComboPot5, comboId); }),
                    new BeatAction.Action(beat + 1.5f, delegate { CreateItemInstance(beat + 1.5f, HitExpressionLength(length), "Item05", expression, KarateManPot.ItemType.ComboBarrel, comboId); }),
                });
            }

            if (noCombo != (int) SpecialSoundTypes.NoVoice)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("karateman/punchy1", beat + 1f, pitch: pitch),
                    new MultiSound.Sound("karateman/punchy2", beat + 1.25f, pitch: pitch),
                    new MultiSound.Sound("karateman/punchy3", beat + 1.5f, pitch: pitch),
                    new MultiSound.Sound("karateman/punchy4", beat + 1.75f, pitch: pitch),
                    new MultiSound.Sound("karateman/ko", beat + 2f, pitch: pitch),
                    new MultiSound.Sound("karateman/pow", beat + 2.5f, pitch: pitch)
                }, forcePlay: true);
            }
        }

        public void Kick(float beat, bool ball, int expression, int length, bool pitchShift, int noBarrel)
        {
            float pitch = 1f;

            if (noBarrel != (int) SpecialSoundTypes.VoiceOnly)
            {
                Jukebox.PlayOneShotGame("karateman/barrelOutKicks", forcePlay: true);
                CreateItemInstance(beat, HitExpressionLength(length), "Item05", expression, KarateManPot.ItemType.KickBarrel, content: ball);
            }

            if (pitchShift)
                pitch = Conductor.instance.songBpm / 128;

            if (noBarrel != (int)SpecialSoundTypes.NoVoice)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("karateman/punchKick1", beat + 1f, pitch: pitch),
                    new MultiSound.Sound("karateman/punchKick2", beat + 1.5f, pitch: pitch),
                    new MultiSound.Sound("karateman/punchKick3", beat + 1.75f, pitch: pitch),
                    new MultiSound.Sound("karateman/punchKick4", beat + 2.5f, pitch : pitch),
                }, forcePlay: true);
            }
        }

        public GameObject CreateItemInstance(float beat, float hitExpLength, string awakeAnim, int successExpression, KarateManPot.ItemType type = KarateManPot.ItemType.Pot, int comboId = -1, bool content = false)
        {
            GameObject mobj = GameObject.Instantiate(Item, ItemHolder);
            KarateManPot mobjDat = mobj.GetComponent<KarateManPot>();
            mobjDat.type = type;
            mobjDat.startBeat = beat;
            mobjDat.awakeAnim = awakeAnim;
            mobjDat.comboId = comboId;
            mobjDat.OnHitExpression = successExpression;
            mobjDat.HitExpressionLength = hitExpLength;
            mobjDat.KickBarrelContent = content;

            mobj.SetActive(true);
            
            return mobj;
        }

        void SetBgEffectsToLast(float beat)
        {
            var bgfx = GameManager.instance.Beatmap.entities.FindAll(en => en.datamodel == "karateman/set background effects");
            for (int i = 0; i < bgfx.Count; i++)
            {
                var e = bgfx[i];
                if (e.beat > beat)
                    break;
                SetBgEffectsUnloaded(e.beat, e.length, e["type"], e["type2"], e["colorA"], e["colorB"], e["type3"], e["type4"], e["type5"], e["colorC"], e["colorD"]);
            }
            var camfx = GameManager.instance.Beatmap.entities.FindAll(en => en.datamodel == "karateman/special camera");
            DoSpecialCamera(0, 0, true);
            for (int i = 0; i < camfx.Count; i++)
            {
                var e = camfx[i];
                if (e.beat > beat)
                    break;
                DoSpecialCamera(e.beat, e.length, e["toggle"]);
            }
            var objfx = GameManager.instance.Beatmap.entities.FindAll(en => en.datamodel == "karateman/set object colors");
            for (int i = 0; i < objfx.Count; i++)
            {
                var e = objfx[i];
                if (e.beat > beat)
                    break;
                UpdateMaterialColour(e["colorA"], e["colorB"], e["colorC"]);
            }
            SetBgAndShadowCol(WantBgChangeStart, WantBgChangeLength, bgType, (int) currentShadowType, bgColour, customShadowColour, (int)currentBgEffect);
            SetBgTexture(textureType, textureFilterType, filterColour, filterColour);
        }

        public static void SetBgEffectsUnloaded(float beat, float length, int newBgType, int newShadowType, Color bgCol, Color shadowCol, int bgFx, int texture, int textureFilter, Color filterCol, Color filterColNext)
        {
            WantBgChangeStart = beat;
            WantBgChangeLength = length;
            bgType = newBgType;
            currentShadowType = newShadowType;
            bgColour = bgCol;
            customShadowColour = shadowCol;
            currentBgEffect = bgFx;
            textureType = texture;
            textureFilterType = textureFilter;
            filterColour = filterCol;
            filterColourNext = filterColNext;
        }

        public void SetBgAndShadowCol(float beat, float length, int newBgType, int shadowType, Color a, Color b, int fx)
        {
            SetBgFx(fx, beat, length);
            UpdateShadowColour(shadowType, b);

            bgType = newBgType;
            if (bgType == (int) BackgroundType.Custom)
                bgColour = a;
            else
                bgColour = BackgroundColors[newBgType];
            BGPlane.color = bgColour;

            //😢
            if (fx != (int) BackgroundFXType.Fade)
            {
                bgColourLast = bgColour;
                oldShadowColour = GetShadowColor(true);
            }

            if (textureFilterType == (int) ShadowType.Tinted)
                filterColour = Color.LerpUnclamped(bgColour, ShadowBlendColor, 0.45f);
        }

        public void SetBgFx(int fx, float beat, float length)
        {
            switch (fx)
            {
                case (int) BackgroundFXType.Fade:
                    bgColourLast = bgColour;
                    bgFadeTime = beat;
                    bgFadeDuration = length;
                    break;
                default:
                    currentBgEffect = fx;
                    break;
            }
        }

        public void SetBgTexture(int type, int filterType, Color filterColor, Color nextColor)
        {
            textureType = type;
            textureFilterType = filterType;
            if (textureFilterType == (int) ShadowType.Tinted)
                filterColour = Color.LerpUnclamped(bgColour, filterColor, 0.45f);
            else
            {
                filterColour = filterColor;
                filterColourNext = nextColor;
            }
            switch (textureType)
            {
                case (int) BackgroundTextureType.Blood:
                    BGBlood.SetActive(true);
                    BGGradient.SetActive(false);
                    BGRadial.SetActive(false);
                    break;
                case (int) BackgroundTextureType.Gradient:
                    BGGradient.SetActive(true);
                    BGBlood.SetActive(false);
                    BGRadial.SetActive(false);
                    break;
                case (int) BackgroundTextureType.Radial:
                    BGRadial.SetActive(true);
                    BGBlood.SetActive(false);
                    BGGradient.SetActive(false);
                    break;
                default:
                    BGGradient.SetActive(false);
                    BGBlood.SetActive(false);
                    BGRadial.SetActive(false);
                    break;
            }
            UpdateFilterColour(bgColour, filterColour);
        }

        public void SetGameplayMods(float beat, int mode, int bg, bool combo, bool noriBreak, bool highFlow, int soundVer, float breakDelay)
        {
            NoriGO.SetActive(true);
            Nori.SetNoriMode(beat, mode);
            highNoriBackground = bg;
            NoriBreakSound = noriBreak;
            HighFlowPunch = highFlow;
            SoundEffectsVersion = soundVer;
            PotBreakDelay = breakDelay;

            IsComboEnable = combo;
        }

        void UpdateFilterColour(Color bgColor, Color filterColor)
        {
            bgGradientRenderer = BGGradient.GetComponent<SpriteRenderer>();
            bgBloodRenderer = BGBlood.GetComponent<SpriteRenderer>();
            bgRadialRenderer = BGRadial.GetComponent<SpriteRenderer>();
            Color col;
            if (textureFilterType == (int) ShadowType.Tinted)
                col = Color.LerpUnclamped(bgColor, ShadowBlendColor, 0.45f);
            else
                col = filterColor;
            
            bgGradientRenderer.color = col;
            bgBloodRenderer.color = col;
            bgRadialRenderer.color = col;
        }

        public static Color ShadowBlendColor = new Color(195 / 255f, 48 / 255f, 2 / 255f);
        public Color GetShadowColor(bool next = false)
        {
            Color lastCol, nextCol;
            lastCol = oldShadowColour;

            if(currentShadowType == (int) ShadowType.Custom)
                nextCol = customShadowColour;
            else if(bgType < (int) BackgroundType.Custom)
                nextCol = ShadowColors[bgType];
            else
                nextCol = Color.LerpUnclamped(bgColour, ShadowBlendColor, 0.45f);

            float fadeProg = Conductor.instance.GetPositionFromBeat(bgFadeTime, bgFadeDuration);
            if (fadeProg <= 1f && fadeProg >= 0)
            {
                return Color.LerpUnclamped(lastCol, nextCol, fadeProg);
            }
            return next ? nextCol : lastCol;
        }

        public void UpdateShadowColour(int type, Color colour)
        {

            if(currentShadowType == (int) ShadowType.Custom)
                oldShadowColour = customShadowColour;
            else if(bgType < (int) BackgroundType.Custom)
                oldShadowColour = ShadowColors[bgType];
            else
                oldShadowColour = Color.LerpUnclamped(bgColour, ShadowBlendColor, 0.45f);

            currentShadowType = type;
            customShadowColour = colour;
        }

        public static void UpdateMaterialColour(Color mainCol, Color highlightCol, Color objectCol)
        {
            BodyColor = mainCol;
            HighlightColor = highlightCol;
            ItemColor = objectCol;
        }

        public void SetParticleEffect(float beat, int type, float windStrength, float particleStrength)
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

        public void ToggleBop(bool toggle, float length)
        {
            if (toggle)
                Joe.bop.length = Single.MaxValue;
            else
                Joe.bop.length = 0;

            BopLength = length;
        }

        public static void ToggleBopUnloaded(bool toggle)
        {
            WantBop = toggle;
        }
        public void Prepare(float beat, float length)
        {
            Joe.Prepare(beat, length);
        }

        public void SetFaceExpression(int face)
        {
            Joe.SetFaceExpression(face);
        }
    }
}
