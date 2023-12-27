using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCJumper : SuperCurveObject
    {
        [SerializeField] private float _jumpDistanceX;
        [SerializeField] private float _jumpDistanceY;
        [SerializeField] private float _jumpDistanceSmallX;
        [SerializeField] private float _jumpDistanceSmallY;
        [SerializeField] private float _jumpOffsetY;
        [SerializeField] private float _jumpHeight = 2f;

        private Path _path;
        private Animator _anim;
        private TotemClimb _game;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _game = TotemClimb.instance;
            _path = new Path();
            _path.positions = new PathPos[2];
            _path.positions[0] = new PathPos() 
            { 
                duration = 1,
                pos = new(transform.localPosition.x, transform.localPosition.y),
                height = _jumpHeight
            };
            _path.positions[1] = new PathPos()
            {
                pos = new(transform.localPosition.x + _jumpDistanceX, transform.localPosition.y + _jumpDistanceY + _jumpOffsetY)
            };
        }

        public void StartJumping(double beat, bool inTriple = false)
        {
            bool nextIsTriple = _game.IsTripleBeat(beat + 1);
            StartCoroutine(JumpCo(beat, 1f, nextIsTriple && !inTriple));
            if (beat + 1 >= _game.EndBeat) return;
            
            _game.ScheduleInput(beat, 1, Minigame.InputAction_BasicPress, nextIsTriple ? JustTripleEnter : Just, nextIsTriple ? MissTripleEnter : Miss, Empty);
        }

        public void StartTripleJumping(double beat, bool exit)
        {
            StartCoroutine(JumpCo(beat, 0.5f, true));
            if (beat + 0.5 >= _game.EndBeat) return;
            _game.ScheduleInput(beat, 0.5, Minigame.InputAction_BasicPress, exit ? Just : JustTripleExit, exit ? Miss : MissTripleExit, Empty);
        }

        public void Bop()
        {
            _anim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        private IEnumerator JumpCo(double beat, float length = 1f, bool small = false)
        {
            _path.positions[0].duration = length - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat + length));
            _anim.Play("Jump", 0, 0);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while(normalizedBeat < 1f)
            {
                transform.localPosition = GetPathPositionFromBeat(_path, Math.Min(Conductor.instance.songPositionInBeatsAsDouble, beat + _path.positions[0].duration), beat);

                if (normalizedBeat >= 0.5f && !playedFall)
                {
                    _anim.Play("Fall", 0, 0);
                    playedFall = true;
                }

                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
                yield return null;
            }
            _path.positions[0].pos += small ? new Vector3(_jumpDistanceSmallX, _jumpDistanceSmallY) : new Vector3(_jumpDistanceX, _jumpDistanceY);
            _path.positions[1].pos += small ? new Vector3(_jumpDistanceSmallX, _jumpDistanceSmallY) : new Vector3(_jumpDistanceX, _jumpDistanceY);
            _anim.Play("Idle", 0, 0);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            bool isTriple = _game.IsTripleBeat(caller.startBeat + caller.timer);
            StartJumping(caller.startBeat + caller.timer, isTriple);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SoundByte.PlayOneShotGame(isTriple ? "totemClimb/totemlandb" : "totemClimb/totemland");
        }

        private void JustTripleEnter(PlayerActionEvent caller, float state)
        {
            StartTripleJumping(caller.startBeat + caller.timer, false);
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SoundByte.PlayOneShotGame("totemClimb/totemland");
        }

        private void JustTripleExit(PlayerActionEvent caller, float state)
        {
            StartTripleJumping(caller.startBeat + caller.timer, true);
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SoundByte.PlayOneShotGame("totemClimb/totemland");
        }

        private void Miss(PlayerActionEvent caller)
        {
            StartJumping(caller.startBeat + caller.timer);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
        }

        private void MissTripleEnter(PlayerActionEvent caller)
        {
            StartTripleJumping(caller.startBeat + caller.timer, false);
        }

        private void MissTripleExit(PlayerActionEvent caller)
        {
            StartTripleJumping(caller.startBeat + caller.timer, true);
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}

