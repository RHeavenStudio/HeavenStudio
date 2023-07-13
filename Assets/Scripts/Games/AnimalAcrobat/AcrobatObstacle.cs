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

        private void Awake()
        {
            halfAngle = fullRotateAngle / 2;
            func = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutQuad);
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
            if ((Mathf.Floor(normalizedSwingBeat) + negativeOffset) % 2 == 0)
            {
                rotatePivot.localEulerAngles = new Vector3(0, 0, func(-halfAngle, halfAngle, normalizedAdjusted));
                if (type == ObstacleType.Monkeys) anim.DoNormalizedAnimation("WhiteMonkeysSwing", normalizedAdjusted);
            }
            else
            {
                rotatePivot.localEulerAngles = new Vector3(0, 0, func(halfAngle, -halfAngle, normalizedAdjusted));
                if (type == ObstacleType.Monkeys) anim.DoNormalizedAnimation("WhiteMonkeysSwing", 1 - normalizedAdjusted);
            }
        }
    }
}


