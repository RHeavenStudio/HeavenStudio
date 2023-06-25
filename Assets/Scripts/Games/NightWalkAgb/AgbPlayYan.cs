using HeavenStudio.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AgbNightWalk
{
    public class AgbPlayYan : SuperCurveObject
    {
        private enum JumpingState
        {
            Flying,
            Walking,
            Jumping,
            Shocked,
            Falling,
            Whiffing,
            Floating
        }
        private JumpingState jumpingState;
        private AgbNightWalk game;
        private double jumpBeat;
        [SerializeField] private List<Animator> balloons = new List<Animator>();
        [SerializeField] private GameObject star;
        private Path jumpPath;
        private Path whiffPath;
        private Animator anim;
        private float fallStartY;
        private double playYanFallBeat;
        private double walkBeat;
        [SerializeField] private float randomMinBalloonX = -0.45f;
        [SerializeField] private float randomMaxBalloonX = 0.45f;

        private void Awake()
        {
            game = AgbNightWalk.instance;
            jumpPath = game.GetPath("Jump");
            whiffPath = game.GetPath("Whiff");
            anim = GetComponent<Animator>();
            foreach (var balloon in balloons)
            {
                balloon.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));
                Transform balloonTrans = balloon.transform.parent;
                balloonTrans.localPosition = new Vector3(balloonTrans.localPosition.x + UnityEngine.Random.Range(randomMinBalloonX, randomMaxBalloonX), balloonTrans.localPosition.y);
            }
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                switch (jumpingState)
                {
                    case JumpingState.Jumping:
                        Vector3 pos = GetPathPositionFromBeat(jumpPath, Math.Min(jumpBeat + 1, cond.songPositionInBeatsAsDouble), jumpBeat);
                        transform.localPosition = pos;
                        float normalizedBeat = cond.GetPositionFromBeat(jumpBeat, jumpPath.positions[0].duration);
                        if (normalizedBeat >= 1f)
                        {
                            Walk();
                        }
                        break;
                    case JumpingState.Walking:
                        transform.localPosition = Vector3.zero;
                        anim.DoScaledAnimation("Walk", walkBeat, 0.5f, 0.75f);
                        if (PlayerInput.Pressed() && !game.IsExpectingInputNow(InputType.STANDARD_DOWN))
                        {
                            Whiff(cond.songPositionInBeatsAsDouble);
                        }
                        break;
                    case JumpingState.Flying:
                        transform.localPosition = Vector3.zero;
                        break;
                    case JumpingState.Shocked:
                        break;
                    case JumpingState.Falling:
                        float normalizedFallBeat = cond.GetPositionFromBeat(playYanFallBeat, 2);
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
                        float newPlayYanY = func(fallStartY, -12, normalizedFallBeat);
                        transform.localPosition = new Vector3(0, newPlayYanY);
                        break;
                    case JumpingState.Whiffing:
                        Vector3 pos2 = GetPathPositionFromBeat(whiffPath, Math.Min(jumpBeat + 0.5, cond.songPositionInBeatsAsDouble), jumpBeat);
                        transform.localPosition = pos2;
                        float normalizedBeat2 = cond.GetPositionFromBeat(jumpBeat, 0.5);
                        if (normalizedBeat2 >= 1f)
                        {
                            Walk();
                        }
                        break;
                    case JumpingState.Floating:
                        float normalizedFloatBeat = cond.GetPositionFromBeat(playYanFallBeat, 10);
                        EasingFunction.Function funcF = EasingFunction.GetEasingFunction(EasingFunction.Ease.Linear);
                        float newPlayYanYF = funcF(fallStartY, 12, normalizedFloatBeat);
                        transform.localPosition = new Vector3(0, newPlayYanYF);
                        break;
                }
            }
        }

        public void Shock()
        {
            jumpingState = JumpingState.Shocked;
            anim.DoScaledAnimationAsync("Shock", 0.5f);
            fallStartY = transform.localPosition.y;
            SoundByte.PlayOneShotGame("nightWalkAgb/shock");
        }

        public void Fall(double beat)
        {
            jumpingState = JumpingState.Falling;
            anim.Play("Jump", 0, 0);
            playYanFallBeat = beat;
            SoundByte.PlayOneShotGame("nightWalkAgb/fall");
            Update();
        }

        public void Float(double beat)
        {
            jumpingState = JumpingState.Floating;
            anim.Play("Jump", 0, 0);
            playYanFallBeat = beat;
            fallStartY = transform.localPosition.y;
            star.SetActive(true);
            Update();
        }

        public void Jump(double beat)
        {
            jumpingState = JumpingState.Jumping;
            jumpBeat = beat;
            anim.Play("Jump", 0, 0);
            jumpPath.positions[0].duration = 1 - (float)Conductor.instance.SecsToBeats(Minigame.earlyTime, Conductor.instance.GetBpmAtBeat(jumpBeat));
            Update();
        }

        public void Whiff(double beat)
        {
            jumpingState = JumpingState.Whiffing;
            jumpBeat = beat;
            anim.Play("Jump", 0, 0);
            SoundByte.PlayOneShotGame("nightWalkAgb/whiff");
            Update();
        }

        public void Walk()
        {
            if (jumpingState == JumpingState.Walking) return;
            jumpingState = JumpingState.Walking;
            walkBeat = Conductor.instance.songPositionInBeats;
        }
        public void PopBalloon(int index, bool instant)
        {
            if (instant)
            {
                balloons[index].DoNormalizedAnimation("Pop", 1);
                return;
            }
            balloons[index].DoScaledAnimationAsync("Pop", 0.5f);
        }

        public void PopAll()
        {
            foreach (var balloon in balloons)
            {
                balloon.DoNormalizedAnimation("Pop", 1);
            }
        }

        public void Hide()
        {
            anim.gameObject.SetActive(false);
        }
    }
}


