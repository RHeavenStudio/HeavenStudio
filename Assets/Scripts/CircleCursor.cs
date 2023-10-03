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
        [SerializeField] private GameObject DSGuy;
        [SerializeField] private Animator DSGuyAnimator;
        [SerializeField] private float flickCoeff = 0.35f;
        [SerializeField] private float flickInitMul = 1.5f;
        public GameObject InnerCircle;
        [SerializeField] private GameObject Circle;
        private bool isOpen;
        private Vector3 vel, flickStart, flickDeltaPos;

        private void Start()
        {
            // Cursor.visible = false;
        }

        private void Open()
        {
            vel = Vector3.zero;
            flickDeltaPos = Vector3.zero;
            DSGuyAnimator.Play("Open", -1);
            Circle.SetActive(false);
            isOpen = true;
        }

        private void Close()
        {
            DSGuyAnimator.Play("Close", -1);
            Circle.SetActive(true);
            isOpen = false;
        }

        private void Flick(Vector3 startPos, Vector3 newVel)
        {
            flickStart = startPos;
            vel = newVel;
            DSGuyAnimator.Play("Flick", -1);
            Circle.SetActive(true);
            isOpen = false;
        }

        private void Update()
        {
            Vector3 pos = PlayerInput.GetInputController(1).GetPointer();
            Vector3 deltaPos = pos - transform.position;

            if (follow)
            {
                Vector2 direction = (pos - transform.position).normalized;
                this.GetComponent<Rigidbody2D>().velocity = new Vector2(direction.x * mouseMoveSpeed, direction.y * mouseMoveSpeed);
            }
            else
            {
                gameObject.transform.position = pos;
                if (vel.magnitude > 0.05f)
                {
                    vel -= flickCoeff * Time.deltaTime * vel;
                    flickDeltaPos += vel * Time.deltaTime;
                    DSGuy.transform.position = flickStart + flickDeltaPos;
                }
                else
                {
                    vel = Vector3.zero;
                    flickDeltaPos = Vector3.zero;
                    DSGuy.transform.position = pos;
                }

                if (PlayerInput.GetIsAction(Minigame.InputAction_BasicPress))
                {
                    Open();
                }
                else if (PlayerInput.GetIsAction(Minigame.InputAction_BasicRelease))
                {
                    Close();
                }
                else if (PlayerInput.GetIsAction(Minigame.InputAction_FlickRelease))
                {
                    Flick(pos, deltaPos * flickInitMul);
                }

                if ((!PlayerInput.PlayerHasControl()) && isOpen)
                {
                    Close();
                }
            }
        }
    }
}