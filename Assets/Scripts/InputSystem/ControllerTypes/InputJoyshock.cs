using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

using static JSL;

namespace HeavenStudio.InputSystem.Loaders
{
    public static class InputJoyshockInitializer
    {
        [LoadOrder(1)]
        public static InputController[] Initialize()
        {
            InputJoyshock.joyshocks = new();
            PlayerInput.PlayerInputCleanUp += DisposeJoyshocks;
            PlayerInput.PlayerInputRefresh.Add(Refresh);

            InputJoyshock.JslEventInit();

            InputController[] controllers;
            int jslDevicesFound = 0;
            int jslDevicesConnected = 0;
            int[] jslDeviceHandles;

            jslDevicesFound = JslConnectDevices();
            if (jslDevicesFound > 0)
            {
                jslDeviceHandles = new int[jslDevicesFound];
                jslDevicesConnected = JslGetConnectedDeviceHandles(jslDeviceHandles, jslDevicesFound);
                if (jslDevicesConnected < jslDevicesFound)
                {
                    Debug.Log("Found " + jslDevicesFound + " JoyShocks, but only " + jslDevicesConnected + " are connected.");
                }
                else
                {
                    Debug.Log("Found " + jslDevicesFound + " JoyShocks.");
                    Debug.Log("Connected " + jslDevicesConnected + " JoyShocks.");
                }

                controllers = new InputController[jslDevicesConnected];
                foreach (int i in jslDeviceHandles)
                {
                    Debug.Log("Setting up JoyShock: ( Handle " + i + ", type " + JslGetControllerType(i) + " )");
                    InputJoyshock joyshock = new InputJoyshock(i);
                    joyshock.SetPlayer(null);
                    joyshock.InitializeController();
                    controllers[i] = joyshock;
                }
                return controllers;
            }
            Debug.Log("No JoyShocks found.");
            return null;
        }

        public static void DisposeJoyshocks()
        {
            foreach (InputJoyshock joyshock in InputJoyshock.joyshocks.Values)
            {
                joyshock.CleanUp();
            }
            JslDisconnectAndDisposeAll();
        }

        public static InputController[] Refresh()
        {
            InputJoyshock.joyshocks.Clear();
            InputController[] controllers;
            int jslDevicesFound = 0;
            int jslDevicesConnected = 0;
            int[] jslDeviceHandles;

            jslDevicesFound = JslConnectDevices();
            if (jslDevicesFound > 0)
            {
                jslDeviceHandles = new int[jslDevicesFound];
                jslDevicesConnected = JslGetConnectedDeviceHandles(jslDeviceHandles, jslDevicesFound);

                controllers = new InputController[jslDevicesConnected];
                foreach (int i in jslDeviceHandles)
                {
                    Debug.Log("Setting up JoyShock: ( Handle " + i + ", type " + JslGetControllerType(i) + " )");
                    InputJoyshock joyshock = new InputJoyshock(i);
                    joyshock.SetPlayer(null);
                    joyshock.InitializeController();
                    controllers[i] = joyshock;
                }
                return controllers;
            }
            Debug.Log("No JoyShocks found.");
            return null;
        }
    }
}

namespace HeavenStudio.InputSystem
{
    public class InputJoyshock : InputController
    {
        static string[] joyShockNames =
        {
            "Unknown",
            "Joy-Con (L)",
            "Joy-Con (R)",
            "Pro Controller",
            "DualShock 4",
            "DualSense"
        };

        static int[] dsPlayerColours = new[]
        {
            0xd41817,
            0x04d4fa,
            0x05ff08,
            0xffdd01,
            0xe906c9,
            0xcc6020,
            0x888888
        };

        //TODO: see if single joy-con mappings differ from a normal pad (they don't!)
        int[] defaultMappings = new[]
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
        int[] defaultMappingsL = new[]
        {
            -1,
            -1,
            -1,
            -1,
            ButtonMaskLeft,
            ButtonMaskDown,
            ButtonMaskUp,
            ButtonMaskRight,
            ButtonMaskSL,
            ButtonMaskSR,
            ButtonMaskMinus,
            -1
        };
        int[] defaultMappingsR = new[]
        {
            -1,
            -1,
            -1,
            -1,
            ButtonMaskE,
            ButtonMaskN,
            ButtonMaskS,
            ButtonMaskW,
            ButtonMaskSL,
            ButtonMaskSR,
            ButtonMaskPlus,
            -1
        };

