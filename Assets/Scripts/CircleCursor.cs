using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using HeavenStudio.Games;

namespace HeavenStudio
{
    public class CircleCursor : MonoBehaviour
    {
        [SerializeField] private bool follow = false;
        [SerializeField] private float mouseMoveSpeed;

        [Header("DSGuy")]
        [SerializeField] private GameObject Eyes;
        [SerializeField] private GameObject OuterCircle;
        public GameObject InnerCircle;
        [SerializeField] private GameObject Circle;
        private Tween outerCircleTween, eyesTween;

        private void Start()
        {
            // Cursor.visible = false;
        }

        private void Update()
        {
            Vector3 pos = PlayerInput.GetInputController(1).GetPointer();

            if (follow)
            {
                Vector2 direction = (pos - transform.position).normalized;
                this.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x * mouseMoveSpeed, direction.y * mouseMoveSpeed);
            }
            else
            {
                this.gameObject.transform.position = new Vector3(pos.x, pos.y, 0);

                if (PlayerInput.GetIsAction(Minigame.InputAction_BasicPress))
                {
                    Circle.transform.DOScale(0, 0.5f).SetEase(Ease.OutExpo);
                    InnerCircle.SetActive(true);
                    outerCircleTween.Kill();
                    outerCircleTween = OuterCircle.transform.DOScale(1, 0.15f).SetEase(Ease.OutExpo);

                    Eyes.SetActive(true);
                    eyesTween.Kill();
                    eyesTween = Eyes.transform.DOLocalMoveY(0.15f, 0.15f).SetEase(Ease.OutExpo);
                }
                else if (PlayerInput.GetIsAction(Minigame.InputAction_BasicRelease))
                {
                    Circle.transform.DOScale(0.2f, 0.5f).SetEase(Ease.OutExpo);
                    InnerCircle.SetActive(false);
                    outerCircleTween.Kill();
                    outerCircleTween = OuterCircle.transform.DOScale(0, 0.15f);

                    eyesTween.Kill();
                    eyesTween = Eyes.transform.DOLocalMoveY(-0.66f, 0.15f).OnComplete(delegate { Eyes.SetActive(false); });
                }
            }
        }
    }
}