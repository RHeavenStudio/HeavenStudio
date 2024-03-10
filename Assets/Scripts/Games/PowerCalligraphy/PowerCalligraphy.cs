using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

using Jukebox;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbPowerCalligraphy
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("powerCalligraphy", "Power Calligraphy", "ffffff", false, false, new List<GameAction>()
            {
                new GameAction("re", "Re (レ)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.re, e["prepare"]); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.re); },
                    parameters = new List<Param>() 
                    {
                        new Param("prepare", false, "Force Prepare", "Toggle if the cue should be prepared."),
                    },
                    defaultLength = 8f,
                },
                new GameAction("comma", "Comma (、)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.comma, e["prepare"]); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.comma); },
                    parameters = new List<Param>() 
                    {
                        new Param("prepare", false, "Force Prepare", "Toggle if the cue should be prepared."),
                    },
                    defaultLength = 8f,
                },
                new GameAction("chikara", "Chikara (力)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.chikara, e["prepare"]); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.chikara); },
                    parameters = new List<Param>() 
                    {
                        new Param("prepare", false, "Force Prepare", "Toggle if the cue should be prepared."),
                    },
                    defaultLength = 8f,
                },
                new GameAction("onore", "Onore (己)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.onore, e["prepare"]); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.onore); },
                    parameters = new List<Param>() 
                    {
                        new Param("prepare", false, "Force Prepare", "Toggle if the cue should be prepared."),
                    },
                    defaultLength = 8f,
                },
                new GameAction("sun", "Sun (寸)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.sun, e["prepare"]); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.sun); },
                    parameters = new List<Param>() 
                    {
                        new Param("prepare", false, "Force Prepare", "Toggle if the cue should be prepared."),
                    },
                    defaultLength = 8f,
                },
                new GameAction("kokoro", "Kokoro (心)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.kokoro, e["prepare"]); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.kokoro); },
                    parameters = new List<Param>() 
                    {
                        new Param("prepare", false, "Force Prepare", "Toggle if the cue should be prepared."),
                    },
                    defaultLength = 8f,
                },
                new GameAction("face", "Face (つるニハ○○ムし)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat,
                        e["korean"] ? (int)PowerCalligraphy.CharacterType.face_kr : (int)PowerCalligraphy.CharacterType.face, e["prepare"]); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, 
                        e["korean"] ? (int)PowerCalligraphy.CharacterType.face_kr : (int)PowerCalligraphy.CharacterType.face); },
                    parameters = new List<Param>() 
                    {
                        new Param("korean", false, "Korean Version", "Change the character to Korean version. (つ３ニハ○○ムし)"),
                        new Param("prepare", false, "Force Prepare", "Toggle if the cue should be prepared."),
                    },
                    defaultLength = 12f,
                },
                new GameAction("changeScrollSpeed", "Change Scroll Speed")
                {
                    function = delegate {var e = eventCaller.currentEntity;
                        PowerCalligraphy.instance.ChangeScrollSpeed(e["x"], e["y"]);},
                    parameters = new List<Param>() 
                    {
                        new Param("x", new EntityTypes.Float(-20, 20, 0), "X"),
                        new Param("y", new EntityTypes.Float(-20, 20, 0), "Y"),
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("end", "The End")
                {
                    function = delegate {PowerCalligraphy.instance.TheEnd();},
                    defaultLength = 0.5f,
                },
            },
            new List<string>() { "agb", "normal" }, "agbCalligraphy", "en", new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_PowerCalligraphy;
    public class PowerCalligraphy : Minigame
    {
        [SerializeField] List<GameObject> basePapers = new List<GameObject>();
        public Transform paperHolder;
        public Writing endPaper;

        public Vector3 scrollSpeed = new Vector3();

        public static Nullable<QueuedPaper> queuedPaper = null;

        public enum CharacterType
        {
            re,
            comma,
            chikara,
            onore,
            sun,
            kokoro,
            face,
            face_kr,
            NONE,
        }
        public struct QueuedPaper
        {
            public double beat;
            public int type;
        }

        public static PowerCalligraphy instance = null;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        Writing nowPaper;
        bool isPrepare = false;
        void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused) return;

            if (queuedPaper is not null)
            {
                Prepare(queuedPaper.Value.beat, queuedPaper.Value.type);
                queuedPaper = null;
            }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                if (nowPaper.onGoing && nowPaper.Stroke == 0)
                {
                    nowPaper.ProcessInput("fast");
                    ScoreMiss();
                }
            }
            if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress))
            {
                if (nowPaper.onGoing && nowPaper.Stroke != 0)
                {
                    nowPaper.ProcessInput("fast");
                    ScoreMiss();
                }
            }
        }

        private void SpawnPaper(double beat, int type)
        {
            nowPaper = Instantiate(basePapers[type], paperHolder).GetComponent<Writing>();
            nowPaper.targetBeat = beat;
            nowPaper.scrollSpeed = scrollSpeed;
            nowPaper.gameObject.SetActive(true);
            nowPaper.Init();
        }

        public void Write(double beat, int type)
        {
            if (!isPrepare) Prepare(beat, type);
            nowPaper.Play();
            isPrepare=false;
        }

        public void QueuePaper(double beat, int type, bool prepare)
        {
            if (GameManager.instance.currentGame != "powerCalligraphy" || prepare)
            {
                queuedPaper = new QueuedPaper()
                {
                    beat = beat,
                    type = type,
                };
            }
            else if(Conductor.instance.songPositionInBeats < beat)
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat-1, delegate{ Prepare(beat, type);})
                });
            }
        }
        public void Prepare(double beat, int type)
        {
            SpawnPaper(beat, type);
            isPrepare = true;
        }

        public void ChangeScrollSpeed(float x, float y)
        {
            scrollSpeed = new Vector3(x, y, 0);
            nowPaper.scrollSpeed = scrollSpeed;
        }

        public void TheEnd()
        {
            endPaper.TheEnd();
        }
    }
}