using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;
using HeavenStudio.Games.Scripts_ForkLifter;

namespace HeavenStudio.Games.Loaders
{
    using static Minigames;
    public static class RvlForkLoader
    {
        public static Minigame AddGame(EventCaller eventCaller) {
            return new Minigame("forkLifter", "Fork Lifter", "f1f1f1", false, false, new List<GameAction>()
            {
                new GameAction("flick", "Flick Food")
                {
                    inactiveFunction = delegate {
                        var e = eventCaller.currentEntity;
                        ForkLifter.Flick(e.beat);
                    },
                    function = delegate {
                        var e = eventCaller.currentEntity;
                        ForkLifter.Flick(e.beat);
                        ForkLifter.instance.FlickActive(e.beat, e["type"]);
                    },
                    defaultLength = 3,
                    parameters = new List<Param>()
                    {
                        new Param("type", ForkLifter.FlickType.Pea, "Object", "Choose the object to be flicked.")
                    },
                },
                new GameAction("prepare", "Prepare Hand")
                {
                    function = delegate { ForkLifter.instance.ForkLifterHand.Prepare(eventCaller.currentEntity["mute"]); }, 
                    defaultLength = 0.5f,
                    parameters = new List<Param>()
                    {
                        new Param("mute", false, "Mute", "Toggle if the prepare sound effect should play.")
                    }
                },
                new GameAction("gulp", "Swallow")
                {
                    function = delegate { ForkLifter.playerInstance.Eat(eventCaller.currentEntity["sfx"]); },
                    parameters = new List<Param>()
                    {
                        new Param("sfx", ForkLifterPlayer.EatType.Default, "SFX", "Choose the SFX to play.")
                    }
                },
                new GameAction("sigh", "Sigh")
                {
                    function = delegate { SoundByte.PlayOneShot("games/forkLifter/sigh"); }
                },
                new GameAction("color", "Background Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity; ForkLifter.instance.BackgroundColor(e.beat, e.length, e["start"], e["end"], e["ease"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", Color.white, "Start Color", "Set the color at the start of the event."),
                        new Param("end", Color.white, "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                },
                new GameAction("colorGrad", "Gradient Appearance")
                {
                    function = delegate { var e = eventCaller.currentEntity; ForkLifter.instance.BackgroundColorGrad(e.beat, e.length, e["start"], e["end"], e["ease"]); },
                    resizable = true,
                    parameters = new List<Param>()
                    {
                        new Param("start", Color.white, "Start Color", "Set the color at the start of the event."),
                        new Param("end", Color.white, "End Color", "Set the color at the end of the event."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease", "Set the easing of the action.")
                    },
                },
            },
            new List<string>() {"rvl", "normal"},
            "rvlfork", "en",
            new List<string>() {}
            );
        }
    }
}

namespace HeavenStudio.Games
{
    using Jukebox;
    using Scripts_ForkLifter;

    public class ForkLifter : Minigame
    {
        public enum FlickType
        {
            Pea,
            TopBun,
            Burger,
            BottomBun
        }

        public static ForkLifter instance;
        public static ForkLifterPlayer playerInstance => ForkLifterPlayer.instance;

        [Header("References")]
        public ForkLifterHand ForkLifterHand;

        [Header("Objects")]
        public Animator handAnim;
        public GameObject flickedObject;
        public SpriteRenderer peaPreview;
        [SerializeField] SpriteRenderer bg;
        [SerializeField] SpriteRenderer bgGradient;
        [SerializeField] SpriteRenderer viewerCircle;
        [SerializeField] SpriteRenderer playerShadow;
        [SerializeField] SpriteRenderer handShadow;

        public Sprite[] peaSprites;
        public Sprite[] peaHitSprites;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            BackgroundColorUpdate();
        }

        public override void OnPlay(double beat)
        {
            OnGameSwitch(beat);
        }

