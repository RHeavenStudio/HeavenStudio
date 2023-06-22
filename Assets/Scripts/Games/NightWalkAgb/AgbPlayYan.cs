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
            Jumping
        }
        private JumpingState jumpingState;
        private AgbNightWalk game;
        private double jumpBeat;
        [SerializeField] private List<Animator> balloons = new List<Animator>();
        private Path jumpPath;
        private Animator anim;

        private void Awake()
        {
            game = AgbNightWalk.instance;
            jumpPath = game.GetPath("Jump");
            anim = GetComponent<Animator>();
            foreach (var balloon in balloons)
            {
                balloon.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));
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
                        float normalizedBeat = cond.GetPositionFromBeat(jumpBeat, 1);
                        if (normalizedBeat >= 1f)
                        {
                            Walk();
                        }
                        break;
                    case JumpingState.Walking:
                        transform.localPosition = Vector3.zero;
                        break;
                    case JumpingState.Flying:
                        transform.localPosition = Vector3.zero;
                        break;
                }
            }
        }

        public void Jump(double beat)
        {
            jumpingState = JumpingState.Jumping;
            jumpBeat = beat;
            anim.Play("Jump", 0, 0);
            Update();
        }

        public void Walk()
        {
            if (jumpingState == JumpingState.Walking) return;
            jumpingState = JumpingState.Walking;
            anim.DoScaledAnimationAsync("Walk", 0.5f);
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


