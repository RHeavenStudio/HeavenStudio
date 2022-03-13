using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_RhythmTweezers
{
    public class LongHair : PlayerActionObject
    {
        public float createBeat;
        public GameObject hairSprite;
        public GameObject stubbleSprite;
        private RhythmTweezers game;
        private Tweezers tweezers;
        private Animator anim;
        private int pluckState = 0;

        public GameObject holder;
        public GameObject loop;

        private Sound pullSound;

        private void Awake()
        {
            game = RhythmTweezers.instance;
            anim = GetComponent<Animator>();
            tweezers = game.Tweezers;
        }

        private void Update()
        {
            float stateBeat;

            switch (pluckState)
            {
                // Able to be held.
                case 0:
                    stateBeat = Conductor.instance.GetPositionFromMargin(createBeat + game.tweezerBeatOffset + game.beatInterval, 1f);
                    StateCheck(stateBeat);

                    if (PlayerInput.Pressed(true))
                    {
                        if (state.perfect)
                        {
                            pullSound = Jukebox.PlayOneShotGame($"rhythmTweezers/longPull{UnityEngine.Random.Range(1, 5)}");
                            pluckState = 1;
                            ResetState();
                        }
                        else if (state.notPerfect())
                        {
                            // I don't know what happens if you mess up here.
                            pluckState = -1;
                        }
                    }
                    break;
                
                // In held state. Able to be released.
                case 1:
                    stateBeat = Conductor.instance.GetPositionFromMargin(createBeat + game.tweezerBeatOffset + game.beatInterval + 0.5f, 1f);
                    StateCheck(stateBeat);

                    if (PlayerInput.PressedUp(true))
                    {
                        // It's possible to release earlier than earlyTime,
                        // and the hair will automatically be released before lateTime,
                        // so standard state checking isn't applied here
                        // (though StateCheck is still used for autoplay).
                        if (stateBeat >= Minigame.perfectTime)
                        {
                            Ace();
                        }
                        else
                        {
                            var normalized = Conductor.instance.GetPositionFromBeat(createBeat + game.tweezerBeatOffset + game.beatInterval, 0.5f);
                            // Hair gets released early and returns whoops.
                            anim.Play("LoopPullReverse", 0, normalized);
                            tweezers.anim.Play("Idle", 0, 0);

                            if (pullSound != null)
                                pullSound.Stop();

                            pluckState = -1;
                        }
                    }
                    break;
                
                // Released or missed. Can't be held or released.
                default:
                    break;
            }

            if (pluckState == 1)
            {
                Vector3 tst = tweezers.tweezerSpriteTrans.position;
                var hairDirection = new Vector3(tst.x + 0.173f, tst.y) - holder.transform.position;
                holder.transform.rotation = Quaternion.FromToRotation(Vector3.down, hairDirection);

                float normalizedBeat = Conductor.instance.GetPositionFromBeat(createBeat + game.tweezerBeatOffset + game.beatInterval, 0.5f);
                anim.Play("LoopPull", 0, normalizedBeat);
                tweezers.anim.Play("Tweezers_LongPluck", 0, normalizedBeat);

                // Auto-release if holding at release time.
                if (normalizedBeat >= 1f)
                    Ace();
            }

            loop.transform.localScale = Vector2.one / holder.transform.localScale;
        }

        public void Ace()
        {
            tweezers.LongPluck(true, this);
            tweezers.hitOnFrame++;

            if (pullSound != null)
                pullSound.Stop();

            pluckState = -1;
        }

        public override void OnAce()
        {
            if (pluckState == 0)
            {
                pullSound = Jukebox.PlayOneShotGame($"rhythmTweezers/longPull{UnityEngine.Random.Range(1, 5)}");
                pluckState = 1;
                ResetState();
            }
            else if (pluckState == 1)
            {
                Ace();
            }
        }
    }
}