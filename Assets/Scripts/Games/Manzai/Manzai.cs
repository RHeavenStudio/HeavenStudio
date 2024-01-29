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
            return new Minigame("manzai", "(WIP) Manzai", "72003D", false, false, new List<GameAction>()
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
                        Manzai.PunSFX(e.beat, e["pun"], e["pitch"], e["boing"]); },

                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        Manzai.instance.DoPun(e.beat, e["boing"], e["pun"]); },
                    defaultLength = 4,

                    parameters = new List<Param>()
                    {
                        new Param("boing", Manzai.BoingType.Normal, "Pun Type", "Will Kosuke mess up his pun?"),
                        /*new Param("random", true, "Random Voiceline", "Use a random pun?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "pun" }),
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "unused" })
                        }),*/
                        new Param("pun", Manzai.Puns.FutongaFuttonda, "Which Pun?", "Which pun will Kosuke say?"),
                        //new Param("unused", false, "Include Unused", "Will unused puns be picked?"),
                        new Param("pitch", true, "Pitch Voiceline", "Will the pun pitch with the tempo?"),
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

        [SerializeField] Transform PivotL;
        [SerializeField] Transform PivotR;
        [SerializeField] Transform PivotD;

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
        bool canDodge = true;



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
            /*AichiniAichinna,
            AmmeteAmena,
            ChainaniNichaina,
            DenwariDenwa,*/                    //short animation
            FutongaFuttonda,
            /*HiromegaHirameida,
            IkagariKatta,
            IkugawaIkura,                    //short animation (boing unused)
            KaeruBurikaeru,
            KarewaKare,
            KouchagaKouchou,
            KusagaKusai,                     //short animation (boing unused)
            MegaminiwaMegane,
            MikangaMikannai,
            NekogaNekoronda,
            OkanewaOkkane,
            OkurezeKitteOkure,
            OmochinoKimochi,
            OmoinoHokaOmoi,
            PuringaTappurin,
            RakudawaRakugana,
            RoukadaKatarouka,
            SaiyoMinasai,
            SakanaKanaMasakana,
            SarugaSaru,                      //short animation (boing unused)
            ShaiinniNanariNashain_Unused,    //fully unused
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
            if (ravenCanBop && ravenCanBopTemp)
                RavenAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void BopAnimationVulture()
        {
            if (vultureCanBop && vultureCanBopTemp)
                VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void DoPun(double beat, int isBoing, int whichPun)
        {
            int punOrBoing = isBoing;

            //if(isBoing == (int)Manzai.BoingType.Random)
            //{
            //    punOrBoing = UnityEngine.Random.Range(0, 5) % 2;
            //}

            if (punOrBoing == (int)Manzai.BoingType.Normal)
            {
                DoPunHai(beat, whichPun);
            }

            if (punOrBoing == (int)Manzai.BoingType.Boing)
            {
                DoPunBoing(beat, whichPun);
            }
            //Debug.Log(punOrBoing);
        }

        public static void PunSFX(double beat, int whichPun, bool isPitched, int isBoing)
        {
            var punName= Enum.GetName(typeof(Puns), whichPun);
            float pitch = isPitched ? Conductor.instance.songBpm/98 : 1;
            var sounds = new List<MultiSound.Sound>();
            int boing  = isBoing;
            int length = boingLengths.GetValueOrDefault(punName);
            int syllables = boing == 0 ? 9 : (length != 0 ? length : 4);

            Debug.Log(length);

            for (int i = 0; i < syllables; i++) {
                sounds.Add(new MultiSound.Sound($"manzai/{punName}{i + 1}", beat + (i * 0.25), pitch, offset: i == 0 ? 0.05 : 0));
            }

            if (isBoing == 1)
            {
                sounds.Add(new MultiSound.Sound("manzai/boing", syllables == 6 ? beat + 1.50 : beat + 1.25 , pitch));
            }

            MultiSound.Play(sounds.ToArray(), forcePlay: true);
        }

        public void DoPunHai(double beat, int whichPun)
        {
            vultureCanBop = false;
            canDodge = false;
            int bubbleAnimation = UnityEngine.Random.Range(0, 2);
            var punName= Enum.GetName(typeof(Puns), whichPun);

            ScheduleInput(beat, 2.5f, InputAction_BasicPress, bubbleAnimation == 0 ? HaiJustL : HaiJustR, HaiMiss, Nothing);
            ScheduleInput(beat, 3.0f, InputAction_BasicPress, bubbleAnimation == 0 ? HaiJustR : HaiJustL, HaiMiss, Nothing);

            if ((punName == "DenwariDenwa") || (punName == "IkugawaIkura") || (punName == "KusagaKusai") || (punName == "SarugaSaru"))
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 0.50f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 2.25f, delegate { ravenCanBop = false; canDodge = true; }),
                    new BeatAction.Action(beat + 3.25f, delegate { vultureCanBop = true; ravenCanBop = true; }),
                    new BeatAction.Action(beat + 3.25f, delegate { audienceRespond(beat); }),
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
                    new BeatAction.Action(beat + 2.25f, delegate { ravenCanBop = false; canDodge = true; }),
                    new BeatAction.Action(beat + 3.25f, delegate { vultureCanBop = true; ravenCanBop = true; }),
                    new BeatAction.Action(beat + 3.25f, delegate { audienceRespond(beat); }),
                });
            }
        }

        public void HaiJustFull(float state, int side)
        {
            SoundByte.PlayOneShotGame("manzai/hai", pitch: Conductor.instance.songBpm/98);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("manzai/miss1");
                SoundByte.PlayOneShotGame("manzai/missClick");
            }
            else
            {
                SoundByte.PlayOneShotGame("manzai/HaiAccent");
                if (side == 0)
                {
                    hitHaiL = true;
                }
                if (side == 1)
                {
                    hitHaiR = true;
                }
            }
            RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
            VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void HaiJustL(PlayerActionEvent caller, float state)
        {
            // var euler = PivotL.eulerAngles;
            // euler.z = UnityEngine.Random.Range(0, 100);
            int side = 0;

            HaiBubbleL.DoScaledAnimationAsync("HaiL", 0.5f);
            HaiJustFull(state, side);
        }

        public void HaiJustR(PlayerActionEvent caller, float state)
        {
            int side = 1;

            HaiBubbleR.DoScaledAnimationAsync("HaiR", 0.5f);
            HaiJustFull(state, side);
        }


        public void HaiMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("manzai/disappointed");
            
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
        }

        public void DoPunBoing(double beat, int whichPun)
        {
            vultureCanBop = false;
            canDodge = false;
            int bubbleAnimation = UnityEngine.Random.Range(0, 2);
            var punName= Enum.GetName(typeof(Puns), whichPun);
            int length = boingLengths.GetValueOrDefault(punName);
            int syllables = length != 0 ? length : 4;

            ScheduleInput(beat, 2.5f, InputAction_Alt, BoingJust, BoingMiss, Nothing);

            if ((punName == "DenwariDenwa") || (punName == "IkugawaIkura") || (punName == "KusagaKusai") || (punName == "SarugaSaru"))
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 0.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 0.50f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 1.25f, delegate { isPreparingForBoing = true; }),
                    new BeatAction.Action(syllables == 6 ? beat + 1.50 : beat + 1.25, delegate { VultureAnim.DoScaledAnimationAsync("Boing", 0.5f); }),
                    new BeatAction.Action(syllables == 6 ? beat + 1.75 : beat + 1.50, delegate { canDodge = true; }),
                    new BeatAction.Action(beat + 2.00f, delegate { SlapReady(); }),
                    new BeatAction.Action(beat + 2.25f, delegate { ravenCanBop = false; }),
                    new BeatAction.Action(beat + 3.00f, delegate { audienceRespond(beat); }),
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
                    new BeatAction.Action(beat + 1.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
                    new BeatAction.Action(beat + 1.25f, delegate { isPreparingForBoing = true; }),
                    new BeatAction.Action(syllables == 6 ? beat + 1.50 : beat + 1.25, delegate { VultureAnim.DoScaledAnimationAsync("Boing", 0.5f); }),
                    new BeatAction.Action(syllables == 6 ? beat + 1.75 : beat + 1.50, delegate { canDodge = true; }),
                    new BeatAction.Action(beat + 2.00f, delegate { SlapReady(); }),
                    new BeatAction.Action(beat + 2.25f, delegate { ravenCanBop = false; }),
                    new BeatAction.Action(beat + 3.00f, delegate { audienceRespond(beat); }),
                    new BeatAction.Action(beat + 3.20f, delegate { isPreparingForBoing = false; }),
                    new BeatAction.Action(beat + 3.25f, delegate { vultureCanBop = true; ravenCanBop = true; }),
                });
            }
        }

        public void audienceRespond(double beat)
        {
            if (hitHaiL && hitHaiR)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 3.50f, delegate { SoundByte.PlayOneShotGame("manzai/haiClap"); }),
                });
            }
            if (hitDonaiyanen)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + 3.00f, delegate { SoundByte.PlayOneShotGame("manzai/donaiyanenLaugh"); }),
                });
            }
            hitHaiL = false;
            hitHaiR = false;
            hitDonaiyanen = false;
        }

        public void BoingJust(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("manzai/donaiyanen", pitch: Conductor.instance.songBpm/98);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("manzai/miss1");
                SoundByte.PlayOneShotGame("manzai/missClick");
            }
            else
            {
                SoundByte.PlayOneShotGame("manzai/donaiyanenAccent");
                hitDonaiyanen = true;
            }
            RavenAnim.DoScaledAnimationAsync("Attack", 0.5f);
            VultureAnim.DoScaledAnimationAsync("Damage", 0.5f);

            int bubbleAnimation = UnityEngine.Random.Range(0, 2);
            DonaiyanenBubble.DoScaledAnimationAsync(bubbleAnimation == 1 ? "DonaiyanenL" : "DonaiyanenR", 0.5f);
        }

        public void BoingMiss(PlayerActionEvent caller)
        {
            if (!missedWithWrongButton)
            {
                SoundByte.PlayOneShotGame("manzai/disappointed");
            }
            missedWithWrongButton = false;
        }

        public void CustomBoing(double beat)
        {
            SoundByte.PlayOneShotGame("manzai/Boing", pitch: Conductor.instance.songBpm/98);
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
                        ravenCanBop = false;
                    }
                    else
                    {
                        SoundByte.PlayOneShotGame("manzai/hai");
                        RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
                        VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
                        lastWhiffBeat = conductor.songPositionInBeatsAsDouble;
                        ravenCanBopTemp = false;
                    }
                }
                else
                {
                    SoundByte.PlayOneShotGame("manzai/hai");
                    RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
                    VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
                    lastWhiffBeat = conductor.songPositionInBeatsAsDouble;
                    ravenCanBopTemp = false;
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
                lastWhiffBeat = conductor.songPositionInBeatsAsDouble;
                ravenCanBopTemp = false;
                if (canDodge)
                {
                    VultureAnim.DoScaledAnimationAsync("Dodge", 0.5f);
                    vultureCanBopTemp = false;
                }
            }
            if (PlayerInput.GetIsAction(InputAction_BasicPress) && IsExpectingInputNow(InputAction_Alt) && PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch)
            {
                SoundByte.PlayOneShotGame("manzai/missWrongButton");
                SoundByte.PlayOneShotGame("manzai/hai");
                RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
                VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
                missedWithWrongButton = true;
            }
            if ((lastWhiffBeat + 1) < conductor.songPositionInBeatsAsDouble)
            {
                ravenCanBopTemp = true;
                vultureCanBopTemp = true;
            }
            if (canDodge == false)
            {
                vultureCanBopTemp = true;
            }
        }

        public void LateUpdate()
        {
            var rot = PivotL.eulerAngles;
            rot.z = 50;
        }

        public override void OnGameSwitch(double beat)
        {
            foreach(var entity in GameManager.instance.Beatmap.Entities)
            {
                if(entity.beat > beat) //the list is sorted based on the beat of the entity, so this should work fine.
                {
                    break;
                }
                if(entity.datamodel != "manzai/pun" || entity.beat + entity.length <= beat) //check for dispenses that happen right before the switch
                {
                    continue;
                }
                bool isOnGameSwitchBeat = entity.beat == beat;
                DoPun(entity.beat, entity["boing"], entity["pun"]);
                break;
            }
        }
    }
}
