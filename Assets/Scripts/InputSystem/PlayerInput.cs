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

namespace HeavenStudio.InputSystem
{
    public class LoadOrder : Attribute {
        public int Order { get; set; }
        public LoadOrder(int order) {
            Order = order;
        }
    }
}

namespace HeavenStudio
{
    public class PlayerInput
    {
        //Clockwise
        public const int UP = 0;
        public const int RIGHT = 1;
        public const int DOWN = 2;
        public const int LEFT = 3;
        
        static List<InputController> inputDevices;
        static InputController.ControlStyles currentControlStyle = InputController.ControlStyles.Pad;

        public delegate InputController[] InputControllerInitializer();

        public delegate void InputControllerDispose();
        public static event InputControllerDispose PlayerInputCleanUp;

        public delegate InputController[] InputControllerRefresh();
        public static List<InputControllerRefresh> PlayerInputRefresh;

        static List<InputControllerInitializer> loadRunners;
        static void BuildLoadRunnerList() {
            PlayerInputRefresh = new();
            loadRunners = System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.Namespace == "HeavenStudio.InputSystem.Loaders" && x.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static) != null)
            .Select(t => (InputControllerInitializer) Delegate.CreateDelegate(
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
            foreach (InputControllerInitializer runner in loadRunners) {
                InputController[] controllers = runner();
                if (controllers != null) {
                    inputDevices.AddRange(controllers);
                }
            }
            
            return inputDevices.Count;
        }

        public static int RefreshInputControllers()
        {
            inputDevices = new List<InputController>();
            if (PlayerInputRefresh != null) {
                foreach (InputControllerRefresh runner in PlayerInputRefresh) {
                    InputController[] controllers = runner();
                    if (controllers != null) {
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
            // Needed so Keyboard works on MacOS and Linux
            #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            inputDevices = new List<InputController>();
            if(inputDevices.Count < 1)
            {
                InputKeyboard keyboard = new InputKeyboard();
                keyboard.SetPlayer(1);
                keyboard.InitializeController();
                inputDevices.Add(keyboard);
            }
            #endif
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
            
            
            // Needed so Keyboard works on MacOS and Linux
            #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            inputDevices = new List<InputController>();
            if(inputDevices.Count < 1)
            {
                InputKeyboard keyboard = new InputKeyboard();
                keyboard.SetPlayer(1);
                keyboard.InitializeController();
                inputDevices.Add(keyboard);
            }
            #endif
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
            // Needed so Keyboard works on MacOS and Linux
            #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            inputDevices = new List<InputController>();
            if(inputDevices.Count < 1)
            {
                InputKeyboard keyboard = new InputKeyboard();
                keyboard.SetPlayer(1);
                keyboard.InitializeController();
                inputDevices.Add(keyboard);
            }
            #endif
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
            return !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }
        
        /*--------------------*/
        /* MAIN INPUT METHODS */
        /*--------------------*/

        public static bool GetIsAction(string action)
        {
            return false;
        }
        
        // BUTTONS
        //TODO: refactor for controller and custom binds, currently uses temporary button checks
        
        public static bool Pressed(bool includeDPad = false)
        {
            bool keyDown = GetInputController(1).GetActionDown((int) InputController.ActionsPad.East, out _) || (includeDPad && GetAnyDirectionDown());
            return keyDown && !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }

        public static bool Pressed(out double dt, bool includeDPad = false)
        {
            bool keyDown = GetInputController(1).GetActionDown((int) InputController.ActionsPad.East, out dt) || (includeDPad && GetAnyDirectionDown());
            return keyDown && !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }
        
        public static bool PressedUp(bool includeDPad = false)
        {
            bool keyUp = GetInputController(1).GetActionUp((int) InputController.ActionsPad.East, out _) || (includeDPad && GetAnyDirectionUp());
            return keyUp && !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }
        
        public static bool PressedUp(out double dt, bool includeDPad = false)
        {
            bool keyUp = GetInputController(1).GetActionUp((int) InputController.ActionsPad.East, out dt) || (includeDPad && GetAnyDirectionUp());
            return keyUp && !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }
        
        public static bool Pressing(bool includeDPad = false)
        {
            bool pressing = GetInputController(1).GetAction((int) InputController.ActionsPad.East) || (includeDPad && GetAnyDirection());
            return pressing && !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }
        
        
        public static bool AltPressed()
        {
            bool down = GetInputController(1).GetActionDown((int) InputController.ActionsPad.South, out _);
            return down && PlayerHasControl();
        }

        public static bool AltPressed(out double dt)
        {
            bool down = GetInputController(1).GetActionDown((int) InputController.ActionsPad.South, out dt);
            return down && PlayerHasControl();
        }
        
        public static bool AltPressedUp()
        {
            bool up = GetInputController(1).GetActionUp((int) InputController.ActionsPad.South, out _);
            return up && PlayerHasControl();
        }
        
        public static bool AltPressedUp(out double dt)
        {
            bool up = GetInputController(1).GetActionUp((int) InputController.ActionsPad.South, out dt);
            return up && PlayerHasControl();
        }
        
        public static bool AltPressing()
        {
            bool pressing = GetInputController(1).GetAction((int) InputController.ActionsPad.South);
            return pressing && PlayerHasControl();
        }
        
        //Directions
        
        public static bool GetAnyDirectionDown()
        {
            InputController c = GetInputController(1);
            return (c.GetHatDirectionDown((InputController.InputDirection) UP, out _)
            || c.GetHatDirectionDown((InputController.InputDirection) DOWN, out _)
            || c.GetHatDirectionDown((InputController.InputDirection) LEFT, out _)
            || c.GetHatDirectionDown((InputController.InputDirection) RIGHT, out _)
            ) && PlayerHasControl();
        }

        public static bool GetAnyDirectionDown(out double dt)
        {
            InputController c = GetInputController(1);
            bool r1 = c.GetHatDirectionDown((InputController.InputDirection)UP, out double d1);
            bool r2 = c.GetHatDirectionDown((InputController.InputDirection)DOWN, out double d2);
            bool r3 = c.GetHatDirectionDown((InputController.InputDirection)LEFT, out double d3);
            bool r4 = c.GetHatDirectionDown((InputController.InputDirection)RIGHT, out double d4);
            bool r = (r1 || r2 || r3 || r4) && PlayerHasControl();
            dt = Math.Max(Math.Max(Math.Max(d1, d2), d3), d4);
            return r;
        }
        
        public static bool GetAnyDirectionUp()
        {
            InputController c = GetInputController(1);
            return (c.GetHatDirectionUp((InputController.InputDirection) UP, out _)
            || c.GetHatDirectionUp((InputController.InputDirection) DOWN, out _)
            || c.GetHatDirectionUp((InputController.InputDirection) LEFT, out _)
            || c.GetHatDirectionUp((InputController.InputDirection) RIGHT, out _)
            ) && PlayerHasControl();            
        }

        public static bool GetAnyDirectionUp(out double dt)
        {
            InputController c = GetInputController(1);
            bool r1 = c.GetHatDirectionUp((InputController.InputDirection)UP, out double d1);
            bool r2 = c.GetHatDirectionUp((InputController.InputDirection)DOWN, out double d2);
            bool r3 = c.GetHatDirectionUp((InputController.InputDirection)LEFT, out double d3);
            bool r4 = c.GetHatDirectionUp((InputController.InputDirection)RIGHT, out double d4);
            bool r = (r1 || r2 || r3 || r4) && PlayerHasControl();
            dt = Math.Max(Math.Max(Math.Max(d1, d2), d3), d4);
            return r;
        }
        
        public static bool GetAnyDirection()
        {
            InputController c = GetInputController(1);
            return (c.GetHatDirection((InputController.InputDirection) UP)
            || c.GetHatDirection((InputController.InputDirection) DOWN)
            || c.GetHatDirection((InputController.InputDirection) LEFT)
            || c.GetHatDirection((InputController.InputDirection) RIGHT)
            ) && PlayerHasControl();
        }
        
        public static bool GetSpecificDirection(int direction)
        {
            return GetInputController(1).GetHatDirection((InputController.InputDirection) direction) && PlayerHasControl();
        }
        
        public static bool GetSpecificDirectionDown(int direction)
        {
            return GetInputController(1).GetHatDirectionDown((InputController.InputDirection) direction, out _) && PlayerHasControl();
        }
        
        public static bool GetSpecificDirectionUp(int direction)
        {
            return GetInputController(1).GetHatDirectionUp((InputController.InputDirection) direction, out _) && PlayerHasControl();
        }
        
        public static bool GetSpecificDirectionDown(int direction, out double dt)
        {
            return GetInputController(1).GetHatDirectionDown((InputController.InputDirection) direction, out dt) && PlayerHasControl();
        }
        
        public static bool GetSpecificDirectionUp(int direction, out double dt)
        {
            return GetInputController(1).GetHatDirectionUp((InputController.InputDirection) direction, out dt) && PlayerHasControl();
        }
    }
}
