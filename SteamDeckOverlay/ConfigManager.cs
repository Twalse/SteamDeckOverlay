using System;
using System.IO;
using System.Text.Json;

namespace SteamDeckOverlay
{
    public class Config
    {
        public bool StartMinimized { get; set; } = false;
        public double Opacity { get; set; } = 0.8;
        public bool ShowTopPanel { get; set; } = true;
        public bool ShowBottomPanel { get; set; } = true;
        public bool OverlayVisible { get; set; } = true;
    }

    public class ConfigManager
    {
        private readonly string _configFilePath;

        public Config CurrentConfig { get; private set; }
        
        public event Action? ConfigChanged;

        public ConfigManager()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataFolder, "SteamDeckOverlay");
            Directory.CreateDirectory(appFolder);
            _configFilePath = Path.Combine(appFolder, "config.json");

            LoadConfig();
        }

        public void LoadConfig()
        {
            if (File.Exists(_configFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_configFilePath);
                    CurrentConfig = JsonSerializer.Deserialize<Config>(json) ?? new Config();
                }
                catch
                {
                    CurrentConfig = new Config();
                }
            }
            else
            {
                CurrentConfig = new Config();
                SaveConfig();
            }
        }

        public void SaveConfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(CurrentConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);
                ConfigChanged?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}
