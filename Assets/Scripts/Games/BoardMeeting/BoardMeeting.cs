using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using HeavenStudio.Util;
using DG.Tweening;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;

    public static class RvlBoardMeetingLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("boardMeeting", "Board Meeting", "d37912", false, false, new List<GameAction>()
            {
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { BoardMeeting.instance.Prepare(); }
                },
                new GameAction("spinEqui", "Spin")
                {
                    function = delegate {var e = eventCaller.currentEntity; BoardMeeting.instance.SpinEqui(e.beat, e.length); },
                    resizable = true,
                    priority = 2
                },
                new GameAction("spin", "Spin (Range)")
                {
                    function = delegate {var e = eventCaller.currentEntity; BoardMeeting.instance.Spin(e["start"], e["end"]); },
                    parameters = new List<Param>()
                    {
                        new Param("start", new EntityTypes.Integer(1, 6, 1), "Starting Pig", "Which pig from the far left (1) or far right (4) should be the first to spin?"),
                        new Param("end", new EntityTypes.Integer(1, 6, 4), "Ending Pig", "Which pig from the far left (1) or far right (4) should be the last to spin?")
                    },
                    priority = 2
                },
                new GameAction("stop", "Stop")
                {
                    function = delegate { var e = eventCaller.currentEntity; BoardMeeting.instance.Stop(e.beat, e.length); },
                    resizable = true,
                    priority = 1
                },
                new GameAction("assStop", "Assistant Stop")
                {
                    function = delegate { var e = eventCaller.currentEntity; BoardMeeting.instance.AssistantStop(e.beat); },
                    defaultLength = 3f
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; BoardMeeting.instance.Bop(e.beat, e.length, e["bop"], e["auto"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Should the executives and assistant bop?"),
                        new Param("auto", false, "Bop (Auto)", "Should the executives and assistant auto bop?")
                    }
                },
                new GameAction("changeCount", "Change Executives")
                {
                    function = delegate { BoardMeeting.instance.ChangeExecutiveCount(eventCaller.currentEntity["amount"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("amount", new EntityTypes.Integer(3, 5, 4), "Amount", "How many executives will there be?")
                    }
                }
            },
            new List<string>() {"rvl", "normal"},
            "rvlrotation", "en",
            new List<string>() {"en"}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BoardMeeting;
    using System;

    public class BoardMeeting : Minigame
    {
        [Header("Components")]
        [SerializeField] Transform farLeft;
        [SerializeField] Transform farRight;
        [SerializeField] Animator assistantAnim;

        [Header("Properties")]
        [SerializeField] int executiveCount = 4;
        [SerializeField] List<BMExecutive> executives = new List<BMExecutive>();
        public BMExecutive firstSpinner;
        [SerializeField] float shakeIntensity = 0.5f;
        private bool assistantCanBop = true;
        private bool executivesCanBop = true;
        [NonSerialized] public Sound chairLoopSound = null;
        int missCounter = 0;
        private Tween shakeTween;

        public static BoardMeeting instance;

        private void Awake()
        {
            instance = this;
            SetupBopRegion("boardMeeting", "bop", "auto");
            InitExecutives();
        }

        private void OnDestroy()
        {
            foreach(var evt in scheduledInputs)
            {
                evt.Disable();
            }
            if (chairLoopSound != null)
            {
                chairLoopSound.KillLoop(0);
                chairLoopSound = null;
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (PlayerInput.GetIsAction(InputAction_BasicPressing) && !IsExpectingInputNow(InputAction_BasicPress.inputLockCategory))
                {
                    if (executives[executiveCount - 1].spinning)
                    {
                        executives[executiveCount - 1].Stop(false);
                        SoundByte.PlayOneShotGame("boardMeeting/miss");
                        SoundByte.PlayOneShot("miss");
                        ScoreMiss();
                        foreach (var evt in scheduledInputs)
                        {
                            evt.Disable();
                        }
                    }
                }
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (!BeatIsInBopRegion(beat)) return;
            SingleBop();
        }

        void SingleBop()
        {
            if (assistantCanBop) 
            {
                if (missCounter > 0) assistantAnim.DoScaledAnimationAsync("MissBop", 0.5f);
                else assistantAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
            if (missCounter > 0) missCounter--;
            if (!executivesCanBop) return;
            foreach (var executive in executives)
            {
                executive.Bop();
            }
        }

        public void Bop(double beat, float length, bool goBop, bool autoBop)
        {
            if (goBop)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(beat + i, delegate
                        {
                            SingleBop();
                        })
                    });
                }
            }
        }

        public void AssistantStop(double beat)
        {
            assistantCanBop = false;
            string twoSound = "boardMeeting/two";
            if (beat % 1 != 0) twoSound = "boardMeeting/twoUra";
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("boardMeeting/one", beat),
                new MultiSound.Sound(twoSound, beat + 0.5f),
                new MultiSound.Sound("boardMeeting/three", beat + 1),
                new MultiSound.Sound("boardMeeting/stopAll", beat + 2)
            });
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { assistantAnim.DoScaledAnimationAsync("One", 0.5f); }),
                new BeatAction.Action(beat + 1, delegate { assistantAnim.DoScaledAnimationAsync("Three", 0.5f); }),
                new BeatAction.Action(beat + 2, delegate 
                { 
                    foreach (var executive in executives)
                    {
                        if (executive.player) continue;
                        executive.Stop();
                    }
                    if (!executives[executiveCount - 1].spinning)
                    {
                        if (chairLoopSound != null)
                        {
                            chairLoopSound.KillLoop(0);
                            chairLoopSound = null;
                        }
                    }
                }),
                new BeatAction.Action(beat + 2.5f, delegate { assistantCanBop = true; })
            });
            ScheduleInput(beat, 2f, InputAction_BasicPress, JustAssistant, MissAssistant, Empty);
        }

        public void Stop(double beat, float length)
        {
            executivesCanBop = false;
            List<BeatAction.Action> stops = new List<BeatAction.Action>();
            for (int i = 0; i < executiveCount; i++)
            {
                if (executives[i].player) break;
                int index = i;
                stops.Add(new BeatAction.Action(beat + length * i, delegate 
                {
                    int ex = executiveCount;
                    if (executiveCount < 4) ex = 4;
                    if (index < ex - 3)
                    {
                        SoundByte.PlayOneShotGame("boardMeeting/stopA");
                    }
                    else if (index == ex - 3) 
                    {
                        SoundByte.PlayOneShotGame("boardMeeting/stopB");
                    }
                    else if (index == ex - 2)
                    {
                        SoundByte.PlayOneShotGame("boardMeeting/stopC");
                    }

                    if (index == executiveCount - 2 && !executives[executiveCount - 1].spinning)
                    {
                        if (chairLoopSound != null)
                        {
                            chairLoopSound.KillLoop(0);
                            chairLoopSound = null;
                        }
                    }
                    executives[index].Stop(); 
                }));
            }
            stops.Add(new BeatAction.Action(beat + length * executiveCount + 0.5f, delegate { executivesCanBop = true; }));
            BeatAction.New(instance, stops);
            ScheduleInput(beat, length * (executiveCount - 1), InputAction_BasicPress, Just, Miss, Empty);
        }

        public void Prepare()
        {
            SoundByte.PlayOneShotGame("boardMeeting/prepare");
            foreach (var executive in executives)
            {
                executive.Prepare();
            }
        }

        public void SpinEqui(double beat, float length)
        {
            if (chairLoopSound == null) chairLoopSound = SoundByte.PlayOneShotGame("boardMeeting/chairLoop", -1, 1, 1, true);
            firstSpinner = executives[0];
            List<BeatAction.Action> rolls = new List<BeatAction.Action>();
            for (int i = 0; i < executiveCount; i++)
            {
                int index = i;
                rolls.Add(new BeatAction.Action(beat + length * i, delegate
                {
                    int ex = executiveCount;
                    string soundToPlay = "A";
                    if (executiveCount < 4) ex = 4;
                    if (index == ex - 3)
                    {
                        soundToPlay = "B";
                    }
                    else if (index == ex - 2)
                    {
                        soundToPlay = "C";
                    }
                    else if (index == ex - 1)
                    {
                        soundToPlay = "Player";
                    }
                    executives[index].Spin(soundToPlay);
                }));
            }
            BeatAction.New(instance, rolls);

        }

        public void Spin(int start, int end)
        {
            if (start > executiveCount || end > executiveCount) return;
            bool forceStart = false;
            if (chairLoopSound == null)
            {
                chairLoopSound = SoundByte.PlayOneShotGame("boardMeeting/chairLoop", -1, 1, 1, true);
                firstSpinner = executives[start - 1];
                forceStart = true;
            }
            for (int i = start - 1; i < end; i++)
            {
                int ex = executiveCount;
                string soundToPlay = "A";
                if (executiveCount < 4) ex = 4;
                if (i == ex - 3)
                {
                    soundToPlay = "B";
                }
                else if (i == ex - 2)
                {
                    soundToPlay = "C";
                }
                else if (i == ex - 1)
                {
                    soundToPlay = "Player";
                }
                executives[i].Spin(soundToPlay, forceStart);
            }
        }

        public void StopChairLoopSoundIfLastToStop()
        {
            if (executives.FindAll(x => x.spinning).Count > 1) return;
            if (chairLoopSound != null)
            {
                chairLoopSound.KillLoop(0);
                chairLoopSound = null;
            }
        }

        public void InitExecutives()
        {
            float startPos = farLeft.position.x;
            float maxWidth = Mathf.Abs(farLeft.position.x - farRight.position.x);

            float betweenDistance = maxWidth / 3;

            maxWidth = betweenDistance * executiveCount;

            startPos = -(maxWidth / 2);

            for (int i = -1; i < executiveCount; i++)
            {
                if (i == -1)
                {
                    assistantAnim.transform.localPosition = new Vector3(startPos + 5.359f, 0);
                }
                else
                {
                    BMExecutive executive;
                    if (i == 0) executive = executives[0];
                    else executive = Instantiate(executives[0], transform);

                    executive.transform.localPosition = new Vector3(startPos + betweenDistance * (i + 1), 0);
                    executive.GetComponent<SortingGroup>().sortingOrder = i;

                    if (i > 0)
                        executives.Add(executive);

                    if (i == executiveCount - 1)
                        executive.player = true;
                }
            }
        }

        public void ChangeExecutiveCount(int count)
        {
            for (int i = 1; i < executiveCount; i++)
            {
                Destroy(executives[i].gameObject);
            }
            executives.RemoveRange(1, executiveCount - 1);
            executiveCount = count;
            InitExecutives();
        }

        void Just(PlayerActionEvent caller, float state)
        {
            if (chairLoopSound != null)
            {
                chairLoopSound.KillLoop(0);
                chairLoopSound = null;
            }
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("boardMeeting/missThrough");
                SoundByte.PlayOneShot("miss");
                executives[executiveCount - 1].Stop(false);
                return;
            }
            SoundByte.PlayOneShotGame("boardMeeting/stopPlayer");
            executives[executiveCount - 1].Stop();
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 1f, delegate
                {
                    foreach (var executive in executives)
                    {
                        executive.Smile();
                    }
                })
            });
        }

        void JustAssistant(PlayerActionEvent caller, float state)
        {
            if (chairLoopSound != null)
            {
                chairLoopSound.KillLoop(0);
                chairLoopSound = null;
            }
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShotGame("boardMeeting/missThrough");
                SoundByte.PlayOneShot("miss");
                executives[executiveCount - 1].Stop(false);
                return;
            }
            if (shakeTween != null)
                shakeTween.Kill(true);

            DOTween.Punch(() => GameCamera.AdditionalPosition, x => GameCamera.AdditionalPosition = x, new Vector3(shakeIntensity, 0, 0),
                Conductor.instance.pitchedSecPerBeat * 0.5f, 18, 1f);
            executives[executiveCount - 1].Stop();
            assistantAnim.DoScaledAnimationAsync("Stop", 0.5f);
            SoundByte.PlayOneShotGame("boardMeeting/stopAllPlayer");
            BeatAction.New(instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(caller.timer + caller.startBeat + 1f, delegate
                {
                    foreach (var executive in executives)
                    {
                        executive.Smile();
                    }
                })
            });
        }

        void Miss(PlayerActionEvent caller)
        {
            if (executives[executiveCount - 1].spinning)
            {
                executives[executiveCount - 1].Stop(false);
                SoundByte.PlayOneShotGame("boardMeeting/missThrough");
                SoundByte.PlayOneShot("miss");
                if (chairLoopSound != null)
                {
                    chairLoopSound.KillLoop(0);
                    chairLoopSound = null;
                }
            }
        }

        void MissAssistant(PlayerActionEvent caller)
        {
            if (executives[executiveCount - 1].spinning)
            {
                executives[executiveCount - 1].Stop(false);
                SoundByte.PlayOneShotGame("boardMeeting/missThrough");
                SoundByte.PlayOneShot("miss");
                if (chairLoopSound != null)
                {
                    chairLoopSound.KillLoop(0);
                    chairLoopSound = null;
                }
            }
            assistantAnim.Play("MissIdle", 0, 0);
            missCounter = 2;
        }

        void Empty(PlayerActionEvent caller) { }
    }
}

