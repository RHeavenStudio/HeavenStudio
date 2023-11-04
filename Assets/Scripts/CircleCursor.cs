using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using HeavenStudio.Games;

namespace HeavenStudio
{
    public class CircleCursor : MonoBehaviour
    {
        static readonly GradientAlphaKey[] cursorAlphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(0.5f, 0),
            new GradientAlphaKey(0, 1)
        };

        [SerializeField] private bool follow = false;
        [SerializeField] private float mouseMoveSpeed;
        [SerializeField] private float sideChangeTrailTime;

        [Header("DSGuy")]
        [SerializeField] private GameObject DSGuy;
        [SerializeField] private Animator DSGuyAnimator;
        [SerializeField] private float flickCoeff = 0.35f;
        [SerializeField] private float flickInitMul = 1.5f;
        [SerializeField] private TrailRenderer splitTouchSnapEffect;
        public GameObject InnerCircle;
        [SerializeField] private GameObject Circle;
        private bool isOpen;
        private Vector3 vel, flickStart, flickDeltaPos;
        private Color colorMain, colorL, colorR;
        private Gradient gradientL, gradientR;
        private bool lastLeftRightState;
        private float trailEnableTime;

        private SpriteRenderer innerCircleRenderer;

        private void Start()
        {
            // Cursor.visible = false;
            if (splitTouchSnapEffect != null)
            {
                splitTouchSnapEffect.emitting = false;
            }
            if (InnerCircle != null)
            {
                innerCircleRenderer = InnerCircle.GetComponent<SpriteRenderer>();
            }
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
                bool lrState = PlayerInput.GetInputController(1).GetPointerLeftRight();
                if (splitTouchSnapEffect != null && PlayerInput.CurrentControlStyle == InputSystem.InputController.ControlStyles.Touch && (GameManager.instance?.GameHasSplitColours ?? false))
                {
                    if (lrState != lastLeftRightState)
                    {
                        lastLeftRightState = lrState;
                        trailEnableTime = sideChangeTrailTime;
                        splitTouchSnapEffect.emitting = true;

                        innerCircleRenderer.color = lastLeftRightState ? colorR : colorL;
                        splitTouchSnapEffect.colorGradient = lastLeftRightState ? gradientR : gradientL;
                    }

                    if (trailEnableTime <= 0)
                    {
                        trailEnableTime = 0;
                        splitTouchSnapEffect.emitting = false;
                    }
                    else
                    {
                        trailEnableTime -= Time.deltaTime;
                    }
                }
                else if (splitTouchSnapEffect != null && splitTouchSnapEffect.emitting)
                {
                    ClearTrail();
                }

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
                    ClearTrail();
                }

                if ((!PlayerInput.PlayerHasControl()) && isOpen)
                {
                    Close();
                    ClearTrail();
                    if (splitTouchSnapEffect != null)
                    {
                        splitTouchSnapEffect.emitting = false;
                    }
                }
            }
        }

        public void LockCursor(bool toggle)
        {
            PlayerInput.GetInputController(1).TogglePointerLock(toggle);
            ClearTrail();
        }

        public void ClearTrail()
        {
            trailEnableTime = 0;
            if (splitTouchSnapEffect != null)
            {
                splitTouchSnapEffect.Clear();
            }
        }

        public void SetCursorColors(Color main, Color left, Color right)
        {
            if (innerCircleRenderer == null) innerCircleRenderer = InnerCircle.GetComponent<SpriteRenderer>();

            colorMain = main;
            colorL = left;
            colorR = right;
            if (PlayerInput.CurrentControlStyle == InputSystem.InputController.ControlStyles.Touch && (GameManager.instance?.GameHasSplitColours ?? false))
            {
                innerCircleRenderer.color = lastLeftRightState ? colorR : colorL;
                gradientL = new Gradient()
                {
                    colorKeys = new GradientColorKey[]
                    {
                        new GradientColorKey(colorL, 0),
                        new GradientColorKey(colorL, 1)
                    },
                    alphaKeys = cursorAlphaKeys
                };
                gradientR = new Gradient()
                {
                    colorKeys = new GradientColorKey[]
                    {
                        new GradientColorKey(colorR, 0),
                        new GradientColorKey(colorR, 1)
                    },
                    alphaKeys = cursorAlphaKeys
                };
            }
            else
            {
                innerCircleRenderer.color = colorMain;
            }
        }
    }
}