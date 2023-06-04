using DG.Tweening;
using NaughtyBezierCurves;
using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class CtrBearLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("blueBear", "Blue Bear", "b4e6f6", false, false, new List<GameAction>()
            {
                new GameAction("donut", "Donut")
                {
                    function = delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, false); }, 
                    defaultLength = 3,
                },
                new GameAction("cake", "Cake")
                {
                    function = delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, true); }, 
                    defaultLength = 4,
                },
                new GameAction("setEmotion", "Set Emotion")
                {
                    function = delegate { var e = eventCaller.currentEntity; BlueBear.instance.SetEmotion(e.beat, e.length, e["type"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", BlueBear.EmotionType.ClosedEyes, "Type", "Which emotion should the blue bear use?")
                    }
                },
                new GameAction("wind", "Wind")
                {
                    function = delegate { BlueBear.instance.Wind(); },
                    defaultLength = 0.5f
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
            new List<string>() {"ctr", "normal"},
            "ctrbear", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_BlueBear;
    public class BlueBear : Minigame
    {
        public enum EmotionType
        {
            Neutral,
            ClosedEyes,
            LookUp,
            Smile,
            Sad,
            InstaSad,
            Sigh
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
        float emotionStartBeat;
        float emotionLength;
        string emotionAnimName;
        bool crying;

        [Header("Curves")]
        public BezierCurve3D donutCurve;
        public BezierCurve3D cakeCurve;

        [Header("Gradients")]
        public Gradient donutGradient;
        public Gradient cakeGradient;

        private bool squashing;

        public static BlueBear instance;

        void OnDestroy()
        {
            if (Conductor.instance.isPlaying || Conductor.instance.isPaused) return;
            rightCrumbAppearThreshold = 15;
            leftCrumbAppearThreshold = 30;
            eatenTreats = 0;
            foreach (var evt in scheduledInputs)
            {
                evt.Disable();
            }
        }

        private void Awake()
        {
            instance = this;
            if (Conductor.instance.isPlaying || Conductor.instance.isPaused) EatTreat(true);
        }

        private void Update()
        {
            headAndBodyAnim.SetBool("ShouldOpenMouth", foodHolder.childCount != 0);

            if (PlayerInput.GetAnyDirectionDown() && !IsExpectingInputNow(InputType.DIRECTION_DOWN))
            {
                Bite(true);
            }
            else if (PlayerInput.Pressed() && !IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                Bite(false);
            }

            var cond = Conductor.instance;

            if (cond.isPlaying && !cond.isPaused)
            {
                float normalizedBeat = cond.GetPositionFromBeat(emotionStartBeat, emotionLength);
                if (normalizedBeat >= 0 && normalizedBeat <= 1f)
                {
                    //headAndBodyAnim.DoNormalizedAnimation(emotionAnimName, normalizedBeat);
                }
            }
        }

        public void Wind()
        {
            windAnim.Play("Wind", 0, 0);
        }

        public void Bite(bool left)
        {
            if (crying)
            {
                headAndBodyAnim.Play(left ? "CryBiteL" : "CryBiteR", 0, 0);
            }
            else
            {
                headAndBodyAnim.Play(left ? "BiteL" : "BiteR", 0, 0);
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
                    bagsAnim.Play("Idle", 0, 0);
                }
            }
        }

        public void SetEmotion(float beat, float length, int emotion)
        {
            switch (emotion)
            {
                case (int)EmotionType.Neutral:
                    if (emotionAnimName == "Smile")
                    {
                        headAndBodyAnim.Play("StopSmile", 0, 0);
                        emotionAnimName = "";
                    }
                    else
                    {
                        headAndBodyAnim.Play("Idle", 0, 0);
                    }
                    crying = false;
                    break;
                case (int)EmotionType.ClosedEyes:
                    headAndBodyAnim.Play("EyesClosed", 0, 0);
                    crying = false;
                    break;
                case (int)EmotionType.LookUp:
                    emotionStartBeat = beat;
                    emotionLength = length;
                    emotionAnimName = "OpenEyes";
                    headAndBodyAnim.Play(emotionAnimName, 0, 0);
                    crying = false;
                    break;
                case (int)EmotionType.Smile:
                    emotionStartBeat = beat;
                    emotionLength = length;
                    emotionAnimName = "Smile";
                    headAndBodyAnim.Play(emotionAnimName, 0, 0);
                    crying = false;
                    break;
                case (int)EmotionType.Sad:
                    emotionStartBeat = beat;
                    emotionLength = length;
                    emotionAnimName = "Sad";
                    headAndBodyAnim.Play(emotionAnimName, 0, 0);
                    crying = true;
                    break;
                case (int)EmotionType.InstaSad:
                    headAndBodyAnim.Play("CryIdle", 0, 0);
                    crying = true;
                    break;
                case (int)EmotionType.Sigh:
                    headAndBodyAnim.Play("Sigh", 0, 0);
                    crying = false;
                    break;
                default:
                    break;
            }
        }

        public void SpawnTreat(float beat, bool isCake)
        {
            var objectToSpawn = isCake ? cakeBase : donutBase;
            var newTreat = GameObject.Instantiate(objectToSpawn, foodHolder);
            
            var treatComp = newTreat.GetComponent<Treat>();
            treatComp.startBeat = beat;
            treatComp.curve = isCake ? cakeCurve : donutCurve;

            newTreat.SetActive(true);

            Jukebox.PlayOneShotGame(isCake ? "blueBear/cake" : "blueBear/donut");

            SquashBag(isCake);
        }

        public void SquashBag(bool isCake)
        {
            squashing = true;
            bagsAnim.Play("Squashing", 0, 0);

            individualBagHolder.SetActive(true);

            if (isCake)
            {
                cakeBagAnim.Play("CakeSquash", 0, 0);
            }
            else
            {
                donutBagAnim.Play("DonutSquash", 0, 0);
            }
        }
    }
}
