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
        private double passTurnBeat = -1;
        private double passTurnEndBeat = -1;
        [NonSerialized] public int hairsLeft;
        private int eyeSize = 0;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            vegetableAnim = RhythmTweezers.instance.VegetableAnimator;

            game = RhythmTweezers.instance;
        }

        public void Init(double beat, double endBeat)
        {
            passTurnBeat = beat;
            passTurnEndBeat = endBeat;
            Update();
        }

        private void Update()
        {
            if (passTurnBeat != -1)
            {
                // Set tweezer angle.
                float tweezerTime = Conductor.instance.GetPositionFromBeat(passTurnBeat, Math.Max(passTurnEndBeat - 1f - passTurnBeat, 1));
                var unclampedAngle = -58f + (116 * tweezerTime);
                var tweezerAngle = Mathf.Clamp(unclampedAngle, -180f, 180f);

                transform.eulerAngles = new Vector3(0, 0, tweezerAngle);

                // Set tweezer to follow vegetable.
                var currentTweezerPos = transform.localPosition;
                var vegetablePos = game.Vegetable.transform.localPosition;
                var vegetableHolderPos = game.VegetableHolder.transform.localPosition;
                transform.localPosition = new Vector3(vegetableHolderPos.x, vegetablePos.y + 1f, currentTweezerPos.z);

                if (tweezerAngle == 180)
                {
                    Destroy(gameObject);
                }
            }
            if (PlayerInput.GetIsAction(RhythmTweezers.InputAction_Press) && !game.IsExpectingInputNow(RhythmTweezers.InputAction_Press))
            {
                DropHeldHair();
                anim.Play("Tweezers_Pluck", 0, 0);
            }
        }

        private void LateUpdate()
        {
        }

        public void Pluck(bool ace, Hair hair)
        {
            DropHeldHair();
            if (hair == null) return;

            if (ace)
            {
                SoundByte.PlayOneShotGame($"rhythmTweezers/shortPluck{UnityEngine.Random.Range(1, 21)}");

                hair.hairSprite.SetActive(false);
                hair.stubbleSprite.SetActive(true);

                hairsLeft--;
                eyeSize = Mathf.Clamp(eyeSize + 1, 0, 10);

                if (hairsLeft <= 0)
                    vegetableAnim.Play("HopFinal", 0, 0);
                else
                    vegetableAnim.Play("Hop" + eyeSize.ToString(), 0, 0);

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

            // pluckingThisFrame = true; // Prevents standard pluck from playing in LateUpdate().
            holdingHair = true;
        }

        public void LongPluck(bool ace, LongHair hair)
        {
            DropHeldHair();
            if (hair == null) return;

            if (ace)
            {
                SoundByte.PlayOneShotGame("rhythmTweezers/longPullEnd");

                hair.hairSprite.SetActive(false);
                hair.stubbleSprite.SetActive(true);
                // Making transparent instead of disabling because animators are silly.
                hair.loop.GetComponent<SpriteRenderer>().color = Color.clear;

                hairsLeft--;
                eyeSize = Mathf.Clamp(eyeSize + 1, 0, 10);

                if (hairsLeft <= 0)
                    vegetableAnim.Play("HopFinal", 0, 0);
                else
                    vegetableAnim.Play("Hop" + eyeSize.ToString(), 0, 0);

                anim.Play("Tweezers_Pluck_Success", 0, 0);
            }

            // pluckingThisFrame = true;
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