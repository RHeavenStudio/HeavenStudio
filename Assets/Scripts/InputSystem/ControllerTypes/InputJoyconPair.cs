using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

using static JSL;

namespace HeavenStudio.InputSystem.Loaders
{
    public static class InputJoyconPairInitializer
    {
        [LoadOrder(3)]
        public static InputController[] Initialize()
        {
            PlayerInput.PlayerInputRefresh.Add(Refresh);
            return Refresh();
        }

        public static InputController[] Refresh()
        {
            int joyconLCount = 0, joyconRCount = 0;
            foreach (InputController con in PlayerInput.GetInputControllers())
            {
                if (con is InputJoyshock)
                {
                    InputJoyshock joyshock = (InputJoyshock)con;
                    if (joyshock.GetJoyshockType() == TypeJoyConLeft)
                    {
                        joyconLCount++;
                    }
                    else if (joyshock.GetJoyshockType() == TypeJoyConRight)
                    {
                        joyconRCount++;
                    }
                }
            }
            if (joyconLCount > 0 && joyconRCount > 0)
            {
                InputJoyconPair joyconPair = new InputJoyconPair();
                joyconPair.SetPlayer(null);
                joyconPair.InitializeController();
                return new InputController[] { joyconPair };
            }
            else
            {
                Debug.Log("No Joy-Con connected.");
                return null;
            }
        }
    }
}

namespace HeavenStudio.InputSystem
{
    public class InputJoyconPair : InputController
    {
        static readonly string[] nsConButtonNames = new[]
        {
            "Up",
            "Down",
            "Left",
            "Right",
            "Plus",
            "Minus",
            "Left Stick Click",
            "Right Stick Click",
            "L",
            "R",
            "ZL",
            "ZR",
            "B",
            "A",
            "Y",
            "X",
            "Home",
            "Capture",
            "", // mic on playstation, unused here
            "SL",
            "SR",
            "", // fnl on playstation, unused here
            "", // fnr on playstation, unused here
            "Stick Up",
            "Stick Down",
            "Stick Left",
            "Stick Right",
        };

        static int[] defaultMappings
        {
            get
            {
                return new[]
                {
                    ButtonMaskUp,
                    ButtonMaskDown,
                    ButtonMaskLeft,
                    ButtonMaskRight,
                    ButtonMaskS,
                    ButtonMaskE,
                    ButtonMaskW,
                    ButtonMaskN,
                    ButtonMaskL,
                    ButtonMaskR,
                    ButtonMaskPlus,
                    -1
                };
            }
        }

        InputJoyshock leftController, rightController;

        int GetButtonForAction(int action)
        {
            if (currentBindings.Pad == null) return -1;
            if (action < 0 || action >= BINDS_MAX) return -1;

            ControlBindings actionMap = currentBindings;
            if (actionMap.Pad[action] > ButtonMaskFnR) return -1;

            return actionMap.Pad[action];
        }

