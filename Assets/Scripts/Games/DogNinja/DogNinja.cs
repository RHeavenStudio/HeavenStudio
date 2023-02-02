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
                    function = delegate { DogNinja.instance.ThrowObject(eventCaller.currentEntity.beat, eventCaller.currentEntity["toggle"], eventCaller.currentEntity["type"], eventCaller.currentEntity["text"], true); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Random Fruit", "Randomize the fruit"),
                        new Param("type", DogNinja.ObjectType.Apple, "Object", "The object to be thrown"),
                        new Param("text", "", "Alt. Objects", "An alternative object; one that doesn't exist in the main menu"),
                    }
                },
                new GameAction("ThrowObjectRight", "Throw Right Object")
                {
                    function = delegate { DogNinja.instance.ThrowObject(eventCaller.currentEntity.beat, eventCaller.currentEntity["toggle"], eventCaller.currentEntity["type"], eventCaller.currentEntity["text"], false); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", true, "Random Fruit", "Randomize the fruit"),
                        new Param("type", DogNinja.ObjectType.Apple, "Object", "The object to be thrown"),
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
        public GameObject Dog;
        public Transform ObjectHolder;
        public Transform HalvesHolder;
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
        private bool permaBop = true;
        public static bool dontBop = false;
        
        public static DogNinja instance;

        public enum ObjectType
        {
            Apple = 0,      // fruit
            Bone = 6,       // bone
            Broccoli = 1,   // fruit
            Carrot = 2,     // fruit
            Cucumber = 3,   // fruit
            Pan = 7,        // pan
            Pepper = 4,     // fruit
            Potato = 5,     // fruit
            Tire = 8,       // tire
            Custom = 9,     // directs to custom stuff
        }

        public enum CustomObject
        {
            TacoBell,
            NecoArc,
        }
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            //Debug.Log(rnd.NextDouble());
            
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && DogAnim.IsAnimationNotPlaying())
            {
                DogAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }

            if (PlayerInput.Pressed())
            {
                DogAnim.DoScaledAnimationAsync("Bop", 0.5f);
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("teehee :)");
                DogAnim.Play("Bop", 0, 0);
            }
        }

        public void Bop(float beat, bool bop, bool manual)
        {
            //if (manual) { DogAnim.Play("Bop", 0, 0); }

            if (bop) {
                dontBop = true;
            } else {
                dontBop = false;
            }
        }

        public void ThrowObject(float beat, bool isRandom, int ObjType, string textObj, bool fromLeft)
        {
            int ObjSprite;
            if (ObjType == 9) {
                Enum.TryParse(textObj, out CustomObject notIntObj);
                ObjSprite = (int) notIntObj;
            } else if (isRandom) {
                System.Random rd = new System.Random();
                ObjSprite = rd.Next(0, 4);
                Debug.Log(ObjSprite);
            } else { ObjSprite = ObjType; }
            WhichObject.sprite = ObjectTypes[ObjSprite];

            ThrowObject Object = Instantiate(ObjectBase).GetComponent<ThrowObject>();
            Object.startBeat = beat;
            Object.type = ObjType;
            Object.curve = fromLeft ? CurveFromLeft : CurveFromRight;
            Object.textObj = textObj;
        }

        public void CutEverything(float beat, bool sound)
        {
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
            }
        }

        public static void HereWeGoSFX(float x) 
        {
            MultiSound.Sound[] PlayHWG = new MultiSound.Sound[] { 
                    new MultiSound.Sound("dogNinja/here", x), 
                    new MultiSound.Sound("dogNinja/we", x + 0.5f),
                    new MultiSound.Sound("dogNinja/go", x + 1f)
                };
        }

        public void HereWeGo(float beat)
        {
            HereWeGoSFX(beat);
        }

        public static void HereWeGoInactive(float beat)
        {
            HereWeGoSFX(beat);
        }
    }
}
