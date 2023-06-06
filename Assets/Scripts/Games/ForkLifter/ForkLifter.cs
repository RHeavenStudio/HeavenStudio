using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

using DG.Tweening;

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

                    function = delegate { Jukebox.PlayOneShot("games/forkLifter/sigh"); }
                },
                new GameAction("color", "Background Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; ForkLifter.instance.FadeBackgroundColor(e["start"], e["end"], e.length, e["instant"]); },
                    parameters = new List<Param>()
                    {
                        new Param("start", Color.white, "Start Color", "The color to start fading from."),
                        new Param("end", Color.white, "End Color", "The color to end the fade."),
                        new Param("instant", false, "Instant", "If checked, the background color will instantly change to the start color.")
                    },
                    resizable = true
                },
                new GameAction("colorGrad", "Gradient Color")
                {
                    function = delegate { var e = eventCaller.currentEntity; ForkLifter.instance.FadeGradientColor(e["start"], e["end"], e.length, e["instant"]); },
                    parameters = new List<Param>()
                    {
                        new Param("start", Color.white, "Start Color", "The color to start fading from."),
                        new Param("end", Color.white, "End Color", "The color to end the fade."),
                        new Param("instant", false, "Instant", "If checked, the gradient color will instantly change to the start color.")
                    },
                    resizable = true
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
        Tween bgColorTween;
        Tween bgGradientColorTween;
        Tween viewerCircleColorTween;
        Tween playerShadowColorTween;
        Tween handShadowColorTween;

        public Sprite[] peaSprites;
        public Sprite[] peaHitSprites;

        private void Awake()
        {
            instance = this;
        }

        public override void OnGameSwitch(float beat)
        {
            base.OnGameSwitch(beat);
            ForkLifterHand.CheckNextFlick();
        }

        public void Flick(float beat, int type)
        {
            Jukebox.PlayOneShotGame("forkLifter/flick");
            handAnim.Play("Hand_Flick", 0, 0);
            ForkLifterHand.currentFlickIndex++;
            GameObject fo = Instantiate(flickedObject);
            fo.transform.parent = flickedObject.transform.parent;
            Pea pea = fo.GetComponent<Pea>();
            pea.startBeat = beat;
            pea.type = type;
            fo.SetActive(true);
        }

        public void ChangeBackgroundColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgColorTween != null)
                bgColorTween.Kill(true);
            if (viewerCircleColorTween != null)
                viewerCircleColorTween.Kill(true);
            if (handShadowColorTween != null) handShadowColorTween.Kill(true);

            if (seconds == 0)
            {
                bg.color = color;
                viewerCircle.color = color;
                handShadow.color = color;
            }
            else
            {
                bgColorTween = bg.DOColor(color, seconds);
                handShadowColorTween = handShadow.DOColor(color, seconds);
                viewerCircleColorTween = viewerCircle.DOColor(color, seconds);
            }
        }

        public void FadeBackgroundColor(Color start, Color end, float beats, bool instant)
        {
            ChangeBackgroundColor(start, 0f);
            if (!instant) ChangeBackgroundColor(end, beats);
        }

        public void ChangeGradientColor(Color color, float beats)
        {
            var seconds = Conductor.instance.secPerBeat * beats;

            if (bgGradientColorTween != null)
                bgGradientColorTween.Kill(true);
            if (playerShadowColorTween != null) playerShadowColorTween.Kill(true);

            if (seconds == 0)
            {
                bgGradient.color = color;
                playerShadow.color = color;
            }
            else
            {
                bgGradientColorTween = bgGradient.DOColor(color, seconds);
                playerShadowColorTween = playerShadow.DOColor(color, seconds);
            }
        }

        public void FadeGradientColor(Color start, Color end, float beats, bool instant)
        {
            ChangeGradientColor(start, 0f);
            if (!instant) ChangeGradientColor(end, beats);
        }
    }

}