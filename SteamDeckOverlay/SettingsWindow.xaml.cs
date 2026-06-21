using System;
using System.Windows;

namespace SteamDeckOverlay
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigManager _configManager;
        private bool _isLoaded = false;

        public SettingsWindow(ConfigManager configManager)
        {
            InitializeComponent();
            _configManager = configManager;
            
            LoadSettings();
            _isLoaded = true;
        }

        private void LoadSettings()
        {
            var config = _configManager.CurrentConfig;
            SldOpacity.Value = config.Opacity;
            TxtOpacityVal.Text = $"{(int)(config.Opacity * 100)}%";
            ChkTopPanel.IsChecked = config.ShowTopPanel;
            ChkBottomPanel.IsChecked = config.ShowBottomPanel;
        }

        private void SaveSettings()
        {
            if (!_isLoaded) return;

            var config = _configManager.CurrentConfig;
            config.Opacity = SldOpacity.Value;
            config.ShowTopPanel = ChkTopPanel.IsChecked ?? true;
            config.ShowBottomPanel = ChkBottomPanel.IsChecked ?? true;
            
            _configManager.SaveConfig();
        }

        private void SldOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtOpacityVal != null)
                TxtOpacityVal.Text = $"{(int)(SldOpacity.Value * 100)}%";
            
            SaveSettings();
        }

        private void ChkTopPanel_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void ChkBottomPanel_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            var config = _configManager.CurrentConfig;
            config.Opacity = 0.8;
            config.ShowTopPanel = true;
            config.ShowBottomPanel = true;
            _configManager.SaveConfig();

            _isLoaded = false;
            LoadSettings();
            _isLoaded = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // Force GC as requested to free memory after settings close
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
