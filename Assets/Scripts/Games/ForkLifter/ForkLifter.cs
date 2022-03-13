using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

using DG.Tweening;

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