        public static Dictionary<int, InputJoyshock> joyshocks;

        float stickDeadzone = 0.5f;

        int joyshockHandle;
        int type;
        int splitType;
        int lightbarColour;
        string joyshockName;
        DateTime startTime;

        //buttons, sticks, triggers
        JoyshockButtonState[] buttonStates = new JoyshockButtonState[BINDS_MAX];
        JOY_SHOCK_STATE joyBtStateCurrent;
        //gyro and accelerometer
        IMU_STATE joyImuStateCurrent, joyImuStateLast;
        //touchpad
        TOUCH_STATE joyTouchStateCurrent, joyTouchStateLast;

        // controller settings
        JSL_SETTINGS joySettings;

        InputJoyshock otherHalf;

        public struct JoyshockButtonState
        {
            public double dt;     // time passed since state
            public bool pressed;    // true if button is down
            public bool isDelta;    // true if the button changed state since last frame
        }

        public struct TimestampedState
        {
            public double timestamp;
            public JOY_SHOCK_STATE input;
        }

        protected List<TimestampedState> inputStack;        // asynnc input events / polling should feed into this dict
        protected List<TimestampedState> lastInputStack;    // when processing input copy the inputStack to this dict
        protected bool wantClearInputStack = false;         // strobe from main thread to clear the input stack
        protected double reportTime = 0;

        public InputJoyshock(int handle)
        {
            joyshockHandle = handle;
        }

        int GetButtonForSplitType(int action)
        {
            if (action < 0 || action >= BINDS_MAX) return -1;
            if (otherHalf == null)
            {
                switch (splitType)
                {
                    case SplitLeft:
                        return defaultMappingsL[action];
                    case SplitRight:
                        return defaultMappingsR[action];
                    default:
                        return defaultMappings[action];
                }
            }
            return defaultMappings[action];
        }

        public static void JslEventInit()
        {
            JslSetCallback(JslEventCallback);
        }

        static void JslEventCallback(int handle, JOY_SHOCK_STATE state, JOY_SHOCK_STATE lastState,
        IMU_STATE imuState, IMU_STATE lastImuState, float deltaTime)
        {
            if (!joyshocks.ContainsKey(handle)) return;
            InputJoyshock js = joyshocks[handle];
            if (js == null) return;
            if (js.inputStack == null) return;

            if (js.wantClearInputStack)
            {
                js.inputStack.Clear();
                js.wantClearInputStack = false;
            }
            js.inputStack.Add(new TimestampedState
            {
                timestamp = (DateTime.Now - js.startTime).TotalSeconds,
                input = state
            });
        }

        public override void InitializeController()
        {
            startTime = DateTime.Now;
            inputStack = new();
            lastInputStack = new();

            buttonStates = new JoyshockButtonState[BINDS_MAX];
            joyBtStateCurrent = new JOY_SHOCK_STATE();

            joyImuStateCurrent = new IMU_STATE();
            joyImuStateLast = new IMU_STATE();

            joyTouchStateCurrent = new TOUCH_STATE();
            joyTouchStateLast = new TOUCH_STATE();


            //FUTURE: remappable controls

            joySettings = JslGetControllerInfoAndSettings(joyshockHandle);
            type = joySettings.controllerType;
            joyshockName = joyShockNames[type];

            splitType = joySettings.splitType;

            joyshocks.Add(joyshockHandle, this);
        }

        public void CleanUp()
        {
            JslSetPlayerNumber(joyshockHandle, 0);
            JslSetLightColour(joyshockHandle, 0);
        }

