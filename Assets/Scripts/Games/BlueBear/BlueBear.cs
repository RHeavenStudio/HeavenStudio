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
                    function = delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, false, eventCaller.currentEntity.beat, eventCaller.currentEntity["long"], eventCaller.currentEntity["open"]); },
                    defaultLength = 3,
                    parameters = new()
                    {
                        new("long", false, "Mouth Hold", "Toggle if Beary should keep his mouth and neck extended after catching the treat."),
                        new("open", true, "Open Mouth", "Toggle if Beary should open his mouth in preparation for the treat.")
                    }
                },
                new GameAction("cake", "Cake")
                {
                    preFunction = delegate { BlueBear.TreatSound(eventCaller.currentEntity.beat, true); },
                    function = delegate { BlueBear.instance.SpawnTreat(eventCaller.currentEntity.beat, true, eventCaller.currentEntity.beat, eventCaller.currentEntity["long"], eventCaller.currentEntity["open"]); },
                    defaultLength = 4,
                    parameters = new()
                    {
                        new("long", false, "Mouth Hold", "Toggle if Beary should keep his mouth and neck extended after catching the treat."),
                        new("open", true, "Open Mouth", "Toggle if Beary should open his mouth in preparation for the treat.")
                    }
                },
                new GameAction("stretchEmotion", "Emotion")
                {
                    function = delegate { var e = eventCaller.currentEntity; if (e["instant"] || e["type"] == (int)BlueBear.EmotionStretchType.NoEmotion) BlueBear.instance.SetEmotion(e.beat, e["type"]); },
                    defaultLength = 4,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", BlueBear.EmotionStretchType.NoEmotion, "Emotion", "Set the emotion animation Beary should play.", new()
                        {
                            new((x, _) => (int)x != (int)BlueBear.EmotionStretchType.NoEmotion, new string[] { "instant" })
                        }),
                        new Param("instant", false, "Instant", "Toggle if the emotion should jump to it's end state. Making \"LookUp\" instant will cause Beary to keep his head down."),
                    }
                },
                new GameAction("wind", "Wind")
                {
                    function = delegate { BlueBear.instance.Wind(); },
                    defaultLength = 0.5f
                },
                new GameAction("sigh", "Sigh")
                {
                    function = delegate { BlueBear.instance.Sigh(); },
                    defaultLength = 0.5f
                },
                new GameAction("open", "Open Mouth")
                {
                    function = delegate { BlueBear.instance.OpenMouth(); },
                    defaultLength = 0.5f
                },
                new GameAction("story", "Memory Drawing")
                {
                    defaultLength = 4,
                    parameters = new List<Param>()
                    {
                        new Param("story", BlueBear.StoryType.Date, "Memory", "Set the memory that should appear."),
                        new Param("enter", true, "Enter", "Toggle if the memory should enter or exit the scene.")
                    },
                    resizable = true
                },
                new GameAction("crumb", "Set Crumb Thresholds")
                {
                    function = delegate { var e = eventCaller.currentEntity; BlueBear.instance.SetCrumbThreshold(e["right"], e["left"], e["reset"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("right", new EntityTypes.Integer(0, 500, 15), "Right Crumb", "Set how many treats Beary needs to eat for the right crumb to appear."),
                        new Param("left", new EntityTypes.Integer(0, 500, 30), "Left Crumb", "Set how many treats Beary needs to eat for the left crumb to appear."),
                        new Param("reset", false, "Reset Treats Eaten", "Toggle if the current amount of treats eaten (and crumbs) should be reset.")
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
        public enum EmotionStretchType
        {
            NoEmotion = -1,
            LookUp = 0,
            Smile = 1, 
            StartCrying = 2,
            ClosedEyes = 3,
            SmileInstant = 4,
            CryingInstant
        }

        public static bool IsInstantEmotion(int emotion)
        {
            return emotion is (int)EmotionStretchType.NoEmotion or (int)EmotionStretchType.ClosedEyes or (int)EmotionStretchType.SmileInstant or (int)EmotionStretchType.CryingInstant;
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
        private int rightCrumbAppearThreshold = 15;
        private int leftCrumbAppearThreshold = 30;
        private int eatenTreats = 0;
        bool crying;
        private List<RiqEntity> _allStoryEvents = new();
        [SerializeField] private SuperCurveObject.Path[] _treatCurves;

        [Header("Gradients")]
        public Gradient donutGradient;
        public Gradient cakeGradient;

        private bool squashing;
        [NonSerialized] public int shouldOpenMouthCount = 0;

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

        private void Awake()
        {
            instance = this;
            _allStoryEvents = EventCaller.GetAllInGameManagerList("blueBear", new string[] { "story" });
            UpdateStory();
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
            headAndBodyAnim.SetBool("ShouldOpenMouth", shouldOpenMouthCount != 0 || _wantMouthOpen);
            if (headAndBodyAnim.GetBool("ShouldOpenMouth"))
            {
                _emotionCancelledBeat = Conductor.instance.songPositionInBeatsAsDouble;
            }

            if (PlayerInput.GetIsAction(InputAction_Left) && !IsExpectingInputNow(InputAction_Left.inputLockCategory))
            {
                SoundByte.PlayOneShotGame("blueBear/whiff", -1, SoundByte.GetPitchFromSemiTones(UnityEngine.Random.Range(-1, 2), false));
                Bite(Conductor.instance.songPositionInBeatsAsDouble, true, false);
            }
            else if (PlayerInput.GetIsAction(InputAction_Right) && !IsExpectingInputNow(InputAction_Right.inputLockCategory))
            {
                SoundByte.PlayOneShotGame("blueBear/whiff", -1, SoundByte.GetPitchFromSemiTones(UnityEngine.Random.Range(-1, 2), false));
                Bite(Conductor.instance.songPositionInBeatsAsDouble, false, false);
            }

            UpdateEmotions();

            UpdateStory();
            headAndBodyAnim.SetScaledAnimationSpeed();
            bagsAnim.SetScaledAnimationSpeed();
            cakeBagAnim.SetScaledAnimationSpeed();
            donutBagAnim.SetScaledAnimationSpeed();
            windAnim.SetScaledAnimationSpeed();
        }

        private double _emotionCancelledBeat = -1;
        private int _emotionIndex = 0;
        private List<RiqEntity> _allEmotionsStretch = new();
        private EmotionStretchType _lastEmotion = EmotionStretchType.LookUp;
        private bool _firstEmotionFrame = true;

        private void UpdateEmotions()
        {
            var cond = Conductor.instance;
            if (_allEmotionsStretch.Count == 0 || _emotionIndex >= _allEmotionsStretch.Count) return;

            var beat = cond.songPositionInBeatsAsDouble;

            var e = _allEmotionsStretch[_emotionIndex];

            if (beat > e.beat + e.length)
            {
                _emotionIndex++;
                _lastEmotion = (EmotionStretchType)_allEmotionsStretch[_emotionIndex - 1]["type"];
                crying = _lastEmotion == EmotionStretchType.StartCrying;
                _firstEmotionFrame = true;
                if (_lastEmotion == EmotionStretchType.Smile)
                {
                    headAndBodyAnim.DoScaledAnimationAsync("StopSmile", 0.5f);
                }

                UpdateEmotions();
                return;
            }

            if (beat >= e.beat && beat < e.beat + e.length && !(_emotionCancelledBeat >= e.beat))
            {
                _lastEmotion = (EmotionStretchType)e["type"];
                crying = _lastEmotion == EmotionStretchType.StartCrying;

                if (_firstEmotionFrame && _lastEmotion == EmotionStretchType.Smile)
                {
                    headAndBodyAnim.DoScaledAnimationAsync("Smile", 0.5f);
                    _firstEmotionFrame = false;
                }

                float normalizedBeat = cond.GetPositionFromBeat(e.beat, e.length);

                string animName = (EmotionStretchType)e["type"] switch
                {
                    EmotionStretchType.LookUp => "OpenEyes",
                    EmotionStretchType.Smile => "",
                    EmotionStretchType.StartCrying => "Sad",
                    EmotionStretchType.NoEmotion => "",
                    _ => throw new NotImplementedException(),
                };
                if (animName != "") headAndBodyAnim.DoNormalizedAnimation(animName, normalizedBeat);
            }
        }

        private void HandleEmotions(double beat)
        {
            _allEmotionsStretch = EventCaller.GetAllInGameManagerList("blueBear", new string[] { "stretchEmotion" }).FindAll(x => !IsInstantEmotion(x["type"]));
            if (_allEmotionsStretch.Count != 0)
            {
                UpdateEmotions();
                var allEmosBeforeBeat = _allEmotionsStretch.FindAll(x => x.beat < beat);
                if (allEmosBeforeBeat.Count != 0)
                {
                    if ((EmotionStretchType)allEmosBeforeBeat[^1]["type"] == EmotionStretchType.StartCrying)
                    {
                        headAndBodyAnim.DoScaledAnimationAsync("CryIdle", 0.5f);
                    }
                    else if ((EmotionStretchType)allEmosBeforeBeat[^1]["type"] == EmotionStretchType.Smile)
                    {
                        headAndBodyAnim.DoScaledAnimationAsync("SmileIdle", 0.5f);
                    }
                }
            }
            var allSetEmotionsBeforeBeat = EventCaller.GetAllInGameManagerList("blueBear", new string[] { "stretchEmotion" }).FindAll(x => IsInstantEmotion(x["type"]) && x.beat < beat);
            if (allSetEmotionsBeforeBeat.Count == 0) return;
            var lastEvent = allSetEmotionsBeforeBeat[^1];
            SetEmotion(lastEvent.beat, lastEvent["type"]);
        }

        public override void OnPlay(double beat)
        {
            HandleTreatsOnStart(beat);
            HandleEmotions(beat);
            HandleCrumbs(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            HandleTreatsOnStart(beat);
            HandleEmotions(beat);
            HandleCrumbs(beat);
        }

        private void HandleTreatsOnStart(double gameswitchBeat)
        {
            var allTreatEvents = EventCaller.GetAllInGameManagerList("blueBear", new string[] { "donut", "cake" });

            foreach (var e in allTreatEvents)
            {
                if (e.beat + e.length - 1 > gameswitchBeat && e.beat < gameswitchBeat)
                {
                    SpawnTreat(e.beat, e.datamodel == "blueBear/cake", gameswitchBeat, e["long"], e["open"]);
                }
            }
        }

        public void Wind()
        {
            windAnim.DoScaledAnimationAsync("Wind", 0.5f);
        }

        public void Sigh()
        {
            headAndBodyAnim.DoScaledAnimationAsync("Sigh", 0.5f);
        }

        public void OpenMouth()
        {
            _wantMouthOpen = true;
        }

        public void Bite(double beat, bool left, bool longBite)
        {
            _emotionCancelledBeat = beat;
            _wantMouthOpen = false;
            if (crying)
            {
                headAndBodyAnim.DoScaledAnimationAsync((longBite ? "Long" : "") + (left ? "CryBiteL" : "CryBiteR"), 0.5f);
            }
            else
            {
                headAndBodyAnim.DoScaledAnimationAsync((longBite ? "Long" : "") + (left ? "BiteL" : "BiteR"), 0.5f);
            }
        }

        private void HandleCrumbs(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("blueBear", new string[] { "crumb" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count == 0) return;
            var lastCrumbEvent = allEventsBeforeBeat[^1];
            SetCrumbThreshold(lastCrumbEvent["right"], lastCrumbEvent["left"], lastCrumbEvent["reset"]);
            EatTreat(false);
        }

        public void SetCrumbThreshold(int rightThreshold, int leftThreshold, bool reset)
        {
            rightCrumbAppearThreshold = rightThreshold;
            leftCrumbAppearThreshold = leftThreshold;
            if (reset) eatenTreats = 0;
        }

        public void EatTreat(bool appendTreats = true)
        {
            if (appendTreats) eatenTreats++;
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

        private bool _wantMouthOpen = false;

        public void SetEmotion(double beat, int emotion)
        {
            _emotionCancelledBeat = beat;
            _wantMouthOpen = false;
            crying = false;
            switch (emotion)
            {
                case (int)EmotionStretchType.NoEmotion:
                    headAndBodyAnim.DoScaledAnimationAsync("Idle", 0.5f);
                    break;
                case (int)EmotionStretchType.ClosedEyes:
                    headAndBodyAnim.DoScaledAnimationAsync("EyesClosed", 0.5f);
                    break;
                case (int)EmotionStretchType.CryingInstant:
                    headAndBodyAnim.DoScaledAnimationAsync("CryIdle", 0.5f);
                    crying = true;
                    break;
                case (int)EmotionStretchType.SmileInstant:
                    headAndBodyAnim.DoScaledAnimationAsync("SmileIdle", 0.5f);
                    break;
                default:
                    break;
            }
        }

        public void SpawnTreat(double beat, bool isCake, double gameSwitchBeat, bool longBite, bool shouldOpen)
        {
            var objectToSpawn = isCake ? cakeBase : donutBase;
            var newTreat = GameObject.Instantiate(objectToSpawn, foodHolder);

            var treatComp = newTreat.GetComponent<Treat>();
            treatComp.startBeat = beat;
            treatComp.hold = longBite;
            treatComp.shouldOpen = shouldOpen;
            if (shouldOpen) shouldOpenMouthCount++;

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
