using HeavenStudio.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_FlipperFlop
{
    public class FlipperFlopFlipper : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] Animator faceAnim;
        [SerializeField] GameObject leftImpact;
        [SerializeField] GameObject rightImpact;
        public bool player;
        public bool left;
        bool up;
        bool canBlink = true;
        private FlipperFlop game;

        private void Awake()
        {
            faceAnim.Play("FaceNormal", 0, 0);
            game = FlipperFlop.instance;
        }

        private void Update()
        {
            if (UnityEngine.Random.Range(1, 600) == 1 && canBlink && !isPlaying(faceAnim, "FlipperBlink"))
            {
                faceAnim.DoScaledAnimationAsync("FlipperBlink", 0.5f);
            }
        }

        public void Impact(bool enableRight)
        {
            if (enableRight)
            {
                rightImpact.SetActive(true);
            }
            else
            {
                leftImpact.SetActive(true);
            }
            faceAnim.Play("FaceAngry", 0, 0);
            BeatAction.New(game.gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 0.1f, delegate { leftImpact.SetActive(false); rightImpact.SetActive(false); }),
                new BeatAction.Action(Conductor.instance.songPositionInBeats + 0.3f, delegate { faceAnim.Play("FaceAnnoyed", 0, 0); })
            });
        }

        public void PrepareFlip()
        {
            anim.DoScaledAnimationAsync("PrepareFlop", 0.5f);
        }

        public void Flip(bool roll, bool hit, bool barely = false, bool dontSwitch = false)
        {
            if (roll)
            {
                if (player && hit && !barely)
                {
                    Jukebox.PlayOneShotGame("flipperFlop/roll" + (left ? "L" : "R"));
                    faceAnim.Play("FaceNormal", 0, 0);
                    canBlink = true;
                }
                else if (player && barely && hit)
                {
                    Jukebox.PlayOneShotGame("flipperFlop/tink");
                    faceAnim.Play("FaceBarely", 0, 0);
                    canBlink = false;
                }
                else if (player && !hit)
                {
                    faceAnim.Play("FaceOw");
                    canBlink = false;
                    Jukebox.PlayOneShotGame("flipperFlop/failgroan");
                    game.BumpIntoOtherSeal(!left);
                    BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(Conductor.instance.songPositionInBeats + 0.3f, delegate { faceAnim.Play("FaceGoofy"); }),
                    });
                }

                up = !up;
            }
            else
            {
                if (player && hit)
                {
                    if (up && !barely)
                    {
                        faceAnim.Play("FaceNormal", 0, 0);
                        Jukebox.PlayOneShotGame($"flipperFlop/flipB{UnityEngine.Random.Range(1, 3)}");
                        canBlink = true;
                    }
                    else if (!barely)
                    {
                        Jukebox.PlayOneShotGame($"flipperFlop/flip{UnityEngine.Random.Range(1, 3)}");
                        faceAnim.Play("FaceNormal", 0, 0);
                        canBlink = true;
                    }
                    else
                    {
                        Jukebox.PlayOneShotGame("flipperFlop/tink");
                        faceAnim.Play("FaceBarely", 0, 0);
                        canBlink = false;
                    }
                }
                else if (player)
                {
                    faceAnim.Play("FaceOw");
                    canBlink = false;
                    string shouldReverse = up ? "Reverse" : "";
                    string leftOrRight = left ? "Left" : "Right";

                    Jukebox.PlayOneShotGame("flipperFlop/failgroan", -1, 1, 0.5f);
                    Jukebox.PlayOneShotGame("flipperFlop/punch", -1, 1, 0.5f);
                    anim.DoScaledAnimationAsync(shouldReverse + "MissFlop" + leftOrRight, 0.5f);
                    game.BumpIntoOtherSeal(!left);
                    BeatAction.New(this.gameObject, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(Conductor.instance.songPositionInBeats + 0.3f, delegate { faceAnim.Play("FaceGoofy"); }),
                    });
                }
            }
            if (hit || barely || roll)
            {
                string shouldReverse = up ? "Reverse" : "";
                if (roll) shouldReverse = !up ? "Reverse" : "";
                string leftOrRight = left ? "Left" : "Right";
                string rollOrFlop = roll ? "Roll" : "Flop";

                anim.DoScaledAnimationAsync(shouldReverse + rollOrFlop + leftOrRight, roll ? 0.8f : 0.5f);
            }

            if (!dontSwitch) left = !left;
        } 

        public void Bop()
        {
            anim.DoScaledAnimationAsync("FlipperBop", 0.5f);
            faceAnim.Play("FaceNormal", 0, 0);
            canBlink = true;
        }

        bool isPlaying(Animator anim, string stateName)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                    anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                return true;
            else
                return false;
        }
    }
}


