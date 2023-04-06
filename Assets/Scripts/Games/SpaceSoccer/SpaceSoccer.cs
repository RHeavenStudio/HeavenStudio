using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class NtrSoccerLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("spaceSoccer", "Space Soccer", "ff7d27", false, false, new List<GameAction>()
            {
                new GameAction("ball dispense", "Ball Dispense")
                {
                    function = delegate { SpaceSoccer.instance.Dispense(eventCaller.currentEntity.beat, !eventCaller.currentEntity["toggle"]); },
                    defaultLength = 2f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Disable Sound", "Disables the dispense sound")
                    },
                    inactiveFunction = delegate { if (!eventCaller.currentEntity["toggle"]) { SpaceSoccer.DispenseSound(eventCaller.currentEntity.beat); } }
                },
                new GameAction("high kick-toe!", "High Kick-Toe!")
                {
                    defaultLength = 3f,
                    parameters = new List<Param>()
                    {
                        new Param("swing", new EntityTypes.Float(0, 1, 0.5f), "Swing", "The amount of swing")
                    }
                },
                new GameAction("npc kickers enter or exit", "NPC Kickers Enter or Exit")
                {
                    function = delegate { var e = eventCaller.currentEntity; },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Should Exit?", "Whether the kickers should exit or enter.")
                    },
                    resizable = true
                },
                new GameAction("changeBG", "Change Background Color")
                {
                    function = delegate {var e = eventCaller.currentEntity; SpaceSoccer.instance.FadeBackgroundColor(e["start"], e["end"], e["startDots"], e["endDots"], e.length, e["toggle"]); },
                    defaultLength = 1f,
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", SpaceSoccer.defaultBGColor, "Start Color", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("end", SpaceSoccer.defaultBGColor, "End Color", "The end color for the fade."),
                        new Param("startDots", Color.white, "Start Color (Dots)", "The start color for the fade or the color that will be switched to if -instant- is ticked on."),
                        new Param("endDots", Color.white, "End Color (Dots)", "The end color for the fade."),
                        new Param("toggle", false, "Instant", "Should the background instantly change color?")
                    }
                },
                new GameAction("scroll", "Scrolling Background") 
                {
                    function = delegate { var e = eventCaller.currentEntity; SpaceSoccer.instance.UpdateScrollSpeed(e.beat, e["x"], e["y"]); },
                    defaultLength = 0.5f,
                    parameters = new List<Param>() {
                        new Param("x", new EntityTypes.Float(-100f, 100f, 22f), "Horizontal", "Horizontal Speed Multiplier."),
                        new Param("y", new EntityTypes.Float(-100f, 100f, 6f), "Vertical", "Vertical Speed Multiplier."),
                    }
                },
                // This is still here for "backwards-compatibility" but is hidden in the editor (it does absolutely nothing however)
                new GameAction("keep-up", "")
                {
                    defaultLength = 4f,
                    resizable = true,
                    hidden = true
                },
                new GameAction("npc kickers instant enter or exit", "NPC Kickers Instant Enter or Exit")
                {
                    function = delegate { var e = eventCaller.currentEntity; },
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("toggle", false, "Should Exit?", "Whether the kickers should be exited or entered.")
                    },
                    hidden = true
                },
            });
        }
    }
}

namespace HeavenStudio.Games
{
    using Scripts_SpaceSoccer;
    using HeavenStudio.Common;
    using UnityEngine.Rendering;

    public class SpaceSoccer : Minigame
    {
        private static Color _defaultBGColor;
        public static Color defaultBGColor
        {
            get
            {
                ColorUtility.TryParseHtmlString("#FF7D27", out _defaultBGColor);
                return _defaultBGColor;
            }
        }
        [Header("Components")]
        [SerializeField] private GameObject kickerPrefab;
        [SerializeField] private GameObject ballRef;
        [SerializeField] private List<Kicker> kickers;
        [SerializeField] private SuperScroll backgroundSprite;
        [SerializeField] private SpriteRenderer bg;

        [Header("Properties")]
        [SerializeField] private bool ballDispensed; //unused
        float scrollLengthX = 22f;
        float scrollLengthY = 6f;
        Tween bgColorTween;
        Tween dotColorTween;

        public static SpaceSoccer instance { get; private set; }

        private void Awake()
        {
            instance = this;
            UpdateSpaceKickers(5);
        }

