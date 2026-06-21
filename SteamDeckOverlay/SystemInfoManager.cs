using System;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SteamDeckOverlay
{
    public class SystemInfo
    {
        public string Time { get; set; } = string.Empty;
        public int BatteryPercentage { get; set; } = -1;
        public bool IsCharging { get; set; }
        public string NetworkType { get; set; } = "❌"; // default disconnected
        public string NetworkSpeed { get; set; } = "↓0 Мбит/с ↑0 Мбит/с";
    }

    public class SystemInfoManager
    {
        public SystemInfo CurrentInfo { get; private set; } = new SystemInfo();
        public event Action? InfoUpdated;

        private CancellationTokenSource? _updateCts;
        
        private long _lastBytesReceived = 0;
        private long _lastBytesSent = 0;
        private DateTime _lastNetworkTime = DateTime.MinValue;

        public void StartUpdating()
        {
            _updateCts = new CancellationTokenSource();
            
            Task.Run(async () =>
            {
                while (!_updateCts.Token.IsCancellationRequested)
                {
                    UpdateSystemInfo();
                    InfoUpdated?.Invoke();
                    await Task.Delay(2000, _updateCts.Token); // 2 seconds to reduce CPU
                }
            }, _updateCts.Token);
        }

        private void UpdateSystemInfo()
        {
            try
            {
                CurrentInfo.Time = DateTime.Now.ToString("HH:mm");

                // Update Battery
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        if (mo["EstimatedChargeRemaining"] != null)
                        {
                            CurrentInfo.BatteryPercentage = Convert.ToInt32(mo["EstimatedChargeRemaining"]);
                        }
                        if (mo["BatteryStatus"] != null)
                        {
                            int status = Convert.ToInt32(mo["BatteryStatus"]);
                            CurrentInfo.IsCharging = status == 2; // 2 = AC / Charging
                        }
                        break;
                    }
                }
                catch (PlatformNotSupportedException) 
                {
                    // Fallback or ignore for non-Windows (or Linux Sandbox)
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Battery info error: {ex.Message}");
                }

                // Update WiFi / Ethernet Status & Speed
                try
                {
                    var activeInterface = NetworkInterface.GetAllNetworkInterfaces()
                        .FirstOrDefault(x => (x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || 
                                              x.NetworkInterfaceType == NetworkInterfaceType.Ethernet) && 
                                              x.OperationalStatus == OperationalStatus.Up && 
                                              !x.Description.ToLower().Contains("virtual"));
                    
                    if (activeInterface != null)
                    {
                        if (activeInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        {
                            CurrentInfo.NetworkType = "📶";
                        }
                        else
                        {
                            CurrentInfo.NetworkType = "🌐";
                        }

                        var stats = activeInterface.GetIPv4Statistics();
                        long currentReceived = stats.BytesReceived;
                        long currentSent = stats.BytesSent;
                        DateTime now = DateTime.Now;

                        if (_lastNetworkTime != DateTime.MinValue)
                        {
                            double seconds = (now - _lastNetworkTime).TotalSeconds;
                            if (seconds > 0)
                            {
                                // bits per second = (bytes * 8) / seconds
                                // megabits per second = bps / 1_000_000
                                double recvMbps = ((currentReceived - _lastBytesReceived) * 8.0) / seconds / 1_000_000.0;
                                double sentMbps = ((currentSent - _lastBytesSent) * 8.0) / seconds / 1_000_000.0;

                                // Prevent negative spikes 
                                if (recvMbps < 0) recvMbps = 0;
                                if (sentMbps < 0) sentMbps = 0;

                                CurrentInfo.NetworkSpeed = $"↓{recvMbps:F1} Мбит/с ↑{sentMbps:F1} Мбит/с";
                            }
                        }

                        _lastBytesReceived = currentReceived;
                        _lastBytesSent = currentSent;
                        _lastNetworkTime = now;
                    }
                    else
                    {
                        CurrentInfo.NetworkType = "❌";
                        CurrentInfo.NetworkSpeed = "↓0 Мбит/с ↑0 Мбит/с";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Net info error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                 Debug.WriteLine($"System info update error: {ex.Message}");
            }
        }

        public void StopUpdating()
        {
            _updateCts?.Cancel();
            _updateCts?.Dispose();
        }
    }
}
