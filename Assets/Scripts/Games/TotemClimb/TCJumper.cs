using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HeavenStudio.Util;
using static UnityEngine.GraphicsBuffer;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCJumper : SuperCurveObject
    {
        [SerializeField] private Transform _initialPoint;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _jumpHeightTriple = 1f;
        [SerializeField] private float _jumpHighHeight = 6f;

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
            if (_game.IsTripleBeat(beat))
            {
                _path.positions[1] = new PathPos()
                {
                    target = _game.GetJumperFrogPointAtBeat(beat, -1)
                };
            }
            else
            {
                _path.positions[1] = new PathPos()
                {
                    target = _game.GetJumperPointAtBeat(beat)
                };
            }
        }

        public void StartJumping(double beat, bool miss = false)
        {
            bool nextIsTriple = _game.IsTripleBeat(beat + 1);
            bool nextIsHigh = _game.IsHighBeat(beat + 1);
            StartCoroutine(JumpCo(beat, miss));
            if (beat + 1 >= _game.EndBeat) return;
            if (nextIsHigh)
            {
                _game.ScheduleInput(beat, 1, Minigame.InputAction_BasicPress, JustHold, Empty, Empty);
                _game.ScheduleInput(beat, 3, Minigame.InputAction_FlickRelease, JustRelease, MissRelease, Empty);
            }
            else _game.ScheduleInput(beat, 1, Minigame.InputAction_BasicPress, nextIsTriple ? JustTripleEnter : Just, nextIsTriple ? MissTripleEnter : Miss, Empty);
        }

        public void HighJump(double beat, bool miss)
        {
            bool nextIsTriple = _game.IsTripleBeat(beat + 2);
            bool nextIsHigh = _game.IsHighBeat(beat + 2);
            StartCoroutine(JumpHighCo(beat, miss));
            if (beat + 2 >= _game.EndBeat) return;
            if (nextIsHigh)
            {
                _game.ScheduleInput(beat, 2, Minigame.InputAction_BasicPress, JustHold, Empty, Empty);
                _game.ScheduleInput(beat, 4, Minigame.InputAction_FlickRelease, JustRelease, MissRelease, Empty);
            }
            else _game.ScheduleInput(beat, 2, Minigame.InputAction_BasicPress, nextIsTriple ? JustTripleEnter : Just, nextIsTriple ? MissTripleEnter : Miss, Empty);
        }

        public void TripleJumping(double beat, bool enter, bool miss = false)
        {
            StartCoroutine(JumpTripleCo(beat, enter, miss));
            if (beat + 0.5 >= _game.EndBeat) return;
            _game.ScheduleInput(beat, 0.5, Minigame.InputAction_BasicPress, enter ? JustTripleExit : Just, enter ? MissTripleExit : Miss, Empty);
        }

        public void Bop()
        {
            _anim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        private IEnumerator JumpCo(double beat, bool miss)
        {
            if (beat >= _startBeat)
            {
                _path = new Path();
                _path.positions = new PathPos[2];
                _path.positions[0] = new PathPos()
                {
                    duration = 1 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat + 1)),
                    height = _jumpHeight,
                    target = _game.IsTripleBeat(beat) ? _game.GetJumperFrogPointAtBeat(beat, 1) : _game.GetJumperPointAtBeat(beat)
                };
                
                if (_game.IsHighBeat(beat + 1))
                {
                    _path.positions[1] = new PathPos()
                    {
                        target = _game.GetDragonPointAtBeat(beat + 1)
                    };
                }
                else
                {
                    _path.positions[1] = new PathPos()
                    {
                        target = _game.IsTripleBeat(beat + 1) ? _game.GetJumperFrogPointAtBeat(beat + 1, -1) : _game.GetJumperPointAtBeat(beat + 1)
                    };
                }
            }
            if (!miss) _anim.Play("Jump", 0, 0);
            else _anim.DoScaledAnimationAsync("Miss", 0.5f);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while(normalizedBeat < 1f)
            {
                transform.position = GetPathPositionFromBeat(_path, Math.Min(Conductor.instance.songPositionInBeatsAsDouble, beat + _path.positions[0].duration), beat);

                if (normalizedBeat >= 0.5f && !playedFall)
                {
                    if (!miss) _anim.Play("Fall", 0, 0);
                    playedFall = true;
                }

                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
                yield return null;
            }
            _anim.Play("Idle", 0, 0);
        }

        private IEnumerator JumpTripleCo(double beat, bool enter, bool miss)
        {
            if (beat >= _startBeat)
            {
                _path = new Path();
                _path.positions = new PathPos[2];
                _path.positions[0] = new PathPos()
                {
                    duration = 0.5f - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat + 0.5)),
                    height = _jumpHeightTriple,
                    target = _game.GetJumperFrogPointAtBeat(beat, enter ? -1 : 0)
                };
                _path.positions[1] = new PathPos()
                {
                    target = _game.GetJumperFrogPointAtBeat(beat + 0.5, enter ? 0 : 1)
                };
            }
            if (!miss) _anim.Play("Jump", 0, 0);
            else _anim.DoScaledAnimationAsync("Miss", 0.5f);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while (normalizedBeat < 1f)
            {
                transform.position = GetPathPositionFromBeat(_path, Math.Min(Conductor.instance.songPositionInBeatsAsDouble, beat + _path.positions[0].duration), beat);

                if (normalizedBeat >= 0.5f && !playedFall)
                {
                    if (!miss) _anim.Play("Fall", 0, 0);
                    playedFall = true;
                }

                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
                yield return null;
            }
            _anim.Play("Idle", 0, 0);
        }

        private IEnumerator JumpHighCo(double beat, bool miss)
        {
            if (beat >= _startBeat)
            {
                _path = new Path();
                _path.positions = new PathPos[2];
                _path.positions[0] = new PathPos()
                {
                    duration = 2 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat + 2)),
                    height = _jumpHighHeight,
                    target = _game.GetDragonPointAtBeat(beat)
                };

                if (_game.IsHighBeat(beat + 2))
                {
                    _path.positions[1] = new PathPos()
                    {
                        target = _game.GetDragonPointAtBeat(beat + 2)
                    };
                }
                else
                {
                    _path.positions[1] = new PathPos()
                    {
                        target = _game.IsTripleBeat(beat + 2) ? _game.GetJumperFrogPointAtBeat(beat + 2, -1) : _game.GetJumperPointAtBeat(beat + 2)
                    };
                }
            }
            if (!miss) _anim.Play("Jump", 0, 0);
            else _anim.DoScaledAnimationAsync("Miss", 0.5f);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while (normalizedBeat < 1f)
            {
                transform.position = GetPathPositionFromBeat(_path, Math.Min(Conductor.instance.songPositionInBeatsAsDouble, beat + _path.positions[0].duration), beat);

                if (normalizedBeat >= 0.5f && !playedFall)
                {
                    if (!miss) _anim.Play("Fall", 0, 0);
                    playedFall = true;
                }

                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
                yield return null;
            }
            _anim.Play("Idle", 0, 0);
        }

        private IEnumerator HoldCo(double beat)
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, 1);
            Transform dragonPoint = _game.GetDragonPointAtBeat(beat);
            while (normalizedBeat < 1f)
            {
                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, 1);
                transform.position = dragonPoint.position;
                yield return null;
            }
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            bool isTriple = _game.IsTripleBeat(caller.startBeat + caller.timer);
            StartJumping(caller.startBeat + caller.timer);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
            if (isTriple) _game.FallFrogAtBeat(caller.startBeat + caller.timer, 1);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                return;
            }
            SoundByte.PlayOneShotGame(isTriple ? "totemClimb/totemlandb" : "totemClimb/totemland");
        }

        private void JustTripleEnter(PlayerActionEvent caller, float state)
        {
            TripleJumping(caller.startBeat + caller.timer, true);
            _game.FallFrogAtBeat(caller.startBeat + caller.timer, -1);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                return;
            }
            SoundByte.PlayOneShotGame("totemClimb/totemland");
        }

        private void JustTripleExit(PlayerActionEvent caller, float state)
        {
            TripleJumping(caller.startBeat + caller.timer, false);
            _game.FallFrogAtBeat(caller.startBeat + caller.timer, 0);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                return;
            }
            SoundByte.PlayOneShotGame("totemClimb/totemland");
        }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("totemClimb/chargejump");
            _game.HoldDragonAtBeat(caller.startBeat + caller.timer);
            StartCoroutine(HoldCo(caller.startBeat + caller.timer));
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                return;
            }
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            HighJump(caller.startBeat + caller.timer, false);
            _game.ReleaseDragonAtBeat(caller.startBeat + caller.timer);
            if (state >= 1f || state <= -1f)
            {
                SoundByte.PlayOneShot("nearMiss");
                return;
            }
            SoundByte.PlayOneShotGame("totemClimb/superjumpgood");
        }

        private void Miss(PlayerActionEvent caller)
        {
            StartJumping(caller.startBeat + caller.timer, true);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
            if (_game.IsTripleBeat(caller.startBeat + caller.timer)) _game.FallFrogAtBeat(caller.startBeat + caller.timer, 1);
            SoundByte.PlayOneShot("miss");
        }

        private void MissTripleEnter(PlayerActionEvent caller)
        {
            TripleJumping(caller.startBeat + caller.timer, true, true);
            _game.FallFrogAtBeat(caller.startBeat + caller.timer, -1);
            SoundByte.PlayOneShot("miss");
        }

        private void MissTripleExit(PlayerActionEvent caller)
        {
            TripleJumping(caller.startBeat + caller.timer, false, true);
            _game.FallFrogAtBeat(caller.startBeat + caller.timer, 0);
            SoundByte.PlayOneShot("miss");
        }

        private void MissRelease(PlayerActionEvent caller)
        {
            HighJump(caller.startBeat + caller.timer, true);
            _game.ReleaseDragonAtBeat(caller.startBeat + caller.timer);
            SoundByte.PlayOneShot("miss");
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}

