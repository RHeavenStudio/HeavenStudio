using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_AnimalAcrobat
{
    public enum ObstacleType
    {
        Elephant,
        Giraffe,
        Monkeys,
        Monkey
    }
    public class AcrobatObstacle : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private float holdLength = 2f;
        [SerializeField] private float fullRotateAngle = 120f;
        [SerializeField] private Animator anim;
        [SerializeField] private ObstacleType type;
        [SerializeField] private Transform rotatePivot;
        private double startBeat = double.MinValue;
        private float halfAngle;
        private EasingFunction.Function func;
        private AnimalAcrobat game;

        private void Awake()
        {
            halfAngle = fullRotateAngle / 2;
            func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutQuad);
            game = AnimalAcrobat.instance;
        }

        public void Init(double beat, double gameSwitchBeat)
        {
            startBeat = beat;
            MultiSound.Play(new MultiSound.Sound[]
            {
                new MultiSound.Sound("animalAcrobat/eek", startBeat + holdLength - 2),
                new MultiSound.Sound("animalAcrobat/eek", startBeat + holdLength - 1),
            });
            if (type is ObstacleType.Elephant or ObstacleType.Giraffe)
            {
                bool isElephant = type is ObstacleType.Elephant;
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat + holdLength - 2, delegate
                    {
                        anim.DoScaledAnimationAsync(isElephant ? "ElephantEar" : "GiraffeEar", 0.5f);
                    }),
                    new BeatAction.Action(startBeat + holdLength - 1, delegate
                    {
                        anim.DoScaledAnimationAsync(isElephant ? "ElephantEar" : "GiraffeEar", 0.5f);
                    }),
                });
            }
            game.ScheduleInput(beat - 1, 1, InputType.STANDARD_DOWN, JustHold, Empty, Empty);
            Update();
        }

        private void Update()
        {
            var cond = Conductor.instance;
            if (startBeat != double.MinValue)
            {
                SwingUpdate(cond);
            }
        }

        private void SwingUpdate(Conductor cond)
        {
            float normalizedSwingBeat = cond.GetPositionFromBeat(startBeat, holdLength);
            float negativeOffset = (normalizedSwingBeat < 0) ? -1 : 0;
            float normalizedAdjusted = Mathf.Abs(normalizedSwingBeat % 1);
            bool goingRight = (Mathf.Floor(normalizedSwingBeat) + negativeOffset) % 2 == 0;
            float dirMult = goingRight ? 1 : -1;
            rotatePivot.localEulerAngles = new Vector3(0, 0, func(-halfAngle * dirMult, halfAngle * dirMult, normalizedAdjusted));
            if (type == ObstacleType.Monkeys) anim.DoNormalizedAnimation("WhiteMonkeysSwing", normalizedAdjusted);
        }

        private void JustHold(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("animalAcrobat/grab");
            game.ScheduleInput(startBeat, holdLength, InputType.STANDARD_UP, JustRelease, Empty, Empty);
        }

        private void JustRelease(PlayerActionEvent caller, float state)
        {
            double beat = caller.startBeat + caller.timer;
            switch (type)
            {
                case ObstacleType.Giraffe:
                    SoundByte.PlayOneShotGame("animalAcrobat/giraffeRelease");
                    SoundByte.PlayOneShotGame("animalAcrobat/giraffeReleaseLoop", beat, 1, 1, true).SetLoopParams(beat + 4, 0);
                    MultiSound.Play(new MultiSound.Sound[] 
                    {
                        new MultiSound.Sound("animalAcrobat/flip", beat + 3),
                        new MultiSound.Sound("cymbal", beat + 4)
                    });
                    break;
                default:
                    SoundByte.PlayOneShotGame("animalAcrobat/release");
                    SoundByte.PlayOneShotGame("animalAcrobat/flip", beat + 1);
                    break;
            }
        }

        private void Empty(PlayerActionEvent caller) { }
    }
}


