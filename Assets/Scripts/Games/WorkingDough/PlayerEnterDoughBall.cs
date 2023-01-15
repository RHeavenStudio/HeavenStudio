using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_WorkingDough
{
    public class PlayerEnterDoughBall : PlayerActionObject
    {
        public float startBeat;
        public bool isBig;
        public enum Rating
        {
            Succesful = 0,
            Barely = 1,
            Miss = 2
        }

        public enum PlayerFlyingStage
        {
            EnteringUp = 0,
            EnteringDown = 1,
            ExitingUp = 2,
            ExitingDown = 3,
        }
        public PlayerFlyingStage currentFlyingStage;
        public Rating rating;

        [NonSerialized] public BezierCurve3D enterUpCurve;
        [NonSerialized] public BezierCurve3D enterDownCurve;
        [NonSerialized] public BezierCurve3D exitUpCurve;
        [NonSerialized] public BezierCurve3D exitDownCurve;

        private WorkingDough game;

        private void Awake()
        {
            game = WorkingDough.instance;
        }

        private void Start()
        {
            game.ScheduleInput(startBeat, 1, isBig ? InputType.STANDARD_ALT_DOWN : InputType.STANDARD_DOWN, Barely, Miss, Nothing);
            Debug.Log(rating.ToString());
        }

        private void Update()
        {
            var cond = Conductor.instance;

            float flyPos = 0f;

            switch (currentFlyingStage)
            {
                case PlayerFlyingStage.EnteringUp:
                    flyPos = cond.GetPositionFromBeat(startBeat, 0.5f);
                    transform.position = enterUpCurve.GetPoint(flyPos);
                    if (flyPos > 1f) currentFlyingStage = PlayerFlyingStage.EnteringDown;
                    break;
                case PlayerFlyingStage.EnteringDown:
                    flyPos = cond.GetPositionFromBeat(startBeat + 0.5f, 0.5f);

                    transform.position = enterDownCurve.GetPoint(flyPos);
                    if (flyPos > 1f)
                    {
                        Debug.Log(rating.ToString());
                        if (rating == Rating.Succesful || rating == Rating.Barely)
                        {
                            currentFlyingStage = PlayerFlyingStage.ExitingUp;
                        }
                        else
                        {
                            GameObject.Destroy(gameObject);
                        }
                    }
                    break;
                case PlayerFlyingStage.ExitingUp:
                    flyPos = cond.GetPositionFromBeat(startBeat + 1f, 0.5f);

                    transform.position = exitUpCurve.GetPoint(flyPos);
                    if (flyPos > 1f) currentFlyingStage = PlayerFlyingStage.ExitingDown;
                    break;
                case PlayerFlyingStage.ExitingDown:
                    flyPos = cond.GetPositionFromBeat(startBeat + 1.5f, 0.5f);

                    transform.position = exitDownCurve.GetPoint(flyPos);
                    if (flyPos > 1f) GameObject.Destroy(gameObject);
                    break;
            }
        }

        private void Success()
        {
            rating = Rating.Succesful;
            if (isBig)
            {
                game.doughDudesPlayer.GetComponent<Animator>().Play("BigDoughJump", 0, 0);
                Jukebox.PlayOneShotGame("workingDough/rightBig");
            }
            else
            {
                game.doughDudesPlayer.GetComponent<Animator>().Play("SmallDoughJump", 0, 0);
                Jukebox.PlayOneShotGame("workingDough/rightSmall");
            }
        }

        private void Barely(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                rating = Rating.Barely;
                return;
            }
            Success();
        }

        private void Miss(PlayerActionEvent caller)
        {
            rating = Rating.Miss;
        }

        private void Nothing(PlayerActionEvent caller) {}
    }
}


