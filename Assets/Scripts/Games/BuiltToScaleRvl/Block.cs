using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_BuiltToScaleRvl
{
    using HeavenStudio.Util;
    public class Block : MonoBehaviour
    {
        public int position;
        private Animator blockAnim;

        [System.NonSerialized] public bool isOpen = false;
        [System.NonSerialized] public bool isPrepare = false;
        private double closeBeat = double.MinValue, shootBeat = double.MinValue;

        private Vector3 _thisPosition;
        private Vector3 _otherPosition;
        [SerializeField] private float _slideOffset;

        private Coroutine _currentMove;

        private BuiltToScaleRvl game;

        private void Awake()
        {
            _thisPosition = transform.localPosition;
            _otherPosition = _thisPosition + new Vector3(0, _slideOffset);
            game = BuiltToScaleRvl.instance;
            blockAnim = GetComponent<Animator>();
            if (!BuiltToScaleRvl.IsPositionInRange(position)) position = 0;
        }

        public void Move(double beat, double length, bool inToScene, int ease)
        {
            if (_currentMove != null) StopCoroutine(_currentMove);
            _currentMove = StartCoroutine(MoveCo(beat, length, inToScene, ease));
        }

        private IEnumerator MoveCo(double beat, double length, bool inToScene, int ease)
        {
            if (length <= 0)
            {
                transform.localPosition = inToScene ? _thisPosition : _otherPosition;
                yield break;
            }
            float normalized = Conductor.instance.GetPositionFromBeat(beat, length, false);
            Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease)ease);
            while (normalized <= 1f)
            {
                normalized = Conductor.instance.GetPositionFromBeat(beat, length);
                float newPosY = func(inToScene ? _otherPosition.y : _thisPosition.y, inToScene ? _thisPosition.y : _otherPosition.y, normalized);
                transform.localPosition = new Vector3(_thisPosition.x, newPosY, _thisPosition.z);
                yield return null;
            }
        }

        public void Bounce(double beat)
        {
            SoundByte.PlayOneShotGame(position switch {
                0 => "builtToScaleRvl/left",
                1 => "builtToScaleRvl/middleLeft",
                2 => "builtToScaleRvl/middleRight",
                3 => "builtToScaleRvl/right",
                _ => throw new System.NotImplementedException()
            });
            blockAnim.Play("bounce", 0, 0);
            if (closeBeat < beat) closeBeat = beat;
        }
        public void BounceNearlyMiss()
        {
            blockAnim.Play("open", 0, 0);
        }
        public void BounceMiss()
        {
            if (isOpen) {
                blockAnim.Play("miss_open", 0, 0);
            } else {
                blockAnim.Play("miss", 0, 0);
            }
        }

        public void Prepare(double beat)
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/playerRetract");
            if (PlayerInput.CurrentControlStyle is InputSystem.InputController.ControlStyles.Pad) {
                blockAnim.Play("prepare B", 0, 0);
            } else {
                blockAnim.Play("prepare AB", 0, 0);
            }
            isOpen = false; 
            isPrepare = true;
            shootBeat = beat;
        }
        public void Shoot()
        {
            SoundByte.PlayOneShotGame("builtToScaleRvl/shoot");
            blockAnim.Play("shoot", 0, 0);
            isPrepare = false;
        }
        public void ShootNearlyMiss()
        {
            if (PlayerInput.CurrentControlStyle is InputSystem.InputController.ControlStyles.Pad) {
                blockAnim.Play("shoot miss B", 0, 0);
            } else {
                blockAnim.Play("shoot miss AB", 0, 0);
            }
            isPrepare = false;
        }
        public void ShootMiss()
        {
            if (!isPrepare) return;
            if (PlayerInput.CurrentControlStyle is InputSystem.InputController.ControlStyles.Pad) {
                blockAnim.Play("shoot miss B", 0, 0);
            } else {
                blockAnim.Play("shoot miss AB", 0, 0);
            }
            isPrepare = false;
        }
        
        public void Open()
        {
            if (isPrepare) return;
            blockAnim.Play("open", 0, 0);
            isOpen = true;
        }
        public void Idle(double beat = double.MinValue)
        {
            if (closeBeat > beat || shootBeat >= beat) return;
            blockAnim.Play("idle", 0, 0);
            isOpen = false;
        }
    }
}