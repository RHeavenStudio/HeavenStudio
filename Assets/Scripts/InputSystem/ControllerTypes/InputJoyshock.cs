using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

using static JSL;

namespace HeavenStudio.InputSystem.Loaders
{
    public static class InputJoyshockInitializer
    {
        static bool failedJsl = false;
        [LoadOrder(2)]
        public static InputController[] Initialize()
        {
            failedJsl = false;
            InputJoyshock.joyshocks = new();
            PlayerInput.PlayerInputCleanUp += DisposeJoyshocks;
            PlayerInput.PlayerInputRefresh.Add(Refresh);

            List<InputController> controllers;
            int jslDevicesFound = 0;
            int jslDevicesConnected = 0;
            int[] jslDeviceHandles;

            try
            {
                JslDisconnectAndDisposeAll();
                jslDevicesFound = JslConnectDevices();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to initialize JoyShockLibrary: " + e.Message);
                failedJsl = true;
                return null;
            }
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

                controllers = new();
                foreach (int i in jslDeviceHandles)
                {
                    Debug.Log("Setting up JoyShock: ( Handle " + i + ", type " + JslGetControllerType(i) + " )");
                    InputJoyshock joyshock = new InputJoyshock(i);
                    joyshock.SetPlayer(null);
                    joyshock.InitializeController();
                    controllers.Add(joyshock);
                }
                InputJoyshock.JslEventInit();
                return controllers.ToArray();
            }
            Debug.Log("No JoyShocks found.");
            return null;
        }

        public static void DisposeJoyshocks()
        {
            if (failedJsl) return;
            foreach (InputJoyshock joyshock in InputJoyshock.joyshocks.Values)
            {
                joyshock.CleanUp();
            }
            JslDisconnectAndDisposeAll();
            JslSetCallback(null);
        }

