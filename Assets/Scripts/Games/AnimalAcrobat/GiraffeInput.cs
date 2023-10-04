using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class GiraffeInput : MonoBehaviour
    {
        private AcrobatObstacle _mainScript;

        private AnimalAcrobat _game;

        private Sound _drumRollSound;

        private void Awake()
        {
            _game = AnimalAcrobat.instance;
            _mainScript = GetComponent<AcrobatObstacle>();
            _mainScript.OnInit += Init;
        }

        private void OnDestroy()
        {
            if (_drumRollSound != null) _drumRollSound.Stop();
        }

        public void Init(double beat, bool beforeGiraffe)
        {
            _game.ScheduleInput(beat - 1, 1, InputType.STANDARD_DOWN, beforeGiraffe ? JustHoldGiraffe : JustHold, Empty, Empty);
            _game.ScheduleInput(beat, 4, InputType.STANDARD_UP, JustRelease, Empty, Empty);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("animalAcrobat/eek", beat + 2),
                new MultiSound.Sound("animalAcrobat/eek", beat + 3)
            });
        }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/catch");
        }

        private void JustHoldGiraffe(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/giraffeCatch");
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/giraffeJump");

            double beat = caller.startBeat + caller.timer;

            _drumRollSound = SoundByte.PlayOneShotGame("animalAcrobat/giraffeDrumroll", beat, 1, 1, true);
            StartCoroutine(GiraffeDrumRollLoop(beat));

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("animalAcrobat/giraffeCymbal", beat + 2),
                new MultiSound.Sound("animalAcrobat/applause", beat + 2.25),
                new MultiSound.Sound("animalAcrobat/turn", beat + 3)
            });
        }

        private IEnumerator GiraffeDrumRollLoop(double beat)
        {
            float fadeOutNormalized = 0;
            var cond = Conductor.instance;
            while (fadeOutNormalized <= 1)
            {
                fadeOutNormalized = cond.GetPositionFromBeat(beat, 1);
                _drumRollSound.SetVolume(Mathf.Lerp(1, 0.125f, fadeOutNormalized));
                yield return null;
            }

            float fadeInNormalized = 0;

            while (fadeInNormalized <= 1)
            {
                fadeInNormalized = cond.GetPositionFromBeat(beat + 1, 3);
                _drumRollSound.SetVolume(Mathf.Lerp(0.125f, 1, fadeOutNormalized));
                yield return null;
            }

            float fadeOutFinal = 0;
            double fadeOutLength = cond.SecsToBeats(0.587, cond.GetBpmAtBeat(beat + 4));

            while (fadeOutFinal <= 1)
            {
                fadeOutFinal = cond.GetPositionFromBeat(beat + 4, fadeOutLength);
                _drumRollSound.SetVolume(Mathf.Lerp(1, 0, fadeOutNormalized));
                yield return null;
            }
            _drumRollSound.Stop();
            _drumRollSound = null;
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}