        public override void UpdateState()
        {
            reportTime = (DateTime.Now - startTime).TotalSeconds;
            lastInputStack = new(inputStack);
            wantClearInputStack = true;

            for (int i = 0; i < buttonStates.Length; i++)
            {
                buttonStates[i].isDelta = false;
            }
            foreach(TimestampedState state in lastInputStack)
            {
                joyBtStateCurrent = state.input;

                for (int i = 0; i < buttonStates.Length; i++)
                {
                    int bt = GetButtonForSplitType(i);
                    if (bt != -1)
                    {
                        bool pressed = BitwiseUtils.WantCurrent(state.input.buttons, 1 << bt);
                        if (pressed != buttonStates[i].pressed && !buttonStates[i].isDelta)
                        {
                            buttonStates[i].pressed = pressed;
                            buttonStates[i].isDelta = true;
                            buttonStates[i].dt = reportTime - state.timestamp;
                        }
                    }
                }
            }

            //stick direction state, only handled on update
            //split controllers will need to be rotated to compensate
            //left rotates counterclockwise, right rotates clockwise, all by 90 degrees
            float xAxis = 0f;
            float yAxis = 0f;
            if (otherHalf == null)
            {
                switch (splitType)
                {
                    case SplitLeft:
                        xAxis = -joyBtStateCurrent.stickLY;
                        yAxis = joyBtStateCurrent.stickLX;
                        break;
                    case SplitRight: //use the right stick instead
                        xAxis = joyBtStateCurrent.stickRY;
                        yAxis = -joyBtStateCurrent.stickRX;
                        break;
                    case SplitFull:
                        xAxis = joyBtStateCurrent.stickLX;
                        yAxis = joyBtStateCurrent.stickLY;
                        break;
                }
            }
            else
            {
                xAxis = joyBtStateCurrent.stickLX;
                yAxis = joyBtStateCurrent.stickLY;
            }
            
            directionStateLast = directionStateCurrent;
            directionStateCurrent = 0;
            directionStateCurrent |= ((yAxis >= stickDeadzone) ? (1 << ((int) InputDirection.Up)) : 0);
            directionStateCurrent |= ((yAxis <= -stickDeadzone) ? (1 << ((int) InputDirection.Down)) : 0);
            directionStateCurrent |= ((xAxis >= stickDeadzone) ? (1 << ((int) InputDirection.Right)) : 0);
            directionStateCurrent |= ((xAxis <= -stickDeadzone) ? (1 << ((int) InputDirection.Left)) : 0);
            //Debug.Log("stick direction: " + directionStateCurrent + "| x axis: " + xAxis + " y axis: " + yAxis);

            lastInputStack.Clear();
        }

        public override string GetDeviceName()
        {
            if (otherHalf != null)
                return "Joy-Con Pair";
            return joyshockName;
        }

        public override InputFeatures GetFeatures()
        {
            InputFeatures features = InputFeatures.Readable_MotionSensor | InputFeatures.Extra_Rumble | InputFeatures.Style_Pad | InputFeatures.Style_Baton;
            switch (type)
            {
                case TypeJoyConLeft:
                    features |= InputFeatures.Readable_ShellColour | InputFeatures.Readable_ButtonColour | InputFeatures.Writable_PlayerLED | InputFeatures.Extra_SplitControllerLeft | InputFeatures.Extra_HDRumble;
                    break;
                case TypeJoyConRight:
                    features |= InputFeatures.Readable_ShellColour | InputFeatures.Readable_ButtonColour | InputFeatures.Writable_PlayerLED | InputFeatures.Extra_SplitControllerRight | InputFeatures.Extra_HDRumble;
                    break;
                case TypeProController:
                    features |= InputFeatures.Readable_ShellColour | InputFeatures.Readable_ButtonColour | InputFeatures.Readable_LeftGripColour | InputFeatures.Readable_RightGripColour | InputFeatures.Writable_PlayerLED | InputFeatures.Extra_HDRumble;
                    break;
                case TypeDualShock4:
                    features |= InputFeatures.Readable_AnalogueTriggers | InputFeatures.Readable_Pointer | InputFeatures.Writable_LightBar;
                    break;
                case TypeDualSense:
                    features |= InputFeatures.Readable_AnalogueTriggers | InputFeatures.Readable_Pointer | InputFeatures.Writable_PlayerLED | InputFeatures.Writable_LightBar;
                    break;
            }
            return features;
        }

