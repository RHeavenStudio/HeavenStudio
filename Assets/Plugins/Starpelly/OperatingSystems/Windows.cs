using System;
using System.Runtime.InteropServices;

using Starpelly.Enums.Windows;
using System.Text;

namespace Starpelly.OS
{
    public class Windows
    {
        /// <summary>
        /// Gets the current title of the game window.
        /// </summary>
        /// <returns>The current title the window is, will probably only be used for changing the window in ChangeWindowTitle()</returns>
        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = User32.GetForegroundWindow();

            if (User32.GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        /// <summary>
        /// Changes the game's window title.
        /// </summary>
        /// <param name="newTitle">The title the window will be changed to.</param>
        public static void ChangeWindowTitle(string newTitle)
        {
            var windowPtr = User32.FindWindow(null, GetActiveWindowTitle());
            User32.SetWindowText(windowPtr, newTitle);
        }

        #region Input

        /// <summary>
        /// Simulates a real key press passed in.
        /// </summary>
        public static void KeyPress(KeyCodeWin keyCode)
        {
            INPUT input = new INPUT
            {
                type = SendInputEventType.InputKeyboard,
                mkhi = new MOUSEANDKEYBOARDINPUT
                {
                    ki = new KEYBOARDINPUT
                    {
                        wVk = (ushort)keyCode,
                        wScan = 0,
                        dwFlags = 0, // if nothing, key down
                        time = 0,
                        dwExtraInfo = IntPtr.Zero,
                    }
                }
            };

            INPUT input2 = new INPUT
            {
                type = SendInputEventType.InputKeyboard,
                mkhi = new MOUSEANDKEYBOARDINPUT
                {
                    ki = new KEYBOARDINPUT
                    {
                        wVk = (ushort)keyCode,
                        wScan = 0,
                        dwFlags = 2, // key up
                        time = 0,
                        dwExtraInfo = IntPtr.Zero,
                    }
                }
            };

            INPUT[] inputs = new INPUT[] { input, input2 }; // Combined, it's a keystroke
            User32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }


        /// <summary>
        /// Sets your mouse pointer.
        /// </summary>
        public static void ChangeCursor(WindowsCursor cursor)
        {
            User32.SetCursor(User32.LoadCursor(IntPtr.Zero, (int)cursor));
        }

        /// <summary>
        /// Immediately clicks the left mouse button.
        /// </summary>
        public static void ClickLeftMouseButton()
        {
            INPUT mouseDownInput = new INPUT();
            mouseDownInput.type = SendInputEventType.InputMouse;
            mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENT_LEFTDOWN;
            User32.SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));

            INPUT mouseUpInput = new INPUT();
            mouseUpInput.type = SendInputEventType.InputMouse;
            mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENT_LEFTUP;
            User32.SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
        }

        /// <summary>
        /// Immediately clicks the right mouse button.
        /// </summary>
        public static void ClickRightMouseButton()
        {
            INPUT mouseDownInput = new INPUT();
            mouseDownInput.type = SendInputEventType.InputMouse;
            mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENT_RIGHTDOWN;
            User32.SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));

            INPUT mouseUpInput = new INPUT();
            mouseUpInput.type = SendInputEventType.InputMouse;
            mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENT_RIGHTUP;
            User32.SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
        }

        /// <summary>
        /// Moves your cursor in the x and y params implemented, plus the current mouse pos.
        /// </summary>
        /// <param name="dx">Direction X</param>
        /// <param name="dy">Direction Y</param>
        public static void MouseMove(int dx, int dy)
        {
            INPUT mouseMove = new INPUT();
            mouseMove.type = SendInputEventType.InputMouse;
            mouseMove.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENT_MOVE;
            mouseMove.mkhi.mi.dx = dx;
            mouseMove.mkhi.mi.dy = dy;
            User32.SendInput(1, ref mouseMove, Marshal.SizeOf(new INPUT()));
        }
        #endregion

    }
}