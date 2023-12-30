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
                    function = delegate { Manzai.instance.Bop(); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("who", Manzai.WhoBops.Both, "Who Bops?", "Which bird bops"),
                        new Param("auto", false, "Automatic?", "Whether to bop to the beat or not automatically"),
                    }
                },
                new GameAction("pun", "Pun")
                {
                    function = delegate { Manzai.instance.DoPun(eventCaller.currentEntity.beat); },
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
                    }
                },
                new GameAction("customBoing", "Custom Boing")
                {
                    //function = delegate {},
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
        };


        [SerializeField] Animator VultureAnim;
        [SerializeField] Animator RavenAnim;


        public enum WhoBops
        {
            None,
            Kasuke,
            Kosuke,
            Both,
        }

        public enum BoingType
        {
            Normal,
            Boing,
            Random,
        }

        public enum Puns
        {
            AichiniAichinna,
            AmmeteAmena,
            ChainaniNichaina,
            DenwariDenwa,                    //short animation
            FutongaFuttonda,
            HiromegaHirameida,
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
            UmetteUmena,
        }


        public static Manzai instance;


        public void Awake()
        {
            instance = this;
        }

        public void Bop()
        {
            VultureAnim.DoScaledAnimationAsync("Bop", 0.5f);
            RavenAnim.DoScaledAnimationAsync("Bop", 0.5f);
        }
        public void DoPun(double beat)
        {
            SoundByte.PlayOneShotGame("manzai/"+sfxDefs[0].sfx);
            ScheduleInput(beat, 2.5f, InputAction_BasicPress, HaiJust, HaiMiss, Nothing);
            ScheduleInput(beat, 3.0f, InputAction_BasicPress, HaiJust, HaiMiss, Nothing);
        }
        public void HaiJust(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("manzai/hai");
            SoundByte.PlayOneShotGame("manzai/haiAccent");
            RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
        }
        public void HaiMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("manzai/hai");
            RavenAnim.DoScaledAnimationAsync("Talk", 0.5f);
        }
        public void Nothing(PlayerActionEvent caller)
        {
            
        }
    }
}
