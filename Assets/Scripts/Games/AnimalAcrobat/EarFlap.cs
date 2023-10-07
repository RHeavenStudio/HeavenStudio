using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class EarFlap : MonoBehaviour
    {
        [SerializeField] private Animator _anim;
        [SerializeField] private string _animName;
        [SerializeField] private double _holdLength;
        private AcrobatObstacle _mainScript;

        private void Awake()
        {
            _mainScript = GetComponent<AcrobatObstacle>();
            _mainScript.OnInit += Init;
        }

        private void Init(double beat, bool unused)
        {
            BeatAction.New(this, new()
            {
                new(0, delegate
                {
                    //nasty hack, why does beataction do this?
                }),
                new(beat + _holdLength - 2, delegate
                {
                    _anim.DoScaledAnimationAsync(_animName, 0.5f);
                }),
                new(beat + _holdLength - 1, delegate
                {
                    _anim.DoScaledAnimationAsync(_animName, 0.5f);
                })
            });
        }
    }
}

