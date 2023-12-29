using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCJumper : SuperCurveObject
    {
        [SerializeField] private Transform _initialPoint;
        [SerializeField] private float _jumpHeight = 2f;

        private Path _path;
        private Animator _anim;
        private TotemClimb _game;
        private double _startBeat;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _game = TotemClimb.instance;
        }

        public void InitPath(double beat)
        {
            _startBeat = beat;
            _path = new Path();
            _path.positions = new PathPos[2];
            _path.positions[0] = new PathPos()
            {
                duration = 1 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat)),
                target = _initialPoint,
                height = _jumpHeight
            };
            _path.positions[1] = new PathPos()
            {
                target = _game.GetJumperPointAtBeat(beat)
            };
        }

        public void StartJumping(double beat)
        {
            bool nextIsTriple = _game.IsTripleBeat(beat + 1);
            StartCoroutine(JumpCo(beat));
            if (beat + 1 >= _game.EndBeat) return;
            
            _game.ScheduleInput(beat, 1, Minigame.InputAction_BasicPress, Just, Miss, Empty);
        }

        public void Bop()
        {
            _anim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        private IEnumerator JumpCo(double beat)
        {
            if (beat >= _startBeat)
            {
                _path = new Path();
                _path.positions = new PathPos[2];
                _path.positions[0] = new PathPos()
                {
                    duration = 1 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat + 1)),
                    height = _jumpHeight,
                    target = _game.GetJumperPointAtBeat(beat)
                };
                _path.positions[1] = new PathPos()
                {
                    target = _game.GetJumperPointAtBeat(beat + 1)
                };
            }
            _anim.Play("Jump", 0, 0);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while(normalizedBeat < 1f)
            {
                transform.position = GetPathPositionFromBeat(_path, Math.Min(Conductor.instance.songPositionInBeatsAsDouble, beat + _path.positions[0].duration), beat);

                if (normalizedBeat >= 0.5f && !playedFall)
                {
                    _anim.Play("Fall", 0, 0);
                    playedFall = true;
                }

                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
                yield return null;
            }
            _anim.Play("Idle", 0, 0);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            bool isTriple = _game.IsTripleBeat(caller.startBeat + caller.timer);
            StartJumping(caller.startBeat + caller.timer);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
            if (state >= 1f || state <= -1f)
            {
                return;
            }
            SoundByte.PlayOneShotGame(isTriple ? "totemClimb/totemlandb" : "totemClimb/totemland");
        }

        private void Miss(PlayerActionEvent caller)
        {
            StartJumping(caller.startBeat + caller.timer);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}

