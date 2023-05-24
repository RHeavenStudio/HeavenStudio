using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDogNinjaLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("dogNinja", "Dog Ninja", "554899", false, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate { DogNinja.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity["toggle"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Enable Bopping", "Whether to bop to the beat or not"),
                    }
                },
                new GameAction("Prepare", "Prepare")
                {
                    function = delegate { DogNinja.instance.Prepare(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f,
                },
                new GameAction("ThrowObject", "Throw Object")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.QueueObject(e.beat, e["direction"], e["typeL"], e["typeR"], e["shouldPrepare"], false); },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; DogNinja.QueueObject(e.beat, e["direction"], e["typeL"], e["typeR"], e["shouldPrepare"], e["muteThrow"]); },
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("direction", DogNinja.ObjectDirection.Left, "Which Side", "Whether the object should come from the left, right, or both sides"),
                        new Param("typeL", DogNinja.ObjectType.Random, "Left \nObject", "The object to be thrown from the left"),
                        new Param("typeR", DogNinja.ObjectType.Random, "Right Object", "The object to be thrown from the right"),
                        new Param("shouldPrepare", true, "Prepare?", "Should the dog prepare?"),
                        new Param("muteThrow", false, "Mute", "Mute the throw? (ONLY WHEN INACTIVE)"),
                    },
                },
                new GameAction("CutEverything", "Cut Everything!")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.CutEverything(e.beat, e["toggle"], e["text"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Play Sound", "Whether to play the 'FlyIn' SFX or not"),
                        new Param("text", "Cut everything!", "Custom Text", "What text should the sign display?")
                    }
                },
                new GameAction("HereWeGo", "Here We Go!")
                {
                    function = delegate { DogNinja.instance.HereWeGo(eventCaller.currentEntity.beat); },
                    defaultLength = 2,
                    inactiveFunction = delegate { DogNinja.HereWeGoInactive(eventCaller.currentEntity.beat); },
                    preFunctionLength = 1,
                },

                // these are still here for backwards-compatibility but are hidden in the editor
                new GameAction("ThrowObjectLeft", "Throw Object Left")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.QueueObject(e.beat, 0, e["type"], 0, true, false);},
                    defaultLength = 2,
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                    },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; DogNinja.QueueObject(e.beat, 0, e["type"], 0, true, false);},
                },
                new GameAction("ThrowObjectRight", "Throw Object Right")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.QueueObject(e.beat, 1, 0, e["type"], true, false);},
                    defaultLength = 2,
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                    },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; DogNinja.QueueObject(e.beat, 1, 0, e["type"], true, false);},
                },
                new GameAction("ThrowObjectBoth", "Throw Object Both")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.QueueObject(e.beat, 2, e["typeL"], e["typeR"], true, false);},
                    defaultLength = 2,
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("typeL", DogNinja.ObjectType.Random, "Left Object", "The object on the left to be thrown"),
                        new Param("typeR", DogNinja.ObjectType.Random, "Right Object", "The object on the right to be thrown"),
                    },
                    inactiveFunction = delegate { var e = eventCaller.currentEntity; DogNinja.QueueObject(e.beat, 2, e["typeL"], e["typeR"], true, false);},
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_DogNinja;
    public class DogNinja : Minigame
    {
        static List<QueuedThrow> queuedThrows = new List<QueuedThrow>();
        struct QueuedThrow
        {
            public float beat;
            public int direction;
            public int typeL;
            public int typeR;
            public string sfxNumL;
            public string sfxNumR;
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

        private float lastReportedBeat = 0f;
        private bool birdOnScreen = false;
        static bool dontBop = false;
        private const string sfxNum = "dogNinja/";

        public static DogNinja instance;

        public enum ObjectDirection
        {
            Left,
            Right,
            Both,
        }

        public enum ObjectType
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
        
        private void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (!Conductor.instance.isPlaying || Conductor.instance.isPaused) {
                if (queuedThrows.Count > 0) queuedThrows.Clear();
            }
        }

        private void Update()
        {
            if (DogAnim.GetBool("needPrepare") && DogAnim.IsAnimationNotPlaying())
            {
                DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
                DogAnim.SetBool("needPrepare", true);
            }
            
            if (PlayerInput.Pressed(true) && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                System.Random rd = new System.Random();
                string slice;
                int LorR = rd.Next(0,2);
                if (LorR < 1) {
                    slice = "WhiffRight";
                } else {
                    slice = "WhiffLeft";
                }

                DogAnim.DoScaledAnimationAsync(slice, 0.5f);
                Jukebox.PlayOneShotGame("dogNinja/whiff");
                DogAnim.SetBool("needPrepare", false);
            }

            if (queuedThrows.Count > 0) {
                foreach (var obj in queuedThrows) { ThrowObject(obj.beat, obj.direction, obj.typeL, obj.typeR, obj.sfxNumL, obj.sfxNumR); }
                queuedThrows.Clear();
            }

            //if () queuedThrows.Clear();
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && DogAnim.IsAnimationNotPlaying() && !dontBop)
            {
                DogAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
        }

        public void Bop(float beat, bool bop)
        {
            dontBop = !bop;
        }

        public static void QueueObject(float beat, int direction, int typeL, int typeR, bool prepare, bool muteThrow)
        {
            int ObjSprite = 1;
            if (typeL == 0 || typeR == 0) {
                // random object code. it makes a random number from 1-7 and sets that as the sprite
                System.Random rd = new System.Random();
                ObjSprite = rd.Next(1, 7);
            }
            
            string sfxNumL = "dogNinja/";
            if (direction is 0 or 2) {
                sfxNumL += typeL < 7 ? "fruit" : Enum.GetName(typeof(ObjectType), typeL);
                if (typeL == 0) typeL = ObjSprite;
                if (!muteThrow) Jukebox.PlayOneShotGame(sfxNumL+"1", forcePlay: true);
            }
            
            string sfxNumR = "dogNinja/";
            if (direction is 1 or 2) {
                sfxNumR += typeR < 7 ? "fruit" : Enum.GetName(typeof(ObjectType), typeR);
                if (typeR == 0) typeR = ObjSprite;
                if (!(direction == 2 && typeL == typeR) && !muteThrow) Jukebox.PlayOneShotGame(sfxNumR+"1", forcePlay: true);
            }

            queuedThrows.Add(new QueuedThrow() {
                beat = beat,
                direction = direction,
                typeL = typeL,
                typeR = typeR,
                sfxNumL = sfxNumL,
                sfxNumR = sfxNumR,
            });

            if (prepare) DogNinja.instance.DogAnim.SetBool("needPrepare", true);
        }

        public void ThrowObject(float beat, int direction, int typeL, int typeR, string sfxNumL, string sfxNumR)
        {
            // instantiate a game object and give it its variables
            if (direction is 0 or 2) {
                WhichObject.sprite = ObjectTypes[typeL];
                ThrowObject ObjectL = Instantiate(ObjectBase, gameObject.transform).GetComponent<ThrowObject>();
                ObjectL.startBeat = beat;
                ObjectL.curve = CurveFromLeft;
                ObjectL.fromLeft = true;
                ObjectL.direction = direction;
                ObjectL.type = typeL;
                ObjectL.sfxNum = sfxNumL;
                if (direction == 2) ObjectL.shouldSfx = (typeL == typeR);
            }

            if (direction is 1 or 2) {
                WhichObject.sprite = ObjectTypes[typeR];
                ThrowObject ObjectR = Instantiate(ObjectBase, gameObject.transform).GetComponent<ThrowObject>();
                ObjectR.startBeat = beat;
                ObjectR.curve = CurveFromRight;
                ObjectR.fromLeft = false;
                ObjectR.direction = direction;
                ObjectR.type = typeR;
                ObjectR.sfxNum = sfxNumR;
                if (direction == 2) ObjectR.shouldSfx = !(typeL == typeR);
            }
        }

        public void CutEverything(float beat, bool sound, string customText)
        {
            // plays one anim with sfx when it's not on screen, plays a different anim with no sfx when on screen. ez
            if (!birdOnScreen) {
                FullBird.SetActive(true);
                if (sound) { 
                    Jukebox.PlayOneShotGame(sfxNum+"bird_flap"); 
                }
                BirdAnim.Play("FlyIn", 0, 0);
                birdOnScreen = true;
                cutEverythingText.text = customText;
            } else {
                BirdAnim.Play("FlyOut", 0, 0);
                birdOnScreen = false;
            }
        }

        public void Prepare(float beat)
        {
            if (!DogAnim.GetBool("needPrepare")) DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
            DogAnim.SetBool("needPrepare", true);
        }

        public void HereWeGo(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] {
                    new MultiSound.Sound(sfxNum+"here", beat), 
                    new MultiSound.Sound(sfxNum+"we", beat + 0.5f),
                    new MultiSound.Sound(sfxNum+"go", beat + 1f)
                }, forcePlay: true);
        }

        public static void HereWeGoInactive(float beat)
        {
            DogNinja.instance.HereWeGo(beat);
        }
    }
}
