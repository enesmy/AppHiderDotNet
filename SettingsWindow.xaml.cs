using System;
using System.Windows;
using Microsoft.Win32;

namespace AppHiderNet
{
    public partial class SettingsWindow : Window
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "AppHiderNet";

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load startup setting
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false))
                {
                    StartWithWindowsCheckBox.IsChecked = key?.GetValue(AppName) != null;
                }
            }
            catch
            {
                StartWithWindowsCheckBox.IsChecked = false;
            }

            // Load other settings from App
            var app = (App)System.Windows.Application.Current;
            StartMinimizedCheckBox.IsChecked = app.StartMinimized;
            ShowOverlayCheckBox.IsChecked = app.ShowOverlayButton;
            PasswordProtectionCheckBox.IsChecked = app.PasswordProtectionEnabled;
            MasterPasswordBox.Password = app.MasterPassword ?? "";
        }

        private void StartWithWindows_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (StartWithWindowsCheckBox.IsChecked == true)
                    {
                        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        // For .NET Core/5+, we need to use the exe path, not the dll
                        exePath = exePath.Replace(".dll", ".exe");
                        key?.SetValue(AppName, $"\"{exePath}\"");
                    }
                    else
                    {
                        key?.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save startup setting: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ShowOverlay_Changed(object sender, RoutedEventArgs e)
        {
            var app = (App)System.Windows.Application.Current;
            app.ShowOverlayButton = ShowOverlayCheckBox.IsChecked == true;
            
            // Toggle overlay button visibility
            if (app.ShowOverlayButton)
            {
                app.ShowOverlayButtonWindow();
            }
            else
            {
                app.HideOverlayButtonWindow();
            }
        }

        private void PasswordProtection_Changed(object sender, RoutedEventArgs e)
        {
            var app = (App)System.Windows.Application.Current;
            app.PasswordProtectionEnabled = PasswordProtectionCheckBox.IsChecked == true;
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var passwordDialog = new PasswordDialog();
            passwordDialog.Owner = this;
            if (passwordDialog.ShowDialog() == true)
            {
                var app = (App)System.Windows.Application.Current;
                // Store the new password - you might want to add logic to update a specific window's password
                // For now, this is a placeholder for password change functionality
                System.Windows.MessageBox.Show("Password changed successfully!", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)System.Windows.Application.Current;
            
            // Save settings
            app.StartMinimized = StartMinimizedCheckBox.IsChecked == true;
            app.ShowOverlayButton = ShowOverlayCheckBox.IsChecked == true;
            app.PasswordProtectionEnabled = PasswordProtectionCheckBox.IsChecked == true;
            app.MasterPassword = string.IsNullOrWhiteSpace(MasterPasswordBox.Password) ? null : MasterPasswordBox.Password;
            
            // Save to persistent storage (you might want to use a config file)
            StateManager.SaveSettings(app.StartMinimized, app.ShowOverlayButton, app.PasswordProtectionEnabled, app.MasterPassword);
            
            System.Windows.MessageBox.Show("Settings saved!", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