        public override bool GetIsConnected()
        {
            return JslStillConnected(joyshockHandle);
        }

        public override bool GetIsPoorConnection()
        {
            return false;
        }

        public override int GetLastButtonDown()
        {
            for (int i = 0; i < buttonStates.Length; i++)
            {
                if (buttonStates[i].pressed && buttonStates[i].isDelta)
                {
                    return i;
                }
            }
            return -1;
        }

        public override KeyCode GetLastKeyDown()
        {
            return KeyCode.None;
        }

        public override bool GetButton(int button)
        {
            if (button == -1) {return false;}
            return buttonStates[button].pressed;
        }

        public override bool GetButtonDown(int button, out double dt)
        {
            if (button == -1) {dt = 0; return false;}
            dt = buttonStates[button].dt;
            return buttonStates[button].pressed && buttonStates[button].isDelta;
        }

        public override bool GetButtonUp(int button, out double dt)
        {
            if (button == -1) {dt = 0; return false;}
            dt = buttonStates[button].dt;
            return !buttonStates[button].pressed && buttonStates[button].isDelta;
        }

        public override float GetAxis(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.AxisLTrigger:
                    return joyBtStateCurrent.lTrigger;
                case InputAxis.AxisRTrigger:
                    return joyBtStateCurrent.rTrigger;
                case InputAxis.AxisLStickX:
                    return joyBtStateCurrent.stickLX;
                case InputAxis.AxisLStickY:
                    return joyBtStateCurrent.stickLY;
                case InputAxis.AxisRStickX:
                    return joyBtStateCurrent.stickRX;
                case InputAxis.AxisRStickY:
                    return joyBtStateCurrent.stickRY;
                case InputAxis.TouchpadX:   //isn't updated for now, so always returns 0f
                    //return joyTouchStateCurrent.t0X;
                case InputAxis.TouchpadY:
                    //return joyTouchStateCurrent.t0Y;
                default:
                    return 0f;
            }
        }

        public override bool GetHatDirection(InputDirection direction)
        {
            int bt;
            switch (direction)
            {
                case InputDirection.Up:
                    bt = GetButtonForSplitType(0);
                    break;
                case InputDirection.Down:
                    bt = GetButtonForSplitType(1);
                    break;
                case InputDirection.Left:
                    bt = GetButtonForSplitType(2);
                    break;
                case InputDirection.Right:
                    bt = GetButtonForSplitType(3);
                    break;
                default:
                    return false;
            }
            return GetButton(bt) || BitwiseUtils.WantCurrent(directionStateCurrent, 1 << (int) direction);
        }

        public override bool GetHatDirectionDown(InputDirection direction, out double dt)
        {
            int bt;
            switch (direction)
            {
                case InputDirection.Up:
                    bt = GetButtonForSplitType(0);
                    break;
                case InputDirection.Down:
                    bt = GetButtonForSplitType(1);
                    break;
                case InputDirection.Left:
                    bt = GetButtonForSplitType(2);
                    break;
                case InputDirection.Right:
                    bt = GetButtonForSplitType(3);
                    break;
                default:
                    dt = 0;
                    return false;
            }
            bool btbool = GetButtonDown(bt, out dt);
            if (!btbool) dt = 0;
            return btbool || BitwiseUtils.WantCurrentAndNotLast(directionStateCurrent, directionStateLast, 1 << (int) direction);
        }

        public override bool GetHatDirectionUp(InputDirection direction, out double dt)
        {
            int bt;
            switch (direction)
            {
                case InputDirection.Up:
                    bt = GetButtonForSplitType(0);
                    break;
                case InputDirection.Down:
                    bt = GetButtonForSplitType(1);
                    break;
                case InputDirection.Left:
                    bt = GetButtonForSplitType(2);
                    break;
                case InputDirection.Right:
                    bt = GetButtonForSplitType(3);
                    break;
                default:
                    dt = 0;
                    return false;
            }
            bool btbool = GetButtonUp(bt, out dt);
            if (!btbool) dt = 0;
            return btbool || BitwiseUtils.WantNotCurrentAndLast(directionStateCurrent, directionStateLast, 1 << (int) direction);
        }

