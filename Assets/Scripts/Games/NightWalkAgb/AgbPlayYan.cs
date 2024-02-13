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
            Floating,
            Rolling,
            HighJumping,
            JumpingFall,
            HighJumpingFall
        }
        private JumpingState jumpingState;
        private AgbNightWalk game;
        private double jumpBeat;
        [SerializeField] private List<Animator> balloons = new List<Animator>();
        [SerializeField] private Animator star;
        private Path jumpPath;
        private Path whiffPath;
        private Path highJumpPath;
        private Animator anim;
        private float fallStartY;
        private double playYanFallBeat;
        private double walkBeat;
        [SerializeField] private float randomMinBalloonX = -0.45f;
        [SerializeField] private float randomMaxBalloonX = 0.45f;
        [SerializeField] private Transform spriteTrans; //for rolling rotation

        private void Awake()
        {
            game = AgbNightWalk.instance;
            jumpPath = game.GetPath("Jump");
            whiffPath = game.GetPath("Whiff");
            highJumpPath = game.GetPath("highJump");
            anim = GetComponent<Animator>();
            foreach (var balloon in balloons)
            {
                balloon.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));
                Transform balloonTrans = balloon.transform.parent;
                balloonTrans.localPosition = new Vector3(balloonTrans.localPosition.x + UnityEngine.Random.Range(randomMinBalloonX, randomMaxBalloonX), balloonTrans.localPosition.y);
            }
        }
        bool hasFallen;
        private void Update()
        {
            var cond = Conductor.instance;
            if (cond.isPlaying && !cond.isPaused)
            {
                switch (jumpingState)
                {
                    case JumpingState.Jumping:
                        Vector3 pos = GetPathPositionFromBeat(jumpPath, Math.Min(jumpBeat + jumpPath.positions[0].duration, cond.songPositionInBeatsAsDouble), jumpBeat);
                        transform.localPosition = pos;
                        float normalizedBeat = cond.GetPositionFromBeat(jumpBeat, jumpPath.positions[0].duration);
                        if (normalizedBeat >= 1f)
                        {
                            Walk();
                        }
                        break;
                    case JumpingState.JumpingFall:
                        Vector3 posf = GetPathPositionFromBeat(jumpPath, cond.songPositionInBeatsAsDouble, jumpBeat);
                        transform.localPosition = posf;
                        float normalizedBeatf = cond.GetPositionFromBeat(jumpBeat, jumpPath.positions[0].duration);
                        if (normalizedBeatf >= 1f && !hasFallen)
                        {
                            hasFallen = true;
                            SoundByte.PlayOneShotGame("nightWalkAgb/fall");
                        }
                        break;
                    case JumpingState.Walking:
                        transform.localPosition = Vector3.zero;
                        anim.DoScaledAnimation("Walk", walkBeat, 0.5f, 0.35f);
                        if (PlayerInput.GetIsAction(Minigame.InputAction_BasicPress) && !game.IsExpectingInputNow(Minigame.InputAction_BasicPress))
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
                    case JumpingState.Rolling:
                        float normalizedRoll = cond.GetPositionFromBeat(jumpBeat, 0.5f);
                        float newRot = Mathf.LerpUnclamped(0, -360, normalizedRoll);
                        spriteTrans.localEulerAngles = new Vector3(0, 0, newRot);
                        break;
                    case JumpingState.HighJumping:
                        Vector3 posH = GetPathPositionFromBeat(highJumpPath, Math.Min(jumpBeat + highJumpPath.positions[0].duration, cond.songPositionInBeatsAsDouble), jumpBeat);
                        transform.localPosition = posH;
                        float normalizedBeatH = cond.GetPositionFromBeat(jumpBeat, highJumpPath.positions[0].duration);
                        if (normalizedBeatH >= 1f)
                        {
                            Walk();
                        }
                        break;
                    case JumpingState.HighJumpingFall:
                        Vector3 posHf = GetPathPositionFromBeat(highJumpPath, cond.songPositionInBeatsAsDouble, jumpBeat);
                        transform.localPosition = posHf;
                        float normalizedBeatHf = cond.GetPositionFromBeat(jumpBeat, highJumpPath.positions[0].duration);
                        if (normalizedBeatHf >= 1f && !hasFallen)
                        {
                            hasFallen = true;
                            SoundByte.PlayOneShotGame("nightWalkAgb/fall");
                        }
                        break;
                }
            }
        }

        public void Shock(bool roll = false)
        {
            jumpingState = JumpingState.Shocked;
            anim.DoScaledAnimationAsync(roll ? "RollShock" : "Shock", 0.5f);
            SoundByte.PlayOneShotGame("nightWalkAgb/shock");
            spriteTrans.localEulerAngles = Vector3.zero;
        }

        public void Fall(double beat)
        {
            jumpingState = JumpingState.Falling;
            anim.Play("Jump", 0, 0);
            playYanFallBeat = beat;
            fallStartY = transform.localPosition.y;
            SoundByte.PlayOneShotGame("nightWalkAgb/fall");
            spriteTrans.localEulerAngles = Vector3.zero;
            Update();
        }

        public void Float(double beat)
        {
            jumpingState = JumpingState.Floating;
            anim.Play("Jump", 0, 0);
            playYanFallBeat = beat;
            fallStartY = transform.localPosition.y;
            star.gameObject.SetActive(true);
            StarBlink();
            spriteTrans.localEulerAngles = Vector3.zero;
            Update();
        }

        private void StarBlink()
        {
            if (UnityEngine.Random.Range(1, 3) == 1) star.DoScaledAnimationAsync("Blink", 0.5f);
            Invoke("StarBlink", UnityEngine.Random.Range(0.1f, 0.3f));
        }
        public void Jump(double beat, bool fall = false)
        {
            jumpingState = fall ? JumpingState.JumpingFall : JumpingState.Jumping;
            jumpBeat = beat;
            anim.Play("Jump", 0, 0);
            spriteTrans.localEulerAngles = Vector3.zero;
            jumpPath.positions[0].duration = 1 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(jumpBeat));
            Update();
        }

        public void HighJump(double beat, bool fall = false, bool barely = false)
        {
            jumpingState = fall ? JumpingState.HighJumpingFall : JumpingState.HighJumping;
            jumpBeat = beat;
            anim.DoScaledAnimationAsync("HighJump", 0.5f);
            spriteTrans.localEulerAngles = Vector3.zero;
            highJumpPath.positions[0].duration = 1.5f - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(jumpBeat));
            highJumpPath.positions[0].height = barely ? 3.5f : 4.5f;
            Update();
        }

        public void Roll(double beat)
        {
            jumpingState = JumpingState.Rolling;
            jumpBeat = beat;
            anim.DoScaledAnimationAsync("Roll", 0.5f);
            Update();
        }

        public void Whiff(double beat)
        {
            jumpingState = JumpingState.Whiffing;
            jumpBeat = beat;
            anim.Play("Jump", 0, 0);
            SoundByte.PlayOneShotGame("nightWalkAgb/whiff");
            spriteTrans.localEulerAngles = Vector3.zero;
            Update();
        }

        public void Walk()
        {
            if (jumpingState == JumpingState.Walking) return;
            jumpingState = JumpingState.Walking;
            walkBeat = Conductor.instance.songPositionInBeats;
            spriteTrans.localEulerAngles = Vector3.zero;
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


