using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AppHiderNet
{
    public class AppSettings
    {
        public bool StartMinimized { get; set; }
        public bool ShowOverlayButton { get; set; }
        public bool PasswordProtectionEnabled { get; set; }
        public string? MasterPassword { get; set; }
    }

    public static class StateManager
    {
        private static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hidden_apps.json");
        private static string SettingsFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public static void Save(List<HiddenApp> apps)
        {
            try
            {
                string json = JsonSerializer.Serialize(apps);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                // Log or ignore
                System.Diagnostics.Debug.WriteLine($"Failed to save state: {ex.Message}");
            }
        }

        public static List<HiddenApp> Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<List<HiddenApp>>(json) ?? new List<HiddenApp>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load state: {ex.Message}");
            }
            return new List<HiddenApp>();
        }

        public static void SaveSettings(bool startMinimized, bool showOverlay, bool passwordProtection, string? masterPassword)
        {
            try
            {
                var settings = new AppSettings
                {
                    StartMinimized = startMinimized,
                    ShowOverlayButton = showOverlay,
                    PasswordProtectionEnabled = passwordProtection,
                    MasterPassword = masterPassword
                };
                string json = JsonSerializer.Serialize(settings);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
            return new AppSettings();
        }
    }
}
