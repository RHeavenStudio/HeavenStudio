using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Starpelly;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class AgbHairLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("rhythmTweezers", "Rhythm Tweezers", "a14fa1", false, false, new List<GameAction>()
            {
                new GameAction("start interval", "Start Interval")
                {
                    function = delegate { RhythmTweezers.instance.SetIntervalStart(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }, 
                    defaultLength = 4f, 
                    resizable = true,
                    priority = 1,
                    inactiveFunction = delegate { RhythmTweezers.InactiveInterval(eventCaller.currentEntity.beat, eventCaller.currentEntity.length); }
                },
                new GameAction("short hair", "Short Hair")
                {
                    inactiveFunction = delegate { RhythmTweezers.SpawnHairInactive(eventCaller.currentEntity.beat); },
                    function = delegate { RhythmTweezers.instance.SpawnHair(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f
                },
                new GameAction("long hair", "Curly Hair")
                {
                    inactiveFunction = delegate { RhythmTweezers.SpawnLongHairInactive(eventCaller.currentEntity.beat); },
                    function = delegate { RhythmTweezers.instance.SpawnLongHair(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f
                },
                new GameAction("passTurn", "Pass Turn")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.PassTurn(e.beat, e.length); },
                    resizable = true,
                    preFunction = delegate { var e = eventCaller.currentEntity; RhythmTweezers.PrePassTurn(e.beat, e.length); }
                },
                new GameAction("next vegetable", "Swap Vegetable")
                {
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity; 
                        if (!e["instant"]) RhythmTweezers.instance.NextVegetable(e.beat, e["type"], e["colorA"], e["colorB"]); 
                        else RhythmTweezers.instance.ChangeVegetableImmediate(e["type"], e["colorA"], e["colorB"]);
                    }, 
                    defaultLength = 0.5f, 
                    parameters = new List<Param>() 
                    {
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "The vegetable to switch to"),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Onion Color", "The color of the onion"),
                        new Param("colorB", RhythmTweezers.defaultPotatoColor, "Potato Color", "The color of the potato"),
                        new Param("instant", false, "Instant", "Instantly change vegetable?")
                    },
                    priority = 3
                },
                new GameAction("noPeek", "No Peeking Sign")
                {
                    preFunction = delegate { var e = eventCaller.currentEntity; RhythmTweezers.PreNoPeeking(e.beat, e.length, e["type"]); },
                    defaultLength = 4f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("type", RhythmTweezers.NoPeekSignType.Full, "Sign Type", "Which sign will be used?")
                    }
                },
                new GameAction("fade background color", "Background Fade")
                {
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity; 
                        if (e["instant"])
                        {
                            RhythmTweezers.instance.ChangeBackgroundColor(e["colorA"], 0f);
                        }
                        else
                        {
                            RhythmTweezers.instance.FadeBackgroundColor(e["colorA"], e["colorB"], e.length);
                        }
                    },
                    resizable = true, 
                    parameters = new List<Param>() 
                    {
                        new Param("colorA", Color.white, "Start Color", "The starting color in the fade"),
                        new Param("colorB", RhythmTweezers.defaultBgColor, "End Color", "The ending color in the fade"),
                        new Param("instant", false, "Instant", "Instantly change color to start color?")
                    } 
                },
                new GameAction("altSmile", "Use Alt Smile")
                {
                    function = delegate
                    {
                        RhythmTweezers.instance.VegetableAnimator.SetBool("UseAltSmile", !RhythmTweezers.instance.VegetableAnimator.GetBool("UseAltSmile"));
                    },
                    defaultLength = 0.5f
                },
                //backwards compatibility
                new GameAction("change vegetable", "Change Vegetable (Instant)")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.ChangeVegetableImmediate(e["type"], e["colorA"], e["colorB"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "The vegetable to switch to"),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Onion Color", "The color of the onion"),
                        new Param("colorB", RhythmTweezers.defaultPotatoColor, "Potato Color", "The color of the potato")
                    },
                    hidden = true,
                },
                new GameAction("set background color", "Background Colour")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.ChangeBackgroundColor(e["colorA"], 0f); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("colorA", RhythmTweezers.defaultBgColor, "Background Color", "The background color to change to")
                    },
                    hidden = true
                },
            },
            new List<string>() {"agb", "repeat"},
            "agbhair", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_RhythmTweezers;

    public class RhythmTweezers : Minigame
    {
        public enum VegetableType
        {
            Onion,
            Potato
        }

        public enum NoPeekSignType
        {
            Full,
            HalfRight,
            HalfLeft
        }

        private struct QueuedPeek
        {
            public float beat;
            public float length;
            public int type;
        }

        [Header("References")]
        public Transform VegetableHolder;
        public SpriteRenderer Vegetable;
        public SpriteRenderer VegetableDupe;
        public Animator VegetableAnimator;
        public SpriteRenderer bg;
        public Tweezers Tweezers;
        public GameObject hairBase;
        public GameObject longHairBase;
        public GameObject pluckedHairBase;
        [SerializeField] NoPeekingSign noPeekingRef;

        public GameObject HairsHolder;
        public GameObject DroppedHairsHolder;
        [NonSerialized] public int hairsLeft = 0;

        [Header("Variables")]
        private float passTurnBeat;
        private float passTurnEndBeat = 2;
        private static List<QueuedPeek> queuedPeeks = new List<QueuedPeek>();

        [Header("Sprites")]
        public Sprite pluckedHairSprite;
        public Sprite missedHairSprite;
        public Sprite onionSprite;
        public Sprite potatoSprite;

        [NonSerialized] public int eyeSize = 0;

        Tween transitionTween;
        bool transitioning = false;
        Tween bgColorTween;

        private static Color _defaultOnionColor;
        public static Color defaultOnionColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#C89600", out _defaultOnionColor);
                return _defaultOnionColor;
            }
        }

        private static Color _defaultPotatoColor;
        public static Color defaultPotatoColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FFDC00", out _defaultPotatoColor);
                return _defaultPotatoColor;
            }
        }

        private static Color _defaultBgColor;
        public static Color defaultBgColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#A14FA1", out _defaultBgColor);
                return _defaultBgColor;
            }
        }

        public static RhythmTweezers instance { get; set; }
        private static CallAndResponseHandler crHandlerInstance;

        private List<Hair> spawnedHairs = new List<Hair>();
        private List<LongHair> spawnedLongs = new List<LongHair>();

        private static List<float> passedTurns = new List<float>();

        private void Awake()
        {
            instance = this;
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(4);
            }
            if (crHandlerInstance.queuedEvents.Count > 0)
            {
                foreach (var crEvent in crHandlerInstance.queuedEvents)
                {
                    if (crEvent.tag == "Hair")
                    {
                        Hair hair = Instantiate(hairBase, HairsHolder.transform).GetComponent<Hair>();
                        spawnedHairs.Add(hair);
                        hair.gameObject.SetActive(true);
                        hair.GetComponent<Animator>().Play("SmallAppear", 0, 1);
                        float rot = -58f + 116 * Mathp.Normalize(crEvent.relativeBeat, 0, crHandlerInstance.intervalLength - 1);
                        hair.transform.eulerAngles = new Vector3(0, 0, rot);
                        hair.createBeat = crEvent.beat;
                    }
                    else if (crEvent.tag == "Long")
                    {
                        LongHair hair = Instantiate(longHairBase, HairsHolder.transform).GetComponent<LongHair>();
                        spawnedLongs.Add(hair);
                        hair.gameObject.SetActive(true);
                        hair.GetComponent<Animator>().Play("LongAppear", 0, 1);
                        float rot = -58f + 116 * Mathp.Normalize(crEvent.relativeBeat, 0, crHandlerInstance.intervalLength - 1);
                        hair.transform.eulerAngles = new Vector3(0, 0, rot);
                        hair.createBeat = crEvent.beat;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (crHandlerInstance != null && !Conductor.instance.isPlaying)
            {
                crHandlerInstance = null;
            }
        }

        public static void SpawnHairInactive(float beat)
        {
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(4);
            }
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
            crHandlerInstance.AddEvent(beat, 0, "Hair");
        }

        public static void SpawnLongHairInactive(float beat)
        {
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(4);
            }
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
            crHandlerInstance.AddEvent(beat, 0.5f, "Long");
        }

        public void SpawnHair(float beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
            // End transition early if the next hair is a lil early.
            StopTransitionIfActive();

            crHandlerInstance.AddEvent(beat, 0, "Hair");

            Jukebox.PlayOneShotGame("rhythmTweezers/shortAppear", beat);
            Hair hair = Instantiate(hairBase, HairsHolder.transform).GetComponent<Hair>();
            spawnedHairs.Add(hair);
            hair.gameObject.SetActive(true);
            hair.GetComponent<Animator>().Play("SmallAppear", 0, 0);

            float rot = -58f + 116 * crHandlerInstance.GetIntervalProgressFromBeat(beat, 1);
            hair.transform.eulerAngles = new Vector3(0, 0, rot);
            hair.createBeat = beat;
        }

        public void SpawnLongHair(float beat)
        {
            if (crHandlerInstance.queuedEvents.Count > 0 && crHandlerInstance.queuedEvents.Find(x => x.beat == beat || (beat >= x.beat && beat <= x.beat + x.length)) != null) return;
            StopTransitionIfActive();

            crHandlerInstance.AddEvent(beat, 0.5f, "Long");

            Jukebox.PlayOneShotGame("rhythmTweezers/longAppear", beat);
            LongHair hair = Instantiate(longHairBase, HairsHolder.transform).GetComponent<LongHair>();
            spawnedLongs.Add(hair);
            hair.gameObject.SetActive(true);
            hair.GetComponent<Animator>().Play("LongAppear", 0, 0);

            float rot = -58f + 116 * crHandlerInstance.GetIntervalProgressFromBeat(beat, 1);
            hair.transform.eulerAngles = new Vector3(0, 0, rot);
            hair.createBeat = beat;
        }

        public void SetIntervalStart(float beat, float interval = 4f)
        {
            StopTransitionIfActive();
            hairsLeft = 0;
            eyeSize = 0;
            crHandlerInstance.StartInterval(beat, interval);
        }

        public static void InactiveInterval(float beat, float interval)
        {
            if (crHandlerInstance == null)
            {
                crHandlerInstance = new CallAndResponseHandler(4);
            }
            crHandlerInstance.StartInterval(beat, interval);
        }

        public void PassTurn(float beat, float length)
        {
            if (crHandlerInstance.queuedEvents.Count > 0)
            {
                hairsLeft = crHandlerInstance.queuedEvents.Count;
                foreach (var crEvent in crHandlerInstance.queuedEvents)
                {
                    if (crEvent.tag == "Hair")
                    {
                        Hair hairToInput = spawnedHairs.Find(x => x.createBeat == crEvent.beat);
                        hairToInput.StartInput(beat + length, crEvent.relativeBeat);
                    }
                    else if (crEvent.tag == "Long")
                    {
                        LongHair hairToInput = spawnedLongs.Find(x => x.createBeat == crEvent.beat);
                        hairToInput.StartInput(beat + length, crEvent.relativeBeat);
                    }
                }
                crHandlerInstance.queuedEvents.Clear();
            }
        }

        public static void PrePassTurn(float beat, float length)
        {
            if (GameManager.instance.currentGame == "rhythmTweezers")
            {
                instance.SetPassTurnValues(beat + length);
            }
            else
            {
                passedTurns.Add(beat + length);
            }
        }

        private void SetPassTurnValues(float startBeat)
        {
            if (crHandlerInstance.intervalLength <= 0) return;
            passTurnBeat = startBeat - 1f;
            passTurnEndBeat = startBeat + crHandlerInstance.intervalLength;
        }

        const float vegDupeOffset = 16.7f;
        public void NextVegetable(float beat, int type, Color onionColor, Color potatoColor)
        {
            transitioning = true;

            Jukebox.PlayOneShotGame("rhythmTweezers/register", beat);

            Sprite nextVeggieSprite = type == 0 ? onionSprite : potatoSprite;
            Color nextColor = type == 0 ? onionColor : potatoColor;

            VegetableDupe.sprite = nextVeggieSprite;
            VegetableDupe.color = nextColor;

            // Move both vegetables to the left by vegDupeOffset, then reset their positions.
            // On position reset, reset state of core vegetable.
            transitionTween = VegetableHolder.DOLocalMoveX(-vegDupeOffset, Conductor.instance.secPerBeat * 0.5f / Conductor.instance.musicSource.pitch)
                .OnComplete(() => {

                var holderPos = VegetableHolder.localPosition;
                VegetableHolder.localPosition = new Vector3(0f, holderPos.y, holderPos.z);

                Vegetable.sprite = nextVeggieSprite;
                Vegetable.color = nextColor;

                ResetVegetable();
                transitioning = false;

            }).SetEase(Ease.InOutSine);
        }

        public void ChangeVegetableImmediate(int type, Color onionColor, Color potatoColor)
        {
            StopTransitionIfActive();
            
            Sprite newSprite = type == 0 ? onionSprite : potatoSprite;
            Color newColor = type == 0 ? onionColor : potatoColor;

            Vegetable.sprite = newSprite;
            Vegetable.color = newColor;
            VegetableDupe.sprite = newSprite;
            VegetableDupe.color = newColor;
        }

        public void ChangeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);

            if (seconds == 0)
            {
                bg.color = color;
            }
            else
            {
                bgColorTween = bg.DOColor(color, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats)
        {
            ChangeBackgroundColor(start, 0f);
            ChangeBackgroundColor(end, beats);
        }

        public static void PreNoPeeking(float beat, float length, int type)
        {
            if (GameManager.instance.currentGame == "rhythmTweezers")
            {
                instance.NoPeeking(beat, length, type);
            }
            else
            {
                queuedPeeks.Add(new QueuedPeek()
                {
                    beat = beat,
                    length = length,
                    type = type
                });
            }
        }

        public void NoPeeking(float beat, float length, int type)
        {
            NoPeekingSign spawnedNoPeekingSign = Instantiate(noPeekingRef, transform);
            spawnedNoPeekingSign.gameObject.SetActive(true);
            spawnedNoPeekingSign.Init(beat, length, type);
        }

        private void Update()
        {
            if (Conductor.instance.isPlaying && !Conductor.instance.isPaused)
            {
                if (passedTurns.Count > 0)
                {
                    foreach (var turn in passedTurns)
                    {
                        SetPassTurnValues(turn);
                    }
                    passedTurns.Clear();
                }
                if (queuedPeeks.Count > 0)
                {
                    foreach (var peek in queuedPeeks)
                    {
                        NoPeeking(peek.beat, peek.length, peek.type);
                    }
                    queuedPeeks.Clear();
                }
            }
        }

        private void LateUpdate()
        {
            // Set tweezer angle.
            var tweezerAngle = -180f;
            
            var tweezerTime = Conductor.instance.songPositionInBeats;
            var unclampedAngle = -58f + 116 * Mathp.Normalize(tweezerTime, passTurnBeat + 1f, passTurnEndBeat - 1f);
            tweezerAngle = Mathf.Clamp(unclampedAngle, -180f, 180f);

            Tweezers.transform.eulerAngles = new Vector3(0, 0, tweezerAngle);

            // Set tweezer to follow vegetable.
            var currentTweezerPos = Tweezers.transform.localPosition;
            var vegetablePos = Vegetable.transform.localPosition;
            var vegetableHolderPos = VegetableHolder.transform.localPosition;
            Tweezers.transform.localPosition = new Vector3(vegetableHolderPos.x, vegetablePos.y + 1f, currentTweezerPos.z);
        }

        private void ResetVegetable()
        {
            // If the tweezers happen to be holding a hair, drop it immediately so it can be destroyed below.
            Tweezers.DropHeldHair();

            foreach (Transform t in HairsHolder.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            foreach (Transform t in DroppedHairsHolder.transform)
            {
                GameObject.Destroy(t.gameObject);
            }

            VegetableAnimator.Play("Idle", 0, 0);

            eyeSize = 0;
        }

        private void StopTransitionIfActive()
        {
            if (transitioning)
            {
                if (transitionTween != null)
                {
                    transitionTween.Complete(true);
                    transitionTween.Kill();
                }
            }
        }
    }
}
