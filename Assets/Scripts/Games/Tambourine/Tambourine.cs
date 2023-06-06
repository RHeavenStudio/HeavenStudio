using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlTambourineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tambourine", "Tambourine", "388cd0", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.StartInterval(e.beat, e.length); },
                    defaultLength = 8f,
                    resizable = true,
                    priority = 1
                },
                new GameAction("shake", "Shake")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.MonkeyInput(e.beat, false); },
                    defaultLength = 0.5f,
                    priority = 2
                },
                new GameAction("hit", "Hit")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.MonkeyInput(e.beat, true); },
                    defaultLength = 0.5f,
                    priority = 2
                },
                new GameAction("pass turn", "Pass Turn")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.PassTurn(e.beat, e.length); },
                    defaultLength = 1f,
                    resizable = true,
                    priority = 3
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.Bop(e.beat, e.length, e["whoBops"], e["whoBopsAuto"]); },
                    parameters = new List<Param>()
                    {
                        new Param("whoBops", Tambourine.WhoBops.Both, "Who Bops", "Who will bop."),
                        new Param("whoBopsAuto", Tambourine.WhoBops.None, "Who Bops (Auto)", "Who will auto bop."),
                    },
                    resizable = true,
                    priority = 4
                },
                new GameAction("success", "Success")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.SuccessFace(e.beat); },
                    defaultLength = 1f,
                    priority = 4,
                },
                new GameAction("fade background", "Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.FadeBackgroundColor(e["colorA"], e["colorB"], e.length, e["instant"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", Color.white, "Start Color", "The starting color of the fade."),
                        new Param("colorB", Tambourine.defaultBGColor, "End Color", "The ending color of the fade."),
                        new Param("instant", false, "Instant", "Instantly set the color of the background to the start color?")
                    }
                },
                //backwards-compatibility
                new GameAction("set background color", "Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.ChangeBackgroundColor(e["colorA"], 0f); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", Tambourine.defaultBGColor, "Background Color", "The background color to change to.")
                    },
                    hidden = true
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
        private static Color _defaultBGColor;
        public static Color defaultBGColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#388cd0", out _defaultBGColor);
                return _defaultBGColor;
            }
        }

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
        bool intervalStarted;
        float intervalStartBeat;
        float beatInterval = 8f;
        float misses;
        bool frogPresent;
        bool monkeyGoBop;
        bool handsGoBop;

        Tween bgColorTween;
        public GameEvent bop = new GameEvent();

        public enum WhoBops
        {
            Monkey,
            Player,
            Both,
            None
        }

        static List<QueuedTambourineInput> queuedInputs = new List<QueuedTambourineInput>();
        struct QueuedTambourineInput
        {
            public bool hit;
            public float beatAwayFromStart;
        }

        public static Tambourine instance;

        void Awake()
        {
            instance = this;
            sweatAnimator.Play("NoSweat", 0, 0);
            frogAnimator.Play("FrogExited", 0, 0);
            handsAnimator.Play("Idle", 0, 0);
            monkeyAnimator.Play("MonkeyIdle", 0, 0);
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        void Update()
        {
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
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
            }
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused && intervalStarted)
            {
                intervalStarted = false;
            }
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                handsAnimator.Play("Shake", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/shake/{UnityEngine.Random.Range(1, 6)}");
                sweatAnimator.Play("Sweating", 0, 0);
                SummonFrog();
                ScoreMiss();
                if (!intervalStarted)
                {
                    sadFace.SetActive(true);
                }
            }
            else if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
            {
                handsAnimator.Play("Smack", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/hit/{UnityEngine.Random.Range(1, 6)}");
                sweatAnimator.Play("Sweating", 0, 0);
                SummonFrog();
                ScoreMiss();
                if (!intervalStarted)
                {
                    sadFace.SetActive(true);
                }
            }
        }

        public void StartInterval(float beat, float interval)
        {
            intervalStartBeat = beat;
            beatInterval = interval;
            if (!intervalStarted)
            {
                DesummonFrog();
                sadFace.SetActive(false);
                //queuedInputs.Clear();
                misses = 0;
                intervalStarted = true;
            }
        }

        public void MonkeyInput(float beat, bool hit)
        {
            if (!intervalStarted)
            {
                StartInterval(beat, beatInterval);
            }
            if (hit)
            {
                monkeyAnimator.Play("MonkeySmack", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/monkey/hit/{UnityEngine.Random.Range(1, 6)}");
            }
            else
            {
                monkeyAnimator.Play("MonkeyShake", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/monkey/shake/{UnityEngine.Random.Range(1, 6)}");
            }
            queuedInputs.Add(new QueuedTambourineInput()
            {
                hit = hit,
                beatAwayFromStart = beat - intervalStartBeat,
            });
        }

        public void PassTurn(float beat, float length)
        {
            if (queuedInputs.Count == 0) return;
            monkeyAnimator.Play("MonkeyPassTurn", 0, 0);
            Jukebox.PlayOneShotGame($"tambourine/monkey/turnPass/{UnityEngine.Random.Range(1, 6)}");
            happyFace.SetActive(true);
            intervalStarted = false;
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.3f, delegate { happyFace.SetActive(false); })
            });
            foreach (var input in queuedInputs)
            {
                if (input.hit)
                {
                    ScheduleInput(beat, length + input.beatAwayFromStart, InputType.STANDARD_ALT_DOWN , JustHit, Miss , Nothing);
                }
                else
                {
                    ScheduleInput(beat, length + input.beatAwayFromStart, InputType.STANDARD_DOWN, JustShake, Miss, Nothing);
                }
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + length + input.beatAwayFromStart, delegate { Bop(beat + length + input.beatAwayFromStart, 1, (int)WhoBops.Monkey, (int)WhoBops.None); })
                });
            }
            queuedInputs.Clear();
        }

        public void Bop(float beat, float length, int whoBops, int whoBopsAuto)
        {
            monkeyGoBop = whoBopsAuto == (int)WhoBops.Monkey || whoBopsAuto == (int)WhoBops.Both;
            handsGoBop = whoBopsAuto == (int)WhoBops.Player || whoBopsAuto == (int)WhoBops.Both;
            for (int i = 0; i < length; i++)
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate
                    {
                        switch (whoBops)
                        {
                            case (int) WhoBops.Monkey:
                                monkeyAnimator.Play("MonkeyBop", 0, 0);
                                break;
                            case (int) WhoBops.Player:
                                handsAnimator.Play("Bop", 0, 0);
                                break;
                            case (int) WhoBops.Both:
                                monkeyAnimator.Play("MonkeyBop", 0, 0);
                                handsAnimator.Play("Bop", 0, 0);
                                break;
                            default: 
                                break;
                        }
                    })
                });
            }

        }

        public void SuccessFace(float beat)
        {
            DesummonFrog();
            if (misses > 0) return;
            flowerParticles.Play();
            Jukebox.PlayOneShotGame($"tambourine/player/turnPass/sweep");
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("tambourine/player/turnPass/note1", beat),
                new MultiSound.Sound("tambourine/player/turnPass/note2", beat + 0.1f),
                new MultiSound.Sound("tambourine/player/turnPass/note3", beat + 0.2f),
                new MultiSound.Sound("tambourine/player/turnPass/note3", beat + 0.3f),
            }, forcePlay: true);
            happyFace.SetActive(true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate { happyFace.SetActive(false); }),
            });
        }

        public void JustHit(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                handsAnimator.Play("Smack", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/hit/{UnityEngine.Random.Range(1, 6)}");
                Jukebox.PlayOneShotGame("tambourine/miss");
                sweatAnimator.Play("Sweating", 0, 0);
                misses++;
                if (!intervalStarted)
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
                handsAnimator.Play("Shake", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/shake/{UnityEngine.Random.Range(1, 6)}");
                Jukebox.PlayOneShotGame("tambourine/miss");
                sweatAnimator.Play("Sweating", 0, 0);
                misses++;
                if (!intervalStarted)
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
                handsAnimator.Play("Smack", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/hit/{UnityEngine.Random.Range(1, 6)}");
            }
            else
            {
                handsAnimator.Play("Shake", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/shake/{UnityEngine.Random.Range(1, 6)}");
            }
        }

        public void Miss(PlayerActionEvent caller)
        {
            SummonFrog();
            sweatAnimator.Play("Sweating", 0, 0);
            misses++;
            if (!intervalStarted)
            {
                sadFace.SetActive(true);
            }
        }

        public void ChangeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);

            if (seconds == 0)
            {
                bg.color = color;
            }
            else
            {
                bgColorTween = bg.DOColor(color, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats, bool instant)
        {
            ChangeBackgroundColor(start, 0f);
            if (!instant) ChangeBackgroundColor(end, beats);
        }

        public void SummonFrog()
        {
            if (frogPresent) return;
            Jukebox.PlayOneShotGame("tambourine/frog");
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