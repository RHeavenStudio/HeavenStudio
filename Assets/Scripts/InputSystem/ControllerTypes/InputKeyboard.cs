using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static JSL;

namespace HeavenStudio.InputSystem.Loaders
{
    public static class InputKeyboardInitializer
    {
        [LoadOrder(0)]
        public static InputController[] Initialize()
        {
            PlayerInput.PlayerInputRefresh.Add(Refresh);

            InputKeyboard keyboard = new InputKeyboard();
            keyboard.SetPlayer(1);
            keyboard.InitializeController();
            return new InputController[] { keyboard };
        }

        public static InputController[] Refresh()
        {
            InputKeyboard keyboard = new InputKeyboard();
            keyboard.SetPlayer(1);
            keyboard.InitializeController();
            return new InputController[] { keyboard };
        }
    }
}

namespace HeavenStudio.InputSystem
{
    public class InputKeyboard : InputController
    {
        private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode))
        .Cast<KeyCode>()
        .Where(k => ((int)k < (int)KeyCode.Mouse0))
        .ToArray();

        //FUTURE: remappable controls
        //KeyCode[] mappings = new KeyCode[Enum.GetNames(typeof(ButtonsPad)).Length];
        KeyCode[] defaultMappings = new KeyCode[]
        {
            KeyCode.W,              // dpad up
            KeyCode.S,              // dpad down
            KeyCode.A,              // dpad left  
            KeyCode.D,              // dpad right
            KeyCode.K,              // south face button
            KeyCode.J,              // east face button
            KeyCode.I,              // west face button
            KeyCode.U,              // north face button
            KeyCode.C,              // left shoulder button
            KeyCode.N,              // right shoulder button
            KeyCode.Escape,         // start button
        };

        InputDirection hatDirectionCurrent;
        InputDirection hatDirectionLast;

        public override void InitializeController()
        {
            //FUTURE: remappable controls
        }

        public override void UpdateState()
        {
            // Update the state of the controller
        }
        
        public override string GetDeviceName()
        {
            return "Keyboard";
        }

        public override InputFeatures GetFeatures()
        {
            return InputFeatures.Readable_StringInput | InputFeatures.Style_Pad | InputFeatures.Style_Baton;
        }

        public override bool GetIsConnected()
        {
            return true;
        }

        public override bool GetIsPoorConnection()
        {
            return false;
        }

        public override int GetLastButtonDown()
        {
            return 0;
        }

        public override KeyCode GetLastKeyDown()
        {
            if (Input.anyKeyDown)
            {
                for (KeyCode i = keyCodes[1]; i <= KeyCode.Menu; i++)
                {
                    if (Input.GetKeyDown(i))
                        return i;
                }
            }
            return KeyCode.None;
        }

        public override bool GetButton(int button)
        {
            return Input.GetKey(defaultMappings[button]);
        }

        public override bool GetButtonDown(int button, out double dt)
        {
            dt = 0;
            return Input.GetKeyDown(defaultMappings[button]);
        }

        public override bool GetButtonUp(int button, out double dt)
        {
            dt = 0;
            return Input.GetKeyUp(defaultMappings[button]);
        }

        public override float GetAxis(InputAxis axis)
        {
            return 0;
        }
        
        //todo: directionals
        public override bool GetHatDirection(InputDirection direction)
        {
            switch (direction)
            {
                case InputDirection.Up:
                    return Input.GetKey(defaultMappings[0]);
                case InputDirection.Down:
                    return Input.GetKey(defaultMappings[1]);
                case InputDirection.Left:
                    return Input.GetKey(defaultMappings[2]);
                case InputDirection.Right:
                    return Input.GetKey(defaultMappings[3]);
                default:
                    return false;
            }
        }

        public override bool GetHatDirectionDown(InputDirection direction, out double dt)
        {
            dt = 0;
            switch (direction)
            {
                case InputDirection.Up:
                    return Input.GetKeyDown(defaultMappings[0]);
                case InputDirection.Down:
                    return Input.GetKeyDown(defaultMappings[1]);
                case InputDirection.Left:
                    return Input.GetKeyDown(defaultMappings[2]);
                case InputDirection.Right:
                    return Input.GetKeyDown(defaultMappings[3]);
                default:
                    return false;
            }
        }

        public override bool GetHatDirectionUp(InputDirection direction, out double dt)
        {
            dt = 0;
            switch (direction)
            {
                case InputDirection.Up:
                    return Input.GetKeyUp(defaultMappings[0]);
                case InputDirection.Down:
                    return Input.GetKeyUp(defaultMappings[1]);
                case InputDirection.Left:
                    return Input.GetKeyUp(defaultMappings[2]);
                case InputDirection.Right:
                    return Input.GetKeyUp(defaultMappings[3]);
                default:
                    return false;
            }
        }

        public override void SetPlayer(int? playerNum)
        {
            if (playerNum == -1 || playerNum == null)
            {
                this.playerNum = null;
                return;
            }
            this.playerNum = (int) playerNum;
        }

        public override int? GetPlayer()
        {
            return playerNum;
        }
    }
}