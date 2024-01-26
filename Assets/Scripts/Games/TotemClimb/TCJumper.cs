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
        [SerializeField] private ParticleSystem _highParticle;
        [SerializeField] private ParticleSystem _highMissParticle;
        [SerializeField] private ParticleSystem _jumpParticle;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _jumpHeightTriple = 1f;
        [SerializeField] private float _jumpHighHeight = 6f;

        private Path _path;
        private Animator _anim;
        private TotemClimb _game;
        private double _startBeat;
        private double _onPlayBeat;

        private IEnumerator _currentStateCo;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _game = TotemClimb.instance;
        }

        public void InitPath(double beat, double onPlayBeat)
        {
            _onPlayBeat = onPlayBeat;
            _startBeat = beat;
            _path = new Path();
            _path.positions = new PathPos[2];
            _path.positions[0] = new PathPos()
            {
                duration = 1 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat)),
                target = _initialPoint,
                height = _jumpHeight
            };
            if (_game.IsHighBeat(beat))
            {
                _path.positions[1] = new PathPos()
                {
                    target = _game.GetDragonPointAtBeat(beat)
                };
            }
            else
            {
                _path.positions[1] = new PathPos()
                {
                    target = _game.IsTripleBeat(beat) ? _game.GetJumperFrogPointAtBeat(beat, -1) : _game.GetJumperPointAtBeat(beat)
                };
            }
        }

        public void StartJumping(double beat, bool miss = false, bool nearMiss = false)
        {
            bool nextIsTriple = _game.IsTripleBeat(beat + 1);
            bool nextIsHigh = _game.IsHighBeat(beat + 1);
            SwitchToNextState(JumpCo(beat, miss, nearMiss));
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
            SwitchToNextState(JumpHighCo(beat, miss));
            if (beat + 2 >= _game.EndBeat) return;
            if (nextIsHigh)
            {
                _game.ScheduleInput(beat, 2, Minigame.InputAction_BasicPress, JustHold, Empty, Empty);
                _game.ScheduleInput(beat, 4, Minigame.InputAction_FlickRelease, JustRelease, MissRelease, Empty);
            }
            else _game.ScheduleInput(beat, 2, Minigame.InputAction_BasicPress, nextIsTriple ? JustTripleEnter : Just, nextIsTriple ? MissTripleEnter : Miss, Empty);
        }

        public void TripleJumping(double beat, bool enter, bool miss = false, bool nearMiss = false)
        {
            SwitchToNextState(JumpTripleCo(beat, enter, miss, nearMiss));
            if (beat + 0.5 >= _game.EndBeat) return;
            _game.ScheduleInput(beat, 0.5, Minigame.InputAction_BasicPress, enter ? JustTripleExit : Just, enter ? MissTripleExit : Miss, Empty);
        }

        public void Bop()
        {
            _anim.DoScaledAnimationAsync("Bop", 0.5f);
        }

        private IEnumerator JumpCo(double beat, bool miss, bool nearMiss)
        {
            if (beat >= _startBeat)
            {
                _path = new Path();
                _path.positions = new PathPos[2];
                _path.positions[0] = new PathPos()
                {
                    duration = (_game.EndBeat <= beat + 1) ? 1 : 1 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat + 1)),
                    height = _jumpHeight,
                    target = _game.IsTripleBeat(beat) ? _game.GetJumperFrogPointAtBeat(beat, 1) : _game.GetJumperPointAtBeat(beat)
                };
                
                if (beat + 1 >= _onPlayBeat && _game.EndBeat <= beat + 1 && _game.UseEndTotem)
                {
                    _path.positions[1] = new PathPos()
                    {
                        target = _game.EndJumperPoint
                    };
                }
                else if (_game.IsHighBeat(beat + 1))
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
            if (!miss) _anim.DoScaledAnimationAsync(nearMiss ? "NearMiss" : "Jump", 0.5f);
            else _anim.DoScaledAnimationAsync("Miss", 0.5f);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while(normalizedBeat < 1f)
            {
                transform.position = GetPathPositionFromBeat(_path, Math.Clamp(Conductor.instance.songPositionInBeatsAsDouble, beat, beat + _path.positions[0].duration), beat);

                if (normalizedBeat >= 0.5f && !playedFall)
                {
                    if (!miss && !nearMiss) _anim.Play("Fall", 0, 0);
                    playedFall = true;
                }
                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
                yield return null;
            }
            transform.position = GetPathPositionFromBeat(_path, beat + _path.positions[0].duration, beat);
            _anim.Play("Idle", 0, 0);
            if (beat + 1 >= _onPlayBeat && _game.EndBeat <= beat + 1)
            {
                SoundByte.PlayOneShotGame("totemClimb/totemland");
                if (_game.UseEndTotem)
                {
                    _game.DoEndTotemEvents(beat + 1);
                    gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator JumpTripleCo(double beat, bool enter, bool miss, bool nearMiss)
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
            if (!miss) _anim.DoScaledAnimationAsync(nearMiss ? "NearMiss" : "Jump", 0.5f);
            else _anim.DoScaledAnimationAsync("Miss", 0.5f);

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while (normalizedBeat < 1f)
            {
                transform.position = GetPathPositionFromBeat(_path, Math.Clamp(Conductor.instance.songPositionInBeatsAsDouble, beat, beat + _path.positions[0].duration), beat);

                if (normalizedBeat >= 0.5f && !playedFall)
                {
                    if (!miss && !nearMiss) _anim.Play("Fall", 0, 0);
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
                    duration = (_game.EndBeat <= beat + 2) ? 2 : 2 - (float)Conductor.instance.SecsToBeats(Minigame.justEarlyTime, Conductor.instance.GetBpmAtBeat(beat + 2)),
                    height = _jumpHighHeight,
                    target = _game.GetDragonPointAtBeat(beat)
                };

                if (beat + 2 >= _onPlayBeat && _game.EndBeat <= beat + 2 && _game.UseEndTotem)
                {
                    _path.positions[1] = new PathPos()
                    {
                        target = _game.EndJumperPoint
                    };
                }
                else if (_game.IsHighBeat(beat + 2))
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
            if (!miss)
            {
                _anim.Play("HighJump", 0, 0);
                _highParticle.PlayScaledAsync(0.5f);
            }
            else
            {
                _anim.DoScaledAnimationAsync("HighMiss", 0.5f);
                _highMissParticle.PlayScaledAsync(0.5f);
            }

            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
            bool playedFall = false;

            while (normalizedBeat < 1f)
            {
                transform.position = GetPathPositionFromBeat(_path, Math.Clamp(Conductor.instance.songPositionInBeatsAsDouble, beat, beat + _path.positions[0].duration), beat);

                if (normalizedBeat >= 0.5f && !playedFall)
                {
                    if (!miss) _anim.Play("HighFall", 0, 0);
                    playedFall = true;
                }

                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, _path.positions[0].duration);
                yield return null;
            }
            transform.position = GetPathPositionFromBeat(_path, beat + _path.positions[0].duration, beat);
            _anim.Play("Idle", 0, 0);
            _highParticle.Stop();
            _highMissParticle.Stop();
            if (beat + 2 >= _onPlayBeat && _game.EndBeat <= beat + 2)
            {
                SoundByte.PlayOneShotGame("totemClimb/totemland");
                if (_game.UseEndTotem)
                {
                    _game.DoEndTotemEvents(beat + 2);
                    gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator HoldCo(double beat)
        {
            float normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, 2);
            Transform dragonPoint = _game.GetDragonPointAtBeat(beat);
            bool canUnHold = true;
            while (normalizedBeat < 1f)
            {
                normalizedBeat = Conductor.instance.GetPositionFromBeat(beat, 2);
                transform.position = dragonPoint.position;

                if (!_game.IsExpectingInputNow(Minigame.InputAction_FlickRelease) 
                    && (PlayerInput.GetIsAction(Minigame.InputAction_FlickRelease) || PlayerInput.GetIsAction(Minigame.InputAction_BasicRelease))
                    && canUnHold)
                {
                    _anim.DoScaledAnimationAsync("UnHold", 0.5f);
                    _game.ScoreMiss();
                    canUnHold = false;
                }
                if (PlayerInput.GetIsAction(Minigame.InputAction_BasicPress) && !canUnHold)
                {
                    _anim.DoScaledAnimationAsync("Hold", 0.5f);
                    SoundByte.PlayOneShot("nearMiss");
                    _game.ScoreMiss();
                    canUnHold = true;
                }
                yield return null;
            }
        }

        private void SwitchToNextState(IEnumerator coroutine)
        {
            _highParticle.Stop();
            _highMissParticle.Stop();
            if (_currentStateCo != null) StopCoroutine(_currentStateCo);
            _currentStateCo = coroutine;
            StartCoroutine(_currentStateCo);
        }

        private void Just(PlayerActionEvent caller, float state)
        {
            bool isTriple = _game.IsTripleBeat(caller.startBeat + caller.timer);
            StartJumping(caller.startBeat + caller.timer, false, state >= 1f || state <= -1f);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
            if (isTriple) _game.FallFrogAtBeat(caller.startBeat + caller.timer, 1);
            if (state >= 1f || state <= -1f)
            {
                if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("nearMiss");
                return;
            }
            _jumpParticle.PlayScaledAsync(0.5f);
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShotGame(isTriple ? "totemClimb/totemlandb" : "totemClimb/totemland");
        }

        private void JustTripleEnter(PlayerActionEvent caller, float state)
        {
            TripleJumping(caller.startBeat + caller.timer, true, false, state >= 1f || state <= -1f);
            _game.FallFrogAtBeat(caller.startBeat + caller.timer, -1);
            if (state >= 1f || state <= -1f)
            {
                if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("nearMiss");
                return;
            }
            _jumpParticle.PlayScaledAsync(0.5f);
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShotGame("totemClimb/totemland");
        }

        private void JustTripleExit(PlayerActionEvent caller, float state)
        {
            TripleJumping(caller.startBeat + caller.timer, false, false, state >= 1f || state <= -1f);
            _game.FallFrogAtBeat(caller.startBeat + caller.timer, 0);
            if (state >= 1f || state <= -1f)
            {
                if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("nearMiss");
                return;
            }
            _jumpParticle.PlayScaledAsync(0.5f);
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShotGame("totemClimb/totemland");
        }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShotGame("totemClimb/chargejump");
            _game.HoldDragonAtBeat(caller.startBeat + caller.timer);
            SwitchToNextState(HoldCo(caller.startBeat + caller.timer));
            _anim.DoScaledAnimationAsync("Hold", 0.5f);
            if (state >= 1f || state <= -1f)
            {
                if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("nearMiss");
                return;
            }
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            HighJump(caller.startBeat + caller.timer, state >= 1f && state <= -1f);
            _game.ReleaseDragonAtBeat(caller.startBeat + caller.timer);
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShotGame("totemClimb/superjumpgood");
            if (state >= 1f || state <= -1f)
            {
                if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("nearMiss");
                return;
            }
            _jumpParticle.PlayScaledAsync(0.5f);
        }

        private void Miss(PlayerActionEvent caller)
        {
            StartJumping(caller.startBeat + caller.timer, true);
            _game.BopTotemAtBeat(caller.startBeat + caller.timer);
            if (_game.IsTripleBeat(caller.startBeat + caller.timer)) _game.FallFrogAtBeat(caller.startBeat + caller.timer, 1);
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("miss");
        }

        private void MissTripleEnter(PlayerActionEvent caller)
        {
            TripleJumping(caller.startBeat + caller.timer, true, true);
            _game.FallFrogAtBeat(caller.startBeat + caller.timer, -1);
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("miss");
        }

        private void MissTripleExit(PlayerActionEvent caller)
        {
            TripleJumping(caller.startBeat + caller.timer, false, true);
            _game.FallFrogAtBeat(caller.startBeat + caller.timer, 0);
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("miss");
        }

        private void MissRelease(PlayerActionEvent caller)
        {
            HighJump(caller.startBeat + caller.timer, true);
            _game.ReleaseDragonAtBeat(caller.startBeat + caller.timer);
            if (caller.startBeat + caller.timer >= _onPlayBeat) SoundByte.PlayOneShot("miss");
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}

