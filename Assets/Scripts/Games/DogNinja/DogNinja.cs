using NaughtyBezierCurves;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDogNinjaLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("dogNinja", "Dog Ninja", "554899", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out DogNinja instance)) {
                            instance.Bop(e.beat, e.length, e["auto"], e["toggle"]);
                        }
                    },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Bop", "Toggle if Dog Ninja should bop for the duration of this event."),
                        new Param("auto", false, "Bop (Auto)", "Toggle if Dog Ninja should automatically bop until another Bop event is reached."),
                    }
                },
                new GameAction("Prepare", "Prepare")
                {
                    function = delegate {
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out DogNinja instance)) {
                            instance.Prepare(eventCaller.currentEntity.beat);
                        }
                    },
                    defaultLength = 0.5f,
                },
                new GameAction("ThrowObject", "Throw Object")
                {
                    // function = delegate {
                    //     var e = eventCaller.currentEntity;
                    //     (int typeL, int typeR) = e["diffObjs"] ? ((int)e["typeL"], (int)e["typeR"]) : ((int)e["type"], (int)e["type"]);
                    //     DogNinja.QueueObject(e.beat, e["direction"], e["diffObjs"], typeL, typeR, e["shouldPrepare"], false, e);
                    // },
                    // inactiveFunction = delegate {
                    //     var e = eventCaller.currentEntity;
                    //     (int typeL, int typeR) = e["diffObjs"] ? ((int)e["typeL"], (int)e["typeR"]) : ((int)e["type"], (int)e["type"]);
                    //     DogNinja.QueueObject(e.beat, e["direction"], e["diffObjs"], typeL, typeR, e["shouldPrepare"], e["muteThrow"], e);
                    // },
                    preFunction = delegate {
                        var e = eventCaller.currentEntity;
                        (int typeL, int typeR) = e["diffObjs"] ? ((int)e["typeL"], (int)e["typeR"]) : ((int)e["type"], (int)e["type"]);
                        DogNinja.QueueObject(e.beat, e["direction"], e["diffObjs"], e["type"], typeL, typeR, e["shouldPrepare"], e["muteThrow"], e);
                    },
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("direction", DogNinja.ObjectDirection.Left, "Which Side", "Choose the side(s) the object(s) should be thrown from."),
                        new Param("diffObjs", false, "Different Objects", "Toggle if the sides should be different.", new() {
                            new((x, _) => (bool)x, "typeL", "typeR"),
                            new((x, _) => !(bool)x, "type"),
                        }),
                        new Param("type", DogNinja.ObjectType.Random, "Object", "Choose the object to be thrown."),
                        new Param("typeL", DogNinja.ObjectType.Random, "Left Object", "Choose the object to be thrown from the left."),
                        new Param("typeR", DogNinja.ObjectType.Random, "Right Object", "Choose the object to be thrown from the right."),
                        new Param("shouldPrepare", true, "Prepare", "Toggle if Dog Ninja should automatically prepare for this cue."),
                        new Param("muteThrow", false, "Mute", "Toggle if the cue should be muted. This only applies when the cue is started from another game."),
                    },
                },
                new GameAction("CutEverything", "Mister Eagle's Sign")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        if (eventCaller.gameManager.minigameObj.TryGetComponent(out DogNinja instance)) {
                            instance.CutEverything(e.beat, e.length, e["toggle"], e["text"]);
                        }
                    },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Play Sound", "Toggle if the sound effect should play for flying in."),
                        new Param("text", "Cut everything!", "Sign Text", "Set the text to be displayed on the sign.")
                    }
                },
                new GameAction("HereWeGo", "Here We Go!")
                {
                    function = delegate { DogNinja.HereWeGo(eventCaller.currentEntity.beat); },
                    inactiveFunction = delegate { DogNinja.HereWeGo(eventCaller.currentEntity.beat); },
                    defaultLength = 2,
                    preFunctionLength = 1,
                },
            },
            new List<string>() { "ntr", "normal" },
            "ntrninja", "en",
            new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_DogNinja;
    public class DogNinja : Minigame
    {
        struct QueuedThrow
        {
            public int[] types;
            public string sfxNumL, sfxNumR;
        }

        [Header("Animators")]
        public Animator DogAnim;    // dog misc animations
        public Animator BirdAnim;   // bird flying in and out

        [Header("References")]
        [SerializeField] GameObject ObjectBase;
        [SerializeField] GameObject FullBird;
        [SerializeField] SpriteRenderer WhichObject;
        public SpriteRenderer WhichLeftHalf;
        public SpriteRenderer WhichRightHalf;
        [SerializeField] TMP_Text cutEverythingText;

        [Header("Curves")]
        [SerializeField] BezierCurve3D CurveFromLeft;
        [SerializeField] BezierCurve3D CurveFromRight;

        [SerializeField] Sprite[] ObjectTypes;

        private bool autoBop;
        public bool queuePrepare;
        public bool preparing;

        public enum ObjectDirection
        {
            Left,
            Right,
            Both,
        }

        public enum ObjectType : int
        {
            Random,     // random fruit
            Apple,      // fruit
            Broccoli,   // fruit
            Carrot,     // fruit
            Cucumber,   // fruit
            Pepper,     // fruit
            Potato,     // fruit
            Bone,       // bone
            Pan,        // pan
            Tire,       // tire
            // custom objects that aren't in the og game
            AirBatter,
            Karateka,
            IaiGiriGaiden,
            ThumpFarm,
            BattingShow,
            MeatGrinder,
            Idol,
            TacoBell,
            //YaseiNoIkiG3M4,
        }

        protected static bool IA_PadAny(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }

        public static PlayerInput.InputAction InputAction_Press =
            new("NtrNinjaPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
            IA_PadAny, IA_TouchFlick, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_TouchPress =
            new("NtrNinjaTouchRelease", new int[] { IAEmptyCat, IAPressCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicPress, IA_Empty);
        public static PlayerInput.InputAction InputAction_TouchRelease =
            new("NtrNinjaTouchRelease", new int[] { IAEmptyCat, IAReleaseCat, IAEmptyCat },
            IA_Empty, IA_TouchBasicRelease, IA_Empty);

        public override void OnLateBeatPulse(double beat)
        {
            if (autoBop && preparing && (DogAnim.IsAnimationNotPlaying() || DogAnim.IsPlayingAnimationNames("Idle"))) {
                DogAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
        }

        public override void OnPlay(double beat) => Persist(beat, true);
        public override void OnGameSwitch(double beat) => Persist(beat, false);
        private void Persist(double beat, bool onPlay)
        {
            foreach (var e in gameManager.Beatmap.Entities.FindAll(e => e.datamodel is "dogNinja/ThrowObject" && beat > (onPlay ? e.beat : e.beat - 2) && beat < e.beat + 1))
            {
                QueuedThrow t = new();
                if (e.dynamicData.TryGetValue("throwData", out dynamic temp)) {
                    t = temp;
                } else {
                    QueueObject(e.beat, e["direction"], e["diffObjs"], e["type"], e["typeL"], e["typeR"], e["shouldPrepare"], true, e);
                }
                ThrowObject(e.beat, e["direction"], e["shouldPrepare"], t.types, t.sfxNumL, t.sfxNumR);
            }
        }

        private void Update()
        {
            // prepare queuing stuff
            if (queuePrepare && DogAnim.IsAnimationNotPlaying())
            {
                DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
                queuePrepare = false;
            }

            // controls stuff
            if (PlayerInput.GetIsAction(InputAction_TouchPress) && !GameManager.instance.autoplay)
            {
                queuePrepare = true;
                DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
            }
            if (PlayerInput.GetIsAction(InputAction_TouchRelease) && (!IsExpectingInputNow(InputAction_Press)) && (!GameManager.instance.autoplay))
            {
                queuePrepare = false;
                preparing = false;
                DogAnim.DoScaledAnimationAsync("Unprepare", 0.5f);
            }

            if (PlayerInput.GetIsAction(InputAction_Press) && !IsExpectingInputNow(InputAction_Press))
            {
                string slice = UnityEngine.Random.Range(0, 1f) < 0.5f ? "WhiffRight" : "WhiffLeft";
                DogAnim.DoScaledAnimationAsync(slice, 0.5f);

                SoundByte.PlayOneShotGame("dogNinja/whiff");
                queuePrepare = false;
            }
        }

        public void Bop(double beat, float length, bool auto, bool bop)
        {
            autoBop = auto;
            if (!bop) return;

            List<BeatAction.Action> actions = new();
            for (int i = 0; i < length; i++) {
                actions.Add(new(beat + i, delegate { DogAnim.DoScaledAnimationAsync("Bop", 0.5f); }));
            }
            if (actions.Count > 0) BeatAction.New(this, actions);
        }

        public static void QueueObject(double beat, int direction, bool diffObjs, int type, int typeL, int typeR, bool prepare, bool muteThrow, RiqEntity e)
        {
            int randomObj = 1;
            int[] types = diffObjs ? new[] { typeL, typeR } : new[] { type, type };
            string[] sfxNums = new string[2];

            for (int i = 0; i < 2; i++)
            {
                if (types[i] == 0 && (diffObjs || i == 0)) randomObj = UnityEngine.Random.Range((int)ObjectType.Apple, (int)ObjectType.Potato + 1);
                if (types[i] == 0) types[i] = randomObj;
                sfxNums[i] = "dogNinja/" + (types[i] < 7 ? "fruit" : Enum.GetName(typeof(ObjectType), types[i]));
            }

            if (!muteThrow) {
                for (int i = 0; i < (direction == 2 && diffObjs ? 2 : 1); i++) {
                    SoundByte.PlayOneShotGame(sfxNums[i] + "1", beat, forcePlay: true);
                }
            }

            if (GameManager.instance.minigameObj.TryGetComponent(out DogNinja instance)) {
                instance.ThrowObject(beat, direction, prepare, types, sfxNums[0], sfxNums[1]);
            } else {
                var queuedThrow = new QueuedThrow() {
                    types = types,
                    sfxNumL = sfxNums[0],
                    sfxNumR = sfxNums[1],
                };
                // funny static variable workaround :)
                if (!e.dynamicData.TryAdd("throwData", queuedThrow)) {
                    e["throwData"] = queuedThrow;
                }
            }
        }

        public void ThrowObject(double beat, int direction, bool prepare, int[] types, string sfxNumL, string sfxNumR)
        {
            BeatAction.New(this, new() {
                new(beat, () => queuePrepare = prepare && (PlayerInput.CurrentControlStyle != InputController.ControlStyles.Touch || GameManager.instance.autoplay))
            });
            for (int i = 0; i < (direction == 2 ? 2 : 1); i++)
            {
                bool l = direction is 2 ? i == 0 : direction == 0;
                WhichObject.sprite = ObjectTypes[l ? types[0] : types[1]];
                ThrowObject obj = Instantiate(ObjectBase, gameObject.transform).GetComponent<ThrowObject>();
                obj.startBeat = beat;
                obj.direction = direction;
                obj.fromLeft = l;
                obj.type = types[i];
                obj.sfxNum = l ? sfxNumL : sfxNumR;
                if (direction == 2) obj.shouldSfx = l == (types[0] == types[1]);
            }
            // if (direction is 0 or 2)
            // {
            //     WhichObject.sprite = ObjectTypes[typeL];
            //     ThrowObject ObjectL = Instantiate(ObjectBase, gameObject.transform).GetComponent<ThrowObject>();
            //     ObjectL.startBeat = beat;
            //     ObjectL.curve = CurveFromLeft;
            //     ObjectL.fromLeft = true;
            //     ObjectL.direction = direction;
            //     ObjectL.type = typeL;
            //     ObjectL.sfxNum = sfxNumL;
            //     if (direction == 2) ObjectL.shouldSfx = (typeL == typeR);
            // }

            // if (direction is 1 or 2)
            // {
            //     WhichObject.sprite = ObjectTypes[typeR];
            //     ThrowObject ObjectR = Instantiate(ObjectBase, gameObject.transform).GetComponent<ThrowObject>();
            //     ObjectR.startBeat = beat;
            //     ObjectR.curve = CurveFromRight;
            //     ObjectR.fromLeft = false;
            //     ObjectR.direction = direction;
            //     ObjectR.type = typeR;
            //     ObjectR.sfxNum = sfxNumR;
            //     if (direction == 2) ObjectR.shouldSfx = !(typeL == typeR);
            // }
        }

        public void CutEverything(double beat, float length, bool sound, string customText)
        {
            if (sound) SoundByte.PlayOneShotGame("dogNinja/bird_flap");
            BirdAnim.DoScaledAnimationAsync("FlyIn", 0.5f);
            cutEverythingText.text = customText;

            BeatAction.New(this, new() {
                new(beat + length, () => BirdAnim.Play("FlyOut", 0, 0))
            });
        }

        public void Prepare(double beat)
        {
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && PlayerInput.PlayerHasControl()) return;
            if (!queuePrepare) DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
            queuePrepare = true;
        }

        public static void HereWeGo(double beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                new MultiSound.Sound("dogNinja/here", beat),
                new MultiSound.Sound("dogNinja/we", beat + 0.5f),
                new MultiSound.Sound("dogNinja/go", beat + 1f)
            }, forcePlay: true);
        }
    }
}