        public override void OnGameSwitch(double beat)
        {
            var actions = GameManager.instance.Beatmap.Entities.FindAll(e => e.datamodel.Split('/')[0] == "forkLifter");

            var actionsBefore = actions.FindAll(e => e.beat < beat);

            var lastColor = actionsBefore.FindLast(e => e.datamodel == "forkLifter/color");
            if (lastColor != null) {
                BackgroundColor(lastColor.beat, lastColor.length, lastColor["start"], lastColor["end"], lastColor["ease"]);
            }

            var lastColorGrad = actionsBefore.FindLast(e => e.datamodel == "forkLifter/colorGrad");
            if (lastColorGrad != null) {
                BackgroundColorGrad(lastColorGrad.beat, lastColorGrad.length, lastColorGrad["start"], lastColorGrad["end"], lastColorGrad["ease"]);
            }

            var tempFlicks = actions.FindAll(e => e.datamodel == "forkLifter/flick");

            foreach (var e in tempFlicks.FindAll(e => e.beat < beat && e.beat + 2 > beat)) {
                FlickActive(e.beat, e["type"]);
            }

            ForkLifterHand.allFlickEntities = tempFlicks.FindAll(e => e.beat >= beat);
            ForkLifterHand.CheckNextFlick();
        }

        public static void Flick(double beat)
        {
            var offset = SoundByte.GetClipLengthGame("forkLifter/zoomFast") - 0.03;
            SoundByte.PlayOneShotGame("forkLifter/zoomFast", beat + 2, offset: offset, forcePlay: true);

            SoundByte.PlayOneShotGame("forkLifter/flick", forcePlay: true);
        }

        public void FlickActive(double beat, int type)
        {

            handAnim.DoScaledAnimationFromBeatAsync("Hand_Flick", 0.5f, beat);
            ForkLifterHand.currentFlickIndex++;
            GameObject fo = Instantiate(flickedObject);
            fo.transform.parent = flickedObject.transform.parent;
            Pea pea = fo.GetComponent<Pea>();
            pea.startBeat = beat;
            pea.type = type;
            fo.SetActive(true);
        }

        private double colorStartBeat = -1;
        private float colorLength = 0f;
        private Color colorStart = Color.white; //obviously put to the default color of the game
        private Color colorEnd = Color.white;
        private Util.EasingFunction.Ease colorEase; //putting Util in case this game is using jukebox

        private double colorStartBeatGrad = -1;
        private float colorLengthGrad = 0f;
        private Color colorStartGrad = Color.white; //obviously put to the default color of the game
        private Color colorEndGrad = Color.white;
        private Util.EasingFunction.Ease colorEaseGrad; //putting Util in case this game is using jukebox

        //call this in update
        private void BackgroundColorUpdate()
        {
            float normalizedBeat = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeat, colorLength));

            var func = Util.EasingFunction.GetEasingFunction(colorEase);

            float newR = func(colorStart.r, colorEnd.r, normalizedBeat);
            float newG = func(colorStart.g, colorEnd.g, normalizedBeat);
            float newB = func(colorStart.b, colorEnd.b, normalizedBeat);

            bg.color = new Color(newR, newG, newB);
            viewerCircle.color = new Color(newR, newG, newB);
            handShadow.color = new Color(newR, newG, newB);

            float normalizedBeatGrad = Mathf.Clamp01(Conductor.instance.GetPositionFromBeat(colorStartBeatGrad, colorLengthGrad));

            var funcGrad = Util.EasingFunction.GetEasingFunction(colorEaseGrad);

            float newRGrad = funcGrad(colorStartGrad.r, colorEndGrad.r, normalizedBeatGrad);
            float newGGrad = funcGrad(colorStartGrad.g, colorEndGrad.g, normalizedBeatGrad);
            float newBGrad = funcGrad(colorStartGrad.b, colorEndGrad.b, normalizedBeatGrad);

            bgGradient.color = new Color(newRGrad, newGGrad, newBGrad);
            playerShadow.color = new Color(newRGrad, newGGrad, newBGrad);
        }

        public void BackgroundColor(double beat, float length, Color colorStartSet, Color colorEndSet, int ease)
        {
            colorStartBeat = beat;
            colorLength = length;
            colorStart = colorStartSet;
            colorEnd = colorEndSet;
            colorEase = (Util.EasingFunction.Ease)ease;
        }

        public void BackgroundColorGrad(double beat, float length, Color colorStartSet, Color colorEndSet, int ease)
        {
            colorStartBeatGrad = beat;
            colorLengthGrad = length;
            colorStartGrad = colorStartSet;
            colorEndGrad = colorEndSet;
            colorEaseGrad = (Util.EasingFunction.Ease)ease;
        }
    }
}