using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;

using static JSL;

namespace HeavenStudio.InputSystem.Loaders
{
    public static class InputJoyconPairInitializer
    {
        [LoadOrder(3)]
        public static InputController[] Initialize()
        {
            PlayerInput.PlayerInputRefresh.Add(Refresh);
            return Refresh();
        }

        public static InputController[] Refresh()
        {
            int joyconLCount = 0, joyconRCount = 0;
            foreach (InputController con in PlayerInput.GetInputControllers())
            {
                if (con is InputJoyshock)
                {
                    InputJoyshock joyshock = (InputJoyshock)con;
                    if (joyshock.GetJoyshockType() == TypeJoyConLeft)
                    {
                        joyconLCount++;
                    }
                    else if (joyshock.GetJoyshockType() == TypeJoyConRight)
                    {
                        joyconRCount++;
                    }
                }
            }
            if (joyconLCount > 0 && joyconRCount > 0)
            {
                InputJoyconPair joyconPair = new InputJoyconPair();
                joyconPair.SetPlayer(null);
                joyconPair.InitializeController();
                return new InputController[] { joyconPair };
            }
            else
            {
                Debug.Log("No Joy-Con connected.");
                return null;
            }
        }
    }
}

namespace HeavenStudio.InputSystem
{
    public class InputJoyconPair : InputController
    {
        public override bool GetAction(ControlStyles style, int action)
        {
            throw new NotImplementedException();
        }

        public override bool GetActionDown(ControlStyles style, int action, out double dt)
        {
            throw new NotImplementedException();
        }

        public override bool GetActionUp(ControlStyles style, int action, out double dt)
        {
            throw new NotImplementedException();
        }

        public override float GetAxis(InputAxis axis)
        {
            throw new NotImplementedException();
        }

        public override int GetBindingsVersion()
        {
            throw new NotImplementedException();
        }

        public override string[] GetButtonNames()
        {
            throw new NotImplementedException();
        }

        public override ControlBindings GetCurrentBindings()
        {
            throw new NotImplementedException();
        }

        public override bool GetCurrentStyleSupported()
        {
            throw new NotImplementedException();
        }

        public override ControlBindings GetDefaultBindings()
        {
            throw new NotImplementedException();
        }

        public override ControlStyles GetDefaultStyle()
        {
            throw new NotImplementedException();
        }

        public override string GetDeviceName()
        {
            throw new NotImplementedException();
        }

        public override InputFeatures GetFeatures()
        {
            throw new NotImplementedException();
        }

        public override bool GetFlick(out double dt)
        {
            throw new NotImplementedException();
        }

        public override bool GetHatDirection(InputDirection direction)
        {
            throw new NotImplementedException();
        }

        public override bool GetHatDirectionDown(InputDirection direction, out double dt)
        {
            throw new NotImplementedException();
        }

        public override bool GetHatDirectionUp(InputDirection direction, out double dt)
        {
            throw new NotImplementedException();
        }

        public override bool GetIsActionUnbindable(int action, ControlStyles style)
        {
            throw new NotImplementedException();
        }

        public override bool GetIsConnected()
        {
            throw new NotImplementedException();
        }

        public override bool GetIsPoorConnection()
        {
            throw new NotImplementedException();
        }

        public override int GetLastActionDown()
        {
            throw new NotImplementedException();
        }

        public override int GetLastButtonDown(bool strict = false)
        {
            throw new NotImplementedException();
        }

        public override int? GetPlayer()
        {
            throw new NotImplementedException();
        }

        public override Vector2 GetPointer()
        {
            throw new NotImplementedException();
        }

        public override bool GetPointerLeftRight()
        {
            throw new NotImplementedException();
        }

        public override bool GetSlide(out double dt)
        {
            throw new NotImplementedException();
        }

        public override bool GetSqueeze()
        {
            throw new NotImplementedException();
        }

        public override bool GetSqueezeDown(out double dt)
        {
            throw new NotImplementedException();
        }

        public override bool GetSqueezeUp(out double dt)
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetVector(InputVector vector)
        {
            throw new NotImplementedException();
        }

        public override void InitializeController()
        {
            throw new NotImplementedException();
        }

        public override void OnSelected()
        {
            throw new NotImplementedException();
        }

        public override void RecentrePointer()
        {
            throw new NotImplementedException();
        }

        public override void ResetBindings()
        {
            throw new NotImplementedException();
        }

        public override void SetCurrentBindings(ControlBindings newBinds)
        {
            throw new NotImplementedException();
        }

        public override void SetMaterialProperties(Material m)
        {
            throw new NotImplementedException();
        }

        public override void SetPlayer(int? playerNum)
        {
            throw new NotImplementedException();
        }

        public override void TogglePointerLock(bool locked)
        {
            throw new NotImplementedException();
        }

        public override ControlBindings UpdateBindings(ControlBindings lastBinds)
        {
            throw new NotImplementedException();
        }

        public override void UpdateState()
        {
            throw new NotImplementedException();
        }
    }
}