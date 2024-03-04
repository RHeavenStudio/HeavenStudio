using HeavenStudio.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public class GiraffeInput : MonoBehaviour
    {
        [SerializeField] private Animator _monkey;
        [SerializeField] private ParticleSystem _holdParticle;
        private AcrobatObstacle _mainScript;

        private AnimalAcrobat _game;

        private Sound _drumRollSound;

        private PlayerActionEvent _releaseAction;

        private void Awake()
        {
            _game = AnimalAcrobat.instance;
            _mainScript = GetComponent<AcrobatObstacle>();
            _mainScript.OnInit += Init;
        }

        private void OnDestroy()
        {
            if (_drumRollSound != null) _drumRollSound.KillLoop(0.587);
        }

        public void Init(double beat, bool beforeGiraffe)
        {
            _game.ScheduleInput(beat - 1, 1, Minigame.InputAction_BasicPress, beforeGiraffe ? JustHoldGiraffe : JustHold, Miss, Empty);
            _releaseAction = _game.ScheduleInput(beat, 4, Minigame.InputAction_FlickRelease, JustRelease, Miss, Empty);
            _monkey.gameObject.SetActive(false);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("animalAcrobat/eek", beat + 2),
                new MultiSound.Sound("animalAcrobat/eek", beat + 3)
            });
        }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/catch");
            _monkey.gameObject.SetActive(true);
            _monkey.DoScaledAnimationAsync("PlayerHang", 1f, 0.4f);
            _game.PlayerSetActive(false);
            SpawnParticle();
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(0, delegate {}),
                new BeatAction.Action(caller.startBeat + caller.timer + 2, delegate
                {
                    _monkey.DoScaledAnimationAsync("PlayerHanging", 0.5f);
                })
            });
        }

        private void JustHoldGiraffe(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/giraffeCatch");
            _monkey.gameObject.SetActive(true);
            _monkey.DoScaledAnimationAsync("PlayerHang", 1f, 0.4f);
            _game.PlayerSetActive(false);
            SpawnParticle();
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(0, delegate {}),
                new BeatAction.Action(caller.startBeat + caller.timer + 2, delegate
                {
                    _monkey.DoScaledAnimationAsync("PlayerHanging", 0.25f);
                })
            });
        }

        private void SpawnParticle()
        {
            ParticleSystem spawnedParticle = Instantiate(_holdParticle, transform);
            spawnedParticle.transform.position = _holdParticle.transform.position;
            spawnedParticle.Play();
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/giraffeJump");
            _monkey.gameObject.SetActive(false);
            _game.PlayerJump(caller.startBeat + caller.timer, true);

            double beat = caller.startBeat + caller.timer;

            _drumRollSound = SoundByte.PlayOneShotGame("animalAcrobat/giraffeDrumroll", beat, 1, 1, true);
            GiraffeDrumRollLoop(beat);

            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("animalAcrobat/giraffeCymbal", beat + 2),
                new MultiSound.Sound("animalAcrobat/applause", beat + 2.25),
                new MultiSound.Sound("animalAcrobat/turn", beat + 3)
            });
        }

        private void GiraffeDrumRollLoop(double beat)
        {
            _drumRollSound.LerpVolume(beat, 1, 1, 0.125f);

            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 1, delegate
                {
                    _drumRollSound.LerpVolume(beat + 1, 3, 0.125f, 1);
                }),
                new BeatAction.Action(beat + 4, delegate
                {
                    _drumRollSound.KillLoop(0.587);
                })
            });
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (_game.MonkeyMissed) return;
            SoundByte.PlayOneShotGame("animalAcrobat/miss");
            _releaseAction?.Disable();
            _releaseAction?.QueueDeletion();
            _game.MonkeyMissed = true;
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}
