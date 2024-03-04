using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class WhiteMonkeysSwing : MonoBehaviour
    {
        [SerializeField] private Animator _anim;
        [SerializeField] private float _animLength = 0.6f;
        [SerializeField] private double _beatLength = 3;
        [SerializeField] private EasingFunction.Ease _ease = EasingFunction.Ease.EaseInOutQuad;
        private AcrobatObstacle _mainScript;

        private double _startBeat;
        private EasingFunction.Function _func;

        private void Awake()
        {
            _mainScript = GetComponent<AcrobatObstacle>();
            _mainScript.OnInit += Init;
            _func = EasingFunction.GetEasingFunction(_ease);
        }

        private void Init(double beat, bool _beforeGiraffe)
        {
            _startBeat = beat;
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (!cond.isPlaying) return;

            float normalized = cond.GetPositionFromBeat(_startBeat, _beatLength);
            _anim.DoNormalizedAnimation("WhiteMonkeysSwing", _func(0, _animLength, Mathf.Clamp01(normalized)));
        }
    }
}

