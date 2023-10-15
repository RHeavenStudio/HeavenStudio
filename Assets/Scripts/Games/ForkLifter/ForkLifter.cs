using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

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
                    function = delegate { var e = eventCaller.currentEntity; ForkLifter.instance.Flick(e.beat, e["type"]); }, 
                    defaultLength = 3,
                    parameters = new List<Param>()
                    {
                        new Param("type", ForkLifter.FlickType.Pea, "Object", "The object to be flicked")
                    }
                },
                new GameAction("prepare", "Prepare Hand")
                {
                    function = delegate { ForkLifter.instance.ForkLifterHand.Prepare(); }, 
                    defaultLength = 0.5f
                },
                new GameAction("gulp", "Swallow")
                {
                    function = delegate { ForkLifter.playerInstance.Eat(); }
                },
                new GameAction("sigh", "Sigh")
                {

                    function = delegate { SoundByte.PlayOneShot("games/forkLifter/sigh"); }
                },
                new GameAction("color", "Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; ForkLifter.instance.BackgroundColor(e.beat, e.length, e["start"], e["end"], e["ease"]); },
                    parameters = new List<Param>()
                    {
                        new Param("start", Color.white, "Start Color", "The color to start fading from."),
                        new Param("end", Color.white, "End Color", "The color to end the fade."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
                    },
                    resizable = true
                },
                new GameAction("colorGrad", "Gradient Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; ForkLifter.instance.BackgroundColorGrad(e.beat, e.length, e["start"], e["end"], e["ease"]); },
                    parameters = new List<Param>()
                    {
                        new Param("start", Color.white, "Start Color", "The color to start fading from."),
                        new Param("end", Color.white, "End Color", "The color to end the fade."),
                        new Param("ease", Util.EasingFunction.Ease.Linear, "Ease")
                    },
                    resizable = true
                },
                new GameAction("bop", "Bop")
                {
                    function = delegate { var e = eventCaller.currentEntity; ForkLifter.instance.Bop(e.beat, e.length, e["bop"], e["autoBop"]); },
                    parameters = new List<Param>()
                    {
                        new Param("bop", true, "Keep Bopping", "Should Fork bop for the duration of the block?"),
                        new Param("autoBop", false, "Keep Bopping (Auto)", "Should Fork bop indefinitely?"),
                    },
                    resizable = true,
                },
                
                // These are still here for backwards-compatibility but are hidden in the editor
                new GameAction("pea", "")
                {
                    function = delegate { ForkLifter.instance.Flick(eventCaller.currentEntity.beat, 0); }, 
                    defaultLength = 3, 
                    hidden = true
                },
                new GameAction("topbun", "")
                {
                    function = delegate { ForkLifter.instance.Flick(eventCaller.currentEntity.beat, 1); }, 
                    defaultLength = 3, 
                    hidden = true
                },
                new GameAction("burger", "")
                {
                    function = delegate { ForkLifter.instance.Flick(eventCaller.currentEntity.beat, 2); }, 
                    defaultLength = 3, 
                    hidden = true
                },
                new GameAction("bottombun", "")
                {
                    function = delegate { ForkLifter.instance.Flick(eventCaller.currentEntity.beat, 3); }, 
                    defaultLength = 3, 
                    hidden = true
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

        public override void OnGameSwitch(double beat)
        {
            base.OnGameSwitch(beat);
            ForkLifterHand.CheckNextFlick();
            PersistColor(beat);
        }

        
        public void Bop(double beat, double length, bool doesBop, bool autoBop)
        {
            playerInstance.shouldBop = (autoBop || doesBop);
            if (!autoBop && doesBop) {
                BeatAction.New(gameObject, new List<BeatAction.Action>() {
                    new BeatAction.Action(beat + length, delegate {
                        playerInstance.shouldBop = false;
                    })
                });
            }
        }

        public void Flick(double beat, int type)
        {
            SoundByte.PlayOneShotGame("forkLifter/flick");
            handAnim.Play("Hand_Flick", 0, 0);
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

            float newRGrad = func(colorStartGrad.r, colorEndGrad.r, normalizedBeatGrad);
            float newGGrad = func(colorStartGrad.g, colorEndGrad.g, normalizedBeatGrad);
            float newBGrad = func(colorStartGrad.b, colorEndGrad.b, normalizedBeatGrad);

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

        public override void OnPlay(double beat)
        {
            PersistColor(beat);
        }
    }
}