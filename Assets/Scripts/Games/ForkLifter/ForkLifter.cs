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

        private ColorEase bgColorEase = new(Color.white);
        private ColorEase gradColorEase = new(Color.white);

        //call this in update
        private void BackgroundColorUpdate()
        {
            bg.color =
            viewerCircle.color =
            handShadow.color = bgColorEase.GetColor();

            bgGradient.color =
            playerShadow.color = gradColorEase.GetColor();
        }

        public void BackgroundColor(double beat, float length, Color startColor, Color endColor, int ease)
        {
            bgColorEase = new ColorEase(beat, length, startColor, endColor, ease);
        }

        public void BackgroundColorGrad(double beat, float length, Color startColor, Color endColor, int ease)
        {
            gradColorEase = new ColorEase(beat, length, startColor, endColor, ease);
        }

        //call this in OnPlay(double beat) and OnGameSwitch(double beat)
        private void PersistColor(double beat)
        {
            var allEventsBeforeBeat = EventCaller.GetAllInGameManagerList("forkLifter", new string[] { "color" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeat.Count > 0)
            {
                allEventsBeforeBeat.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEvent = allEventsBeforeBeat[^1];
                BackgroundColor(lastEvent.beat, lastEvent.length, lastEvent["start"], lastEvent["end"], lastEvent["ease"]);
            }

            var allEventsBeforeBeatGrad = EventCaller.GetAllInGameManagerList("forkLifter", new string[] { "colorGrad" }).FindAll(x => x.beat < beat);
            if (allEventsBeforeBeatGrad.Count > 0)
            {
                allEventsBeforeBeatGrad.Sort((x, y) => x.beat.CompareTo(y.beat)); //just in case
                var lastEventGrad = allEventsBeforeBeatGrad[^1];
                BackgroundColorGrad(lastEventGrad.beat, lastEventGrad.length, lastEventGrad["start"], lastEventGrad["end"], lastEventGrad["ease"]);
            }
        }
    }
}