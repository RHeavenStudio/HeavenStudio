using NaughtyBezierCurves;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlManzaiLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("manzai", "Manzai", "72003D", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        Manzai.instance.Bop(e.beat, e.length, e["who"], e["bop"], e["auto"]);
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", Manzai.WhoBops.Both, "Who Bops?", "Which bird bops"),
                        new Param("bop", true, "Enable Bopping", "Whether to bop to the beat or not"),
                        new Param("auto", false, "Automatic?", "Whether to bop to the beat or not automatically"),
                    }
                },
                new GameAction("pun", "Pun")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        Manzai.PunSFX(e.beat, e["pun"], e["pitch"], 0, e["crowd"], e["random"]); },

                    /*function = delegate { 
                        var e = eventCaller.currentEntity;
                        Manzai.instance.DoPun(e.beat, 0, e["pun"]); },*/
                    defaultLength = 4,

                    parameters = new List<Param>()
                    {
                        //new Param("boing", Manzai.BoingType.Normal, "Pun Type", "Will Kosuke mess up his pun?"),
                        new Param("random", true, "Random Voiceline", "Use a random pun?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "pun" }),
                            //new Param.CollapseParam((x, _) => (bool)x, new string[] { "unused" })
                        }),
                        new Param("pun", Manzai.Puns.FutongaFuttonda, "Which Pun?", "Which pun will Kosuke say?"),
                        //new Param("unused", false, "Include Unused", "Will unused puns be picked?"),
                        new Param("pitch", true, "Pitch Voiceline", "Will the pun pitch with the tempo?"),
                        new Param("crowd", Manzai.Crowd.Default, "Crowd Sounds", "How the crowd will sound during the input."),
                    }
                },
                new GameAction("boing", "Pun (Boing)")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        Manzai.PunSFX(e.beat, e["pun"], e["pitch"], 1, e["crowd"], e["random"]); },

                    /*function = delegate { 
                        var e = eventCaller.currentEntity;
                        Manzai.instance.DoPun(e.beat, 1, e["pun"]); },*/
                    defaultLength = 4,

                    parameters = new List<Param>()
                    {
                        //new Param("boing", Manzai.BoingType.Normal, "Pun Type", "Will Kosuke mess up his pun?"),
                        new Param("random", true, "Random Voiceline", "Use a random pun?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "pun" }),
                            //new Param.CollapseParam((x, _) => (bool)x, new string[] { "unused" })
                        }),
                        new Param("pun", Manzai.Puns.FutongaFuttonda, "Which Pun?", "Which pun will Kosuke say?"),
                        //new Param("unused", false, "Include Unused", "Will unused puns be picked?"),
                        new Param("pitch", true, "Pitch Voiceline", "Will the pun pitch with the tempo?"),
                        new Param("crowd", Manzai.Crowd.Default, "Crowd Sounds", "How the crowd will sound during the input."),
                    }
                },
                /* new GameAction("customBoing", "Custom Boing")
                {
                    function = delegate { Manzai.instance.CustomBoing(eventCaller.currentEntity.beat); },
                    defaultLength = 0.5f,
                }, */
                new GameAction("slide", "Birds Slide")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        Manzai.instance.BirdsSlide(e.beat, e.length, e["goToSide"], e["ease"], e["animation"]); 
                    },
                    defaultLength = 0.5f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("goToSide", Manzai.WhichSide.Outside, "Go to Which Side?", "Which side of the stage the birds will move to?"),
                        new Param("ease", EasingFunction.Ease.EaseOutQuad, "Ease", "Which ease should the movement have?"),
                        new Param("animation", true, "Play Animation?", "Whether the birds will use the slide animation"),
                    },
                },
                new GameAction("lights", "Toggle Lights")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity; 
                        Manzai.instance.ToggleLights(e["lightsEnabled"]); 
                    },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("lightsEnabled", false, "Spotlights", "Whether the spotlights will be turned on"),
                    },
                },
                new GameAction("crowd", "Crowd Animations")
                {
                    preFunction = delegate {
                        var e = eventCaller.currentEntity; 
                        Manzai.instance.CrowdAnimation(e.beat, e.length, e["animation"], e["loop"]); 
                    },
                    defaultLength = 1.0f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("animation", Manzai.CrowdAnimationList.Bop, "Animation", "What animation the crowd will play."),
                        new Param("loop", new EntityTypes.Integer(1, 16, 4), "Loop Interval (x4)", "How many quarter-beats the animation will wait before looping"),
                    },
                }},
                new List<string>() { "rvl", "normal" },
                "rvlmanzai", "jp",
                new List<string>() { "jp" }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_DogNinja;
    public class Manzai : Minigame
    {
        /* struct SfxDef
        {
            public string sfx;
            public float boingLength;
            public SfxDef (string sfx, float boingLength = 2.0f)
            {
                this.sfx = sfx;
                this.boingLength = boingLength;
            }
        }

        static readonly List<SfxDef> sfxDefs = new()
        {
            new("futongaFuttonda", 0.75),
        }; */


        [SerializeField] Animator VultureAnim;
        [SerializeField] Animator RavenAnim;
        [SerializeField] Animator HaiBubbleL;
        [SerializeField] Animator HaiBubbleR;
        [SerializeField] Animator DonaiyanenBubble;
        [SerializeField] Animator BothBirdsAnim;
        [SerializeField] Animator StageAnim;
        [SerializeField] Animator CrowdAnim;

        [SerializeField] Transform PivotL;
        [SerializeField] Transform PivotR;
        [SerializeField] Transform PivotD;
        [SerializeField] Transform CrowdPos;

        [SerializeField] ParticleSystem Feathers;

        bool ravenBop = true;
        bool vultureBop = true;

        bool ravenCanBop = true;
        bool vultureCanBop = true;

        bool ravenCanBopTemp = true;
        bool vultureCanBopTemp = true;

        bool isMoving;
        double movingStartBeat;
        double movingLength;
        string moveAnim;
        EasingFunction.Ease lastEase;

        bool isPreparingForBoing = false;
        bool missedWithWrongButton = false;

        bool hitHaiL = false;
        bool hitHaiR = false;
        bool hitDonaiyanen = false;

        double lastWhiffBeat = double.MinValue;

        double lastTappedBeat = double.MinValue;
        bool canDodge = true;

        float randomBubbleBoth = 0.0f;

        Sound crowdSound;

        bool isHolding = false;
        bool isPlayingReadyAnimationForTapMode = false;

        bool easterEgg1 = false;
        bool easterEgg2 = false;

        bool boingHasCrowdSounds = true;
        bool crowdCanCheerSound = true;
        bool crowdCanCheerAnimation = true;
        bool crowdIsCheering = false;
        double crowdLastMissAnimation = double.MinValue;

        bool jumpUp = false;
        bool jumpDown = false;
        float jumpStart;
        float jumpApex;
        float jumpLength;
        double startJumpTime = double.MinValue;
        float jumpHeight = 1;


        public enum WhoBops
        {
            Kasuke,
            Kosuke,
            Both,
        }

        public enum BoingType
        {
            Normal,
            Boing,
            //Random,
        }

        public enum Puns
        {
            AichiniAichinna,
            AmmeteAmena,
            /*ChainaniNichaina,
            DenwariDenwa,*/                    //short animation
            FutongaFuttonda,
            /*HiromegaHirameida,
            IkagariKatta,
            IkugawaIkura,                    //short animation (boing unused)
            KaeruBurikaeru,
            KarewaKare,
            KouchagaKouchou,
            KusagaKusai,                     //short animation (boing unused)
            MegaminiwaMegane,*/
            MikangaMikannai,
            /*NekogaNekoronda,*/
            OkanewaOkkane,
            /*OkurezeKitteOkure,
            OmochinoKimochi,
            OmoinoHokaOmoi,
            PuringaTappurin,*/
            RakudawaRakugana,
            /*RoukadaKatarouka,
            SaiyoMinasai,
            SakanaKanaMasakana,*/
            SarugaSaru,                      //short animation (boing unused)
            /*ShaiinniNanariNashain_Unused,    //fully unused
            SuikawaYasuika,
            TaigaTabetaina,
            TaininiKittai,
            TaiyoGamiTaiyou,
            ToiletNiIttoire,
            TonakaiyoOtonokoi,
            TorinikugaTorininkui,
            UmetteUmena,*/
            Muted,
        }

        static readonly Dictionary<string, int> boingLengths = new() {
            { "IkaggariKatta",                3 },
            { "KusagaKusai",                  3 },
            { "MegaminiwaMegane",             5 },
            { "OmoinoHokaOmoi",               6 },
            { "SakanaKanaMasakana",           5 },
            { "SarugaSaru",                   3 },
            { "ShaiinniNanariNashain_Unused", 5 },
            { "TaiyoGamiTaiyou",              5 },
            { "TonakaiyoOtonokoi",            5 },
            { "TorinikugaTorininkui",         5 },
        };

        public enum WhichSide
        {
            Inside,
            Outside,
        }

        public enum Crowd
        {
            Default,
            Practice,
            Silent,
        }

        public enum CrowdAnimationList
        {
            Idle,
            Bop,
            Cheer,
            Uproar,
            Angry,
            Jump,
        }

        const int IAAltDownCat = IAMAXCAT;

        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_Alt);
        }
        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlComediansAlt", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchFlick, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("RvlDateTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);


        public static Manzai instance;


        public void Awake()
        {
            instance = this;
            SetupBopRegion("manzai", "bop", "auto");
        }

        public override void OnLateBeatPulse(double beat)
        {   
            if (BeatIsInBopRegion(beat))
            {
                if (ravenBop) BopAnimationRaven();
                if (vultureBop) BopAnimationVulture();
            }
        }

        public void Bop(double beat, float length, int whoBops, bool bop, bool autoBop)
        {
            ravenBop = (whoBops is (int)WhoBops.Kasuke or (int)WhoBops.Both) && autoBop;
            vultureBop = (whoBops is (int)WhoBops.Kosuke or (int)WhoBops.Both) && autoBop;

            if (bop)
            {
                var actions = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                { 
                    if (whoBops is (int)WhoBops.Kasuke or (int)WhoBops.Both) actions.Add(new(beat + i, delegate { BopAnimationRaven(); }));
                    if (whoBops is (int)WhoBops.Kosuke or (int)WhoBops.Both) actions.Add(new(beat + i, delegate { BopAnimationVulture(); }));
                }
                BeatAction.New(this, actions);
            }
        }

        public void BopAnimationRaven()
        {
            if (ravenCanBop && ravenCanBopTemp && !isHolding)
                RavenAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void BopAnimationVulture()
        {
            if (vultureCanBop && vultureCanBopTemp)
                VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void DoPun(double beat, int isBoing, int whichPun, bool isPitched, int crowdSounds)
        {
            int punOrBoing = isBoing;

            //if(isBoing == (int)Manzai.BoingType.Random)
            //{
            //    punOrBoing = UnityEngine.Random.Range(0, 5) % 2;
            //}

            if (punOrBoing == (int)Manzai.BoingType.Normal)
            {
                DoPunHai(beat, whichPun, isPitched, crowdSounds);
            }

            if (punOrBoing == (int)Manzai.BoingType.Boing)
            {
                DoPunBoing(beat, whichPun, isPitched, crowdSounds);
            }
            //Debug.Log(punOrBoing);

            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 4.0, delegate { crowdAnimReset(); }),
            });
        }

        public static void PunSFX(double beat, int whichPun, bool isPitched, int isBoing, int crowdSounds, bool random)
        {
            if (random) whichPun = UnityEngine.Random.Range(0, 7);
            var punName= Enum.GetName(typeof(Puns), whichPun);
            float pitch = isPitched ? (Conductor.instance.GetBpmAtBeat(beat)/98)*Conductor.instance.TimelinePitch : 1;
            double offset = isPitched ? (0.05/(Conductor.instance.GetBpmAtBeat(beat)/98)) : 0.05;
            var sounds = new List<MultiSound.Sound>();
            int boing  = isBoing;
            int length = boingLengths.GetValueOrDefault(punName);
            int syllables = boing == 0 ? 9 : (length != 0 ? length : 4);

            BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { Manzai.instance.DoPun(beat, isBoing, whichPun, isPitched, crowdSounds); }),
                });

            for (int i = 0; i < syllables; i++) {
                sounds.Add(new MultiSound.Sound($"manzai/{punName}{i + 1}", beat + (i * 0.25), pitch, offset: i == 0 ? offset : 0));
            }

            if (isBoing == 1)
            {
                sounds.Add(new MultiSound.Sound("manzai/boing", syllables == 6 ? beat + 1.50 : beat + 1.25 , pitch, volume: 0.8f));
                sounds.Add(new MultiSound.Sound("manzai/comedy", syllables == 6 ? beat + 1.50 : beat + 1.25 , pitch, volume: 0.8f));
            }

            MultiSound.Play(sounds.ToArray(), forcePlay: true);
        }

        public void DoPunHai(double beat, int whichPun, bool isPitched, int crowdSounds)
        {
            vultureCanBop = false;
            canDodge = false;
            int bubbleAnimation = UnityEngine.Random.Range(0, 2);
            var punName= Enum.GetName(typeof(Puns), whichPun);
            if (isPitched)
            {
                ScheduleInput(beat, 2.5f, InputAction_BasicPress, bubbleAnimation == 0 ? HaiJustLP : HaiJustRP, crowdSounds == 0 ? HaiMiss : Nothing, Nothing);
                ScheduleInput(beat, 3.0f, InputAction_BasicPress, bubbleAnimation == 0 ? HaiJustRP : HaiJustLP, crowdSounds == 0 ? HaiMiss : Nothing, Nothing);
            }
            else
            {
                ScheduleInput(beat, 2.5f, InputAction_BasicPress, bubbleAnimation == 0 ? HaiJustL  : HaiJustR,  crowdSounds == 0 ? HaiMiss : Nothing, Nothing);
                ScheduleInput(beat, 3.0f, InputAction_BasicPress, bubbleAnimation == 0 ? HaiJustR  : HaiJustL,  crowdSounds == 0 ? HaiMiss : Nothing, Nothing);
            }

            if (crowdSounds == 1)
            {
                float pitch = isPitched ? (Conductor.instance.GetBpmAtBeat(beat)/98)*Conductor.instance.TimelinePitch : 1;
                double offset = isPitched ? (0.03/(Conductor.instance.GetBpmAtBeat(beat)/98)) : 0.03;

                var sounds = new List<MultiSound.Sound>();
                sounds.Add(new MultiSound.Sound("manzai/crowdHai1", beat + 2.50, pitch, offset: offset, volume: 0.7f));
                sounds.Add(new MultiSound.Sound("manzai/crowdHai2", beat + 3.00, pitch, offset: offset, volume: 0.7f));
                MultiSound.Play(sounds.ToArray(), forcePlay: true);
            }

            if ((punName == "DenwariDenwa") || (punName == "IkugawaIkura") || (punName == "KusagaKusai") || (punName == "SarugaSaru"))
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 0.50f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 2.25f, delegate { ravenCanBop = false; canDodge = true; bubbleRandom(); }),
                    new BeatAction.Action(beat + 2.25f, delegate { crowdCanCheerSound = (crowdSounds == 0); }),
                    new BeatAction.Action(beat + 3.25f, delegate { vultureCanBop = true; ravenCanBop = true; }),
                    new BeatAction.Action(beat + 3.25f, delegate { audienceRespond(beat, crowdSounds); }),
                });
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 0.50f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 1.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 1.50f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 2.25f, delegate { ravenCanBop = false; canDodge = true; bubbleRandom(); }),
                    new BeatAction.Action(beat + 2.25f, delegate { crowdCanCheerSound = (crowdSounds == 0); }),
                    new BeatAction.Action(beat + 3.25f, delegate { vultureCanBop = true; ravenCanBop = true; }),
                    new BeatAction.Action(beat + 3.25f, delegate { audienceRespond(beat, crowdSounds); }),
                });
            }
        }

        public void  bubbleRandom()
        {
            randomBubbleBoth = UnityEngine.Random.Range(-100, 101) / 1000.0f;
        }

        public void HaiJustFull(float state, int side, PlayerActionEvent caller, bool isPitched)
        {
            double beat = caller.startBeat + caller.timer;

            SoundByte.PlayOneShotGame("manzai/hai", pitch: isPitched ? (Conductor.instance.GetBpmAtBeat(beat)/98)*Conductor.instance.TimelinePitch : 1);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("manzai/miss1");
                SoundByte.PlayOneShotGame("manzai/missClick");
                if (crowdCanCheerSound && crowdCanCheerAnimation)
                {
                    CrowdAnim.DoScaledAnimationAsync("Angry", 0.5f);
                    crowdIsCheering = false;
                    crowdLastMissAnimation = conductor.songPositionInBeatsAsDouble;
                }
            }
            else
            {
                SoundByte.PlayOneShotGame("manzai/HaiAccent");

                if (crowdCanCheerSound && crowdCanCheerAnimation && !crowdIsCheering)
                {
                    CrowdAnim.DoScaledAnimationAsync("Cheer", 0.5f);
                    crowdIsCheering = true;
                }

                if (side == 0)
                {
                    float randomL = UnityEngine.Random.Range(-40, 41) / 1000.0f;

                    Quaternion rotL = new Quaternion();
                    rotL.Set(0, 0, randomBubbleBoth + randomL, 1);
                    PivotL.rotation = rotL;
                    HaiBubbleL.DoScaledAnimationAsync("HaiL", 0.5f);
                    hitHaiL = true;
                }
                if (side == 1)
                {
                    float randomR = UnityEngine.Random.Range(-40, 41) / 1000.0f;

                    Quaternion rotR = new Quaternion();
                    rotR.Set(0, 0, randomBubbleBoth + randomR, 1);
                    PivotR.rotation = rotR;
                    HaiBubbleR.DoScaledAnimationAsync("HaiR", 0.5f);
                    hitHaiR = true;
                }
            }
            if (easterEgg1 && easterEgg2)
            {
                RavenAnim.DoScaledAnimationAsync(side == 0 ? "Move" : "MoveM", 0.5f);
            }
            else
            {
                RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
            }
            VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);

            if (crowdSound != null)
            {
                crowdSound.Stop();
            }
        }

        public void HaiJustL(PlayerActionEvent caller, float state) => HaiJustFull(state, 0, caller, false);

        public void HaiJustR(PlayerActionEvent caller, float state) => HaiJustFull(state, 1, caller, false);

        public void HaiJustLP(PlayerActionEvent caller, float state) => HaiJustFull(state, 0, caller, true);

        public void HaiJustRP(PlayerActionEvent caller, float state) => HaiJustFull(state, 1, caller, true);

        public void HaiMiss(PlayerActionEvent caller)
        {
            if (crowdSound != null)
            {
                crowdSound.Stop();
            }
            crowdSound = SoundByte.PlayOneShotGame("manzai/disappointed");
            
            if (crowdCanCheerAnimation)
            {
                CrowdAnim.DoScaledAnimationAsync("Angry", 0.5f);
                crowdIsCheering = false;
                crowdLastMissAnimation = conductor.songPositionInBeatsAsDouble;
            }
            
            //SoundByte.PlayOneShotGame("manzai/hai");
            //RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
            //VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void SlapReady()
        {
            if (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
            {
                RavenAnim.DoScaledAnimationAsync("Ready", 0.5f);
            }
            else
            {
                if (GameManager.instance.autoplay)
                {
                    RavenAnim.DoScaledAnimationAsync("Ready", 0.5f);
                }
            }
        }

        public void DoPunBoing(double beat, int whichPun, bool isPitched, int crowdSounds)
        {
            vultureCanBop = false;
            canDodge = false;
            int bubbleAnimation = UnityEngine.Random.Range(0, 2);
            var punName= Enum.GetName(typeof(Puns), whichPun);
            int length = boingLengths.GetValueOrDefault(punName);
            int syllables = length != 0 ? length : 4;

            ScheduleInput(beat, 2.5f, InputAction_Alt, isPitched ? BoingJustP : BoingJustNP , crowdSounds == 0 ? BoingMiss : Nothing, Nothing);

            if (crowdSounds == 1)
            {
                float pitch = isPitched ? (Conductor.instance.GetBpmAtBeat(beat)/98)*Conductor.instance.TimelinePitch : 1;
                double offset = isPitched ? (0.03/(Conductor.instance.GetBpmAtBeat(beat)/98)) : 0.03;

                var sounds = new List<MultiSound.Sound>();
                sounds.Add(new MultiSound.Sound("manzai/crowdDon1", beat + 0.00 + 2.50, pitch, offset: offset, volume: 0.7f));
                sounds.Add(new MultiSound.Sound("manzai/crowdDon2", beat + 0.25 + 2.50, pitch,                 volume: 0.7f));
                sounds.Add(new MultiSound.Sound("manzai/crowdDon3", beat + 0.75 + 2.50, pitch,                 volume: 0.7f));
                sounds.Add(new MultiSound.Sound("manzai/crowdDon4", beat + 1.00 + 2.50, pitch,                 volume: 0.7f));
                MultiSound.Play(sounds.ToArray(), forcePlay: true);
            }

            if ((punName == "DenwariDenwa") || (punName == "IkugawaIkura") || (punName == "KusagaKusai") || (punName == "SarugaSaru"))
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 0.50f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 0.50f, delegate { isPreparingForBoing = true; }),
                    new BeatAction.Action(syllables == 6 ? beat + 1.50 : beat + 1.25, delegate { VultureAnim.DoScaledAnimationAsync("Boing", 0.5f); }),
                    new BeatAction.Action(syllables == 6 ? beat + 1.75 : beat + 1.50, delegate { canDodge = true; }),
                    new BeatAction.Action(beat + 2.00f, delegate { ravenCanBop = false; }),
                    new BeatAction.Action(beat + 2.00f, delegate { SlapReady(); }),
                    new BeatAction.Action(beat + 2.25f, delegate { boingHasCrowdSounds = (crowdSounds == 0); }),
                    new BeatAction.Action(beat + 2.25f, delegate { crowdCanCheerSound = (crowdSounds == 0); }),
                    new BeatAction.Action(beat + 3.00f, delegate { audienceRespond(beat, crowdSounds); }),
                    new BeatAction.Action(beat + 3.20f, delegate { isPreparingForBoing = false; }),
                    new BeatAction.Action(beat + 3.25f, delegate { vultureCanBop = true; ravenCanBop = true; }),
                });
            }
            else
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 0.50f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 0.50f, delegate { isPreparingForBoing = true; }),
                    new BeatAction.Action(beat + 1.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(syllables == 6 ? beat + 1.50 : beat + 1.25, delegate { VultureAnim.DoScaledAnimationAsync("Boing", 0.5f); }),
                    new BeatAction.Action(syllables == 6 ? beat + 1.75 : beat + 1.50, delegate { canDodge = true; }),
                    new BeatAction.Action(beat + 2.00f, delegate { ravenCanBop = false; }),
                    new BeatAction.Action(beat + 2.00f, delegate { SlapReady(); }),
                    new BeatAction.Action(beat + 2.25f, delegate { boingHasCrowdSounds = (crowdSounds == 0); }),
                    new BeatAction.Action(beat + 2.25f, delegate { crowdCanCheerSound = (crowdSounds == 0); }),
                    new BeatAction.Action(beat + 3.00f, delegate { audienceRespond(beat, crowdSounds); }),
                    new BeatAction.Action(beat + 3.20f, delegate { isPreparingForBoing = false; }),
                    new BeatAction.Action(beat + 3.25f, delegate { vultureCanBop = true; ravenCanBop = true; }),
                });
            }
        }

        public void audienceRespond(double beat, int crowdSounds)
        {
            if (crowdSounds == 0)
            {
                if (hitHaiL && hitHaiR)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 3.50f, delegate { crowdSound = SoundByte.PlayOneShotGame("manzai/haiClap"); }),
                    });
                }
                if (hitDonaiyanen)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + 3.00f, delegate { crowdSound = SoundByte.PlayOneShotGame("manzai/donaiyanenLaugh"); }),
                    });
                }
                crowdCanCheerSound = false;
            }
            hitHaiL = false;
            hitHaiR = false;
            hitDonaiyanen = false;
        }

        public void BoingJustP(PlayerActionEvent caller, float state)
        {
            BoingJust(caller, state, true);
        }

        public void BoingJustNP(PlayerActionEvent caller, float state)
        {
            BoingJust(caller, state, false);
        }

        public void BoingJust(PlayerActionEvent caller, float state, bool isPitched)
        {
            double beat = caller.startBeat + caller.timer;
            float pitch = isPitched ? (Conductor.instance.GetBpmAtBeat(beat)/98)*Conductor.instance.TimelinePitch : 1;

            var sounds = new List<MultiSound.Sound>();
            sounds.Add(new MultiSound.Sound("manzai/donaiyanen1", beat + 0.00, pitch));
            sounds.Add(new MultiSound.Sound("manzai/donaiyanen2", beat + 0.25, pitch));
            sounds.Add(new MultiSound.Sound("manzai/donaiyanen3", beat + 0.75, pitch));
            sounds.Add(new MultiSound.Sound("manzai/donaiyanen4", beat + 1.00, pitch));
            MultiSound.Play(sounds.ToArray(), forcePlay: true);

            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("manzai/miss1");
                SoundByte.PlayOneShotGame("manzai/missClick");
                if (crowdCanCheerSound && crowdCanCheerAnimation)
                {
                    CrowdAnim.DoScaledAnimationAsync("Angry", 0.5f);
                    crowdIsCheering = false;
                    crowdLastMissAnimation = conductor.songPositionInBeatsAsDouble;
                }
            }
            else
            {
                SoundByte.PlayOneShotGame("manzai/donaiyanenAccent");

                if (crowdCanCheerSound && crowdCanCheerAnimation)
                {
                    CrowdAnim.DoScaledAnimationAsync("Uproar", 0.5f);
                    crowdIsCheering = true;
                }

                Feathers.Play();

                hitDonaiyanen = true;
            }
            RavenAnim.DoScaledAnimationAsync("Attack", 0.5f);
            VultureAnim.DoScaledAnimationAsync("Damage", 0.5f);


            float randomD = UnityEngine.Random.Range(-40, 41) / 200.0f;
            float randomH = UnityEngine.Random.Range(-50, 51) / 30.0f;
            float randomV = UnityEngine.Random.Range(40, 81) / 40.0f;

            Quaternion rotD = new Quaternion();
            rotD.Set(0, 0, randomD, 1);
            PivotD.rotation = rotD;

            Vector3 posD = new Vector3();
            posD.Set(randomH - 1.5f, randomV, 0);
            PivotD.position = posD;

            DonaiyanenBubble.DoScaledAnimationAsync("Donaiyanen", 0.5f);

            if (crowdSound != null)
            {
                crowdSound.Stop();
            }
        }

        public void BoingMiss(PlayerActionEvent caller)
        {
            if (crowdSound != null)
            {
                crowdSound.Stop();
            }

            if (!missedWithWrongButton)
            {
                crowdSound = SoundByte.PlayOneShotGame("manzai/disappointed");
                missedWithWrongButton = false;
            }

            if (crowdCanCheerAnimation)
            {
                CrowdAnim.DoScaledAnimationAsync("Angry", 0.5f);
                crowdIsCheering = false;
                crowdLastMissAnimation = conductor.songPositionInBeatsAsDouble;
            }
        }

        public void CustomBoing(double beat)
        {
            SoundByte.PlayOneShotGame("manzai/boing", pitch: (Conductor.instance.GetBpmAtBeat(beat)/98)*Conductor.instance.TimelinePitch, volume: 0.8f);
            SoundByte.PlayOneShotGame("manzai/comedy", pitch: (Conductor.instance.GetBpmAtBeat(beat)/98)*Conductor.instance.TimelinePitch, volume: 0.8f);
            VultureAnim.DoScaledAnimationAsync("Boing", 0.5f);
        }

        public void Nothing(PlayerActionEvent caller)
        {

        }

        public void BirdsSlide(double beat, double length, int goToSide, int ease, bool animation)
        {
            vultureCanBop = false;
            ravenCanBop = false;
            if (animation) 
            {
                RavenAnim.DoScaledAnimationAsync("Move", 0.5f);
                VultureAnim.DoScaledAnimationAsync("Move", 0.5f);
            }
            movingStartBeat = beat;
            movingLength = length;
            moveAnim = (goToSide == 0 ? "SlideIn" : "SlideOut");
            if (goToSide == 0)
            {
                canDodge = true;
            }
            else
            {
                canDodge = false;
            }
            isMoving = true;
            lastEase = (EasingFunction.Ease)ease;
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.75f, delegate { vultureCanBop = true; ravenCanBop = true; }),
            });
        }

        public void ToggleLights(bool lightsEnabled)
        {
            StageAnim.DoScaledAnimationAsync(lightsEnabled ? "LightsOff" : "LightsOn", 0.5f);
        }

        public void CrowdAnimation(double beat, double length, int animation, int loop)
        {
            double loopAsDouble = loop * 0.25;

            var actions = new List<BeatAction.Action>();
                if (animation != 1)
                {
                    actions.Add(new(beat - 0.25, delegate { crowdCanCheerAnimation = false; }));
                }
                if (animation != 1 && animation != 5)
                {
                    actions.Add(new(beat, delegate { doCrowdAnimation(animation, loopAsDouble, beat); }));
                    actions.Add(new(beat + length, delegate { CrowdAnim.DoScaledAnimationAsync("Idle"); }));
                }
                else
                {
                    for (int i = 0; i * loopAsDouble < length; i++)
                    { 
                        actions.Add(new(beat + i * loopAsDouble, delegate { doCrowdAnimation(animation, loopAsDouble, beat); }));
                    }
                }
                actions.Add(new(beat + length, delegate { crowdCanCheerAnimation = true; }));
            BeatAction.New(this, actions);
        }

        public void doCrowdAnimation(int animation, double loop, double beat)
        {
            var crowdAnimation= Enum.GetName(typeof(CrowdAnimationList), animation);

            if (!crowdIsCheering && ((crowdLastMissAnimation + 2) < conductor.songPositionInBeatsAsDouble) && animation == 1)
            {
                CrowdAnim.DoScaledAnimationAsync($"{crowdAnimation}", 0.5f);
            }
            if (animation == 5)
            {
                jumpHeight = Math.Min((float)loop, 2f);
                jumpLength = (float)loop;

                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { startJumpTime = conductor.songPositionInBeatsAsDouble; }),
                });
            }
            if (animation != 1 && animation != 5)
            {
                CrowdAnim.DoScaledAnimationAsync($"{crowdAnimation}", 0.5f);
            }
        }

        public void crowdJumpAnimation()
        {
            float jumpPos = Conductor.instance.GetPositionFromBeat(startJumpTime, jumpLength);
            if (jumpPos >= 0 && jumpPos <= 1f)
            {
                float yMul = jumpPos * 2f - 1f;
                float yWeight = -(yMul * yMul) + 1f;
                CrowdPos.transform.localPosition = new Vector3(0, jumpHeight * yWeight, -2);
            }
        }

        public void crowdAnimReset()
        {
            if (crowdIsCheering)
            {
                CrowdAnim.DoScaledAnimationAsync("Idle", 0.5f);
                crowdIsCheering = false;
            }
        }

        public void kasukeHaiAnimFull()
        {
            SoundByte.PlayOneShotGame("manzai/hai");
            if (easterEgg1 && easterEgg2)
            {
                int danceSide = UnityEngine.Random.Range(0, 2);
                RavenAnim.DoScaledAnimationAsync(danceSide == 0 ? "Move" : "MoveM", 0.5f);
            }
            else
            {
                RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
            }
            VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        private void Update()
        {
            if (isMoving) {
                float normalizedBeat = Conductor.instance.GetPositionFromBeat(movingStartBeat, movingLength);
                EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                float newPos = func(0f, 1f, normalizedBeat);
                BothBirdsAnim.DoNormalizedAnimation(moveAnim, newPos);
                if (normalizedBeat >= 1f) isMoving = false;
            }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_Alt))
            {
                if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
                {
                    if (isPreparingForBoing)
                    {
                        RavenAnim.DoScaledAnimationAsync("Ready", 0.5f);
                        isPlayingReadyAnimationForTapMode = true;
                        ravenCanBop = false;
                    }
                    else
                    {
                        kasukeHaiAnimFull();
                        Manzai.instance.ScoreMiss(0.5f);
                        lastWhiffBeat = conductor.songPositionInBeatsAsDouble;
                        ravenCanBopTemp = false;
                        if (crowdCanCheerAnimation)
                        {
                            CrowdAnim.DoScaledAnimationAsync("Angry", 0.5f);
                            crowdIsCheering = false;
                            crowdLastMissAnimation = conductor.songPositionInBeatsAsDouble;
                        }
                    }
                }
                else
                {
                    kasukeHaiAnimFull();
                    Manzai.instance.ScoreMiss(0.5f);
                    lastWhiffBeat = conductor.songPositionInBeatsAsDouble;
                    ravenCanBopTemp = false;
                    if (crowdCanCheerAnimation)
                    {
                        CrowdAnim.DoScaledAnimationAsync("Angry", 0.5f);
                        crowdIsCheering = false;
                        crowdLastMissAnimation = conductor.songPositionInBeatsAsDouble;
                    }
                }
            }

            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.GetIsAction(InputAction_BasicRelease) && isPreparingForBoing)
            {
                RavenAnim.DoScaledAnimationAsync("Bop", 0.5f);
                ravenCanBop = true;
            }

            if (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_Alt))
            {
                SoundByte.PlayOneShotGame("manzai/miss2");
                RavenAnim.DoScaledAnimationAsync("Spin", 0.5f);
                Manzai.instance.ScoreMiss(0.5f);
                lastWhiffBeat = conductor.songPositionInBeatsAsDouble;
                ravenCanBopTemp = false;
                if (canDodge)
                {
                    VultureAnim.DoScaledAnimationAsync("Dodge", 0.5f);
                    vultureCanBopTemp = false;
                }
                if (crowdCanCheerAnimation)
                {
                    CrowdAnim.DoScaledAnimationAsync("Angry", 0.5f);
                    crowdIsCheering = false;
                    crowdLastMissAnimation = conductor.songPositionInBeatsAsDouble;
                }
            }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && IsExpectingInputNow(InputAction_Alt) && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
            {
                if (boingHasCrowdSounds)
                {
                    crowdSound = SoundByte.PlayOneShotGame("manzai/missWrongButton");
                }
                else
                {
                    crowdSound = SoundByte.PlayOneShotGame("manzai/missClick");
                }
                kasukeHaiAnimFull();
                missedWithWrongButton = true;
                if (crowdCanCheerAnimation)
                {
                    CrowdAnim.DoScaledAnimationAsync("Angry", 0.5f);
                    crowdIsCheering = false;
                    crowdLastMissAnimation = conductor.songPositionInBeatsAsDouble;
                }
            }

            if ((lastWhiffBeat + 1) < conductor.songPositionInBeatsAsDouble)
            {
                ravenCanBopTemp = true;
                vultureCanBopTemp = true;
            }

            if (((lastTappedBeat + 1) < conductor.songPositionInBeatsAsDouble && !isPlayingReadyAnimationForTapMode && isHolding))
            {
                RavenAnim.DoScaledAnimationAsync("Ready", 0.5f);
                isPlayingReadyAnimationForTapMode = true;
            }

            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.GetIsAction(InputAction_BasicPress))
            {
                isHolding = true;
                lastTappedBeat = conductor.songPositionInBeatsAsDouble;
            }

            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && (PlayerInput.GetIsAction(InputAction_BasicRelease) || PlayerInput.GetIsAction(InputAction_Alt)))
            {
                if(isPlayingReadyAnimationForTapMode && PlayerInput.GetIsAction(InputAction_BasicRelease))
                {
                    RavenAnim.DoScaledAnimationAsync("Bop", 0.5f);
                }
                isHolding = false;
                isPlayingReadyAnimationForTapMode = false;
            }

            if (PlayerInput.GetPadDown(InputController.ActionsPad.ButtonL))
            {
                easterEgg1 = true;
            }

            if (PlayerInput.GetPadUp(InputController.ActionsPad.ButtonL))
            {
                easterEgg1 = false;
            }

            if (PlayerInput.GetPadDown(InputController.ActionsPad.ButtonR))
            {
                easterEgg2 = true;
            }

            if (PlayerInput.GetPadUp(InputController.ActionsPad.ButtonR))
            {
                easterEgg2 = false;
            }

            if (canDodge == false)
            {
                vultureCanBopTemp = true;
            }
        }

        public void LateUpdate()
        {
            crowdJumpAnimation();
        }

        public override void OnGameSwitch(double beat)
        {
            foreach(var entity in GameManager.instance.Beatmap.Entities)
            {
                if(entity.beat > beat) //the list is sorted based on the beat of the entity, so this should work fine.
                {
                    break;
                }
                if((entity.datamodel != "manzai/pun" &&  entity.datamodel != "manzai/boing") || entity.beat + entity.length < beat) //check for pun that happen right before the switch
                {
                    continue;
                }
                bool isOnGameSwitchBeat = entity.beat == beat;
                if(entity.datamodel == "manzai/pun")   {DoPun(entity.beat, 0, entity["pun"], entity["pitch"], entity["crowd"]);}
                if(entity.datamodel == "manzai/boing") {DoPun(entity.beat, 1, entity["pun"], entity["pitch"], entity["crowd"]);}
            }
        }
    }
}
