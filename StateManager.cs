using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AppHiderNet
{
    public static class StateManager
    {
        private static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hidden_apps.json");

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
    }
}
