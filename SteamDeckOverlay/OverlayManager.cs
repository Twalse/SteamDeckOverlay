using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDeckOverlay
{
    public class OverlayManager
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int GWL_EXSTYLE = -20;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private IntPtr _hwnd;
        private CancellationTokenSource? _topmostCts;

        public void Initialize(Window window)
        {
            var hwndHelper = new WindowInteropHelper(window);
            _hwnd = hwndHelper.Handle;

            MakeWindowOverlay();
            StartTopmostEnforcer();
        }

        private void MakeWindowOverlay()
        {
            int extendedStyle = GetWindowLong(_hwnd, GWL_EXSTYLE);
            
            // Base styles
            // Note: We intentionally DO NOT set WS_EX_TRANSPARENT here.
            // WPF handles click-through automatically for completely transparent pixels
            // when AllowsTransparency="True" and WindowStyle="None" are set.
            // Setting WS_EX_TRANSPARENT at the OS level would make the entire window unclickable.
            extendedStyle |= WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_NOACTIVATE;

            SetWindowLong(_hwnd, GWL_EXSTYLE, extendedStyle);
        }

        private void StartTopmostEnforcer()
        {
            _topmostCts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_topmostCts.Token.IsCancellationRequested)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0, 
                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                    });
                    
                    // Push to top every 1 second to combat aggressive full-screen games
                    await Task.Delay(1000, _topmostCts.Token);
                }
            }, _topmostCts.Token);
        }

        public void Cleanup()
        {
            _topmostCts?.Cancel();
            _topmostCts?.Dispose();
        }
    }
}
