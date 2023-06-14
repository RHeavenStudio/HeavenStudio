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

        private Splashdown game;

        private double startBeat;

        private enum MovementState
        {
            None,
            Dive,
            Jumping,
            Raise
        }

        private MovementState currentMovementState;

        private void Awake()
        {
            game = Splashdown.instance;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            switch (currentMovementState)
            {
                case MovementState.None:
                    synchretteTransform.localPosition = Vector3.zero;
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
                        float newPosYUp = func(-3, 4.5f, normalizedUpBeat);
                        synchretteTransform.localPosition = new Vector3(0f, newPosYUp, 0f);
                    }
                    else
                    {
                        EasingFunction.Function func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInCubic);
                        float newPosYDown = func(4.5f, -3, normalizedDownBeat);
                        synchretteTransform.localPosition = new Vector3(0f, newPosYDown, 0f);
                    }
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
            }
        }

        public void Appear(bool miss = false)
        {
            SetState(MovementState.None, startBeat);
            if (!miss) anim.DoScaledAnimationAsync("Appear" + game.currentAppearType, 0.4f);
            else anim.DoScaledAnimationAsync("MissAppear", 0.4f);
            Instantiate(splashPrefab, splashHolder).Init("Appearsplash");
        }

        public void GoDown()
        {
            SetState(MovementState.Dive, startBeat);
            Instantiate(splashPrefab, splashHolder).Init("GodownSplash");
        }

        public void Jump(double beat)
        {
            SetState(MovementState.Jumping, beat);
            Instantiate(splashPrefab, splashHolder).Init("Appearsplash");
            anim.Play("Dolphin", 0, 0);
            BeatAction.New(gameObject, new List<BeatAction.Action>()
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
        }
    }
}

