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
            return new Minigame("manzai", "Manzai", "554899", false, false, new List<GameAction>()
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
                        Manzai.PunSFX(e.beat, e["pun"], e["pitch"]); },

                    function = delegate { 
                        var e = eventCaller.currentEntity;
                        Manzai.instance.DoPun(e.beat, e["boing"]); },
                    defaultLength = 4,

                    parameters = new List<Param>()
                    {
                        new Param("boing", Manzai.BoingType.Normal, "Pun Type", "Will Kosuke mess up his pun?"),
                        new Param("random", true, "Random Voiceline", "Use a random pun?", new List<Param.CollapseParam>()
                        {
                            new Param.CollapseParam((x, _) => !(bool)x, new string[] { "pun" }),
                            new Param.CollapseParam((x, _) => (bool)x, new string[] { "unused" })
                        }),
                        new Param("pun", Manzai.Puns.FutongaFuttonda, "Which Pun?", "Which pun will Kosuke say?"),
                        new Param("unused", false, "Include Unused", "Will unused puns be picked?"),
                        new Param("pitch", true, "Pitch Voiceline", "Will the pun pitch with the tempo?"),
                    }
                },
                new GameAction("customBoing", "Custom Boing")
                {
                    function = delegate { Manzai.instance.CustomBoing(eventCaller.currentEntity.beat); },
                    defaultLength = 0.5f,
                },
                new GameAction("slidein", "Slide In")
                {
                    function = delegate { Manzai.instance.BirdsSlideIn(eventCaller.currentEntity.beat); },
                    defaultLength = 0.5f,
                },
                new GameAction("slideout", "Slide Out")
                {
                    function = delegate { Manzai.instance.BirdsSlideOut(eventCaller.currentEntity.beat); },
                    defaultLength = 0.5f,
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    //using Scripts_DogNinja;
    public class Manzai : Minigame
    {
        struct SfxDef
        {
            public string sfx;
            public double[] timings;
            public SfxDef (string sfx, double[] timings)
            {
                this.sfx = sfx;
                this.timings = timings;
            }
        }

        static readonly List<SfxDef> sfxDefs = new()
        {
            new("futongaFuttonda", new double[] {}),
            new("futongaFuttondaBoing", new double[] {}),
        };


        [SerializeField] Animator VultureAnim;
        [SerializeField] Animator RavenAnim;
        [SerializeField] Animator HaiBubbleL;
        [SerializeField] Animator HaiBubbleR;
        [SerializeField] Animator BothBirdsAnim;

        bool ravenBop = true;
        bool vultureBop = true;


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
            // AichiniAichinna,
            // AmmeteAmena,
            // ChainaniNichaina,
            // DenwariDenwa,                    //short animation
            FutongaFuttonda,
            // HiromegaHirameida,
            // IkagariKatta,
            // IkugawaIkura,                    //short animation (boing unused)
            // KaeruBurikaeru,
            // KarewaKare,
            // KouchagaKouchou,
            // KusagaKusai,                     //short animation (boing unused)
            // MegaminiwaMegane,
            MikangaMikannai,
            // NekogaNekoronda,
            OkanewaOkkane,
            // OkurezeKitteOkure,
            // OmochinoKimochi,
            // OmoinoHokaOmoi,
            // PuringaTappurin,
            // RakudawaRakugana,
            // RoukadaKatarouka,
            // SaiyoMinasai,
            // SakanaKanaMasakana,
            // SarugaSaru,                      //short animation (boing unused)
            // ShaiinniNanariNashain_Unused,    //fully unused
            // SuikawaYasuika,
            // TaigaTabetaina,
            // TaininiKittai,
            // TaiyoGamiTaiyou,
            // ToiletNiIttoire,
            // TonakaiyoOtonokoi,
            // TorinikugaTorininkui,
            // UmetteUmena,
        }


        public static Manzai instance;


        public void Awake()
        {
            instance = this;
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (vultureBop)
                VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);

            if (ravenBop)
                RavenAnim.DoScaledAnimationAsync("Bop", 0.5f);
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
                    if (whoBops is (int)WhoBops.Kasuke or (int)WhoBops.Both) actions.Add(new(beat + i, delegate { RavenAnim.DoScaledAnimationAsync("Bop", 0.5f); }));
                    if (whoBops is (int)WhoBops.Kosuke or (int)WhoBops.Both) actions.Add(new(beat + i, delegate { VultureAnim.DoScaledAnimationAsync("Bop", 0.5f); }));
                }
                BeatAction.New(this, actions);
            }
        }

        public void DoPun(double beat, int isBoing)
        {
            int punOrBoing = isBoing;

            //if(isBoing == (int)Manzai.BoingType.Random)
            //{
            //    punOrBoing = UnityEngine.Random.Range(0, 5) % 2;
            //}

            if(punOrBoing == (int)Manzai.BoingType.Normal)
            {
                DoPunHai(beat);
            }

            if(punOrBoing == (int)Manzai.BoingType.Boing)
            {
                DoPunBoing(beat);
            }
            //Debug.Log(punOrBoing);
        }

        public static void PunSFX(double beat, int whichPun, bool isPitched)
        {

            var punName= Enum.GetName(typeof(Puns), whichPun);
            float pitch = isPitched ? Conductor.instance.songBpm/98 : 1;

            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound($"manzai/{punName}1", beat + 0.00f, pitch, offset: 0.05),
                new MultiSound.Sound($"manzai/{punName}2", beat + 0.25f, pitch),
                new MultiSound.Sound($"manzai/{punName}3", beat + 0.50f, pitch),
                new MultiSound.Sound($"manzai/{punName}4", beat + 0.75f, pitch),
                new MultiSound.Sound($"manzai/{punName}5", beat + 1.00f, pitch),
                new MultiSound.Sound($"manzai/{punName}6", beat + 1.25f, pitch),
                new MultiSound.Sound($"manzai/{punName}7", beat + 1.50f, pitch),
                new MultiSound.Sound($"manzai/{punName}8", beat + 1.75f, pitch),
                new MultiSound.Sound($"manzai/{punName}9", beat + 2.00f, pitch),
            }, forcePlay: true);
        }

        public void DoPunHai(double beat)
        {
            int bubbleAnimation = UnityEngine.Random.Range(0, 2);

            ScheduleInput(beat, 2.5f, InputAction_BasicPress, bubbleAnimation == 0 ? HaiJustL : HaiJustR, HaiMiss, Nothing);
            ScheduleInput(beat, 3.0f, InputAction_BasicPress, bubbleAnimation == 0 ? HaiJustR : HaiJustL, HaiMiss, Nothing);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.0f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),

                new BeatAction.Action(beat + 0.5f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),

                new BeatAction.Action(beat + 1.0f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),

                new BeatAction.Action(beat + 1.5f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),
            });
        }

        public void HaiJustFull()
        {
            SoundByte.PlayOneShotGame("manzai/hai", pitch: Conductor.instance.songBpm/98);
            SoundByte.PlayOneShotGame("manzai/haiAccent");
            RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
            VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void HaiJustL(PlayerActionEvent caller, float state)
        {
            HaiBubbleL.DoScaledAnimationAsync("HaiL", 0.5f);
            HaiJustFull();
        }

        public void HaiJustR(PlayerActionEvent caller, float state)
        {
            HaiBubbleR.DoScaledAnimationAsync("HaiR", 0.5f);
            HaiJustFull();
        }


        public void HaiMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("manzai/disappointed");
            
            //SoundByte.PlayOneShotGame("manzai/hai");
            //RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
            //VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void DoPunBoing(double beat)
        {
            int bubbleAnimation = UnityEngine.Random.Range(0, 2);

            //SoundByte.PlayOneShotGame("manzai/"+sfxDefs[1].sfx, pitch: Conductor.instance.songBpm/98);

            ScheduleInput(beat, 2.5f, InputAction_BasicPress, BoingJust, BoingMiss, Nothing);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),

                new BeatAction.Action(beat + 0.50f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),

                new BeatAction.Action(beat + 1.00f, delegate { VultureAnim.DoScaledAnimationAsync("Talk", 0.5f); }),

                new BeatAction.Action(beat + 1.25f, delegate { VultureAnim.DoScaledAnimationAsync("Boing", 0.5f); }),

                new BeatAction.Action(beat + 2.00f, delegate { RavenAnim.DoScaledAnimationAsync("Ready", 0.5f); }),
            });
        }

        public void BoingJust(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("manzai/donaiyanen", pitch: Conductor.instance.songBpm/98);
            SoundByte.PlayOneShotGame("manzai/donaiyanenAccent");
            RavenAnim.DoScaledAnimationAsync("Attack", 0.5f);
            VultureAnim.DoScaledAnimationAsync("Damage", 0.5f);
        }

        public void BoingMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("manzai/disappointed");
        }

        public void CustomBoing(double beat)
        {
            SoundByte.PlayOneShotGame("manzai/Boing", pitch: Conductor.instance.songBpm/98);
            VultureAnim.DoScaledAnimationAsync("Boing", 0.5f);
        }

        public void Nothing(PlayerActionEvent caller)
        {

        }

        public void BirdsSlideIn(double beat)
        {
            BothBirdsAnim.DoScaledAnimationAsync("SlideIn", 0.5f);
        }

        public void BirdsSlideOut(double beat)
        {
            BothBirdsAnim.DoScaledAnimationAsync("SlideOut", 0.5f);
        }
    }
}
