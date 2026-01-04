using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AppHiderNet
{
    public partial class SafeModeManagerWindow : Window
    {
        public SafeModeManagerWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var app = (App)System.Windows.Application.Current;
            
            // Load apps in Safe Mode list
            SafeModeAppsList.ItemsSource = app.SafeModeAppPaths.ToList();
            
            // Load password settings
            bool hasPassword = !string.IsNullOrWhiteSpace(app.SafeModePassword);
            RequirePasswordCheck.IsChecked = hasPassword;
            SafeModePasswordBox.Password = app.SafeModePassword ?? "";
            SafeModePasswordBox.IsEnabled = hasPassword;
        }

        private void RequirePasswordCheck_Changed(object sender, RoutedEventArgs e)
        {
            SafeModePasswordBox.IsEnabled = RequirePasswordCheck.IsChecked == true;
            if (RequirePasswordCheck.IsChecked == false)
            {
                SafeModePasswordBox.Password = "";
            }
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            if (SafeModeAppsList.SelectedItem != null)
            {
                var app = (App)System.Windows.Application.Current;
                string selectedPath = SafeModeAppsList.SelectedItem.ToString();
                
                app.SafeModeAppPaths.Remove(selectedPath);
                
                // Refresh the list
                SafeModeAppsList.ItemsSource = app.SafeModeAppPaths.ToList();
                
                System.Windows.MessageBox.Show("App removed from Safe Mode list.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Please select an app to remove.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)System.Windows.Application.Current;
            
            // Save password setting
            if (RequirePasswordCheck.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(SafeModePasswordBox.Password))
                {
                    System.Windows.MessageBox.Show("Please enter a password or uncheck 'Require Password'.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                app.SafeModePassword = SafeModePasswordBox.Password;
            }
            else
            {
                app.SafeModePassword = null;
            }
            
            // Save settings
            StateManager.SaveSettings(
                app.StartMinimized, 
                app.ShowOverlayButton, 
                app.PasswordProtectionEnabled, 
                app.MasterPassword,
                app.SafeModeAppPaths,
                app.SafeModePassword);
            
            System.Windows.MessageBox.Show("Settings saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
