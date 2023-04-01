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
            return new Minigame("forkLifter", "Fork to Eat", "f1f1f1", false, false, new List<GameAction>()
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
            });
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
            GameObject fo = Instantiate(flickedObject);
            fo.transform.parent = flickedObject.transform.parent;
            Pea pea = fo.GetComponent<Pea>();
            pea.startBeat = beat;
            pea.type = type;
            fo.SetActive(true);
        }
    }

}