using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class ObstacleInput : MonoBehaviour
    {
        [SerializeField] private Animator _monkey;
        [SerializeField] private double _holdLength;

        private AcrobatObstacle _mainScript;

        private AnimalAcrobat _game;

        private void Awake()
        {
            _game = AnimalAcrobat.instance;
            _mainScript = GetComponent<AcrobatObstacle>();
            _mainScript.OnInit += Init;
        }

        public void Init(double beat, bool beforeGiraffe)
        {
            _game.ScheduleInput(beat - 1, 1, InputType.STANDARD_DOWN, beforeGiraffe ? JustHoldGiraffe : JustHold, Empty, Empty);
            _game.ScheduleInput(beat, _holdLength, InputType.STANDARD_UP, JustRelease, Empty, Empty);
            _monkey.gameObject.SetActive(false);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("animalAcrobat/eek", beat + _holdLength - 2),
                new MultiSound.Sound("animalAcrobat/eek", beat + _holdLength - 1)
            });
        }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/catch");
            _monkey.gameObject.SetActive(true);
            _monkey.DoScaledAnimationAsync("PlayerHang", 0.75f);
            _game.PlayerSetActive(false);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(0, delegate {}),
                new BeatAction.Action(caller.startBeat + caller.timer + (_holdLength * 0.5), delegate
                {
                    _monkey.DoScaledAnimationAsync("PlayerHanging", 0.5f);
                })
            });
        }

        private void JustHoldGiraffe(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/giraffeCatch");
            _monkey.gameObject.SetActive(true);
            _monkey.DoScaledAnimationAsync("PlayerHang", 0.75f);
            _game.PlayerSetActive(false);
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(0, delegate {}),
                new BeatAction.Action(caller.startBeat + caller.timer + (_holdLength * 0.5), delegate
                {
                    _monkey.DoScaledAnimationAsync("PlayerHanging", 0.5f);
                })
            });
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/release");

            SoundByte.PlayOneShotGame("animalAcrobat/turn", caller.startBeat + caller.timer + 1);

            _monkey.gameObject.SetActive(false);
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}

