using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrDogNinjaLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("dogNinja", "Dog Ninja \n<color=#eb5454>[WIP]</color>", "524999", true, false, new List<GameAction>()
            {
                new GameAction("Bop", "Bop")
                {
                    function = delegate { DogNinja.instance.Bop(eventCaller.currentEntity.beat, eventCaller.currentEntity["toggle"], eventCaller.currentEntity["toggle"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Enable Bopping", "Whether to bop to the beat or not"),
                        new Param("toggle2", false, "Manual Bop \n<color=#eb5454>[WIP]</color>", "Bop, regardless of beat"),
                    }
                },
                new GameAction("ThrowObjectLeft", "Throw Left Object")
                {
                    function = delegate { DogNinja.instance.ThrowObject(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], eventCaller.currentEntity["text"], true); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                        new Param("text", "", "Alt. Objects", "An alternative object; one that doesn't exist in the main menu"),
                    }
                },
                new GameAction("ThrowObjectRight", "Throw Right Object")
                {
                    function = delegate { DogNinja.instance.ThrowObject(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], eventCaller.currentEntity["text"], false); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                        new Param("text", "", "Alt. Objects", "An alternative object; one that doesn't exist in the main menu"),
                    }
                },
                new GameAction("CutEverything", "Cut Everything!")
                {
                    function = delegate { DogNinja.instance.CutEverything(eventCaller.currentEntity.beat, eventCaller.currentEntity["toggle"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Play Sound", "Whether to play the 'FlyIn' SFX or not"),
                    }
                },
                new GameAction("HereWeGo", "Here We Go!")
                {
                    function = delegate { DogNinja.instance.HereWeGo(eventCaller.currentEntity.beat); },
                    defaultLength = 2,
                    inactiveFunction = delegate { DogNinja.HereWeGoInactive(eventCaller.currentEntity.beat); },
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
        public Animator BirdAnim;   // bird flying in and out
        public Animator DogAnim;    // dog misc animations
        
        [Header("References")]
        public GameObject ObjectBase;
        public GameObject HalvesLeftBase;
        public GameObject HalvesRightBase;
        public GameObject FullBird;
        //public GameObject Dog;
        public Transform ObjectHolder;
        public Transform LeftHalf;
        public Transform RightHalf;
        public SpriteRenderer WhichObject;
        public SpriteRenderer WhichLeftHalf;
        public SpriteRenderer WhichRightHalf;
        
        [Header("Curves")]
        public BezierCurve3D CurveFromLeft;
        public BezierCurve3D CurveFromRight;

        public Sprite[] ObjectTypes;
        public Sprite[] ObjectHalves;
        public Sprite[] CustomObjects;

        private float lastReportedBeat = 0f;
        private bool birdOnScreen = false;
        static bool dontBop = false;
        
        public float leftNumber = 0;
        public float rightNumber = 0;
        
        public static DogNinja instance;

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
            Custom,     // directs to custom stuff
        }

        // input these into the secret object box and boom. new object. 
        public enum CustomObject
        {
            TacoBell,
            AirBatter,
            Karateka,
            IaiGiriGaiden,
            ThumpFarm,
            BattingShow,
            MeatGrinder,
            YaseiNoIkiG3M4,
        }
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && DogAnim.IsAnimationNotPlaying() && !dontBop)
            {
                DogAnim.Play("Bop", 0, 0);
            };

            if (PlayerInput.Pressed())
            {
                System.Random rd = new System.Random();
                string slice;
                int LorR = rd.Next(0,2);
                if (LorR < 1) {
                    slice = "SliceRight";
                } else {
                    slice = "SliceLeft";
                };

                DogAnim.Play(slice, 0, 0);
            };
        }

        public void Bop(float beat, bool bop, bool manual)
        {
            if (manual) DogAnim.Play("Bop", 0, 0);

            if (bop) {
                dontBop = false;
            } else {
                dontBop = true;
            };
        }

        public void ThrowObject(float beat, int ObjType, string textObj, bool fromLeft)
        {
            int ObjSprite = 0;
            if (ObjType == 10) {
                // custom object code, uses the enum to turn the input string into integer to get the sprite
                Enum.TryParse(textObj, true, out CustomObject notIntObj);
                ObjSprite = (int) notIntObj;
                WhichObject.sprite = CustomObjects[ObjSprite];
            } else if (ObjType == 0) {
                // random object code. it makes a random number from 1-6 and sets that as the sprite
                System.Random rd = new System.Random();
                WhichObject.sprite = ObjectTypes[rd.Next(1, 6)];
            } else { WhichObject.sprite = ObjectTypes[ObjType]; };

            if (fromLeft) {
                leftNumber = Conductor.instance.songPositionInBeats;
            } else {
                rightNumber = Conductor.instance.songPositionInBeats;
            }

            // instantiate a game object and give it its variables
            ThrowObject Object = Instantiate(ObjectBase).GetComponent<ThrowObject>();
            Object.startBeat = beat;
            Object.type = ObjType;
            Object.curve = fromLeft ? CurveFromLeft : CurveFromRight;
            Object.fromLeft = fromLeft;
            Object.textObj = textObj;
        }

        public void CutEverything(float beat, bool sound)
        {
            
            // plays one anim with sfx when it's not on screen, plays a different anim with no sfx when on screen. ez
            if (!birdOnScreen) {
                FullBird.SetActive(true);
                if (sound) { 
                    Jukebox.PlayOneShotGame("dogNinja/bird_flap"); 
                }
                BirdAnim.Play("FlyIn", 0, 0);
                birdOnScreen = true;
            } else {
                BirdAnim.Play("FlyOut", 0, 0);
                birdOnScreen = false;
            };
        }

        // it's repeated code but the alternative saves no space

        public void HereWeGo(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("dogNinja/here", beat), 
                    new MultiSound.Sound("dogNinja/we", beat + 0.5f),
                    new MultiSound.Sound("dogNinja/go", beat + 1f)
                });
        }

        public static void HereWeGoInactive(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("dogNinja/here", beat), 
                    new MultiSound.Sound("dogNinja/we", beat + 0.5f),
                    new MultiSound.Sound("dogNinja/go", beat + 1f)
                }, forcePlay: true);
        }
    }
}
