using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbClapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("clappyTrio", "The Clappy Trio", "deffff", false, false, new List<GameAction>()
            {
                new GameAction("clap", "Clap")
                {
                    function = delegate { ClappyTrio.instance.Clap(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 
                    resizable = true
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClappyTrio.instance.BopToggle(e.beat, e.length, e["bop"], e["autoBop"], e["emo"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Should the lions bop?"),
                        new Param("autoBop", false, "Bop (Auto)", "Should the lions auto bop?"),
                        new Param("emo", false, "Disable Emotion", "Should the lions just show the neutral face while bopping?")
                    }
                },
                new GameAction("prepare", "Prepare Stance")
                {
                    function = delegate { ClappyTrio.instance.Prepare(eventCaller.currentEntity["toggle"] ? 3 : 0); }, 
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Alt", "Whether or not the alternate version should be played")
                    }
                },
                new GameAction("sign", "Sign Enter")
                {
                    function = delegate { var e = eventCaller.currentEntity;  ClappyTrio.instance.Sign(e.beat, e.length, e["ease"], e["down"]); },
                    parameters = new List<Param>()
                    {
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "Which ease should the sign move with?"),
                        new Param("down", true, "Down", "Should the sign go down?")
                    },
                    resizable = true
                },
                new GameAction("change lion count", "Change Lion Count")
                {
                    function = delegate { ClappyTrio.instance.ChangeLionCount((int)eventCaller.currentEntity["valA"]); }, 
                    defaultLength = 0.5f,  
                    parameters = new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Integer(3, 8, 3), "Lion Count", "The amount of lions")
                    }
                },
                // This is still here for backwards-compatibility but is hidden in the editor
                new GameAction("prepare_alt", "")
                {
                    function = delegate { ClappyTrio.instance.Prepare(3); }, 
                    hidden = true
                },
            },
            new List<string>() {"agb", "normal"},
            "agbclap", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_ClappyTrio;

    public class ClappyTrio : Minigame
    {
        public int lionCount = 3;

        public List<GameObject> Lion;

        [SerializeField] private Sprite[] faces;

        private bool isClapping;
        private float currentClappingLength;
        private float lastClapStart;
        private int clapIndex;

        private ClappyTrioPlayer ClappyTrioPlayer;

        public bool playerHitLast = false;
        public bool missed;
        bool shouldBop;
        bool doEmotion = true;
        public int emoCounter;

        public GameEvent bop = new GameEvent();

        [SerializeField] Animator signAnim;
        float signStartBeat;
        float signLength;
        EasingFunction.Ease lastEase;
        bool signGoDown;

        public static ClappyTrio instance { get; set; }

        MultiSound clapSounds = null;
        BeatAction clapAction = null;

        private void Awake()
        {
            instance = this;
            clapSounds = null;
            InitLions();
        }
        public override void OnGameSwitch(float beat)
        {
            DynamicBeatmap.DynamicEntity changeLion = GameManager.instance.Beatmap.entities.FindLast(c => c.datamodel == "clappyTrio/change lion count" && c.beat <= beat);
            if(changeLion != null)
            {
                EventCaller.instance.CallEvent(changeLion, true);
            }
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (shouldBop) Bop(cond.songPositionInBeats);
            }
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(signStartBeat, signLength);

                if (normalizedBeat > 0 && normalizedBeat <= 1)
                {
                    EasingFunction.Function func = EasingFunction.GetEasingFunction(lastEase);
                    float newPos = func(0, 1, normalizedBeat);
                    signAnim.DoNormalizedAnimation(signGoDown ? "Enter" : "Exit", newPos);
                }
            }
        }

        public void Sign(float beat, float length, int ease, bool down)
        {
            Jukebox.PlayOneShotGame("clappyTrio/sign");
            signStartBeat = beat;
            signLength = length;
            lastEase = (EasingFunction.Ease)ease;
            signGoDown = down;
        }

        private void InitLions()
        {
            float startPos = -3.066667f;
            float maxWidth = 12.266668f;

            for (int i = 0; i < lionCount; i++)
            {
                GameObject lion;
                if (i == 0)
                    lion = Lion[0];
                else
                    lion = Instantiate(Lion[0], Lion[0].transform.parent);

                lion.transform.localPosition = new Vector3(startPos + ((maxWidth / (lionCount + 1)) * (i + 1)), 0);

                if (i > 0)
                    Lion.Add(lion);

                if (i == lionCount - 1)
                    ClappyTrioPlayer = lion.AddComponent<ClappyTrioPlayer>();
            }

            if (clapSounds != null)
                clapSounds.Delete();

            if (clapAction != null)
                clapAction.Delete();
        }

        public void Clap(float beat, float length)
        {
            ClappyTrioPlayer.clapStarted = true;
            ClappyTrioPlayer.canHit = true; // this is technically a lie, this just restores the ability to hit

            playerHitLast = false;
            isClapping = true;
            
            // makes the other lions clap
            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
            for (int i = 0; i < Lion.Count - 1; i++)
            {
                int idx = i;
                sounds.Add(new MultiSound.Sound((i > 0) ? "clappyTrio/middleClap" : "clappyTrio/leftClap", beat + (length * i)));
                actions.Add(new BeatAction.Action(beat + (length * i), delegate { SetFace(idx, 4); Lion[idx].GetComponent<Animator>().Play("Clap", 0, 0);}));
            }
            clapSounds = MultiSound.Play(sounds.ToArray());
            clapAction = BeatAction.New(this.gameObject, actions);

            // prepare player input
            ClappyTrioPlayer.QueueClap(beat, length * (Lion.Count - 1));
        }

        public void Prepare(int type)
        {
            for (int i = 0; i < Lion.Count; i++)
            {
                SetFace(i, type);
            }
            PlayAnimationAll("Prepare");
            Jukebox.PlayOneShotGame("clappyTrio/ready");
        }

        public void BopToggle(float beat, float length, bool startBop, bool autoBop, bool emo)
        {
            doEmotion = !emo;
            shouldBop = autoBop;
            if (startBop)
            {
                for (int i = 0; i < length; i++)
                {
                    if (i == 0 && startBop && autoBop) continue;
                    float spawnBeat = beat + i;
                    BeatAction.New(instance.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(spawnBeat, delegate { Bop(spawnBeat); })
                    });
                }
            }
        }

        public void Bop(float beat)
        {
            if (doEmotion && emoCounter > 0)
            {
                if (playerHitLast)
                {
                    for (int i = 0; i < Lion.Count; i++)
                    {
                        SetFace(i, 1);
                    }
                }
                else if (missed)
                {
                    var a = EventCaller.GetAllInGameManagerList("clappyTrio", new string[] { "clap" });
                    var b = a.FindAll(c => c.beat < beat);

                    if (b.Count > 0)
                    {
                        for (int i = 0; i < Lion.Count; i++)
                        {
                            if (i == Lion.Count - 1)
                            {
                                SetFace(i, 0);
                            }
                            else
                            {
                                SetFace(i, 2);
                            }
                        }
                    }
                }
                emoCounter--;
            }
            else
            {
                for (int i = 0; i < Lion.Count; i++)
                {
                    SetFace(i, 0);
                }
            }

            PlayAnimationAll("Bop");
        }

        public void ChangeLionCount(int lions)
        {
            for(int i=1; i<lionCount; i++)
            {
                Destroy(Lion[i]);
            }
            Lion.RemoveRange(1, lionCount - 1);
            lionCount = lions;
            SetFace(0, 0);
            InitLions();
            PlayAnimationAll("Idle");
        }

        private void PlayAnimationAll(string anim)
        {
            for (int i = 0; i < Lion.Count; i++)
            {
                Lion[i].GetComponent<Animator>().Play(anim, -1, 0);
            }
        }

        public void SetFace(int lion, int type)
        {
            Lion[lion].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = faces[type];
        }
    }
}