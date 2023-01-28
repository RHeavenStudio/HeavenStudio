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
                        new Param("toggle", false, "Manual Bop", "Bop, regardless of beat"),
                    }
                },
                new GameAction("ThrowObjectLeft", "Throw Left Object")
                {
                    function = delegate { DogNinja.instance.ThrowObject(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], true); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Apple, "Object", "The object to be thrown"),
                    }
                },
                new GameAction("ThrowObjectRight", "Throw Right Object")
                {
                    function = delegate { DogNinja.instance.ThrowObject(eventCaller.currentEntity.beat, eventCaller.currentEntity["type"], false); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Apple, "Object", "The object to be thrown"),
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
        
        [Header("Curves")]
        public BezierCurve3D CurveFromLeft;
        public BezierCurve3D CurveFromRight;

        public Sprite[] ObjectTypes;
        public Sprite[] ObjectHalves;

        private float lastReportedBeat = 0f;
        private bool birdOnScreen = false;
        private bool bopOn = true;
        
        public static DogNinja instance;

        public enum ObjectType
        {
            Apple,      // 0, fruit1
            Bone,       // 1, bone1
            Broccoli,   // 2, fruit1
            Carrot,     // 3, fruit1
            Cucumber,   // 4, fruit1
            Pan,        // 5, pan1
            Pepper,     // 6, fruit1
            Potato,     // 7, fruit1
            Tire,       // 8, tire1
            TacoBell,   // 9, pan1 -> tacobell
        }
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && bopOn)
            {
                DogAnim.Play("Bop", 0, 0);
            }

            if (PlayerInput.Pressed())
            {
                DogAnim.Play("Slice", 0, 0);
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("teehee :)");
                DogAnim.Play("Bop", 0, 0);
            }
        }

        public void Bop(float beat, bool bop, bool manual)
        {
            if (manual) { DogAnim.Play("Bop"); }

            if (bop) {
                bopOn = true;
            } else {
                bopOn = false;
            }
        }

        public void ThrowObject(float beat, int ObjType, bool fromLeft)
        {
            ThrowObject Object = Instantiate(ObjectBase).GetComponent<ThrowObject>();
            Object.startBeat = beat;
            Object.type = ObjType;
            Object.fromLeft = fromLeft;
            Object.leftCurve = CurveFromLeft;
            Object.rightCurve = CurveFromRight;
        }

        public void CutEverything(float beat, bool sound)
        {
            if (!birdOnScreen) {
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

        public void HereWeGo(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("dogNinja/here", beat), 
                    new MultiSound.Sound("dogNinja/we", beat + 0.5f),
                    new MultiSound.Sound("dogNinja/go", beat + 1f)
                }, forcePlay: true);
        }

        public static void HereWeGoInactive(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { 
                    new MultiSound.Sound("dogNinja/here", beat), 
                    new MultiSound.Sound("dogNinja/we", beat + 0.5f),
                    new MultiSound.Sound("dogNinja/go", beat + 1f)
                }, forcePlay: true);

            // what could have been :cry: (idk how to do this)
            /* MultiSound.Sound[] HereWeGoSFX = new MultiSound.Sound[] { 
                    new MultiSound.Sound("dogNinja/here", beat), 
                    new MultiSound.Sound("dogNinja/we", beat + 0.5f),
                    new MultiSound.Sound("dogNinja/go", beat + 1f)
                }; */
        }
    }
}
