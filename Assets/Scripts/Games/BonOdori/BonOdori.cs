using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class AgbBonOdoriLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("bonOdori", "The☆Bon Odori \n<color=#adadad>(Za☆Bon Odori)</color>", "312B9F", false, false, new List<GameAction>()
            {                   
                new GameAction("bop", "Bop")
                {   
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.ToggleBop(e.beat, e.length, e["toggle"], e["auto"]);},
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if the Donpans and Yagura-chan should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if the Donpans and Yagura-chan should automatically bop until another Bop event is reached."),
                    },
                },

                new GameAction("pan", "Pan")
                {
                    preFunction = delegate
                    {
                        var e = eventCaller.currentEntity;
                        string variation = "variation" + (new string[] { "Pan", "Pa", "Pa_n" })[e["type"]];
                        BonOdori.instance.PreClap(e.beat, e[variation], e["type"], e["mute"],e["clapType"], e["semitone"]);
                    },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute Sound", "Mute the voice line."),
                        new Param("type",BonOdori.typePan.Pan, "Type", "Set the type of voice line.", new(){
                            new((x, _) => (int)x == 0, new string[] { "variationPan"}),
                            new((x, _) => (int)x == 1, new string[] { "variationPa"}),
                            new((x, _) => (int)x == 2, new string[] { "variationPa_n"}),
                        }),
                        new Param("variationPan", BonOdori.variationPan.PanC, "Pan Type", "Set the variation of the voice line."),
                        new Param("variationPa", BonOdori.variationPa.PaG, "Pa Type", "Set the variation of the voice line."),
                        new Param("variationPa_n", BonOdori.variationPa_n.Pa_nA , "Pa-n Type", "Set the variation of the voice line."),
                        new Param("clapType", BonOdori.typeClap.SideClap, "Clap Type", "Set the type of clap."),
                        new Param("semitone", new EntityTypes.Integer(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0), "Semitone", "Set the number of semitones up or down this note should be pitched."),
                    },
                },

                new GameAction("don", "Don")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        string variation = "variation" + (new string[] { "Don", "Do", "Do_n" })[e["type"]];
                        BonOdori.instance.PlayDon(e.beat, e[variation], e["type"], e["semitone"]);
                    },
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("type",BonOdori.typeDon.Don, "Type", "Set the type of voiceline", new(){
                            new((x, _) => (int)x == 0, new string[] { "variationDon"}),
                            new((x, _) => (int)x == 1, new string[] { "variationDo"}),
                            new((x, _) => (int)x == 2, new string[] { "variationDo_n"}),
                        }),
                        new Param("variationDon", BonOdori.variationDon.DonA, "Don Type", "Set the variation of the voice line."),
                        new Param("variationDo", BonOdori.variationDo.DoC, "Do Type", "Set the variation of the voice line."),
                        new Param("variationDo_n", BonOdori.variationDo_n.Do_nA, "Do-n Type", "Set the variation of the voice line."),
                        new Param("semitone", new EntityTypes.Integer(-EntityTypes.Note.maxSemitones, EntityTypes.Note.maxSemitones, 0), "Semitone", "Set the number of semitones up or down this note should be pitched."),
                    },
                },

                new GameAction("show text", "Show Text")
                {
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.ShowText(e["line 1"], e["line 2"], e["line 3"], e["line 4"], e["line 5"]);},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {   
                        new Param("whichLine", new EntityTypes.Integer(1,5,1), "Line", "Which line to modify.", new()
                        {
                            new((x, _) => (int)x == 1, new string[] { "line 1"}),
                            new((x, _) => (int)x == 2, new string[] { "line 2"}),
                            new((x, _) => (int)x == 3, new string[] { "line 3"}),
                            new((x, _) => (int)x == 4, new string[] { "line 4"}),
                            new((x, _) => (int)x == 5, new string[] { "line 5"}),
                        }),
                        new Param("line 1", "Type r| for red text, g| for green text and y| for yellow text. These can be used multiple times in a single line.", "Line 1", "Set the text for line 1."),
                        new Param("line 2", "", "Line 2", "Set the text for line 2."),
                        new Param("line 3", "", "Line 3", "Set the text for line 3."),
                        new Param("line 4", "", "Line 4", "Set the text for line 4."),
                        new Param("line 5", "", "Line 5", "Set the text for line 5."),
                    },
                    priority = 1
                },
                
                new GameAction("delete text", "Delete Text")
                {
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.DeleteText(e["line 1"], e["line 2"], e["line 3"], e["line 4"], e["line 5"]);},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("line 1", false, "Line 1", "Delete the contents of line 1."),
                        new Param("line 2", false, "Line 2", "Delete the contents of line 2."),
                        new Param("line 3", false, "Line 3", "Delete the contents of line 3."),
                        new Param("line 4", false, "Line 4", "Delete the contents of line 4."),
                        new Param("line 5", false, "Line 5", "Delete the contents of line 5."),
                    },
                },
                
                new GameAction("scroll text", "Scroll Text")
                {
                    function = delegate { var e = eventCaller.currentEntity; BonOdori.instance.ScrollText(e.beat, e.length, e["line 1"], e["line 2"], e["line 3"], e["line 4"], e["line 5"]);},
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("line 1", false, "Line 1", "Scroll the contents of line 1."),
                        new Param("line 2", false, "Line 2", "Scroll the contents of line 2."),
                        new Param("line 3", false, "Line 3", "Scroll the contents of line 3."),
                        new Param("line 4", false, "Line 4", "Scroll the contents of line 4."),
                        new Param("line 5", false, "Line 5", "Scroll the contents of line 5."),
                    },

                },
                
                new GameAction("bow", "Bow")
                {
                    function = delegate { BonOdori.instance.Bow(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);},
                    defaultLength = 2f,
                    resizable = true,

                },

                // new GameAction("spin", "Spin")
                // {
                //     function = delegate { BonOdori.instance.Spin(eventCaller.currentEntity.beat, eventCaller.currentEntity.length);},
                //     defaultLength = 1f,
                // },

                new GameAction("toggle bg", "Toggle Darker Background")
                {
                    function = delegate { BonOdori.instance.DarkBG(eventCaller.currentEntity.beat, eventCaller.currentEntity["toggle"], eventCaller.currentEntity.length);},
                    defaultLength = 1f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Darken Background", "Darkens the background"),
                    }
                },
            },
            new List<string>() { "agb", "normal" }, "agbBonOdori", "en", new List<string>() { }
            );
        }
    };
};
namespace HeavenStudio.Games
{
    public class BonOdori : Minigame
    {
        string prefix;
        double beatUniversal;
        bool noBopPlayer = false;
        bool noBopDonpans = false;
        List<double> noBopBeatsPlayer = new ();
        List<double> noBopBeatsDonpans = new ();
        string suffix;
        SpriteRenderer darkPlane;
        string clapTypeString = "ClapFront";
        string[] originalTexts = new string[5];
        Coroutine[] Scrolls = new Coroutine[5];
        Coroutine DarkerBG;
        bool darkBgIsOn = false;

