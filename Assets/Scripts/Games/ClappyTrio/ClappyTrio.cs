using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;

using HeavenStudio.Util;
using Jukebox;
using Jukebox.Legacy;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbClapLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("clappyTrio", "The Clappy Trio", "deffff", false, false, new List<GameAction>()
            {
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; ClappyTrio.instance.BopToggle(e.beat, e.length, e["bop"], e["autoBop"], e["emo"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the lions should bop for the duration of this event."),
                        new Param("autoBop", false, "Bop (Auto)", "Toggle if the lions should automatically bop until another Bop event is reached."),
                        new Param("emo", false, "Disable Emotion", "Toggle if the (non-player) lions should react to the player's performance when bopping.")
                    }
                },
                new GameAction("clap", "Clap")
                {
                    function = delegate { ClappyTrio.instance.Clap(eventCaller.currentEntity.beat, eventCaller.currentEntity.length, eventCaller.currentEntity.beat); }, 
                    resizable = true
                },
                new GameAction("prepare", "Prepare")
                {
                    function = delegate { ClappyTrio.instance.Prepare(eventCaller.currentEntity["toggle"] ? 3 : 0); }, 
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Alternate Pose", "Toggle if the lions should prepare using the alternate \"determined\" pose.")
                    }
                },
                new GameAction("sign", "Sign Control")
                {
                    function = delegate { var e = eventCaller.currentEntity;  ClappyTrio.instance.Sign(e.beat, e.length, e["ease"], e["down"]); },
                    parameters = new List<Param>()
                    {
                        new Param("down", true, "Enter", "Toggle if the sign should enter or exit the scene."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                    resizable = true
                },
                new GameAction("change lion count", "Change Lion Number")
                {
                    function = delegate { ClappyTrio.instance.ChangeLionCount((int)eventCaller.currentEntity["valA"]); }, 
                    defaultLength = 0.5f,  
                    parameters = new List<Param>()
                    {
                        new Param("valA", new EntityTypes.Integer(3, 8, 3), "Lions", "Set how many lions there will be. The player is always the rightmost lion.")
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

        private ClappyTrioPlayer ClappyTrioPlayer;
        public int misses;
        bool shouldBop;
        bool doEmotion = true;
        public int emoCounter;

        public GameEvent bop = new GameEvent();

        [SerializeField] Animator signAnim;
        double signStartBeat;
        float signLength;
        Util.EasingFunction.Ease lastEase;
        bool signGoDown;

        public static ClappyTrio instance { get; set; }

        MultiSound clapSounds = null;
        CancellationTokenSource clapAction = null;

        private void Awake()
        {
            instance = this;
            clapSounds = null;
            InitLions();
        }
        public override void OnGameSwitch(double beat)
        {
            InitClaps(beat);
        }

        private void InitClaps(double beat)
        {
            RiqEntity changeLion = GameManager.instance.Beatmap.Entities.FindLast(c => c.datamodel == "clappyTrio/change lion count" && c.beat <= beat);
            if (changeLion != null)
            {
                EventCaller.instance.CallEvent(changeLion, true);
            }

            var allClaps = EventCaller.GetAllInGameManagerList("clappyTrio", new string[] { "clap" });

            foreach (var c in allClaps)
            {
                if (c.beat < beat && c.beat + (c.length * (lionCount - 1)) >= beat)
                {
                    Clap(c.beat, c.length, beat);
                }
            }
        }

        public override void OnBeatPulse(double beat)
        {
            if (shouldBop) Bop(Conductor.instance.songPositionInBeatsAsDouble);
        }

        void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(signStartBeat, signLength);

                if (normalizedBeat > 0 && normalizedBeat <= 1)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(lastEase);
                    float newPos = func(0, 1, normalizedBeat);
                    signAnim.DoNormalizedAnimation(signGoDown ? "Enter" : "Exit", newPos);
                }
            }
        }

        public void Sign(double beat, float length, int ease, bool down)
        {
            SoundByte.PlayOneShotGame("clappyTrio/sign");
            signStartBeat = beat;
            signLength = length;
            lastEase = (Util.EasingFunction.Ease)ease;
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
            {
                clapAction.Cancel();
                clapAction.Dispose();
            }
        }

        public void Clap(double beat, float length, double gameSwitchBeat)
        {
            ClappyTrioPlayer.clapStarted = true;
            ClappyTrioPlayer.canHit = true; // this is technically a lie, this just restores the ability to hit
            
            // makes the other lions clap
            List<MultiSound.Sound> sounds = new List<MultiSound.Sound>();
            List<BeatAction.Action> actions = new List<BeatAction.Action>();
            for (int i = 0; i < Lion.Count - 1; i++)
            {
                bool isBeforeGameSwitch = beat + (length * i) < gameSwitchBeat;
                int idx = i;
                if (isBeforeGameSwitch)
                {
                    SetFace(idx, 4); 
                    Lion[idx].GetComponent<Animator>().Play("Clap", 0, 1);
                }
                else
                {
                    sounds.Add(new MultiSound.Sound((i > 0) ? "clappyTrio/middleClap" : "clappyTrio/leftClap", beat + (length * i)));
                    actions.Add(new BeatAction.Action(beat + (length * i), delegate 
                    { SetFace(idx, 4); Lion[idx].GetComponent<Animator>().Play("Clap", 0, 0); }));
                }
            }
            if (sounds.Count > 0) clapSounds = MultiSound.Play(sounds.ToArray());
            if (actions.Count > 0) clapAction = BeatAction.New(this, actions);

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
            SoundByte.PlayOneShotGame("clappyTrio/ready");
        }

        public void BopToggle(double beat, float length, bool startBop, bool autoBop, bool emo)
        {
            doEmotion = !emo;
            shouldBop = autoBop;
            if (startBop)
            {
                List<BeatAction.Action> bops = new List<BeatAction.Action>();
                for (int i = 0; i < length; i++)
                {
                    if (i == 0 && startBop && autoBop) continue;
                    double spawnBeat = beat + i;
                    bops.Add(new BeatAction.Action(spawnBeat, delegate { Bop(spawnBeat); }));
                    if (i == length - 1)
                    {
                        bops.Add(new BeatAction.Action(spawnBeat, delegate { misses = 0; }));
                    }
                }
                if (bops.Count > 0) BeatAction.New(instance, bops);
            }
        }

        public void Bop(double beat)
        {
            if (doEmotion && emoCounter > 0)
            {
                if (misses == 0)
                {
                    for (int i = 0; i < Lion.Count; i++)
                    {
                        SetFace(i, 1);
                    }
                }
                else if (misses > 0)
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