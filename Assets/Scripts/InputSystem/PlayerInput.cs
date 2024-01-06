using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.InputSystem;

using static JSL;
using HeavenStudio.Games;
using System.Diagnostics.Contracts;

namespace HeavenStudio.InputSystem
{
    public class LoadOrder : Attribute
    {
        public int Order { get; set; }
        public LoadOrder(int order)
        {
            Order = order;
        }
    }
}

namespace HeavenStudio
{
    public class PlayerInput
    {
        public class InputAction
        {
            public delegate bool ActionQuery(out double dt);

            public string name;
            public int[] inputLockCategory;
            public ActionQuery padAction, touchAction, batonAction;

            public InputAction(string name, int[] inputLockCategory, ActionQuery pad, ActionQuery touch, ActionQuery baton)
            {
                this.name = name;
                this.inputLockCategory = inputLockCategory;
                padAction = pad;
                touchAction = touch;
                batonAction = baton;
            }
        }

        public static InputController.ControlStyles CurrentControlStyle = InputController.ControlStyles.Pad;

        static List<InputController> inputDevices;

        public delegate InputController[] InputControllerInitializer();

        public delegate void InputControllerDispose();
        public static event InputControllerDispose PlayerInputCleanUp;

        public delegate InputController[] InputControllerRefresh();
        public static List<InputControllerRefresh> PlayerInputRefresh;

