using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlTambourineLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("tambourine", "Tambourine \n<color=#eb5454>[WIP]</color>", "812021", false, false, new List<GameAction>()
            {
                new GameAction("beat intervals", "Start Interval")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.StartInterval(e.beat, e.length); },
                    defaultLength = 4f,
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
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.Bop(e.beat, e["whoBops"]); },
                    parameters = new List<Param>()
                    {
                        new Param("whoBops", Tambourine.WhoBops.Both, "Who Bops", "Who will bop."),
                    },
                    defaultLength = 1f,
                    priority = 4
                },
                new GameAction("success", "Success")
                {
                    function = delegate {var e = eventCaller.currentEntity; Tambourine.instance.SuccessFace(e.beat); },
                    defaultLength = 1f,
                    priority = 4,
                }
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class Tambourine : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator handsAnimator;
        [SerializeField] Animator monkeyAnimator;
        [SerializeField] ParticleSystem flowerParticles;
        [SerializeField] GameObject monkeyFace;
        [SerializeField] Animator sweatAnimator;

        [Header("Variables")]
        bool intervalStarted;
        float intervalStartBeat;
        float beatInterval = 4f;
        float misses;

        [Header("Sprites")]
        [SerializeField] Sprite monkeyGrimace;
        [SerializeField] Sprite monkeySmile;

        public enum WhoBops
        {
            Monkey,
            Player,
            Both
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
        }

        void Update()
        {
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
                if (!intervalStarted)
                {
                    monkeyFace.GetComponent<SpriteRenderer>().sprite = monkeyGrimace;
                    monkeyFace.SetActive(true);
                }
            }
            else if (PlayerInput.AltPressed() && !IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
            {
                handsAnimator.Play("Smack", 0, 0);
                Jukebox.PlayOneShotGame($"tambourine/player/hit/{UnityEngine.Random.Range(1, 6)}");
                sweatAnimator.Play("Sweating", 0, 0);
                SummonFrog();
                if (!intervalStarted)
                {
                    monkeyFace.GetComponent<SpriteRenderer>().sprite = monkeyGrimace;
                    monkeyFace.SetActive(true);
                }
            }
        }

        public void StartInterval(float beat, float interval)
        {
            intervalStartBeat = beat;
            beatInterval = interval;
            if (!intervalStarted)
            {
                monkeyFace.SetActive(false);
                queuedInputs.Clear();
                misses = 0;
                intervalStarted = true;
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + beatInterval, delegate { intervalStarted = false; }),
                });
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
            monkeyFace.GetComponent<SpriteRenderer>().sprite = monkeySmile;
            monkeyFace.SetActive(true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.3f, delegate { monkeyFace.SetActive(false); })
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
                    new BeatAction.Action(beat + length + input.beatAwayFromStart, delegate { Bop(beat + length + input.beatAwayFromStart, (int)WhoBops.Monkey); })
                });
            }
        }

        public void Bop(float beat, int whoBops)
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
            }
        }

        public void SuccessFace(float beat)
        {
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
            monkeyFace.GetComponent<SpriteRenderer>().sprite = monkeySmile;
            monkeyFace.SetActive(true);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate { monkeyFace.SetActive(false); }),
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
                    monkeyFace.GetComponent<SpriteRenderer>().sprite = monkeyGrimace;
                    monkeyFace.SetActive(true);
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
                    monkeyFace.GetComponent<SpriteRenderer>().sprite = monkeyGrimace;
                    monkeyFace.SetActive(true);
                }
                return;
            }
            Success(false);
        }

        public void Success(bool hit)
        {
            monkeyFace.SetActive(false);
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
                monkeyFace.GetComponent<SpriteRenderer>().sprite = monkeyGrimace;
                monkeyFace.SetActive(true);
            }
        }

        public void SummonFrog()
        {
            Jukebox.PlayOneShotGame("tambourine/frog");
        }

        public void Nothing(PlayerActionEvent caller) {}
    }
}