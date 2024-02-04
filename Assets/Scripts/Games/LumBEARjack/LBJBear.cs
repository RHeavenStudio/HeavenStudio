using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJBear : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator _anim;

        private bool _rested = false;
        private bool _restSound = true;

        public void SwingWhiff(bool sound = true)
        {
            if (_rested) return;
            if (sound) SoundByte.PlayOneShotGame("lumbearjack/swing", -1, SoundByte.GetPitchFromCents(Random.Range(-200, 201), false));
            _anim.DoScaledAnimationAsync("BeastWhiff", 0.75f);
        }

        public void Cut(double beat, bool huh, bool huhL)
        {
            _anim.DoScaledAnimationAsync(huh ? "BeastHalfCut" : "BeastCut", 0.75f);
            if (!huh) return;
            BeatAction.New(this, new()
            {
                new(beat + 1, delegate
                {
                    _anim.DoScaledAnimationAsync(huhL ? "BeastHuhL" : "BeastHuhR", 0.5f);
                }),
                new(beat + 2, delegate
                {
                    _anim.DoScaledAnimationAsync("BeastReady", 0.75f);
                })
            });
        }

        public void CutMid()
        {
            _anim.DoScaledAnimationAsync("BeastCutMid", 0.75f);
        }

        public void Bop()
        {
            if (_anim.IsPlayingAnimationNames("BeastWhiff", "BeastRest") || _rested) return;
            _anim.DoScaledAnimationAsync("BeastBop", 0.5f);
        }

        public void Rest(bool instant, bool sound)
        {
            _anim.DoScaledAnimationAsync("BeastRest", 0.5f, instant ? 1 : 0);
            _rested = true;
        }

        public void RestSound()
        {
            if (_restSound) SoundByte.PlayOneShotGame("lumbearjack/sigh" + (Random.Range(1, 3) == 1 ? "A" : "B"));
        }
    }

}
