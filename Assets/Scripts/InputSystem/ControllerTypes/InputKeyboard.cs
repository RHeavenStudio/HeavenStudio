using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        static ControlBindings defaultBindings
        {
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
                    PointerSensitivity = 3,
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

        public override bool GetAction(ControlStyles style, int button)
        {
            if (button < 0) return false;
            return Input.GetKey((KeyCode)currentBindings.Pad[button]);
        }

        public override bool GetActionDown(ControlStyles style, int button, out double dt)
        {
            dt = 0;
            if (button < 0) return false;
            return Input.GetKeyDown((KeyCode)currentBindings.Pad[button]);
        }

        public override bool GetActionUp(ControlStyles style, int button, out double dt)
        {
            dt = 0;
            if (button < 0) return false;
            return Input.GetKeyUp((KeyCode)currentBindings.Pad[button]);
        }

        public override float GetAxis(InputAxis axis)
        {
            return 0;
        }

        public override Vector3 GetVector(InputVector vec)
        {
            return Vector3.zero;
        }

        public override Vector2 GetPointer()
        {
            Camera cam = GameManager.instance.CursorCam;
            Vector3 rawPointerPos = Input.mousePosition;
            rawPointerPos.z = Mathf.Abs(cam.gameObject.transform.position.z);
            return cam.ScreenToWorldPoint(rawPointerPos);
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
            this.playerNum = (int)playerNum;
        }

        public override int? GetPlayer()
        {
            return playerNum;
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
            bool b = ColorUtility.TryParseHtmlString("#F4F4F4", out Color colour);
            m.SetColor("_BodyColor", b ? colour : Color.white);
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