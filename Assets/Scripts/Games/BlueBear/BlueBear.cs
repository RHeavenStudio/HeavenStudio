using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrBearLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("blueBear", "Blue Bear", "b4e6f6", "e7e7e7", "bf9d34", false, false, new List<GameAction>()
            {
                new GameAction("donut", "Donut")
                {
                    preFunction = delegate { BlueBear.TreatSound(eventCaller.currentEntity.beat, false); },
                    function = delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, false, eventCaller.currentEntity.beat); },
                    defaultLength = 3,
                },
                new GameAction("cake", "Cake")
                {
                    preFunction = delegate { BlueBear.TreatSound(eventCaller.currentEntity.beat, true); },
                    function = delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, true, eventCaller.currentEntity.beat); },
                    defaultLength = 4,
                },
                new GameAction("setEmotion", "Emotion")
                {
                    function = delegate { var e = eventCaller.currentEntity; BlueBear.instance.SetEmotion(e["type"]); },
                    parameters = new List<Param>()
                    {
                        new Param("type", BlueBear.EmotionType.ClosedEyes, "Type", "Which emotion should the blue bear use?")
                    }
                },
                new GameAction("stretchEmotion", "Long Emotion")
                {   
                    parameters = new List<Param>()
                    {
                        new Param("type", BlueBear.EmotionStretchType.StartCrying, "Type", "Which emotion should the blue bear use?")
                    }
                },
                new GameAction("wind", "Wind")
                {
                    function = delegate { BlueBear.instance.Wind(); },
                    defaultLength = 0.5f
                },
                new GameAction("story", "Story")
                {
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("story", BlueBear.StoryType.Date, "Story"),
                        new Param("enter", true, "Enter")
                    },
                    resizable = true
                },
                new GameAction("crumb", "Set Crumb Threshold")
                {
                    function = delegate { var e = eventCaller.currentEntity; BlueBear.instance.SetCrumbThreshold(e["right"], e["left"], e["reset"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("right", new EntityTypes.Integer(0, 500, 15), "Right Crumb", "How many treats should the bear eat before the right crumb can appear on his face?"),
                        new Param("left", new EntityTypes.Integer(0, 500, 30), "Left Crumb", "How many treats should the bear eat before the left crumb can appear on his face?"),
                        new Param("reset", false, "Reset Treats Eaten", "Should the numbers of treats eaten be reset?")
                    }
                }
            },
            new List<string>() { "ctr", "normal" },
            "ctrbear", "en",
            new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_BlueBear;

    public class BlueBear : Minigame
    {
        public enum EmotionType
        {
            Neutral,
            ClosedEyes,
            Cry,
            Sigh
        }
        public enum EmotionStretchType
        {
            LookUp,
            Smile, 
            StartCrying,
        }
        public enum StoryType
        {
            Date,
            Gift,
            Girl,
            Eat,
            BreakUp
        }
        [Header("Animators")]
        public Animator headAndBodyAnim; // Head and body
        public Animator bagsAnim; // Both bags sprite
        public Animator donutBagAnim; // Individual donut bag
        public Animator cakeBagAnim; // Individual cake bag
        [SerializeField] Animator windAnim;

        [Header("References")]
        [SerializeField] GameObject leftCrumb;
        [SerializeField] GameObject rightCrumb;
        [SerializeField] private Animator _storyAnim;
        public GameObject donutBase;
        public GameObject cakeBase;
        public GameObject crumbsBase;
        public Transform foodHolder;
        public Transform crumbsHolder;
        public GameObject individualBagHolder;

        [Header("Variables")]
        static int rightCrumbAppearThreshold = 15;
        static int leftCrumbAppearThreshold = 30;
        static int eatenTreats = 0;
        bool crying;
        private List<RiqEntity> _allStoryEvents = new();
        [SerializeField] private SuperCurveObject.Path[] _treatCurves;

        [Header("Gradients")]
        public Gradient donutGradient;
        public Gradient cakeGradient;

        private bool squashing;

        public static BlueBear instance;

        const int IALeft = 0;
        const int IARight = 1;
        protected static bool IA_PadLeft(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                    || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
        }
        protected static bool IA_BatonLeft(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.West, out dt);
        }
        protected static bool IA_TouchLeft(out double dt)
        {
            bool want = PlayerInput.GetTouchDown(InputController.ActionsTouch.Left, out dt);
            bool simul = false;
            // if (!want)
            // {
            //     simul = PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
            //                 && instance.IsExpectingInputNow(InputAction_Left.inputLockCategory)
            //                 && instance.IsExpectingInputNow(InputAction_Right.inputLockCategory);
            // }
            return want || simul;
        }

        protected static bool IA_PadRight(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt);
        }
        protected static bool IA_BatonRight(out double dt)
        {
            return PlayerInput.GetBatonDown(InputController.ActionsBaton.East, out dt);
        }
        protected static bool IA_TouchRight(out double dt)
        {
            bool want = PlayerInput.GetTouchDown(InputController.ActionsTouch.Right, out dt);
            bool simul = false;
            // if (!want)
            // {
            //     simul = PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
            //                 && instance.IsExpectingInputNow(InputAction_Right.inputLockCategory)
            //                 && instance.IsExpectingInputNow(InputAction_Left.inputLockCategory);
            // }
            return want || simul;
        }

        public static PlayerInput.InputAction InputAction_Left =
            new("CtrBearLeft", new int[] { IALeft, IALeft, IALeft },
            IA_PadLeft, IA_TouchLeft, IA_BatonLeft);

        public static PlayerInput.InputAction InputAction_Right =
            new("CtrBearRight", new int[] { IARight, IARight, IARight },
            IA_PadRight, IA_TouchRight, IA_BatonRight);

        // Editor gizmo to draw trajectories
        new void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            foreach (SuperCurveObject.Path path in _treatCurves)
            {
                if (path.preview)
                {
                    donutBase.GetComponent<SuperCurveObject>().DrawEditorGizmo(path);
                }
            }
        }

        public SuperCurveObject.Path GetPath(string name)
        {
            foreach (SuperCurveObject.Path path in _treatCurves)
            {
                if (path.name == name)
                {
                    return path;
                }
            }
            return default(SuperCurveObject.Path);
        }

        void OnDestroy()
        {
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
            if (Conductor.instance.isPlaying || Conductor.instance.isPaused) return;
            rightCrumbAppearThreshold = 15;
            leftCrumbAppearThreshold = 30;
            eatenTreats = 0;
        }

        private void Awake()
        {
            instance = this;
            if (Conductor.instance.isPlaying || Conductor.instance.isPaused) EatTreat(true);
            _allStoryEvents = EventCaller.GetAllInGameManagerList("blueBear", new string[] { "story" });
            UpdateStory();
            headAndBodyAnim.DoScaledAnimationAsync("Idle", 0.5f);
            bagsAnim.DoScaledAnimationAsync("Idle", 0.5f);
        }

        private int _storyIndex = 0;

        private void UpdateStory()
        {
            var cond = Conductor.instance;

            if (_storyIndex >= _allStoryEvents.Count) return;

            var currentStory = _allStoryEvents[_storyIndex];

            if (cond.songPositionInBeatsAsDouble >= currentStory.beat + currentStory.length && _storyIndex + 1 != _allStoryEvents.Count)
            {
                _storyIndex++;
                UpdateStory();
                return;
            }

            float normalizedBeat = Mathf.Clamp01(cond.GetPositionFromBeat(currentStory.beat, currentStory.length));

            bool enter = currentStory["enter"];

            switch (currentStory["story"])
            {
                case (int)StoryType.Date:
                    _storyAnim.DoNormalizedAnimation(enter ? "Flashback0" : "Flashback0Exit", normalizedBeat);
                    break;
                case (int)StoryType.Gift:
                    _storyAnim.DoNormalizedAnimation(enter ? "Flashback1" : "Flashback1Exit", normalizedBeat);
                    break;
                case (int)StoryType.Girl:
                    _storyAnim.DoNormalizedAnimation(enter ? "Flashback2" : "Flashback2Exit", normalizedBeat);
                    break;
                case (int)StoryType.Eat:
                    _storyAnim.DoNormalizedAnimation(enter ? "Flashback3" : "Flashback3Exit", normalizedBeat);
                    break;
                default:
                    _storyAnim.DoNormalizedAnimation(enter ? "Breakup" : "BreakupExit", normalizedBeat);
                    break;
            }
        }

        private void Update()
        {
            headAndBodyAnim.SetBool("ShouldOpenMouth", foodHolder.childCount != 0);

            if (PlayerInput.GetIsAction(InputAction_Left) && !IsExpectingInputNow(InputAction_Left.inputLockCategory))
            {
                Bite(true);
            }
            else if (PlayerInput.GetIsAction(InputAction_Right) && !IsExpectingInputNow(InputAction_Right.inputLockCategory))
            {
                Bite(false);
            }

            UpdateStory();
            headAndBodyAnim.SetScaledAnimationSpeed();
            bagsAnim.SetScaledAnimationSpeed();
            cakeBagAnim.SetScaledAnimationSpeed();
            donutBagAnim.SetScaledAnimationSpeed();
            windAnim.SetScaledAnimationSpeed();
        }

        public override void OnPlay(double beat)
        {
            HandleTreatsOnStart(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            HandleTreatsOnStart(beat);
        }

        private void HandleTreatsOnStart(double gameswitchBeat)
        {
            var allTreatEvents = EventCaller.GetAllInGameManagerList("blueBear", new string[] { "donut", "cake" });

            foreach (var e in allTreatEvents)
            {
                if (e.beat + e.length - 1 > gameswitchBeat && e.beat < gameswitchBeat)
                {
                    SpawnTreat(e.beat, e.datamodel == "blueBear/cake", gameswitchBeat);
                }
            }
        }

        public void Wind()
        {
            windAnim.DoScaledAnimationAsync("Wind", 0.5f);
        }

        public void Bite(bool left)
        {
            if (crying)
            {
                headAndBodyAnim.DoScaledAnimationAsync(left ? "CryBiteL" : "CryBiteR", 0.5f);
            }
            else
            {
                headAndBodyAnim.DoScaledAnimationAsync(left ? "BiteL" : "BiteR", 0.5f);
            }
        }

        public void SetCrumbThreshold(int rightThreshold, int leftThreshold, bool reset)
        {
            rightCrumbAppearThreshold = rightThreshold;
            leftCrumbAppearThreshold = leftThreshold;
            if (reset) eatenTreats = 0;
        }

        public void EatTreat(bool onlyCheck = false)
        {
            if (!onlyCheck) eatenTreats++;
            if (eatenTreats >= leftCrumbAppearThreshold)
            {
                leftCrumb.SetActive(true);
            }
            else
            {
                leftCrumb.SetActive(false);
            }
            if (eatenTreats >= rightCrumbAppearThreshold)
            {
                rightCrumb.SetActive(true);
            }
            else
            {
                rightCrumb.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            if (squashing)
            {
                var dState = donutBagAnim.GetCurrentAnimatorStateInfo(0);
                var cState = cakeBagAnim.GetCurrentAnimatorStateInfo(0);

                bool noDonutSquash = dState.IsName("DonutIdle");
                bool noCakeSquash = cState.IsName("CakeIdle");

                if (noDonutSquash && noCakeSquash)
                {
                    squashing = false;
                    bagsAnim.DoScaledAnimationAsync("Idle", 0.5f);
                }
            }
        }

        public void SetEmotion(int emotion)
        {
            switch (emotion)
            {
                case (int)EmotionType.Neutral:
                    //check if smiling then play "StopSmile"
                    headAndBodyAnim.DoScaledAnimationAsync("Idle", 0.5f);
                    crying = false;
                    break;
                case (int)EmotionType.ClosedEyes:
                    headAndBodyAnim.DoScaledAnimationAsync("EyesClosed", 0.5f);
                    crying = false;
                    break;
                /*
                case (int)EmotionType.LookUp:
                    emotionStartBeat = beat;
                    emotionLength = length;
                    emotionAnimName = "OpenEyes";
                    headAndBodyAnim.DoScaledAnimationAsync(emotionAnimName, 0.5f);
                    crying = false;
                    break;
                case (int)EmotionType.Smile:
                    emotionStartBeat = beat;
                    emotionLength = length;
                    emotionAnimName = "Smile";
                    headAndBodyAnim.DoScaledAnimationAsync(emotionAnimName, 0.5f);
                    crying = false;
                    break;
                case (int)EmotionType.Sad:
                    emotionStartBeat = beat;
                    emotionLength = length;
                    emotionAnimName = "Sad";
                    headAndBodyAnim.DoScaledAnimationAsync(emotionAnimName, 0.5f);
                    crying = true;
                    break;
                */
                case (int)EmotionType.Cry:
                    headAndBodyAnim.DoScaledAnimationAsync("CryIdle", 0.5f);
                    crying = true;
                    break;
                case (int)EmotionType.Sigh:
                    headAndBodyAnim.DoScaledAnimationAsync("Sigh", 0.5f);
                    crying = false;
                    break;
                default:
                    break;
            }
        }

        public void SpawnTreat(double beat, bool isCake, double gameSwitchBeat)
        {
            var objectToSpawn = isCake ? cakeBase : donutBase;
            var newTreat = GameObject.Instantiate(objectToSpawn, foodHolder);

            var treatComp = newTreat.GetComponent<Treat>();
            treatComp.startBeat = beat;

            newTreat.SetActive(true);

            if (beat >= gameSwitchBeat) SquashBag(isCake);
        }

        public static void TreatSound(double beat, bool isCake)
        {
            SoundByte.PlayOneShot(isCake ? "games/blueBear/cake" : "games/blueBear/donut", beat);
        }

        public void SquashBag(bool isCake)
        {
            squashing = true;
            bagsAnim.DoScaledAnimationAsync("Squashing", 0.5f);

            individualBagHolder.SetActive(true);

            if (isCake)
            {
                cakeBagAnim.DoScaledAnimationAsync("CakeSquash", 0.5f);
            }
            else
            {
                donutBagAnim.DoScaledAnimationAsync("DonutSquash", 0.5f);
            }
        }
    }
}