        public static InputController[] Refresh()
        {
            if (failedJsl) return null;
            InputJoyshock.joyshocks.Clear();
            List<InputController> controllers;
            int jslDevicesFound = 0;
            int jslDevicesConnected = 0;
            int[] jslDeviceHandles;

            JslDisconnectAndDisposeAll();
            jslDevicesFound = JslConnectDevices();
            if (jslDevicesFound > 0)
            {
                jslDeviceHandles = new int[jslDevicesFound];
                jslDevicesConnected = JslGetConnectedDeviceHandles(jslDeviceHandles, jslDevicesFound);

                controllers = new();
                foreach (int i in jslDeviceHandles)
                {
                    Debug.Log("Setting up JoyShock: ( Handle " + i + ", type " + JslGetControllerType(i) + " )");
                    InputJoyshock joyshock = new InputJoyshock(i);
                    joyshock.SetPlayer(null);
                    joyshock.InitializeController();
                    controllers.Add(joyshock);
                }
                InputJoyshock.JslEventInit();
                return controllers.ToArray();
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
        static readonly string[] joyShockNames =
        {
            "Unknown",
            "Joy-Con (L)",
            "Joy-Con (R)",
            "Pro Controller",
            "DualShock 4",
            "DualSense/Edge"  // jsl doesn't expose a new device ID for DSE
        };

        static readonly int[] dsPlayerColours = new[]
        {
            0xd41817,
            0x04d4fa,
            0x05ff08,
            0xffdd01,
            0xe906c9,
            0xcc6020,
            0x888888
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
        static int[] defaultMappingsL
        {
            get
            {
                return new[]
                {
                    23,
                    24,
                    25,
                    26,
                    ButtonMaskLeft,
                    ButtonMaskDown,
                    ButtonMaskUp,
                    ButtonMaskRight,
                    ButtonMaskSL,
                    ButtonMaskSR,
                    ButtonMaskMinus,
                    -1
                };
            }
        }

        static int[] defaultMappingsR
        {
            get
            {
                return new[]
                {
                    23,
                    24,
                    25,
                    26,
                    ButtonMaskE,
                    ButtonMaskN,
                    ButtonMaskS,
                    ButtonMaskW,
                    ButtonMaskSL,
                    ButtonMaskSR,
                    ButtonMaskPlus,
                    -1
                };
            }
        }

        static readonly string[] nsProButtonNames = new[]
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
        };

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

        static readonly string[] ps4ButtonNames = new[]
        {
            "Up",
            "Down",
            "Left",
            "Right",
            "Options",
            "Share",
            "L3",
            "R3",
            "L",
            "R",
            "L2",
            "R2",
            "X",
            "Circle",
            "Square",
            "Triangle",
            "PS",
            "Touchpad Click",
            "Mic",
        };

        static readonly string[] ps5ButtonNames = new[]
        {
            "Up",
            "Down",
            "Left",
            "Right",
            "Options",
            "Share",
            "L3",
            "R3",
            "L",
            "R",
            "L2",
            "R2",
            "X",
            "Circle",
            "Square",
            "Triangle",
            "PS",
            "Create",
            "Mic",
            "Left Grip",
            "Right Grip",
            "Left Function",
            "Right Function",
        };

        static readonly float debounceTime = 1f / 90f;

        public static Dictionary<int, InputJoyshock> joyshocks;

        const float STICK_DEAD = 0.6f;

        int joyshockHandle;
        int type;
        int splitType;
        int lightbarColour;
        string joyshockName;
        DateTime startTime;
        bool joyConWantRotatedStick = true;

        //buttons, sticks, triggers
        JoyshockButtonState[] actionStates = new JoyshockButtonState[BINDS_MAX];
        JoyshockButtonState[] buttonStates = new JoyshockButtonState[ButtonMaskFnR + 1];
        JOY_SHOCK_STATE joyBtStateCurrent;
        //gyro and accelerometer
        IMU_STATE joyImuStateCurrent, joyImuStateLast;
        //touchpad
        TOUCH_STATE joyTouchStateCurrent, joyTouchStateLast;

        // controller settings
        JSL_SETTINGS joySettings;

        public struct JoyshockButtonState
        {
            public double dt;     // time passed since state
            public bool pressed;    // true if button is down
            public float debounce;  // timer to ignore button updates
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

        int GetButtonForAction(int action)
        {
            if (currentBindings.Pad == null) return -1;
            if (action < 0 || action >= BINDS_MAX) return -1;
            ControlBindings actionMap = currentBindings;
            if (actionMap.Pad[action] > ButtonMaskFnR) return -1;

            return actionMap.Pad[action];
        }

        public static void JslEventInit()
        {
            JslSetCallback(JslEventCallback);
        }

        static void JslEventCallback(int handle, JOY_SHOCK_STATE state, JOY_SHOCK_STATE lastState,
        IMU_STATE imuState, IMU_STATE lastImuState, float deltaTime)
        {
            if (joyshocks == null || !joyshocks.ContainsKey(handle)) return;
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


            js.joyImuStateCurrent = imuState;
            js.joyImuStateLast = lastImuState;
        }

        public override void InitializeController()
        {
            startTime = DateTime.Now;
            inputStack = new();
            lastInputStack = new();

            actionStates = new JoyshockButtonState[BINDS_MAX];
            buttonStates = new JoyshockButtonState[ButtonMaskFnR + 1];
            joyBtStateCurrent = new JOY_SHOCK_STATE();

            joyImuStateCurrent = new IMU_STATE();
            joyImuStateLast = new IMU_STATE();

            joyTouchStateCurrent = new TOUCH_STATE();
            joyTouchStateLast = new TOUCH_STATE();


            joySettings = JslGetControllerInfoAndSettings(joyshockHandle);
            type = joySettings.controllerType;
            joyshockName = joyShockNames[type];

            splitType = joySettings.splitType;

            joyshocks.Add(joyshockHandle, this);

            LoadBindings();
        }

        public void CleanUp()
        {
            JslSetPlayerNumber(joyshockHandle, 0);
            JslSetLightColour(joyshockHandle, 0);
        }

        public override void UpdateState()
        {
            reportTime = (DateTime.Now - startTime).TotalSeconds;
            lastInputStack.Capacity = inputStack.Count;
            lastInputStack = new(inputStack);
            wantClearInputStack = true;

            for (int i = 0; i < actionStates.Length; i++)
            {
                actionStates[i].isDelta = false;
                actionStates[i].debounce -= Time.deltaTime;
                if (actionStates[i].debounce < 0)
                    actionStates[i].debounce = 0;
            }
            for (int i = 0; i < buttonStates.Length; i++)
            {
                buttonStates[i].isDelta = false;
                buttonStates[i].debounce -= Time.deltaTime;
                if (buttonStates[i].debounce < 0)
                    buttonStates[i].debounce = 0;
            }

            foreach (TimestampedState state in lastInputStack)
            {
                joyBtStateCurrent = state.input;

                for (int i = 0; i < actionStates.Length; i++)
                {
                    int bt = GetButtonForAction(i);
                    if (bt != -1)
                    {
                        bool pressed = BitwiseUtils.WantCurrent(state.input.buttons, 1 << bt);
                        if (pressed != actionStates[i].pressed && !actionStates[i].isDelta)
                        {
                            if (actionStates[i].debounce <= 0)
                            {
                                actionStates[i].pressed = pressed;
                                actionStates[i].isDelta = true;
                                actionStates[i].dt = reportTime - state.timestamp;
                            }
                            actionStates[i].debounce = debounceTime;
                        }
                    }
                }

                for (int i = 0; i < buttonStates.Length; i++)
                {
                    bool pressed = BitwiseUtils.WantCurrent(state.input.buttons, 1 << i);
                    if (pressed != buttonStates[i].pressed && !buttonStates[i].isDelta)
                    {
                        if (buttonStates[i].debounce <= 0)
                        {
                            buttonStates[i].pressed = pressed;
                            buttonStates[i].isDelta = true;
                            buttonStates[i].dt = reportTime - state.timestamp;
                        }
                        buttonStates[i].debounce = debounceTime;
                    }
                }
            }

            //stick direction state, only handled on update
            //split controllers will need to be rotated to compensate
            //left rotates counterclockwise, right rotates clockwise, all by 90 degrees
            float xAxis = 0f;
            float yAxis = 0f;

            if (joyConWantRotatedStick && type is TypeJoyConLeft or TypeJoyConRight)
            {
                switch (type)
                {
                    case TypeJoyConLeft:
                        xAxis = -joyBtStateCurrent.stickLY;
                        yAxis = joyBtStateCurrent.stickLX;
                        break;
                    case TypeJoyConRight:
                        xAxis = joyBtStateCurrent.stickRY;
                        yAxis = -joyBtStateCurrent.stickRX;
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
            directionStateCurrent |= (yAxis >= STICK_DEAD) ? (1 << (int)ActionsPad.Up) : 0;
            directionStateCurrent |= (yAxis <= -STICK_DEAD) ? (1 << (int)ActionsPad.Down) : 0;
            directionStateCurrent |= (xAxis <= -STICK_DEAD) ? (1 << (int)ActionsPad.Left) : 0;
            directionStateCurrent |= (xAxis >= STICK_DEAD) ? (1 << (int)ActionsPad.Right) : 0;
            // Debug.Log($"{GetDeviceName()} stick direction: {directionStateCurrent}| x axis: {xAxis}, y axis: {yAxis}");

            lastInputStack.Clear();
        }

        public override void OnSelected()
        {
            SetRotatedStickMode(true);
            Task.Run(() => SelectionVibrate());
        }

        async void SelectionVibrate()
        {
            JslSetRumbleFrequency(GetHandle(), 0.5f, 0.5f, 80f, 160f);
            await Task.Delay(100);
            JslSetRumbleFrequency(GetHandle(), 0f, 0f, 160f, 320f);
        }

        public override string GetDeviceName()
        {
            return joyshockName;
        }

        public override string[] GetButtonNames()
        {
            switch (type)
            {
                case TypeProController:
                    return nsProButtonNames;
                case TypeDualShock4:
                    return ps4ButtonNames;
                case TypeDualSense:
                    return ps5ButtonNames;
                default:
                    return nsConButtonNames;
            }
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

        public override ControlBindings GetDefaultBindings()
        {
            ControlBindings binds = new ControlBindings();
            switch (type)
            {
                case TypeJoyConLeft:
                    binds.Pad = defaultMappingsL;
                    break;
                case TypeJoyConRight:
                    binds.Pad = defaultMappingsR;
                    break;
                case TypeProController:
                    binds.Pad = defaultMappings;
                    break;
                case TypeDualShock4:
                    binds.Pad = defaultMappings;
                    break;
                case TypeDualSense:
                    binds.Pad = defaultMappings;
                    break;
            }
            binds.PointerSensitivity = 3;
            binds.version = GetBindingsVersion();
            return binds;
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

        public override ControlBindings UpdateBindings(ControlBindings lastBinds)
        {
            if (lastBinds.version == 0)
            {
                switch (type)
                {
                    case TypeProController:
                    case TypeDualShock4:
                    case TypeDualSense:
                        return lastBinds;
                    case TypeJoyConLeft:
                    case TypeJoyConRight:
                        for (int i = 0; i < lastBinds.Pad.Length; i++)
                        {
                            if (lastBinds.Pad[i] is 18 or 19)
                                lastBinds.Pad[i] = lastBinds.Pad[i] + 1;
                        }
                        return lastBinds;
                }
            }
            return lastBinds;
        }

        public override int GetBindingsVersion()
        {
            return 1;
        }

        public override bool GetIsActionUnbindable(int action, ControlStyles style)
        {
            if (type is TypeJoyConLeft or TypeJoyConRight)
            {
                if (style is ControlStyles.Pad)
                {
                    return action is 0 or 1 or 2 or 3;
                }
            }
            return false;
        }

        public override int GetLastButtonDown(bool strict = false)
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

        public override int GetLastActionDown()
        {
            for (int i = 0; i < actionStates.Length; i++)
            {
                if (i is 0 or 1 or 2 or 3)
                {
                    if (BitwiseUtils.WantCurrentAndNotLast(directionStateCurrent, directionStateLast, 1 << i))
                    {
                        return i;
                    }
                }
                if (actionStates[i].pressed && actionStates[i].isDelta)
                {
                    return i;
                }
            }
            return -1;
        }

        public JoyshockButtonState GetButtonState(int button)
        {
            return buttonStates[button];
        }

        public JoyshockButtonState[] GetButtonStates()
        {
            return buttonStates;
        }

        public override bool GetAction(ControlStyles style, int action)
        {
            if (action == -1) { return false; }
            bool stick = false;
            if (action is 0 or 1 or 2 or 3)
            {
                stick = BitwiseUtils.WantCurrent(directionStateCurrent, 1 << action);
            }
            return actionStates[action].pressed || stick;
        }

        public override bool GetActionDown(ControlStyles style, int action, out double dt)
        {
            if (action == -1) { dt = 0; return false; }
            dt = actionStates[action].dt;
            bool bt = actionStates[action].pressed && actionStates[action].isDelta;
            if (bt)
            {
                return true;
            }
            else if (action is 0 or 1 or 2 or 3)
            {
                dt = 0;
                return BitwiseUtils.WantCurrentAndNotLast(directionStateCurrent, directionStateLast, 1 << action);
            }
            return false;
        }

        public override bool GetActionUp(ControlStyles style, int button, out double dt)
        {
            if (button == -1) { dt = 0; return false; }
            dt = actionStates[button].dt;
            bool bt = !actionStates[button].pressed && actionStates[button].isDelta;
            if (bt)
            {
                return true;
            }
            else if (button is 0 or 1 or 2 or 3)
            {
                dt = 0;
                return BitwiseUtils.WantNotCurrentAndLast(directionStateCurrent, directionStateLast, 1 << button);
            }
            return false;
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
                case InputAxis.PointerX:   //isn't updated for now, so always returns 0f
                //return joyTouchStateCurrent.t0X;
                case InputAxis.PointerY:
                //return joyTouchStateCurrent.t0Y;
                default:
                    return 0f;
            }
        }

        public override Vector3 GetVector(InputVector vec)
        {
            switch (vec)
            {
                case InputVector.LStick:
                    return new Vector3(joyBtStateCurrent.stickLX, joyBtStateCurrent.stickLY, 0f);
                case InputVector.RStick:
                    return new Vector3(joyBtStateCurrent.stickRX, joyBtStateCurrent.stickRY, 0f);
                case InputVector.Pointer:
                    return new Vector3(joyTouchStateCurrent.t0X, joyTouchStateCurrent.t0Y, 0f);
                case InputVector.Accelerometer:
                    return new Vector3(joyImuStateCurrent.accelX, joyImuStateCurrent.accelY, joyImuStateCurrent.accelZ);
                case InputVector.Gyroscope:
                    return new Vector3(joyImuStateCurrent.gyroX, joyImuStateCurrent.gyroY, joyImuStateCurrent.gyroZ);
            }
            return Vector3.zero;
        }

        public override Vector2 GetPointer()
        {
            Camera cam = GameManager.instance.CursorCam;
            Vector3 rawPointerPos = Input.mousePosition;
            rawPointerPos.z = Mathf.Abs(cam.gameObject.transform.position.z);
            return cam.ScreenToWorldPoint(rawPointerPos);
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
            int ledMask = (int)this.playerNum;
            if (type == TypeDualSense)
            {
                if (playerNum <= 5)
                {
                    ledMask = DualSensePlayerMask[Math.Max((int)this.playerNum + 1, 1)];
                }
            }
            JslSetPlayerNumber(joyshockHandle, ledMask);
            lightbarColour = GetLightbarColourForPlayer((int)this.playerNum);
            JslSetLightColour(joyshockHandle, lightbarColour);
        }

        public override int? GetPlayer()
        {
            return this.playerNum;
        }

        public Color GetBodyColor()
        {
            return BitwiseUtils.IntToRgb(joySettings.bodyColour);
        }

        public Color GetButtonColor()
        {
            if (joySettings.buttonColour == 0xFFFFFF)
                return GetBodyColor();
            return BitwiseUtils.IntToRgb(joySettings.buttonColour);
        }

        public Color GetLeftGripColor()
        {
            if (joySettings.lGripColour == 0xFFFFFF)
                return GetBodyColor();
            return BitwiseUtils.IntToRgb(joySettings.lGripColour);
        }

        public Color GetRightGripColor()
        {
            if (joySettings.rGripColour == 0xFFFFFF)
                return GetBodyColor();
            return BitwiseUtils.IntToRgb(joySettings.rGripColour);
        }

        public Color GetLightbarColour()
        {
            return BitwiseUtils.IntToRgb(lightbarColour);
        }

        public int GetJoyshockType()
        {
            return type;
        }

        public void SetRotatedStickMode(bool rotated)
        {
            joyConWantRotatedStick = rotated;
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
            JslSetRumble(joyshockHandle, 0, 0);
            JslSetLightColour(joyshockHandle, 0);
            JslSetPlayerNumber(joyshockHandle, 0);
        }

        public override bool GetFlick(out double dt)
        {
            dt = 0;
            return false;
        }

        public override bool GetSlide(out double dt)
        {
            dt = 0;
            return false;
        }

        public override void SetMaterialProperties(Material m)
        {
            Color colour;
            switch (GetDeviceName())
            {
                case "Joy-Con (L)":
                case "Joy-Con (R)":
                    m.SetColor("_BodyColor", GetBodyColor());
                    m.SetColor("_BtnColor", GetButtonColor());
                    m.SetColor("_LGripColor", ColorUtility.TryParseHtmlString("#2F353A", out colour) ? colour : Color.white);
                    m.SetColor("_RGripColor", ColorUtility.TryParseHtmlString("#2F353A", out colour) ? colour : Color.white);
                    break;
                case "DualShock 4":
                    m.SetColor("_BodyColor", ColorUtility.TryParseHtmlString("#E1E2E4", out colour) ? colour : Color.white);
                    m.SetColor("_BtnColor", ColorUtility.TryParseHtmlString("#414246", out colour) ? colour : Color.white);
                    m.SetColor("_LGripColor", GetLightbarColour());
                    m.SetColor("_RGripColor", GetLightbarColour());
                    break;
                case "DualSense":
                    m.SetColor("_BodyColor", ColorUtility.TryParseHtmlString("#DEE0EB", out colour) ? colour : Color.white);
                    m.SetColor("_BtnColor", ColorUtility.TryParseHtmlString("#272D39", out colour) ? colour : Color.white);
                    m.SetColor("_LGripColor", GetLightbarColour());
                    m.SetColor("_RGripColor", GetLightbarColour());
                    break;
                default:
                    m.SetColor("_BodyColor", GetBodyColor());
                    m.SetColor("_BtnColor", GetButtonColor());
                    m.SetColor("_LGripColor", GetLeftGripColor());
                    m.SetColor("_RGripColor", GetRightGripColor());
                    break;
            }
        }

        public override bool GetCurrentStyleSupported()
        {
            return PlayerInput.CurrentControlStyle is ControlStyles.Pad; // or ControlStyles.Baton
        }

        public override ControlStyles GetDefaultStyle()
        {
            return ControlStyles.Pad;
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
        }

        public override void RecentrePointer()
        {
        }

        public override bool GetPointerLeftRight()
        {
            return false;
        }
    }
}