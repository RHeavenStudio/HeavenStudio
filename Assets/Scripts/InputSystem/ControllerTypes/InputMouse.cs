using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

namespace HeavenStudio.InputSystem.Loaders
{
    public static class InputMouseInitializer
    {
        [LoadOrder(1)]
        public static InputController[] Initialize()
        {
            PlayerInput.PlayerInputRefresh.Add(Refresh);

            InputMouse mouse = new InputMouse();
            mouse.SetPlayer(null);
            mouse.InitializeController();
            return new InputController[] { mouse };
        }

        public static InputController[] Refresh()
        {
            InputMouse mouse = new InputMouse();
            mouse.SetPlayer(null);
            mouse.InitializeController();
            return new InputController[] { mouse };
        }
    }
}

namespace HeavenStudio.InputSystem
{
    public class InputMouse : InputController
    {
        const double SlideMarginTime = 0.25;
        const double BigMoveMarginSpd = 0.15;
        const double SmallMoveMarginSpd = 0.02;
        const double SlideDist = 0.75;

        private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode))
            .Cast<KeyCode>()
            .Where(k => ((int)k <= (int)KeyCode.Mouse6))
            .ToArray();

        static ControlBindings defaultBindings
        {
            get
            {
                return new ControlBindings()
                {
                    Touch = new int[]
                    {
                        -1,
                        (int)KeyCode.Mouse0,
                        (int)KeyCode.Mouse1,
                        (int)KeyCode.Z,
                        (int)KeyCode.X,
                        (int)KeyCode.Mouse2,
                        (int)KeyCode.Escape
                    },
                    PointerSensitivity = 3,
                };
            }
        }

        static Vector3 screenBounds = Vector3.zero;
        static Vector3 ScreenBounds
        {
            get
            {
                screenBounds.x = Screen.width;
                screenBounds.y = Screen.height;
                return screenBounds;
            }
        }

        Vector3 realPos, lastRealPos;
        Vector3 rawPointerPos, lastRawPointerPos, startPointerPos, lastDownPointerPos, lastUpPointerPos;
        Vector3 deltaPointerPos, lastDeltaPointerPos;
        double timeMoveChange, timeDirectionChange;
        byte dirLeftRight, dirUpDown;
        bool hasFlicked, hasSwiped, isInMove, isInSwipe;

        public override void InitializeController()
        {
            LoadBindings();
        }

        public override void UpdateState()
        {
            if (GameManager.instance == null) return;
            Camera cam = GameManager.instance.CursorCam;

            hasFlicked = false;
            hasSwiped = false;
            lastRawPointerPos = rawPointerPos;
            lastRealPos = realPos;
            lastDeltaPointerPos = deltaPointerPos;

            float pointerXmove = Input.GetAxisRaw("Mouse X");
            float pointerYmove = Input.GetAxisRaw("Mouse Y");

            if (rawPointerPos.x < ScreenBounds.x * 0.2f && pointerXmove > 0)
            {
                pointerXmove *= 2;
            }
            else if (rawPointerPos.x > ScreenBounds.x * 0.8f && pointerXmove < 0)
            {
                pointerXmove *= 2;
            }

            rawPointerPos += new Vector3(pointerXmove, pointerYmove, 0) * currentBindings.PointerSensitivity;
            rawPointerPos.z = Mathf.Abs(cam.gameObject.transform.position.z);
            realPos = cam.ScreenToWorldPoint(rawPointerPos);

            if (GetActionDown(ControlStyles.Touch, (int)ActionsTouch.PointerReset, out _))
            {
                RecentrePointer();
                lastRawPointerPos = rawPointerPos;
                lastRealPos = realPos;
            }
            deltaPointerPos = realPos - lastRealPos;

            bool forcedToBounds = false;
            if (pointerXmove > 0 && rawPointerPos.x < 0)
            {
                rawPointerPos.x = 0;
                forcedToBounds = true;
            }
            else if (pointerXmove < 0 && rawPointerPos.x > ScreenBounds.x)
            {
                rawPointerPos.x = ScreenBounds.x;
                forcedToBounds = true;
            }
            if (pointerYmove > 0 && rawPointerPos.y < 0)
            {
                rawPointerPos.y = 0;
                forcedToBounds = true;
            }
            else if (pointerYmove < 0 && rawPointerPos.y > ScreenBounds.y)
            {
                rawPointerPos.y = ScreenBounds.y;
                forcedToBounds = true;
            }

            if (forcedToBounds)
            {
                lastRawPointerPos = rawPointerPos;
                realPos = cam.ScreenToWorldPoint(rawPointerPos);
                startPointerPos = realPos;
                lastRealPos = realPos;
            }

            float speed = deltaPointerPos.magnitude;
            if (GetAction(ControlStyles.Touch, 0))
            {
                // Debug.Log($"pointer speed: {deltaPointerPos.magnitude}, direction: {deltaPointerPos.normalized}");
                if (speed >= BigMoveMarginSpd)
                {
                    if (!isInMove)
                    {
                        isInMove = true;
                    }
                    if ((!isInSwipe)
                        && (Time.realtimeSinceStartupAsDouble - timeMoveChange <= SlideMarginTime)
                        && (realPos - startPointerPos).magnitude > SlideDist)
                    {
                        hasSwiped = true;
                        isInSwipe = true;
                    }
                }
                else
                {
                    if (speed < SmallMoveMarginSpd)
                    {
                        timeMoveChange = Time.realtimeSinceStartupAsDouble;
                        isInMove = false;
                        isInSwipe = false;
                        startPointerPos = realPos;
                    }
                }
            }

            if (GetActionUp(ControlStyles.Touch, 0, out double dt))
            {
                isInMove = false;
                isInSwipe = false;
                lastRawPointerPos = rawPointerPos;
                lastRealPos = realPos;
                startPointerPos = realPos;
                lastDeltaPointerPos = deltaPointerPos;
                hasFlicked = speed >= BigMoveMarginSpd;
            }
        }

        public override void OnSelected()
        {

        }

        public override string GetDeviceName()
        {
            return "Mouse";
        }

        public override string[] GetButtonNames()
        {
            string[] names = new string[(int)KeyCode.Mouse6 + 1];
            for (int i = 0; i < keyCodes.Length; i++)
            {
                names[(int)keyCodes[i]] = keyCodes[i].ToString();
            }
            return names;
        }

        public override InputFeatures GetFeatures()
        {
            return InputFeatures.Readable_Pointer | InputFeatures.Style_Touch;
        }

        public override bool GetIsConnected()
        {
            return Input.mousePresent;
        }

        public override bool GetIsPoorConnection()
        {
            return false;
        }

        public override ControlBindings GetDefaultBindings()
        {
            return defaultBindings;
        }

        public override void ResetBindings()
        {
            currentBindings = GetDefaultBindings();
        }

        public override ControlBindings GetCurrentBindings()
        {
            return currentBindings;
        }

        public override void SetCurrentBindings(ControlBindings newBinds)
        {
            currentBindings = newBinds;
        }

        public override bool GetIsActionUnbindable(int action, ControlStyles style)
        {
            return action is 0;
        }

        public override int GetLastButtonDown()
        {
            if (Input.anyKeyDown)
            {
                for (KeyCode i = keyCodes[1]; i <= KeyCode.Mouse6; i++)
                {
                    if (Input.GetKeyDown(i))
                        return (int)i;
                }
            }
            return (int)KeyCode.None;
        }

        public override int GetLastActionDown()
        {
            for (int i = 0; i < BINDS_MAX; i++)
            {
                if (Input.GetKeyDown((KeyCode)currentBindings.Pad[i]))
                    return i;
            }
            return -1;
        }

        public override bool GetAction(ControlStyles style, int button)
        {
            if (button < 0) return false;
            if (button is 0 or 1 or 2)
            {
                bool bt = Input.GetKey((KeyCode)currentBindings.Touch[1]) || Input.GetKey((KeyCode)currentBindings.Touch[2]);
                switch (button)
                {
                    case 0:
                        return bt;
                    case 1:
                        if (bt && rawPointerPos.x <= ScreenBounds.x * 0.5f)
                        {
                            lastDownPointerPos = rawPointerPos;
                            return true;
                        }
                        return false;
                    case 2:
                        if (bt && rawPointerPos.x >= ScreenBounds.x * 0.5f)
                        {
                            lastDownPointerPos = rawPointerPos;
                            return true;
                        }
                        return false;
                }
            }
            return Input.GetKey((KeyCode)currentBindings.Touch[button]);
        }

        public override bool GetActionDown(ControlStyles style, int button, out double dt)
        {
            dt = 0;
            if (button < 0) return false;
            if (button is 0 or 1 or 2)
            {
                bool bt = Input.GetKeyDown((KeyCode)currentBindings.Touch[1]) || Input.GetKeyDown((KeyCode)currentBindings.Touch[2]);
                switch (button)
                {
                    case 0:
                        return bt;
                    case 1:
                        if (bt && rawPointerPos.x <= ScreenBounds.x * 0.5f)
                        {
                            lastDownPointerPos = rawPointerPos;
                            startPointerPos = rawPointerPos;
                            return true;
                        }
                        return false;
                    case 2:
                        if (bt && rawPointerPos.x >= ScreenBounds.x * 0.5f)
                        {
                            lastDownPointerPos = rawPointerPos;
                            startPointerPos = rawPointerPos;
                            return true;
                        }
                        return false;
                }
            }
            return Input.GetKeyDown((KeyCode)currentBindings.Touch[button]);
        }

        public override bool GetActionUp(ControlStyles style, int button, out double dt)
        {
            dt = 0;
            if (button < 0) return false;
            if (button is 0 or 1 or 2)
            {
                bool bt = (Input.GetKeyUp((KeyCode)currentBindings.Touch[1]) || Input.GetKeyUp((KeyCode)currentBindings.Touch[2]))
                    && !(Input.GetKey((KeyCode)currentBindings.Touch[1]) || Input.GetKey((KeyCode)currentBindings.Touch[2]));
                switch (button)
                {
                    case 0:
                        return bt;
                    case 1:
                        if (bt && rawPointerPos.x <= ScreenBounds.x * 0.5f)
                        {
                            lastUpPointerPos = rawPointerPos;
                            return true;
                        }
                        return false;
                    case 2:
                        if (bt && rawPointerPos.x >= ScreenBounds.x * 0.5f)
                        {
                            lastUpPointerPos = rawPointerPos;
                            return true;
                        }
                        return false;
                }
            }
            return Input.GetKeyUp((KeyCode)currentBindings.Touch[button]);
        }

        public override float GetAxis(InputAxis axis)
        {
            return 0;
        }

        public override Vector3 GetVector(InputVector vec)
        {
            return Vector3.zero;
        }

        public override Vector2 GetPointer()
        {
            return realPos;
        }

        //todo: directionals
        public override bool GetHatDirection(InputDirection direction)
        {
            return false;
        }

        public override bool GetHatDirectionDown(InputDirection direction, out double dt)
        {
            dt = 0;
            return false;
        }

        public override bool GetHatDirectionUp(InputDirection direction, out double dt)
        {
           dt = 0;
            return false;
        }

        public override void SetPlayer(int? playerNum)
        {
            if (playerNum == -1 || playerNum == null)
            {
                this.playerNum = null;
                return;
            }
            this.playerNum = (int)playerNum;
        }

        public override int? GetPlayer()
        {
            return playerNum;
        }

        public override bool GetFlick(out double dt)
        {
            dt = 0;
            return hasFlicked;
        }

        public override bool GetSlide(out double dt)
        {
            dt = hasSwiped ? Time.realtimeSinceStartupAsDouble - timeMoveChange : 0;
            return hasSwiped;
        }

        public override void SetMaterialProperties(Material m)
        {
            bool b = ColorUtility.TryParseHtmlString("#F4F4F4", out Color colour);
            m.SetColor("_BodyColor", b ? colour : Color.white);
        }

        public override bool GetCurrentStyleSupported()
        {
            return PlayerInput.CurrentControlStyle is ControlStyles.Touch;
        }

        public override ControlStyles GetDefaultStyle()
        {
            return ControlStyles.Touch;
        }

        public override bool GetSqueezeDown(out double dt)
        {
            dt = 0;
            return false;
        }

        public override bool GetSqueezeUp(out double dt)
        {
            dt = 0;
            return false;
        }

        public override bool GetSqueeze()
        {
            return false;
        }

        public override void TogglePointerLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            RecentrePointer();
        }

        public override void RecentrePointer()
        {
            rawPointerPos = ScreenBounds * 0.5f;
        }

        public override bool GetPointerLeftRight()
        {
            return rawPointerPos.x > ScreenBounds.x * 0.5f;
        }
    }
}