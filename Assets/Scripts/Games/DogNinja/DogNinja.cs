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
            return new Minigame("dogNinja", "Dog Ninja", "554899", true, false, new List<GameAction>()
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
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowObject(e.beat, e["direction"], e["typeL"], e["typeR"]); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("direction", DogNinja.ObjectDirection.Left, "Which Side", "Whether the object should come from the left, right, or both sides"),
                        new Param("typeL", DogNinja.ObjectType.Random, "Left \nObject", "The object to be thrown from the left"),
                        new Param("typeR", DogNinja.ObjectType.Random, "Right Object", "The object to be thrown from the right"),
                    }
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
                },

                // these are still here for backwards-compatibility but are hidden in the editor
                new GameAction("ThrowObjectLeft", "Throw Object Left")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowObject(e.beat, 0, e["type"], 0); }, 
                    defaultLength = 2,
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                    }
                },
                new GameAction("ThrowObjectRight", "Throw Object Right")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowObject(e.beat, 1, 0, e["type"]); }, 
                    defaultLength = 2,
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                    }
                },
                new GameAction("ThrowObjectBoth", "Throw Object Both")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowObject(e.beat, 2, e["typeL"], e["typeR"]); }, 
                    defaultLength = 2,
                    hidden = true,
                    parameters = new List<Param>()
                    {
                        new Param("typeL", DogNinja.ObjectType.Random, "Left Object", "The object on the left to be thrown"),
                        new Param("typeR", DogNinja.ObjectType.Random, "Right Object", "The object on the right to be thrown"),
                    }
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
        [Header("Animators")]
        public Animator DogAnim;    // dog misc animations
        public Animator BirdAnim;   // bird flying in and out
        
        [Header("References")]
        [SerializeField] GameObject ObjectBase;
        [SerializeField] GameObject FullBird;
        [SerializeField] SpriteRenderer WhichObject;
        [SerializeField] Transform ObjectHolder;
        public SpriteRenderer WhichLeftHalf;
        public SpriteRenderer WhichRightHalf;
        [SerializeField] Canvas cutEverythingCanvas;
        [SerializeField] TMP_Text cutEverythingText;
        
        [Header("Curves")]
        [SerializeField] BezierCurve3D CurveFromLeft;
        [SerializeField] BezierCurve3D CurveFromRight;

        [SerializeField] Sprite[] ObjectTypes;

        private float lastReportedBeat = 0f;
        private bool birdOnScreen = false;
        public bool usesCustomObject = false;
        static bool dontBop = false;
        public bool needPrepare = false;
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

        /*
        public enum CustomObject
        {
            TacoBell,
            AirBatter,
            Karateka,
            IaiGiriGaiden,
            ThumpFarm,
            BattingShow,
            MeatGrinder,
            // remove "//" to unleash an eons long dormant hell-beast
            //YaseiNoIkiG3M4,
            //AmongUs,
        }
        */
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (DogAnim.GetBool("needPrepare") && DogAnim.IsAnimationNotPlaying())
            {
                DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
                DogAnim.SetBool("needPrepare", true);
            }
            
            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
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

        public void ThrowObject(float beat, int direction, int typeL, int typeR)
        {
            int ObjSprite = 1;
            if ((typeL == 0 && direction == 0) 
            ||  (typeR == 0 && direction == 1) 
            ||  ((typeL == 0 || typeR == 0) && direction == 2)) {
                // random object code. it makes a random number from 1-6 and sets that as the sprite
                System.Random rd = new System.Random();
                ObjSprite = rd.Next(1, 7);
                WhichObject.sprite = ObjectTypes[ObjSprite];
                typeL = ObjSprite;
                typeR = ObjSprite;
            }

            // instantiate a game object and give it its variables
            if (direction == 0 || direction == 2) {
                WhichObject.sprite = ObjectTypes[typeL];
                ThrowObject ObjectL = Instantiate(ObjectBase, ObjectHolder).GetComponent<ThrowObject>();
                ObjectL.startBeat = beat;
                ObjectL.curve = CurveFromLeft;
                ObjectL.fromLeft = true;
                ObjectL.direction = direction;
                ObjectL.type = typeL;
                ObjectL.textObj = Enum.GetName(typeof(ObjectType), typeL);
            }

            if (direction == 1 || direction == 2) {
                WhichObject.sprite = ObjectTypes[typeR];
                ThrowObject ObjectR = Instantiate(ObjectBase, ObjectHolder).GetComponent<ThrowObject>();
                ObjectR.startBeat = beat;
                ObjectR.curve = CurveFromRight;
                ObjectR.fromLeft = false;
                ObjectR.direction = direction;
                ObjectR.type = typeR;
                ObjectR.textObj = Enum.GetName(typeof(ObjectType), typeR);
            }
        }

        // only here for backwards compatibility
        public void ThrowBothObject(float beat, int ObjType1, int ObjType2)
        {
            ThrowObject(beat, 2, ObjType1, ObjType2);
            //ThrowObject(beat, 1, 0, ObjType2);
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