        public override void SetPlayer(int? playerNum)
        {
            //TODO: dualshock 4 and dualsense lightbar colour support
            if (playerNum == -1 || playerNum == null)
            {
                this.playerNum = null;
                JslSetPlayerNumber(joyshockHandle, 0);
                JslSetLightColour(joyshockHandle, 0);
                return;
            }
            this.playerNum = playerNum;
            int ledMask = (int) this.playerNum;
            if (type == TypeDualSense)
            {
                if (playerNum <= 4)
                {
                    ledMask = DualSensePlayerMask[(int) this.playerNum];
                }
            }
            JslSetPlayerNumber(joyshockHandle, ledMask);
            lightbarColour = GetLightbarColourForPlayer((int) this.playerNum);
            JslSetLightColour(joyshockHandle, lightbarColour);
        }

        public override int? GetPlayer()
        {
            return this.playerNum;
        }

        public Color GetBodyColor()
        {
            if (otherHalf != null)
            {
                // gets the colour of the right controller if is split
                return BitwiseUtils.IntToRgb(splitType == SplitRight ? joySettings.bodyColour : GetOtherHalf().joySettings.bodyColour);
            }
            return BitwiseUtils.IntToRgb(joySettings.bodyColour);
        }

        public Color GetButtonColor()
        {
            return BitwiseUtils.IntToRgb(joySettings.buttonColour);
        }

        public Color GetLeftGripColor()
        {
            if (otherHalf != null)
            {
                return BitwiseUtils.IntToRgb(splitType == SplitLeft ? joySettings.lGripColour : GetOtherHalf().joySettings.lGripColour);
            }
            return BitwiseUtils.IntToRgb(joySettings.lGripColour);
        }

        public Color GetRightGripColor()
        {
            if (otherHalf != null)
            {
                return BitwiseUtils.IntToRgb(splitType == SplitRight ? joySettings.rGripColour : GetOtherHalf().joySettings.rGripColour);
            }
            return BitwiseUtils.IntToRgb(joySettings.rGripColour);
        }

        public Color GetLightbarColour()
        {
            return BitwiseUtils.IntToRgb(lightbarColour);
        }

        public void SetLightbarColour(Color color)
        {
            lightbarColour = BitwiseUtils.RgbToInt(color);
            JslSetLightColour(joyshockHandle, lightbarColour);
        }

        public static int GetLightbarColourForPlayer(int playerNum = 0)
        {
            if (playerNum < 0)
            {
                return dsPlayerColours[dsPlayerColours.Length - 1];
            }

            playerNum = Math.Min(playerNum, dsPlayerColours.Length - 1);
            return dsPlayerColours[playerNum];
        }

        public int GetHandle()
        {
            return joyshockHandle;
        }

        public void DisconnectJoyshock()
        {
            if (otherHalf != null)
            {
                otherHalf = null;
            }
            JslSetRumble(joyshockHandle, 0, 0);
            JslSetLightColour(joyshockHandle, 0);
            JslSetPlayerNumber(joyshockHandle, 0);
        }

        public void AssignOtherHalf(InputJoyshock otherHalf, bool force = false)
        {
            InputFeatures features = otherHalf.GetFeatures();
            if (features.HasFlag(InputFeatures.Extra_SplitControllerLeft) || features.HasFlag(InputFeatures.Extra_SplitControllerRight))
            {
                //two-way link
                this.otherHalf = otherHalf;
                this.otherHalf.UnAssignOtherHalf(); //juste en cas
                this.otherHalf.otherHalf = this;
                this.otherHalf.SetPlayer(this.playerNum);
            }
            else if (force)
            {
                UnAssignOtherHalf();
            }
        }

        public void UnAssignOtherHalf()
        {
            if (otherHalf != null)
            {
                this.otherHalf.otherHalf = null;
                this.otherHalf.SetPlayer(-1);
            }
            otherHalf = null;
        }

        public InputJoyshock GetOtherHalf()
        {
            return otherHalf;
        }
    }
}