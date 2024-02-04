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

        public void SwingWhiff(bool sound = true, bool force = false)
        {
            if (_anim.IsPlayingAnimationNames("BeastWhiff", "BeastCut", "BeastCutMid", "BeastHalfCut") && !force) return;
            if (sound) SoundByte.PlayOneShotGame("lumbearjack/swing", -1, SoundByte.GetPitchFromCents(Random.Range(-200, 201), false));
            _anim.DoScaledAnimationAsync("BeastWhiff", 0.5f);
        }

        public void Cut(double beat, bool huh, bool huhL)
        {
            _anim.DoScaledAnimationAsync(huh ? "BeastHalfCut" : "BeastCut", 0.5f);
            if (!huh) return;
            BeatAction.New(this, new()
            {
                new(beat + 1, delegate
                {
                    _anim.DoScaledAnimationAsync(huhL ? "BeastHuhL" : "BeastHuhR", 0.5f);
                })
            });
        }

        public void CutMid()
        {
            _anim.DoScaledAnimationAsync("BeastCutMid", 0.5f);
        }

        public void Bop()
        {
            if (_anim.IsPlayingAnimationNames("BeastWhiff")) return;
            _anim.DoScaledAnimationAsync("BeastBop", 0.5f);
        }
    }

}
