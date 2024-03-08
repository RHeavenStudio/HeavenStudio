using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlTambourineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tambourine", "Tambourine", "388cd0", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.Bop(e.beat, e.length, e["whoBops"], e["whoBopsAuto"]); },
                    parameters = new List<Param>()
                    {
                        new Param("whoBops", Tambourine.WhoBops.Both, "Bop", "Set the character(s) to bop for the duration of this event."),
                        new Param("whoBopsAuto", Tambourine.WhoBops.None, "Bop (Auto)", "Set the character(s) to automatically bop until another Bop event is reached."),
                    },
                    resizable = true,
                    priority = 4
                },
                new GameAction("beat intervals", "Start Interval")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Tambourine.PreInterval(e.beat, e.length, e["auto"]); },
                    defaultLength = 7f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("auto", true, "Auto Pass Turn", "Toggle if the turn should be passed automatically at the end of the start interval.")
                    }
                },
                new GameAction("shake", "Shake")
                {
                    defaultLength = 0.5f,
                },
                new GameAction("hit", "Hit")
                {
                    defaultLength = 0.5f,
                },
                new GameAction("pass turn", "Pass Turn")
                {
                    preFunction = delegate
                    {
                        Tambourine.PrePassTurn(eventCaller.currentEntity.beat);
                    },
                    resizable = true,
                    preFunctionLength = 1f
                },
                new GameAction("success", "Success")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.SuccessFace(e.beat); },
                    defaultLength = 1f,
                    priority = 4,
                },
                new GameAction("fade background", "Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.BackgroundColor(e.beat, e.length, e["colorStart"], e["colorEnd"], e["ease"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorStart", Color.white, "Start Color", "Set the color at the start of the event."),
                        new Param("colorEnd", Tambourine.defaultBGColor, "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    }
                },
            },
            new List<string>() {"rvl", "repeat"},
            "rvldrum", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    public class Tambourine : Minigame
    {
        public static Color defaultBGColor = new Color(0.22f, 0.55f, 0.82f);

        [Header("Components")]
        [SerializeField] Animator handsAnimator;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] Animator monkeyAnimator;
        [SerializeField] ParticleSystem flowerParticles;
        [SerializeField] GameObject happyFace;
        [SerializeField] GameObject sadFace;
        [SerializeField] Animator sweatAnimator;
        [SerializeField] Animator frogAnimator;

        [Header("Variables")]
        float misses;
        bool frogPresent;
        bool monkeyGoBop;
        bool handsGoBop;
        public GameEvent bop = new GameEvent();

        public enum WhoBops
        {
            Monkey,
            Player,
            Both,
            None
        }

        public static Tambourine instance;

        const int IA_AltPress = IAMAXCAT;
        protected static bool IA_TouchNrmPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && !instance.IsExpectingInputNow(InputAction_Alt);
        }

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_Alt);
        }

        public static PlayerInput.InputAction InputAction_Nrm =
            new("RvlDrumAlt", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchNrmPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_Alt =
            new("RvlDrumAlt", new int[] { IA_AltPress, IA_AltPress, IA_AltPress },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);

        void Awake()
        {
            instance = this;
            sweatAnimator.Play("NoSweat", 0, 0);
            frogAnimator.Play("FrogExited", 0, 0);
            handsAnimator.Play("Idle", 0, 0);
            monkeyAnimator.Play("MonkeyIdle", 0, 0);
        }

        void Update()
        {
            BackgroundColorUpdate();
            if (Conductor.instance.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (monkeyGoBop)
                {
                    monkeyAnimator.Play("MonkeyBop", 0, 0);
                }
                if (handsGoBop)
                {
                    handsAnimator.Play("Bop", 0, 0);
                }
            }
            if (PlayerInput.GetIsAction(InputAction_Nrm) && !IsExpectingInputNow(InputAction_Nrm))
            {
                handsAnimator.Play("Shake", 0, 0);
                SoundByte.PlayOneShotGame($"tambourine/player/shake/{UnityEngine.Random.Range(1, 6)}");
                sweatAnimator.Play("Sweating", 0, 0);
                SummonFrog();
                ScoreMiss();
                if (!IntervalIsGoingOn())
                {
                    sadFace.SetActive(true);
                }
            }
            else if (PlayerInput.GetIsAction(InputAction_Alt) && !IsExpectingInputNow(InputAction_Alt))
            {
                handsAnimator.Play("Smack", 0, 0);
                SoundByte.PlayOneShotGame($"tambourine/player/hit/{UnityEngine.Random.Range(1, 6)}");
                sweatAnimator.Play("Sweating", 0, 0);
                SummonFrog();
                ScoreMiss();
                if (!IntervalIsGoingOn())
                {
                    sadFace.SetActive(true);
                }
            }

            if (passedTurns.Count > 0)
            {
                foreach (var pass in passedTurns)
                {
                    PassTurnStandalone(pass);
                }
                passedTurns.Clear();
            }
        }

        private bool IntervalIsGoingOn()
        {
            double beat = Conductor.instance.songPositionInBeats;
            return EventCaller.GetAllInGameManagerList("tambourine", new string[] { "beat intervals" }).Find(x => beat >= x.beat && beat < x.beat + x.length) != null;
        }

        private List<RiqEntity> GetAllInputsBetweenBeat(double beat, double endBeat)
        {
            return EventCaller.GetAllInGameManagerList("tambourine", new string[] { "shake", "hit" }).FindAll(x => x.beat >= beat && x.beat < endBeat);
        }

        public static void PreInterval(double beat, float interval, bool autoPassTurn)
        {
            if (GameManager.instance.currentGame == "tambourine")
            {
                instance.StartInterval(beat, interval, beat, autoPassTurn);
            }
            else
            {
                queuedIntervals.Add(new QueuedInterval()
                {
                    beat = beat,
                    interval = interval,
                    autoPassTurn = autoPassTurn
                });
            }
        }

        public override void OnGameSwitch(double beat)
        {
            if (queuedIntervals.Count > 0)
            {
                foreach (var interval in queuedIntervals)
                {
                    StartInterval(interval.beat, interval.interval, beat, interval.autoPassTurn);
                }
                queuedIntervals.Clear();
            }
            PersistColor(beat);
        }

        private struct QueuedInterval
        {
            public double beat;
            public float interval;
            public bool autoPassTurn;
        }

        private static List<QueuedInterval> queuedIntervals = new();

        private void StartInterval(double beat, float interval, double gameSwitchBeat, bool autoPassTurn)
        {
            List<BeatAction.Action> actions = new()
            {
                new BeatAction.Action(beat, delegate
                {
                    DesummonFrog();
                    sadFace.SetActive(false);
                })
            };

            var relevantInputs = GetAllInputsBetweenBeat(beat, beat + interval);
            relevantInputs.Sort((x, y) => x.beat.CompareTo(y.beat));

            List<MultiSound.Sound> sounds = new();
            for (int i = 0; i < relevantInputs.Count; i++)
            {
                bool isHit = relevantInputs[i].datamodel == "tambourine/hit";
                double inputBeat = relevantInputs[i].beat;
                if (inputBeat >= gameSwitchBeat)
                {
                    actions.Add(new BeatAction.Action(inputBeat, delegate
                    {
                        MonkeyInput(inputBeat, isHit);
                    }));
                    sounds.Add(new MultiSound.Sound($"tambourine/monkey/{(isHit ? "hit" : "shake")}/m{(isHit ? "h" : "s")}{UnityEngine.Random.Range(1, 6)}", inputBeat));
                }
            }

            BeatAction.New(this, actions);
            MultiSound.Play(sounds.ToArray(), true, true);

            if (autoPassTurn)
            {
                PassTurn(beat + interval, beat, interval, 1);
            }
        }

        public void MonkeyInput(double beat, bool hit)
        {
            if (hit)
            {
                monkeyAnimator.DoScaledAnimationAsync("MonkeySmack", 0.5f);
            }
            else
            {
                monkeyAnimator.DoScaledAnimationAsync("MonkeyShake", 0.5f);
            }
        }

        private RiqEntity GetLastIntervalBeforeBeat(double beat)
        {
            return EventCaller.GetAllInGameManagerList("tambourine", new string[] { "beat intervals" }).FindLast(x => x.beat <= beat);
        }

        public static void PrePassTurn(double beat)
        {
            if (GameManager.instance.currentGame == "tambourine")
            {
                instance.PassTurnStandalone(beat);
            }
            else
            {
                passedTurns.Add(beat);
            }
        }

        private static List<double> passedTurns = new();

        private void PassTurnStandalone(double beat)
        {
            var lastInterval = GetLastIntervalBeforeBeat(beat);
            float length = EventCaller.GetAllInGameManagerList("tambourine", new string[] { "pass turn" }).Find(x => x.beat == beat).length;
            if (lastInterval != null) PassTurn(beat, lastInterval.beat, lastInterval.length, length);
        }

        private void PassTurn(double beat, double intervalBeat, float intervalLength, float length)
        {
            SoundByte.PlayOneShotGame($"tambourine/monkey/turnPass/tp{UnityEngine.Random.Range(1, 6)}", beat);
            List<BeatAction.Action> actions = new()
            {
                new BeatAction.Action(beat, delegate
                {
                                monkeyAnimator.DoScaledAnimationAsync("MonkeyPassTurn", 0.5f);
                    happyFace.SetActive(true);
                }),
                new BeatAction.Action(beat + 0.3f, delegate { happyFace.SetActive(false); })
            };
            var relevantInputs = GetAllInputsBetweenBeat(intervalBeat, intervalBeat + intervalLength);
            relevantInputs.Sort((x, y) => x.beat.CompareTo(y.beat));
            for (int i = 0; i < relevantInputs.Count; i++)
            {
                bool isHit = relevantInputs[i].datamodel == "tambourine/hit";
                double inputBeat = relevantInputs[i].beat - intervalBeat;
                actions.Add(new BeatAction.Action(inputBeat, delegate
                {
                    if (isHit) ScheduleInput(beat + length, inputBeat, InputAction_Alt, JustHit, Miss, Nothing);
                    else ScheduleInput(beat + length, inputBeat, InputAction_Nrm, JustShake, Miss, Nothing);
                    Bop(beat + length + inputBeat, 1, (int)WhoBops.Monkey, (int)WhoBops.None);
                }));
            }
            BeatAction.New(this, actions);
        }

        public void Bop(double beat, float length, int whoBops, int whoBopsAuto)
        {
            monkeyGoBop = whoBopsAuto == (int)WhoBops.Monkey || whoBopsAuto == (int)WhoBops.Both;
            handsGoBop = whoBopsAuto == (int)WhoBops.Player || whoBopsAuto == (int)WhoBops.Both;
            for (int i = 0; i < length; i++)
            {
                BeatAction.New(instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate
                    {
                        switch (whoBops)
                        {
                            case (int) WhoBops.Monkey:
                                monkeyAnimator.DoScaledAnimationAsync("MonkeyBop", 0.5f);
                                break;
                            case (int) WhoBops.Player:
                                handsAnimator.DoScaledAnimationAsync("Bop", 0.5f);
                                break;
                            case (int) WhoBops.Both:
                                monkeyAnimator.DoScaledAnimationAsync("MonkeyBop", 0.5f);
                                handsAnimator.DoScaledAnimationAsync("Bop", 0.5f);
                                break;
                            default: 
                                break;
                        }
                    })
                });
            }

        }

        public void SuccessFace(double beat)
        {
            DesummonFrog();
            if (misses > 0) return;
            flowerParticles.Play();
            SoundByte.PlayOneShotGame($"tambourine/player/turnPass/sweep");
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tambourine/player/turnPass/note1", beat),
                new MultiSound.Sound("tambourine/player/turnPass/note2", beat + 0.1f),
                new MultiSound.Sound("tambourine/player/turnPass/note3", beat + 0.2f),
                new MultiSound.Sound("tambourine/player/turnPass/note3", beat + 0.3f),
            }, forcePlay: true);
            happyFace.SetActive(true);
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate { happyFace.SetActive(false); }),
            });
        }

        public void JustHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                handsAnimator.DoScaledAnimationAsync("Smack", 0.5f);
                SoundByte.PlayOneShotGame($"tambourine/player/hit/ph{UnityEngine.Random.Range(1, 6)}");
                SoundByte.PlayOneShotGame("tambourine/miss");
                sweatAnimator.DoScaledAnimationAsync("Sweating", 0.5f);
                misses++;
                if (!IntervalIsGoingOn())
                {
                    sadFace.SetActive(true);
                }
                return;
            }
            Success(true);
        }

        public void JustShake(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                handsAnimator.DoScaledAnimationAsync("Shake", 0.5f);
                SoundByte.PlayOneShotGame($"tambourine/player/shake/ps{UnityEngine.Random.Range(1, 6)}");
                SoundByte.PlayOneShotGame("tambourine/miss");
                sweatAnimator.DoScaledAnimationAsync("Sweating", 0.5f);
                misses++;
                if (!IntervalIsGoingOn())
                {
                    sadFace.SetActive(true);
                }
                return;
            }
            Success(false);
        }

        public void Success(bool hit)
        {
            sadFace.SetActive(false);
            if (hit)
            {
                handsAnimator.DoScaledAnimationAsync("Smack", 0.5f);
                SoundByte.PlayOneShotGame($"tambourine/player/hit/ph{UnityEngine.Random.Range(1, 6)}");
            }
            else
            {
                handsAnimator.DoScaledAnimationAsync("Shake", 0.5f);
                SoundByte.PlayOneShotGame($"tambourine/player/shake/ps{UnityEngine.Random.Range(1, 6)}");
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            SummonFrog();
            sweatAnimator.DoScaledAnimationAsync("Sweating", 0.5f);
            misses++;
            if (!IntervalIsGoingOn())
            {
                sadFace.SetActive(true);
            }
        }

        private ColorEase bgColorEase = new(defaultBGColor);

        //call this in update
        private void BackgroundColorUpdate()
        {
            bg.color = bgColorEase.GetColor();
        }

        public void BackgroundColor(double beat, float length, Color startColor, Color endColor, int ease)
        {
            bgColorEase = new(beat, length, startColor, endColor, ease);
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("tambourine", new string[] { "fade background" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["colorStart"], lastEvent["colorEnd"], lastEvent["ease"]);
            }
        }

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }

        public void SummonFrog()
        {
            if (frogPresent) return;
            SoundByte.PlayOneShotGame("tambourine/frog");
            frogAnimator.Play("FrogEnter", 0, 0);
            frogPresent = true;
        }

        public void DesummonFrog()
        {
            if (!frogPresent) return;
            frogAnimator.Play("FrogExit", 0, 0);
            frogPresent = false;
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}