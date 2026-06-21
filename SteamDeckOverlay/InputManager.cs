using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SteamDeckOverlay
{
    public class InputManager
    {
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size => Marshal.SizeOf(typeof(INPUT));
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_SCANCODE = 0x0008;

        // Virtual Key Codes and Scan Codes
        private const ushort VK_MENU = 0x12;
        private const ushort SC_ALT = 0x38;

        private const ushort VK_LEFT = 0x25;
        private const ushort SC_LEFT = 0x4B;

        private const ushort VK_LWIN = 0x5B;
        private const ushort SC_LWIN = 0x5B;

        private const ushort VK_D = 0x44;
        private const ushort SC_D = 0x20;

        private const ushort VK_TAB = 0x09;
        private const ushort SC_TAB = 0x0F;

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public static void SimulateBack()
        {
            try
            {
                INPUT[] inputs = new INPUT[4];
                
                inputs[0].type = INPUT_KEYBOARD;
                inputs[0].U.ki.wVk = 0;
                inputs[0].U.ki.wScan = SC_ALT;
                inputs[0].U.ki.dwFlags = KEYEVENTF_SCANCODE;

                inputs[1].type = INPUT_KEYBOARD;
                inputs[1].U.ki.wVk = 0;
                inputs[1].U.ki.wScan = SC_LEFT;
                inputs[1].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY;

                inputs[2].type = INPUT_KEYBOARD;
                inputs[2].U.ki.wVk = 0;
                inputs[2].U.ki.wScan = SC_LEFT;
                inputs[2].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;

                inputs[3].type = INPUT_KEYBOARD;
                inputs[3].U.ki.wVk = 0;
                inputs[3].U.ki.wScan = SC_ALT;
                inputs[3].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;

                SendInput((uint)inputs.Length, inputs, INPUT.Size);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to simulate Back: {ex.Message}");
            }
        }

        public static void SimulateHome()
        {
            try
            {
                INPUT[] inputs = new INPUT[4];

                inputs[0].type = INPUT_KEYBOARD;
                inputs[0].U.ki.wVk = 0;
                inputs[0].U.ki.wScan = SC_LWIN;
                inputs[0].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY;

                inputs[1].type = INPUT_KEYBOARD;
                inputs[1].U.ki.wVk = 0;
                inputs[1].U.ki.wScan = SC_D;
                inputs[1].U.ki.dwFlags = KEYEVENTF_SCANCODE;

                inputs[2].type = INPUT_KEYBOARD;
                inputs[2].U.ki.wVk = 0;
                inputs[2].U.ki.wScan = SC_D;
                inputs[2].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;

                inputs[3].type = INPUT_KEYBOARD;
                inputs[3].U.ki.wVk = 0;
                inputs[3].U.ki.wScan = SC_LWIN;
                inputs[3].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;

                SendInput((uint)inputs.Length, inputs, INPUT.Size);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to simulate Home: {ex.Message}");
            }
        }

        public static void SimulateWindows()
        {
            try
            {
                INPUT[] inputs = new INPUT[4];

                inputs[0].type = INPUT_KEYBOARD;
                inputs[0].U.ki.wVk = 0;
                inputs[0].U.ki.wScan = SC_LWIN;
                inputs[0].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY;

                inputs[1].type = INPUT_KEYBOARD;
                inputs[1].U.ki.wVk = 0;
                inputs[1].U.ki.wScan = SC_TAB;
                inputs[1].U.ki.dwFlags = KEYEVENTF_SCANCODE;

                inputs[2].type = INPUT_KEYBOARD;
                inputs[2].U.ki.wVk = 0;
                inputs[2].U.ki.wScan = SC_TAB;
                inputs[2].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;

                inputs[3].type = INPUT_KEYBOARD;
                inputs[3].U.ki.wVk = 0;
                inputs[3].U.ki.wScan = SC_LWIN;
                inputs[3].U.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;

                SendInput((uint)inputs.Length, inputs, INPUT.Size);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to simulate Windows: {ex.Message}");
            }
        }

        public static void ToggleKeyboard()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "osk.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to toggle keyboard: {ex.Message}");
            }
        }
    }
}
