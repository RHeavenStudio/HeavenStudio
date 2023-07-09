using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            TouchpadX = 6,
            TouchpadY = 7
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
            Readable_ShellColour        = 1 << 0,
            Readable_ButtonColour       = 1 << 1,
            Readable_LeftGripColour     = 1 << 2,
            Readable_RightGripColour    = 1 << 3,
            Readable_AnalogueTriggers   = 1 << 4,
            Readable_StringInput        = 1 << 5,
            Readable_Pointer            = 1 << 6,
            Readable_MotionSensor       = 1 << 7,

            //writable properties
            Writable_PlayerLED          = 1 << 8,
            Writable_LightBar           = 1 << 9,
            Writable_Chroma             = 1 << 10,
            Writable_Speaker            = 1 << 11,

            //other / "special" properties
            Extra_SplitControllerLeft   = 1 << 12,
            Extra_SplitControllerRight  = 1 << 13,
            Extra_Rumble                = 1 << 14,
            Extra_HDRumble              = 1 << 15,

            //supported control styles
            Style_Pad                   = 1 << 16,
            Style_Baton                 = 1 << 17,
            Style_Touch                 = 1 << 18,
        };

        //Following enums are specific to Heaven Studio, can be removed in other applications
        //Control styles in Heaven Studio
        public enum ControlStyles
        {
            Pad,
            Baton,
            Touch,
            Move
        }

        public const int BINDS_MAX = 12; //maximum number of binds per controller

        //buttons used in Heaven Studio gameplay (Pad Style)
        public enum ButtonsPad : int
        {
            Up = 0,
            Down = 1,
            Left = 2,
            Right = 3,
            South = 4,
            East = 5,
            West = 6,
            North = 7,
            L = 8,
            R = 9,
            Pause = 10,
        }

        //FUTURE: buttons used in Heaven Studio gameplay ("Form Baton" / WiiMote Style)
        public enum ButtonsBaton : int
        {
            South = 0,      //-- all these...
            East = 1,       // |
            West = 2,       // |
            North = 3,      //--
            Face = 4,       // < ...are also equivalent to this, but with added directionality
            Trigger = 5,    // should never be used alone, but still bindable separately (controller logic should handle confirming & timestamping face + trigger input)
            Up = 6,     // Wiimote 1
            Down = 7,   // Wiimote 2
            Pause = 8,
        }

        //FUTURE: buttons used in Heaven Studio gameplay (Touch Style)
        public enum ButtonsTouch : int
        {
            Tap = 0,   // flicks are handled like a motion, don't have a binding
            Left = 1,     // also maps to tap, but with directionality (tap the left side of the panel)
            Right = 2,    // also maps to tap, but with directionality (tap the right side of the panel)
            ButtonL = 3,
            ButtonR = 4,
            Pause = 5,
        }

        [System.Serializable]
        public struct ControlBindings
        {
            public string ControllerName;
            public int[] Pad;
            public int[] Baton;
            public int[] Touch;
        }

        // FUTURE: Move Style needs to be implemented per-game (maybe implement checks for common actions?)

        protected int currentInputFlags = 0;
        protected int lastInputFlags = 0;

        protected int? playerNum;
        protected int directionStateCurrent = 0;
        protected int directionStateLast = 0;

        public abstract void InitializeController();
        public abstract void UpdateState(); // Update the state of the controller

        public abstract string GetDeviceName(); // Get the name of the controller
        public abstract InputFeatures GetFeatures(); // Get the features of the controller
        public abstract bool GetIsConnected();
        public abstract bool GetIsPoorConnection();

        // public abstract int[] GetDefaultMappings(ControlStyles style);
        // public abstract int[] GetCurrentMappings(ControlStyles style);
        // public abstract int[] SetCurrentMappings(ControlStyles style);

        public abstract int GetLastButtonDown();    // Get the last button down
        public abstract KeyCode GetLastKeyDown();   // Get the last key down (used for keyboards and other devices that use Keycode)
        public abstract bool GetButton(int button); // is button currently pressed?
        public abstract bool GetButtonDown(int button, out double dt); // is button just pressed?
        public abstract bool GetButtonUp(int button, out double dt);   // is button just released?
        public abstract float GetAxis(InputAxis axis);    // Get the value of an axis
        public abstract bool GetHatDirection(InputDirection direction);    // is direction active?
        public abstract bool GetHatDirectionDown(InputDirection direction, out double dt); // direction just became active?
        public abstract bool GetHatDirectionUp(InputDirection direction, out double dt);  // direction just became inactive?

        public abstract void SetPlayer(int? playerNum);  // Set the player number (starts at 1, set to -1 or null for no player)
        public abstract int? GetPlayer();            // Get the player number (null if no player)

        //public abstract Sprite GetDisplayIcon();    //"big icon" for the controller in the settings menu
        //public abstract Sprite GetPlaybackIcon();   //"small icon" for the controller during playback
    }
}