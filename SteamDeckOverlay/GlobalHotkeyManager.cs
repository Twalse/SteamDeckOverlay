using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace SteamDeckOverlay
{
    public class GlobalHotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_H = 0x48;
        private const int HOTKEY_ID = 9000;

        private IntPtr _hwnd;
        private HwndSource? _source;
        private Action? _onHotKeyPressed;

        public void Initialize(System.Windows.Window window, Action onHotKeyPressed)
        {
            var helper = new WindowInteropHelper(window);
            helper.EnsureHandle(); // Ensure window handle is created even if hidden
            _hwnd = helper.Handle;
            
            _source = HwndSource.FromHwnd(_hwnd);
            _source?.AddHook(HwndHook);
            _onHotKeyPressed = onHotKeyPressed;

            RegisterHotKey(_hwnd, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_H);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                _onHotKeyPressed?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Cleanup()
        {
            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                _source = null;
            }
            UnregisterHotKey(_hwnd, HOTKEY_ID);
        }
    }
}
