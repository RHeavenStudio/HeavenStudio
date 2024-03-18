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
                new GameAction("bop", "Bop")
                {
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.ToggleBop(e.beat, e.length, e["bop"], e["bopAuto"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Bop", "Toggle if the paddlers should bop for the duration of this event."),
                        new Param("bopAuto", false, "Bop (Auto)", "Toggle if the paddlers should automatically bop until another Bop event is reached.")
                    }
                },
                new GameAction("re", "Re (レ)")
                {
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.re); },
                    defaultLength = 8f,
                },
                new GameAction("comma", "Comma (、)")
                {
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.comma); },
                    defaultLength = 8f,
                },
                new GameAction("chikara", "Chikara (力)")
                {
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.chikara); },
                    defaultLength = 8f,
                },
                new GameAction("onore", "Onore (己)")
                {
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.onore); },
                    defaultLength = 8f,
                },
                new GameAction("sun", "Sun (寸)")
                {
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.sun); },
                    defaultLength = 8f,
                },
                new GameAction("kokoro", "Kokoro (心)")
                {
                    function = delegate {var e = eventCaller.currentEntity; PowerCalligraphy.instance.Write(e.beat, (int)PowerCalligraphy.CharacterType.kokoro); },
                    defaultLength = 8f,
                },
                new GameAction("face", "Face (つるニハ○○ムし)")
                {
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
                new GameAction("chounin events", "Chounin Animations")
                {
                    function = delegate { var e = eventCaller.currentEntity; PowerCalligraphy.instance.PlayChouninAnimation(e["type"], e["pos"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", PowerCalligraphy.ChouninType.Dance, "Animation", "Set the animation for Chounin to perform."),
                        new Param("pos", new EntityTypes.Float(0, 14, 0), "Position", "Set the position of Chounin."),
                    }
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
        [SerializeField] List<RuntimeAnimatorController> shiftCntls = new List<RuntimeAnimatorController>();
        public Transform shiftHolder;
        public Transform paperHolder;
        public Animator endPaper;
        public GameObject[] Chounin;
        
        public Animator fudePosAnim;
        public Animator fudeAnim;
        public Animator shiftAnim;
        public Fude playerFude;

        [Header("Variables")]
        public Vector3 scrollSpeed = new Vector3();
        public float chouninSpeed;
        float chouninRate => chouninSpeed / (Conductor.instance.pitchedSecPerBeat * 2f);

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
            SetupBopRegion("powerCalligraphy", "bop", "bopAuto");
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
            if (!cond.isPlaying || cond.isPaused) return;

            if (PlayerInput.GetIsAction(InputAction_BasicPress) && !IsExpectingInputNow(InputAction_BasicPress))
            {
                if (nowPaper.onGoing && nowPaper.Stroke == 1)
                {
                    nowPaper.ProcessInput("fast");
                    ChouninMiss();
                    ScoreMiss();
                }
            }
            if (PlayerInput.GetIsAction(InputAction_FlickPress) && !IsExpectingInputNow(InputAction_FlickPress))
            {
                if (nowPaper.onGoing && nowPaper.Stroke != 1)
                {
                    nowPaper.ProcessInput("fast");
                    ChouninMiss();
                    ScoreMiss();
                }
            }

            if (isChouninMove) UpdateChouninPos(chouninRate * Time.deltaTime);
        }

        private void SpawnPaper(int type)
        {
            if (nowPaper is not null) nowPaper.transform.SetParent(paperHolder.transform, true);
            nowPaper = Instantiate(basePapers[type], paperHolder).GetComponent<Writing>();
            nowPaper.scrollSpeed = scrollSpeed;
            nowPaper.gameObject.SetActive(true);
            nowPaper.Init();
            fudePosAnim.runtimeAnimatorController = fudePosCntls[type];
            fudePosAnim.Play("0", 0, 0);
            shiftAnim.runtimeAnimatorController = shiftCntls[type];
            shiftHolder.transform.position = new Vector3(0, 0, 0);
        }

        public void Write(double beat, int type)
        {
            Prepare(type);
            nowPaper.transform.SetParent(shiftHolder.transform, true);
            nowPaper.startBeat = beat;
            nowPaper.Play();
            double nextBeat = beat + nowPaper.nextBeat;
            BeatAction.New(instance, new List<BeatAction.Action>(){
                new BeatAction.Action(beat, delegate{isPrepare = false;}),
                new BeatAction.Action(nextBeat, delegate{NextPrepare(nextBeat);}),
            });
        }

        public void Prepare(int type)
        {
            if (!isPrepare)
            {
                isPrepare = true;
                SpawnPaper(type);
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

        public override void OnBeatPulse(double beat)
        {
            if (BeatIsInBopRegion(beat)) Bop();
        }

        public void ToggleBop(double beat, float length, bool bopOrNah, bool autoBop)
        {
            if (bopOrNah)
            {
                for (int i = 0; i < length; i++)
                {
                    BeatAction.New(instance, new() {new BeatAction.Action(beat + i, delegate {Bop();}) });
                }
            }
        }

        public void Bop()
        {
            if (chouninType != (int)ChouninType.Dance) return;
            isChouninMove = true;
            double beat = Conductor.instance.songPositionInBeats;
            
            for (int i=0; i<2; i++) {
                int j = 0;
                foreach (Transform child in Chounin[i].transform) {
                    var animator = child.GetComponent<Animator>();
                    if (animator != null) {
                        if ((int)(beat%2) == j%2) {
                            animator.DoScaledAnimationAsync("dance1", 0.5f);
                        } else {
                            animator.DoScaledAnimationAsync("dance0", 0.5f);
                        }
                    }
                    j++;
                }
            }
        }

        public enum ChouninType {
            Dance,
            Bow,
            Idle,
        }
        bool isChouninMove = false;
        int chouninType = -1;
        public void PlayChouninAnimation(int type, float pos)
        {
            isChouninMove = false;
            chouninType = type;
            switch (type) 
            {
                case (int)ChouninType.Dance:
                    isChouninMove = true;
                    Bop();
                    break;
                case (int)ChouninType.Bow:
                    ChouninAnim("bow");
                    break;
                default:
                    ChouninAnim("idle");
                    break;
            }
            if (pos>0) UpdateChouninPos(pos);
        }

        public void ChouninAnim(string type)
        {
            for (int i=0; i<2; i++) {
                foreach (Transform child in Chounin[i].transform) {
                    var animator = child.GetComponent<Animator>();
                    if (animator != null) {
                        if (i%2 == 1) {
                            animator.DoScaledAnimationAsync($"{type}1", 0.5f);
                        } else {
                            animator.DoScaledAnimationAsync($"{type}0", 0.5f);
                        }
                    }
                }
            }
        }

        public void ChouninMiss()
        {
            isChouninMove = false;
            double beat = Conductor.instance.songPositionInBeats;
            var currentChouninType = chouninType;
            BeatAction.New(instance, new() {new BeatAction.Action(beat + 1.5f, delegate {
                if (chouninType == -1) chouninType = currentChouninType;
            })});
            chouninType = -1;
            ChouninAnim("fall");
        }

        private void UpdateChouninPos(float pos)
        {
            foreach (Transform child in Chounin[0].transform) {
                var childPos = child.localPosition;
                var newChildY = childPos.y - pos;
                newChildY = newChildY < -6 ? newChildY + 12 : newChildY;
                child.localPosition = new Vector3(childPos.x, newChildY, childPos.z);
            }

            foreach (Transform child in Chounin[1].transform) {
                var childPos = child.localPosition;
                var newChildY = childPos.y + pos;
                newChildY = newChildY > 6 ? newChildY - 12 : newChildY;
                child.localPosition = new Vector3(childPos.x, newChildY, childPos.z);
            }
        }
    }
}