        private void Update()
        {
            var cond = Conductor.instance;
            float normalizedX = cond.GetPositionFromBeat(0, scrollLengthX);
            float normalizedY = cond.GetPositionFromBeat(0, scrollLengthY);
            backgroundSprite.NormalizedX = -normalizedX;
            backgroundSprite.NormalizedY = -normalizedY;
        }

        public override void OnGameSwitch(float beat)
        {
            foreach(var entity in GameManager.instance.Beatmap.entities)
            {
                if(entity.beat > beat) //the list is sorted based on the beat of the entity, so this should work fine.
                {
                    break;
                }
                if(entity.datamodel != "spaceSoccer/ball dispense" || entity.beat + entity.length <= beat) //check for dispenses that happen right before the switch
                {
                    continue;
                }
                Dispense(entity.beat, false);
                break;
            }
        }

        public void UpdateScrollSpeed(float beat, float scrollSpeedX, float scrollSpeedY) 
        {
            var cond = Conductor.instance;
            scrollLengthX = scrollSpeedX;
            scrollLengthY = scrollSpeedY;
        }

        public void UpdateSpaceKickers(int amount, float xDistance = 1.75f, float yDistance = 0.25f, float zDistance = 0.75f)
        {
            foreach (var kicker in kickers)
            {
                if (!kicker.player) 
                {
                    Destroy(kicker.transform.parent.gameObject);
                } 
            }
            List<Kicker> kickersToPut = new List<Kicker>();
            kickersToPut.Add(kickers[0]);
            for (int i = 1; i < amount; i++)
            {
                Transform kickerHolder = Instantiate(kickerPrefab, transform).transform;
                kickerHolder.transform.position = new Vector3(kickerHolder.transform.position.x - xDistance * i, kickerHolder.transform.position.y - yDistance * i, kickerHolder.transform.position.z + zDistance * i);
                Kicker spawnedKicker = kickerHolder.GetChild(0).GetComponent<Kicker>();
                CircularMotion circularMotion = spawnedKicker.GetComponent<CircularMotion>();
                circularMotion.width = 0.85f - Mathf.Pow(amount * 1.5f, -1f);
                circularMotion.height = 0.5f - Mathf.Pow(amount * 1.5f, -1f);
                spawnedKicker.zValue = kickerHolder.transform.position.z;
                if (0 > zDistance)
                {
                    spawnedKicker.GetComponent<SortingGroup>().sortingOrder = i;
                }
                else
                {
                    spawnedKicker.GetComponent<SortingGroup>().sortingOrder = -i;
                }

                kickersToPut.Add(spawnedKicker);
                kickerHolder.gameObject.SetActive(true);
            }
            kickers = kickersToPut;
        }

        public void Dispense(float beat, bool playSound = true)
        {
            ballDispensed = true;
            for (int i = 0; i < kickers.Count; i++)
            {
                Kicker kicker = kickers[i];
                if (i == 0) kicker.player = true;

                if (kicker.ball != null) return;

                GameObject ball = Instantiate(ballRef, transform);
                ball.SetActive(true);
                Ball ball_ = ball.GetComponent<Ball>();
                ball_.Init(kicker, beat);
                if (kicker.player && playSound)
                {
                    DispenseSound(beat);
                }
                kicker.DispenseBall(beat);

                kicker.canKick = true;
            }
        }

        public static void DispenseSound(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[]
                {
                new MultiSound.Sound("spaceSoccer/dispenseNoise",   beat),
                new MultiSound.Sound("spaceSoccer/dispenseTumble1", beat),
                new MultiSound.Sound("spaceSoccer/dispenseTumble2", beat + 0.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble2B",beat + 0.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble3", beat + 0.75f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble4", beat + 1f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble5", beat + 1.25f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble6", beat + 1.5f),
                new MultiSound.Sound("spaceSoccer/dispenseTumble6B",beat + 1.75f),
                }, forcePlay:true);
        }

        public void ChangeBackgroundColor(Color color, Color dotColor, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);
            if (dotColorTween != null)
                dotColorTween.Kill(true);

            if (seconds == 0)
            {
                bg.color = color;
                backgroundSprite.Material.SetColor("_Color", dotColor);
            }
            else
            {
                bgColorTween = bg.DOColor(color, seconds);
                dotColorTween = backgroundSprite.Material.DOColor(dotColor, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, Color startDot, Color endDot, float beats, bool instant)
        {
            ChangeBackgroundColor(start, startDot, 0f);
            if (!instant) ChangeBackgroundColor(end, endDot, beats);
        }
    }

}