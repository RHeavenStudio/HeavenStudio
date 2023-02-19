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
                new GameAction("Prepare", "Prepare")
                {
                    function = delegate { DogNinja.instance.Prepare(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f,
                },
                new GameAction("ThrowObjectLeft", "Throw Object")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowObject(e.beat, e["type"], e["text"], e["left"]); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                        new Param("text", "", "Alt. Objects", "An alternative object; one that doesn't exist in the main menu"),
                        new Param("left", true, "Throw from left?", "Whether the object should come from the left or right")
                    }
                },
                new GameAction("ThrowObjectBoth", "Throw Right & Left Object")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowBothObject(e.beat, e["type"], e["type2"], e["text"], e["text2"]); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Left Object", "The object on the left to be thrown"),
                        new Param("type2", DogNinja.ObjectType.Random, "Right Object", "The object on the right to be thrown"),
                        new Param("text", "", "Left Alt. Object", "An alternative object on the left; one that doesn't exist in the main menu"),
                        new Param("text2", "", "Right Alt. Object", "An alternative object on the right; one that doesn't exist in the main menu"),
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
        public Animator DogAnim;    // dog misc animations
        public Animator BirdAnim;   // bird flying in and out
        
        [Header("References")]
        public GameObject ObjectBase;
        public GameObject HalvesLeftBase;
        public GameObject HalvesRightBase;
        public GameObject FullBird;
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
        string sfxNum = "dogNinja/";
        
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

        // input these into the secret object box for custom objects 
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
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            

            if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                System.Random rd = new System.Random();
                string slice;
                int LorR = rd.Next(0,2);
                if (LorR < 1) {
                    slice = "WhiffRight";
                } else {
                    slice = "WhiffLeft";
                };

                //DogAnim.DoScaledAnimation(slice, lastReportedBeat);
                
                DogAnim.DoScaledAnimationAsync(slice, 0.5f);
                Jukebox.PlayOneShotGame("dogNinja/whiff");
            };
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && DogAnim.IsAnimationNotPlaying() && !dontBop)
            {
                DogAnim.DoScaledAnimationAsync("Bop", 0.5f);
            };
        }

        public void Bop(float beat, bool bop, bool manual)
        {
            if (manual) DogAnim.DoScaledAnimationAsync("Bop", 0.5f);
            dontBop = !bop;
        }

        // my solution for making three functions for three cues; just put the big complicated code into another function
        public void WhichObjectMath(int ObjType, string textObj)
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
        }

        public void ThrowObject(float beat, int ObjType, string textObj, bool fromLeft)
        {
            WhichObjectMath(ObjType, textObj);

            // instantiate a game object and give it its variables
            ThrowObject Object = Instantiate(ObjectBase).GetComponent<ThrowObject>();
            Object.startBeat = beat;
            Object.type = ObjType;
            Object.curve = fromLeft ? CurveFromLeft : CurveFromRight;
            Object.fromLeft = fromLeft;
            Object.textObj = textObj;
            Object.sfxNum = sfxNum;
        }

        public void ThrowBothObject(float beat, int ObjType1, int ObjType2, string textObj1, string textObj2)
        {
            ThrowObject(beat, ObjType1, textObj1, false);
            ThrowObject(beat, ObjType2, textObj2, true);
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

        public void Prepare(float beat)
        {
            DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
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
