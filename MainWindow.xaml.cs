using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace AppHiderNet
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Bind directly to the ObservableCollection in App
            HiddenAppsList.ItemsSource = ((App)System.Windows.Application.Current).HiddenWindowsList;
        }

        public void RefreshList()
        {
            // No longer needed with ObservableCollection, but kept for compatibility if called
        }

        private void RestoreSelected_Click(object sender, RoutedEventArgs e)
        {
            if (HiddenAppsList.SelectedValue != null)
            {
                IntPtr hwnd = (IntPtr)HiddenAppsList.SelectedValue;
                ((App)System.Windows.Application.Current).ShowHiddenWindowPublic(hwnd);
                // No need to call RefreshList(), ObservableCollection handles it
            }
        }

        private void KillSelected_Click(object sender, RoutedEventArgs e)
        {
            if (HiddenAppsList.SelectedValue != null)
            {
                IntPtr hwnd = (IntPtr)HiddenAppsList.SelectedValue;
                var app = (App)System.Windows.Application.Current;
                
                try
                {
                    uint pid;
                    NativeMethods.GetWindowThreadProcessId(hwnd, out pid);
                    if (pid != 0)
                    {
                        var process = System.Diagnostics.Process.GetProcessById((int)pid);
                        process.Kill();
                        
                        // Remove from list manually since the window is gone and won't be "shown"
                        app.Dispatcher.Invoke(() => 
                        {
                            var itemToRemove = app.HiddenWindowsList.FirstOrDefault(x => x.Key == hwnd);
                            if (!itemToRemove.Equals(default(KeyValuePair<IntPtr, string>)))
                            {
                                app.HiddenWindowsList.Remove(itemToRemove);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Could not kill process: {ex.Message}");
                }
            }
        }

        private void RestoreAll_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)System.Windows.Application.Current;
            // Create a copy of keys to avoid modification exception during iteration
            var keys = app.HiddenWindowsList.Select(x => x.Key).ToList();
            
            foreach (var hwnd in keys)
            {
                app.ShowHiddenWindowPublic(hwnd);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Don't close the app, just hide the window
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }
    }
}