        int GetActionForButton(int button, ControlStyles style)
        {
            if (style != ControlStyles.Pad) return -1;
            if (currentBindings.Pad == null) return -1;
            if (button < 0 || button >= ButtonMaskFnR) return -1;

            ControlBindings actionMap = currentBindings;
            for (int i = 0; i < BINDS_MAX; i++)
            {
                if (actionMap.Pad[i] == button)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SetLeftController(InputJoyshock leftController)
        {
            this.leftController = leftController;
        }

        public void SetRightController(InputJoyshock rightController)
        {
            this.rightController = rightController;
        }

        public bool HasControllers()
        {
            return leftController != null && rightController != null;
        }

        public override bool GetAction(ControlStyles style, int action)
        {
            if (leftController == null || rightController == null)
            {
                return false;
            }
            int button = GetButtonForAction(action);
            if (button == -1) { return false; }
            return leftController.GetButtonState(button).pressed || rightController.GetButtonState(button).pressed;
        }

        public override bool GetActionDown(ControlStyles style, int action, out double dt)
        {
            dt = 0;
            if (leftController == null || rightController == null)
            {
                return false;
            }
            int button = GetButtonForAction(action);
            if (button == -1) { dt = 0; return false; }
            InputJoyshock.JoyshockButtonState leftState = leftController.GetButtonState(button);
            if (leftState.pressed && leftState.isDelta)
            {
                dt = leftState.dt;
                return true;
            }
            InputJoyshock.JoyshockButtonState rightState = rightController.GetButtonState(button);
            if (rightState.pressed && rightState.isDelta)
            {
                dt = rightState.dt;
                return true;
            }
            return false;
        }

        public override bool GetActionUp(ControlStyles style, int action, out double dt)
        {
            dt = 0;
            if (leftController == null || rightController == null)
            {
                return false;
            }
            int button = GetButtonForAction(action);
            if (button == -1) { dt = 0; return false; }
            InputJoyshock.JoyshockButtonState leftState = leftController.GetButtonState(button);
            if (!leftState.pressed && leftState.isDelta)
            {
                dt = leftState.dt;
                return true;
            }
            InputJoyshock.JoyshockButtonState rightState = rightController.GetButtonState(button);
            if (!rightState.pressed && rightState.isDelta)
            {
                dt = rightState.dt;
                return true;
            }
            return false;
        }

        public override float GetAxis(InputAxis axis)
        {
            if (leftController == null || rightController == null)
            {
                return 0;
            }
            return leftController.GetAxis(axis) + rightController.GetAxis(axis);
        }

        public override int GetBindingsVersion()
        {
            return 1;
        }

        public override string[] GetButtonNames()
        {
            return nsConButtonNames;
        }

        public override ControlBindings GetCurrentBindings()
        {
            return currentBindings;
        }

        public override bool GetCurrentStyleSupported()
        {
            return PlayerInput.CurrentControlStyle is ControlStyles.Pad; // or ControlStyles.Baton
        }

        public override ControlBindings GetDefaultBindings()
        {
            ControlBindings binds = new ControlBindings
            {
                Pad = defaultMappings,
                version = GetBindingsVersion(),
                PointerSensitivity = 3
            };
            return binds;
        }

        public override ControlStyles GetDefaultStyle()
        {
            return ControlStyles.Pad;
        }

        public override string GetDeviceName()
        {
            return "Joy-Con Pair";
        }

        public override InputFeatures GetFeatures()
        {
            if (leftController == null || rightController == null)
            {
                return 0;
            }
            InputFeatures features = leftController.GetFeatures() | rightController.GetFeatures();
            return features;
        }

        public override bool GetFlick(out double dt)
        {
            if (leftController == null || rightController == null)
            {
                dt = 0;
                return false;
            }
            return leftController.GetFlick(out dt) || rightController.GetFlick(out dt);
        }

        public override bool GetIsActionUnbindable(int action, ControlStyles style)
        {
            return false;
        }

        public override bool GetIsConnected()
        {
            if (leftController == null || rightController == null)
            {
                return false;
            }
            return leftController.GetIsConnected() && rightController.GetIsConnected();
        }

        public override bool GetIsPoorConnection()
        {
            if (leftController == null || rightController == null)
            {
                return false;
            }
            return leftController.GetIsPoorConnection() || rightController.GetIsPoorConnection();
        }

        public override int GetLastActionDown()
        {
            if (leftController == null || rightController == null)
            {
                return -1;
            }
            int lastLeftButton = leftController.GetLastButtonDown();
            int lastRightButton = rightController.GetLastButtonDown();
            int leftAction = GetActionForButton(lastLeftButton, ControlStyles.Pad);
            int rightAction = GetActionForButton(lastRightButton, ControlStyles.Pad);
            if (leftAction == -1 && rightAction == -1)
            {
                return -1;
            }
            if (leftAction == -1)
            {
                return rightAction;
            }
            else
            {
                return leftAction;
            }
        }

        public override int GetLastButtonDown(bool strict = false)
        {
            if (strict || leftController == null || rightController == null)
            {
                return -1;
            }
            return Math.Max(leftController.GetLastButtonDown(strict), rightController.GetLastButtonDown(strict));
        }

        public override int? GetPlayer()
        {
            return playerNum;
        }

        public override Vector2 GetPointer()
        {
            Camera cam = GameManager.instance.CursorCam;
            Vector3 rawPointerPos = Input.mousePosition;
            rawPointerPos.z = Mathf.Abs(cam.gameObject.transform.position.z);
            return cam.ScreenToWorldPoint(rawPointerPos);
        }

        public override bool GetPointerLeftRight()
        {
            return false;
        }

        public override bool GetSlide(out double dt)
        {
            dt = 0;
            return false;
        }

        public override bool GetSqueeze()
        {
            return false;
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

        public override Vector3 GetVector(InputVector vector)
        {
            if (leftController == null || rightController == null)
            {
                return Vector3.zero;
            }
            return leftController.GetVector(vector) + rightController.GetVector(vector);
        }

        public override void InitializeController()
        {
            leftController = null;
            rightController = null;
            LoadBindings();
        }

        public override void OnSelected()
        {
            if (leftController == null || rightController == null)
            {
                return;
            }
            leftController.OnSelected();
            leftController.SetRotatedStickMode(false);
            rightController.OnSelected();
            rightController.SetRotatedStickMode(false);
        }

        public override void RecentrePointer()
        {
        }

        public override void ResetBindings()
        {
            currentBindings = GetDefaultBindings();
        }

        public override void SetCurrentBindings(ControlBindings newBinds)
        {
            currentBindings = newBinds;
        }

        public override void SetMaterialProperties(Material m)
        {
            Color colour;
            m.SetColor("_BodyColor", ColorUtility.TryParseHtmlString("#2F353A", out colour) ? colour : Color.white);
            m.SetColor("_BtnColor", ColorUtility.TryParseHtmlString("#2F353A", out colour) ? colour : Color.white);
            if (leftController == null)
            {
                m.SetColor("_LGripColor", Color.white);
            }
            else
            {
                m.SetColor("_LGripColor", leftController.GetBodyColor());
            }
            if (rightController == null)
            {
                m.SetColor("_RGripColor", Color.white);
            }
            else
            {
                m.SetColor("_RGripColor", rightController.GetBodyColor());
            }
        }

        public override void SetPlayer(int? playerNum)
        {
            this.playerNum = playerNum;
            int handle;
            if (leftController != null)
            {
                handle = leftController.GetHandle();
                if (playerNum == -1 || playerNum == null)
                {
                    JslSetPlayerNumber(handle, 0);
                }
                else
                {
                    JslSetPlayerNumber(handle, (int)playerNum);
                }
            }
            if (rightController != null)
            {
                handle = rightController.GetHandle();
                if (playerNum == -1 || playerNum == null)
                {
                    JslSetPlayerNumber(handle, 0);
                }
                else
                {
                    JslSetPlayerNumber(handle, (int)playerNum);
                }
            }
        }

        public override void TogglePointerLock(bool locked)
        {
        }

        public override ControlBindings UpdateBindings(ControlBindings lastBinds)
        {
            if (lastBinds.version == 0)
            {
                return GetDefaultBindings();
            }
            return lastBinds;
        }

        public override void UpdateState()
        {
        }
    }
}