using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AppHiderNet
{
    public partial class VisibleAppsWindow : Window
    {
        public class AppItem
        {
            public IntPtr Hwnd { get; set; }
            public string Title { get; set; }
            public override string ToString() => Title;
        }

        public VisibleAppsWindow()
        {
            InitializeComponent();
            LoadApps();
        }

        private void LoadApps()
        {
            var apps = new List<AppItem>();
            NativeMethods.EnumWindows((hwnd, lParam) =>
            {
                if (NativeMethods.IsWindowVisible(hwnd))
                {
                    StringBuilder sb = new StringBuilder(256);
                    NativeMethods.GetWindowText(hwnd, sb, 256);
                    string title = sb.ToString();

                    if (!string.IsNullOrWhiteSpace(title) && title != "App Hider Manager" && title != "Overlay" && title != "Program Manager")
                    {
                        apps.Add(new AppItem { Hwnd = hwnd, Title = title });
                    }
                }
                return true;
            }, IntPtr.Zero);

            AppsList.ItemsSource = apps;
        }

        private void HideSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedApps = AppsList.SelectedItems.Cast<AppItem>().ToList();
            if (selectedApps.Count == 0) return;

            string password = null;
            if (PasswordProtectCheck.IsChecked == true)
            {
                var pwdDialog = new PasswordDialog();
                if (pwdDialog.ShowDialog() == true)
                {
                    password = pwdDialog.Password;
                }
                else
                {
                    return; // Cancelled
                }
            }

            var app = (App)System.Windows.Application.Current;
            foreach (var item in selectedApps)
            {
                app.HideWindowPublic(item.Hwnd, item.Title, password);
            }

            this.Close();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadApps();
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
