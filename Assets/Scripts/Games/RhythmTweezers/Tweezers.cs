using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_RhythmTweezers
{
    public class Tweezers : MonoBehaviour
    {
        public int hitOnFrame;
        [NonSerialized] public Animator anim;
        private Animator vegetableAnim;
        private RhythmTweezers game;
        private bool pluckingThisFrame;
        private bool holdingHair;
        public SpriteRenderer heldHairSprite;
        public Transform tweezerSpriteTrans;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            vegetableAnim = RhythmTweezers.instance.VegetableAnimator;

            game = RhythmTweezers.instance;
        }

        private void LateUpdate()
        {
            if (PlayerInput.Pressed(true))
            {
                if (!pluckingThisFrame) // Did you do a successful pluck earlier in the frame?
                {
                    DropHeldHair();
                    anim.Play("Tweezers_Pluck", 0, 0);
                }
            }

            pluckingThisFrame = false;
        }

        public void Pluck(bool ace, Hair hair)
        {
            DropHeldHair();

            if (ace)
            {
                SoundByte.PlayOneShotGame($"rhythmTweezers/shortPluck{UnityEngine.Random.Range(1, 21)}");

                hair.hairSprite.SetActive(false);
                hair.stubbleSprite.SetActive(true);

                game.hairsLeft--;
                game.eyeSize = Mathf.Clamp(game.eyeSize + 1, 0, 10);

                if (game.hairsLeft <= 0)
                    vegetableAnim.Play("HopFinal", 0, 0);
                else
                    vegetableAnim.Play("Hop" + game.eyeSize.ToString(), 0, 0);

                anim.Play("Tweezers_Pluck_Success", 0, 0);
            }
            else
            {
                SoundByte.PlayOneShotGame($"rhythmTweezers/shortPluck{UnityEngine.Random.Range(1, 21)}");
                SoundByte.PlayOneShot("miss");

                hair.hairSprite.SetActive(false);
                hair.missedSprite.SetActive(true);

                vegetableAnim.Play("Blink", 0, 0);

                anim.Play("Tweezers_Pluck_Fail", 0, 0);
            }

            pluckingThisFrame = true; // Prevents standard pluck from playing in LateUpdate().
            holdingHair = true;
        }

        public void LongPluck(bool ace, LongHair hair)
        {
            DropHeldHair();

            if (ace)
            {
                SoundByte.PlayOneShotGame("rhythmTweezers/longPullEnd");

                hair.hairSprite.SetActive(false);
                hair.stubbleSprite.SetActive(true);
                // Making transparent instead of disabling because animators are silly.
                hair.loop.GetComponent<SpriteRenderer>().color = Color.clear;

                game.hairsLeft--;
                game.eyeSize = Mathf.Clamp(game.eyeSize + 1, 0, 10);

                if (game.hairsLeft <= 0)
                    vegetableAnim.Play("HopFinal", 0, 0);
                else
                    vegetableAnim.Play("Hop" + game.eyeSize.ToString(), 0, 0);

                anim.Play("Tweezers_Pluck_Success", 0, 0);
            }

            pluckingThisFrame = true;
            holdingHair = true;
        }

        public void DropHeldHair()
        {
            if (!holdingHair) return;

            var droppedHair = GameObject.Instantiate(game.pluckedHairBase, game.DroppedHairsHolder.transform).GetComponent<SpriteRenderer>();
            droppedHair.gameObject.SetActive(true);

            droppedHair.transform.position = heldHairSprite.transform.position;
            droppedHair.transform.rotation = heldHairSprite.transform.rotation;

            droppedHair.sprite = heldHairSprite.sprite;

            // Make the hair spin.
            // (The prefab has a Rigidbody2D component already so that it falls)
            droppedHair.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
            droppedHair.GetComponent<Rigidbody2D>().angularVelocity = UnityEngine.Random.Range(-120f, 120f);

            holdingHair = false;
        }
    }
}