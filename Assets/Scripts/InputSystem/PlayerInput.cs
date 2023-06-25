using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.InputSystem;

using static JSL;

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

        public delegate InputController[] InputControllerInitializer();

        public delegate void InputControllerDispose();
        public static event InputControllerDispose PlayerInputCleanUp;

        static List<InputControllerInitializer> loadRunners;
        static void BuildLoadRunnerList() {
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
        
        // BUTTONS
        //TODO: refactor for controller and custom binds, currently uses temporary button checks
        
        public static bool Pressed(bool includeDPad = false)
        {
            bool keyDown = GetInputController(1).GetButtonDown((int) InputController.ButtonsPad.PadE) || (includeDPad && GetAnyDirectionDown());
            return keyDown && !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput ;
        }
        
        public static bool PressedUp(bool includeDPad = false)
        {
            bool keyUp = GetInputController(1).GetButtonUp((int) InputController.ButtonsPad.PadE) || (includeDPad && GetAnyDirectionUp());
            return keyUp && !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }
        
        public static bool Pressing(bool includeDPad = false)
        {
            bool pressing = GetInputController(1).GetButton((int) InputController.ButtonsPad.PadE) || (includeDPad && GetAnyDirection());
            return pressing && !GameManager.instance.autoplay && Conductor.instance.isPlaying && GameManager.instance.canInput;
        }
        
        
        public static bool AltPressed()
        {
            bool down = GetInputController(1).GetButtonDown((int) InputController.ButtonsPad.PadS);
            return down && PlayerHasControl();
        }
        
        public static bool AltPressedUp()
        {
            bool up = GetInputController(1).GetButtonUp((int) InputController.ButtonsPad.PadS);
            return up && PlayerHasControl();
        }
        
        public static bool AltPressing()
        {
            bool pressing = GetInputController(1).GetButton((int) InputController.ButtonsPad.PadS);
            return pressing && PlayerHasControl();
        }
        
        //Directions
        
        public static bool GetAnyDirectionDown()
        {
            InputController c = GetInputController(1);
            return (c.GetHatDirectionDown((InputController.InputDirection) UP)
            || c.GetHatDirectionDown((InputController.InputDirection) DOWN)
            || c.GetHatDirectionDown((InputController.InputDirection) LEFT)
            || c.GetHatDirectionDown((InputController.InputDirection) RIGHT)
            ) && PlayerHasControl();
            
        }
        
        public static bool GetAnyDirectionUp()
        {
            InputController c = GetInputController(1);
            return (c.GetHatDirectionUp((InputController.InputDirection) UP)
            || c.GetHatDirectionUp((InputController.InputDirection) DOWN)
            || c.GetHatDirectionUp((InputController.InputDirection) LEFT)
            || c.GetHatDirectionUp((InputController.InputDirection) RIGHT)
            ) && PlayerHasControl();
            
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
            return GetInputController(1).GetHatDirectionDown((InputController.InputDirection) direction) && PlayerHasControl();
        }
        
        public static bool GetSpecificDirectionUp(int direction)
        {
            return GetInputController(1).GetHatDirectionUp((InputController.InputDirection) direction) && PlayerHasControl();
        }
    }
}
