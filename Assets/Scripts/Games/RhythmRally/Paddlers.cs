using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_RhythmRally
{
    public class Paddlers : PlayerActionObject
    {
        private RhythmRally game;
        private Animator playerAnim;
        private Animator opponentAnim;
        private Conductor cond;

        void Awake()
        {
            game = RhythmRally.instance;
            playerAnim = game.playerAnim;
            opponentAnim = game.opponentAnim;
            cond = Conductor.instance;
        }

        void Update()
        {
            if (!game.served || game.missed || !game.started) return;

            var cond = Conductor.instance;

            float stateBeat = cond.GetPositionFromMargin(game.targetBeat, 1f);
            StateCheck(stateBeat);

            if (PlayerInput.Pressed())
            {
                if (state.perfect)
                {
                    Ace();
                }
                else if (state.notPerfect())
                {
                    Miss();
                    Jukebox.PlayOneShot("miss");
                    playerAnim.Play("Swing", 0, 0);

                    game.missCurve.KeyPoints[0].Position = game.ball.transform.position;
                    game.missCurve.transform.localScale = new Vector3(state.early ? 1f : -1f, 1f, 1f);
                    game.missBeat = cond.songPositionInBeats;
                }
                else
                {
                    // Play "whoosh" sound here
                    playerAnim.Play("Swing", 0, 0);
                }
            }

            if (stateBeat > Minigame.EndTime())
            {
                Miss();
                game.ball.SetActive(false);
            }
        }

        void Ace()
        {
            game.served = false;

            var hitBeat = cond.songPositionInBeats;

            var bounceBeat = game.targetBeat + 1f;

            if (game.rallySpeed == RhythmRally.RallySpeed.Slow)
            {
                bounceBeat = game.targetBeat + 2f;
            }
            else if (game.rallySpeed == RhythmRally.RallySpeed.SuperFast)
            {
                bounceBeat = game.targetBeat + 0.5f;
            }

            playerAnim.Play("Swing", 0, 0);
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("rhythmRally/Return", hitBeat), new MultiSound.Sound("rhythmRally/ReturnBounce", bounceBeat) });
            BounceFX(bounceBeat);
        }

        void Miss()
        {
            game.served = false;
            game.missed = true;

            var whistleBeat = game.targetBeat + 1f;

            if (game.rallySpeed == RhythmRally.RallySpeed.Slow)
            {
                whistleBeat = game.targetBeat + 2f;
            }
            else if (game.rallySpeed == RhythmRally.RallySpeed.SuperFast)
            {
                whistleBeat = game.targetBeat + 0.5f;
            }

            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("rhythmRally/Whistle", whistleBeat) });
        }

        public void BounceFX(float bounceBeat)
        {
            BeatAction.New(this.gameObject, new List<BeatAction.Action>()
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

        public override void OnAce()
        {
            Ace();
        }
    }
}
