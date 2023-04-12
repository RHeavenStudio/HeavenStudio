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
                    function = delegate { var e = eventCaller.currentEntity; SpaceSoccer.instance.NPCKickersEnterOrExit(e.beat, e.length, e["choice"], e["ease"], e["amount"], e["x"], e["y"], e["z"]); },
                    defaultLength = 4f,
                    parameters = new List<Param>()
                    {
                        new Param("choice", SpaceSoccer.AnimationToPlay.Enter, "Enter Or Exit", "Whether the kickers should exit or enter."),
                        new Param("ease", EasingFunction.Ease.Linear, "Ease", "The Ease of the entering or exiting."),
                        new Param("amount", new EntityTypes.Integer(2, 30, 5), "Amount", "Amount of Space Kickers."),
                        new Param("x", new EntityTypes.Float(-30, 30, 1.75f), "X Distance", "How much distance should there be between the space kickers on the x axis?"),
                        new Param("y", new EntityTypes.Float(-30, 30, 0.25f), "Y Distance", "How much distance should there be between the space kickers on the y axis?"),
                        new Param("z", new EntityTypes.Float(-30, 30, 0.75f), "Z Distance", "How much distance should there be between the space kickers on the z axis?"),
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
                    function = delegate 
                    { 
                        var e = eventCaller.currentEntity;
                        int choice;
                        if (e["toggle"])
                        {
                            choice = (int)SpaceSoccer.AnimationToPlay.Exit;
                        }
                        else
                        {
                            choice = (int)SpaceSoccer.AnimationToPlay.Enter;
                        }
                        SpaceSoccer.instance.NPCKickersEnterOrExit(e.beat, e.length, choice, (int)EasingFunction.Ease.Instant, 5, 1.75f, 0.25f, 0.75f);
                    },
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
        public enum AnimationToPlay
        {
            Enter = 0,
            Exit = 1
        }
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
        public bool ballDispensed;
        float lastDispensedBeat;
        float scrollBeat;
        float scrollOffsetX;
        float scrollOffsetY;
        float scrollLengthX = 22f;
        float scrollLengthY = 6f;
        Tween bgColorTween;
        Tween dotColorTween;

        public static SpaceSoccer instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            float normalizedX = cond.GetPositionFromBeat(scrollBeat, scrollLengthX);
            float normalizedY = cond.GetPositionFromBeat(scrollBeat, scrollLengthY);
            backgroundSprite.NormalizedX = -scrollOffsetX - normalizedX;
            backgroundSprite.NormalizedY = -scrollOffsetY - normalizedY;
        }

        public void NPCKickersEnterOrExit(float beat, float length, int animToPut, int easeToPut, int amount, float xDistance, float yDistance, float zDistance)
        {
            UpdateSpaceKickers(amount, xDistance, yDistance, zDistance);
            string animName = "Enter";
            switch (animToPut)
            {
                case (int)AnimationToPlay.Enter:
                    animName = "Enter";
                    break;
                case (int)AnimationToPlay.Exit:
                    animName = "Exit";
                    break;
            }
            foreach (var kicker in kickers)
            {
                if (kicker.player) continue;
                kicker.SetAnimParams(beat, length, animName, easeToPut);
            }
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
            scrollOffsetX = cond.GetPositionFromBeat(scrollBeat, scrollLengthX);
            scrollOffsetY = cond.GetPositionFromBeat(scrollBeat, scrollLengthY);
            scrollLengthX = scrollSpeedX;
            scrollLengthY = scrollSpeedY;
            scrollBeat = beat;
        }

        public void UpdateKickersPositions(float beat, float length, int ease, float xDistance, float yDistance, float zDistance)
        {
            for (int i = 1; i < kickers.Count; i++)
            {
                kickers[i].transform.parent.position = new Vector3(3.384f - xDistance * i, -yDistance * i, zDistance * i);
            }
        }

        public void UpdateSpaceKickers(int amount, float xDistance = 1.75f, float yDistance = 0.25f, float zDistance = 0.75f)
        {
            for (int i = kickers.Count - 1; i > 0; i--)
            {
                if (i >= amount)
                {
                    Kicker kickerToDestroy = kickers[i];
                    kickers.Remove(kickerToDestroy);
                    Destroy(kickerToDestroy.transform.parent.gameObject);
                }
            }
            UpdateKickersPositions(0, 0, (int)EasingFunction.Ease.Instant, xDistance, yDistance, zDistance);
            for (int i = kickers.Count; i < amount; i++)
            {
                Transform kickerHolder = Instantiate(kickerPrefab, transform).transform;
                kickerHolder.transform.position = new Vector3(kickerHolder.transform.position.x - xDistance * i, kickerHolder.transform.position.y - yDistance * i, kickerHolder.transform.position.z + zDistance * i);
                Kicker spawnedKicker = kickerHolder.GetChild(0).GetComponent<Kicker>();
                CircularMotion circularMotion = spawnedKicker.GetComponent<CircularMotion>();
                circularMotion.width = 0.85f - Mathf.Pow(amount * 1.5f, -1f);
                circularMotion.height = 0.5f - Mathf.Pow(amount * 1.5f, -1f);
                circularMotion.timeOffset = kickers[0].GetComponent<CircularMotion>().timeCounter;
                spawnedKicker.zValue = kickerHolder.transform.position.z;
                if (0 > zDistance)
                {
                    spawnedKicker.GetComponent<SortingGroup>().sortingOrder = i;
                }
                else
                {
                    spawnedKicker.GetComponent<SortingGroup>().sortingOrder = -i;
                }

                kickers.Add(spawnedKicker);
                kickerHolder.gameObject.SetActive(true);
            }
            if (ballDispensed) Dispense(lastDispensedBeat, false, true);
        }

        public void Dispense(float beat, bool playSound = true, bool ignorePlayer = false)
        {
            if (!ballDispensed) lastDispensedBeat = beat;
            ballDispensed = true;
            for (int i = 0; i < kickers.Count; i++)
            {
                Kicker kicker = kickers[i];
                if (i == 0) kicker.player = true;

                if (kicker.ball != null || (ignorePlayer && i == 0)) continue;

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