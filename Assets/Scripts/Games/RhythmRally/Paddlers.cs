using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_RhythmRally
{
    public class Paddlers : MonoBehaviour
    {
        private RhythmRally game;
        private Animator playerAnim;
        private Animator opponentAnim;
        private Conductor cond;

        public void Init()
        {
            game = RhythmRally.instance;
            playerAnim = game.playerAnim;
            opponentAnim = game.opponentAnim;
            cond = Conductor.instance;
        }

        void Update()
        {
            if (!game.served || game.missed || !game.started) return;

            if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN))
            {
                // Play "whoosh" sound here
                playerAnim.DoScaledAnimationAsync("Swing", 0.5f); ;
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

            playerAnim.DoScaledAnimationAsync("Swing", 0.5f); ;
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("rhythmRally/Return", hitBeat), new MultiSound.Sound("rhythmRally/ReturnBounce", bounceBeat) });
            BounceFX(bounceBeat);
            game.ball.SetActive(true);
        }

        void NearMiss(float state)
        {
            MissBall();
            SoundByte.PlayOneShot("miss");
            playerAnim.DoScaledAnimationAsync("Swing", 0.5f); ;

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
            if (state >= 1f || state <= -1f) {
                NearMiss(state);
                return; 
            }
            Ace();
        }

        public void Miss(PlayerActionEvent caller) 
        {
            MissBall();
        }

        public void Out(PlayerActionEvent caller) {}
    }
}
