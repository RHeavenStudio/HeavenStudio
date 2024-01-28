using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

using HeavenStudio.Util;
using HeavenStudio.InputSystem;

namespace HeavenStudio.Games.Scripts_RhythmRally
{
    public class Paddlers : MonoBehaviour
    {
        private RhythmRally game;
        private Animator playerAnim;
        private Animator opponentAnim;
        private Conductor cond;

        public bool PlayerDown { get { return playerDown; } }
        bool playerDown = false;

        public void Init()
        {
            game = RhythmRally.instance;
            playerAnim = game.playerAnim;
            opponentAnim = game.opponentAnim;
            cond = Conductor.instance;
        }

        void Update()
        {
            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch && !GameManager.instance.autoplay)
            {
                if (PlayerInput.GetIsAction(RhythmRally.InputAction_BasicPress))
                {
                    playerAnim.Play("Ready1");
                    playerDown = true;
                }
                if (PlayerInput.GetIsAction(RhythmRally.InputAction_BasicRelease))
                {
                    playerAnim.Play("UnReady1");
                    playerDown = false;
                }
            }
            else
            {
                if (!game.started) return;
            }
            if (PlayerInput.GetIsAction(RhythmRally.InputAction_FlickPress) && !game.IsExpectingInputNow(RhythmRally.InputAction_FlickPress))
            {
                // Play "whoosh" sound here
                playerAnim.DoScaledAnimationAsync("Swing", 0.5f);
                playerDown = false;
            }
        }

        void Ace()
        {
            game.served = false;

            var hitBeat = cond.songPositionInBeatsAsDouble;

            var bounceBeat = game.serveBeat + game.targetBeat + 1f;

            if (game.rallySpeed == RhythmRally.RallySpeed.Slow)
            {
                bounceBeat = game.serveBeat + game.targetBeat + 2f;
            }
            else if (game.rallySpeed == RhythmRally.RallySpeed.SuperFast)
            {
                bounceBeat = game.serveBeat + game.targetBeat + 0.5f;
            }

            playerAnim.DoScaledAnimationAsync("Swing", 0.5f);
            playerDown = false;
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("rhythmRally/Return", hitBeat), new MultiSound.Sound("rhythmRally/ReturnBounce", bounceBeat) });
            BounceFX(bounceBeat);
            game.ball.SetActive(true);
        }

        void NearMiss(float state)
        {
            MissBall();
            SoundByte.PlayOneShot("miss");
            playerAnim.DoScaledAnimationAsync("Swing", 0.5f);
            playerDown = false;

            game.missCurve.KeyPoints[0].Position = game.ball.transform.position;
            game.missCurve.transform.localScale = new Vector3(-state, 1f, 1f);
            game.missBeat = cond.songPositionInBeatsAsDouble;
            game.ball.SetActive(true);
        }

        void MissBall()
        {
            game.served = false;
            game.missed = true;

            var whistleBeat = game.serveBeat + game.targetBeat + 1f;

            if (game.rallySpeed == RhythmRally.RallySpeed.Slow)
            {
                whistleBeat = game.serveBeat + game.targetBeat + 2f;
            }
            else if (game.rallySpeed == RhythmRally.RallySpeed.SuperFast)
            {
                whistleBeat = game.serveBeat + game.targetBeat + 0.5f;
            }

            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("rhythmRally/Whistle", whistleBeat) });
        }

        public void BounceFX(double bounceBeat)
        {
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(bounceBeat, delegate
                {
                    GameObject ballHitFX2 = Instantiate(RhythmRally.instance.ballHitFX.gameObject, RhythmRally.instance.gameObject.transform);
                    ballHitFX2.SetActive(true);
                    ballHitFX2.transform.position = new Vector3(ballHitFX2.transform.position.x, ballHitFX2.transform.position.y, RhythmRally.instance.ball.transform.position.z);
                    ballHitFX2.GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, 0), 0.65f).SetEase(Ease.OutExpo).OnComplete(delegate { Destroy(ballHitFX2); });
                })
            });
        }

        public void Just(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                NearMiss(state);
                return;
            }
            Ace();
        }

        public void Miss(PlayerActionEvent caller)
        {
            MissBall();
        }

        public void Out(PlayerActionEvent caller) { }
    }
}
