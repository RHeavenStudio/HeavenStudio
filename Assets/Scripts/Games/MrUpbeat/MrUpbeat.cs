using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using DG.Tweening;
using TMPro;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbUpbeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("mrUpbeat", "Mr. Upbeat", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("stepping", "Start Stepping")
                {
                    preFunctionLength = 0.5f,
                    preFunction = delegate {var e = eventCaller.currentEntity; MrUpbeat.Stepping(e.beat, e.length); },
                    defaultLength = 4f,
                    resizable = true,
                },
                new GameAction("blipping", "Beeping")
                {
                    function = delegate {var e = eventCaller.currentEntity; MrUpbeat.Blipping(e.beat, e.length); },
                    defaultLength = 4f,
                    resizable = true,
                    inactiveFunction = delegate {var e = eventCaller.currentEntity; MrUpbeat.Blipping(e.beat, e.length); },
                },
                new GameAction("ding!", "Ding!")
                {
                    function = delegate { MrUpbeat.instance.Ding(eventCaller.currentEntity["toggle"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Applause")
                    }
                },
                new GameAction("changeBG", "Change Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; MrUpbeat.instance.FadeBackgroundColor(e["start"], e["end"], e.length, e["toggle"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", new Color(0.878f, 0.878f, 0.878f), "Start Color", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("end", new Color(0.878f, 0.878f, 0.878f), "End Color", "The end color for the fade."),
                        new Param("toggle", false, "Instant", "Should the background instantly change color?")
                    }
                },
                new GameAction("upbeatColors", "Upbeat Colors")
                {
                    function = delegate {var e = eventCaller.currentEntity; MrUpbeat.instance.UpbeatColors(e["blipColor"], e["setShadow"], e["shadowColor"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("blipColor", new Color(0, 1f, 0), "Blip Color", "Change blip color"),
                        new Param("setShadow", false, "Set Shadow Color?", "Should Mr. Upbeat's shadow be custom?"),
                        new Param("shadowColor", new Color(1f, 1f, 1f, 0), "Shadow Color", "If \"Set Shadow Color\" is checked, this will set the shadow's color."),
                    }
                },
                new GameAction("blipEvents", "Blip Events")
                {
                    function = delegate {var e = eventCaller.currentEntity; MrUpbeat.instance.BlipEvents(e.beat, e["letter"], e["shouldGrow"], e["resetBlip"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("letter", "", "Letter To Appear", "Which letter to appear on the blip"),
                        new Param("shouldGrow", true, "Grow Antenna?", "Should Mr. Upbeat's antenna grow?"),
                        new Param("resetBlip", false, "Reset Antenna?", "Should Mr. Upbeat's antenna reset?"),
                    }
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MrUpbeat;

    public class MrUpbeat : Minigame
    {
        static List<float> queuedBeeps = new List<float>();
        static List<queuedUpbeatInputs> queuedInputs = new List<queuedUpbeatInputs>();
        public struct queuedUpbeatInputs
        {
            public float beat;
            public bool goRight;
        }

        [Header("References")]
        [SerializeField] Animator metronomeAnim;
        [SerializeField] UpbeatMan man;
        [SerializeField] Material blipMaterial;
        [SerializeField] SpriteRenderer blipSr;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] SpriteRenderer[] shadowSr;
        [SerializeField] Sprite[] blips;
        [SerializeField] TMP_Text blipText;
        [SerializeField] GameObject canvas;

        [Header("Properties")]
        bool startLeft;
        Tween bgColorTween;
        int blipSize = 0;
        string blipString = "M";

        public static MrUpbeat instance;

        private void Awake()
        {
            instance = this;
            
            blipMaterial.SetColor("_ColorBravo", new Color(0, 1f, 0));
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
                if (queuedBeeps.Count > 0) queuedBeeps.Clear();
            }
        }

        public void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedInputs.Count > 0)
                {
                    foreach (var input in queuedInputs)
                    {
                        ScheduleInput(cond.songPositionInBeats, input.beat - cond.songPositionInBeats, InputType.STANDARD_DOWN, Success, Miss, Nothing);
                        if (input.goRight)
                        {
                            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(input.beat - 0.5f, delegate { MrUpbeat.instance.metronomeAnim.DoScaledAnimationAsync("MetronomeGoLeft", 0.5f); }),
                                new BeatAction.Action(input.beat - 0.5f, delegate { Jukebox.PlayOneShotGame("mrUpbeat/metronomeRight"); }),
                            });
                        }
                        else
                        {
                            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                            {
                                new BeatAction.Action(input.beat - 0.5f, delegate { MrUpbeat.instance.metronomeAnim.DoScaledAnimationAsync("MetronomeGoRight", 0.5f); }),
                                new BeatAction.Action(input.beat - 0.5f, delegate { Jukebox.PlayOneShotGame("mrUpbeat/metronomeLeft"); }),
                            });
                        }
                    }
                    startLeft = (queuedInputs.Count % 2 != 0);
                    queuedInputs.Clear();
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    man.Step();
                }
            }

            if (queuedBeeps.Count > 0) {
                var beepAnims = new List<BeatAction.Action>();
                foreach (var item in queuedBeeps) {
                    beepAnims.Add(new BeatAction.Action(item, delegate { 
                        man.blipAnimator.Play("Blip"+(blipSize+1), 0, 0);
                        if (blipSize == 3 && blipString != "")  blipText.text = blipString;
                        canvas.SetActive(blipSize == 3);
                    }));
                }
                BeatAction.New(instance.gameObject, beepAnims);
                queuedBeeps.Clear();
            }
        }

        public void Ding(bool applause)
        {
            Jukebox.PlayOneShotGame("mrUpbeat/ding");
            if (applause) Jukebox.PlayOneShot("applause");
        }

        public static void Blipping(float beat, float length)
        {
            List<MultiSound.Sound> beeps = new List<MultiSound.Sound>();

            for (int i = 0; i < length + 1; i++) 
            {
                beeps.Add(new MultiSound.Sound("mrUpbeat/blip", beat + i));
                queuedBeeps.Add(beat + i);
            }

            MultiSound.Play(beeps.ToArray(), forcePlay: true);
        }

        public static void Stepping(float beat, float length)
        {
            if (GameManager.instance.currentGame == "mrUpbeat")
            {
                float offSet = MrUpbeat.instance.startLeft ? 1 : 0;
                for (int i = 0; i < length + 1; i++)
                {
                    MrUpbeat.instance.ScheduleInput(beat - 1, 1 + i, InputType.STANDARD_DOWN, MrUpbeat.instance.Success, MrUpbeat.instance.Miss, MrUpbeat.instance.Nothing);
                    if ((i + offSet) % 2 == 0)
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i - 0.5f, delegate { MrUpbeat.instance.metronomeAnim.DoScaledAnimationAsync("MetronomeGoLeft", 0.5f); }),
                            new BeatAction.Action(beat + i - 0.5f, delegate { Jukebox.PlayOneShotGame("mrUpbeat/metronomeRight"); }),
                        });
                    }
                    else
                    {
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(beat + i - 0.5f, delegate { MrUpbeat.instance.metronomeAnim.DoScaledAnimationAsync("MetronomeGoRight", 0.5f); }),
                            new BeatAction.Action(beat + i - 0.5f, delegate { Jukebox.PlayOneShotGame("mrUpbeat/metronomeLeft"); }),
                        });
                    }
                }
                MrUpbeat.instance.startLeft = ((length + 1) % 2 != 0);
            }
            else
            {
                for (int i = 0; i < length + 1; i++)
                {
                    queuedInputs.Add(new queuedUpbeatInputs
                    {
                        beat = beat + i,
                        goRight = i % 2 == 0
                    });
                }
            }
        }

        public void Success(PlayerActionEvent caller, float state)
        {
            man.Step();
        }

        public void Miss(PlayerActionEvent caller)
        {
            man.Fall();
        }

        bool isPlaying(Animator anim, string stateName)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                    anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                return true;
            else
                return false;
        }

        public void ChangeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);

            if (seconds == 0) {
                bg.color = color;
            } else {
                bgColorTween = bg.DOColor(color, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats, bool instant)
        {
            ChangeBackgroundColor(start, 0f);
            if (!instant) ChangeBackgroundColor(end, beats);
        }

        public void UpbeatColors(Color blipColor, bool setShadow, Color shadowColor)
        {
            blipMaterial.SetColor("_ColorBravo", blipColor);

            if (setShadow) foreach (var shadow in shadowSr)
            {
                shadow.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 1);
            }
        }

        public void BlipEvents(float beat, string inputLetter, bool shouldGrow, bool resetBlip)
        {
            if (shouldGrow && blipSize < 3) blipSize++;
            if (resetBlip) blipSize = 0;
            blipString = inputLetter;
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}