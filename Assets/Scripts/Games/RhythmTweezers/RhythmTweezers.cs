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
                    priority = 1
                },
                new GameAction("short hair", "Short Hair")
                {

                    function = delegate { RhythmTweezers.instance.SpawnHair(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f
                },
                new GameAction("long hair", "Curly Hair")
                {
                    function = delegate { RhythmTweezers.instance.SpawnLongHair(eventCaller.currentEntity.beat); }, 
                    defaultLength = 0.5f
                },
                new GameAction("next vegetable", "Swap Vegetable")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.NextVegetable(e.beat, e["type"], e["colorA"], e["colorB"]); }, 
                    defaultLength = 0.5f, 
                    parameters = new List<Param>() 
                    {
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "The vegetable to switch to"),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Onion Color", "The color of the onion"),
                        new Param("colorB", RhythmTweezers.defaultPotatoColor, "Potato Color", "The color of the potato")
                    },
                    priority = 3
                },
                new GameAction("change vegetable", "Change Vegetable (Instant)")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.ChangeVegetableImmediate(e["type"], e["colorA"], e["colorB"]); }, 
                    defaultLength = 0.5f, 
                    parameters = new List<Param>() 
                    {
                        new Param("type", RhythmTweezers.VegetableType.Onion, "Type", "The vegetable to switch to"),
                        new Param("colorA", RhythmTweezers.defaultOnionColor, "Onion Color", "The color of the onion"),
                        new Param("colorB", RhythmTweezers.defaultPotatoColor, "Potato Color", "The color of the potato")
                    }
                },
                new GameAction("set tweezer delay", "Offset Tweezer")
                {
                    function = delegate { RhythmTweezers.instance.tweezerBeatOffset = eventCaller.currentEntity.length; },
                    resizable = true,
                    priority = 2
                },
                new GameAction("reset tweezer delay", "Reset Tweezer Offset")
                {
                    function = delegate { RhythmTweezers.instance.tweezerBeatOffset = 0f; }, 
                    defaultLength = 0.5f,
                    priority = 2
                },
                new GameAction("set background color", "Background Colour")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.ChangeBackgroundColor(e["colorA"], 0f); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>() 
                    {
                        new Param("colorA", RhythmTweezers.defaultBgColor, "Background Color", "The background color to change to")
                    } 
                },
                new GameAction("fade background color", "Background Fade")
                {
                    function = delegate { var e = eventCaller.currentEntity; RhythmTweezers.instance.FadeBackgroundColor(e["colorA"], e["colorB"], e.length); },
                    resizable = true, 
                    parameters = new List<Param>() 
                    {
                        new Param("colorA", Color.white, "Start Color", "The starting color in the fade"),
                        new Param("colorB", RhythmTweezers.defaultBgColor, "End Color", "The ending color in the fade")
                    } 
                }
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

        public GameObject HairsHolder;
        public GameObject DroppedHairsHolder;
        [NonSerialized] public int hairsLeft = 0;

        [Header("Variables")]
        public float beatInterval = 4f;
        float intervalStartBeat;
        bool intervalStarted;
        public float tweezerBeatOffset = 0f;

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

        private void Awake()
        {
            instance = this;
        }

        public void SpawnHair(float beat)
        {
            // End transition early if the next hair is a lil early.
            StopTransitionIfActive();

            // If interval hasn't started, assume this is the first hair of the interval.
            if (!intervalStarted)
            {
                SetIntervalStart(beat, beatInterval);
            }

            Jukebox.PlayOneShotGame("rhythmTweezers/shortAppear", beat);
            Hair hair = Instantiate(hairBase, HairsHolder.transform).GetComponent<Hair>();
            hair.gameObject.SetActive(true);
            hair.GetComponent<Animator>().Play("SmallAppear", 0, 0);

            float rot = -58f + 116 * Mathp.Normalize(beat, intervalStartBeat, intervalStartBeat + beatInterval - 1f);
            hair.transform.eulerAngles = new Vector3(0, 0, rot);
            hair.createBeat = beat;
            hairsLeft++;
        }

        public void SpawnLongHair(float beat)
        {
            StopTransitionIfActive();

            if (!intervalStarted)
            {
                SetIntervalStart(beat, beatInterval);
            }

            Jukebox.PlayOneShotGame("rhythmTweezers/longAppear", beat);
            LongHair hair = Instantiate(longHairBase, HairsHolder.transform).GetComponent<LongHair>();
            hair.gameObject.SetActive(true);
            hair.GetComponent<Animator>().Play("LongAppear", 0, 0);

            float rot = -58f + 116 * Mathp.Normalize(beat, intervalStartBeat, intervalStartBeat + beatInterval - 1f);
            hair.transform.eulerAngles = new Vector3(0, 0, rot);
            hair.createBeat = beat;
            hairsLeft++;
        }

        public void SetIntervalStart(float beat, float interval = 4f)
        {
            // Don't do these things if the interval was already started.
            if (!intervalStarted)
            {
                // End transition early if the interval starts a lil early.
                StopTransitionIfActive();
                hairsLeft = 0;
                intervalStarted = true;
            }

            intervalStartBeat = beat;
            beatInterval = interval;
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
                intervalStarted = false;

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

        private void Update()
        {
            if (!Conductor.instance.isPlaying && !Conductor.instance.isPaused && intervalStarted)
            {
                StopTransitionIfActive();
                ResetVegetable();
                intervalStarted = false;
            }
        }

        private void LateUpdate()
        {
            // Set tweezer angle.
            var tweezerAngle = -180f;
            
            if (intervalStarted)
            {
                var tweezerTime = Conductor.instance.songPositionInBeats - beatInterval - tweezerBeatOffset;
                var unclampedAngle = -58f + 116 * Mathp.Normalize(tweezerTime, intervalStartBeat, intervalStartBeat + beatInterval - 1f);
                tweezerAngle = Mathf.Clamp(unclampedAngle, -180f, 180f);
            }

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
