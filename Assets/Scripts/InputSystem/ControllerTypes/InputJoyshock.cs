using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

using static JSL;

namespace HeavenStudio.InputSystem
{
    public static class InputJoyshockInitializer
    {
        [LoadOrder(1)]
        public static InputController[] Initialize()
        {
            PlayerInput.PlayerInputCleanUp += DisposeJoyshocks;

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
            JslDisconnectAndDisposeAll();
        }
    }

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
        int[] mappings = new[]
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
        };
        int[] mappingsSplitLeft = new[]
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
        };
        int[] mappingsSplitRight = new[]
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
        };

        float stickDeadzone = 0.5f;

        int joyshockHandle;
        int type;
        int splitType;
        int lightbarColour;
        string joyshockName;

        //buttons, sticks, triggers
        JOY_SHOCK_STATE joyBtStateCurrent, joyBtStateLast;
        //gyro and accelerometer
        IMU_STATE joyImuStateCurrent, joyImuStateLast;
        //touchpad
        TOUCH_STATE joyTouchStateCurrent, joyTouchStateLast;

        InputJoyshock otherHalf;

        public InputJoyshock(int handle)
        {
            joyshockHandle = handle;
        }

        public override void InitializeController()
        {
            joyBtStateCurrent = new JOY_SHOCK_STATE();
            joyBtStateLast = new JOY_SHOCK_STATE();
            joyImuStateCurrent = new IMU_STATE();
            joyImuStateLast = new IMU_STATE();
            joyTouchStateCurrent = new TOUCH_STATE();
            joyTouchStateLast = new TOUCH_STATE();

            //FUTURE: remappable controls

            type = JslGetControllerType(joyshockHandle);
            joyshockName = joyShockNames[type];

            splitType = JslGetControllerSplitType(joyshockHandle);
        }

        public override void UpdateState()
        {
            //buttons
            joyBtStateLast = joyBtStateCurrent;
            joyBtStateCurrent = JslGetSimpleState(joyshockHandle);

            //gyro and accelerometer
            joyImuStateLast = joyImuStateCurrent;
            joyImuStateCurrent = JslGetIMUState(joyshockHandle);

            //touchpad
            joyTouchStateLast = joyTouchStateCurrent;
            joyTouchStateCurrent = JslGetTouchState(joyshockHandle);

            //stick direction state
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

        public override int GetLastButtonDown()
        {
            return BitwiseUtils.FirstSetBit(joyBtStateCurrent.buttons & joyBtStateLast.buttons);
        }

        public override KeyCode GetLastKeyDown()
        {
            return KeyCode.None;
        }

        public override bool GetButton(int button)
        {
            int bt = 0;
            if (otherHalf == null)
            {
                if (splitType == SplitLeft)
                {
                    bt = mappingsSplitLeft[button];
                }
                else if (splitType == SplitRight)
                {
                    bt = mappingsSplitRight[button];
                }
                else
                {
                    bt = mappings[button];
                }
                return BitwiseUtils.WantCurrent(joyBtStateCurrent.buttons, 1 << bt);
            }
            bt = mappings[button];
            return BitwiseUtils.WantCurrent(joyBtStateCurrent.buttons, 1 << bt) || BitwiseUtils.WantCurrent(otherHalf.joyBtStateCurrent.buttons, 1 << bt);
        }

        public override bool GetButtonDown(int button)
        {
            int bt = 0;
            if (otherHalf == null)
            {
                if (splitType == SplitLeft)
                {
                    bt = mappingsSplitLeft[button];
                }
                else if (splitType == SplitRight)
                {
                    bt = mappingsSplitRight[button];
                }
                else
                {
                    bt = mappings[button];
                }
                return BitwiseUtils.WantCurrentAndNotLast(joyBtStateCurrent.buttons, joyBtStateLast.buttons, 1 << bt);
            }
            bt = mappings[button];
            return BitwiseUtils.WantCurrentAndNotLast(joyBtStateCurrent.buttons, joyBtStateLast.buttons, 1 << bt) || BitwiseUtils.WantCurrentAndNotLast(otherHalf.joyBtStateCurrent.buttons, otherHalf.joyBtStateLast.buttons, 1 << bt);
        }

        public override bool GetButtonUp(int button)
        {
            int bt = 0;
            if (otherHalf == null)
            {
                if (splitType == SplitLeft)
                {
                    bt = mappingsSplitLeft[button];
                }
                else if (splitType == SplitRight)
                {
                    bt = mappingsSplitRight[button];
                }
                else
                {
                    bt = mappings[button];
                }
                return BitwiseUtils.WantNotCurrentAndLast(joyBtStateCurrent.buttons, joyBtStateLast.buttons, 1 << bt);
            }
            bt = mappings[button];
            return BitwiseUtils.WantNotCurrentAndLast(joyBtStateCurrent.buttons, joyBtStateLast.buttons, 1 << bt) || BitwiseUtils.WantNotCurrentAndLast(otherHalf.joyBtStateCurrent.buttons, otherHalf.joyBtStateLast.buttons, 1 << bt);
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
            //todo: check analogue stick hat direction too
            int bt;
            switch (direction)
            {
                case InputDirection.Up:
                    bt = mappings[0];
                    break;
                case InputDirection.Down:
                    bt = mappings[1];
                    break;
                case InputDirection.Left:
                    bt = mappings[2];
                    break;
                case InputDirection.Right:
                    bt = mappings[3];
                    break;
                default:
                    return false;
            }
            return GetButton(bt) || BitwiseUtils.WantCurrent(directionStateCurrent, 1 << (int) direction);
        }

        public override bool GetHatDirectionDown(InputDirection direction)
        {
            //todo: check analogue stick hat direction too
            int bt;
            switch (direction)
            {
                case InputDirection.Up:
                    bt = mappings[0];
                    break;
                case InputDirection.Down:
                    bt = mappings[1];
                    break;
                case InputDirection.Left:
                    bt = mappings[2];
                    break;
                case InputDirection.Right:
                    bt = mappings[3];
                    break;
                default:
                    return false;
            }
            return GetButtonDown(bt) || BitwiseUtils.WantCurrentAndNotLast(directionStateCurrent, directionStateLast, 1 << (int) direction);
        }

        public override bool GetHatDirectionUp(InputDirection direction)
        {
            //todo: check analogue stick hat direction too
            int bt;
            switch (direction)
            {
                case InputDirection.Up:
                    bt = mappings[0];
                    break;
                case InputDirection.Down:
                    bt = mappings[1];
                    break;
                case InputDirection.Left:
                    bt = mappings[2];
                    break;
                case InputDirection.Right:
                    bt = mappings[3];
                    break;
                default:
                    return false;
            }
            return GetButtonUp(bt) || BitwiseUtils.WantNotCurrentAndLast(directionStateCurrent, directionStateLast, 1 << (int) direction);
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
                return BitwiseUtils.IntToRgb(splitType == SplitRight ? JslGetControllerColour(joyshockHandle) : JslGetControllerColour(GetOtherHalf().GetHandle()));
            }
            return BitwiseUtils.IntToRgb(JslGetControllerColour(joyshockHandle));
        }

        public Color GetButtonColor()
        {
            return BitwiseUtils.IntToRgb(JslGetControllerButtonColour(joyshockHandle));
        }

        public Color GetLeftGripColor()
        {
            if (otherHalf != null)
            {
                return BitwiseUtils.IntToRgb(splitType == SplitLeft ? JslGetControllerColour(joyshockHandle) : JslGetControllerColour(GetOtherHalf().GetHandle()));
            }
            return BitwiseUtils.IntToRgb(JslGetControllerLeftGripColour(joyshockHandle));
        }

        public Color GetRightGripColor()
        {
            if (otherHalf != null)
            {
                return BitwiseUtils.IntToRgb(splitType == SplitRight ? JslGetControllerColour(joyshockHandle) : JslGetControllerColour(GetOtherHalf().GetHandle()));
            }
            return BitwiseUtils.IntToRgb(JslGetControllerRightGripColour(joyshockHandle));
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