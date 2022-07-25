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
        //Buttons and Axis used by most controllers
        public enum InputButtons : int
        {
            ButtonPadUp = 0,
            ButtonPadDown = 1,
            ButtonPadLeft = 2,
            ButtonPadRight = 3,
            ButtonPlus = 4,
            ButtonOptions = 4,
            ButtonMinus = 5,
            ButtonShare = 5,
            ButtonLClick = 6,
            ButtonRClick = 7,
            ButtonL = 8,
            ButtonR = 9,
            ButtonZL = 10,
            ButtonZR = 11,
            ButtonFaceS = 12,
            ButtonFaceE = 13,
            ButtonFaceW = 14,
            ButtonFaceN = 15,
            ButtonHome = 16,
            ButtonPS = 16,
            ButtonCapture = 17,
            ButtonTouchpadClick = 17,
            ButtonSL = 18,
            ButtonSR = 19,
        }
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
            Readable_ShellColour,
            Readable_ButtonColour,
            Readable_LeftGripColour,
            Readable_RightGripColour,
            Readable_AnalogueTriggers,
            Readable_StringInput,
            Readable_Pointer,
            Readable_MotionSensor,

            //writable properties
            Writable_PlayerLED,
            Writable_LightBar,
            Writable_Chroma,
            Writable_Speaker,

            //other / "special" properties
            Extra_SplitControllerLeft,
            Extra_SplitControllerRight,
            Extra_Rumble,
            Extra_HDRumble,

            //supported control styles
            Style_Pad,
            Style_Baton,
            Style_Touch
        };

        //Following enums are specific to Heaven Studio, can be removed in other applications
        //Control styles in Heaven Studio
        public enum ControlStyles
        {
            Pad,
            Baton,
            Touch
        }

        //buttons used in Heaven Studio gameplay (Pad Style)
        public enum ButtonsPad : int
        {
            PadUp = 0,
            PadDown = 1,
            PadLeft = 2,
            PadRight = 3,
            PadS = 4,
            PadE = 5,
            PadW = 6,
            PadN = 7,
            PadL = 8,
            PadR = 9,
            PadPause = 10,
        }

        //FUTURE: buttons used in Heaven Studio gameplay ("Form Baton" / WiiMote Style)
        public enum ButtonsBaton : int
        {
            BatonS = 0, //-- all these...
            BatonE = 1, // |
            BatonW = 2, // |
            BatonN = 3, //--
            BatonA = 4, // < ...map to this, but are directional
            BatonB = 5, // should never be used alone
            Baton1 = 6,
            Baton2 = 7,
            BatonPause = 8,
        }

        //FUTURE: buttons used in Heaven Studio gameplay (Touch Style)
        public enum ButtonsTouch : int
        {
            TouchL = 0,
            TouchR = 1,
            TouchTap = 2,
            TouchFlick = 3,
            TouchButtonL = 4,
            TouchButtonR = 4,
        }
    
        protected int? playerNum;
        protected int directionStateCurrent = 0;
        protected int directionStateLast = 0;

        public abstract void InitializeController();
        public abstract void UpdateState(); // Update the state of the controller

        public abstract string GetDeviceName(); // Get the name of the controller
        public abstract InputFeatures GetFeatures(); // Get the features of the controller

        public abstract int GetLastButtonDown();    // Get the last button down
        public abstract KeyCode GetLastKeyDown();   // Get the last key down (used for keyboards and other devices that use Keycode)
        public abstract bool GetButton(int button); // is button currently pressed?
        public abstract bool GetButtonDown(int button); // is button just pressed?
        public abstract bool GetButtonUp(int button);   // is button just released?
        public abstract float GetAxis(InputAxis axis);    // Get the value of an axis
        public abstract bool GetHatDirection(InputDirection direction);    // is direction active?
        public abstract bool GetHatDirectionDown(InputDirection direction); // direction just became active?
        public abstract bool GetHatDirectionUp(InputDirection direction);  // direction just became inactive?

        public abstract void SetPlayer(int? playerNum);  // Set the player number (starts at 1, set to -1 or null for no player)
        public abstract int? GetPlayer();            // Get the player number (null if no player)

        //public abstract Sprite GetDisplayIcon();    //"big icon" for the controller in the settings menu
        //public abstract Sprite GetPlaybackIcon();   //"small icon" for the controller during playback
    }
}