using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlRingsideLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("ringside", "Ringside \n<color=#eb5454>[WIP]</color>", "WUTRU3", false, false, new List<GameAction>()
            {
                new GameAction("question", "Question")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.Question(e.beat, e["alt"]); },
                    parameters = new List<Param>()
                    {
                        new Param("alt", false, "Alt", "Whether the alt voice line should be used or not.")
                    },
                    defaultLength = 4f
                },
                new GameAction("woahYouGoBigGuy", "Woah You Go Big Guy!")
                {
                    function = delegate {var e = eventCaller.currentEntity; Ringside.instance.BigGuy(e.beat); },
                    defaultLength = 4f
                },
                new GameAction("poseForTheFans", "Pose For The Fans!")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; Ringside.PoseForTheFans(e.beat, e["and"]); },
                    parameters = new List<Param>()
                    {
                        new Param("and", false, "And", "Whether the And voice line should be said or not.")
                    },
                    defaultLength = 4f
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    public class Ringside : Minigame
    {
        [Header("Components")]
        [SerializeField] Animator wrestlerAnim;
        [SerializeField] Animator reporterAnim;

        [Header("Variables")]
        public int currentQuestion = 1;
        public static List<float> queuedPoses = new List<float>();


        public static Ringside instance;

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused)
            {
                if (queuedPoses.Count > 0) queuedPoses.Clear();
            }
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                if (queuedPoses.Count > 0)
                {
                    foreach(var p in queuedPoses)
                    {
                        PoseCheck(p);
                    }
                    wrestlerAnim.Play("PreparePoseIdle", 0, 0);
                    queuedPoses.Clear();
                }
                if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
                {
                    wrestlerAnim.Play("Ye", 0, 0);
                    Jukebox.PlayOneShotGame($"ringside/ye{UnityEngine.Random.Range(1, 4)}");
                }
            }
        }

        public void Question(float beat, bool alt)
        {
            if (alt)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"ringside/wub{currentQuestion}", beat),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-1", beat + 0.5f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-2", beat + 0.75f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-3", beat + 1f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-4", beat + 1.25f),
                }, forcePlay: true);
            }
            else
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound($"ringside/wubba{currentQuestion}-1", beat),
                    new MultiSound.Sound($"ringside/wubba{currentQuestion}-2", beat + 0.25f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-1", beat + 0.5f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-2", beat + 0.75f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-3", beat + 1f),
                    new MultiSound.Sound($"ringside/dubba{currentQuestion}-4", beat + 1.25f),
                }, forcePlay: true);
            }
            ThatTrue(beat + 1.25f);
        }

        public void ThatTrue(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/that{currentQuestion}", beat + 0.25f),
                new MultiSound.Sound($"ringside/true{currentQuestion}", beat + 0.75f),
            }, forcePlay: true);
            ScheduleInput(beat, 1.75f, InputType.STANDARD_DOWN, JustQuestion, Miss, Nothing);
            if (currentQuestion < 3)
            {
                currentQuestion++;
            } 
            else
            {
                currentQuestion = 1;
            }
        }

        public void BigGuy(float beat)
        {
            float youBeat = 0.65f;
            if (currentQuestion == 3) youBeat = 0.7f;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/woah{currentQuestion}", beat),
                new MultiSound.Sound($"ringside/you{currentQuestion}", beat + youBeat),
                new MultiSound.Sound($"ringside/go{currentQuestion}", beat + 1f),
                new MultiSound.Sound($"ringside/big{currentQuestion}", beat + 1.5f),
                new MultiSound.Sound($"ringside/guy{currentQuestion}", beat + 2f),
            }, forcePlay: true);

            ScheduleInput(beat, 2.5f, InputType.STANDARD_DOWN, JustBigGuyFirst, Miss, Nothing);
            ScheduleInput(beat, 3f, InputType.STANDARD_DOWN, JustBigGuySecond, Miss, Nothing);
            if (currentQuestion < 3)
            {
                currentQuestion++;
            }
            else
            {
                currentQuestion = 1;
            }
        }

        public static void PoseForTheFans(float beat, bool and)
        {
            if (and)
            {
                MultiSound.Play(new MultiSound.Sound[]
                {
                    new MultiSound.Sound("ringside/poseAnd", beat - 0.5f),
                }, forcePlay: true);
            }
            int poseLineRandom = UnityEngine.Random.Range(1, 3);
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound($"ringside/pose{poseLineRandom}", beat),
                new MultiSound.Sound($"ringside/for{poseLineRandom}", beat + 0.5f),
                new MultiSound.Sound($"ringside/the{poseLineRandom}", beat + 0.75f),
                new MultiSound.Sound($"ringside/fans{poseLineRandom}", beat + 1f),
            }, forcePlay: true);
            if (GameManager.instance.currentGame == "ringside")
            {
                Ringside.instance.PoseCheck(beat);
                BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(beat, delegate { Ringside.instance.wrestlerAnim.DoScaledAnimationAsync("PreparePose", 0.25f); }),
                });
            }
            else
            {
                queuedPoses.Add(beat);
            }
        }

        public void PoseCheck(float beat)
        {
            ScheduleInput(beat, 2f, InputType.STANDARD_ALT_DOWN, JustPoseForTheFans, MissPose, Nothing);
        }

        public void JustQuestion(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessQuestion();
        }

        public void SuccessQuestion()
        {
            wrestlerAnim.Play("Ye", 0, 0);
            Jukebox.PlayOneShotGame($"ringside/ye{UnityEngine.Random.Range(1, 4)}");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 0.5f, delegate { Jukebox.PlayOneShotGame("ringside/yeCamera"); }),
            });
        }

        public void JustBigGuyFirst(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessBigGuyFirst();
        }

        public void SuccessBigGuyFirst()
        {
            Jukebox.PlayOneShotGame($"ringside/muscles1");
            wrestlerAnim.Play("BigGuyOne", 0, 0);
        }

        public void JustBigGuySecond(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessBigGuySecond();
        }

        public void SuccessBigGuySecond()
        {
            Jukebox.PlayOneShotGame($"ringside/muscles2");
            wrestlerAnim.Play("BigGuyTwo", 0, 0);
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 0.5f, delegate { Jukebox.PlayOneShotGame("ringside/musclesCamera"); }),
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 1f, delegate { wrestlerAnim.Play("Idle", 0, 0); }),
            });
        }

        public void JustPoseForTheFans(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SuccessPoseForTheFans();
        }

        public void SuccessPoseForTheFans()
        {
            wrestlerAnim.Play("Pose1", 0, 0);
            Jukebox.PlayOneShotGame($"ringside/yell{UnityEngine.Random.Range(1, 7)}");
            BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 1f, delegate { Jukebox.PlayOneShotGame("ringside/poseCamera"); }),
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 2f, delegate { wrestlerAnim.Play("Idle", 0, 0); }),
            });
        }

        public void Miss(PlayerActionEvent caller)
        {

        }
        
        public void MissPose(PlayerActionEvent caller)
        {

        }

        public void Nothing(PlayerActionEvent caller){}
    }
}