        [SerializeField] TMP_Text[] Texts;
        [SerializeField] TMP_Text[] TextsBlue;
        [SerializeField] Animator[] Donpans;
        [SerializeField] Animator[] DonpansFace;
        [SerializeField] Animator Judge;
        [SerializeField] Animator JudgeFace;
        [SerializeField] GameObject DarkPlane;

        public enum typeClap
        {
            SideClap = 0,
            FrontClap = 1
        }
        private static List<QueuedClaps> queuedClaps = new();

        private struct QueuedClaps
        {
            public double beat;
            public int variation;
            public int typeSpeak;
            public bool muted;
            public int clapType;
            public int semitone;
        }

        public enum typePan
        {
            Pan = 0,
            Pa = 1,
            Pa_n = 2
        }
        public enum typeDon
        {
            Don = 0,
            Do = 1,
            Do_n = 2
        }

        public enum variationPan
        {
            PanC = 0,
            PanE = 1,
            PanA = 2
        }
        public enum variationPa_n
        {
            Pa_nA = 0,
            Pa_nC = 1
        }
        public enum variationPa
        {
            PaG = 0

        }
        public enum variationDon
        {
            DonA = 0,
            DonD = 1,
            DonC = 2,
            DonG = 3
        }
        public enum variationDo_n
        {
            Do_nA = 0,
            Do_nG = 1
        }
        public enum variationDo
        {
            DoC = 0,
            DoG = 1
        }

        public static BonOdori instance { get; set; }

        public void Awake()
        {
            instance = this;
            SetupBopRegion("bonOdori", "bop", "auto");

            darkPlane = DarkPlane.GetComponent<SpriteRenderer>();
        }

        public void OnStop()
        {
            DarkPlane.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        }

