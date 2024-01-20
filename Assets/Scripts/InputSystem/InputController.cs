using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace HeavenStudio.InputSystem
{
    /// <summary>
    /// Generic class to allow adapting any type and combination of HIDs to a universal controller format.
    /// Specifically designed for Heaven Studio, but can be adapted to any use.
    /// </summary>
    public abstract class InputController
    {
        public enum InputAxis : int
        {
            AxisLTrigger = 4,
            AxisRTrigger = 5,
            AxisLStickX = 0,
            AxisLStickY = 1,
            AxisRStickX = 2,
            AxisRStickY = 3,
            PointerX = 6,
            PointerY = 7,
            PointerDeltaX = 8,
            PointerDeltaY = 9
        }

        public enum InputVector : int
        {
            LStick = 0,
            RStick = 1,
            Pointer = 2,
            PointerSpeed = 3,
            Gyroscope = 4,
            Accelerometer = 5,
        }

        //D-Pad directions, usable to adapt analogue sticks to cardinal directions
        public enum InputDirection : int
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3,
        }

        //Common specific controller features
        [System.Flags]
        public enum InputFeatures
        {
            //readable properties
            Readable_ShellColour = 1 << 0,
            Readable_ButtonColour = 1 << 1,
            Readable_LeftGripColour = 1 << 2,
            Readable_RightGripColour = 1 << 3,
            Readable_AnalogueTriggers = 1 << 4,
            Readable_StringInput = 1 << 5,
            Readable_Pointer = 1 << 6,
            Readable_MotionSensor = 1 << 7,

            //writable properties
            Writable_PlayerLED = 1 << 8,
            Writable_LightBar = 1 << 9,
            Writable_Chroma = 1 << 10,
            Writable_Speaker = 1 << 11,

            //other / "special" properties
            Extra_SplitControllerLeft = 1 << 12,
            Extra_SplitControllerRight = 1 << 13,
            Extra_Rumble = 1 << 14,
            Extra_HDRumble = 1 << 15,

            //supported control styles
            Style_Pad = 1 << 16,
            Style_Baton = 1 << 17,
            Style_Touch = 1 << 18,
        };

        //Following enums are specific to Heaven Studio, can be removed in other applications
        //Control styles in Heaven Studio
        public enum ControlStyles
        {
            Pad,
            Touch,
            Baton,
            // Move
        }

        public const int BINDS_MAX = 12; //maximum number of binds per controller

        //buttons used in Heaven Studio gameplay (Pad Style)
        public enum ActionsPad : int
        {
            Up = 0,
            Down = 1,
            Left = 2,
            Right = 3,
            South = 4,
            East = 5,
            West = 6,
            North = 7,
            ButtonL = 8,
            ButtonR = 9,
            Pause = 10,
        }

        //buttons used in Heaven Studio gameplay ("Form Baton" / WiiMote Style)
        public enum ActionsBaton : int
        {
            North = 0,      //-- all these...
            South = 1,      // |
            West = 2,       // |
            East = 3,       //--
            Trigger = 4,    // < ...are also equivalent to this, but with added directionality
            Face = 5,
            Down = 6,     // Wiimote 1
            Up = 7,   // Wiimote 2
            Pause = 8,
        }

        //buttons used in Heaven Studio gameplay (Touch Style)
        public enum ActionsTouch : int
        {
            // flicks and swipes are handled as motions, and don't have bindings
            Tap = 0,
            Left = 1,     // also maps to tap, but with directionality (tap the left side of the panel). internally, maps to Pointer 1
            Right = 2,    // also maps to tap, but with directionality (tap the right side of the panel). internally, maps to Pointer 2
            ButtonL = 3,
            ButtonR = 4,
            PointerReset = 5,
            Pause = 6,
        }

        //buttons used in Heaven Studio gameplay (Move Style)
        public enum ActionsMove : int
        {
            Button = 0,
            Pause = 1,
        }

        [System.Serializable]
        public struct ControlBindings
        {
            public int[] Pad;
            public int[] Baton;
            public int[] Touch;
            public int[] Move;
            public float PointerSensitivity;
        }

        // FUTURE: Move Style needs to be implemented per-game (maybe implement checks for common actions?)

        protected ControlBindings currentBindings;

        protected int? playerNum;
        protected int directionStateCurrent = 0;
        protected int directionStateLast = 0;

        public abstract void InitializeController();
        public abstract void UpdateState(); // Update the state of the controller

        public abstract void OnSelected();

        public abstract string GetDeviceName(); // Get the name of the controller
        public abstract string[] GetButtonNames(); // Get the names of the buttons on the controller
        public abstract InputFeatures GetFeatures(); // Get the features of the controller
        public abstract bool GetIsConnected();
        public abstract bool GetIsPoorConnection();

        public void SaveBindings()
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/controls"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/controls");
            string path = $"{Application.persistentDataPath}/controls/{GetDeviceName()}.json";
            string json = JsonUtility.ToJson(currentBindings);
            File.WriteAllText(path, json);
        }

        public void LoadBindings()
        {
            string path = $"{Application.persistentDataPath}/controls/{GetDeviceName()}.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                if (json is not null or "")
                {
                    currentBindings = JsonUtility.FromJson<ControlBindings>(json);
                }
                else
                {
                    ResetBindings();
                }
            }
            else
            {
                ResetBindings();
            }
        }

        /// <summary>
        /// Gets the controller's default mappings
        /// </summary>
        /// <returns></returns>
        public abstract ControlBindings GetDefaultBindings();

        /// <summary>
        /// Resets the controller's mappings to default
        /// </summary>
        public abstract void ResetBindings();

        /// <summary>
        /// Gets the controller's current mappings
        /// </summary>
        /// <returns></returns>
        public abstract ControlBindings GetCurrentBindings();

        /// <summary>
        /// Sets the controller's current mappings
        /// </summary>
        /// <param name="newBinds"></param>
        public abstract void SetCurrentBindings(ControlBindings newBinds);

        /// <summary>
        /// Whether a given action can have be rebount
        /// </summary>
        /// <param name="action">action to check</param>
        /// <param name="style">control style to check</param>
        /// <returns></returns>
        public abstract bool GetIsActionUnbindable(int action, ControlStyles style);

        /// <summary>
        /// Gets the last pressed physical button
        /// </summary>
        /// <returns></returns>
        public abstract int GetLastButtonDown(bool strict = false);

        /// <summary>
        /// Gets the last pressed virtual action
        /// </summary>
        /// <returns></returns>
        public abstract int GetLastActionDown();

        /// <summary>
        /// True if the given action is being held
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public abstract bool GetAction(ControlStyles style, int action);

        /// <summary>
        /// True if the action was just pressed this Update
        /// </summary>
        /// <param name="action"></param>
        /// <param name="dt">time since the reported event, use to compensate for controller delays</param>
        /// <returns></returns>
        public abstract bool GetActionDown(ControlStyles style, int action, out double dt);

        /// <summary>
        /// True if the action was just released this Update
        /// </summary>
        /// <param name="action"></param>
        /// <param name="dt">time since the reported event, use to compensate for controller delays</param>
        /// <returns></returns>
        public abstract bool GetActionUp(ControlStyles style, int action, out double dt);

        /// <summary>
        /// Get the value of an analogue axis
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public abstract float GetAxis(InputAxis axis);

        /// <summary>
        /// Get the value of a 3d vector (2d vectors have z set to 0)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public abstract Vector3 GetVector(InputVector vector);

        /// <summary>
        /// Get the value of a pointer mapped to world coordinates
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetPointer();
        public abstract bool GetPointerLeftRight();
        public abstract void TogglePointerLock(bool locked);
        public abstract void RecentrePointer();

        /// <summary>
        /// True if the current direction is active
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public abstract bool GetHatDirection(InputDirection direction);

        /// <summary>
        /// True if the current direction just became active this Update
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="dt">time since the reported event, use to compensate for controller delays</param>
        /// <returns></returns>
        public abstract bool GetHatDirectionDown(InputDirection direction, out double dt);

        /// <summary>
        /// True if the current direction just became inactive this Update
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="dt">time since the reported event, use to compensate for controller delays</param>
        /// <returns></returns>
        public abstract bool GetHatDirectionUp(InputDirection direction, out double dt);

        public abstract bool GetFlick(out double dt);
        public abstract bool GetSlide(out double dt);
        public abstract bool GetSqueezeDown(out double dt);
        public abstract bool GetSqueezeUp(out double dt);
        public abstract bool GetSqueeze();

        /// <summary>
        /// Sets the player number (starts at 1, set to -1 or null for no player)
        /// </summary>
        /// <param name="playerNum"></param>
        public abstract void SetPlayer(int? playerNum);

        /// <summary>
        /// Gets the player number (starts at 1, -1 or null for no player)
        /// </summary>
        /// <returns></returns>
        public abstract int? GetPlayer();

        public abstract void SetMaterialProperties(Material m);

        public abstract bool GetCurrentStyleSupported();
        public abstract ControlStyles GetDefaultStyle();
    }
}