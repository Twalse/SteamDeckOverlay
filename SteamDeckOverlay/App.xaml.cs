using System;
using System.Threading;
using System.Windows;

namespace SteamDeckOverlay
{
    public partial class App : System.Windows.Application
    {
        private Mutex? _mutex;
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private MainWindow? _mainWindow;
        private GlobalHotkeyManager? _hotkeyManager;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            const string appName = "SteamDeckOverlayAppUniqueMutex";
            _mutex = new Mutex(true, appName, out bool createdNew);

            if (!createdNew)
            {
                System.Windows.MessageBox.Show("An instance of the application is already running.", "SteamDeckOverlay", MessageBoxButton.OK, MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }

            InitializeTrayIcon();

            _mainWindow = new MainWindow();

            _hotkeyManager = new GlobalHotkeyManager();
            _hotkeyManager.Initialize(_mainWindow, () =>
            {
                _mainWindow.ToggleOverlayVisibility();
            });

            if (_mainWindow.ConfigManager.CurrentConfig.StartMinimized)
            {
                // App runs in background (Tray only) initially if configured
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.Show();
            }
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "SteamDeck Overlay"
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            
            var toggleItem = new System.Windows.Forms.ToolStripMenuItem("Toggle Overlay Visibility");
            toggleItem.Click += (s, ev) => _mainWindow?.ToggleOverlayVisibility();
            
            var settingsItem = new System.Windows.Forms.ToolStripMenuItem("Settings");
            settingsItem.Click += (s, ev) => 
            {
                if (_mainWindow != null)
                {
                    var settings = new SettingsWindow(_mainWindow.ConfigManager);
                    settings.Show();
                }
            };
            
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
            exitItem.Click += (s, ev) => Current.Shutdown();

            contextMenu.Items.Add(toggleItem);
            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, ev) => _mainWindow?.ToggleOverlayVisibility();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            _hotkeyManager?.Cleanup();

            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }
}
