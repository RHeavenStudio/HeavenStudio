using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Starpelly;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbUpbeatLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("mrUpbeat", "Mr. Upbeat", "FFFFFF", false, false, new List<GameAction>()
            {
                new GameAction("stepping", "Start Stepping")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; MrUpbeat.Stepping(e.beat, e.length); },
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("blipping", "Beeping")
                {
                    function = delegate {var e = eventCaller.currentEntity; MrUpbeat.instance.Blipping(e.beat, e.length); },
                    defaultLength = 4f,
                    resizable = true
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
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_MrUpbeat;

    public class MrUpbeat : Minigame
    {
        [Header("References")]
        public Animator metronomeAnim;
        public UpbeatMan man;

        [Header("Properties")]
        static List<queuedUpbeatInputs> queuedInputs = new List<queuedUpbeatInputs>();
        public struct queuedUpbeatInputs
        {
            public float beat;
            public bool goRight;
        }

        public static MrUpbeat instance;

        private void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedInputs.Count > 0) queuedInputs.Clear();
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
                    queuedInputs.Clear();
                }
            }
        }

        public void Ding(bool applause)
        {
            Jukebox.PlayOneShotGame("mrUpbeat/ding");
            if (applause) Jukebox.PlayOneShot("applause");
        }

        public void Blipping(float beat, float length)
        {
            for (int i = 0; i < length + 1; i++) 
            {
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat + i, delegate { man.Blip(); }),
                });
            }
        }

        public static void Stepping(float beat, float length)
        {
            if (GameManager.instance.currentGame == "mrUpbeat")
            {
                float offSet = 0;
                if (!MrUpbeat.instance.isPlaying(MrUpbeat.instance.metronomeAnim, "MetronomeIdle") && !MrUpbeat.instance.isPlaying(MrUpbeat.instance.metronomeAnim, "MetronomeGoRight"))
                {
                    offSet = 1;
                }
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

        public void Nothing(PlayerActionEvent caller) {}
    }
}