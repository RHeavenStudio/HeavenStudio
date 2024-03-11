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
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.re); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.re); },
                    defaultLength = 8f,
                },
                new GameAction("comma", "Comma (、)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.comma); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.comma); },
                    defaultLength = 8f,
                },
                new GameAction("chikara", "Chikara (力)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.chikara); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.chikara); },
                    defaultLength = 8f,
                },
                new GameAction("onore", "Onore (己)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.onore); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.onore); },
                    defaultLength = 8f,
                },
                new GameAction("sun", "Sun (寸)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.sun); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.sun); },
                    defaultLength = 8f,
                },
                new GameAction("kokoro", "Kokoro (心)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat, (int)PowerCalligraphy.CharacterType.kokoro); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.kokoro); },
                    defaultLength = 8f,
                },
                new GameAction("face", "Face (つるニハ○○ムし)")
                {
                    preFunction = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.QueuePaper(e.beat,
                        e["korean"] ? (int)PowerCalligraphy.CharacterType.face_kr : (int)PowerCalligraphy.CharacterType.face); },
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, 
                        e["korean"] ? (int)PowerCalligraphy.CharacterType.face_kr : (int)PowerCalligraphy.CharacterType.face); },
                    parameters = new List<Param>() 
                    {
                        new Param("korean", false, "Korean Version", "Change the character to Korean version. (つ３ニハ○○ムし)"),
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
        [Header("References")]
        [SerializeField] List<GameObject> basePapers = new List<GameObject>();
        [SerializeField] List<RuntimeAnimatorController> fudePosCntls = new List<RuntimeAnimatorController>();
        public Transform paperHolder;
        public Animator endPaper;
        
        public Animator fudePosAnim;
        public Animator fudeAnim;

        public static int queuedType;

        [Header("Variables")]
        public Vector3 scrollSpeed = new Vector3();

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

        double gameStartBeat;
        public static PowerCalligraphy instance = null;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }
        public override void OnGameSwitch(double beat)
        {
            gameStartBeat = beat;
            NextPrepare(beat);
        }
        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        Writing nowPaper;
        bool isPrepare = false;
        void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying || cond.isPaused)
            {
                if (!cond.isPaused) queuedType = (int)CharacterType.NONE;
                return;
            }

            if (queuedType != (int)CharacterType.NONE)
            {
                Prepare(queuedType);
                queuedType = (int)CharacterType.NONE;
            }

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                if (nowPaper.onGoing && nowPaper.Stroke == 1)
                {
                    nowPaper.ProcessInput("fast");
                    ScoreMiss();
                }
            }
            if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress))
            {
                if (nowPaper.onGoing && nowPaper.Stroke != 1)
                {
                    nowPaper.ProcessInput("fast");
                    ScoreMiss();
                }
            }
        }

        private void SpawnPaper(int type)
        {
            nowPaper = Instantiate(basePapers[type], paperHolder).GetComponent<Writing>();
            nowPaper.scrollSpeed = scrollSpeed;
            nowPaper.gameObject.SetActive(true);
            nowPaper.Init();
            fudePosAnim.runtimeAnimatorController = fudePosCntls[type];
        }

        public void Write(double beat, int type)
        {
            Prepare(type);
            nowPaper.startBeat = beat;
            nowPaper.Play();
            isPrepare=false;
            double nextBeat = beat + nowPaper.nextBeat;
            BeatAction.New(instance, new List<BeatAction.Action>(){
                new BeatAction.Action(nextBeat, delegate{ NextPrepare(nextBeat);})
            });
        }

        public void QueuePaper(double beat, int type)
        {
            if (GameManager.instance.currentGame != "powerCalligraphy")
            {
                queuedType = type;
            }
            else if(Conductor.instance.songPositionInBeats < beat)
            {
                BeatAction.New(instance, new List<BeatAction.Action>(){
                    new BeatAction.Action(beat-1, delegate{ Prepare(type);})
                });
            }
        }
        public void Prepare(int type)
        {
            if (!isPrepare)
            {
                SpawnPaper(type);
                isPrepare = true;
            }
        }
        public void NextPrepare(double beat) // Prepare next paper
        {
            double endBeat = double.MaxValue;
            var entities = gameManager.Beatmap.Entities;

            RiqEntity firstEnd = entities.Find(c => (c.datamodel.StartsWith("gameManager/switchGame") || c.datamodel.Equals("gameManager/end")) && c.beat > gameStartBeat);
            endBeat = firstEnd?.beat ?? endBeat;

            RiqEntity nextPaper = entities.Find(v => 
                (v.datamodel is "powerCalligraphy/re" or "powerCalligraphy/comma" or "powerCalligraphy/chikara" or "powerCalligraphy/onore" or "powerCalligraphy/sun" or "powerCalligraphy/kokoro" or "powerCalligraphy/face")
                && v.beat >= beat && v.beat < endBeat);

            if (nextPaper is not null)
            {
                int type = nextPaper.datamodel switch
                {
                    "powerCalligraphy/re" => (int)CharacterType.re,
                    "powerCalligraphy/comma" => (int)CharacterType.comma,
                    "powerCalligraphy/chikara" => (int)CharacterType.chikara,
                    "powerCalligraphy/onore" => (int)CharacterType.onore,
                    "powerCalligraphy/sun" => (int)CharacterType.sun,
                    "powerCalligraphy/kokoro" => (int)CharacterType.kokoro,
                    "powerCalligraphy/face" => nextPaper["korean"] ? (int)PowerCalligraphy.CharacterType.face_kr : (int)PowerCalligraphy.CharacterType.face,
                    _ => throw new NotImplementedException()
                };

                Prepare(type);
            }
        }

        public void ChangeScrollSpeed(float x, float y)
        {
            scrollSpeed = new Vector3(x, y, 0);
            nowPaper.scrollSpeed = scrollSpeed;
        }

        public void TheEnd()
        {
            fudePosAnim.runtimeAnimatorController = fudePosCntls[(int)CharacterType.NONE];
            fudePosAnim.Play("fudePos-end");
            endPaper.Play("paper-end");
        }
    }
}