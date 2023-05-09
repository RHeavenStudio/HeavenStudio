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
                new GameAction("start stepping", "Start Stepping")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; MrUpbeat.StartStepping(e.beat, e.length, e["force"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("force", false, "Force Onbeat", "Force Mr. Upbeat to step on the offbeats")
                    },
                    resizable = true,
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
                    function = delegate {var e = eventCaller.currentEntity; MrUpbeat.instance.BlipEvents(e["letter"], e["shouldGrow"], e["resetBlip"], e["beep"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("letter", "", "Letter To Appear", "Which letter to appear on the blip"),
                        new Param("shouldGrow", true, "Grow Antenna?", "Should Mr. Upbeat's antenna grow?"),
                        new Param("resetBlip", false, "Reset Antenna?", "Should Mr. Upbeat's antenna reset?"),
                        new Param("beep", true, "Should Beep?", "Should Mr. Upbeat beep every offbeat?"),
                    }
                },

                // these are hidden in the editor but blah blah blah blah
                new GameAction("stepping", "Start Stepping")
                {
                    preFunctionLength = 0.5f,
                    preFunction = delegate {var e = eventCaller.currentEntity; MrUpbeat.StartStepping(e.beat, 0, false); },
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
        static List<float> queuedInputs = new List<float>();

        [Header("References")]
        [SerializeField] Animator metronomeAnim;
        [SerializeField] UpbeatMan man;
        [SerializeField] Material blipMaterial;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] SpriteRenderer[] shadowSr;
        [SerializeField] TMP_Text blipText;
        public GameObject textObj;

        [Header("Properties")]
        private Tween bgColorTween;
        private int blipSize = 0;
        private int stepIterate = 0;
        private string blipString = "M";
        private bool shouldBeep = true;
        private bool autoStep;
        private float lastReportedBeat = 0f;

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
            if (Conductor.instance.ReportBeat(ref lastReportedBeat, 0.5f) && shouldBeep)
            {
                Jukebox.PlayOneShotGame("mrUpbeat/blip");
                man.blipAnimator.Play("Blip"+(blipSize+1), 0, 0);
                if (blipSize == 3)  blipText.text = blipString;
                textObj.SetActive(blipSize == 3 && blipString != "");
            }

            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && autoStep)
            {
                bool x = stepIterate % 2 == 1;
                MrUpbeat.instance.metronomeAnim.DoScaledAnimationAsync(x ? "MetronomeGoRight" : "MetronomeGoLeft", 0.5f);
                Jukebox.PlayOneShotGame(x ? "mrUpbeat/metronomeLeft" : "mrUpbeat/metronomeRight");
                ScheduleInput(Conductor.instance.songPositionInBeats, 0.5f, InputType.STANDARD_DOWN, Success, Miss, Nothing);
                stepIterate++;
            }
            
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedInputs.Count > 0)
                {
                    stepIterate = 0;
                    foreach (var input in queuedInputs)
                    {
                        bool x = stepIterate % 2 == 1;
                        BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(input - 0.5f, delegate { 
                                MrUpbeat.instance.metronomeAnim.DoScaledAnimationAsync(x ? "MetronomeGoRight" : "MetronomeGoLeft", 0.5f);
                                Jukebox.PlayOneShotGame(x ? "mrUpbeat/metronomeLeft" : "mrUpbeat/metronomeRight");
                                ScheduleInput(input - 0.5f, 0.5f, InputType.STANDARD_DOWN, Success, Miss, Nothing);
                            }),
                        });
                        stepIterate++;
                    }
                    queuedInputs.Clear();
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN)) {
                    man.Step();
                }
            }
        }

        public void Ding(bool applause)
        {
            Jukebox.PlayOneShotGame("mrUpbeat/ding");
            if (applause) Jukebox.PlayOneShot("applause");
            shouldBeep = false;
        }

        public static void StartStepping(float beat, float length, bool force)
        {
            if (GameManager.instance.currentGame != "mrUpbeat") Blipping(beat, length);
            MrUpbeat.instance.shouldBeep = true;
            var dings = EventCaller.GetAllInGameManagerList("mrUpbeat", new string[] { "ding!" });
            if (dings.Count == 0) {
                MrUpbeat.instance.autoStep = true;
                return;
            }
            int whichDing = 0;
            for (int i = 0; i < dings.Count; i++) {
                if (dings[i].beat > beat) {
                    whichDing = i;
                    break;
                }
            }
            for (int i = 0; i < dings[whichDing].beat-beat + 1; i++) {
                if (i > length) {
                    queuedInputs.Add(force ? beat + i : MathF.Floor(beat) - 0.5f + i);
                }
            }
        }

        public static void Blipping(float beat, float length)
        {
            List<MultiSound.Sound> beeps = new List<MultiSound.Sound>();
            var switchGames = EventCaller.GetAllInGameManagerList("mrUpbeat", new string[] { "switchGame" });
            int whichSwitch = 0;
            if (switchGames.Count != 0) {
                for (int i = 0; i < switchGames.Count; i++) {
                    if (switchGames[i].beat > beat) {
                        whichSwitch = i;
                        break;
                    }
                }
            }

            for (int i = 0; i < switchGames[whichSwitch].beat-beat; i++) 
            {
                beeps.Add(new MultiSound.Sound("mrUpbeat/blip", beat + i));
                queuedBeeps.Add(beat + i);
            }
            MultiSound.Play(beeps.ToArray(), forcePlay: true);
        }

        public static void Stepping(float beat, float length, bool force)
        {
            
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

        public void BlipEvents(string inputLetter, bool shouldGrow, bool resetBlip, bool beep)
        {
            if (shouldGrow && blipSize < 3) blipSize++;
            if (resetBlip) blipSize = 0;
            blipString = inputLetter;
            beep = shouldBeep;
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}