using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using System;

namespace HeavenStudio.Games.Scripts_Splashdown
{
    public class NtrSynchrette : MonoBehaviour
    {
        [SerializeField] private NtrSplash splashPrefab;
        [SerializeField] private Animator anim;
        [SerializeField] private Transform synchretteTransform;
        [SerializeField] private Transform splashHolder;
        [SerializeField] private Animator throwAnim;

        private Splashdown game;

        private double startBeat;

        private bool missedJump;

        [SerializeField] private float jumpHeight = 5f;
        [SerializeField] private float jumpStart = -3f;

        private enum MovementState
        {
            None,
            Dive,
            Jumping,
            Raise,
            Stand,
            JumpIntoWater
        }

        private MovementState currentMovementState;

        private void Awake()
        {
            game = Splashdown.instance;
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            switch (currentMovementState)
            {
                case MovementState.None:
                    float normalizedFloatDownBeat = cond.GetPositionFromBeat(0, 1) % 2;
                    float normalizedFloatUpBeat = cond.GetPositionFromBeat(1, 1) % 2;
                    if (normalizedFloatUpBeat <= 1f)
                    {
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutQuad);
                        float newPosYUp = func(0, -0.5f, normalizedFloatUpBeat);
                        synchretteTransform.localPosition = new Vector3(0f, newPosYUp, 0f);
                    }
                    else
                    {
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutQuad);
                        float newPosYDown = func(-0.5f, 0, normalizedFloatDownBeat);
                        synchretteTransform.localPosition = new Vector3(0f, newPosYDown, 0f);
                    }
                    break;
                case MovementState.Dive:
                    synchretteTransform.localPosition = new Vector3(0f, -6f, 0f);
                    break;
                case MovementState.Jumping:
                    float normalizedUpBeat = cond.GetPositionFromBeat(startBeat, 1);
                    float normalizedDownBeat = cond.GetPositionFromBeat(startBeat + 1, 1);
                    if (normalizedUpBeat <= 1f)
                    {
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutCubic);
                        float newPosYUp = func(jumpStart, jumpHeight, normalizedUpBeat);
                        synchretteTransform.localPosition = new Vector3(0f, newPosYUp, 0f);
                    }
                    else
                    {
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInCubic);
                        float newPosYDown = func(jumpHeight, jumpStart, normalizedDownBeat);
                        synchretteTransform.localPosition = new Vector3(0f, newPosYDown, 0f);
                    }
                    if (missedJump) return;
                    float normalizedRotBeat = cond.GetPositionFromBeat(startBeat + 1, 0.25);
                    float newAngle = Mathf.Lerp(0, 360, normalizedRotBeat);
                    synchretteTransform.localEulerAngles = new Vector3(0, 0, newAngle);
                    break;
                case MovementState.Raise:
                    float normalizedBeat = cond.GetPositionFromBeat(startBeat, 1);
                    float newPosY = Mathf.Lerp(-6f, 0, normalizedBeat);
                    synchretteTransform.localPosition = new Vector3(0, newPosY, 0);
                    if (normalizedBeat >= 1)
                    {
                        SetState(MovementState.None, 0);
                    }
                    break;
                case MovementState.Stand:
                    synchretteTransform.localPosition = new Vector3(0, 2.73f, 0);
                    break;
                case MovementState.JumpIntoWater:
                    float normalizedUp = cond.GetPositionFromBeat(startBeat, 0.5);
                    float normalizedDown = cond.GetPositionFromBeat(startBeat + 0.5, 0.5);
                    if (normalizedUp <= 1f)
                    {
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseOutQuad);
                        float newPosYUp = func(2.73f, 4f, normalizedUp);
                        synchretteTransform.localPosition = new Vector3(0f, newPosYUp, 0f);
                    }
                    else
                    {
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInQuad);
                        float newPosYDown = func(4f, -3, normalizedDown);
                        synchretteTransform.localPosition = new Vector3(0f, newPosYDown, 0f);
                    }
                    break;
            }
        }

        public void Appear(bool miss = false, int appearType = 1)
        {
            SetState(MovementState.None, startBeat);
            if (!miss) anim.DoScaledAnimationAsync("Appear" + appearType, 0.4f);
            else anim.DoScaledAnimationAsync("MissAppear", 0.4f);
            Instantiate(splashPrefab, splashHolder).Init("Appearsplash");
        }

        public void GoDown(bool splash = true)
        {
            SetState(MovementState.Dive, startBeat);
            if (splash) Instantiate(splashPrefab, splashHolder).Init("GodownSplash");
        }

        public void Bop()
        {
            anim.DoScaledAnimationAsync("Bop", 0.5f);
            if (currentMovementState != MovementState.Stand)
            {
                SetState(MovementState.Stand, 0);
            }
        }

        public void JumpIntoWater(double beat)
        {
            anim.Play("Idle", 0, 0);
            SetState(MovementState.JumpIntoWater, beat);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.75, delegate
                {
                    Instantiate(splashPrefab, splashHolder).Init("GodownSplash");
                }),
                new BeatAction.Action(beat + 1, delegate
                {
                    SetState(MovementState.Raise, beat + 1);
                })
            });
        }

        public void Jump(double beat, bool missed = false, bool noDolphin = false)
        {
            missedJump = missed;
            SetState(MovementState.Jumping, beat);
            Instantiate(splashPrefab, splashHolder).Init("Appearsplash");
            if (noDolphin)
            {
                anim.DoScaledAnimationAsync(missed ? "JumpMiss" : "JumpOut", 0.5f);
                throwAnim.gameObject.SetActive(true);
                throwAnim.DoScaledAnimationAsync("Throw", 0.5f);
            }
            else
            {
                anim.DoScaledAnimationAsync(missed ? "DolphinMiss" : "Dolphin", 0.5f);
            }
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1.75, delegate { Instantiate(splashPrefab, splashHolder).Init("BigSplash"); }),
                new BeatAction.Action(beat + 2, delegate
                {
                    anim.Play("Idle", 0, 0);
                    SetState(MovementState.Raise, beat + 2);
                })
            });
        }

        private void SetState(MovementState state, double beat)
        {
            currentMovementState = state;
            startBeat = beat;
            synchretteTransform.localEulerAngles = Vector3.zero;
            Update();
        }
    }
}

