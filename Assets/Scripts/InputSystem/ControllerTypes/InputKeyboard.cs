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

        static ControlBindings defaultBindings {
            get
            {
                return new ControlBindings()
                {
                    Pad = new int[]
                    {
                        (int)KeyCode.W,
                        (int)KeyCode.S,
                        (int)KeyCode.A,
                        (int)KeyCode.D,
                        (int)KeyCode.J,
                        (int)KeyCode.K,
                        (int)KeyCode.I,
                        (int)KeyCode.U,
                        (int)KeyCode.E,
                        (int)KeyCode.U,
                        (int)KeyCode.Escape,
                    },
                };
            }
        }

        InputDirection hatDirectionCurrent;
        InputDirection hatDirectionLast;

        public override void InitializeController()
        {
            LoadBindings();
        }

        public override void UpdateState()
        {
            // Update the state of the controller
        }

        public override void OnSelected()
        { 

        }
        
        public override string GetDeviceName()
        {
            return "Keyboard";
        }

        public override string[] GetButtonNames()
        {
            string[] names = new string[(int)KeyCode.Mouse0];
            for (int i = 0; i < keyCodes.Length; i++)
            {
                names[(int)keyCodes[i]] = keyCodes[i].ToString();
            }
            return names;
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

        public override ControlBindings GetDefaultBindings()
        {
            return defaultBindings;
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

        public override bool GetIsActionUnbindable(int action, ControlStyles style)
        {
            return false;
        }

        public override int GetLastButtonDown()
        {
            if (Input.anyKeyDown)
            {
                for (KeyCode i = keyCodes[1]; i <= KeyCode.Menu; i++)
                {
                    if (Input.GetKeyDown(i))
                        return (int)i;
                }
            }
            return (int)KeyCode.None;
        }

        public override int GetLastActionDown()
        {
            for (int i = 0; i < BINDS_MAX; i++)
            {
                if (Input.GetKeyDown((KeyCode)currentBindings.Pad[i]))
                    return i;
            }
            return -1;
        }

        public override bool GetAction(int button)
        {
            return Input.GetKey((KeyCode)currentBindings.Pad[button]);
        }

        public override bool GetActionDown(int button, out double dt)
        {
            dt = 0;
            return Input.GetKeyDown((KeyCode)currentBindings.Pad[button]);
        }

        public override bool GetActionUp(int button, out double dt)
        {
            dt = 0;
            return Input.GetKeyUp((KeyCode)currentBindings.Pad[button]);
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
                    return Input.GetKey((KeyCode)currentBindings.Pad[0]);
                case InputDirection.Down:
                    return Input.GetKey((KeyCode)currentBindings.Pad[1]);
                case InputDirection.Left:
                    return Input.GetKey((KeyCode)currentBindings.Pad[2]);
                case InputDirection.Right:
                    return Input.GetKey((KeyCode)currentBindings.Pad[3]);
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
                    return Input.GetKeyDown((KeyCode)currentBindings.Pad[0]);
                case InputDirection.Down:
                    return Input.GetKeyDown((KeyCode)currentBindings.Pad[1]);
                case InputDirection.Left:
                    return Input.GetKeyDown((KeyCode)currentBindings.Pad[2]);
                case InputDirection.Right:
                    return Input.GetKeyDown((KeyCode)currentBindings.Pad[3]);
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
                    return Input.GetKeyUp((KeyCode)currentBindings.Pad[0]);
                case InputDirection.Down:
                    return Input.GetKeyUp((KeyCode)currentBindings.Pad[1]);
                case InputDirection.Left:
                    return Input.GetKeyUp((KeyCode)currentBindings.Pad[2]);
                case InputDirection.Right:
                    return Input.GetKeyUp((KeyCode)currentBindings.Pad[3]);
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