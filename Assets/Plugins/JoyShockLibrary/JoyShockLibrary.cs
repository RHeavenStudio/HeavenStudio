using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public static class JSL
{
    public const int ButtonMaskUp = 0;
    public const int ButtonMaskDown = 1;
    public const int ButtonMaskLeft = 2;
    public const int ButtonMaskRight = 3;
    public const int ButtonMaskPlus = 4;
    public const int ButtonMaskOptions = 4;
    public const int ButtonMaskMinus = 5;
    public const int ButtonMaskShare = 5;
    public const int ButtonMaskLClick = 6;
    public const int ButtonMaskRClick = 7;
    public const int ButtonMaskL = 8;
    public const int ButtonMaskR = 9;
    public const int ButtonMaskZL = 10;
    public const int ButtonMaskZR = 11;
    public const int ButtonMaskS = 12;
    public const int ButtonMaskE = 13;
    public const int ButtonMaskW = 14;
    public const int ButtonMaskN = 15;
    public const int ButtonMaskHome = 16;
    public const int ButtonMaskPS = 16;
    public const int ButtonMaskCapture = 17;
    public const int ButtonMaskTouchpadClick = 17;
    public const int ButtonMaskSL = 18;
    public const int ButtonMaskSR = 19;

    public const int TypeJoyConLeft = 1;
    public const int TypeJoyConRight = 2;
    public const int TypeProController = 3;
    public const int TypeDualShock4 = 4;
    public const int TypeDualSense = 5;

    public const int SplitLeft = 1;
    public const int SplitRight = 2;
    public const int SplitFull = 3;

    // PS5 Player maps for the DS Player Lightbar
    public static readonly int[] DualSensePlayerMask = {
        4,
        10,
        21,
        27,
        31
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct JOY_SHOCK_STATE
    {
        public int buttons;
        public float lTrigger;
        public float rTrigger;
        public float stickLX;
        public float stickLY;
        public float stickRX;
        public float stickRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMU_STATE
    {
        public float accelX;
        public float accelY;
        public float accelZ;
        public float gyroX;
        public float gyroY;
        public float gyroZ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOTION_STATE {
        public float quatW;
        public float quatX;
        public float quatY;
        public float quatZ;
        public float accelX;
        public float accelY;
        public float accelZ;
        public float gravX;
        public float gravY;
        public float gravZ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOUCH_STATE {
        public int t0Id;
        public int t1Id;
        public bool t0Down;
        public bool t1Down;
        public float t0X;
        public float t0Y;
        public float t1X;
        public float t1Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JSL_AUTO_CALIBRATION {
        public float confidence;
        public bool autoCalibrationEnabled;
        public bool isSteady;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JSL_SETTINGS {
        public int gyroSpace;
        public int bodyColour;
        public int lGripColour;
        public int rGripColour;
        public int buttonColour;
        public int playerNumber;
        public int controllerType;
        public int splitType;
        public bool isCalibrating;
        public bool autoCalibrationEnabled;
        public bool isConnected;
    }

    public delegate void EventCallback(int handle, JOY_SHOCK_STATE state, JOY_SHOCK_STATE lastState,
        IMU_STATE imuState, IMU_STATE lastImuState, float deltaTime);
    
    public delegate void TouchCallback(int handle, TOUCH_STATE state, TOUCH_STATE lastState, float deltaTime);

    public delegate void ConnectionCallback(int handle);
    public delegate void DeconnectionCallback(int handle, bool isConnected);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern int JslConnectDevices();
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern int JslGetConnectedDeviceHandles(int[] deviceHandleArray, int size);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslDisconnectAndDisposeAll();
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool JslStillConnected(int deviceId);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern JOY_SHOCK_STATE JslGetSimpleState(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern IMU_STATE JslGetIMUState(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern MOTION_STATE JslGetMotionState(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern TOUCH_STATE JslGetTouchState(int deviceId);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetGyroSpace(int deviceId, int gyroSpace);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JslGetStickStep(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JslGetTriggerStep(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JslGetPollRate(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JslGetTimeSinceLastUpdate(int deviceId);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JslGetTouchId(int deviceId, bool secondTouch = false);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JslGetTouchDown(int deviceId, bool secondTouch = false);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JslGetTouchX(int deviceId, bool secondTouch = false);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JslGetTouchY(int deviceId, bool secondTouch = false);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslResetContinuousCalibration(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslStartContinuousCalibration(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslPauseContinuousCalibration(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslGetCalibrationOffset(int deviceId, ref float xOffset, ref float yOffset, ref float zOffset);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslGetCalibrationOffset(int deviceId, float xOffset, float yOffset, float zOffset);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern JSL_AUTO_CALIBRATION JslGetAutoCalibrationStatus(int deviceId);

    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetCallback(EventCallback callback);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetTouchCallback(TouchCallback callback);
    // this function will get called for each device when it is newly connected
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetConnectCallback(ConnectionCallback callback);
    // this function will get called for each device when it is disconnected
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetDisconnectCallback(DeconnectionCallback callback);
    
    // super-getter for reading a whole lot of state at once
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern JSL_SETTINGS JslGetControllerInfoAndSettings(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern int JslGetControllerType(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern int JslGetControllerSplitType(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern int JslGetControllerColour(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern int JslGetControllerButtonColour(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern int JslGetControllerLeftGripColour(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern int JslGetControllerRightGripColour(int deviceId);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetLightColour(int deviceId, int colour);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetRumble(int deviceId, int smallRumble, int bigRumble);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetRumbleFrequency(int deviceId, float smallRumble, float bigRumble, float smallFrequency, float bigFrequency);
    [DllImport("JoyShockLibrary", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JslSetPlayerNumber(int deviceId, int number);
}