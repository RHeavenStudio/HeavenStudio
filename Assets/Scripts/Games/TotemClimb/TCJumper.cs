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

        public void StartJumping(double beat)
        {
            StartCoroutine(JumpCo(beat));
            if (beat + 1 >= _game.EndBeat) return;
            _game.ScheduleInput(beat, 1, Minigame.InputAction_BasicPress, Just, Miss, Empty);
        }


        private IEnumerator JumpCo(double beat)
        {
            _path.positions[0].duration = 1 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat + 1));
            _anim.Play("Jump", 0, 0);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while(normalizedBeat < 1)
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
            _path.positions[0].pos += new Vector3(_jumpDistanceX, _jumpDistanceY);
            _path.positions[1].pos += new Vector3(_jumpDistanceX, _jumpDistanceY);
            _anim.Play("Idle", 0, 0);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            StartJumping(caller.startBeat + caller.timer);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
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

        private void Empty(PlayerActionEvent caller) { }
    }
}

