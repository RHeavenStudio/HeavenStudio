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
            return new Minigame("dogNinja", "Dog Ninja", "524999", true, false, new List<GameAction>()
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
                /*
                new GameAction("ThrowObject", "Throw Object")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowObject(e.beat, e["type"], e["text"], e["left"], false); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                        new Param("text", "", "Alt. Objects", "An alternative object; one that doesn't exist in the main menu"),
                        new Param("left", true, "Throw from left?", "Whether the object should come from the left or right")
                    }
                },
                */
                new GameAction("ThrowObjectLeft", "Throw Object Left")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowObject(e.beat, e["type"], e["text"], true, false); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                        new Param("text", "", "Alt. Objects", "An alternative object; one that doesn't exist in the main menu"),
                    }
                },
                new GameAction("ThrowObjectRight", "Throw Object Right")
                {
                    function = delegate { var e = eventCaller.currentEntity; DogNinja.instance.ThrowObject(e.beat, e["type"], e["text"], false, false); }, 
                    defaultLength = 2,
                    parameters = new List<Param>()
                    {
                        new Param("type", DogNinja.ObjectType.Random, "Object", "The object to be thrown"),
                        new Param("text", "", "Alt. Objects", "An alternative object; one that doesn't exist in the main menu"),
                    }
                },
                new GameAction("ThrowObjectBoth", "Throw Object Left & Right")
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
        [SerializeField] Sprite[] CustomObjects;

        private float lastReportedBeat = 0f;
        private bool birdOnScreen = false;
        public bool usesCustomObject = false;
        static bool dontBop = false;
        public bool needPrepare = false;
        private const string sfxNum = "dogNinja/";
        
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
            cutEverythingCanvas.worldCamera = GameCamera.instance.camera;
        }

        private void Update()
        {
            if (DogAnim.GetBool("needPrepare") && DogAnim.IsAnimationNotPlaying())
            {
                DogAnim.DoScaledAnimationAsync("Prepare", 0.5f);
                DogAnim.SetBool("needPrepare", true);
            };
            
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

                DogAnim.DoScaledAnimationAsync(slice, 0.5f);
                Jukebox.PlayOneShotGame("dogNinja/whiff");
                DogAnim.SetBool("needPrepare", false);
            };
        }

        private void LateUpdate() 
        {
            if (Conductor.instance.ReportBeat(ref lastReportedBeat) && DogAnim.IsAnimationNotPlaying() && !dontBop)
            {
                DogAnim.DoScaledAnimationAsync("Bop", 0.5f);
            };
        }

        public void Bop(float beat, bool bop)
        {
            dontBop = !bop;
        }

        public void SetCustomText(string text)
        {
            cutEverythingText.text = text;
        }

        public void ThrowObject(float beat, int ObjType, string textObj, bool fromLeft, bool fromBoth)
        {
            int ObjSprite = ObjType;
            if (ObjType == 10) {
                // custom object code, uses the enum to turn the input string into integer to get the sprite
                Enum.TryParse(textObj, true, out CustomObject notIntObj);
                ObjSprite = (int) notIntObj;
                usesCustomObject = true;
                WhichObject.sprite = CustomObjects[ObjSprite];
            } else if (ObjType == 0) {
                // random object code. it makes a random number from 1-6 and sets that as the sprite
                System.Random rd = new System.Random();
                ObjSprite = rd.Next(1, 6);
                WhichObject.sprite = ObjectTypes[ObjSprite];
            } else { WhichObject.sprite = ObjectTypes[ObjSprite]; };

            // instantiate a game object and give it its variables
            ThrowObject Object = Instantiate(ObjectBase, ObjectHolder).GetComponent<ThrowObject>();
            Object.startBeat = beat;
            Object.curve = fromLeft ? CurveFromLeft : CurveFromRight;
            Object.fromLeft = fromLeft;
            Object.fromBoth = fromBoth;
            Object.textObj = textObj;
            Object.type = ObjType;
            Object.spriteInt = (ObjType == 10 ? ObjSprite + 10 : ObjSprite);
        }

        public void ThrowBothObject(float beat, int ObjType1, int ObjType2, string textObj1, string textObj2)
        {
            ThrowObject(beat, ObjType1, textObj1, false, true);
            ThrowObject(beat, ObjType2, textObj2, true, true);
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
                SetCustomText(customText);
            } else {
                BirdAnim.Play("FlyOut", 0, 0);
                birdOnScreen = false;
            };
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