        static List<InputControllerInitializer> loadRunners;
        static void BuildLoadRunnerList()
        {
            PlayerInputRefresh = new();
            loadRunners = System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.Namespace == "HeavenStudio.InputSystem.Loaders" && x.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static) != null)
            .Select(t => (InputControllerInitializer)Delegate.CreateDelegate(
                typeof(InputControllerInitializer),
                null,
                t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static),
                false
                ))
            .ToList();

            loadRunners.Sort((x, y) => x.Method.GetCustomAttribute<LoadOrder>().Order.CompareTo(y.Method.GetCustomAttribute<LoadOrder>().Order));
        }

        public static int InitInputControllers()
        {
            inputDevices = new List<InputController>();

            BuildLoadRunnerList();
            foreach (InputControllerInitializer runner in loadRunners)
            {
                InputController[] controllers = runner();
                if (controllers != null)
                {
                    inputDevices.AddRange(controllers);
                }
            }

            return inputDevices.Count;
        }

        public static int RefreshInputControllers()
        {
            inputDevices = new List<InputController>();
            if (PlayerInputRefresh != null)
            {
                foreach (InputControllerRefresh runner in PlayerInputRefresh)
                {
                    InputController[] controllers = runner();
                    if (controllers != null)
                    {
                        inputDevices.AddRange(controllers);
                    }
                }
            }
            return inputDevices.Count;
        }

        public static int GetNumControllersConnected()
        {
            return inputDevices.Count;
        }

        public static List<InputController> GetInputControllers()
        {
            return inputDevices;
        }

        public static InputController GetInputController(int player)
        {
            //select input controller that has player field set to player
            //this will return the first controller that has that player number in the case of controller pairs (eg. Joy-Cons)
            //so such controllers should have a reference to the other controller in the pair
            foreach (InputController i in inputDevices)
            {
                if (i.GetPlayer() == player)
                {
                    return i;
                }
            }
            return null;
        }

        public static int GetInputControllerId(int player)
        {
            //select input controller id that has player field set to player
            //this will return the first controller that has that player number in the case of controller pairs (eg. Joy-Cons)
            //so such controllers should have a reference to the other controller in the pair
            //controller IDs are determined by connection order (the Keyboard is always first)
            for (int i = 0; i < inputDevices.Count; i++)
            {
                if (inputDevices[i].GetPlayer() == player)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void UpdateInputControllers()
        {
            foreach (InputController i in inputDevices)
            {
                i.UpdateState();
            }
        }

        public static void CleanUp()
        {
            PlayerInputCleanUp?.Invoke();
        }

        // The autoplay isn't activated AND
        // The song is actually playing AND
        // The GameManager allows you to Input
        public static bool PlayerHasControl()
        {
            if (GameManager.instance == null || Conductor.instance == null) return true;
            return !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }

        /*--------------------*/
        /* MAIN INPUT METHODS */
        /*--------------------*/

        public static bool GetIsAction(InputAction action, out double dt)
        {
            dt = 0;
            switch (CurrentControlStyle)
            {
                case InputController.ControlStyles.Pad:
                    return action.padAction(out dt);
                case InputController.ControlStyles.Touch:
                    return action.touchAction(out dt);
                case InputController.ControlStyles.Baton:
                    return action.batonAction(out dt);
            }
            return false;
        }

        public static bool GetIsAction(InputAction action)
        {
            switch (CurrentControlStyle)
            {
                case InputController.ControlStyles.Pad:
                    return action.padAction(out _);
                case InputController.ControlStyles.Touch:
                    return action.touchAction(out _);
                case InputController.ControlStyles.Baton:
                    return action.batonAction(out _);
            }
            return false;
        }

        public static bool GetPadDown(InputController.ActionsPad ac, out double dt)
        {
            bool a = GetInputController(1).GetActionDown(InputController.ControlStyles.Pad, (int)ac, out dt);
            return a && PlayerHasControl();
        }

        public static bool GetPadDown(InputController.ActionsPad ac)
        {
            bool a = GetInputController(1).GetActionDown(InputController.ControlStyles.Pad, (int)ac, out _);
            return a && PlayerHasControl();
        }

        public static bool GetPadUp(InputController.ActionsPad ac, out double dt)
        {
            bool a = GetInputController(1).GetActionUp(InputController.ControlStyles.Pad, (int)ac, out dt);
            return a && PlayerHasControl();
        }

        public static bool GetPadUp(InputController.ActionsPad ac)
        {
            bool a = GetInputController(1).GetActionUp(InputController.ControlStyles.Pad, (int)ac, out _);
            return a && PlayerHasControl();
        }

        public static bool GetPad(InputController.ActionsPad ac)
        {
            bool a = GetInputController(1).GetAction(InputController.ControlStyles.Pad, (int)ac);
            return a && PlayerHasControl();
        }

        public static bool GetBatonDown(InputController.ActionsBaton ac, out double dt)
        {
            bool a = GetInputController(1).GetActionDown(InputController.ControlStyles.Baton, (int)ac, out dt);
            return a && PlayerHasControl();
        }

        public static bool GetBatonDown(InputController.ActionsBaton ac)
        {
            bool a = GetInputController(1).GetActionDown(InputController.ControlStyles.Baton, (int)ac, out _);
            return a && PlayerHasControl();
        }

        public static bool GetBatonUp(InputController.ActionsBaton ac, out double dt)
        {
            bool a = GetInputController(1).GetActionUp(InputController.ControlStyles.Baton, (int)ac, out dt);
            return a && PlayerHasControl();
        }

        public static bool GetBatonUp(InputController.ActionsBaton ac)
        {
            bool a = GetInputController(1).GetActionUp(InputController.ControlStyles.Baton, (int)ac, out _);
            return a && PlayerHasControl();
        }

        public static bool GetBaton(InputController.ActionsBaton ac)
        {
            bool a = GetInputController(1).GetAction(InputController.ControlStyles.Baton, (int)ac);
            return a && PlayerHasControl();
        }

        public static bool GetSqueeze()
        {
            bool a = GetInputController(1).GetSqueeze();
            return a && PlayerHasControl();
        }

        public static bool GetSqueezeDown()
        {
            bool a = GetInputController(1).GetSqueezeDown(out _);
            return a && PlayerHasControl();
        }

        public static bool GetSqueezeDown(out double dt)
        {
            bool a = GetInputController(1).GetSqueezeDown(out dt);
            return a && PlayerHasControl();
        }

        public static bool GetSqueezeUp()
        {
            bool a = GetInputController(1).GetSqueezeUp(out _);
            return a && PlayerHasControl();
        }

        public static bool GetSqueezeUp(out double dt)
        {
            bool a = GetInputController(1).GetSqueezeUp(out dt);
            return a && PlayerHasControl();
        }

        public static bool GetTouchDown(InputController.ActionsTouch ac, out double dt)
        {
            bool a = GetInputController(1).GetActionDown(InputController.ControlStyles.Touch, (int)ac, out dt);
            return a && PlayerHasControl();
        }

        public static bool GetTouchDown(InputController.ActionsTouch ac)
        {
            bool a = GetInputController(1).GetActionDown(InputController.ControlStyles.Touch, (int)ac, out _);
            return a && PlayerHasControl();
        }

        public static bool GetTouchUp(InputController.ActionsTouch ac, out double dt)
        {
            bool a = GetInputController(1).GetActionUp(InputController.ControlStyles.Touch, (int)ac, out dt);
            return a && PlayerHasControl();
        }

        public static bool GetTouchUp(InputController.ActionsTouch ac)
        {
            bool a = GetInputController(1).GetActionUp(InputController.ControlStyles.Touch, (int)ac, out _);
            return a && PlayerHasControl();
        }

        public static bool GetTouch(InputController.ActionsTouch ac)
        {
            bool a = GetInputController(1).GetAction(InputController.ControlStyles.Touch, (int)ac);
            return a && PlayerHasControl();
        }

        public static bool GetSlide()
        {
            bool a = GetInputController(1).GetSlide(out _);
            return a && PlayerHasControl();
        }

        public static bool GetSlide(out double dt)
        {
            bool a = GetInputController(1).GetSlide(out dt);
            return a && PlayerHasControl();
        }

        public static bool GetFlick()
        {
            bool a = GetInputController(1).GetFlick(out _);
            return a && PlayerHasControl();
        }

        public static bool GetFlick(out double dt)
        {
            bool a = GetInputController(1).GetFlick(out dt);
            return a && PlayerHasControl();
        }
    }
}
