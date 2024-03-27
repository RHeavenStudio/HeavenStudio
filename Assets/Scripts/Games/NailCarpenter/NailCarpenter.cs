using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using Jukebox;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class PcoNailLoader
    {
        public static Minigame AddGame(EventCaller eventCaller)
        {
            return new Minigame("nailCarpenter", "Nail Carpenter", "fab96e", false, false, new List<GameAction>()
            {
                new GameAction("puddingNail", "Pudding Nail")
                {
                    defaultLength = 8f,
                    resizable = true
                },
                new GameAction("cherryNail", "Cherry Nail")
                {
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("cakeNail", "Cake Nail")
                {
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("cakeLongNail", "Cake Long Nail")
                {
                    defaultLength = 4f,
                    resizable = true
                },
                new GameAction("slideFusuma", "Slide Shoji")
                {
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        NailCarpenter.instance.SlideShoji(e.beat, e.length, e["fillRatio"], e["ease"], e["mute"]);
                    },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("fillRatio", new EntityTypes.Float(0f, 1f, 0.3f), "Ratio", "Set how much of the screen the shoji covers."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action."),
                        new Param("mute", false, "Mute", "Toggle if the cue should be muted.")
                    }
                },

            },
            new List<string>() { "pco", "normal" },
            "pconail", "en",
            new List<string>() { }
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_NailCarpenter;

    public class NailCarpenter : Minigame
    {
        const double PATTERN_SEEK_TIME = 8.0;

        [Serializable]
        public struct ObjectPatternItem
        {
            public double beat;
            public ObjectType type;
        }

        public enum ObjectType
        {
            Nail,
            LongNail,
            Sweet,
            ForceCherry,
            ForcePudding,
            ForceCherryPudding,
            ForceShortCake,
            ForceLayerCake,
            None,
            LongCharge
        }

        struct ScheduledPattern
        {
            public double beat;
            public double length;
            public PatternType type;
        }

        enum PatternType
        {
            Pudding,
            Cherry,
            Cake,
            CakeLong,
            None
        }

        [SerializeField] ObjectPatternItem[] puddingPattern;
        [SerializeField] ObjectPatternItem[] cherryPattern;
        [SerializeField] ObjectPatternItem[] cakePattern;
        [SerializeField] ObjectPatternItem[] cakeLongPattern;
        [SerializeField] float scrollMetresPerBeat = 4f;
        [SerializeField] float boardWidth = 19.2f;

        public GameObject baseNail;
        public GameObject baseLongNail;
        public GameObject baseSweet;
        public Animator Carpenter;
        public Animator EffectExclamRed;
        public Animator EffectExclamBlue;

        public Transform scrollingHolder;
        public Transform nailHolder;
        public Transform boardTrans;
        public Transform shojiTrans;

        private bool missed;
        private bool hasSlurped;

        const int IAAltDownCat = IAMAXCAT;
        const int IASweetsCat = IAMAXCAT + 1;

        protected static bool IA_PadAltPress(out double dt)
        {
            return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
        }
        protected static bool IA_BatonAltPress(out double dt)
        {
            return PlayerInput.GetSqueezeDown(out dt);
        }

        protected static bool IA_TouchRegularPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && !instance.IsExpectingInputNow(InputAction_AltPress);
        }
        protected static bool IA_TouchAltPress(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && instance.IsExpectingInputNow(InputAction_AltPress)
                && !instance.IsExpectingInputNow(InputAction_RegPress);
        }

        protected static bool IA_PadSweetsCheck(out double dt)
        {
            return (PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt)
                || PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt))
                && !(instance.IsExpectingInputNow(InputAction_RegPress) || instance.IsExpectingInputNow(InputAction_AltPress));
        }
        protected static bool IA_BatonSweetsCheck(out double dt)
        {
            return (PlayerInput.GetBatonDown(InputController.ActionsBaton.Face, out dt)
                || PlayerInput.GetSqueezeDown(out dt))
                && !(instance.IsExpectingInputNow(InputAction_RegPress) || instance.IsExpectingInputNow(InputAction_AltPress));
        }
        protected static bool IA_TouchSweetsCheck(out double dt)
        {
            return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt)
                && !(instance.IsExpectingInputNow(InputAction_RegPress) || instance.IsExpectingInputNow(InputAction_AltPress));
        }

        public static PlayerInput.InputAction InputAction_RegPress =
            new("PcoNailRegStart", new int[] { IAPressCat, IAPressCat, IAPressCat },
            IA_PadBasicPress, IA_TouchRegularPress, IA_BatonBasicPress);
        public static PlayerInput.InputAction InputAction_AltPress =
            new("PcoNailAltStart", new int[] { IAAltDownCat, IAAltDownCat, IAAltDownCat },
            IA_PadAltPress, IA_TouchAltPress, IA_BatonAltPress);
        public static PlayerInput.InputAction InputAction_SweetsHit =
            new("PcoNailSweetsHit", new int[] { IASweetsCat, IASweetsCat, IASweetsCat },
            IA_PadSweetsCheck, IA_TouchSweetsCheck, IA_BatonSweetsCheck);

        public static NailCarpenter instance;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }

        List<ScheduledPattern> scheduledPatterns = new List<ScheduledPattern>();
        double patternStartBeat, gameStartBeat;
        PatternType patternType, lastPatternType;
        int patternIndex, lastPatternIndex = -1;

        double slideBeat = double.MaxValue;
        double slideLength;
        double cachedPatternLengthPudding, cachedPatternLengthCherry, cachedPatternLengthCake, cachedPatternLengthCakeLong;
        Util.EasingFunction.Ease slideEase;
        float slideRatioLast = 0, slideRatioNext = 0;

        void Start()
        {
            if (!conductor.isPlaying) return;
            UpdatePatterns();
        }

        public override void OnBeatPulse(double beat)
        {
            if (!IsExpectingInputNow(InputAction_AltPress) && UnityEngine.Random.value < 0.1f)
            {
                Carpenter.Play("eyeBlinkFast", 1, 0);
            }
        }

        void Update()
        {
            var currentBeat = conductor.songPositionInBeatsAsDouble;

            if (!conductor.isPlaying) return;

            if (PlayerInput.GetIsAction(InputAction_RegPress) && !IsExpectingInputNow(InputAction_RegPress))
            {
                ScoreMiss();
                SoundByte.PlayOneShot("miss");
                Carpenter.DoScaledAnimationAsync("carpenterHit", 0.25f);
                hasSlurped = false;
            }
            if (PlayerInput.GetIsAction(InputAction_AltPress) && !IsExpectingInputNow(InputAction_AltPress))
            {
                ScoreMiss();
                SoundByte.PlayOneShot("miss");
                Carpenter.DoScaledAnimationAsync("carpenterHit", 0.25f);
                hasSlurped = false;
            }

            // Board scroll.
            var boardPos = boardTrans.localPosition;
            var newBoardX = currentBeat * scrollMetresPerBeat;
            newBoardX %= boardWidth;
            boardTrans.localPosition = new Vector3((float)newBoardX, boardPos.y, boardPos.z);

            UpdatePatterns();
            UpdateShoji(currentBeat);
        }

        public override void OnGameSwitch(double beat)
        {
            cachedPatternLengthPudding = puddingPattern[^1].beat;
            cachedPatternLengthCherry = cherryPattern[^1].beat;
            cachedPatternLengthCake = cakePattern[^1].beat;
            cachedPatternLengthCakeLong = cakeLongPattern[^1].beat;

            double endBeat = double.MaxValue;
            var entities = gameManager.Beatmap.Entities;

            gameStartBeat = beat;
            patternStartBeat = gameStartBeat;
            // find out when the next game switch (or remix end) happens
            RiqEntity firstEnd = entities.Find(c => (c.datamodel.StartsWith("gameManager/switchGame") || c.datamodel.Equals("gameManager/end")) && c.beat > gameStartBeat);
            endBeat = firstEnd?.beat ?? endBeat;

            List<RiqEntity> events = entities.FindAll(v => (v.datamodel is "nailCarpenter/puddingNail" or "nailCarpenter/cherryNail" or "nailCarpenter/cakeNail" or "nailCarpenter/cakeLongNail") && v.beat >= gameStartBeat && v.beat < endBeat);
            scheduledPatterns.Clear();
            patternIndex = 0;
            foreach (var evt in events)
            {
                if (evt.length == 0) continue;
                int patternDivisions = (int)Math.Ceiling(evt.length / PATTERN_SEEK_TIME);
                PatternType patternType = evt.datamodel switch
                {
                    "nailCarpenter/puddingNail" => PatternType.Pudding,
                    "nailCarpenter/cherryNail" => PatternType.Cherry,
                    "nailCarpenter/cakeNail" => PatternType.Cake,
                    "nailCarpenter/cakeLongNail" => PatternType.CakeLong,
                    _ => throw new NotImplementedException()
                };
                for (int i = 0; i < patternDivisions; i++)
                {
                    var pattern = new ScheduledPattern
                    {
                        beat = evt.beat + (PATTERN_SEEK_TIME * i),
                        length = Math.Min(evt.length - (PATTERN_SEEK_TIME * i), PATTERN_SEEK_TIME),
                        type = patternType
                    };
                    scheduledPatterns.Add(pattern);
                }
            }
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        void UpdatePatterns()
        {
            double beat = conductor.songPositionInBeatsAsDouble;
            while (patternStartBeat < beat + PATTERN_SEEK_TIME)
            {
                if (patternIndex < scheduledPatterns.Count)
                {
                    var pattern = scheduledPatterns[patternIndex];
                    if (pattern.type == PatternType.None)
                    {
                        patternIndex++;
                        continue;
                    }
                    if (pattern.beat + pattern.length < patternStartBeat)
                    {
                        patternIndex++;
                        continue;
                    }
                    SpawnPattern(pattern.beat, pattern.length, pattern.type);
                    patternStartBeat = pattern.beat + pattern.length;
                    lastPatternIndex = patternIndex;
                    patternIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        public void SlideShoji(double beat, double length, float fillRatio, int ease, bool mute)
        {
            if (!mute) SoundByte.PlayOneShotGame("nailCarpenter/open", beat, forcePlay: true);
            slideBeat = beat;
            slideLength = length;
            slideEase = (Util.EasingFunction.Ease)ease;
            slideRatioLast = slideRatioNext;
            slideRatioNext = fillRatio;
        }

        void UpdateShoji(double beat)
        {
            if (beat >= slideBeat)
            {
                float slideLast = 17.8f * (1 - slideRatioLast);
                float slideNext = 17.8f * (1 - slideRatioNext);
                Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction(slideEase);
                float slideProg = Conductor.instance.GetPositionFromBeat(slideBeat, slideLength, true);
                slideProg = Mathf.Clamp01(slideProg);
                float slide = func(slideLast, slideNext, slideProg);
                shojiTrans.localPosition = new Vector3(slide, 0, 0);
            }
        }

        private void SpawnPattern(double beat, double length, PatternType pattern)
        {
            if (pattern == PatternType.None) return;
            double patternLength = pattern switch
            {
                PatternType.Pudding => cachedPatternLengthPudding,
                PatternType.Cherry => cachedPatternLengthCherry,
                PatternType.Cake => cachedPatternLengthCake,
                PatternType.CakeLong => cachedPatternLengthCakeLong,
                _ => throw new NotImplementedException()
            };
            patternType = pattern;
            int patternIterations = (int)Math.Ceiling(length / patternLength);
            for (int i = 0; i < patternIterations; i++)
            {
                SpawnPatternSegment(beat + (patternLength * i), gameStartBeat, pattern switch
                {
                    PatternType.Pudding => puddingPattern,
                    PatternType.Cherry => cherryPattern,
                    PatternType.Cake => cakePattern,
                    PatternType.CakeLong => cakeLongPattern,
                    _ => throw new NotImplementedException()
                });
                lastPatternType = patternType;
            }
        }

        private void SpawnPatternSegment(double beat, double startbeat, ObjectPatternItem[] pattern)
        {
            foreach (var item in pattern)
            {
                double itemBeat = beat + item.beat;
                switch (item.type)
                {
                    case ObjectType.LongCharge:
                        SoundByte.PlayOneShotGame("nailCarpenter/signal2", itemBeat, forcePlay: true);
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(itemBeat, delegate
                            {
                                Carpenter.DoScaledAnimationAsync("carpenterArmUp", 0.25f);
                            }),
                        });
                        break;
                    case ObjectType.Nail:
                        SpawnNail(itemBeat, startbeat);
                        break;
                    case ObjectType.LongNail:
                        SpawnLongNail(itemBeat, startbeat);
                        break;
                    case ObjectType.Sweet:
                        // dynamically determine sweet based on pattern and last pattern
                        Sweet.sweetsType sweetType = Sweet.sweetsType.None;
                        switch (patternType)
                        {
                            case PatternType.Pudding:
                                SoundByte.PlayOneShotGame("nailCarpenter/one", itemBeat, forcePlay: true);
                                sweetType = Sweet.sweetsType.Pudding;
                                break;
                            case PatternType.Cherry:
                                SoundByte.PlayOneShotGame("nailCarpenter/three", itemBeat, forcePlay: true);
                                sweetType = Sweet.sweetsType.CherryPudding;
                                break;
                            case PatternType.Cake:
                                SoundByte.PlayOneShotGame("nailCarpenter/alarm", itemBeat, forcePlay: true);
                                sweetType = Sweet.sweetsType.ShortCake;
                                BeatAction.New(instance, new List<BeatAction.Action>()
                                {
                                    new BeatAction.Action(itemBeat, delegate
                                    {
                                        EffectExclamRed.DoScaledAnimationAsync("exclamAppear", 0.25f);
                                    })
                                });
                                break;
                            case PatternType.CakeLong:
                                SoundByte.PlayOneShotGame("nailCarpenter/signal1", itemBeat, forcePlay: true);
                                sweetType = Sweet.sweetsType.LayerCake;
                                BeatAction.New(instance, new List<BeatAction.Action>()
                                {
                                    new BeatAction.Action(itemBeat, delegate
                                    {
                                        EffectExclamBlue.DoScaledAnimationAsync("exclamAppear", 0.25f);
                                    }),
                                });
                                break;
                            default:
                                break;
                        }
                        if (lastPatternType == PatternType.Cake)
                        {
                            SpawnSweet(itemBeat, startbeat, Sweet.sweetsType.Cherry);
                        }
                        else if (sweetType != Sweet.sweetsType.None)
                        {
                            SpawnSweet(itemBeat, startbeat, sweetType);
                        }
                        break;
                    case ObjectType.ForceCherry:
                        SpawnSweet(itemBeat, startbeat, Sweet.sweetsType.Cherry);
                        break;
                    case ObjectType.ForcePudding:
                        SoundByte.PlayOneShotGame("nailCarpenter/one", itemBeat);
                        SpawnSweet(itemBeat, startbeat, Sweet.sweetsType.Pudding);
                        break;
                    case ObjectType.ForceCherryPudding:
                        SoundByte.PlayOneShotGame("nailCarpenter/three", itemBeat, forcePlay: true);
                        SpawnSweet(itemBeat, startbeat, Sweet.sweetsType.CherryPudding);
                        break;
                    case ObjectType.ForceShortCake:
                        SoundByte.PlayOneShotGame("nailCarpenter/alarm", itemBeat, forcePlay: true);
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(itemBeat, delegate
                            {
                                EffectExclamRed.DoScaledAnimationAsync("exclamAppear", 0.25f);
                            })
                        });
                        SpawnSweet(itemBeat, startbeat, Sweet.sweetsType.ShortCake);
                        break;
                    case ObjectType.ForceLayerCake:
                        SoundByte.PlayOneShotGame("nailCarpenter/signal1", itemBeat, forcePlay: true);
                        BeatAction.New(instance, new List<BeatAction.Action>()
                        {
                            new BeatAction.Action(itemBeat, delegate
                            {
                                EffectExclamBlue.DoScaledAnimationAsync("exclamAppear", 0.25f);
                            }),
                        });
                        SpawnSweet(itemBeat, startbeat, Sweet.sweetsType.LayerCake);
                        break;
                    default:
                        break;
                }
            }
        }

        private void SpawnNail(double beat, double startBeat)
        {
            var newNail = Instantiate(baseNail, nailHolder).GetComponent<Nail>();

            newNail.targetBeat = beat;
            newNail.targetX = nailHolder.position.x;
            newNail.metresPerSecond = scrollMetresPerBeat;

            newNail.Init();
            newNail.gameObject.SetActive(true);
        }
        private void SpawnLongNail(double beat, double startBeat)
        {
            var newNail = Instantiate(baseLongNail, nailHolder).GetComponent<LongNail>();

            newNail.targetBeat = beat;
            newNail.targetX = nailHolder.position.x;
            newNail.metresPerSecond = scrollMetresPerBeat;

            newNail.Init();
            newNail.gameObject.SetActive(true);
        }
        private void SpawnSweet(double beat, double startBeat, Sweet.sweetsType sweetType)
        {
            var newSweet = Instantiate(baseSweet, nailHolder).GetComponent<Sweet>();

            newSweet.targetBeat = beat;
            newSweet.sweetType = sweetType;
            newSweet.targetX = nailHolder.position.x;
            newSweet.metresPerSecond = scrollMetresPerBeat;

            newSweet.gameObject.SetActive(true);
            newSweet.Init();
        }
    }
}