        public override void OnLateBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) Bop(beat);
            noBopBeatsPlayer.RemoveAll(x => x+4 < beat);
            noBopBeatsDonpans.RemoveAll(x => x+4 < beat);
        }

        public void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;
            
            if (!cond.NotStopped())
            {
                for (int i = 0; i < Scrolls.Length; i++)
                {
                    StopCoroutine(Scrolls[i]);
                    Texts[i].text = "";
                    TextsBlue[i].text = "";
                    TextsBlue[i].GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));
                }
                StopCoroutine(DarkerBG);
            }

            if (PlayerInput.GetIsAction(BonOdori.InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                ScoreMiss();
                SoundByte.PlayOneShotGame("bonOdori/clap");

                Donpans[0].DoScaledAnimationAsync(clapTypeString, 0.5f);

                if (clapTypeString is "ClapFront")
                {
                    var currentBeat = Conductor.instance.songPositionInBeatsAsDouble;
                    noBopBeatsPlayer.Add(currentBeat);
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(currentBeat, delegate {
                            noBopPlayer = true;
                        }),
                        new BeatAction.Action(currentBeat + 2d, delegate {
                            if (noBopBeatsPlayer[^1] == currentBeat) noBopPlayer = false;
                        })
                    });      
            }
            }

        }
        
        public override void OnGameSwitch(double beat)
        {
            if (queuedClaps.Count > 0)
            {
                foreach (var clap in queuedClaps) Clap(clap.beat, clap.variation, clap.typeSpeak, clap.muted, clap.clapType, clap.semitone);
                queuedClaps.Clear();
            }
        }

        public void PreClap(double beat, int variation, int typeSpeak, bool muted, int clapType, int semitone)
        {
            if (GameManager.instance.currentGame == "bonOdori")
            {
                instance.Clap(beat, variation, typeSpeak, muted, clapType, semitone);
            }
            else
            {
                queuedClaps.Add(new QueuedClaps()
                {
                    beat = beat,
                    variation = variation,
                    typeSpeak = typeSpeak,
                    muted = muted,
                    clapType = clapType,
                    semitone = semitone
                });
            }
        }

        public void Clap(double beat, int variation, int typeSpeak, bool muted, int clapType, int semitone)
        {
            if (!muted)
            {
                string clip = typeSpeak switch
                {
                    0 => "pan",
                    1 => "pa",
                    2 or _ => "pa_n",
                };
                var pitch = SoundByte.GetPitchFromSemiTones(semitone, true);
                SoundByte.PlayOneShotGame($"bonOdori/" + clip + (variation + 1), beat, pitch);  
            }
            SoundByte.PlayOneShotGame("bonOdori/clap2", beat, volume: 0.5f);

            beatUniversal = beat;
            noBopBeatsDonpans.Add(beatUniversal);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 0.1d, delegate {
                    clapTypeString = clapType switch {
                        0 => "ClapSide",
                        1 or _ => "ClapFront",
                    };
                }),
                new BeatAction.Action(beat, delegate {
                    foreach (var chara in Donpans[1..Donpans.Length]) {
                        chara.DoScaledAnimationAsync(clapTypeString, 0.5f);
                    }
                }),
                new BeatAction.Action(beat + 0.05d, delegate {
                    if (clapTypeString is "ClapFront") noBopDonpans = true;
                }),
                new BeatAction.Action(beat + 1.01d, delegate {
                    if (clapTypeString is "ClapFront") noBopDonpans = false;
                }),
            });
            ScheduleInput(beat, 0f, InputAction_BasicPress, Success, Miss, Empty);
        }

        public void PlayDon(double beat, int variation, int typeSpeak, int semitone)
        {
            string clip = typeSpeak switch
            {
                0 => "don",
                1 => "do",
                2 or _ => "do_n",

            };
            var pitch = SoundByte.GetPitchFromSemiTones(semitone, true);

            SoundByte.PlayOneShotGame($"bonOdori/" + clip + (variation + 1), beat, pitch);
            var firstPan = EventCaller.GetAllInGameManagerList("bonOdori", new string[] { "pan" }).Find(x => x.beat >= beat);
            if (firstPan is not null)
            {
                clapTypeString = firstPan["clapType"] switch {
                    0 => "ClapSide",
                    1 or _ => "ClapFront",
                };
            }
        }


        public void Success(PlayerActionEvent caller, float state)
        {
            Donpans[0].DoScaledAnimationAsync(clapTypeString, 0.5f);
            if (state <= -1f || state >= 1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                return;
            }
            SoundByte.PlayOneShotGame("bonOdori/clap");

            var currentBeat = Conductor.instance.songPositionInBeatsAsDouble;
            double closest = noBopBeatsDonpans.Aggregate((x, y) => Math.Abs(x - currentBeat) < Math.Abs(y - currentBeat) ? x : y);
            noBopBeatsPlayer.Add(closest);
            if (clapTypeString is "ClapFront")
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(closest + 0.05d, delegate {
                        noBopPlayer = true;
                    }),
                    new BeatAction.Action(closest + 1.01d, delegate {
                        if (noBopBeatsPlayer[^1] == closest) noBopPlayer = false;
                    })
                });      
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            // SoundByte.PlayOneShot("miss");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beatUniversal + 1d, delegate { JudgeFace.Play("Sad");}),
                new BeatAction.Action(beatUniversal + 3d, delegate { JudgeFace.Play("Neutral");})
            });

        }

        public void Empty(PlayerActionEvent caller) { }

        string ConvertText(string text, bool isScroll = false)
        {
            if (text.Contains("r|") | text.Contains("y|") | text.Contains("g|") | text.Contains("s|") | text.Contains("d|"))
            {
                if (!isScroll)
                {
                    return text.Replace("r|", "<color=#ff0000>")
                                .Replace("g|", "<color=#00ff00>")
                                .Replace("y|", "<color=#ffff00>")
                                .Replace("s|", "<size=0.9375>")
                                .Replace("|s", "</size>")
                                .Replace("d|", "")
                                + "</color>";
                }
                else
                {
                    return text.Replace("r|", "<color=#ff00ff>")
                                .Replace("g|", "<color=#00ffff>")
                                .Replace("y|", "<color=#ffffff>")
                                .Replace("s|", "<size=0.9375>")
                                .Replace("|s", "</size>")
                                .Replace("d|", "")
                                + "</color>";
                }
            }
            return text;
        }

        int[] GetTextPositions(string text)
        {
            string preSplitedText =  text.Replace("r|", "")
                                        .Replace("g|", "")
                                        .Replace("y|", "")
                                        .Replace("s|", "")
                                        .Replace("|s", "");
            string[] parts = preSplitedText.Split(new string[] { "d|" }, StringSplitOptions.None);

            List<int> positions = new List<int>();
            int currentPosition = 0;
            foreach (var part in parts)
            {
                positions.Add(currentPosition);
                currentPosition += part.Length;
            }

            return positions.ToArray();
        }

        public void ShowText(string text1, string text2, string text3, string text4, string text5)
        {
            var texts = new string[]{text1, text2, text3, text4, text5};

            for (int i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                if (text is not "" and not "Type r| for red text, g| for green text and y| for yellow text. These can be used multiple times in a single line.")
                {
                    var scroll = Scrolls[i];
                    var textTMP = Texts[i];
                    var textBlueTMP = TextsBlue[i];

                    if (scroll is not null)
                    {
                        StopCoroutine(scroll);
                        scroll = null;
                    }
                    textBlueTMP.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10f, -10f, -10f, 10));

                    originalTexts[i] = text;
                    text = ConvertText(originalTexts[i], false);

                    textTMP.text = text;
                    textBlueTMP.text = ConvertText(originalTexts[i], true);
                }
            }
        }

        public void DeleteText(bool text1, bool text2, bool text3, bool text4, bool text5)
        {
            var texts = new bool[]{text1, text2, text3, text4, text5};

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == true)
                {
                    var scroll = Scrolls[i];
                    var textTMP = Texts[i];
                    var textBlueTMP = TextsBlue[i];

                    if (scroll is not null)
                    {
                        StopCoroutine(scroll);
                        scroll = null;
                    }
                    textBlueTMP.GetComponent<TextMeshPro>().SetMask(0, new Vector4(-10, -10, -10, 10));
                    textTMP.text = "";
                    textBlueTMP.text = "";
                }
            }
        }

        IEnumerator SmoothText(double beat, float length, TextMeshPro textTMP, float[] maskEdges)
        {
            var cond = Conductor.instance;
            float startTime = Time.time;
            float endTime = startTime + length;
            float duration = ((length / cond.GetBpmAtBeat(beat)) * 60);

            int len = maskEdges.Length;

            while (Time.time < endTime)
            {
                float t = ((Time.time - startTime) / duration);
                
                int index = Math.Max(0, Math.Min((int)Math.Floor(t * (len-1)), len-2));
                t = t * (len-1) - index;
                float maskValue = Mathf.Lerp(maskEdges[index], maskEdges[index+1], t);
                textTMP.SetMask(0, new Vector4(-10, -10, maskValue, 10));

                yield return null;
            }
        }

        public void ScrollText(double beat, float length, bool text1, bool text2, bool text3, bool text4, bool text5)
        {
            var texts = new bool[]{text1, text2, text3, text4, text5};

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i])
                {
                    var textTMP = TextsBlue[i].GetComponent<TextMeshPro>();

                    int[] positions = GetTextPositions(originalTexts[i]);
                    var maskEdges = new List<float>();
                    foreach (var pos in positions)
                    {
                        maskEdges.Add(textTMP.textInfo.characterInfo[pos].topLeft.x);
                    }
                    maskEdges[0] = -textTMP.rectTransform.sizeDelta.x/2;
                    maskEdges.Add(maskEdges[0] + textTMP.preferredWidth);

                    Scrolls[i] = StartCoroutine(SmoothText(beat, length, textTMP, maskEdges.ToArray()));
                }
            }
        }

        public void ToggleBop(double beat, float length, bool bopOrNah, bool autoBop)
        {
            if (autoBop) return;
            if (bopOrNah)
            {
                for (int i = 0; i < length; i++)
                {
                    double bopBeat = beat + i;
                    BeatAction.New(instance, new() {new BeatAction.Action(bopBeat, delegate { Bop(bopBeat);})});
                }
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length, delegate
                    {
                        if (!noBopBeatsDonpans.Any(x => Math.Abs(x - (beat + length)) <= double.Epsilon)) {
                            foreach (var chara in Donpans) {
                                chara.Play("NeutralBopped");
                            }
                        }
                    })
                });                
            }
        }

        private void Bop(double beat)
        {
            if (!noBopPlayer & !noBopBeatsPlayer.Any(x => Math.Abs(x - beat) <= double.Epsilon))
            {
                if (!Donpans[0].IsPlayingAnimationNames("ClapSide", "ClapFront")) {
                    Donpans[0].DoScaledAnimationAsync("Bop", 0.5f);
                }
            }
            if (!noBopDonpans & !noBopBeatsDonpans.Any(x => Math.Abs(x - beat) <= double.Epsilon))
            {
                foreach (var chara in Donpans[1..Donpans.Length]) {
                    if (!chara.IsPlayingAnimationNames("ClapSide", "ClapFront")) {
                        chara.DoScaledAnimationAsync("Bop", 0.5f);
                    }
                }
            }
            Judge.DoScaledAnimationAsync("Bop", 0.5f);
        }

        public void Bow(double beat, float length)
        {
            noBopPlayer = true;
            noBopDonpans = true;
            foreach (var chara in Donpans) {
                chara.Play("Bow");
            }
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + length, delegate {
                    noBopPlayer = false;
                    noBopDonpans = false;
                    if (!noBopBeatsPlayer.Any(x => Math.Abs(x - (beat + length)) <= double.Epsilon)) {
                        Donpans[0].Play("NeutralBopped");
                        if (BeatIsInBopRegion(beat + length)) Bop(beat + length);
                    }
                    if (!noBopBeatsDonpans.Any(x => Math.Abs(x - (beat + length)) <= double.Epsilon)) {
                        foreach (var chara in Donpans[1..Donpans.Length]) {
                            chara.Play("NeutralBopped");
                        }
                        if (BeatIsInBopRegion(beat + length)) Bop(beat + length);
                    }
                })
            });
        }

        // public void Spin(double beat, float length)
        // {

        // }
        
        public void DarkBG(double beat, bool toggle, float length)
        {
            DarkerBG = StartCoroutine(DarkBGCoroutine(beat, toggle, length));
        }

        IEnumerator DarkBGCoroutine(double beat, bool toggle, float length)
        {
            if (toggle)
            {
                if (darkBgIsOn)
                {
                    yield return null;
                }
                else
                {
                    float startTime = Time.time;
                    var cond = Conductor.instance;
                    float realLength = length / cond.GetBpmAtBeat(beat) * 60;
                    while (Time.time < realLength + startTime)
                    {
                        darkPlane.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 0.4666f, (Time.time - startTime) / realLength));
                        darkBgIsOn = true;
                        yield return null;
                    }
                }
            }
            else
            {
                if (!darkBgIsOn)
                {
                    yield return null;
                }
                else
                {
                    float startTime = Time.time;
                    var cond = Conductor.instance;
                    float realLength = length / cond.GetBpmAtBeat(beat) * 60;
                    while (Time.time < realLength + startTime)
                    {
                        darkPlane.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.4666f, 0f, (Time.time - startTime) / realLength));
                        darkBgIsOn = true;
                        yield return null;
                    }
                }
            }
        }
    }
}