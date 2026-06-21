using System;
using System.Windows;
using System.Windows.Input;

namespace SteamDeckOverlay
{
    public partial class MainWindow : Window
    {
        private readonly OverlayManager _overlayManager;
        private readonly SystemInfoManager _systemInfoManager;
        public ConfigManager ConfigManager { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            
            ConfigManager = new ConfigManager();
            ConfigManager.ConfigChanged += ConfigManager_ConfigChanged;
            ApplyConfig();

            _overlayManager = new OverlayManager();
            _systemInfoManager = new SystemInfoManager();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void ConfigManager_ConfigChanged()
        {
            Dispatcher.Invoke(ApplyConfig);
        }

        private void ApplyConfig()
        {
            var conf = ConfigManager.CurrentConfig;
            
            if (conf.OverlayVisible)
            {
                this.Visibility = Visibility.Visible;
                this.Opacity = conf.Opacity;
                TopPanel.Visibility = conf.ShowTopPanel ? Visibility.Visible : Visibility.Collapsed;
                BottomPanel.Visibility = conf.ShowBottomPanel ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        public void ToggleOverlayVisibility()
        {
            ConfigManager.CurrentConfig.OverlayVisible = !ConfigManager.CurrentConfig.OverlayVisible;
            ConfigManager.SaveConfig();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _overlayManager.Initialize(this);
            
            _systemInfoManager.InfoUpdated += SystemInfoManager_InfoUpdated;
            _systemInfoManager.StartUpdating();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _systemInfoManager.StopUpdating();
            _overlayManager.Cleanup();
        }

        private void SystemInfoManager_InfoUpdated()
        {
            Dispatcher.Invoke(() =>
            {
                var info = _systemInfoManager.CurrentInfo;
                TxtTime.Text = info.Time;
                
                string chargeStr = info.IsCharging ? "⚡ " : "";
                string batStr = info.BatteryPercentage >= 0 ? $"{info.BatteryPercentage}%" : "—";
                TxtBattery.Text = $"🔋 {chargeStr}{batStr}";
                
                TxtNetworkType.Text = info.NetworkType;
                TxtNetSpeed.Text = info.NetworkSpeed;
            });
        }

        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // No action needed: WPF handles transparency automatically.
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // No action needed: WPF handles transparency automatically.
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            InputManager.SimulateBack();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            InputManager.SimulateHome();
        }

        private void BtnWindows_Click(object sender, RoutedEventArgs e)
        {
            InputManager.SimulateWindows();
        }

        private void BtnKeyboard_Click(object sender, RoutedEventArgs e)
        {
            InputManager.ToggleKeyboard();
        }

        private void BtnToggleOverlay_Click(object sender, RoutedEventArgs e)
        {
            ToggleOverlayVisibility();
        }
    }
}
