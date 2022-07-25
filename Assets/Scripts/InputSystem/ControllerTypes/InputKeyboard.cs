using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static JSL;

namespace HeavenStudio.InputSystem
{
    public class InputKeyboard : InputController
    {
        static KeyCode[] keyCodes = (KeyCode[]) System.Enum.GetValues(typeof(UnityEngine.KeyCode));

        //FUTURE: remappable controls
        //KeyCode[] mappings = new KeyCode[Enum.GetNames(typeof(ButtonsPad)).Length];
        KeyCode[] mappings = new KeyCode[]
        {
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.X,
            KeyCode.Z,
            KeyCode.C,
            KeyCode.V,
            KeyCode.S,
            KeyCode.D,
            KeyCode.Return,
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

        public override int GetLastButtonDown()
        {
            return 0;
        }

        public override KeyCode GetLastKeyDown()
        {
            for(KeyCode i = keyCodes[1]; i <= KeyCode.Menu; i++) {
                if (Input.GetKeyDown(i))
                    return i;
            }
            return KeyCode.None;
        }

        public override bool GetButton(int button)
        {
            return Input.GetKey(mappings[button]);
        }

        public override bool GetButtonDown(int button)
        {
            return Input.GetKeyDown(mappings[button]);
        }

        public override bool GetButtonUp(int button)
        {
            return Input.GetKeyUp(mappings[button]);
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
                    return Input.GetKey(KeyCode.UpArrow);
                case InputDirection.Down:
                    return Input.GetKey(KeyCode.DownArrow);
                case InputDirection.Left:
                    return Input.GetKey(KeyCode.LeftArrow);
                case InputDirection.Right:
                    return Input.GetKey(KeyCode.RightArrow);
                default:
                    return false;
            }
        }

        public override bool GetHatDirectionDown(InputDirection direction)
        {
            switch (direction)
            {
                case InputDirection.Up:
                    return Input.GetKeyDown(KeyCode.UpArrow);
                case InputDirection.Down:
                    return Input.GetKeyDown(KeyCode.DownArrow);
                case InputDirection.Left:
                    return Input.GetKeyDown(KeyCode.LeftArrow);
                case InputDirection.Right:
                    return Input.GetKeyDown(KeyCode.RightArrow);
                default:
                    return false;
            }
        }

        public override bool GetHatDirectionUp(InputDirection direction)
        {
            switch (direction)
            {
                case InputDirection.Up:
                    return Input.GetKeyUp(KeyCode.UpArrow);
                case InputDirection.Down:
                    return Input.GetKeyUp(KeyCode.DownArrow);
                case InputDirection.Left:
                    return Input.GetKeyUp(KeyCode.LeftArrow);
                case InputDirection.Right:
                    return Input.GetKeyUp(KeyCode.RightArrow);
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